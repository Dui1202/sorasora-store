using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorImageStorageToMediaAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariantOptions_VariationOption_VariationOptionId_Var~",
                table: "ProductVariantOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariantOptions_Variation_VariationId",
                table: "ProductVariantOptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductVariantOptions",
                table: "ProductVariantOptions");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariantOptions_VariationId",
                table: "ProductVariantOptions");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariantOptions_VariationOptionId_VariationId",
                table: "ProductVariantOptions");

            migrationBuilder.DropColumn(
                name: "VariationId",
                table: "ProductVariantOptions");

            migrationBuilder.DropColumn(
                name: "Alt",
                table: "ProductVariantImage");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "ProductVariantImage");

            migrationBuilder.DropColumn(
                name: "Alt",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "ProductImage");

            migrationBuilder.AddColumn<string>(
                name: "MediaAssetId",
                table: "ProductVariantImage",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MediaAssetId",
                table: "ProductImage",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductVariantOptions",
                table: "ProductVariantOptions",
                columns: new[] { "ProductVariantId", "VariationOptionId" });

            migrationBuilder.CreateTable(
                name: "MediaAsset",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAsset", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantImage_MediaAssetId",
                table: "ProductVariantImage",
                column: "MediaAssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImage_MediaAssetId",
                table: "ProductImage",
                column: "MediaAssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaAsset_Path",
                table: "MediaAsset",
                column: "Path",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariantOptions_VariationOption_VariationOptionId",
                table: "ProductVariantOptions",
                column: "VariationOptionId",
                principalTable: "VariationOption",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariantOptions_VariationOption_VariationOptionId",
                table: "ProductVariantOptions");

            migrationBuilder.DropTable(
                name: "MediaAsset");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductVariantOptions",
                table: "ProductVariantOptions");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariantImage_MediaAssetId",
                table: "ProductVariantImage");

            migrationBuilder.DropIndex(
                name: "IX_ProductImage_MediaAssetId",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "ProductVariantImage");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "ProductImage");

            migrationBuilder.AddColumn<string>(
                name: "VariationId",
                table: "ProductVariantOptions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Alt",
                table: "ProductVariantImage",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "ProductVariantImage",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Alt",
                table: "ProductImage",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "ProductImage",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductVariantOptions",
                table: "ProductVariantOptions",
                columns: new[] { "ProductVariantId", "VariationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_VariationId",
                table: "ProductVariantOptions",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_VariationOptionId_VariationId",
                table: "ProductVariantOptions",
                columns: new[] { "VariationOptionId", "VariationId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariantOptions_VariationOption_VariationOptionId_Var~",
                table: "ProductVariantOptions",
                columns: new[] { "VariationOptionId", "VariationId" },
                principalTable: "VariationOption",
                principalColumns: new[] { "Id", "VariationId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariantOptions_Variation_VariationId",
                table: "ProductVariantOptions",
                column: "VariationId",
                principalTable: "Variation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
