using DrugsApp.Domain.VetDrugFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class VetDrugConfiguration : BaseEntityConfiguration<VetDrug>
{
    public override void Configure(EntityTypeBuilder<VetDrug> builder)
    {
        base.Configure(builder);

        builder.ToTable("VetDrugs");

        builder.Property(vd => vd.ProductIdentifier)
            .IsRequired();

        builder.Property(vd => vd.CommercialName)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(vd => vd.CommercialName);

        builder.Property(vd => vd.TypeOfProduct)
            .IsRequired()
            .HasMaxLength(100);
    }
}

