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
using VetCrm.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCors";

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

var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger");
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Pipeline order (kritik)
app.UseHangfireDashboard("/hangfire");
app.UseStaticFiles();

app.UseRouting();

// ✅ CORS mutlaka Authentication/Authorization'dan ÖNCE
app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Hangfire recurring job
RecurringJob.AddOrUpdate<ReminderProcessor>(
    "process-reminders",
    rp => rp.ProcessDueRemindersAsync(),
    "0 9 * * *",
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
    });

app.Run();
