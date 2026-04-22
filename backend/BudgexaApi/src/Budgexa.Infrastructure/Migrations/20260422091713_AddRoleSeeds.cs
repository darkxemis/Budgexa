using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Budgexa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("48196131-9077-4b1e-8072-a3cc04d88acd"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("918c95ae-347b-484d-9280-71c4b9d8f88f"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b75fe8e1-a33b-4ae5-abf8-37ef7919bab8"));

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("6b2e4c9a-1f83-4d7b-b2a5-9e0c3f6a1d74"), "superadministrator" },
                    { new Guid("9f3c2a6e-8b71-4b8d-9c2a-5f6e3d1a7c90"), "freelance" },
                    { new Guid("d4a1f9b2-3c7e-4e5a-8a91-2b6c0f7d8e13"), "administrator" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6b2e4c9a-1f83-4d7b-b2a5-9e0c3f6a1d74"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("9f3c2a6e-8b71-4b8d-9c2a-5f6e3d1a7c90"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d4a1f9b2-3c7e-4e5a-8a91-2b6c0f7d8e13"));

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("48196131-9077-4b1e-8072-a3cc04d88acd"), "administrator" },
                    { new Guid("918c95ae-347b-484d-9280-71c4b9d8f88f"), "superadministrator" },
                    { new Guid("b75fe8e1-a33b-4ae5-abf8-37ef7919bab8"), "freelance" }
                });
        }
    }
}
