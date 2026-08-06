using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenDXP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BlocksJson = table.Column<string>(type: "jsonb", nullable: false),
                    TargetSegment = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrafficPercentage = table.Column<int>(type: "integer", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageVariants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VariantAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: true),
                    VariantLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Impressions = table.Column<int>(type: "integer", nullable: false),
                    Conversions = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariantAnalytics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PageVariants_PageId",
                table: "PageVariants",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_VariantAnalytics_PageId_VariantId",
                table: "VariantAnalytics",
                columns: new[] { "PageId", "VariantId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageVariants");

            migrationBuilder.DropTable(
                name: "VariantAnalytics");
        }
    }
}
