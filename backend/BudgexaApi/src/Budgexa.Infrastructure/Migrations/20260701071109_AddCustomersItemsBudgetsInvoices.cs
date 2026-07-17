using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Budgexa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomersItemsBudgetsInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AddressLine = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Currency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    Currency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TermsAndConditions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgets_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgets_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BudgetLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetLines_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Series = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Currency = table.Column<string>(type: "nchar(3)", fixedLength: true, maxLength: 3, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WithholdingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Statuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Statuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    WithholdingRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WithholdingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLines_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Statuses",
                columns: new[] { "Id", "Group", "Name", "Value" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-111111111111"), "Budget", "Draft", 1 },
                    { new Guid("11111111-2222-4222-8222-222222222222"), "Invoice", "Draft", 1 },
                    { new Guid("22222222-1111-4111-8111-111111111111"), "Budget", "Sent", 2 },
                    { new Guid("22222222-2222-4222-8222-222222222222"), "Invoice", "Issued", 2 },
                    { new Guid("33333333-1111-4111-8111-111111111111"), "Budget", "Approved", 3 },
                    { new Guid("33333333-2222-4222-8222-222222222222"), "Invoice", "PartiallyPaid", 3 },
                    { new Guid("44444444-1111-4111-8111-111111111111"), "Budget", "Rejected", 4 },
                    { new Guid("44444444-2222-4222-8222-222222222222"), "Invoice", "Paid", 4 },
                    { new Guid("55555555-1111-4111-8111-111111111111"), "Budget", "Expired", 5 },
                    { new Guid("55555555-2222-4222-8222-222222222222"), "Invoice", "Overdue", 5 },
                    { new Guid("66666666-1111-4111-8111-111111111111"), "Budget", "Invoiced", 6 },
                    { new Guid("66666666-2222-4222-8222-222222222222"), "Invoice", "Cancelled", 6 }
                });

            migrationBuilder.InsertData(
                table: "StatusTranslations",
                columns: new[] { "Id", "LanguageId", "StatusId", "Translation" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-4111-8111-aaaaaaaaaaa1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("11111111-1111-4111-8111-111111111111"), "Draft" },
                    { new Guid("11111111-1111-4111-8111-aaaaaaaaaaa2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("11111111-1111-4111-8111-111111111111"), "Borrador" },
                    { new Guid("11111111-1111-4111-8111-aaaaaaaaaaa3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("11111111-1111-4111-8111-111111111111"), "Entwurf" },
                    { new Guid("11111111-1111-4111-8111-aaaaaaaaaaa4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("11111111-1111-4111-8111-111111111111"), "Nacrt" },
                    { new Guid("11111111-2222-4222-8222-bbbbbbbbbbb1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("11111111-2222-4222-8222-222222222222"), "Draft" },
                    { new Guid("11111111-2222-4222-8222-bbbbbbbbbbb2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("11111111-2222-4222-8222-222222222222"), "Borrador" },
                    { new Guid("11111111-2222-4222-8222-bbbbbbbbbbb3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("11111111-2222-4222-8222-222222222222"), "Entwurf" },
                    { new Guid("11111111-2222-4222-8222-bbbbbbbbbbb4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("11111111-2222-4222-8222-222222222222"), "Nacrt" },
                    { new Guid("22222222-1111-4111-8111-aaaaaaaaaaa1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("22222222-1111-4111-8111-111111111111"), "Sent" },
                    { new Guid("22222222-1111-4111-8111-aaaaaaaaaaa2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("22222222-1111-4111-8111-111111111111"), "Enviado" },
                    { new Guid("22222222-1111-4111-8111-aaaaaaaaaaa3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("22222222-1111-4111-8111-111111111111"), "Gesendet" },
                    { new Guid("22222222-1111-4111-8111-aaaaaaaaaaa4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("22222222-1111-4111-8111-111111111111"), "Poslano" },
                    { new Guid("22222222-2222-4222-8222-bbbbbbbbbbb1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("22222222-2222-4222-8222-222222222222"), "Issued" },
                    { new Guid("22222222-2222-4222-8222-bbbbbbbbbbb2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("22222222-2222-4222-8222-222222222222"), "Emitida" },
                    { new Guid("22222222-2222-4222-8222-bbbbbbbbbbb3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("22222222-2222-4222-8222-222222222222"), "Ausgestellt" },
                    { new Guid("22222222-2222-4222-8222-bbbbbbbbbbb4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("22222222-2222-4222-8222-222222222222"), "Izdano" },
                    { new Guid("33333333-1111-4111-8111-aaaaaaaaaaa1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("33333333-1111-4111-8111-111111111111"), "Approved" },
                    { new Guid("33333333-1111-4111-8111-aaaaaaaaaaa2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("33333333-1111-4111-8111-111111111111"), "Aprobado" },
                    { new Guid("33333333-1111-4111-8111-aaaaaaaaaaa3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("33333333-1111-4111-8111-111111111111"), "Genehmigt" },
                    { new Guid("33333333-1111-4111-8111-aaaaaaaaaaa4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("33333333-1111-4111-8111-111111111111"), "Odobreno" },
                    { new Guid("33333333-2222-4222-8222-bbbbbbbbbbb1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("33333333-2222-4222-8222-222222222222"), "Partially paid" },
                    { new Guid("33333333-2222-4222-8222-bbbbbbbbbbb2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("33333333-2222-4222-8222-222222222222"), "Parcialmente pagada" },
                    { new Guid("33333333-2222-4222-8222-bbbbbbbbbbb3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("33333333-2222-4222-8222-222222222222"), "Teilweise bezahlt" },
                    { new Guid("33333333-2222-4222-8222-bbbbbbbbbbb4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("33333333-2222-4222-8222-222222222222"), "Djelomično plaćeno" },
                    { new Guid("44444444-1111-4111-8111-aaaaaaaaaaa1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("44444444-1111-4111-8111-111111111111"), "Rejected" },
                    { new Guid("44444444-1111-4111-8111-aaaaaaaaaaa2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("44444444-1111-4111-8111-111111111111"), "Rechazado" },
                    { new Guid("44444444-1111-4111-8111-aaaaaaaaaaa3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("44444444-1111-4111-8111-111111111111"), "Abgelehnt" },
                    { new Guid("44444444-1111-4111-8111-aaaaaaaaaaa4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("44444444-1111-4111-8111-111111111111"), "Odbijeno" },
                    { new Guid("44444444-2222-4222-8222-bbbbbbbbbbb1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("44444444-2222-4222-8222-222222222222"), "Paid" },
                    { new Guid("44444444-2222-4222-8222-bbbbbbbbbbb2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("44444444-2222-4222-8222-222222222222"), "Pagada" },
                    { new Guid("44444444-2222-4222-8222-bbbbbbbbbbb3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("44444444-2222-4222-8222-222222222222"), "Bezahlt" },
                    { new Guid("44444444-2222-4222-8222-bbbbbbbbbbb4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("44444444-2222-4222-8222-222222222222"), "Plaćeno" },
                    { new Guid("55555555-1111-4111-8111-aaaaaaaaaaa1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("55555555-1111-4111-8111-111111111111"), "Expired" },
                    { new Guid("55555555-1111-4111-8111-aaaaaaaaaaa2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("55555555-1111-4111-8111-111111111111"), "Caducado" },
                    { new Guid("55555555-1111-4111-8111-aaaaaaaaaaa3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("55555555-1111-4111-8111-111111111111"), "Abgelaufen" },
                    { new Guid("55555555-1111-4111-8111-aaaaaaaaaaa4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("55555555-1111-4111-8111-111111111111"), "Isteklo" },
                    { new Guid("55555555-2222-4222-8222-bbbbbbbbbbb1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("55555555-2222-4222-8222-222222222222"), "Overdue" },
                    { new Guid("55555555-2222-4222-8222-bbbbbbbbbbb2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("55555555-2222-4222-8222-222222222222"), "Vencida" },
                    { new Guid("55555555-2222-4222-8222-bbbbbbbbbbb3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("55555555-2222-4222-8222-222222222222"), "Überfällig" },
                    { new Guid("55555555-2222-4222-8222-bbbbbbbbbbb4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("55555555-2222-4222-8222-222222222222"), "Dospjelo" },
                    { new Guid("66666666-1111-4111-8111-aaaaaaaaaaa1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("66666666-1111-4111-8111-111111111111"), "Invoiced" },
                    { new Guid("66666666-1111-4111-8111-aaaaaaaaaaa2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("66666666-1111-4111-8111-111111111111"), "Facturado" },
                    { new Guid("66666666-1111-4111-8111-aaaaaaaaaaa3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("66666666-1111-4111-8111-111111111111"), "Fakturiert" },
                    { new Guid("66666666-1111-4111-8111-aaaaaaaaaaa4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("66666666-1111-4111-8111-111111111111"), "Fakturirano" },
                    { new Guid("66666666-2222-4222-8222-bbbbbbbbbbb1"), new Guid("b2c1d3e4-f5a6-4b7c-8d9e-0f1a2b3c4d5e"), new Guid("66666666-2222-4222-8222-222222222222"), "Cancelled" },
                    { new Guid("66666666-2222-4222-8222-bbbbbbbbbbb2"), new Guid("c3d2e1f4-a5b6-4c7d-8e9f-0a1b2c3d4e5f"), new Guid("66666666-2222-4222-8222-222222222222"), "Cancelada" },
                    { new Guid("66666666-2222-4222-8222-bbbbbbbbbbb3"), new Guid("d4e3f2a1-b5c6-4d7e-8f9a-0b1c2d3e4f5a"), new Guid("66666666-2222-4222-8222-222222222222"), "Storniert" },
                    { new Guid("66666666-2222-4222-8222-bbbbbbbbbbb4"), new Guid("e5f4a3b2-c6d7-4e8f-9a0b-1c2d3e4f5a6b"), new Guid("66666666-2222-4222-8222-222222222222"), "Otkazano" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLines_BudgetId",
                table: "BudgetLines",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLines_ItemId",
                table: "BudgetLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CompanyId_Number",
                table: "Budgets",
                columns: new[] { "CompanyId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CustomerId",
                table: "Budgets",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_StatusId",
                table: "Budgets",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CompanyId_TaxId",
                table: "Customers",
                columns: new[] { "CompanyId", "TaxId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_StatusId",
                table: "Customers",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                table: "InvoiceLines",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_ItemId",
                table: "InvoiceLines",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BudgetId",
                table: "Invoices",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CompanyId_Series_Number",
                table: "Invoices",
                columns: new[] { "CompanyId", "Series", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_StatusId",
                table: "Invoices",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CompanyId_Sku",
                table: "Items",
                columns: new[] { "CompanyId", "Sku" },
                unique: true,
                filter: "[Sku] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Items_StatusId",
                table: "Items",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetLines");

            migrationBuilder.DropTable(
                name: "InvoiceLines");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-4111-8111-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-4111-8111-aaaaaaaaaaa2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-4111-8111-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-4111-8111-aaaaaaaaaaa4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-4222-8222-bbbbbbbbbbb1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-4222-8222-bbbbbbbbbbb2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-4222-8222-bbbbbbbbbbb3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-4222-8222-bbbbbbbbbbb4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-1111-4111-8111-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-1111-4111-8111-aaaaaaaaaaa2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-1111-4111-8111-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-1111-4111-8111-aaaaaaaaaaa4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-4222-8222-bbbbbbbbbbb1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-4222-8222-bbbbbbbbbbb2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-4222-8222-bbbbbbbbbbb3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-4222-8222-bbbbbbbbbbb4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-1111-4111-8111-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-1111-4111-8111-aaaaaaaaaaa2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-1111-4111-8111-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-1111-4111-8111-aaaaaaaaaaa4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-2222-4222-8222-bbbbbbbbbbb1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-2222-4222-8222-bbbbbbbbbbb2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-2222-4222-8222-bbbbbbbbbbb3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("33333333-2222-4222-8222-bbbbbbbbbbb4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-1111-4111-8111-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-1111-4111-8111-aaaaaaaaaaa2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-1111-4111-8111-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-1111-4111-8111-aaaaaaaaaaa4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-2222-4222-8222-bbbbbbbbbbb1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-2222-4222-8222-bbbbbbbbbbb2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-2222-4222-8222-bbbbbbbbbbb3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-2222-4222-8222-bbbbbbbbbbb4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-1111-4111-8111-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-1111-4111-8111-aaaaaaaaaaa2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-1111-4111-8111-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-1111-4111-8111-aaaaaaaaaaa4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-2222-4222-8222-bbbbbbbbbbb1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-2222-4222-8222-bbbbbbbbbbb2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-2222-4222-8222-bbbbbbbbbbb3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-2222-4222-8222-bbbbbbbbbbb4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66666666-1111-4111-8111-aaaaaaaaaaa1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66666666-1111-4111-8111-aaaaaaaaaaa2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66666666-1111-4111-8111-aaaaaaaaaaa3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66666666-1111-4111-8111-aaaaaaaaaaa4"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66666666-2222-4222-8222-bbbbbbbbbbb1"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66666666-2222-4222-8222-bbbbbbbbbbb2"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66666666-2222-4222-8222-bbbbbbbbbbb3"));

            migrationBuilder.DeleteData(
                table: "StatusTranslations",
                keyColumn: "Id",
                keyValue: new Guid("66666666-2222-4222-8222-bbbbbbbbbbb4"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-4111-8111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-4222-8222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("22222222-1111-4111-8111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-4222-8222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("33333333-1111-4111-8111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("33333333-2222-4222-8222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("44444444-1111-4111-8111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("44444444-2222-4222-8222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("55555555-1111-4111-8111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("55555555-2222-4222-8222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("66666666-1111-4111-8111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "Id",
                keyValue: new Guid("66666666-2222-4222-8222-222222222222"));
        }
    }
}
