using Models;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class AppDbContext : DbContext
{
  public DbSet<DeviceReading> ElectricityReadings { get; set; }
  public DbSet<EnergyConsumption> EnergyConsumptions { get; set; }
  public DbSet<Location> Locations { get; set; }
  public DbSet<LocationPrice> LocationPrices { get; set; }
  public DbSet<Device> Devices { get; set; }
  public DbSet<LiveConsumption> LiveConsumptions { get; set; }

  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // ── ElectricityReadings ──────────────────────────────────────────────────
    modelBuilder.Entity<DeviceReading>()
      .ToTable("ElectricityReadings");

    // Index on Timestamp for faster time-series queries
    modelBuilder.Entity<DeviceReading>()
      .HasIndex(r => r.Timestamp);

    // Index on DeviceTableId for JOIN queries
    modelBuilder.Entity<DeviceReading>()
      .HasIndex(r => r.DeviceTableId);

    // FK: ElectricityReadings.DeviceTableId → Devices.Id
    modelBuilder.Entity<DeviceReading>()
      .HasOne(r => r.Device)
      .WithMany(d => d.Readings)
      .HasForeignKey(r => r.DeviceTableId)
      .OnDelete(DeleteBehavior.Restrict);

    // ── EnergyConsumptions ───────────────────────────────────────────────────
    modelBuilder.Entity<EnergyConsumption>()
      .HasIndex(ec => ec.DeviceTableId);

    modelBuilder.Entity<EnergyConsumption>()
      .HasIndex(ec => ec.LocationId);

    // FK: EnergyConsumptions.DeviceTableId → Devices.Id
    modelBuilder.Entity<EnergyConsumption>()
      .HasOne(ec => ec.Device)
      .WithMany(d => d.EnergyConsumptions)
      .HasForeignKey(ec => ec.DeviceTableId)
      .OnDelete(DeleteBehavior.Restrict);

    // FK: EnergyConsumptions.LocationId → Locations.Id
    modelBuilder.Entity<EnergyConsumption>()
      .HasOne(ec => ec.Location)
      .WithMany(l => l.EnergyConsumptions)
      .HasForeignKey(ec => ec.LocationId)
      .OnDelete(DeleteBehavior.Restrict);

    // ── Devices ──────────────────────────────────────────────────────────────
    // Unique constraint + index on DeviceId (the hardware identifier string)
    modelBuilder.Entity<Device>()
      .HasIndex(d => d.DeviceId)
      .IsUnique();

    modelBuilder.Entity<Device>()
      .HasIndex(d => d.LocationId);

    // FK: Devices.LocationId → Locations.Id
    modelBuilder.Entity<Device>()
      .HasOne(d => d.Location)
      .WithMany(l => l.Devices)
      .HasForeignKey(d => d.LocationId)
      .OnDelete(DeleteBehavior.Restrict);

    // ── LocationPrices ───────────────────────────────────────────────────────
    modelBuilder.Entity<LocationPrice>()
      .HasIndex(lp => lp.LocationId);

    modelBuilder.Entity<LocationPrice>()
      .HasIndex(lp => lp.EffectiveFrom);

    // FK: LocationPrices.LocationId → Locations.Id
    modelBuilder.Entity<LocationPrice>()
      .HasOne(lp => lp.Location)
      .WithMany(l => l.Prices)
      .HasForeignKey(lp => lp.LocationId)
      .OnDelete(DeleteBehavior.Restrict);

    // FK: LiveConsumptions.DeviceTableId → Devices.Id
    modelBuilder.Entity<LiveConsumption>()
      .HasOne(lc => lc.Device)
      .WithOne()
      .HasForeignKey<LiveConsumption>(lc => lc.DeviceTableId)
      .OnDelete(DeleteBehavior.Restrict);

    // FK: LiveConsumptions.LocationId → Locations.Id
    modelBuilder.Entity<LiveConsumption>()
      .HasOne(lc => lc.Location)
      .WithMany()
      .HasForeignKey(lc => lc.LocationId)
      .OnDelete(DeleteBehavior.Restrict);

    // Unique index on DeviceTableId to ensure one live consumption record per device
    modelBuilder.Entity<LiveConsumption>()
      .HasIndex(lc => lc.DeviceTableId)
      .IsUnique();
  }
}
