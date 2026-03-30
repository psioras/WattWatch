using Models;
using Database;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace WattWatch.Endpoints;

public static class DeviceEndpoints
{
  private static readonly ILogger _logger = Log.ForContext(typeof(DeviceEndpoints));

  public static void MapDeviceEndpoints(this WebApplication app)
  {
    app.MapPost("/devices", PostNewDevice);
    app.MapGet("/devices/{DeviceId}", GetDevice);
    app.MapGet("/devices", GetAllDevices);
    app.MapPatch("/devices/{DeviceId}/update", PatchDevice);
  }

  public static async Task<IResult> PostNewDevice(Device device, AppDbContext db)
  {
    try
    {
      if (device.LocationId == 0)
      {
        _logger.Warning($"Rejected device: LocationId is required and cannot be 0.");
        return Results.BadRequest("LocationId is required and cannot be 0. Create a location first via POST /locations and use its ID when creating the device.");
      }

      db.Devices.Add(device);
      await db.SaveChangesAsync();
      return Results.Created($"/devices/{device.Id}", null);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error adding device: {ex.Message}");
      return Results.Problem("An error occurred while adding the device.");
    }
  }

  public static async Task<IResult> GetDevice(string DeviceId, AppDbContext db)
  {
    try
    {
      var device = await db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.DeviceId == DeviceId);
      if (device == null)
      {
        return Results.NotFound($"Device with ID '{DeviceId}' not found.");
      }
      return Results.Ok(device);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error retrieving device: {ex.Message}");
      return Results.Problem("An error occurred while retrieving the device.");
    }
  }

  public static async Task<IResult> GetAllDevices(AppDbContext db)
  {
    try
    {
      var devices = await db.Devices.AsNoTracking().ToListAsync();
      return Results.Ok(devices);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error retrieving devices: {ex.Message}");
      return Results.Problem("An error occurred while retrieving the devices.");
    }
  }

  public static async Task<IResult> PatchDevice(string DeviceId, PatchDeviceRequest request, AppDbContext db)
  {
    try
    {
      var device = await db.Devices.FirstOrDefaultAsync(d => d.DeviceId == DeviceId);
      if (device == null)
      {
        return Results.NotFound($"Device with ID '{DeviceId}' not found.");
      }

      device.FriendlyName = request.FriendlyName ?? device.FriendlyName;
      if (request.LocationId.HasValue)
      {
        device.LocationId = request.LocationId.Value;
      }

      await db.SaveChangesAsync();
      return Results.Ok(device);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error updating device: {ex.Message}");
      return Results.Problem("An error occurred while updating the device.");
    }
  }
}

public class PatchDeviceRequest
{
  public string? FriendlyName { get; set; }
  public int? LocationId { get; set; }
}
