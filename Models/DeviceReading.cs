namespace Models;

public class DeviceReading
{
  public int Id { get; set; }
  required public string DeviceId { get; set; }
  public int DeviceTableId { get; set; }
  public Device Device { get; set; } = null!;
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
  public double Wattage { get; set; }
}
