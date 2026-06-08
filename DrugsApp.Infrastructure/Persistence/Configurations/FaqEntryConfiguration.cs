using DrugsApp.Domain.SupportFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class FaqEntryConfiguration : BaseEntityConfiguration<FaqEntry>
{
    public override void Configure(EntityTypeBuilder<FaqEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("FaqEntries");

        builder.Property(faqEntry => faqEntry.Question)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(faqEntry => faqEntry.NormalizedQuestion)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(faqEntry => faqEntry.Answer)
            .IsRequired()
            .HasColumnType("text");

        builder.HasIndex(faqEntry => faqEntry.NormalizedQuestion, "Index_FaqEntries_NormalizedQuestion")
            .IsUnique();

        builder.HasIndex(faqEntry => faqEntry.NormalizedQuestion, "Index_FaqEntries_NormalizedQuestion_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(
            faqEntry => new
            {
                faqEntry.UpdatedAtUtc,
                faqEntry.CreatedAtUtc,
                faqEntry.Id
            },
            "Index_FaqEntries_UpdatedAtUtc_CreatedAtUtc_Id");

        builder.HasIndex(
            faqEntry => new
            {
                faqEntry.Question,
                faqEntry.CreatedAtUtc,
                faqEntry.Id
            },
            "Index_FaqEntries_Question_CreatedAtUtc_Id");
    }
}
