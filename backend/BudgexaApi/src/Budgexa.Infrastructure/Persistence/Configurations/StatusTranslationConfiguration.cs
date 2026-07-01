namespace Budgexa.Infrastructure.Persistence.Configurations;

using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class StatusTranslationConfiguration : IEntityTypeConfiguration<StatusTranslation>
{
    public void Configure(EntityTypeBuilder<StatusTranslation> builder)
    {
        builder.HasKey(st => st.Id);

        builder.Property(st => st.Translation)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(st => st.Status)
            .WithMany(s => s.Translations)
            .HasForeignKey(st => st.StatusId);

        builder.HasOne(st => st.Language)
            .WithMany(l => l.StatusTranslations)
            .HasForeignKey(st => st.LanguageId);

        builder.ToTable("StatusTranslations");

        builder.HasData(
            // Status "New"
            StatusTranslation.Create(StatusIds.New, LanguageIds.English, "New", Guid.Parse("b1e2c3d4-5f6a-4b7c-8d9e-0f1a2b3c4d5e")),
            StatusTranslation.Create(StatusIds.New, LanguageIds.Spanish, "Nuevo", Guid.Parse("c2d3e4f5-a6b7-4c8d-9e0f-1a2b3c4d5e6f")),
            StatusTranslation.Create(StatusIds.New, LanguageIds.German, "Neu", Guid.Parse("d3e4f5a6-b7c8-4d9e-0f1a-2b3c4d5e6f7a")),
            StatusTranslation.Create(StatusIds.New, LanguageIds.Croatian, "Novi", Guid.Parse("e4f5a6b7-c8d9-4e0f-1a2b-3c4d5e6f7a8b")),
            // Status "Delete"
            StatusTranslation.Create(StatusIds.Delete, LanguageIds.English, "Delete", Guid.Parse("f5a6b7c8-d9e0-4f1a-2b3c-4d5e6f7a8b9c")),
            StatusTranslation.Create(StatusIds.Delete, LanguageIds.Spanish, "Eliminar", Guid.Parse("a6b7c8d9-e0f1-4a2b-3c4d-5e6f7a8b9c0d")),
            StatusTranslation.Create(StatusIds.Delete, LanguageIds.German, "Löschen", Guid.Parse("b7c8d9e0-f1a2-4b3c-4d5e-6f7a8b9c0d1e")),
            StatusTranslation.Create(StatusIds.Delete, LanguageIds.Croatian, "Izbriši", Guid.Parse("c8d9e0f1-a2b3-4c4d-5e6f-7a8b9c0d1e2f")),

            // Budget "Draft"
            StatusTranslation.Create(StatusIds.Budget.Draft, LanguageIds.English, "Draft", Guid.Parse("11111111-1111-4111-8111-aaaaaaaaaaa1")),
            StatusTranslation.Create(StatusIds.Budget.Draft, LanguageIds.Spanish, "Borrador", Guid.Parse("11111111-1111-4111-8111-aaaaaaaaaaa2")),
            StatusTranslation.Create(StatusIds.Budget.Draft, LanguageIds.German, "Entwurf", Guid.Parse("11111111-1111-4111-8111-aaaaaaaaaaa3")),
            StatusTranslation.Create(StatusIds.Budget.Draft, LanguageIds.Croatian, "Nacrt", Guid.Parse("11111111-1111-4111-8111-aaaaaaaaaaa4")),
            // Budget "Sent"
            StatusTranslation.Create(StatusIds.Budget.Sent, LanguageIds.English, "Sent", Guid.Parse("22222222-1111-4111-8111-aaaaaaaaaaa1")),
            StatusTranslation.Create(StatusIds.Budget.Sent, LanguageIds.Spanish, "Enviado", Guid.Parse("22222222-1111-4111-8111-aaaaaaaaaaa2")),
            StatusTranslation.Create(StatusIds.Budget.Sent, LanguageIds.German, "Gesendet", Guid.Parse("22222222-1111-4111-8111-aaaaaaaaaaa3")),
            StatusTranslation.Create(StatusIds.Budget.Sent, LanguageIds.Croatian, "Poslano", Guid.Parse("22222222-1111-4111-8111-aaaaaaaaaaa4")),
            // Budget "Approved"
            StatusTranslation.Create(StatusIds.Budget.Approved, LanguageIds.English, "Approved", Guid.Parse("33333333-1111-4111-8111-aaaaaaaaaaa1")),
            StatusTranslation.Create(StatusIds.Budget.Approved, LanguageIds.Spanish, "Aprobado", Guid.Parse("33333333-1111-4111-8111-aaaaaaaaaaa2")),
            StatusTranslation.Create(StatusIds.Budget.Approved, LanguageIds.German, "Genehmigt", Guid.Parse("33333333-1111-4111-8111-aaaaaaaaaaa3")),
            StatusTranslation.Create(StatusIds.Budget.Approved, LanguageIds.Croatian, "Odobreno", Guid.Parse("33333333-1111-4111-8111-aaaaaaaaaaa4")),
            // Budget "Rejected"
            StatusTranslation.Create(StatusIds.Budget.Rejected, LanguageIds.English, "Rejected", Guid.Parse("44444444-1111-4111-8111-aaaaaaaaaaa1")),
            StatusTranslation.Create(StatusIds.Budget.Rejected, LanguageIds.Spanish, "Rechazado", Guid.Parse("44444444-1111-4111-8111-aaaaaaaaaaa2")),
            StatusTranslation.Create(StatusIds.Budget.Rejected, LanguageIds.German, "Abgelehnt", Guid.Parse("44444444-1111-4111-8111-aaaaaaaaaaa3")),
            StatusTranslation.Create(StatusIds.Budget.Rejected, LanguageIds.Croatian, "Odbijeno", Guid.Parse("44444444-1111-4111-8111-aaaaaaaaaaa4")),
            // Budget "Expired"
            StatusTranslation.Create(StatusIds.Budget.Expired, LanguageIds.English, "Expired", Guid.Parse("55555555-1111-4111-8111-aaaaaaaaaaa1")),
            StatusTranslation.Create(StatusIds.Budget.Expired, LanguageIds.Spanish, "Caducado", Guid.Parse("55555555-1111-4111-8111-aaaaaaaaaaa2")),
            StatusTranslation.Create(StatusIds.Budget.Expired, LanguageIds.German, "Abgelaufen", Guid.Parse("55555555-1111-4111-8111-aaaaaaaaaaa3")),
            StatusTranslation.Create(StatusIds.Budget.Expired, LanguageIds.Croatian, "Isteklo", Guid.Parse("55555555-1111-4111-8111-aaaaaaaaaaa4")),
            // Budget "Invoiced"
            StatusTranslation.Create(StatusIds.Budget.Invoiced, LanguageIds.English, "Invoiced", Guid.Parse("66666666-1111-4111-8111-aaaaaaaaaaa1")),
            StatusTranslation.Create(StatusIds.Budget.Invoiced, LanguageIds.Spanish, "Facturado", Guid.Parse("66666666-1111-4111-8111-aaaaaaaaaaa2")),
            StatusTranslation.Create(StatusIds.Budget.Invoiced, LanguageIds.German, "Fakturiert", Guid.Parse("66666666-1111-4111-8111-aaaaaaaaaaa3")),
            StatusTranslation.Create(StatusIds.Budget.Invoiced, LanguageIds.Croatian, "Fakturirano", Guid.Parse("66666666-1111-4111-8111-aaaaaaaaaaa4")),

            // Invoice "Draft"
            StatusTranslation.Create(StatusIds.Invoice.Draft, LanguageIds.English, "Draft", Guid.Parse("11111111-2222-4222-8222-bbbbbbbbbbb1")),
            StatusTranslation.Create(StatusIds.Invoice.Draft, LanguageIds.Spanish, "Borrador", Guid.Parse("11111111-2222-4222-8222-bbbbbbbbbbb2")),
            StatusTranslation.Create(StatusIds.Invoice.Draft, LanguageIds.German, "Entwurf", Guid.Parse("11111111-2222-4222-8222-bbbbbbbbbbb3")),
            StatusTranslation.Create(StatusIds.Invoice.Draft, LanguageIds.Croatian, "Nacrt", Guid.Parse("11111111-2222-4222-8222-bbbbbbbbbbb4")),
            // Invoice "Issued"
            StatusTranslation.Create(StatusIds.Invoice.Issued, LanguageIds.English, "Issued", Guid.Parse("22222222-2222-4222-8222-bbbbbbbbbbb1")),
            StatusTranslation.Create(StatusIds.Invoice.Issued, LanguageIds.Spanish, "Emitida", Guid.Parse("22222222-2222-4222-8222-bbbbbbbbbbb2")),
            StatusTranslation.Create(StatusIds.Invoice.Issued, LanguageIds.German, "Ausgestellt", Guid.Parse("22222222-2222-4222-8222-bbbbbbbbbbb3")),
            StatusTranslation.Create(StatusIds.Invoice.Issued, LanguageIds.Croatian, "Izdano", Guid.Parse("22222222-2222-4222-8222-bbbbbbbbbbb4")),
            // Invoice "PartiallyPaid"
            StatusTranslation.Create(StatusIds.Invoice.PartiallyPaid, LanguageIds.English, "Partially paid", Guid.Parse("33333333-2222-4222-8222-bbbbbbbbbbb1")),
            StatusTranslation.Create(StatusIds.Invoice.PartiallyPaid, LanguageIds.Spanish, "Parcialmente pagada", Guid.Parse("33333333-2222-4222-8222-bbbbbbbbbbb2")),
            StatusTranslation.Create(StatusIds.Invoice.PartiallyPaid, LanguageIds.German, "Teilweise bezahlt", Guid.Parse("33333333-2222-4222-8222-bbbbbbbbbbb3")),
            StatusTranslation.Create(StatusIds.Invoice.PartiallyPaid, LanguageIds.Croatian, "Djelomično plaćeno", Guid.Parse("33333333-2222-4222-8222-bbbbbbbbbbb4")),
            // Invoice "Paid"
            StatusTranslation.Create(StatusIds.Invoice.Paid, LanguageIds.English, "Paid", Guid.Parse("44444444-2222-4222-8222-bbbbbbbbbbb1")),
            StatusTranslation.Create(StatusIds.Invoice.Paid, LanguageIds.Spanish, "Pagada", Guid.Parse("44444444-2222-4222-8222-bbbbbbbbbbb2")),
            StatusTranslation.Create(StatusIds.Invoice.Paid, LanguageIds.German, "Bezahlt", Guid.Parse("44444444-2222-4222-8222-bbbbbbbbbbb3")),
            StatusTranslation.Create(StatusIds.Invoice.Paid, LanguageIds.Croatian, "Plaćeno", Guid.Parse("44444444-2222-4222-8222-bbbbbbbbbbb4")),
            // Invoice "Overdue"
            StatusTranslation.Create(StatusIds.Invoice.Overdue, LanguageIds.English, "Overdue", Guid.Parse("55555555-2222-4222-8222-bbbbbbbbbbb1")),
            StatusTranslation.Create(StatusIds.Invoice.Overdue, LanguageIds.Spanish, "Vencida", Guid.Parse("55555555-2222-4222-8222-bbbbbbbbbbb2")),
            StatusTranslation.Create(StatusIds.Invoice.Overdue, LanguageIds.German, "Überfällig", Guid.Parse("55555555-2222-4222-8222-bbbbbbbbbbb3")),
            StatusTranslation.Create(StatusIds.Invoice.Overdue, LanguageIds.Croatian, "Dospjelo", Guid.Parse("55555555-2222-4222-8222-bbbbbbbbbbb4")),
            // Invoice "Cancelled"
            StatusTranslation.Create(StatusIds.Invoice.Cancelled, LanguageIds.English, "Cancelled", Guid.Parse("66666666-2222-4222-8222-bbbbbbbbbbb1")),
            StatusTranslation.Create(StatusIds.Invoice.Cancelled, LanguageIds.Spanish, "Cancelada", Guid.Parse("66666666-2222-4222-8222-bbbbbbbbbbb2")),
            StatusTranslation.Create(StatusIds.Invoice.Cancelled, LanguageIds.German, "Storniert", Guid.Parse("66666666-2222-4222-8222-bbbbbbbbbbb3")),
            StatusTranslation.Create(StatusIds.Invoice.Cancelled, LanguageIds.Croatian, "Otkazano", Guid.Parse("66666666-2222-4222-8222-bbbbbbbbbbb4"))
        );
    }
}