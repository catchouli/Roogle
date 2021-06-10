using Microsoft.EntityFrameworkCore.Migrations;

namespace Roogle.RoogleSpider.Migrations
{
    public partial class AddPageTitle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentsHash",
                table: "Pages",
                newName: "PageHash");

            migrationBuilder.AlterColumn<string>(
                name: "Contents",
                table: "Pages",
                type: "longtext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Pages",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Pages");

            migrationBuilder.RenameColumn(
                name: "PageHash",
                table: "Pages",
                newName: "ContentsHash");

            migrationBuilder.AlterColumn<string>(
                name: "Contents",
                table: "Pages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
