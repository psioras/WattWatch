using Microsoft.EntityFrameworkCore;
using Database;
using WattageAPI.Endpoints;
using Middleware;
using Serilog;
using Services;
using Jobs;
using NCronJob;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, configuration) =>
{
  configuration.ReadFrom.Configuration(hostingContext.Configuration);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<CalculationsService>();

var athensZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Athens");
builder.Services.AddNCronJob(options => options
    .AddJob<DailyEnergyJob>(cron => cron
        .WithCronExpression("0 0 * * *", athensZone)) // Every day at midnight Athens time
    .AddJob<WeeklyEnergyJob>(cron => cron
        .WithCronExpression("0 0 * * 1", athensZone)) // Every Monday at midnight Athens time
    .AddJob<MonthlyEnergyJob>(cron => cron
        .WithCronExpression("0 0 1 * *", athensZone))); // Every 1st day of the month at midnight Athens time

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Wattage API V1");
    options.RoutePrefix = string.Empty;
  });
}
app.UseMiddleware<ApiKeyMiddleware>();
app.MapWattageEndpoints();

app.Run();
