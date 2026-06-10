using DrugsApp.Domain.AuthFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : BaseEntityConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("RefreshTokens");

        builder.Property(refreshToken => refreshToken.UserId)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(refreshToken => refreshToken.ExpiresAtUtc)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.RevokedAtUtc)
            .IsRequired(false);

        builder.Property(refreshToken => refreshToken.ReplacedByRefreshTokenId)
            .IsRequired(false);

        builder.HasIndex(refreshToken => refreshToken.Token, "Index_RefreshTokens_Token")
            .IsUnique();

        builder.HasIndex(
            refreshToken => new
            {
                refreshToken.UserId,
                refreshToken.ExpiresAtUtc
            },
            "Index_RefreshTokens_UserId_ExpiresAtUtc");
    }
}
