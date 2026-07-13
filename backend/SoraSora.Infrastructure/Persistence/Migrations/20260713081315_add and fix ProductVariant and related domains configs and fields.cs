using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addandfixProductVariantandrelateddomainsconfigsandfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariantImage_ProductVariantId",
                table: "ProductVariantImage");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariant_ProductId",
                table: "ProductVariant");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariant_Sku",
                table: "ProductVariant");

            migrationBuilder.DropIndex(
                name: "IX_ProductImage_ProductId",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "DepositPrice",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "IsDepositRequired",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "IsFullPaymentAllowed",
                table: "ProductVariant");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "ProductVariant");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "ProductVariantImage",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Alt",
                table: "ProductVariantImage",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Position",
                table: "ProductVariant",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductVariant",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "ProductVariant",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "ProductImage",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Alt",
                table: "ProductImage",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Product",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublished",
                table: "Product",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Product",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasVariation",
                table: "Product",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Product",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SellMode",
                table: "Product",
                type: "text",
                nullable: false,
                defaultValue: "IN_STOCK");

            migrationBuilder.AddColumn<string>(
                name: "SupplierCountry",
                table: "Product",
                type: "text",
                nullable: false,
                defaultValue: "NONE");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantImage_ProductVariantId",
                table: "ProductVariantImage",
                column: "ProductVariantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariant_ProductId_Position",
                table: "ProductVariant",
                columns: new[] { "ProductId", "Position" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductVariant_CostPrice",
                table: "ProductVariant",
                sql: "\"CostPrice\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductVariant_Position",
                table: "ProductVariant",
                sql: "\"Position\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductVariant_SellingPrice",
                table: "ProductVariant",
                sql: "\"SellingPrice\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductImage_Position",
                table: "ProductImage",
                sql: "\"Position\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariantImage_ProductVariantId",
                table: "ProductVariantImage");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariant_ProductId_Position",
                table: "ProductVariant");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductVariant_CostPrice",
                table: "ProductVariant");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductVariant_Position",
                table: "ProductVariant");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductVariant_SellingPrice",
                table: "ProductVariant");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductImage_Position",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "HasVariation",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "SellMode",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "SupplierCountry",
                table: "Product");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "ProductVariantImage",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "Alt",
                table: "ProductVariantImage",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Position",
                table: "ProductVariant",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ProductVariant",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsArchived",
                table: "ProductVariant",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DepositPrice",
                table: "ProductVariant",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDepositRequired",
                table: "ProductVariant",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFullPaymentAllowed",
                table: "ProductVariant",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "ProductVariant",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "ProductImage",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "Alt",
                table: "ProductImage",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Product",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPublished",
                table: "Product",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Product",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantImage_ProductVariantId",
                table: "ProductVariantImage",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariant_ProductId",
                table: "ProductVariant",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariant_Sku",
                table: "ProductVariant",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImage_ProductId",
                table: "ProductImage",
                column: "ProductId");
        }
    }
}
