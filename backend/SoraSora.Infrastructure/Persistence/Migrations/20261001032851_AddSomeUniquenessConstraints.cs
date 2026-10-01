using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSomeUniquenessConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VariationOption_VariationId_Position",
                table: "VariationOption");

            migrationBuilder.DropIndex(
                name: "IX_Variation_ProductId_Position",
                table: "Variation");

            migrationBuilder.CreateIndex(
                name: "IX_VariationOption_VariationId_Position",
                table: "VariationOption",
                columns: new[] { "VariationId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variation_ProductId_Position",
                table: "Variation",
                columns: new[] { "ProductId", "Position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VariationOption_VariationId_Position",
                table: "VariationOption");

            migrationBuilder.DropIndex(
                name: "IX_Variation_ProductId_Position",
                table: "Variation");

            migrationBuilder.CreateIndex(
                name: "IX_VariationOption_VariationId_Position",
                table: "VariationOption",
                columns: new[] { "VariationId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_Variation_ProductId_Position",
                table: "Variation",
                columns: new[] { "ProductId", "Position" });
        }
    }
}
