using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersModuleConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "ProductVariant",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    CustomerPhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ShippingAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CustomerNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StoreNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    ProductVariantId = table.Column<string>(type: "text", nullable: false),
                    SellingPrice = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.CheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_OrderItems_SellingPrice", "\"SellingPrice\" >= 0");
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ProductVariant");
        }
    }
}
