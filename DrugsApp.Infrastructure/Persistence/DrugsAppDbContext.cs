using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TestFeature.Entities;
using DrugsApp.Domain.VetDrugFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Entities;
using Microsoft.EntityFrameworkCore;
namespace DrugsApp.Infrastructure.Persistence;

public sealed class DrugsAppDbContext(DbContextOptions<DrugsAppDbContext> options) : DbContext(options)
{
    public DbSet<Test> Tests { get; set; } = null!;
    public DbSet<VetDrug> VetDrugs { get; set; } = null!;
    public DbSet<Drug> Drugs { get; set; } = null!;
    public DbSet<DrugForm> DrugForms { get; set; } = null!;
    public DbSet<PharmaceuticalForm> PharmaceuticalForms { get; set; } = null!;
    public DbSet<RouteOfAdministration> RoutesOfAdministration { get; set; } = null!;
    public DbSet<Food> Foods { get; set; } = null!;
    public DbSet<FoodGroup> FoodGroups { get; set; } = null!;
    public DbSet<ActiveSubstance> ActiveSubstances { get; set; } = null!;
    public DbSet<OralMedicationIntakeGuidelines> OralMedicationIntakeGuidelines { get; set; } = null!;
    public DbSet<ActiveSubstanceGroup> ActiveSubstanceGroups { get; set; } = null!;
    public DbSet<TempDrug> TempDrugs { get; set; } = null!;
    public DbSet<TempDrugImportBatch> TempDrugImportBatches { get; set; } = null!;
    public DbSet<DefaultMappingForColumnsAttribute> DefaultMappingForColumnsAttributes { get; set; } = null!;
    public DbSet<Supplement> Supplements { get; set; } = null!;
    public DbSet<SupplementGroup> SupplementGroups { get; set; } = null!;
    public DbSet<Interaction> Interactions { get; set; } = null!;
    public DbSet<InteractionParty> InteractionParties { get; set; } = null!;
    public DbSet<InteractionExpanded> InteractionExpandedRows { get; set; } = null!;
    public DbSet<InteractionSeverity> InteractionSeverities { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DrugsAppDbContext).Assembly);
    }
}
