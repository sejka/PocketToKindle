using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportedArticles",
                columns: table => new
                {
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportedArticles", x => x.Url);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    PocketUsername = table.Column<string>(type: "TEXT", nullable: false),
                    AccessCode = table.Column<string>(type: "TEXT", nullable: false),
                    KindleEmail = table.Column<string>(type: "TEXT", nullable: false),
                    LastProcessingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.PocketUsername);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportedArticles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
