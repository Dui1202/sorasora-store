using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations;

[DbContext(typeof(SoraSoraDbContext))]
[Migration("20260728000000_MakeProductVariantSkuOptional")]
public partial class MakeProductVariantSkuOptional : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_ProductVariant_Sku", table: "ProductVariant");

        migrationBuilder.AlterColumn<string>(
            name: "Sku",
            table: "ProductVariant",
            type: "text",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text"
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            WITH ranked_skus AS (
                SELECT
                    "Id",
                    "Sku",
                    ROW_NUMBER() OVER (PARTITION BY "Sku" ORDER BY "Id") AS "RowNumber"
                FROM "ProductVariant"
            )
            UPDATE "ProductVariant" AS variant
            SET "Sku" =
                COALESCE(NULLIF(variant."Sku", ''), 'ROLLBACK')
                || '-'
                || variant."Id"
            FROM ranked_skus
            WHERE variant."Id" = ranked_skus."Id"
              AND (variant."Sku" IS NULL OR ranked_skus."RowNumber" > 1);
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

        migrationBuilder.CreateIndex(
            name: "IX_ProductVariant_Sku",
            table: "ProductVariant",
            column: "Sku",
            unique: true
        );
    }
}
