using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddMissingProductModuleTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Position",
            table: "VariationOption",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer"
        );

        migrationBuilder.AlterColumn<int>(
            name: "Position",
            table: "Variation",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer"
        );

        migrationBuilder.AlterColumn<int>(
            name: "SellingPrice",
            table: "ProductVariant",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer"
        );

        migrationBuilder.AlterColumn<int>(
            name: "CostPrice",
            table: "ProductVariant",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer"
        );

        migrationBuilder.AddColumn<string>(
            name: "FranchiseId",
            table: "Product",
            type: "text",
            nullable: true
        );

        migrationBuilder.CreateTable(
            name: "Franchise",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Slug = table.Column<string>(type: "text", nullable: true),
                Description = table.Column<string>(type: "text", nullable: true),
                LogoUrl = table.Column<string>(type: "text", nullable: true),
                BannerUrl = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
                UpdatedAt = table.Column<DateTimeOffset>(
                    type: "timestamp with time zone",
                    nullable: false
                ),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Franchise", x => x.Id);
            }
        );

        migrationBuilder.CreateTable(
            name: "ProductImage",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                ProductId = table.Column<string>(type: "text", nullable: false),
                Path = table.Column<string>(type: "text", nullable: false),
                Alt = table.Column<string>(type: "text", nullable: true),
                Position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductImage", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductImage_Product_ProductId",
                    column: x => x.ProductId,
                    principalTable: "Product",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateTable(
            name: "ProductVariantImage",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                ProductVariantId = table.Column<string>(type: "text", nullable: false),
                Path = table.Column<string>(type: "text", nullable: false),
                Alt = table.Column<string>(type: "text", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProductVariantImage", x => x.Id);
                table.ForeignKey(
                    name: "FK_ProductVariantImage_ProductVariant_ProductVariantId",
                    column: x => x.ProductVariantId,
                    principalTable: "ProductVariant",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariant_Sku",
            table: "ProductVariant",
            column: "Sku",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_Product_FranchiseId",
            table: "Product",
            column: "FranchiseId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_Franchise_Name",
            table: "Franchise",
            column: "Name",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_Franchise_Slug",
            table: "Franchise",
            column: "Slug",
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_ProductImage_ProductId",
            table: "ProductImage",
            column: "ProductId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_ProductImage_ProductId_Position",
            table: "ProductImage",
            columns: new[] { "ProductId", "Position" },
            unique: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariantImage_ProductVariantId",
            table: "ProductVariantImage",
            column: "ProductVariantId"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_Product_Franchise_FranchiseId",
            table: "Product",
            column: "FranchiseId",
            principalTable: "Franchise",
            principalColumn: "Id"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Product_Franchise_FranchiseId", table: "Product");

        migrationBuilder.DropTable(name: "Franchise");

        migrationBuilder.DropTable(name: "ProductImage");

        migrationBuilder.DropTable(name: "ProductVariantImage");

        migrationBuilder.DropIndex(name: "IX_ProductVariant_Sku", table: "ProductVariant");

        migrationBuilder.DropIndex(name: "IX_Product_FranchiseId", table: "Product");

        migrationBuilder.DropColumn(name: "FranchiseId", table: "Product");

        migrationBuilder.AlterColumn<int>(
            name: "Position",
            table: "VariationOption",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer",
            oldDefaultValue: 0
        );

        migrationBuilder.AlterColumn<int>(
            name: "Position",
            table: "Variation",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer",
            oldDefaultValue: 0
        );

        migrationBuilder.AlterColumn<int>(
            name: "SellingPrice",
            table: "ProductVariant",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer",
            oldDefaultValue: 0
        );

        migrationBuilder.AlterColumn<int>(
            name: "CostPrice",
            table: "ProductVariant",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer",
            oldDefaultValue: 0
        );
    }
}
