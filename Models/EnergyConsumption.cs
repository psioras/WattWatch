namespace Models;

public class EnergyConsumption
{
  public int Id { get; set; }
  required public string DeviceId { get; set; }
  public int DeviceTableId { get; set; }
  public Device Device { get; set; } = null!;
  public int LocationId { get; set; }
  public Location Location { get; set; } = null!;
  public DateTime PeriodStart { get; set; }
  public DateTime PeriodEnd { get; set; }
  public double kWhConsumption { get; set; }
  public double CostEuros { get; set; }
  public string PeriodType { get; set; } = string.Empty; // e.g., "Daily", "Weekly", "Monthly"
  public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
