using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VetCrm.Api.Services;
using VetCrm.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// CORS için policy adı
const string FrontendCorsPolicy = "FrontendCors";

// DbContext
builder.Services.AddDbContext<VetCrmDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ReminderProcessor DI
builder.Services.AddScoped<ReminderProcessor>();

// Hangfire ayarı
builder.Services.AddHangfire(config =>
    config
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options =>
        {
            options.UseNpgsqlConnection(
                builder.Configuration.GetConnectionString("DefaultConnection"));
        }));

builder.Services.AddHangfireServer();

// CORS kaydı
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ---------- JWT & CURRENT USER ----------

// appsettings.json → "Jwt" bölümünü JwtSettings ile bağla
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// JWT ayarlarını al
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
var keyBytes = Encoding.UTF8.GetBytes(jwtSettings.Key);

// Authentication
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

        // 🔴 BURASI YENİ: Hata sebebini console'a yaz
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("JWT FAILED >> " + ctx.Exception.Message);
                if (ctx.Exception.InnerException != null)
                    Console.WriteLine("INNER >> " + ctx.Exception.InnerException.Message);
                return Task.CompletedTask;
            },
            OnMessageReceived = ctx =>
            {
                Console.WriteLine("JWT HEADER >> " + ctx.Request.Headers["Authorization"]);
                return Task.CompletedTask;
            }
        };
    });

// Authorization – Bilanço sadece BullBoss
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BullBossOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(ctx =>
        {
            var username = ctx.User.FindFirst("username")?.Value
                           ?? ctx.User.Identity?.Name;

            return string.Equals(username, "BullBoss",
                StringComparison.OrdinalIgnoreCase);
        });
    });
});

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard("/hangfire");

app.UseHttpsRedirection();

// CORS
app.UseCors(FrontendCorsPolicy);
app.Use(async (context, next) =>
{
    var auth = context.Request.Headers["Authorization"].ToString();
    Console.WriteLine($"[AUTH HEADER] {auth}");
    await next();
});

// *** SIRA ÖNEMLİ ***: önce Authentication, sonra Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Hatırlatma job'u
RecurringJob.AddOrUpdate<ReminderProcessor>(
    "process-reminders",
    rp => rp.ProcessDueRemindersAsync(),
    "0 9 * * *",
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
    });

app.Run();
