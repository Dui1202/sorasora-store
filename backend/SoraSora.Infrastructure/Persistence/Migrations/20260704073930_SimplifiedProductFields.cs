using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class simplifiedproductfields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "HasVariation", table: "Product");

        migrationBuilder.DropColumn(name: "IsArchived", table: "Product");

        migrationBuilder.DropColumn(name: "PreorderReleaseAt", table: "Product");

        migrationBuilder.DropColumn(name: "SellMode", table: "Product");

        migrationBuilder.DropColumn(name: "SupplierCountry", table: "Product");

        migrationBuilder.AlterColumn<bool>(
            name: "IsPublished",
            table: "Product",
            type: "boolean",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "boolean",
            oldDefaultValue: false
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<bool>(
            name: "IsPublished",
            table: "Product",
            type: "boolean",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "boolean"
        );

        migrationBuilder.AddColumn<bool>(
            name: "HasVariation",
            table: "Product",
            type: "boolean",
            nullable: false,
            defaultValue: false
        );

        migrationBuilder.AddColumn<bool>(
            name: "IsArchived",
            table: "Product",
            type: "boolean",
            nullable: false,
            defaultValue: false
        );

        migrationBuilder.AddColumn<DateTime>(
            name: "PreorderReleaseAt",
            table: "Product",
            type: "timestamp with time zone",
            nullable: true
        );

        migrationBuilder.AddColumn<string>(
            name: "SellMode",
            table: "Product",
            type: "text",
            nullable: false,
            defaultValue: "IN_STOCK"
        );

        migrationBuilder.AddColumn<string>(
            name: "SupplierCountry",
            table: "Product",
            type: "text",
            nullable: false,
            defaultValue: "NONE"
        );
    }
}
