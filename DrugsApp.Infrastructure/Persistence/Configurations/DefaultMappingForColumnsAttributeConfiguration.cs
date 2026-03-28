using DrugsApp.Domain.TempDrugFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class DefaultMappingForColumnsAttributeConfiguration : BaseEntityConfiguration<DefaultMappingForColumnsAttribute>
{
    private static readonly DateTime SeedCreatedAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public override void Configure(EntityTypeBuilder<DefaultMappingForColumnsAttribute> builder)
    {
        base.Configure(builder);

        builder.ToTable("DefaultMappingForColumnsAttributes");

        builder.Property(x => x.SourceColumnName)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.MappedFieldName)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.SourceColumnName)
            .IsUnique();

        builder.HasData(
            new
            {
                Id = Guid.Parse("11F856D0-CA3A-4B95-9C07-9C8EE10473A1"),
                SourceColumnName = "Identyfikator Produktu Leczniczego",
                MappedFieldName = "ProductIdentifier",
                CreatedAtUtc = SeedCreatedAtUtc
            },
            new
            {
                Id = Guid.Parse("A26AB581-877B-490A-93F8-3E9234E36A13"),
                SourceColumnName = "Nazwa Produktu Leczniczego",
                MappedFieldName = "CommercialName",
                CreatedAtUtc = SeedCreatedAtUtc
            },
            new
            {
                Id = Guid.Parse("15C2FEA5-0F41-40AF-9179-F6DD7B06A04C"),
                SourceColumnName = "Nazwa powszechnie stosowana",
                MappedFieldName = "ActiveSubstance",
                CreatedAtUtc = SeedCreatedAtUtc
            },
            new
            {
                Id = Guid.Parse("05D450E8-2C20-4606-913A-5C44A8CA7DC4"),
                SourceColumnName = "Rodzaj preparatu",
                MappedFieldName = "TypeOfProduct",
                CreatedAtUtc = SeedCreatedAtUtc
            },
            new
            {
                Id = Guid.Parse("35AECB0C-29EA-43EA-9A53-6608A7991AC4"),
                SourceColumnName = "Droga podania - Gatunek - Tkanka - Okres karencji",
                MappedFieldName = "RouteOfAdministration",
                CreatedAtUtc = SeedCreatedAtUtc
            },
            new
            {
                Id = Guid.Parse("8CFD62CC-EBB3-4409-933D-BA227A598E4A"),
                SourceColumnName = "Postać farmaceutyczna",
                MappedFieldName = "PharmaceuticalFormulation",
                CreatedAtUtc = SeedCreatedAtUtc
            },
            new
            {
                Id = Guid.Parse("2D03F41A-0A99-49DA-BBC6-9C56A9226156"),
                SourceColumnName = "Kod ATC",
                MappedFieldName = "ATCCode",
                CreatedAtUtc = SeedCreatedAtUtc
            },
            new
            {
                Id = Guid.Parse("7290F113-D8CB-4D3A-B6AB-EF866FA69A17"),
                SourceColumnName = "Ulotka",
                MappedFieldName = "Leaflet",
                CreatedAtUtc = SeedCreatedAtUtc
            },
            new
            {
                Id = Guid.Parse("00DBB8B7-30D5-421F-96A2-2D9920E786CE"),
                SourceColumnName = "Narzędzia edukacyjne dla osoby wykonującej zawód medyczny",
                MappedFieldName = "EducationalMaterials",
                CreatedAtUtc = SeedCreatedAtUtc
            },
            new
            {
                Id = Guid.Parse("A8437908-8F3A-48D0-B4E8-4D5F35EF90B5"),
                SourceColumnName = "Moc",
                MappedFieldName = "Dosage",
                CreatedAtUtc = SeedCreatedAtUtc
            });
    }
}

