using System.Collections.Generic;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class DrugConfiguration : BaseEntityConfiguration<Drug>
{
    public override void Configure(EntityTypeBuilder<Drug> builder)
    {
        base.Configure(builder);

        builder.ToTable("Drugs");

        builder.Property(d => d.CommercialName)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(d => d.CommercialName, "Index_Drugs_CommercialName");

        builder.HasIndex(d => new { d.CommercialName, d.Id }, "Index_Drugs_CommercialName_Id");

        builder.HasIndex(d => d.CommercialName, "Index_Drugs_CommercialName_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.Property(d => d.TypeOfProduct)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.TypeOfDrug)
            .IsRequired()
            .HasMaxLength(64)
            .HasConversion<string>();

        builder.Property(d => d.DrugFormCounter)
            .IsRequired();

        builder.HasMany(d => d.DrugForms)
            .WithOne(df => df.Drug)
            .HasForeignKey(df => df.DrugId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.ActiveSubstances)
            .WithMany(a => a.Drugs)
            .UsingEntity<Dictionary<string, object>>(
                "DrugActiveSubstances",
                right => right
                    .HasOne<ActiveSubstance>()
                    .WithMany()
                    .HasForeignKey("ActiveSubstanceId")
                    .OnDelete(DeleteBehavior.Cascade),
                left => left
                    .HasOne<Drug>()
                    .WithMany()
                    .HasForeignKey("DrugId")
                    .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("DrugActiveSubstances");
                    join.HasKey("DrugId", "ActiveSubstanceId");
                    join.HasIndex("ActiveSubstanceId")
                        .HasDatabaseName("Index_DrugActiveSubstances_ActiveSubstanceId");
                });
    }
}

