using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryDomains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MediaAsset",
                table: "MediaAsset");

            migrationBuilder.DropColumn(
                name: "HasVariation",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "MediaAsset",
                newName: "MediaAssets");

            migrationBuilder.RenameIndex(
                name: "IX_MediaAsset_Path",
                table: "MediaAssets",
                newName: "IX_MediaAssets_Path");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MediaAssets",
                table: "MediaAssets",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MediaAssets",
                table: "MediaAssets");

            migrationBuilder.RenameTable(
                name: "MediaAssets",
                newName: "MediaAsset");

            migrationBuilder.RenameIndex(
                name: "IX_MediaAssets_Path",
                table: "MediaAsset",
                newName: "IX_MediaAsset_Path");

            migrationBuilder.AddColumn<bool>(
                name: "HasVariation",
                table: "Product",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MediaAsset",
                table: "MediaAsset",
                column: "Id");
        }
    }
}
