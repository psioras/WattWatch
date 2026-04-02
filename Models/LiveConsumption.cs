namespace Models;

public class LiveConsumption
{
  public int Id { get; set; }

  public int DeviceTableId { get; set; }
  public Device Device { get; set; } = null!;

  public int LocationId { get; set; }
  public Location Location { get; set; } = null!;

  public double kWhToday { get; set; }
  public double CostToday { get; set; }

  public double kWhCurrentMonth { get; set; }
  public double CostCurrentMonth { get; set; }

  public double kWhCurrentYear { get; set; }
  public double CostCurrentYear { get; set; }

  public double kWhAllTime { get; set; }
  public double CostAllTime { get; set; }

  public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
