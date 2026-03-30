using Models;
using Database;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace WattWatch.Endpoints;

public static class LocationEndpoints
{
  private static readonly ILogger _logger = Log.ForContext(typeof(LocationEndpoints));

  public static void MapLocationEndpoints(this WebApplication app)
  {
    app.MapPost("/locations", PostNewLocation);
    app.MapPost("/locations/{locationId}/prices", PostLocationPrice);
    app.MapPatch("/locations/{locationId}/update", PatchLocation);
  }

  /// <summary>
  /// Endpoint to add a new location to the database. Expects a JSON body with the following format:
  /// {
  /// "name": "string",
  /// "address": "string"
  /// }
  /// </summary>
  ///
  public static async Task<IResult> PostNewLocation(Location location, AppDbContext db)
  {
    try
    {
      db.Locations.Add(location);
      await db.SaveChangesAsync();
      return Results.Created($"/locations/{location.Id}", null);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error adding location: {ex.Message}");
      return Results.Problem("An error occurred while adding the location.");
    }
  }

  /// <summary>
  /// Endpoint to add a new price for a location. Expects a JSON body with the following format:
  /// {
  /// "locationId": 0,
  /// "pricePerKwh": 0,
  /// "effectiveFrom": "1821-03-25"
  /// }
  /// </summary>
  ///
  public static async Task<IResult> PostLocationPrice(LocationPrice price, AppDbContext db)
  {
    try
    {
      var location = await db.Locations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == price.LocationId);
      if (location == null)
      {
        _logger.Warning($"Rejected price: location with ID '{price.LocationId}' does not exist.");
        return Results.BadRequest($"Location with ID '{price.LocationId}' does not exist. Create it first via POST /locations.");
      }

      db.LocationPrices.Add(price);
      await db.SaveChangesAsync();
      return Results.Created($"/locations/{price.LocationId}/prices/{price.Id}", null);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error adding location price: {ex.Message}");
      return Results.Problem("An error occurred while adding the location price.");
    }
  }

  /// Endpoint to update an existing location.
  public static async Task<IResult> PatchLocation(int locationId, PatchLocationRequest request, AppDbContext db)
  {
    try
    {
      var location = await db.Locations.FirstOrDefaultAsync(l => l.Id == locationId);
      if (location == null)
      {
        _logger.Warning($"Rejected update: location with ID '{locationId}' does not exist.");
        return Results.NotFound($"Location with ID '{locationId}' does not exist.");
      }

      // Update only the fields that are provided in the request body
      if (!string.IsNullOrEmpty(request.Name))
      {
        location.Name = request.Name;
      }
      if (!string.IsNullOrEmpty(request.Address))
      {
        location.Address = request.Address;
      }

      await db.SaveChangesAsync();
      return Results.Ok(location);
    }
    catch (Exception ex)
    {
      _logger.Error($"Error updating location: {ex.Message}");
      return Results.Problem("An error occurred while updating the location.");
    }
  }
}

public class PatchLocationRequest
{
  public string? Name { get; set; }
  public string? Address { get; set; }
}



