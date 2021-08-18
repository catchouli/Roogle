using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Roogle.RoogleSpider.Migrations
{
    public partial class ChangePageRankDirtyToUpdateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageRankDirty",
                table: "Pages");

            migrationBuilder.AddColumn<DateTime>(
                name: "PageRankUpdatedTime",
                table: "Pages",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageRankUpdatedTime",
                table: "Pages");

            migrationBuilder.AddColumn<bool>(
                name: "PageRankDirty",
                table: "Pages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
