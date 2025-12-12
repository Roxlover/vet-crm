using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VetCrm.Api.Options;
using VetCrm.Api.Services;
using VetCrm.Api.Storage;
using VetCrm.Infrastructure.Data;
using VetCrm.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCors";

builder.Services.AddDbContext<VetCrmDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ReminderProcessor>();

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

builder.Services.AddScoped<IR2Storage, LocalVisitImageStorage>();


builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost",
                "http://192.168.1.107:5173",
                "capacitor://localhost",
                // ileride web deploy yaparsak:
                "https://app.e-bullvet.com"
            )
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod();
    });
});


builder.WebHost.UseSetting(WebHostDefaults.ServerUrlsKey, "http://0.0.0.0:5239");


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
            var username = ctx.User.FindFirst("username")?.Value
                           ?? ctx.User.Identity?.Name;

            return string.Equals(username, "BullBoss",
                StringComparison.OrdinalIgnoreCase);
        });
    });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();   

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger");

if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHangfireDashboard("/hangfire");
app.UseStaticFiles();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


RecurringJob.AddOrUpdate<ReminderProcessor>(
    "process-reminders",
    rp => rp.ProcessDueRemindersAsync(),
    "0 9 * * *",
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
    });

app.Run();
