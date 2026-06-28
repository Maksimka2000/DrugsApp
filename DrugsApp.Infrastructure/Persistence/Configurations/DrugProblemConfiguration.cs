using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class DrugProblemConfiguration : BaseEntityConfiguration<DrugProblem>
{
    public override void Configure(EntityTypeBuilder<DrugProblem> builder)
    {
        base.Configure(builder);

        builder.ToTable("DrugProblems");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.DrugFormId)
            .IsRequired(false);

        builder.Property(x => x.CustomDrugName)
            .HasMaxLength(300);

        builder.Property(x => x.CustomActiveSubstances)
            .HasMaxLength(300);

        builder.Property(x => x.CustomFormulationName)
            .HasMaxLength(300);

        builder.Property(x => x.CustomDosage)
            .HasMaxLength(200);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ProblemNote)
            .HasMaxLength(2000)
            .IsRequired();

        builder
            .HasOne(x => x.DrugForm)
            .WithMany()
            .HasForeignKey(x => x.DrugFormId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_DrugProblems_UserId");

        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc, x.Id })
            .HasDatabaseName("IX_DrugProblems_UserId_CreatedAtUtc_Id");

        builder.HasIndex(x => new { x.UserId, x.DrugFormId })
            .HasDatabaseName("UX_DrugProblems_UserId_DrugFormId")
            .IsUnique()
            .HasFilter("\"DrugFormId\" IS NOT NULL");

        builder.HasIndex(x => new { x.UserId, x.CustomDrugName })
            .HasDatabaseName("UX_DrugProblems_UserId_CustomDrugName")
            .IsUnique()
            .HasFilter("\"CustomDrugName\" IS NOT NULL");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_DrugProblems_SourceXor",
            "(CASE WHEN \"DrugFormId\" IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN \"CustomDrugName\" IS NOT NULL THEN 1 ELSE 0 END) = 1"));
    }
}
