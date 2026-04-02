using NCronJob;
using Services;

namespace Jobs;

public class LiveEnergyJob : IJob
{
  private readonly CalculationsService _calculationsService;
  private readonly ILogger<LiveEnergyJob> _logger;

  public LiveEnergyJob(CalculationsService calculationsService, ILogger<LiveEnergyJob> logger)
  {
    _calculationsService = calculationsService;
    _logger = logger;
  }

  public async Task RunAsync(IJobExecutionContext context, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Executing LiveEnergyJob at {Time}", DateTimeOffset.Now);
    await _calculationsService.UpdateLiveConsumptionAsync();
    _logger.LogInformation("Finished executing LiveEnergyJob at {Time}", DateTimeOffset.Now);
  }
}

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
    _logger.LogInformation("Executing DailyEnergyJob at {Time}", DateTimeOffset.Now);
    await _calculationsService.TransferLiveToEnergyConsumptionAsync("Daily");
    _logger.LogInformation("Finished executing DailyEnergyJob at {Time}", DateTimeOffset.Now);
  }
}

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
    _logger.LogInformation("Executing MonthlyEnergyJob at {Time}", DateTimeOffset.Now);
    await _calculationsService.TransferLiveToEnergyConsumptionAsync("Monthly");
    _logger.LogInformation("Finished executing MonthlyEnergyJob at {Time}", DateTimeOffset.Now);
  }
}

public class YearlyEnergyJob : IJob
{
  private readonly CalculationsService _calculationsService;
  private readonly ILogger<YearlyEnergyJob> _logger;

  public YearlyEnergyJob(CalculationsService calculationsService, ILogger<YearlyEnergyJob> logger)
  {
    _calculationsService = calculationsService;
    _logger = logger;
  }

  public async Task RunAsync(IJobExecutionContext context, CancellationToken cancellationToken)
  {
    _logger.LogInformation("Executing YearlyEnergyJob at {Time}", DateTimeOffset.Now);
    await _calculationsService.TransferLiveToEnergyConsumptionAsync("Yearly");
    _logger.LogInformation("Finished executing YearlyEnergyJob at {Time}", DateTimeOffset.Now);
  }
}
