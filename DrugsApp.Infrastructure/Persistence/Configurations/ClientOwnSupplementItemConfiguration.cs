using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class ClientOwnSupplementItemConfiguration : BaseEntityConfiguration<ClientOwnSupplementItem>
{
    public override void Configure(EntityTypeBuilder<ClientOwnSupplementItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("client_own_supplement_items");

        builder.Property(x => x.OwnSupplementId)
            .IsRequired();

        builder.Property(x => x.SupplementId)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder
            .HasOne(x => x.Supplement)
            .WithMany()
            .HasForeignKey(x => x.SupplementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OwnSupplementId, x.SupplementId })
            .HasDatabaseName("UX_client_own_supplement_items_own_supplement_id_supplement_id")
            .IsUnique();

        builder.HasIndex(x => new { x.OwnSupplementId, x.SortOrder })
            .HasDatabaseName("UX_client_own_supplement_items_own_supplement_id_sort_order")
            .IsUnique();
    }
}
