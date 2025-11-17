using Microsoft.EntityFrameworkCore;
using VetCrm.Infrastructure.Data;

using Hangfire;
using Hangfire.PostgreSql;

using VetCrm.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS için policy adı
const string FrontendCorsPolicy = "FrontendCors";

builder.Services.AddDbContext<VetCrmDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ReminderProcessor DI
builder.Services.AddScoped<ReminderProcessor>();

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



// DbContext
builder.Services.AddDbContext<VetCrmDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ReminderProcessor DI
builder.Services.AddScoped<ReminderProcessor>();

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Hangfire ayarı (yeni, temiz kullanım)
builder.Services.AddHangfire(config =>
    config
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(options =>
        {
            // Yeni overload: connection string burada veriliyor
            options.UseNpgsqlConnection(
                builder.Configuration.GetConnectionString("DefaultConnection"));
        }));

builder.Services.AddHangfireServer();




var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHangfireDashboard("/hangfire");

app.UseHttpsRedirection();

// CORS burada devreye girsin
app.UseCors(FrontendCorsPolicy);

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

