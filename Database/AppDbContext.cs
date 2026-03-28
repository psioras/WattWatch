using Models;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class AppDbContext : DbContext
{
  public DbSet<DeviceReading> Readings { get; set; }
  public DbSet<EnergyConsumption> EnergyConsumptions { get; set; }

  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    //Add index on Timestamp for faster queries on time series data
    modelBuilder.Entity<DeviceReading>()
      .HasIndex(r => r.Timestamp);
  }
}
