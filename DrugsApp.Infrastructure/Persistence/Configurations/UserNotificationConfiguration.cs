using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationConfiguration : BaseEntityConfiguration<UserNotification>
{
    public override void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        base.Configure(builder);

        builder.ToTable("UserNotifications");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TypeCode)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.PrimaryTagCode)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.AudienceScopeCode)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SummaryText)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.SourceEventKey)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.RelatedAggregateType)
            .HasMaxLength(64);

        builder.Property(x => x.DeepLinkPath)
            .HasMaxLength(512);

        builder.Property(x => x.ActionState)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.RequiresAction)
            .IsRequired();

        builder.Property(x => x.IsRead)
            .IsRequired();

        builder.HasOne(x => x.SystemAnnouncement)
            .WithMany()
            .HasForeignKey(x => x.SystemAnnouncementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.SourceEventKey })
            .IsUnique()
            .HasDatabaseName("IX_UserNotifications_UserId_SourceEventKey");

        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAtUtc, x.Id })
            .HasDatabaseName("IX_UserNotifications_UserId_IsRead_CreatedAtUtc_Id");

        builder.HasIndex(x => new { x.UserId, x.AudienceScopeCode, x.CreatedAtUtc })
            .HasDatabaseName("IX_UserNotifications_UserId_AudienceScopeCode_CreatedAtUtc");
    }
}
