using Models;
using Services;
using Database;
using Serilog;
using ILogger = Serilog.ILogger;
using Microsoft.EntityFrameworkCore;

namespace WattWatch.Endpoints;

public static class LiveEndpoints
{
  private static readonly ILogger _logger = Log.ForContext(typeof(LiveEndpoints));

  public static void MapLiveEndpoints(this WebApplication app)
  {
    app.MapGet("/live-energy", GetAllLiveEnergyConsumptions);
    app.MapGet("/live-energy/{DeviceId}", GetLiveEnergyConsumptionByDevice);
  }

  public static async Task<IResult> GetAllLiveEnergyConsumptions(AppDbContext db)
  {
    try
    {
      var liveData = await db.LiveConsumptions
      .Include(lc => lc.Device) // Include related device data for better context in the response.
      .Include(lc => lc.Location) // Include related location data for better context in the response.
      .AsNoTracking().
      ToListAsync();
      return Results.Ok(liveData);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error retrieving live energy consumption: {ex.Message}");
      return Results.Problem("An error occurred while retrieving live energy consumption data.");
    }
  }

  public static async Task<IResult> GetLiveEnergyConsumptionByDevice(string DeviceId, AppDbContext db)
  {
    try
    {
      var liveData = await db.LiveConsumptions
        .AsNoTracking()
        .Include(lc => lc.Device)
        .Include(lc => lc.Location)
        .FirstOrDefaultAsync(lc => lc.Device.DeviceId == DeviceId);

      if (liveData == null)
      {
        return Results.NotFound($"Live energy consumption data for device '{DeviceId}' not found.");
      }

      return Results.Ok(liveData);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error retrieving live energy consumption for device '{DeviceId}': {ex.Message}");
      return Results.Problem("An error occurred while retrieving live energy consumption data for the specified device.");
    }
  }
}
