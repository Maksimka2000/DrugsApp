using DrugsApp.Domain.SupportFeature;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class FaqEntryConfiguration : BaseEntityConfiguration<FaqEntry>
{
    public override void Configure(EntityTypeBuilder<FaqEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("FaqEntries");

        builder
            .HasMany(faqEntry => faqEntry.Translations)
            .WithOne(translation => translation.FaqEntry)
            .HasForeignKey(translation => translation.FaqEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(
            faqEntry => new
            {
                faqEntry.UpdatedAtUtc,
                faqEntry.CreatedAtUtc,
                faqEntry.Id
            },
            "Index_FaqEntries_UpdatedAtUtc_CreatedAtUtc_Id");
    }
}
