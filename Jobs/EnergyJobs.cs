using NCronJob;
using Services;

namespace Jobs;

// --------------------------------- DAILY ENERGY CONSUMPTION CALCULATION ---------------------------------
// This job calculates the daily energy consumption for all devices and saves the results to the database.

public class DailyEnergyJob : IJob
{
  private readonly CalculationsService _calculationsService;
  private readonly ILogger<DailyEnergyJob> _logger;

  public DailyEnergyJob(CalculationsService calculationsService, ILogger<DailyEnergyJob> logger)
  {
    _calculationsService = calculationsService;
    _logger = logger;
  }

  public async Task RunAsync(IJobExecutionContext context, CancellationToken cancellationToken)
  {
    // Special handling for Athens timezone to ensure calculations run at the correct local time.
    var athensZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Athens");
    var nowInAthens = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, athensZone);
    _logger.LogInformation($"Starting daily energy consumption calculation at {nowInAthens} (Athens time).");

    //Calculate boundaries for the previous day in Athens time.
    var endAthens = nowInAthens.Date; // Start of the current day in Athens time.
    var startAthens = endAthens.AddDays(-1); // Start of the previous day in Athens time.

    // Convert the Athens time boundaries back to UTC for calculations.
    var StartUtc = TimeZoneInfo.ConvertTimeToUtc(startAthens, athensZone);
    var EndUtc = TimeZoneInfo.ConvertTimeToUtc(endAthens, athensZone);

    await _calculationsService.RunCalculationsAsync("Daily", StartUtc, EndUtc);
    _logger.LogInformation("Completed daily energy consumption calculation.");
  }
}

// --------------------------------- WEEKLY ENERGY CONSUMPTION CALCULATION ---------------------------------
// This job calculates the weekly energy consumption for all devices and saves the results to the database.

public class WeeklyEnergyJob : IJob
{
  private readonly CalculationsService _calculationsService;
  private readonly ILogger<WeeklyEnergyJob> _logger;

  public WeeklyEnergyJob(CalculationsService calculationsService, ILogger<WeeklyEnergyJob> logger)
  {
    _calculationsService = calculationsService;
    _logger = logger;
  }

  public async Task RunAsync(IJobExecutionContext context, CancellationToken cancellationToken)
  {
    var athensZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Athens");
    var nowInAthens = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, athensZone);
    _logger.LogInformation($"Starting weekly energy consumption calculation at {nowInAthens} (Athens time).");

    var endAthens = nowInAthens.Date; // Start of the current day in Athens time.
    var startAthens = endAthens.AddDays(-7); // Start of the previous

    var StartUtc = TimeZoneInfo.ConvertTimeToUtc(startAthens, athensZone);
    var EndUtc = TimeZoneInfo.ConvertTimeToUtc(endAthens, athensZone);

    await _calculationsService.RunCalculationsAsync("Weekly", StartUtc, EndUtc);
    _logger.LogInformation("Completed weekly energy consumption calculation.");
  }
}


// --------------------------------- MONTHLY ENERGY CONSUMPTION CALCULATION ---------------------------------
// This job calculates the monthly energy consumption for all devices and saves the results to the database.

public class MonthlyEnergyJob : IJob
{
  private readonly CalculationsService _calculationsService;
  private readonly ILogger<MonthlyEnergyJob> _logger;

  public MonthlyEnergyJob(CalculationsService calculationsService, ILogger<MonthlyEnergyJob> logger)
  {
    _calculationsService = calculationsService;
    _logger = logger;
  }

  public async Task RunAsync(IJobExecutionContext context, CancellationToken cancellationToken)
  {
    var athensZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Athens");
    var nowInAthens = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, athensZone);
    _logger.LogInformation($"Starting monthly energy consumption calculation at {nowInAthens} (Athens time).");

    var endAthens = nowInAthens.Date; // Start of the current day in Athens time.
    var startAthens = endAthens.AddMonths(-1); // Start of the previous month in Athens time.

    var StartUtc = TimeZoneInfo.ConvertTimeToUtc(startAthens, athensZone);
    var EndUtc = TimeZoneInfo.ConvertTimeToUtc(endAthens, athensZone);

    await _calculationsService.RunCalculationsAsync("Monthly", StartUtc, EndUtc);
    _logger.LogInformation("Completed monthly energy consumption calculation.");
  }
}

