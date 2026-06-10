using DrugsApp.Domain.AuthFeature.Entities;
using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

internal sealed class PendingEmailChangeConfiguration : BaseEntityConfiguration<PendingEmailChange>
{
    public override void Configure(EntityTypeBuilder<PendingEmailChange> builder)
    {
        base.Configure(builder);

        builder.ToTable("pending_email_changes");

        builder.HasKey(pending => pending.Id);

        builder.Property(pending => pending.UserId)
            .IsRequired();

        builder.Property(pending => pending.NewEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pending => pending.TokenHash)
            .IsRequired();

        builder.Property(pending => pending.ExpiresAtUtc)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(pending => pending.UserId)
            .IsUnique();

        builder.HasIndex(pending => pending.ExpiresAtUtc);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(pending => pending.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
