using Microsoft.EntityFrameworkCore;
using Database;
using WattWatch.Endpoints;
using Middleware;
using Serilog;
using Services;
using Jobs;
using NCronJob;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostingContext, configuration) =>
{
  configuration.ReadFrom.Configuration(hostingContext.Configuration);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<CalculationsService>();
builder.Services.AddSwaggerGen(c =>
    {
      c.SwaggerDoc("v1", new OpenApiInfo { Title = "WattWatch API", Version = "v1" });
      c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
      {
        Description = "API Key",
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
      });
      c.AddSecurityRequirement((document) => new OpenApiSecurityRequirement()
      {
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
      });
    });

// Ignore circular references caused by bidirectional navigation properties
builder.Services.ConfigureHttpJsonOptions(options =>
{
  options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var athensZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Athens");
builder.Services.AddNCronJob(options => options
    .AddJob<LiveEnergyJob>(cron => cron
        .WithCronExpression("*/1 * * * *", athensZone)) // Every 5 minutes Athens time
    .AddJob<DailyEnergyJob>(cron => cron
        .WithCronExpression("0 0 * * *", athensZone)) // Every day at midnight Athens time
    .AddJob<MonthlyEnergyJob>(cron => cron
        .WithCronExpression("0 0 1 * *", athensZone)) // Every 1st day of the month at midnight Athens 23:47
    .AddJob<YearlyEnergyJob>(cron => cron
        .WithCronExpression("0 0 1 1 *", athensZone))); // Every January 1st at midnight Athens time

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
app.MapDeviceEndpoints();
app.MapLocationEndpoints();
app.MapLiveEndpoints();
app.MapJobEndPoints();

app.Run();
