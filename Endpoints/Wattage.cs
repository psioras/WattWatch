using Models;
using Database;
using ILogger = Serilog.ILogger;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace WattWatch.Endpoints;

public static class WattageEndpoints
{
  private static readonly ILogger _logger = Log.ForContext(typeof(WattageEndpoints));

  public static void MapWattageEndpoints(this WebApplication app)
  {
    app.MapPost("/wattage", AddWattage);
    app.MapGet("/wattage/{DeviceId}", GetWattage);
  }

  /// <summary>
  /// Basic Post endpoint to add a wattage reading to the database. Expects a JSON body with the following format:
  /// {
  ///  "deviceId": "string",
  ///  "wattage": 0,
  ///  "timestamp" is optional and will default to the current time if not provided
  ///  }
  /// </summary>
  /// <param name="reading"></param>
  /// <param name="db"></param>
  /// <returns></returns>
  /// 
  public static async Task<IResult> AddWattage(DeviceReading reading, AppDbContext db)
  {
    try
    {
      var device = await db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.DeviceId == reading.DeviceId);
      if (device == null)
      {
        _logger.Warning($"Rejected wattage reading: Device '{reading.DeviceId}' is not registered.");
        return Results.BadRequest($"Device '{reading.DeviceId}' is not registered. Register it first via POST /devices.");
      }

      reading.DeviceTableId = device.Id;

      _logger.Information($"Adding wattage reading for device {reading.DeviceId}: {reading.Wattage}W at {reading.Timestamp}");
      db.ElectricityReadings.Add(reading);
      await db.SaveChangesAsync();
      return Results.Created($"/wattage/{reading.DeviceId}", null);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error adding wattage reading: {ex.Message}");
      return Results.Problem("An error occurred while adding the wattage reading.");
    }
  }

  /// <summary>
  /// Basic Get endpoint to retrieve the most recent wattage reading for a given device.
  /// </summary>
  /// <param name="DeviceId"></param>
  /// <param name="db"></param>
  /// <returns></returns>
  ///
  public static async Task<IResult> GetWattage(string DeviceId, AppDbContext db)
  {
    try
    {
      _logger.Information($"Retrieving most recent wattage reading for device {DeviceId}");
      var reading = await db.ElectricityReadings.Where(r => r.DeviceId == DeviceId).OrderByDescending(r => r.Timestamp).FirstOrDefaultAsync();
      if (reading == null)
      {
        return Results.NotFound($"No wattage reading found for device {DeviceId}");
      }
      return Results.Ok(reading);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error retrieving wattage reading: {ex.Message}");
      return Results.Problem("An error occurred while getting the wattage reading.");
    }
  }
}
