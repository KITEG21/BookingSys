using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    EventData = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    Actor = table.Column<string>(type: "text", nullable: true),
                    SourceService = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_CorrelationId",
                table: "audit_entries",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_EntityId",
                table: "audit_entries",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_EventType",
                table: "audit_entries",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_SourceService",
                table: "audit_entries",
                column: "SourceService");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entries_Timestamp",
                table: "audit_entries",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_entries");
        }
    }
}
