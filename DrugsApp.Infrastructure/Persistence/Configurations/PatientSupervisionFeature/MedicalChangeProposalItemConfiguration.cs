using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations.PatientSupervisionFeature;

public sealed class MedicalChangeProposalItemConfiguration : BaseEntityConfiguration<MedicalChangeProposalItem>
{
    public override void Configure(EntityTypeBuilder<MedicalChangeProposalItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("medical_change_proposal_items");

        builder.Property(x => x.ProposalId)
            .HasColumnName("proposal_id")
            .IsRequired();

        builder.Property(x => x.CategoryCode)
            .HasColumnName("category_code")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.RelatedCatalogId)
            .HasColumnName("related_catalog_id");

        builder.Property(x => x.CustomValue)
            .HasColumnName("custom_value");

        builder.Property(x => x.DisplayNameSnapshot)
            .HasColumnName("display_name_snapshot")
            .IsRequired();

        builder.Property(x => x.SecondaryTextSnapshot)
            .HasColumnName("secondary_text_snapshot");

        builder.Property(x => x.StartDate)
            .HasColumnName("start_date");

        builder.Property(x => x.EndDate)
            .HasColumnName("end_date");

        builder.Property(x => x.Note)
            .HasColumnName("note");

        builder.Property(x => x.ApplyPayload)
            .HasColumnName("apply_payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(x => new { x.ProposalId, x.SortOrder })
            .HasDatabaseName("ux_medical_change_proposal_items_proposal_sort")
            .IsUnique();

        builder.HasMany(x => x.IntakeTimes)
            .WithOne()
            .HasForeignKey(x => x.ProposalItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
