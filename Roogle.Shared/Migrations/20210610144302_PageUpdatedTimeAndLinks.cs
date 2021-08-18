using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Roogle.RoogleSpider.Migrations
{
    public partial class PageUpdatedTimeAndLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ContentsChanged",
                table: "Pages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PageRank",
                table: "Pages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PageRankDirty",
                table: "Pages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "Pages",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FromPage = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ToPage = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LastSeenTime = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Links", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Links_FromPage_ToPage",
                table: "Links",
                columns: new[] { "FromPage", "ToPage" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Links");

            migrationBuilder.DropColumn(
                name: "ContentsChanged",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "PageRank",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "PageRankDirty",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "Pages");
        }
    }
}
