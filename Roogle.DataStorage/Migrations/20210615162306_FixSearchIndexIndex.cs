using Microsoft.EntityFrameworkCore.Migrations;

namespace Roogle.RoogleSpider.Migrations
{
    public partial class FixSearchIndexIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SearchIndex_Page",
                table: "SearchIndex");

            migrationBuilder.AlterColumn<string>(
                name: "Word",
                table: "SearchIndex",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SearchIndex_Word_Page",
                table: "SearchIndex",
                columns: new[] { "Word", "Page" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SearchIndex_Word_Page",
                table: "SearchIndex");

            migrationBuilder.AlterColumn<string>(
                name: "Word",
                table: "SearchIndex",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SearchIndex_Page",
                table: "SearchIndex",
                column: "Page",
                unique: true);
        }
    }
}
