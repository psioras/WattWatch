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

  // This method calculates energy consumption and cost for a given device and time period, then saves the results to the LiveConsumption table.
  public async Task UpdateLiveConsumptionAsync()
  {
    var devices = await _context.Devices.AsNoTracking().ToListAsync();

    var AthensZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Athens");
    var nowUtc = DateTime.UtcNow;
    var nowInAthens = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, AthensZone);

    // Time calculations. We need to convert the periodStart and periodEnd from UTC to Athens time to determine if they fall within the current day or month in Athens time, which is what we want to display in the LiveConsumption table.
    var midnightAthens = nowInAthens.Date; // Start of the current day in Athens time.
    var startOfMonthAthens = new DateTime(nowInAthens.Year, nowInAthens.Month, 1); // Start of the current month in Athens time.
    var startOfYearAthens = new DateTime(nowInAthens.Year, 1, 1); // Start of the current year in Athens time.
    var allTimeStart = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Arbitrary early date to represent "all time". Hardcoded to save a SQL query to get the earliest reading timestamp.
    var midnightUtc = TimeZoneInfo.ConvertTimeToUtc(midnightAthens, AthensZone);
    var startOfMonthUtc = TimeZoneInfo.ConvertTimeToUtc(startOfMonthAthens, AthensZone);
    var startOfYearUtc = TimeZoneInfo.ConvertTimeToUtc(startOfYearAthens, AthensZone);

    foreach (var device in devices)
    {
      // Calculate energy consumption and cost for today, this month, and all time.
      //
      var kWhToday = await CalculateEnergyConsumption(device.DeviceId, midnightUtc, nowUtc);
      var costToday = await CalculateCostAsync(device.LocationId, kWhToday, nowUtc);
      var kWhCurrentMonth = await CalculateEnergyConsumption(device.DeviceId, startOfMonthUtc, nowUtc);
      var costCurrentMonth = await CalculateCostAsync(device.LocationId, kWhCurrentMonth, nowUtc);
      var kWhCurrentYear = await CalculateEnergyConsumption(device.DeviceId, startOfYearUtc, nowUtc);
      var costCurrentYear = await CalculateCostAsync(device.LocationId, kWhCurrentYear, nowUtc);
      var kWhAllTime = await CalculateEnergyConsumption(device.DeviceId, allTimeStart, nowUtc);
      var costAllTime = await CalculateCostAsync(device.LocationId, kWhAllTime, nowUtc);

      var liveConsumption = new LiveConsumption
      {
        DeviceTableId = device.Id,
        LocationId = device.LocationId,
        kWhToday = kWhToday,
        CostToday = costToday,
        kWhCurrentMonth = kWhCurrentMonth,
        CostCurrentMonth = costCurrentMonth,
        kWhCurrentYear = kWhCurrentYear,
        CostCurrentYear = costCurrentYear,
        kWhAllTime = kWhAllTime,
        CostAllTime = costAllTime
      };
      var existingRecord = await _context.LiveConsumptions.FirstOrDefaultAsync(lc => lc.DeviceTableId == device.Id);
      if (existingRecord == null)
      {
        _logger.LogInformation($"Created the first live consumption record for device {device.DeviceId}.");
        _context.LiveConsumptions.Add(liveConsumption);
      }
      else
      {
        existingRecord.kWhToday = kWhToday;
        existingRecord.CostToday = costToday;
        existingRecord.kWhCurrentMonth = kWhCurrentMonth;
        existingRecord.CostCurrentMonth = costCurrentMonth;
        existingRecord.kWhCurrentYear = kWhCurrentYear;
        existingRecord.CostCurrentYear = costCurrentYear;
        existingRecord.kWhAllTime = kWhAllTime;
        existingRecord.CostAllTime = costAllTime;
        existingRecord.LastUpdated = DateTime.UtcNow;

        _logger.LogInformation($"Updated live consumption record for device {device.DeviceId}.");
      }
    }
    await _context.SaveChangesAsync();
  }

  // This heper method takes data from database LiveConsumption table and moves them over to EnergyConsumption table, to save Daily and Monthly energy consumption results.
  public async Task TransferLiveToEnergyConsumptionAsync(string periodType)
  {
    await UpdateLiveConsumptionAsync(); // Ensure LiveConsumption is up to date before transferring data

    var liveConsumptions = await _context.LiveConsumptions
      .Include(lc => lc.Device)
      .ToListAsync();

    var AthensZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Athens");
    var nowinAthens = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, AthensZone);

    var periodEndAthens = nowinAthens.Date; // Start of the current day in Athens
    DateTime periodStartAthens = periodType switch
    {
      "Daily" => periodEndAthens.AddDays(-1), // Start of the previous day in Athens
      "Monthly" => periodEndAthens.AddMonths(-1), // Start of the previous month in Athens
      "Yearly" => periodEndAthens.AddYears(-1), // Start of the previous year in Athens
      _ => throw new ArgumentException("Invalid period type. Must be 'Daily' or 'Monthly' or 'Yearly'.")
    };
    DateTime periodStartUtc = TimeZoneInfo.ConvertTimeToUtc(periodStartAthens, AthensZone);
    DateTime periodEndUtc = TimeZoneInfo.ConvertTimeToUtc(periodEndAthens, AthensZone);

    foreach (var lc in liveConsumptions)
    {
      var kWh = periodType == "Daily" ? lc.kWhToday
        : periodType == "Monthly" ? lc.kWhCurrentMonth
        : lc.kWhCurrentYear;
      var cost = periodType == "Daily" ? lc.CostToday
        : periodType == "Monthly" ? lc.CostCurrentMonth
        : lc.CostCurrentYear;

      var energyConsumption = new EnergyConsumption
      {
        DeviceId = lc.Device.DeviceId,
        DeviceTableId = lc.DeviceTableId,
        LocationId = lc.LocationId,
        PeriodStart = periodStartUtc,
        PeriodEnd = periodEndUtc,
        kWhConsumption = kWh,
        CostEuros = cost,
        PeriodType = periodType
      };
      _context.EnergyConsumptions.Add(energyConsumption);
    }
    await _context.SaveChangesAsync();
  }

  // This helper method calculates energy consumption in kWh for a given device and time period
  private async Task<double> CalculateEnergyConsumption(string deviceId, DateTime periodStart, DateTime periodEnd)
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
