using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

internal sealed class ClientReproductiveHealthProfileConfiguration : BaseEntityConfiguration<ClientReproductiveHealthProfile>
{
    public override void Configure(EntityTypeBuilder<ClientReproductiveHealthProfile> builder)
    {
        base.Configure(builder);

        builder.ToTable("client_reproductive_health_profiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.UserId)
            .IsRequired();

        builder.Property(profile => profile.ContraceptionMethodCode)
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(profile => profile.CurrentlyPregnant)
            .IsRequired();

        builder.Property(profile => profile.PlanningToGetPregnant)
            .IsRequired();

        builder.Property(profile => profile.Menopause)
            .IsRequired();

        builder.HasIndex(profile => profile.UserId)
            .IsUnique();

        builder.HasOne<UserProfile>()
            .WithOne()
            .HasForeignKey<ClientReproductiveHealthProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
