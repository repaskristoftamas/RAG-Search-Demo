using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace BookStore.SemanticSearch.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "BookEmbeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Author = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(768)", nullable: false),
                    IndexedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookEmbeddings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookEmbeddings_Embedding",
                table: "BookEmbeddings",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "ivfflat")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:lists", 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookEmbeddings");
        }
    }
}
