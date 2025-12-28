using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DrugsApp.Infrastructure.Persistence.Configurations;

public sealed class RouteOfAdministrationConfiguration : BaseEntityConfiguration<RouteOfAdministration>
{
    public override void Configure(EntityTypeBuilder<RouteOfAdministration> builder)
    {
        base.Configure(builder);

        builder.ToTable("RoutesOfAdministration");

        builder.Property(ra => ra.IsOralIntake)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ra => ra.RouteOfAdministrationName)
            .IsRequired()
            .HasMaxLength(200);
      
        builder.HasIndex(ra => ra.RouteOfAdministrationName, "Index_RoutesOfAdministration_RouteOfAdministrationName")
            .IsUnique();

        builder.HasIndex(ra => ra.RouteOfAdministrationName, "Index_RoutesOfAdministration_RouteOfAdministrationName_trgm")
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");
    }
}


