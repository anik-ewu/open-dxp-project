using Microsoft.EntityFrameworkCore.Migrations;
using OpenDXP.Application.Search;

#nullable disable

namespace OpenDXP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Not part of the EF model (see PageSearchEntryConfiguration) - the pgvector EF Core
            // plugin needs EF Core 9, which conflicts with this solution's EF 8 stack. Managed via
            // raw SQL instead; Npgsql's own vector type handling still applies at the ADO.NET level.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");
            migrationBuilder.Sql(
                $"ALTER TABLE \"PageSearchEntries\" ADD COLUMN \"Embedding\" vector({HashingEmbeddingService.Dimensions});");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"PageSearchEntries\" DROP COLUMN \"Embedding\";");
        }
    }
}
