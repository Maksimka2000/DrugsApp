using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations.PatientSupervisionFeature;

public sealed class MedicalChangeProposalItemIntakeTimeConfiguration : BaseEntityConfiguration<MedicalChangeProposalItemIntakeTime>
{
    public override void Configure(EntityTypeBuilder<MedicalChangeProposalItemIntakeTime> builder)
    {
        base.Configure(builder);

        builder.ToTable("medical_change_proposal_item_intake_times");

        builder.Property(x => x.ProposalItemId)
            .HasColumnName("proposal_item_id")
            .IsRequired();

        builder.Property(x => x.IntakeTime)
            .HasColumnName("intake_time")
            .HasColumnType("time")
            .IsRequired();

        builder.HasIndex(x => new { x.ProposalItemId, x.IntakeTime })
            .HasDatabaseName("ux_medical_change_proposal_item_times_item_time")
            .IsUnique();
    }
}
