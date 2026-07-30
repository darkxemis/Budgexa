using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Budgexa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitMeasureItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "Items",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)")
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxRate",
                table: "Items",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)")
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Items",
                type: "nchar(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(3)",
                oldFixedLength: true,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AddColumn<int>(
                name: "UnitMeasure",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 12);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitMeasure",
                table: "Items");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "Items",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)")
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxRate",
                table: "Items",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)")
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Items",
                type: "nchar(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nchar(3)",
                oldFixedLength: true,
                oldMaxLength: 3)
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 15);
        }
    }
}
