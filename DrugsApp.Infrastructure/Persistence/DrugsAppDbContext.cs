using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.TestFeature.Entities;
using DrugsApp.Domain.VetDrugFeature.Entities;
using Microsoft.EntityFrameworkCore;
namespace DrugsApp.Infrastructure.Persistence;

public sealed class DrugsAppDbContext(DbContextOptions<DrugsAppDbContext> options) : DbContext(options)
{
    public DbSet<Test> Tests { get; set; } = null!;
    public DbSet<VetDrug> VetDrugs { get; set; } = null!;
    public DbSet<PharmaceuticalForm> PharmaceuticalForms { get; set; } = null!;
    public DbSet<RouteOfAdministration> RoutesOfAdministration { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DrugsAppDbContext).Assembly);
    }
}
