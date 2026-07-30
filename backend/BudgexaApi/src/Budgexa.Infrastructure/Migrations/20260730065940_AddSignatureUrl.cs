using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Budgexa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignatureUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignatureUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureUrl",
                table: "Users");
        }
    }
}
