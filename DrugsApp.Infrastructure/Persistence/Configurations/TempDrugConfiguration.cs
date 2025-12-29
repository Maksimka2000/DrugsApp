using DrugsApp.Domain.TempDrugFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class TempDrugConfiguration : BaseEntityConfiguration<TempDrug>
{
    public override void Configure(EntityTypeBuilder<TempDrug> builder)
    {
        base.Configure(builder);

        builder.ToTable(
            "TempDrugs",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_TempDrugs_LinkedDrugIds_Xor",
                    "\"LinkedDrugFormId\" IS NULL OR \"LinkedDrugVetId\" IS NULL");
            });

        builder.Property(t => t.LinkedDrugFormId)
            .IsRequired(false);
        
        builder.HasIndex(t => t.LinkedDrugFormId, "Index_TempDrugs_LinkedDrugFormId");

        builder.Property(t => t.LinkedDrugVetId)
            .IsRequired(false);

        builder.HasIndex(t => t.LinkedDrugVetId, "Index_TempDrugs_LinkedDrugVetId");

        builder.Property(t => t.ProductIdentifier)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.ProductIdentifier, "Index_TempDrugs_ProductIdentifier")
            .IsUnique();

        builder.Property(t => t.CommercialName)
            .HasMaxLength(300);

        builder.HasIndex(t => t.CommercialName, "Index_TempDrugs_CommercialName_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.Property(t => t.ActiveSubstance)
            .HasMaxLength(500);

        builder.Property(t => t.TypeOfProduct)
            .HasMaxLength(100);

        builder.HasIndex(t => t.TypeOfProduct, "Index_TempDrugs_TypeOfProduct");

        builder.Property(t => t.RouteOfAdministration)
            .HasMaxLength(300);

        builder.Property(t => t.PharmaceuticalFormulation)
            .HasMaxLength(300);

        builder.Property(t => t.ATCCode)
            .HasMaxLength(64);

        builder.Property(t => t.Leaflet)
            .HasMaxLength(2000);

        builder.Property(t => t.EducationalMaterials)
            .HasMaxLength(2000);

        builder.Property(t => t.Dosage)
            .HasMaxLength(500);

        builder.Property(t => t.LinkedImportId)
            .IsRequired();

        builder.HasIndex(t => t.LinkedImportId, "Index_TempDrugs_LinkedImportId");

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64)
            .HasDefaultValue(TempDrugStatus.PendingReview);

        builder.HasIndex(t => t.Status, "Index_TempDrugs_Status");
    }
}


