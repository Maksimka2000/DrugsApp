using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations.PatientSupervisionFeature;

public sealed class SupervisionConnectionConfiguration : BaseEntityConfiguration<SupervisionConnection>
{
    public override void Configure(EntityTypeBuilder<SupervisionConnection> builder)
    {
        base.Configure(builder);

        builder.ToTable("supervision_connections");

        builder.Property(x => x.PatientUserId)
            .HasColumnName("patient_user_id")
            .IsRequired();

        builder.Property(x => x.HealthcareProfessionalUserId)
            .HasColumnName("healthcare_professional_user_id")
            .IsRequired();

        builder.Property(x => x.AccessLevelCode)
            .HasColumnName("access_level_code")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.ConnectionState)
            .HasColumnName("connection_state")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.ApprovedAtUtc)
            .HasColumnName("approved_at_utc")
            .IsRequired();

        builder.Property(x => x.RemovedAtUtc)
            .HasColumnName("removed_at_utc");

        builder.HasIndex(x => new { x.PatientUserId, x.ConnectionState, x.ApprovedAtUtc, x.Id })
            .HasDatabaseName("ix_supervision_connections_patient_state_approved_desc_id")
            .IsDescending(false, false, true, true);

        builder.HasIndex(x => new { x.HealthcareProfessionalUserId, x.ConnectionState, x.ApprovedAtUtc, x.Id })
            .HasDatabaseName("ix_supervision_connections_provider_state_approved_desc_id")
            .IsDescending(false, false, true, true);

        builder.HasIndex(x => new { x.PatientUserId, x.HealthcareProfessionalUserId })
            .HasDatabaseName("ux_supervision_connections_pair_active")
            .IsUnique()
            .HasFilter("connection_state = 'Active'");
    }
}
