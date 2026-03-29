namespace Models;

public class LocationPrice
{
  public int Id { get; set; }
  public int LocationId { get; set; }
  public Location Location { get; set; } = null!;
  public double PricePerKwh { get; set; }
  public DateTime EffectiveFrom { get; set; }
}
