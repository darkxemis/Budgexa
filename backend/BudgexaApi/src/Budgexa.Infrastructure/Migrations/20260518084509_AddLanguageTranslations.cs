using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Budgexa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LanguageTranslations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TranslationLanguageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Translation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LanguageTranslations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LanguageTranslations_Languages_TranslationLanguageId",
                        column: x => x.TranslationLanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "LanguageTranslations",
                columns: new[] { "Id", "LanguageId", "Translation", "TranslationLanguageId" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), "English", new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e") },
                    { new Guid("a3b4c5d6-e7f8-4a9b-0c1d-2e3f4a5b6c7d"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), "Croatian", new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e") },
                    { new Guid("a7b8c9d0-e1f2-4a3b-4c5d-6e7f8a9b0c1d"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), "Spanisch", new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a") },
                    { new Guid("b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), "Inglés", new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f") },
                    { new Guid("b4c5d6e7-f8a9-4b0c-1d2e-3f4a5b6c7d8e"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), "Croata", new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f") },
                    { new Guid("b8c9d0e1-f2a3-4b4c-5d6e-7f8a9b0c1d2e"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), "Španjolski", new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b") },
                    { new Guid("c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), "Englisch", new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a") },
                    { new Guid("c5d6e7f8-a9b0-4c1d-2e3f-4a5b6c7d8e9f"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), "Kroatisch", new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a") },
                    { new Guid("c9d0e1f2-a3b4-4c5d-6e7f-8a9b0c1d2e3f"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), "German", new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e") },
                    { new Guid("d0e1f2a3-b4c5-4d6e-7f8a-9b0c1d2e3f4a"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), "Alemán", new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f") },
                    { new Guid("d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f8a"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), "Engleski", new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b") },
                    { new Guid("d6e7f8a9-b0c1-4d2e-3f4a-5b6c7d8e9f0a"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), "Hrvatski", new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b") },
                    { new Guid("e1f2a3b4-c5d6-4e7f-8a9b-0c1d2e3f4a5b"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), "Deutsch", new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a") },
                    { new Guid("e5f6a7b8-c9d0-4e1f-2a3b-4c5d6e7f8a9b"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), "Spanish", new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e") },
                    { new Guid("f2a3b4c5-d6e7-4f8a-9b0c-1d2e3f4a5b6c"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), "Njemački", new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b") },
                    { new Guid("f6a7b8c9-d0e1-4f2a-3b4c-5d6e7f8a9b0c"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), "Español", new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LanguageTranslations_LanguageId",
                table: "LanguageTranslations",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageTranslations_TranslationLanguageId",
                table: "LanguageTranslations",
                column: "TranslationLanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LanguageTranslations");
        }
    }
}
