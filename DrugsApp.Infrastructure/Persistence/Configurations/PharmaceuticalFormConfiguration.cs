using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class PharmaceuticalFormConfiguration : BaseEntityConfiguration<PharmaceuticalForm>
{
    public override void Configure(EntityTypeBuilder<PharmaceuticalForm> builder)
    {
        base.Configure(builder);

        builder.ToTable("PharmaceuticalForms");

        builder.Property(pf => pf.FormulationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(pf => pf.FormulationName)
            .IsUnique();
    }
}


