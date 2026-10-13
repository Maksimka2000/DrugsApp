using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations.StudyModeFeature;

public sealed class StudyClinicMemberConfiguration : BaseEntityConfiguration<StudyClinicMember>
{
    public override void Configure(EntityTypeBuilder<StudyClinicMember> builder)
    {
        base.Configure(builder);

        builder.ToTable("study_clinic_members");

        builder.Property(member => member.StudyClinicId)
            .HasColumnName("study_clinic_id")
            .IsRequired();

        builder.Property(member => member.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(member => member.MemberKind)
            .HasColumnName("member_kind")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(member => member.LoginEmail)
            .HasColumnName("login_email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(member => member.ProtectedPassword)
            .HasColumnName("protected_password")
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(member => member.PositionX)
            .HasColumnName("position_x")
            .HasColumnType("double precision")
            .IsRequired();

        builder.Property(member => member.PositionY)
            .HasColumnName("position_y")
            .HasColumnType("double precision")
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<StudyClinicMember>(member => member.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(member => new { member.StudyClinicId, member.MemberKind })
            .HasDatabaseName("ix_study_clinic_members_clinic_kind");

        builder.HasIndex(member => member.UserId)
            .HasDatabaseName("ux_study_clinic_members_user_id")
            .IsUnique();

        builder.HasIndex(member => member.LoginEmail)
            .HasDatabaseName("ux_study_clinic_members_login_email")
            .IsUnique();
    }
}
