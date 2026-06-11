using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF configuration for extended Identity user columns.
/// </summary>
public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(200);

        builder.Property(user => user.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("timezone('utc', now())");

        builder.HasIndex(user => user.FullName)
            .HasDatabaseName("ix_asp_net_users_full_name");

        builder.HasIndex(user => user.CreatedAtUtc)
            .HasDatabaseName("ix_asp_net_users_created_at_utc");
    }
}
