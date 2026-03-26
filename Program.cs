using Microsoft.EntityFrameworkCore;
using Database;
using WattageAPI.Endpoints;
using Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
