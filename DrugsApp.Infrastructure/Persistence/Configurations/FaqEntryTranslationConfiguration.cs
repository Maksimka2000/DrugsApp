using DrugsApp.Domain.SupportFeature;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class FaqEntryTranslationConfiguration : BaseEntityConfiguration<FaqEntryTranslation>
{
    public override void Configure(EntityTypeBuilder<FaqEntryTranslation> builder)
    {
        base.Configure(builder);

        builder.ToTable("FaqEntryTranslations");

        builder.Property(translation => translation.LanguageCode)
            .HasConversion(
                languageCode => languageCode.ToCode(),
                code => FaqLanguageCodeExtensions.ParseCode(code))
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(translation => translation.Question)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(translation => translation.NormalizedQuestion)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(translation => translation.Answer)
            .IsRequired()
            .HasColumnType("text");

        builder.HasIndex(
            translation => new
            {
                translation.FaqEntryId,
                translation.LanguageCode
            },
            "Index_FaqEntryTranslations_FaqEntryId_LanguageCode")
            .IsUnique();

        builder.HasIndex(
            translation => new
            {
                translation.LanguageCode,
                translation.NormalizedQuestion
            },
            "Index_FaqEntryTranslations_LanguageCode_NormalizedQuestion")
            .IsUnique();

        builder.HasIndex(
            translation => translation.NormalizedQuestion,
            "Index_FaqEntryTranslations_NormalizedQuestion_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(
            translation => new
            {
                translation.Question,
                translation.CreatedAtUtc,
                translation.Id
            },
            "Index_FaqEntryTranslations_Question_CreatedAtUtc_Id");
    }
}
