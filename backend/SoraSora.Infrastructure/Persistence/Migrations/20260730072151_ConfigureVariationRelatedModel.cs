using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureVariationRelatedModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VariationOption_VariationId",
                table: "VariationOption");

            migrationBuilder.DropIndex(
                name: "IX_Variation_ProductId",
                table: "Variation");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_VariationOption_Id_VariationId",
                table: "VariationOption",
                columns: new[] { "Id", "VariationId" });

            migrationBuilder.CreateTable(
                name: "ProductVariantOptions",
                columns: table => new
                {
                    ProductVariantId = table.Column<string>(type: "text", nullable: false),
                    VariationId = table.Column<string>(type: "text", nullable: false),
                    VariationOptionId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariantOptions", x => new { x.ProductVariantId, x.VariationId });
                    table.ForeignKey(
                        name: "FK_ProductVariantOptions_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductVariantOptions_VariationOption_VariationOptionId_Var~",
                        columns: x => new { x.VariationOptionId, x.VariationId },
                        principalTable: "VariationOption",
                        principalColumns: new[] { "Id", "VariationId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductVariantOptions_Variation_VariationId",
                        column: x => x.VariationId,
                        principalTable: "Variation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VariationOption_VariationId_Position",
                table: "VariationOption",
                columns: new[] { "VariationId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_VariationOption_VariationId_Value",
                table: "VariationOption",
                columns: new[] { "VariationId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variation_ProductId_Name",
                table: "Variation",
                columns: new[] { "ProductId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variation_ProductId_Position",
                table: "Variation",
                columns: new[] { "ProductId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_ProductVariantId_VariationOptionId",
                table: "ProductVariantOptions",
                columns: new[] { "ProductVariantId", "VariationOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_VariationId",
                table: "ProductVariantOptions",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_VariationOptionId",
                table: "ProductVariantOptions",
                column: "VariationOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantOptions_VariationOptionId_VariationId",
                table: "ProductVariantOptions",
                columns: new[] { "VariationOptionId", "VariationId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductVariantOptions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_VariationOption_Id_VariationId",
                table: "VariationOption");

            migrationBuilder.DropIndex(
                name: "IX_VariationOption_VariationId_Position",
                table: "VariationOption");

            migrationBuilder.DropIndex(
                name: "IX_VariationOption_VariationId_Value",
                table: "VariationOption");

            migrationBuilder.DropIndex(
                name: "IX_Variation_ProductId_Name",
                table: "Variation");

            migrationBuilder.DropIndex(
                name: "IX_Variation_ProductId_Position",
                table: "Variation");

            migrationBuilder.CreateIndex(
                name: "IX_VariationOption_VariationId",
                table: "VariationOption",
                column: "VariationId");

            migrationBuilder.CreateIndex(
                name: "IX_Variation_ProductId",
                table: "Variation",
                column: "ProductId");
        }
    }
}
