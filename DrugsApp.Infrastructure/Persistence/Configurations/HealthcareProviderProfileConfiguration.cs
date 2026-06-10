using DrugsApp.Domain.UserProfileFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

internal sealed class HealthcareProviderProfileConfiguration : BaseEntityConfiguration<HealthcareProviderProfile>
{
    public override void Configure(EntityTypeBuilder<HealthcareProviderProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("healthcare_provider_profiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.UserId)
            .IsRequired();

        builder.Property(profile => profile.ProfessionCode)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(profile => profile.OtherProfessionLabel)
            .HasMaxLength(100);

        builder.Property(profile => profile.WorkplaceLabel)
            .HasMaxLength(150);

        builder.Property(profile => profile.ShortDescription)
            .HasMaxLength(500);

        builder.Property(profile => profile.OnboardingCompletedAtUtc)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(profile => profile.UserId)
            .IsUnique();

        builder.HasOne<UserProfile>()
            .WithOne()
            .HasForeignKey<HealthcareProviderProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
