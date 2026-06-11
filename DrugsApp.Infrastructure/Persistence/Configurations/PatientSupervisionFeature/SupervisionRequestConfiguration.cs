using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DrugsApp.Infrastructure.Persistence.Configurations.PatientSupervisionFeature;

public sealed class SupervisionRequestConfiguration : BaseEntityConfiguration<SupervisionRequest>
{
    private static readonly ValueConverter<SupervisionRequestState, string> RequestStateConverter =
        new(
            x => ToRequestStateCode(x),
            x => ParseRequestState(x));

    public override void Configure(EntityTypeBuilder<SupervisionRequest> builder)
    {
        base.Configure(builder);

        builder.ToTable("supervision_requests");

        builder.Property(x => x.PatientUserId)
            .HasColumnName("patient_user_id")
            .IsRequired();

        builder.Property(x => x.HealthcareProfessionalUserId)
            .HasColumnName("healthcare_professional_user_id")
            .IsRequired();

        builder.Property(x => x.RequestState)
            .HasColumnName("request_state")
            .HasConversion(RequestStateConverter)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(x => x.RequestedAtUtc)
            .HasColumnName("requested_at_utc")
            .IsRequired();

        builder.Property(x => x.ResolvedAtUtc)
            .HasColumnName("resolved_at_utc");

        builder.Property(x => x.ResolvedByUserId)
            .HasColumnName("resolved_by_user_id");

        builder.HasIndex(x => new { x.PatientUserId, x.RequestedAtUtc, x.Id })
            .HasDatabaseName("ix_supervision_requests_patient_requested_desc_id")
            .IsDescending(false, true, true);

        builder.HasIndex(x => new { x.HealthcareProfessionalUserId, x.RequestedAtUtc, x.Id })
            .HasDatabaseName("ix_supervision_requests_provider_requested_desc_id")
            .IsDescending(false, true, true);

        builder.HasIndex(x => new { x.HealthcareProfessionalUserId, x.RequestedAtUtc })
            .HasDatabaseName("ix_supervision_requests_provider_requested");

        builder.HasIndex(x => new { x.RequestState, x.ExpiresAtUtc })
            .HasDatabaseName("ix_supervision_requests_state_expires");

        builder.HasIndex(x => new { x.PatientUserId, x.HealthcareProfessionalUserId })
            .HasDatabaseName("ux_supervision_requests_pair_pending")
            .IsUnique()
            .HasFilter("request_state = 'pending'");
    }

    private static string ToRequestStateCode(SupervisionRequestState state) =>
        state switch
        {
            SupervisionRequestState.Pending => "pending",
            SupervisionRequestState.Approved => "approved",
            SupervisionRequestState.Rejected => "rejected",
            SupervisionRequestState.Expired => "expired",
            SupervisionRequestState.Cancelled => "cancelled",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

    private static SupervisionRequestState ParseRequestState(string value) =>
        value.ToLowerInvariant() switch
        {
            "pending" => SupervisionRequestState.Pending,
            "approved" => SupervisionRequestState.Approved,
            "rejected" => SupervisionRequestState.Rejected,
            "expired" => SupervisionRequestState.Expired,
            "cancelled" => SupervisionRequestState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
}
