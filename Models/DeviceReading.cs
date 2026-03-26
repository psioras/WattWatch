namespace Models;

public class DeviceReading
{
  public int Id { get; set; }
  required public string DeviceId { get; set; }
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
  public double Wattage { get; set; }
}
