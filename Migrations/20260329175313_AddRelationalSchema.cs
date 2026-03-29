using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WattWatch.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Step 1: Rename Readings → ElectricityReadings ────────────────
            migrationBuilder.DropPrimaryKey(
                name: "PK_Readings",
                table: "Readings");

            migrationBuilder.RenameTable(
                name: "Readings",
                newName: "ElectricityReadings");

            migrationBuilder.RenameIndex(
                name: "IX_Readings_Timestamp",
                table: "ElectricityReadings",
                newName: "IX_ElectricityReadings_Timestamp");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ElectricityReadings",
                table: "ElectricityReadings",
                column: "Id");

            // ── Step 2: Create Locations table ───────────────────────────────
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            // ── Step 3: Create LocationPrices table ──────────────────────────
            migrationBuilder.CreateTable(
                name: "LocationPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    PricePerKwh = table.Column<double>(type: "double precision", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationPrices_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── Step 4: Seed default "Home" location ─────────────────────────
            migrationBuilder.Sql(
                """
                INSERT INTO "Locations" ("Name", "Address") VALUES ('Home', NULL);
                """);

            // ── Step 5: Create Devices table ─────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    DeviceType = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── Step 6: Backfill Devices from distinct DeviceIds ─────────────
            // Inserts one Device row per distinct DeviceId found in ElectricityReadings,
            // all assigned to the default 'Home' location.
            migrationBuilder.Sql(
                """
                INSERT INTO "Devices" ("DeviceId", "FriendlyName", "DeviceType", "LocationId")
                SELECT DISTINCT "DeviceId", NULL, 'electricity',
                    (SELECT "Id" FROM "Locations" WHERE "Name" = 'Home' LIMIT 1)
                FROM "ElectricityReadings";
                """);

            // ── Step 7: Add FK columns as NULLABLE to allow backfill ─────────
            migrationBuilder.AddColumn<int>(
                name: "DeviceTableId",
                table: "ElectricityReadings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeviceTableId",
                table: "EnergyConsumptions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "EnergyConsumptions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CostEuros",
                table: "EnergyConsumptions",
                type: "double precision",
                nullable: true);

            // ── Step 8: Backfill ElectricityReadings.DeviceTableId ───────────
            migrationBuilder.Sql(
                """
                UPDATE "ElectricityReadings" r
                SET "DeviceTableId" = d."Id"
                FROM "Devices" d
                WHERE d."DeviceId" = r."DeviceId";
                """);

            // ── Step 9: Backfill EnergyConsumptions FK columns ───────────────
            // DeviceTableId and LocationId are resolved via the Devices table.
            // CostEuros is set to 0 for all historical rows (no price data available).
            migrationBuilder.Sql(
                """
                UPDATE "EnergyConsumptions" ec
                SET "DeviceTableId" = d."Id",
                    "LocationId"    = d."LocationId",
                    "CostEuros"     = 0
                FROM "Devices" d
                WHERE d."DeviceId" = ec."DeviceId";
                """);

            // ── Step 10: Make columns NOT NULL now that backfill is complete ──
            migrationBuilder.AlterColumn<int>(
                name: "DeviceTableId",
                table: "ElectricityReadings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeviceTableId",
                table: "EnergyConsumptions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LocationId",
                table: "EnergyConsumptions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "CostEuros",
                table: "EnergyConsumptions",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            // ── Step 11: Add FK constraints ───────────────────────────────────
            migrationBuilder.AddForeignKey(
                name: "FK_ElectricityReadings_Devices_DeviceTableId",
                table: "ElectricityReadings",
                column: "DeviceTableId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EnergyConsumptions_Devices_DeviceTableId",
                table: "EnergyConsumptions",
                column: "DeviceTableId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EnergyConsumptions_Locations_LocationId",
                table: "EnergyConsumptions",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ── Step 12: Add indexes ──────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_ElectricityReadings_DeviceTableId",
                table: "ElectricityReadings",
                column: "DeviceTableId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyConsumptions_DeviceTableId",
                table: "EnergyConsumptions",
                column: "DeviceTableId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyConsumptions_LocationId",
                table: "EnergyConsumptions",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceId",
                table: "Devices",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_LocationId",
                table: "Devices",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationPrices_EffectiveFrom",
                table: "LocationPrices",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_LocationPrices_LocationId",
                table: "LocationPrices",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK constraints first
            migrationBuilder.DropForeignKey(
                name: "FK_ElectricityReadings_Devices_DeviceTableId",
                table: "ElectricityReadings");

            migrationBuilder.DropForeignKey(
                name: "FK_EnergyConsumptions_Devices_DeviceTableId",
                table: "EnergyConsumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_EnergyConsumptions_Locations_LocationId",
                table: "EnergyConsumptions");

            // Drop indexes on modified tables
            migrationBuilder.DropIndex(
                name: "IX_ElectricityReadings_DeviceTableId",
                table: "ElectricityReadings");

            migrationBuilder.DropIndex(
                name: "IX_EnergyConsumptions_DeviceTableId",
                table: "EnergyConsumptions");

            migrationBuilder.DropIndex(
                name: "IX_EnergyConsumptions_LocationId",
                table: "EnergyConsumptions");

            // Drop new columns from existing tables
            migrationBuilder.DropColumn(
                name: "DeviceTableId",
                table: "ElectricityReadings");

            migrationBuilder.DropColumn(
                name: "CostEuros",
                table: "EnergyConsumptions");

            migrationBuilder.DropColumn(
                name: "DeviceTableId",
                table: "EnergyConsumptions");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "EnergyConsumptions");

            // Drop new tables (order respects FK dependencies)
            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "LocationPrices");

            migrationBuilder.DropTable(
                name: "Locations");

            // Rename ElectricityReadings back to Readings
            migrationBuilder.DropPrimaryKey(
                name: "PK_ElectricityReadings",
                table: "ElectricityReadings");

            migrationBuilder.RenameTable(
                name: "ElectricityReadings",
                newName: "Readings");

            migrationBuilder.RenameIndex(
                name: "IX_ElectricityReadings_Timestamp",
                table: "Readings",
                newName: "IX_Readings_Timestamp");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Readings",
                table: "Readings",
                column: "Id");
        }
    }
}
