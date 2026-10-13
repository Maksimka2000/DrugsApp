using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations.StudyModeFeature;

public sealed class StudyClinicConfiguration : BaseEntityConfiguration<StudyClinic>
{
    public override void Configure(EntityTypeBuilder<StudyClinic> builder)
    {
        base.Configure(builder);

        builder.ToTable("study_clinics");

        builder.Property(clinic => clinic.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(clinic => clinic.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(clinic => clinic.ScenarioDescription)
            .HasColumnName("scenario_description")
            .HasMaxLength(2000);

        builder.Property(clinic => clinic.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        // existing clinics become public so they stay visible to every study mode admin
        builder.Property(clinic => clinic.Visibility)
            .HasColumnName("visibility")
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasDefaultValue(StudyClinicVisibility.Public)
            .IsRequired();

        builder.HasMany(clinic => clinic.Members)
            .WithOne()
            .HasForeignKey(member => member.StudyClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(clinic => clinic.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(clinic => clinic.NormalizedName)
            .HasDatabaseName("ux_study_clinics_normalized_name")
            .IsUnique();

        builder.HasIndex(clinic => new { clinic.CreatedByUserId, clinic.Visibility })
            .HasDatabaseName("ix_study_clinics_created_by_visibility");

        builder.HasIndex(clinic => clinic.CreatedAtUtc)
            .HasDatabaseName("ix_study_clinics_created_at_utc")
            .IsDescending();
    }
}
