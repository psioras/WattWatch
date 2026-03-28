using Microsoft.EntityFrameworkCore;
using Models;
using Database;

namespace Services
{
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
        var deviceIds = await _context.Readings
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
          var kWh = await CalculateEnergyConsumption(deviceId, periodStart, periodEnd);

          if (kWh == 0)
          {
            _logger.LogInformation($"No energy consumption calculated for device {deviceId} during the period {periodStart} to {periodEnd}.");
            continue; // Skip saving if no consumption was calculated
          }

          var record = new EnergyConsumption
          {
            DeviceId = deviceId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            kWhConsumption = kWh,
            PeriodType = periodType
          };

          _context.EnergyConsumptions.Add(record);
        }
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        // Log the exception (you can use a logging framework like Serilog or NLog)
        Console.WriteLine($"Error during calculations: {ex.Message}");
      }
    }

    // This helper method calculates energy consumption in kWh for a given device and time period
    public async Task<double> CalculateEnergyConsumption(string deviceId, DateTime periodStart, DateTime periodEnd)
    {
      var readings = await _context.Readings
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
  }
}
