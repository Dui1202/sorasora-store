using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class RestoreProductVariantSkuAndUpdateDescriptionLimit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Sku",
            table: "ProductVariant",
            type: "text",
            nullable: true
        );

        migrationBuilder.Sql(
            """
            UPDATE "ProductVariant"
            SET "Sku" = 'LEGACY-' || "Id"
            WHERE "Sku" IS NULL;
            """
        );

        migrationBuilder.AlterColumn<string>(
            name: "Sku",
            table: "ProductVariant",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text",
            oldNullable: true
        );

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            table: "Product",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(1000)",
            oldMaxLength: 1000,
            oldNullable: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariant_Sku",
            table: "ProductVariant",
            column: "Sku",
            unique: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_ProductVariant_Sku", table: "ProductVariant");

        migrationBuilder.DropColumn(name: "Sku", table: "ProductVariant");

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            table: "Product",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(2000)",
            oldMaxLength: 2000,
            oldNullable: true
        );
    }
}
