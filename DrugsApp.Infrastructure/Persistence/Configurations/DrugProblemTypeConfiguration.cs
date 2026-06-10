using DrugsApp.Domain.DrugProblemsFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class DrugProblemTypeConfiguration : BaseEntityConfiguration<DrugProblemType>
{
    public override void Configure(EntityTypeBuilder<DrugProblemType> builder)
    {
        base.Configure(builder);

        builder.ToTable("DrugProblemTypes");

        builder.Property(x => x.ProblemTypeName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.ProblemTypeName)
            .HasDatabaseName("UX_DrugProblemTypes_ProblemTypeName_Lower")
            .IsUnique();

        builder.HasIndex(x => new { x.ProblemTypeName, x.Id })
            .HasDatabaseName("IX_DrugProblemTypes_ProblemTypeName_Id");
    }
}
