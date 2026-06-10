using DrugsApp.Domain.NotificationsFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class SystemAnnouncementConfiguration : BaseEntityConfiguration<SystemAnnouncement>
{
    public override void Configure(EntityTypeBuilder<SystemAnnouncement> builder)
    {
        base.Configure(builder);

        builder.ToTable("SystemAnnouncements");

        builder.Property(x => x.AudienceScopeCode)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.PrimaryTagCode)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SummaryText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.BodyText)
            .IsRequired();

        builder.Property(x => x.DeepLinkPath)
            .HasMaxLength(512);

        builder.Property(x => x.ActiveFromUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.ActiveFromUtc, x.ActiveToUtc })
            .HasDatabaseName("IX_SystemAnnouncements_ActiveFromUtc_ActiveToUtc");
    }
}
