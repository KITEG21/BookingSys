using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Policy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "client_blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    BlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_blocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "client_violations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViolationType = table.Column<string>(type: "text", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_violations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_client_blocks_ClientId",
                table: "client_blocks",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_client_blocks_ClientId_IsActive",
                table: "client_blocks",
                columns: new[] { "ClientId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_client_violations_ClientId",
                table: "client_violations",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_client_violations_ClientId_ViolationType",
                table: "client_violations",
                columns: new[] { "ClientId", "ViolationType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_blocks");

            migrationBuilder.DropTable(
                name: "client_violations");
        }
    }
}
