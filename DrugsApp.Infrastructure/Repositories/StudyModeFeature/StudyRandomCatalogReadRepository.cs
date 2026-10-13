using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.StudyModeFeature;

public sealed class StudyRandomCatalogReadRepository(DrugsAppDbContext _context) : IStudyRandomCatalogReadRepository
{
    private const string VETERINARY_PRODUCT_TYPE = "weterynaryjny";

    public async Task<List<StudyRandomDrugFormCandidate>> PickRandomEligibleDrugFormsAsync(
        int count,
        bool oralOnly,
        CancellationToken cancellationToken)
    {
        // same eligibility rule as RegularDrugEligibility: standard, non-veterinary drugs
        IQueryable<DrugForm> query = _context.DrugForms
            .AsNoTracking()
            .Where(drugForm => drugForm.Drug.TypeOfDrug == DrugType.Standard
                && drugForm.Drug.TypeOfProduct.ToLower() != VETERINARY_PRODUCT_TYPE);

        if (oralOnly)
        {
            query = query.Where(drugForm => drugForm.IsOral);
        }

        return await query
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .Select(drugForm => new StudyRandomDrugFormCandidate(drugForm.DrugId, drugForm.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> PickRandomSupplementIdsAsync(int count, CancellationToken cancellationToken)
    {
        return await _context.Supplements
            .AsNoTracking()
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .Select(supplement => supplement.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> PickRandomFoodIdsAsync(int count, CancellationToken cancellationToken)
    {
        return await _context.Foods
            .AsNoTracking()
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .Select(food => food.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> PickRandomHealthConditionIdsAsync(int count, CancellationToken cancellationToken)
    {
        return await _context.HealthConditions
            .AsNoTracking()
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .Select(healthCondition => healthCondition.Id)
            .ToListAsync(cancellationToken);
    }
}
