using System.Collections.Generic;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class DrugFormConfiguration : BaseEntityConfiguration<DrugForm>
{
    public override void Configure(EntityTypeBuilder<DrugForm> builder)
    {
        base.Configure(builder);

        builder.ToTable("DrugForms");

        builder.Property(df => df.ProductIdentifier)
            .IsRequired();

        builder.HasIndex(df => df.ProductIdentifier)
            .HasDatabaseName("Index_DrugForms_ProductIdentifier");

        builder.HasIndex(df => df.DrugId)
            .HasDatabaseName("Index_DrugForms_DrugId");

        builder.Property(df => df.AtcCode)
            .HasMaxLength(32);

        builder.HasIndex(df => df.AtcCode)
            .HasDatabaseName("Index_DrugForms_AtcCode");

        builder.HasIndex(df => df.AtcCode, "Index_DrugForms_AtcCode_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.Property(df => df.Dosage)
            .HasMaxLength(500);

        builder.Property(df => df.Leaflet)
            .HasMaxLength(2000);

        builder.Property(df => df.EducationMaterials)
            .HasMaxLength(2000);

        builder.Property(df => df.IsOral)
            .IsRequired();

        builder.HasMany(df => df.RoutesOfAdministration)
            .WithMany(roa => roa.DrugForms)
            .UsingEntity<Dictionary<string, object>>(
                "DrugFormRoutesOfAdministration",
                right => right
                    .HasOne<RouteOfAdministration>()
                    .WithMany()
                    .HasForeignKey("RouteOfAdministrationId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<DrugForm>()
                    .WithMany()
                    .HasForeignKey("DrugFormId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("DrugFormRoutesOfAdministration");
                    join.HasKey("DrugFormId", "RouteOfAdministrationId");
                    join.HasIndex("RouteOfAdministrationId")
                        .HasDatabaseName("Index_DrugFormRoutesOfAdministration_RouteOfAdministrationId");
                });

        builder.HasMany(df => df.PharmaceuticalForms)
            .WithMany(form => form.DrugForms)
            .UsingEntity<Dictionary<string, object>>(
                "DrugFormPharmaceuticalForms",
                right => right
                    .HasOne<PharmaceuticalForm>()
                    .WithMany()
                    .HasForeignKey("PharmaceuticalFormId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<DrugForm>()
                    .WithMany()
                    .HasForeignKey("DrugFormId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("DrugFormPharmaceuticalForms");
                    join.HasKey("DrugFormId", "PharmaceuticalFormId");
                    join.HasIndex("PharmaceuticalFormId")
                        .HasDatabaseName("Index_DrugFormPharmaceuticalForms_PharmaceuticalFormId");
                });
    }
}

