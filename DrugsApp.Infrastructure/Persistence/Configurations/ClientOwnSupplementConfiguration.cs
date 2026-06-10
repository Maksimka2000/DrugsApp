using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientOwnSupplementConfiguration : BaseEntityConfiguration<ClientOwnSupplement>
{
    public override void Configure(EntityTypeBuilder<ClientOwnSupplement> builder)
    {
        base.Configure(builder);

        builder.ToTable("client_own_supplements");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .IsRequired();

        builder
            .HasMany(x => x.Items)
            .WithOne(x => x.OwnSupplement)
            .HasForeignKey(x => x.OwnSupplementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.NormalizedName })
            .HasDatabaseName("UX_client_own_supplements_user_id_normalized_name")
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.NormalizedName, x.Id })
            .HasDatabaseName("IX_client_own_supplements_user_id_normalized_name_id");
    }
}
