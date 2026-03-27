using System.Text.Json;
using DrugsApp.Domain.TempDrugFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class TempDrugImportBatchConfiguration : BaseEntityConfiguration<TempDrugImportBatch>
{
    public override void Configure(EntityTypeBuilder<TempDrugImportBatch> builder)
    {
        base.Configure(builder);

        builder.ToTable("TempDrugImportBatches");

        builder.Property(b => b.HangfireJobId)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(b => b.ImportedBy)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(b => b.NameOfFile)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(b => b.BlobFileId)
            .IsRequired();

        builder.Property(b => b.ImportStartedAt)
            .IsRequired(false);

        builder.Property(b => b.ImportFinishedAt)
            .IsRequired(false);

        builder.Property(b => b.SourceType)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(b => b.SourcePath)
            .HasMaxLength(2048)
            .IsRequired(false);

        builder.Property(b => b.MappingPlanJson)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(b => b.TotalQuantityCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.ImportedCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.SkippedCounter)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(b => b.RecordsDeletedInRevert)
            .IsRequired()
            .HasDefaultValue(0);

        ConfigureStringList(
            builder.Property(b => b.ImportedIds),
            "[]");

        ConfigureStringList(
            builder.Property(b => b.SkippedIds),
            "[]");

        ConfigureStringList(
            builder.Property(b => b.FailedToCreate),
            "[]");

        ConfigureStringList(
            builder.Property(b => b.RecordsDeletedInRevertIds),
            "[]");

        ConfigureStringList(
            builder.Property(b => b.FailedToDelete),
            "[]");

        builder.Property(b => b.Notes)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(b => b.StatusOfImport)
            .HasMaxLength(64)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(b => b.ErrorNote)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(b => b.ScheduledInfo)
            .IsRequired(false);

        builder.HasIndex(b => b.StatusOfImport);
        builder.HasIndex(b => b.CreatedAtUtc);
    }

    private static void ConfigureStringList(
        PropertyBuilder<List<string>> propertyBuilder,
        string defaultJson)
    {
        propertyBuilder
            .HasColumnType("jsonb")
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => string.IsNullOrWhiteSpace(value)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>())
            .Metadata.SetValueComparer(
                new ValueComparer<List<string>>(
                    (left, right) => left!.SequenceEqual(right!),
                    list => list.Aggregate(0, (accumulator, item) => HashCode.Combine(accumulator, item.GetHashCode())),
                    list => list.ToList()));

        propertyBuilder.HasDefaultValueSql($"'{defaultJson}'::jsonb");
    }

    
}

