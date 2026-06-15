using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations.PatientSupervisionFeature;

public sealed class MedicalChangeProposalConfiguration : BaseEntityConfiguration<MedicalChangeProposal>
{
    public override void Configure(EntityTypeBuilder<MedicalChangeProposal> builder)
    {
        base.Configure(builder);

        builder.ToTable("medical_change_proposals");

        builder.Property(x => x.SupervisionConnectionId)
            .HasColumnName("supervision_connection_id")
            .IsRequired();

        builder.Property(x => x.PatientUserId)
            .HasColumnName("patient_user_id")
            .IsRequired();

        builder.Property(x => x.HealthcareProfessionalUserId)
            .HasColumnName("healthcare_professional_user_id")
            .IsRequired();

        builder.Property(x => x.ProposalState)
            .HasColumnName("proposal_state")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.AccessLevelSnapshot)
            .HasColumnName("access_level_snapshot")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.ProposalSummary)
            .HasColumnName("proposal_summary")
            .IsRequired();

        builder.Property(x => x.SubmittedAtUtc)
            .HasColumnName("submitted_at_utc")
            .IsRequired();

        builder.Property(x => x.ResolvedAtUtc)
            .HasColumnName("resolved_at_utc");

        builder.Property(x => x.ResolvedByPatientUserId)
            .HasColumnName("resolved_by_patient_user_id");

        builder.HasIndex(x => new { x.PatientUserId, x.ProposalState, x.SubmittedAtUtc, x.Id })
            .HasDatabaseName("ix_medical_change_proposals_patient_state_submitted_desc_id")
            .IsDescending(false, false, true, true);

        builder.HasIndex(x => new { x.HealthcareProfessionalUserId, x.ProposalState, x.SubmittedAtUtc, x.Id })
            .HasDatabaseName("ix_medical_change_proposals_provider_state_submitted_desc_id")
            .IsDescending(false, false, true, true);

        builder.HasIndex(x => new { x.SupervisionConnectionId, x.ProposalState })
            .HasDatabaseName("ix_medical_change_proposals_connection_state");

        builder.HasOne<SupervisionConnection>()
            .WithMany()
            .HasForeignKey(x => x.SupervisionConnectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
