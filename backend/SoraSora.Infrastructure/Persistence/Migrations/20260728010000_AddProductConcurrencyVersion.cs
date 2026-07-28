using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoraSora.Infrastructure.Persistence.Migrations;

[DbContext(typeof(SoraSoraDbContext))]
[Migration("20260728010000_AddProductConcurrencyVersion")]
public partial class AddProductConcurrencyVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "Version",
            table: "Product",
            type: "uuid",
            nullable: true
        );

        migrationBuilder.Sql(
            """
            UPDATE "Product"
            SET "Version" = gen_random_uuid()
            WHERE "Version" IS NULL;
            """
        );

        migrationBuilder.AlterColumn<Guid>(
            name: "Version",
            table: "Product",
            type: "uuid",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uuid",
            oldNullable: true
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Version", table: "Product");
    }
}
