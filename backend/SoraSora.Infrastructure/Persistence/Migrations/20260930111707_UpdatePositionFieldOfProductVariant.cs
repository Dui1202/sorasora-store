using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePositionFieldOfProductVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariant_ProductId_Position",
                table: "ProductVariant");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductVariant_Position",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "InventoryItems");

            migrationBuilder.AlterColumn<int>(
                name: "Position",
                table: "ProductVariant",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "ProductVariant",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "InventoryItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariant_ClientId",
                table: "ProductVariant",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariant_ProductId_Position",
                table: "ProductVariant",
                columns: new[] { "ProductId", "Position" },
                unique: true,
                filter: "\"IsArchived\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductVariant_Position",
                table: "ProductVariant",
                sql: "(\"IsArchived\" AND \"Position\" IS NULL) OR (NOT \"IsArchived\" AND \"Position\" IS NOT NULL AND \"Position\" >= 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariant_ClientId",
                table: "ProductVariant");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariant_ProductId_Position",
                table: "ProductVariant");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductVariant_Position",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "InventoryItems");

            migrationBuilder.AlterColumn<int>(
                name: "Position",
                table: "ProductVariant",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClientId",
                table: "ProductVariant",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "InventoryItems",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariant_ProductId_Position",
                table: "ProductVariant",
                columns: new[] { "ProductId", "Position" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductVariant_Position",
                table: "ProductVariant",
                sql: "\"Position\" >= 0");
        }
    }
}
