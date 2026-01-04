using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reporting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_stats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalReservations = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedCount = table.Column<int>(type: "integer", nullable: false),
                    CancelledCount = table.Column<int>(type: "integer", nullable: false),
                    CompletedCount = table.Column<int>(type: "integer", nullable: false),
                    NoShowCount = table.Column<int>(type: "integer", nullable: false),
                    OccupancyRate = table.Column<decimal>(type: "numeric", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_stats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reservation_summaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation_summaries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_stats_Date",
                table: "daily_stats",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservation_summaries_ClientId",
                table: "reservation_summaries",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_summaries_ReservationId",
                table: "reservation_summaries",
                column: "ReservationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reservation_summaries_Start",
                table: "reservation_summaries",
                column: "Start");

            migrationBuilder.CreateIndex(
                name: "IX_reservation_summaries_Status",
                table: "reservation_summaries",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_stats");

            migrationBuilder.DropTable(
                name: "reservation_summaries");
        }
    }
}
