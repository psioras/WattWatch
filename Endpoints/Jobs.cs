using Models;
using Services;
using Database;
using Serilog;
using ILogger = Serilog.ILogger;
using Microsoft.EntityFrameworkCore;

namespace WattWatch.Endpoints;

public static class JobEndpoints
{
  private static readonly ILogger _logger = Log.ForContext(typeof(JobEndpoints));

  public static void MapJobEndPoints(this WebApplication app)
  {
    app.MapPost("/jobs/live-energy", RunLiveEnergyJob);
    app.MapPost("/jobs/daily-energy", RunDailyEnergyJob);
    app.MapPost("/jobs/monthly-energy", RunMonthlyEnergyJob);
    app.MapPost("/jobs/yearly-energy", RunYearlyEnergyJob);
  }

  // Manual trigger endpoint for updating the live energy consumption.
  public static async Task<IResult> RunLiveEnergyJob(CalculationsService calculationsService, AppDbContext db)
  {
    try
    {
      await calculationsService.UpdateLiveConsumptionAsync();
      var liveData = await db.LiveConsumptions
        .Include(lc => lc.Device)
        .Include(lc => lc.Location)
        .AsNoTracking()
        .ToListAsync();

      return Results.Ok(liveData);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error executing LiveEnergyJob: {ex.Message}");
      return Results.Problem("An error occurred while executing the LiveEnergyJob.");
    }
  }

  public static async Task<IResult> RunDailyEnergyJob(CalculationsService calculationsService)
  {
    try
    {
      await calculationsService.TransferLiveToEnergyConsumptionAsync("Daily");
      return Results.Ok("Daily energy job executed successfully.");
    }
    catch (Exception ex)
    {
      _logger.Error($"Error executing DailyEnergyJob: {ex.Message}");
      return Results.Problem("An error occurred while executing the DailyEnergyJob.");
    }
  }

  public static async Task<IResult> RunMonthlyEnergyJob(CalculationsService calculationsService)
  {
    try
    {
      await calculationsService.TransferLiveToEnergyConsumptionAsync("Monthly");
      return Results.Ok("Monthly energy job executed successfully.");
    }
    catch (Exception ex)
    {
      _logger.Error($"Error executing MonthlyEnergyJob: {ex.Message}");
      return Results.Problem("An error occurred while executing the MonthlyEnergyJob.");
    }
  }

  public static async Task<IResult> RunYearlyEnergyJob(CalculationsService calculationsService)
  {
    try
    {
      await calculationsService.TransferLiveToEnergyConsumptionAsync("Yearly");
      return Results.Ok("Yearly energy job executed successfully.");
    }
    catch (Exception ex)
    {
      _logger.Error($"Error executing YearlyEnergyJob: {ex.Message}");
      return Results.Problem("An error occurred while executing the YearlyEnergyJob.");
    }
  }
}

