using DrugsApp.Domain.StudyModeFeature;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.CreateDrugAllergy;
using DrugsApp.Features.AllergiesFeature.CreateFoodAllergy;
using DrugsApp.Features.AllergiesFeature.CreateOtherAllergy;
using DrugsApp.Features.AllergiesFeature.CreateSupplementAllergy;
using DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;
using DrugsApp.Features.HealthConditionFeature.AddUserHealthCondition;
using DrugsApp.Features.MealTimeManagementFeature.CreateMealTime;
using DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;
using DrugsApp.Features.RegularFoodFeature.CreateRegularFood;
using DrugsApp.Features.RegularSupplementsFeature.CreateOwnSupplement;
using DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.ApplicationServices;

/// <summary>
/// Generates random sample medical data for a study patient: loads a shuffled candidate pool (random catalog rows
/// or curated values), then dispatches the existing client create command per candidate inside the caller's
/// transaction, skipping duplicates and stopping at the per-user limit.
/// </summary>
internal sealed class StudyRandomMedicalDataApplicationService(
    ISender _sender,
    IStudyRandomCatalogReadRepository _studyRandomCatalogReadRepository)
    : IStudyRandomMedicalDataApplicationService
{
    private const int MAX_CANDIDATE_POOL = 40;
    private const int MAX_START_DAYS_AGO = 180;
    private const string DATABASE_SOURCE = "database";
    private const string LIMIT_FIELD = "limit";
    private const string ELIGIBILITY_REASON = "eligibility";

    private enum AttemptOutcome
    {
        Created,
        Skipped,
        LimitReached,
        Failed
    }

    /// <summary>
    /// Adds up to <paramref name="count"/> random items to the given section of the study patient.
    /// </summary>
    /// <param name="patientUserId">
    /// Study patient user identifier.
    /// </param>
    /// <param name="section">
    /// Medical section to fill.
    /// </param>
    /// <param name="count">
    /// Requested number of items.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the operation.
    /// </param>
    /// <returns>
    /// Generation outcome or the first unexpected error.
    /// </returns>
    public async Task<ErrorOr<StudyRandomDataResult>> GenerateAsync(
        Guid patientUserId,
        StudyMedicalSection section,
        int count,
        CancellationToken cancellationToken)
    {
        int poolSize = Math.Min(count * 3 + 10, MAX_CANDIDATE_POOL);

        List<Func<Task<(AttemptOutcome Outcome, List<Error> Errors)>>> attempts =
            await BuildAttemptsAsync(patientUserId, section, poolSize, cancellationToken);

        int createdCount = 0;
        foreach (Func<Task<(AttemptOutcome Outcome, List<Error> Errors)>> attempt in attempts)
        {
            if (createdCount >= count)
            {
                break;
            }

            (AttemptOutcome outcome, List<Error> errors) = await attempt();
            switch (outcome)
            {
                case AttemptOutcome.Created:
                    createdCount++;
                    break;
                case AttemptOutcome.LimitReached:
                    return new StudyRandomDataResult(section, count, createdCount, LimitReached: true, errors);
                case AttemptOutcome.Failed:
                    return errors;
            }
        }

        return new StudyRandomDataResult(section, count, createdCount, LimitReached: false, LimitErrors: null);
    }

    private async Task<List<Func<Task<(AttemptOutcome Outcome, List<Error> Errors)>>>> BuildAttemptsAsync(
        Guid patientUserId,
        StudyMedicalSection section,
        int poolSize,
        CancellationToken cancellationToken)
    {
        return section switch
        {
            StudyMedicalSection.DrugAllergies => (await PickDrugFormsAsync(poolSize, oralOnly: false, cancellationToken))
                .Select(candidate => Attempt(new CreateDrugAllergyCommand(
                    patientUserId,
                    new CreateDrugAllergyRequest(candidate.DrugFormId, null, null, null, null, PickOptional(StudyRandomMedicalDataCatalog.AllergyNotes), DATABASE_SOURCE)),
                    cancellationToken))
                .ToList(),

            StudyMedicalSection.SupplementAllergies => (await _studyRandomCatalogReadRepository.PickRandomSupplementIdsAsync(poolSize, cancellationToken))
                .Select(supplementId => Attempt(new CreateSupplementAllergyCommand(
                    patientUserId,
                    new CreateSupplementAllergyRequest(supplementId, null, PickOptional(StudyRandomMedicalDataCatalog.AllergyNotes), DATABASE_SOURCE)),
                    cancellationToken))
                .ToList(),

            StudyMedicalSection.FoodAllergies => (await _studyRandomCatalogReadRepository.PickRandomFoodIdsAsync(poolSize, cancellationToken))
                .Select(foodId => Attempt(new CreateFoodAllergyCommand(
                    patientUserId,
                    new CreateFoodAllergyRequest(foodId, null, PickOptional(StudyRandomMedicalDataCatalog.AllergyNotes), DATABASE_SOURCE)),
                    cancellationToken))
                .ToList(),

            StudyMedicalSection.OtherAllergies => Shuffle(StudyRandomMedicalDataCatalog.OtherAllergyNames)
                .Select(allergyName => Attempt(new CreateOtherAllergyCommand(
                    patientUserId,
                    new CreateOtherAllergyRequest(allergyName, PickOptional(StudyRandomMedicalDataCatalog.AllergyNotes))),
                    cancellationToken))
                .ToList(),

            StudyMedicalSection.HealthConditions => (await _studyRandomCatalogReadRepository.PickRandomHealthConditionIdsAsync(poolSize, cancellationToken))
                .Select(healthConditionId => Attempt(new AddUserHealthConditionCommand(
                    patientUserId,
                    new AddUserHealthConditionRequest(healthConditionId, null, PickOptional(StudyRandomMedicalDataCatalog.HealthConditionNotes))),
                    cancellationToken))
                .ToList(),

            StudyMedicalSection.DrugProblems => (await PickDrugFormsAsync(poolSize, oralOnly: false, cancellationToken))
                .Select(candidate => Attempt(new CreateDrugProblemCommand(
                    patientUserId,
                    new CreateDrugProblemRequest(candidate.DrugFormId, null, null, null, null, PickOne(StudyRandomMedicalDataCatalog.DrugProblemNotes))),
                    cancellationToken))
                .ToList(),

            StudyMedicalSection.RegularDrugs => (await PickDrugFormsAsync(poolSize, oralOnly: true, cancellationToken))
                .Select(candidate => Attempt(new CreateRegularDrugCommand(
                    patientUserId,
                    new CreateRegularDrugRequest(candidate.DrugId, candidate.DrugFormId, PickOngoingStartDate(), null, PickIntakeTimes())),
                    cancellationToken))
                .ToList(),

            StudyMedicalSection.RegularSupplements => (await _studyRandomCatalogReadRepository.PickRandomSupplementIdsAsync(poolSize, cancellationToken))
                .Select(supplementId => Attempt(new CreateRegularSupplementCommand(
                    patientUserId,
                    new CreateRegularSupplementRequest(DATABASE_SOURCE, supplementId, null, PickOngoingStartDate(), null, PickIntakeTimes())),
                    cancellationToken))
                .ToList(),

            StudyMedicalSection.OwnSupplements => await BuildOwnSupplementAttemptsAsync(patientUserId, cancellationToken),

            StudyMedicalSection.RegularFood => (await _studyRandomCatalogReadRepository.PickRandomFoodIdsAsync(poolSize, cancellationToken))
                .Select(foodId => Attempt(new CreateRegularFoodCommand(
                    patientUserId,
                    new CreateRegularFoodRequest(foodId, PickOngoingStartDate(), null, PickIntakeTimes())),
                    cancellationToken))
                .ToList(),

            StudyMedicalSection.MealTimes => Shuffle(StudyRandomMedicalDataCatalog.MealTimes)
                .Select(mealTime => Attempt(new CreateMealTimeCommand(
                    patientUserId,
                    new CreateMealTimeRequest(mealTime.MealName, mealTime.IntakeTime)),
                    cancellationToken))
                .ToList(),

            _ => []
        };
    }

    private async Task<List<Func<Task<(AttemptOutcome Outcome, List<Error> Errors)>>>> BuildOwnSupplementAttemptsAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        List<Guid> supplementIds = await _studyRandomCatalogReadRepository.PickRandomSupplementIdsAsync(
            MAX_CANDIDATE_POOL,
            cancellationToken);
        if (supplementIds.Count == 0)
        {
            return [];
        }

        return Shuffle(StudyRandomMedicalDataCatalog.OwnSupplementNames)
            .Select(name =>
            {
                int memberCount = Math.Min(Random.Shared.Next(2, 5), supplementIds.Count);
                List<Guid> memberIds = Shuffle(supplementIds).Take(memberCount).ToList();
                return Attempt(
                    new CreateOwnSupplementCommand(patientUserId, new CreateOwnSupplementRequest(name, memberIds)),
                    cancellationToken);
            })
            .ToList();
    }

    private Task<List<StudyRandomDrugFormCandidate>> PickDrugFormsAsync(
        int poolSize,
        bool oralOnly,
        CancellationToken cancellationToken) =>
        _studyRandomCatalogReadRepository.PickRandomEligibleDrugFormsAsync(poolSize, oralOnly, cancellationToken);

    private Func<Task<(AttemptOutcome Outcome, List<Error> Errors)>> Attempt<TResponse>(
        IRequest<ErrorOr<TResponse>> command,
        CancellationToken cancellationToken) =>
        async () =>
        {
            ErrorOr<TResponse> result = await _sender.Send(command, cancellationToken);
            return (Classify(result), result.IsError ? result.Errors : []);
        };

    private static AttemptOutcome Classify<TResponse>(ErrorOr<TResponse> result)
    {
        if (!result.IsError)
        {
            return AttemptOutcome.Created;
        }

        Error firstError = result.FirstError;
        bool isLimitConflict = firstError.Type == ErrorType.Conflict && HasMetadata(firstError, "field", LIMIT_FIELD);
        if (isLimitConflict)
        {
            return AttemptOutcome.LimitReached;
        }

        // duplicates, overlaps, ineligible or concurrently removed candidates are rejected before anything is staged
        bool isSkippableCandidate = firstError.Type is ErrorType.Conflict or ErrorType.NotFound
            || (firstError.Type == ErrorType.Validation && HasMetadata(firstError, "reason", ELIGIBILITY_REASON));

        return isSkippableCandidate ? AttemptOutcome.Skipped : AttemptOutcome.Failed;
    }

    private static bool HasMetadata(Error error, string key, string expectedValue) =>
        error.Metadata is not null
        && error.Metadata.TryGetValue(key, out object? value)
        && string.Equals(value?.ToString(), expectedValue, StringComparison.Ordinal);

    private static DateOnly PickOngoingStartDate() =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-Random.Shared.Next(0, MAX_START_DAYS_AGO + 1));

    private static List<TimeOnly> PickIntakeTimes() =>
        Shuffle(StudyRandomMedicalDataCatalog.IntakeTimes)
            .Take(Random.Shared.Next(1, 4))
            .Order()
            .ToList();

    private static string PickOne(IReadOnlyList<string> values) =>
        values[Random.Shared.Next(values.Count)];

    private static string? PickOptional(IReadOnlyList<string> values) =>
        Random.Shared.Next(3) == 0 ? null : PickOne(values);

    private static List<T> Shuffle<T>(IReadOnlyList<T> values)
    {
        T[] shuffled = values.ToArray();
        Random.Shared.Shuffle(shuffled);
        return shuffled.ToList();
    }
}
