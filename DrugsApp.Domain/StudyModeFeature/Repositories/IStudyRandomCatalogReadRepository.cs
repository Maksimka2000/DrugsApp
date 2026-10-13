using DrugsApp.Domain.StudyModeFeature.ReadModels;

namespace DrugsApp.Domain.StudyModeFeature.Repositories;

public interface IStudyRandomCatalogReadRepository
{
    Task<List<StudyRandomDrugFormCandidate>> PickRandomEligibleDrugFormsAsync(
        int count,
        bool oralOnly,
        CancellationToken cancellationToken);

    Task<List<Guid>> PickRandomSupplementIdsAsync(int count, CancellationToken cancellationToken);

    Task<List<Guid>> PickRandomFoodIdsAsync(int count, CancellationToken cancellationToken);

    Task<List<Guid>> PickRandomHealthConditionIdsAsync(int count, CancellationToken cancellationToken);
}
