using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;
using DrugsApp.Domain.AuthFeature.Entities;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TestFeature.Entities;
using DrugsApp.Domain.VetDrugFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SystemTextFeature.Entities;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.MealTimeManagementFeature.Entities;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace DrugsApp.Infrastructure.Persistence;

public sealed class DrugsAppDbContext(DbContextOptions<DrugsAppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
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
    public DbSet<ActiveSubstanceAlcoholInteractionOverride> ActiveSubstanceAlcoholInteractionOverrides { get; set; } = null!;
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
    public DbSet<SystemText> SystemTexts { get; set; } = null!;
    public DbSet<FaqEntry> FaqEntries { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<ClientReproductiveHealthProfile> ClientReproductiveHealthProfiles { get; set; } = null!;
    public DbSet<HealthcareProviderProfile> HealthcareProviderProfiles { get; set; } = null!;
    public DbSet<PendingEmailChange> PendingEmailChanges { get; set; } = null!;
    public DbSet<HealthCondition> HealthConditions { get; set; } = null!;
    public DbSet<UserHealthCondition> UserHealthConditions { get; set; } = null!;
    public DbSet<DrugProblem> DrugProblems { get; set; } = null!;
    public DbSet<DrugProblemType> DrugProblemTypes { get; set; } = null!;
    public DbSet<ClientAllergySummary> ClientAllergySummaries { get; set; } = null!;
    public DbSet<ClientDrugAllergy> ClientDrugAllergies { get; set; } = null!;
    public DbSet<ClientSupplementAllergy> ClientSupplementAllergies { get; set; } = null!;
    public DbSet<ClientFoodAllergy> ClientFoodAllergies { get; set; } = null!;
    public DbSet<ClientOtherAllergy> ClientOtherAllergies { get; set; } = null!;
    public DbSet<ClientMealTime> ClientMealTimes { get; set; } = null!;
    public DbSet<SystemAnnouncement> SystemAnnouncements { get; set; } = null!;
    public DbSet<UserNotification> UserNotifications { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DrugsAppDbContext).Assembly);
    }
}
