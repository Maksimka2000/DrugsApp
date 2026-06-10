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

        builder.Property(x => x.DrugId)
            .IsRequired(false);

        builder.Property(x => x.CustomDrugName)
            .HasMaxLength(300);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ProblemNote)
            .HasMaxLength(2000)
            .IsRequired();

        builder
            .HasOne(x => x.Drug)
            .WithMany()
            .HasForeignKey(x => x.DrugId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_DrugProblems_UserId");

        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc, x.Id })
            .HasDatabaseName("IX_DrugProblems_UserId_CreatedAtUtc_Id");

        builder.HasIndex(x => new { x.UserId, x.DrugId })
            .HasDatabaseName("UX_DrugProblems_UserId_DrugId")
            .IsUnique()
            .HasFilter("\"DrugId\" IS NOT NULL");

        builder.HasIndex(x => new { x.UserId, x.CustomDrugName })
            .HasDatabaseName("UX_DrugProblems_UserId_CustomDrugName")
            .IsUnique()
            .HasFilter("\"CustomDrugName\" IS NOT NULL");

        builder.HasCheckConstraint(
            "CK_DrugProblems_SourceXor",
            "(CASE WHEN \"DrugId\" IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN \"CustomDrugName\" IS NOT NULL THEN 1 ELSE 0 END) = 1");
    }
}
