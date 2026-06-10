using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : BaseEntityConfiguration<UserProfile>
{
    public override void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("user_profiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(profile => profile.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(profile => profile.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(profile => profile.DateOfBirth)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(profile => profile.Gender)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(profile => profile.ProfilePhotoStorageKey)
            .HasMaxLength(64);

        builder.HasIndex(profile => profile.Email)
            .IsUnique();
    }
}
