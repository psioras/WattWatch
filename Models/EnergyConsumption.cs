namespace Models;

public class EnergyConsumption
{
  public int Id { get; set; }
  required public string DeviceId { get; set; }
  public DateTime PeriodStart { get; set; }
  public DateTime PeriodEnd { get; set; }
  public double kWhConsumption { get; set; }
  public string PeriodType { get; set; } // e.g., "Daily", "Weekly", "Monthly"
  public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
