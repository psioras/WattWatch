using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WattWatch.Migrations
{
    /// <inheritdoc />
    public partial class LiveConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceTableId = table.Column<int>(type: "integer", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    kWhToday = table.Column<double>(type: "double precision", nullable: false),
                    CostToday = table.Column<double>(type: "double precision", nullable: false),
                    kWhCurrentMonth = table.Column<double>(type: "double precision", nullable: false),
                    CostCurrentMonth = table.Column<double>(type: "double precision", nullable: false),
                    kWhCurrentYear = table.Column<double>(type: "double precision", nullable: false),
                    CostCurrentYear = table.Column<double>(type: "double precision", nullable: false),
                    kWhAllTime = table.Column<double>(type: "double precision", nullable: false),
                    CostAllTime = table.Column<double>(type: "double precision", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveConsumptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveConsumptions_Devices_DeviceTableId",
                        column: x => x.DeviceTableId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiveConsumptions_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LiveConsumptions_DeviceTableId",
                table: "LiveConsumptions",
                column: "DeviceTableId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiveConsumptions_LocationId",
                table: "LiveConsumptions",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveConsumptions");
        }
    }
}
