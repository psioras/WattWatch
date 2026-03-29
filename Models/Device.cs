namespace Models;

public class Device
{
  public int Id { get; set; }
  required public string DeviceId { get; set; }
  public string? FriendlyName { get; set; }
  public string DeviceType { get; set; } = "electricity";
  public int LocationId { get; set; }
  public Location Location { get; set; } = null!;

  public ICollection<DeviceReading> Readings { get; set; } = [];
  public ICollection<EnergyConsumption> EnergyConsumptions { get; set; } = [];
}
