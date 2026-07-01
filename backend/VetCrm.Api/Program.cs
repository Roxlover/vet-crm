using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VetCrm.Api.Options;
using VetCrm.Api.Services;
using VetCrm.Api.Storage;
using VetCrm.Api.Middlewares;
using VetCrm.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCors";

// ✅ Fix for Npgsql 6.0+ DateTime issue (prevents 500 error when comparing local/UTC dates)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// DB
builder.Services.AddDbContext<VetCrmDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Hangfire
builder.Services.AddScoped<ReminderProcessor>();

builder.Services.AddHangfire(config =>
    config
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options =>
        {
            options.UseNpgsqlConnection(
                builder.Configuration.GetConnectionString("DefaultConnection")
            );
        })
);

builder.Services.AddHangfireServer();

// R2
builder.Services.Configure<R2Options>(builder.Configuration.GetSection("R2"));
builder.Services.AddScoped<IR2Storage, R2VisitImageStorage>();

// CORS (JWT ile uyumlu, credentials yok)
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "https://app.e-bullvet.com",
                "https://www.app.e-bullvet.com",
                "http://localhost:5173",
                "http://localhost",
                "http://192.168.1.107:5173",
                "capacitor://localhost"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();

        // JWT Bearer kullanıyorsun; cookie/credentials taşımıyorsun.
        // Bu yüzden AllowCredentials EKLEME.
        // .AllowCredentials();
    });
});

// Upload limit
builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

// Kestrel bind
builder.WebHost.UseSetting(WebHostDefaults.ServerUrlsKey, "http://0.0.0.0:5239");

// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BullBossOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(ctx =>
        {
            var username = ctx.User.FindFirst("username")?.Value;
            username ??= ctx.User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
            username ??= ctx.User.Identity?.Name;

            return string.Equals(username, "BullBoss", StringComparison.OrdinalIgnoreCase);
        });
    });
});

// Multipart limit (FormOptions)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Otomatik Migration ve Seed Data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetCrmDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        db.Database.Migrate();

        // Eğer hiç kullanıcı yoksa varsayılan kullanıcıları ekle
    if (!db.Users.Any())
    {
        db.Users.Add(new VetCrm.Domain.Entities.User
        {
            FullName = "BullBoss Admin",
            Username = "BullBoss",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "Admin"
        });
        db.SaveChanges();
    }

    if (!db.Diseases.Any())
    {
        var diseases = new List<VetCrm.Domain.Entities.Disease>
        {
            // Orijinal Enfeksiyöz ve Paraziter Hastalıklar
            new() { Name = "Parvovirus (Kanlı İshal)", Category = VetCrm.Domain.Entities.DiseaseCategory.Enfeksiyoz, Species = "Köpek", IsContagious = true },
            new() { Name = "Distemper (Gençlik Hastalığı)", Category = VetCrm.Domain.Entities.DiseaseCategory.Enfeksiyoz, Species = "Köpek", IsContagious = true },
            new() { Name = "FeLV (Kedi Lösemisi)", Category = VetCrm.Domain.Entities.DiseaseCategory.Enfeksiyoz, Species = "Kedi", IsContagious = true },
            new() { Name = "FIP (Feline İnfeksiyöz Peritonit)", Category = VetCrm.Domain.Entities.DiseaseCategory.Enfeksiyoz, Species = "Kedi", IsContagious = false },
            new() { Name = "Ehrlichiosis (Kene Ateşi)", Category = VetCrm.Domain.Entities.DiseaseCategory.Paraziter, Species = "Köpek", IsContagious = false },
            new() { Name = "Leishmaniasis", Category = VetCrm.Domain.Entities.DiseaseCategory.Paraziter, Species = "Köpek", IsContagious = false },
            new() { Name = "Otit (Kulak İltihabı)", Category = VetCrm.Domain.Entities.DiseaseCategory.Diger, Species = "Tümü", IsContagious = false },
            new() { Name = "Dermatofitoz (Mantar)", Category = VetCrm.Domain.Entities.DiseaseCategory.Enfeksiyoz, Species = "Tümü", IsContagious = true },
            new() { Name = "Kalp Kurdu (Dirofilariasis)", Category = VetCrm.Domain.Entities.DiseaseCategory.Paraziter, Species = "Köpek", IsContagious = false },

            // Yeni Kronik Hastalıklar - Kedi
            new() { Name = "Kronik Böbrek Yetmezliği (Kedi)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Yaşlı kedilerde en sık görülen kronik rahatsızlıklardan biri. Böbrek fonksiyonlarında ilerleyici kayıp.", IsContagious = false },
            new() { Name = "Hipertiroidizm", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Tiroid bezinin aşırı çalışması sonucu kilo kaybı, huzursuzluk ve iştah artışı ile seyreder.", IsContagious = false },
            new() { Name = "Diyabetes Mellitus (Kedi)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Özellikle şişman ve orta yaş kedilerde görülen şeker hastalığı.", IsContagious = false },
            new() { Name = "Kronik Sistit / FLUTD", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Alt idrar yolu hastalığı, tekrarlayan ataklarla seyreder.", IsContagious = false },
            new() { Name = "Feline Astım", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Kronik solunum yolu iltihabı, öksürük ve nefes darlığı ile kendini gösterir.", IsContagious = false },
            new() { Name = "İnflamatuar Bağırsak Hastalığı (IBD)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Kronik kusma ve ishal ile seyreden bağırsak iltihabı.", IsContagious = false },
            new() { Name = "Kronik Gingivostomatit", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Ağız içi kronik iltihap, oldukça ağrılı ve tedaviye dirençli olabilir.", IsContagious = false },
            new() { Name = "Hipertrofik Kardiyomiyopati (HCM)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Kalp kası kalınlaşması, bazı ırklarda genetik yatkınlık gösterir.", IsContagious = false },
            new() { Name = "FIV (Kedi AIDS'i)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Kronik immün yetmezlik, genelde kavga/ısırık yoluyla bulaşır.", IsContagious = true },
            new() { Name = "Osteoartrit (Kedi)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Kedi", Description = "Yaşlı kedilerde eklem yıpranması sonucu gelişir.", IsContagious = false },

            // Yeni Kronik Hastalıklar - Köpek
            new() { Name = "Diyabetes Mellitus (Köpek)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Orta yaş ve yaşlı köpeklerde sık görülen şeker hastalığı.", IsContagious = false },
            new() { Name = "Kronik Böbrek Yetmezliği (Köpek)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Özellikle yaşlı köpeklerde görülen ilerleyici böbrek fonksiyon kaybı.", IsContagious = false },
            new() { Name = "Hipotiroidizm", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Tiroid bezinin az çalışması, kilo alımı ve halsizlik ile seyreder.", IsContagious = false },
            new() { Name = "Cushing Sendromu", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Aşırı kortizol üretimi (Hiperadrenokortisizm) sonucu gelişen hormonal hastalık.", IsContagious = false },
            new() { Name = "Addison Hastalığı", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Yetersiz kortizol üretimi sonucu gelişen hormonal yetmezlik.", IsContagious = false },
            new() { Name = "İdiyopatik Epilepsi", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Tekrarlayan nöbetlerle seyreder, genelde genç yaşta başlar.", IsContagious = false },
            new() { Name = "Mitral Kapak Hastalığı", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Kronik kalp yetmezliği, özellikle küçük ırklarda çok yaygındır.", IsContagious = false },
            new() { Name = "Atopik Dermatit", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Kronik alerjik cilt hastalığı, tekrarlayan kaşıntı ve iltihapla seyreder.", IsContagious = false },
            new() { Name = "Kalça Displazisi", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Büyük ırklarda görülen genetik eklem bozukluğu.", IsContagious = false },
            new() { Name = "Osteoartrit (Köpek)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Eklem yıpranması, yaşla birlikte ilerler.", IsContagious = false },
            new() { Name = "Ekzokrin Pankreas Yetmezliği (EPI)", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Sindirim enzimi eksikliği, kilo kaybı ve yağlı dışkı ile seyreder.", IsContagious = false },
            new() { Name = "Kronik Otit Eksterna", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Köpek", Description = "Özellikle sarkık kulaklı ırklarda tekrarlayan kulak iltihabı.", IsContagious = false },

            // Ortak (Kedi & Köpek)
            new() { Name = "Obeziteye Bağlı Kronik Komplikasyonlar", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Tümü", Description = "Aşırı kiloya bağlı eklem yükü artışı ve diyabet riski artışı.", IsContagious = false },
            new() { Name = "Kronik Karaciğer Yetmezliği", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Tümü", Description = "İlerleyici karaciğer fonksiyon kaybı.", IsContagious = false },
            new() { Name = "Dejeneratif Miyelopati", Category = VetCrm.Domain.Entities.DiseaseCategory.Kronik, Species = "Tümü", Description = "Omurilik sinirlerinde ilerleyici dejenerasyon, özellikle yaşlı hayvanlarda görülür.", IsContagious = false }
        };
        db.Diseases.AddRange(diseases);
        db.SaveChanges();
    }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger");
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

try
{
    // Pipeline order (kritik)
    app.UseHangfireDashboard("/hangfire");

    // Hangfire recurring job
    RecurringJob.AddOrUpdate<ReminderProcessor>(
        "process-reminders",
        rp => rp.ProcessDueRemindersAsync(),
        "0 9 * * *",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
        });
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Hangfire failed to initialize on startup. Bypassing...");
}

app.UseStaticFiles();
app.UseRouting();

// ✅ CORS Endpoint Routing'den SONRA, Auth'tan ÖNCE olmalı
app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
