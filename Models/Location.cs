namespace Models;

public class Location
{
  public int Id { get; set; }
  required public string Name { get; set; }
  public string? Address { get; set; }

  public ICollection<Device> Devices { get; set; } = [];
  public ICollection<LocationPrice> Prices { get; set; } = [];
  public ICollection<EnergyConsumption> EnergyConsumptions { get; set; } = [];
}
