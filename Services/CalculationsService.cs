using Microsoft.EntityFrameworkCore;
using Models;
using Database;

namespace Services;

public class CalculationsService
{
  private readonly AppDbContext _context;
  private readonly ILogger<CalculationsService> _logger;

  public CalculationsService(AppDbContext context, ILogger<CalculationsService> logger)
  {
    _context = context;
    _logger = logger;
  }

  //<summary>
  // This method runs the energy consumption calculations for all devices over a specified time period and saves
  // the results to the database.
  // periodType: A string indicating the type of period (e.g., "Daily", "Weekly", "Monthly").
  // periodStart: The start date and time of the period for which to calculate energy consumption
  // periodEnd: The end date and time of the period for which to calculate energy consumption
  //</summary>
  public async Task RunCalculationsAsync(string periodType, DateTime periodStart, DateTime periodEnd)
  {
    try
    {
      var deviceIds = await _context.ElectricityReadings
        .Select(r => r.DeviceId)
        .Distinct()
        .ToListAsync();

      if (!deviceIds.Any())
      {
        _logger.LogInformation("No devices found for calculations.");
        return;
      }

      foreach (var deviceId in deviceIds)
      {
        var device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.DeviceId == deviceId);

        if (device == null)
        {
          _logger.LogWarning($"Device '{deviceId}' found in ElectricityReadings but not registered in Devices table. Skipping.");
          continue;
        }

        var kWh = await CalculateEnergyConsumption(deviceId, periodStart, periodEnd);

        if (kWh == 0)
        {
          _logger.LogInformation($"No energy consumption calculated for device {deviceId} during the period {periodStart} to {periodEnd}.");
          continue;
        }

        var cost = await CalculateCostAsync(device.LocationId, kWh, periodEnd);

        var record = new EnergyConsumption
        {
          DeviceId = deviceId,
          DeviceTableId = device.Id,
          LocationId = device.LocationId,
          PeriodStart = periodStart,
          PeriodEnd = periodEnd,
          kWhConsumption = kWh,
          CostEuros = cost,
          PeriodType = periodType
        };

        _context.EnergyConsumptions.Add(record);
      }
      await _context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error during calculations: {ex.Message}");
    }
  }

  // This helper method calculates energy consumption in kWh for a given device and time period
  public async Task<double> CalculateEnergyConsumption(string deviceId, DateTime periodStart, DateTime periodEnd)
  {
    var readings = await _context.ElectricityReadings
      .Where(r => r.DeviceId == deviceId && r.Timestamp >= periodStart && r.Timestamp <= periodEnd)
      .OrderBy(r => r.Timestamp)
      .ToListAsync();

    if (readings.Count < 2)
    {
      return 0; // Not enough data to calculate consumption
    }

    double totalEnergy = 0;

    // Don't know if it's correct...
    for (int i = 1; i < readings.Count; i++)
    {
      var previous = readings[i - 1];
      var current = readings[i];

      var hours = (current.Timestamp - previous.Timestamp).TotalHours;
      var kWhConsumption = (previous.Wattage + current.Wattage) / 2;

      totalEnergy += kWhConsumption * hours / 1000; // Convert to kWh
    }

    return totalEnergy;
  }

  //<summary>
  // This helper method calculates the cost of energy consumption
  // locationId: The ID of the location for which to calculate the cost
  // kWh: The amount of energy consumed in kWh
  // periodEnd: The end date and time of the period for which to calculate the cost
  // </summary>
  private async Task<double> CalculateCostAsync(int locationId, double kWh, DateTime periodEnd)
  {
    var price = await _context.LocationPrices
      .Where(l => l.LocationId == locationId && l.EffectiveFrom <= DateOnly.FromDateTime(periodEnd))
      .OrderByDescending(l => l.EffectiveFrom)
      .FirstOrDefaultAsync();

    if (price == null)
    {
      _logger.LogWarning($"No price found for location {locationId} at {periodEnd}. Defaulting to 0 cost.");
      return 0;
    }

    double cost = kWh * price.PricePerKwh;
    return cost;
  }
}
