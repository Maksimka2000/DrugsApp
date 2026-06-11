using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.HealthConditionFeature.Entities;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.CreateDrugAllergy;
using DrugsApp.Features.AllergiesFeature.CreateFoodAllergy;
using DrugsApp.Features.AllergiesFeature.CreateOtherAllergy;
using DrugsApp.Features.AllergiesFeature.CreateSupplementAllergy;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;
using DrugsApp.Features.HealthConditionFeature.AddUserHealthCondition;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.ApplicationServices;
using DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;
using DrugsApp.Features.RegularFoodFeature.CreateRegularFood;
using DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using ErrorOr;

namespace DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;

/// <summary>
/// Applies approved proposal items into patient-owned medical domains.
/// </summary>
public sealed class MedicalChangeProposalApplyService(
    IRegularDrugsApplicationService regularDrugsApplicationService,
    IFoodRepository foodRepository,
    IClientRegularFoodRepository clientRegularFoodRepository,
    ISupplementRepository supplementRepository,
    IClientOwnSupplementRepository clientOwnSupplementRepository,
    IClientRegularSupplementRepository clientRegularSupplementRepository,
    IClientDrugAllergyRepository clientDrugAllergyRepository,
    IClientSupplementAllergyRepository clientSupplementAllergyRepository,
    IClientFoodAllergyRepository clientFoodAllergyRepository,
    IClientOtherAllergyRepository clientOtherAllergyRepository,
    IDrugRepository drugRepository,
    IUserHealthConditionRepository userHealthConditionRepository,
    IHealthConditionRepository healthConditionRepository,
    IDrugProblemRepository drugProblemRepository,
    IUserProfileRepository userProfileRepository,
    IReproductiveHealthProfileApplicationService reproductiveHealthProfileApplicationService) : IMedicalChangeProposalApplyService
{
    public async Task<ErrorOr<ApplyProposalResult>> ApplyAsync(
        Guid patientUserId,
        MedicalChangeProposal proposal,
        IReadOnlyCollection<MedicalChangeProposalItem> items,
        CancellationToken cancellationToken)
    {
        var created = new Dictionary<string, int>
        {
            ["regular_drug"] = 0,
            ["regular_supplement"] = 0,
            ["regular_food"] = 0,
            ["allergy_drug"] = 0,
            ["allergy_supplement"] = 0,
            ["allergy_food"] = 0,
            ["allergy_other"] = 0,
            ["health_condition"] = 0,
            ["drug_problem"] = 0,
            ["women_specific"] = 0
        };

        foreach (MedicalChangeProposalItem item in items)
        {
            ErrorOr<Success> applyResult = item.CategoryCode switch
            {
                MedicalChangeProposalCategory.RegularDrug => await ApplyRegularDrugAsync(patientUserId, item, created, cancellationToken),
                MedicalChangeProposalCategory.RegularSupplement => await ApplyRegularSupplementAsync(patientUserId, item, created, cancellationToken),
                MedicalChangeProposalCategory.RegularFood => await ApplyRegularFoodAsync(patientUserId, item, created, cancellationToken),
                MedicalChangeProposalCategory.AllergyDrug => await ApplyDrugAllergyAsync(patientUserId, item, created, cancellationToken),
                MedicalChangeProposalCategory.AllergySupplement => await ApplySupplementAllergyAsync(patientUserId, item, created, cancellationToken),
                MedicalChangeProposalCategory.AllergyFood => await ApplyFoodAllergyAsync(patientUserId, item, created, cancellationToken),
                MedicalChangeProposalCategory.AllergyOther => await ApplyOtherAllergyAsync(patientUserId, item, created, cancellationToken),
                MedicalChangeProposalCategory.HealthCondition => await ApplyHealthConditionAsync(patientUserId, item, created, cancellationToken),
                MedicalChangeProposalCategory.DrugProblem => await ApplyDrugProblemAsync(patientUserId, item, created, cancellationToken),
                MedicalChangeProposalCategory.WomenSpecific => await ApplyWomenSpecificAsync(patientUserId, item, created, cancellationToken),
                _ => Error.Validation(description: $"Unsupported proposal category: {item.CategoryCode}.")
            };

            if (applyResult.IsError)
            {
                return applyResult.Errors;
            }
        }

        return new ApplyProposalResult(created, InvalidatedReason: null);
    }

    private async Task<ErrorOr<Success>> ApplyRegularDrugAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateRegularDrugRequest> requestResult = MedicalChangeProposalJson.Deserialize<CreateRegularDrugRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        CreateRegularDrugRequest request = MergeIntakeTimes(requestResult.Value, item);
        ErrorOr<RegularDrugsFeature.Shared.RegularDrugListItemDto> result = await regularDrugsApplicationService.CreateAsync(
            patientUserId,
            request,
            cancellationToken);
        if (result.IsError)
        {
            return result.Errors;
        }

        created["regular_drug"]++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyRegularSupplementAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateRegularSupplementRequest> requestResult = MedicalChangeProposalJson.Deserialize<CreateRegularSupplementRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        CreateRegularSupplementRequest request = MergeIntakeTimes(requestResult.Value, item);
        RegularSupplementSource? source = RegularSupplementMappings.MapSourceFromToken(request.Source);
        if (source is null)
        {
            return Error.Validation(description: "Invalid source value.", metadata: new() { ["field"] = "source" });
        }

        Guid? supplementId = null;
        Guid? ownSupplementId = null;

        if (source == RegularSupplementSource.Database)
        {
            supplementId = request.SupplementId!.Value;
            Supplement? supplement = await supplementRepository.GetByIdAsync(supplementId.Value, cancellationToken);
            if (supplement is null)
            {
                return Error.NotFound(description: "Supplement not found.");
            }
        }
        else
        {
            ownSupplementId = request.OwnSupplementId!.Value;
            if (await clientOwnSupplementRepository.GetByIdForUserWithMembersAsync(
                    patientUserId,
                    ownSupplementId.Value,
                    cancellationToken) is null)
            {
                return Error.NotFound(description: "Own supplement not found.");
            }
        }

        if (await clientRegularSupplementRepository.ExistsLiveDuplicateForUserAsync(
                patientUserId,
                source.Value,
                supplementId,
                ownSupplementId,
                excludeId: null,
                cancellationToken))
        {
            return Error.Conflict(description: "A live regular supplement schedule already exists for this target.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        ErrorOr<ClientRegularSupplement> createdEntity = ClientRegularSupplement.Create(
            patientUserId,
            source.Value,
            supplementId,
            ownSupplementId,
            request.StartDate,
            request.EndDate,
            request.IntakeTimes,
            today);
        if (createdEntity.IsError)
        {
            return createdEntity.Errors;
        }

        if (source == RegularSupplementSource.Database)
        {
            Supplement supplement = (await supplementRepository.GetByIdAsync(supplementId!.Value, cancellationToken))!;
            createdEntity.Value.AttachCatalogReference(supplement);
        }
        else
        {
            ClientOwnSupplement ownSupplement = (await clientOwnSupplementRepository.GetByIdForUserWithMembersAsync(
                patientUserId,
                ownSupplementId!.Value,
                cancellationToken))!;
            createdEntity.Value.AttachOwnSupplementReference(ownSupplement);
        }

        await clientRegularSupplementRepository.AddAsync(createdEntity.Value, cancellationToken);
        created["regular_supplement"]++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyRegularFoodAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateRegularFoodRequest> requestResult = MedicalChangeProposalJson.Deserialize<CreateRegularFoodRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        CreateRegularFoodRequest request = MergeIntakeTimes(requestResult.Value, item);
        Food? food = await foodRepository.GetByIdAsync(request.FoodId, cancellationToken);
        if (food is null)
        {
            return Error.NotFound(description: "Food not found.");
        }

        if (await clientRegularFoodRepository.ExistsLiveForUserWithFoodIdAsync(
                patientUserId,
                request.FoodId,
                excludeId: null,
                cancellationToken))
        {
            return Error.Conflict(description: "A live regular food schedule already exists for this food.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        ErrorOr<ClientRegularFood> createdEntity = ClientRegularFood.Create(
            patientUserId,
            request.FoodId,
            request.StartDate,
            request.EndDate,
            request.IntakeTimes,
            today);
        if (createdEntity.IsError)
        {
            return createdEntity.Errors;
        }

        createdEntity.Value.AttachFood(food);
        await clientRegularFoodRepository.AddAsync(createdEntity.Value, cancellationToken);
        created["regular_food"]++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyDrugAllergyAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateDrugAllergyRequest> requestResult = MedicalChangeProposalJson.Deserialize<CreateDrugAllergyRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        AllergyRecordSource? source = AllergySharedMappings.MapSourceFromToken(requestResult.Value.Source);
        if (source is null)
        {
            return Error.Validation(description: "Invalid source value.");
        }

        if (source == AllergyRecordSource.Database)
        {
            Guid drugId = requestResult.Value.DrugId!.Value;
            Drug? drug = await drugRepository.GetByIdAsync(drugId, cancellationToken);
            if (drug is null)
            {
                return Error.NotFound(description: "Drug not found.");
            }

            if (drug.TypeOfDrug != DrugType.Standard ||
                drug.TypeOfProduct.Equals("Weterynaryjny", StringComparison.OrdinalIgnoreCase))
            {
                return Error.Validation(description: "Selected drug is not eligible.");
            }

            if (await clientDrugAllergyRepository.ExistsByDrugIdAsync(patientUserId, drugId, cancellationToken))
            {
                return Error.Conflict(description: "Drug allergy already exists for this drug.");
            }

            ErrorOr<ClientDrugAllergy> createdEntity = ClientDrugAllergy.CreateDatabase(
                patientUserId,
                drugId,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await clientDrugAllergyRepository.AddAsync(createdEntity.Value, cancellationToken);
        }
        else
        {
            string customName = requestResult.Value.CustomDrugName!;
            if (await clientDrugAllergyRepository.ExistsCustomNameNormalizedAsync(
                    patientUserId,
                    AllergySharedMappings.NormalizeForLookup(customName),
                    cancellationToken))
            {
                return Error.Conflict(description: "Custom drug allergy with this name already exists.");
            }

            ErrorOr<ClientDrugAllergy> createdEntity = ClientDrugAllergy.CreateCustom(
                patientUserId,
                customName,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await clientDrugAllergyRepository.AddAsync(createdEntity.Value, cancellationToken);
        }

        created["allergy_drug"]++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplySupplementAllergyAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateSupplementAllergyRequest> requestResult = MedicalChangeProposalJson.Deserialize<CreateSupplementAllergyRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        AllergyRecordSource? source = AllergySharedMappings.MapSourceFromToken(requestResult.Value.Source);
        if (source is null)
        {
            return Error.Validation(description: "Invalid source value.");
        }

        if (source == AllergyRecordSource.Database)
        {
            Guid supplementId = requestResult.Value.SupplementId!.Value;
            if (await supplementRepository.GetByIdAsync(supplementId, cancellationToken) is null)
            {
                return Error.NotFound(description: "Supplement not found.");
            }

            if (await clientSupplementAllergyRepository.ExistsBySupplementIdAsync(patientUserId, supplementId, cancellationToken))
            {
                return Error.Conflict(description: "Supplement allergy already exists.");
            }

            ErrorOr<ClientSupplementAllergy> createdEntity = ClientSupplementAllergy.CreateDatabase(
                patientUserId,
                supplementId,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await clientSupplementAllergyRepository.AddAsync(createdEntity.Value, cancellationToken);
        }
        else
        {
            string customName = requestResult.Value.CustomSupplementName!;
            if (await clientSupplementAllergyRepository.ExistsCustomNameNormalizedAsync(
                    patientUserId,
                    AllergySharedMappings.NormalizeForLookup(customName),
                    cancellationToken))
            {
                return Error.Conflict(description: "Custom supplement allergy already exists.");
            }

            ErrorOr<ClientSupplementAllergy> createdEntity = ClientSupplementAllergy.CreateCustom(
                patientUserId,
                customName,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await clientSupplementAllergyRepository.AddAsync(createdEntity.Value, cancellationToken);
        }

        created["allergy_supplement"]++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyFoodAllergyAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateFoodAllergyRequest> requestResult = MedicalChangeProposalJson.Deserialize<CreateFoodAllergyRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        AllergyRecordSource? source = AllergySharedMappings.MapSourceFromToken(requestResult.Value.Source);
        if (source is null)
        {
            return Error.Validation(description: "Invalid source value.");
        }

        if (source == AllergyRecordSource.Database)
        {
            Guid foodId = requestResult.Value.FoodId!.Value;
            if (await foodRepository.GetByIdAsync(foodId, cancellationToken) is null)
            {
                return Error.NotFound(description: "Food not found.");
            }

            if (await clientFoodAllergyRepository.ExistsByFoodIdAsync(patientUserId, foodId, cancellationToken))
            {
                return Error.Conflict(description: "Food allergy already exists.");
            }

            ErrorOr<ClientFoodAllergy> createdEntity = ClientFoodAllergy.CreateDatabase(
                patientUserId,
                foodId,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await clientFoodAllergyRepository.AddAsync(createdEntity.Value, cancellationToken);
        }
        else
        {
            string customName = requestResult.Value.CustomFoodName!;
            if (await clientFoodAllergyRepository.ExistsCustomNameNormalizedAsync(
                    patientUserId,
                    AllergySharedMappings.NormalizeForLookup(customName),
                    cancellationToken))
            {
                return Error.Conflict(description: "Custom food allergy already exists.");
            }

            ErrorOr<ClientFoodAllergy> createdEntity = ClientFoodAllergy.CreateCustom(
                patientUserId,
                customName,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await clientFoodAllergyRepository.AddAsync(createdEntity.Value, cancellationToken);
        }

        created["allergy_food"]++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyOtherAllergyAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateOtherAllergyRequest> requestResult = MedicalChangeProposalJson.Deserialize<CreateOtherAllergyRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        string allergyName = requestResult.Value.AllergyName;
        if (await clientOtherAllergyRepository.ExistsNameNormalizedAsync(
                patientUserId,
                AllergySharedMappings.NormalizeForLookup(allergyName),
                excludeId: null,
                cancellationToken))
        {
            return Error.Conflict(description: "Other allergy with this name already exists.");
        }

        ErrorOr<ClientOtherAllergy> createdEntity = ClientOtherAllergy.Create(
            patientUserId,
            allergyName,
            requestResult.Value.Note);
        if (createdEntity.IsError)
        {
            return createdEntity.Errors;
        }

        await clientOtherAllergyRepository.AddAsync(createdEntity.Value, cancellationToken);
        created["allergy_other"]++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyHealthConditionAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<AddUserHealthConditionRequest> requestResult = MedicalChangeProposalJson.Deserialize<AddUserHealthConditionRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        if (requestResult.Value.HealthConditionId is Guid healthConditionId)
        {
            HealthCondition? healthCondition = await healthConditionRepository.GetByIdAsync(healthConditionId, cancellationToken);
            if (healthCondition is null)
            {
                return Error.NotFound(description: "Health condition not found.");
            }

            if (await userHealthConditionRepository.ExistsCatalogConditionForUserAsync(
                    patientUserId,
                    healthConditionId,
                    cancellationToken))
            {
                return Error.Conflict(description: "Health condition already saved for this user.");
            }

            ErrorOr<UserHealthCondition> createdEntity = UserHealthCondition.CreateDatabase(
                patientUserId,
                healthConditionId,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            createdEntity.Value.AttachHealthCondition(healthCondition);
            await userHealthConditionRepository.AddAsync(createdEntity.Value, cancellationToken);
        }
        else
        {
            string customName = requestResult.Value.CustomHealthConditionName!;
            string normalized = NormalizeName(customName);
            if (await userHealthConditionRepository.ExistsDuplicateForUserAsync(patientUserId, normalized, cancellationToken))
            {
                return Error.Conflict(description: "A health condition with the same name already exists for this user.");
            }

            ErrorOr<UserHealthCondition> createdEntity = UserHealthCondition.CreateCustom(
                patientUserId,
                customName,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await userHealthConditionRepository.AddAsync(createdEntity.Value, cancellationToken);
        }

        created["health_condition"]++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyDrugProblemAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateDrugProblemRequest> requestResult = MedicalChangeProposalJson.Deserialize<CreateDrugProblemRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        if (requestResult.Value.DrugId is Guid drugId)
        {
            Drug? drug = await drugRepository.GetByIdAsync(drugId, cancellationToken);
            if (drug is null)
            {
                return Error.NotFound(description: "Drug not found.");
            }

            if (await drugProblemRepository.ExistsForUserWithDrugIdAsync(patientUserId, drugId, excludeId: null, cancellationToken))
            {
                return Error.Conflict(description: "Drug problem already exists for this drug.");
            }

            ErrorOr<DrugProblem> createdEntity = DrugProblem.CreateDatabase(
                patientUserId,
                drugId,
                requestResult.Value.ProblemNote);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            createdEntity.Value.AttachDrug(drug);
            await drugProblemRepository.AddAsync(createdEntity.Value, cancellationToken);
        }
        else
        {
            string customName = requestResult.Value.CustomDrugName!;
            if (await drugProblemRepository.ExistsForUserWithCustomNameAsync(
                    patientUserId,
                    NormalizeName(customName),
                    excludeId: null,
                    cancellationToken))
            {
                return Error.Conflict(description: "Custom drug problem already exists.");
            }

            ErrorOr<DrugProblem> createdEntity = DrugProblem.CreateCustom(
                patientUserId,
                customName,
                requestResult.Value.ProblemNote);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await drugProblemRepository.AddAsync(createdEntity.Value, cancellationToken);
        }

        created["drug_problem"]++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyWomenSpecificAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        Dictionary<string, int> created,
        CancellationToken cancellationToken)
    {
        ErrorOr<ReproductiveHealthProfileUpsertRequest> requestResult = MedicalChangeProposalJson.Deserialize<ReproductiveHealthProfileUpsertRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        if (await userProfileRepository.GetByUserIdAsync(patientUserId, cancellationToken) is null)
        {
            return Error.NotFound(description: "User profile not found.");
        }

        ErrorOr<ReproductiveHealthProfileUpsertResult> upsertResult = await reproductiveHealthProfileApplicationService.UpsertAsync(
            patientUserId,
            requestResult.Value,
            cancellationToken);
        if (upsertResult.IsError)
        {
            return upsertResult.Errors;
        }

        created["women_specific"]++;
        return Result.Success;
    }

    private static CreateRegularDrugRequest MergeIntakeTimes(
        CreateRegularDrugRequest request,
        MedicalChangeProposalItem item)
    {
        if (request.IntakeTimes is { Count: > 0 })
        {
            return request;
        }

        if (item.IntakeTimes.Count == 0)
        {
            return request;
        }

        return request with
        {
            IntakeTimes = item.IntakeTimes.Select(intake => intake.IntakeTime).ToArray()
        };
    }

    private static CreateRegularFoodRequest MergeIntakeTimes(
        CreateRegularFoodRequest request,
        MedicalChangeProposalItem item)
    {
        if (request.IntakeTimes is { Count: > 0 })
        {
            return request;
        }

        if (item.IntakeTimes.Count == 0)
        {
            return request;
        }

        return request with
        {
            IntakeTimes = item.IntakeTimes.Select(intake => intake.IntakeTime).ToArray()
        };
    }

    private static CreateRegularSupplementRequest MergeIntakeTimes(
        CreateRegularSupplementRequest request,
        MedicalChangeProposalItem item)
    {
        if (request.IntakeTimes is { Count: > 0 })
        {
            return request;
        }

        if (item.IntakeTimes.Count == 0)
        {
            return request;
        }

        return request with
        {
            IntakeTimes = item.IntakeTimes.Select(intake => intake.IntakeTime).ToArray()
        };
    }

    private static string NormalizeName(string value)
    {
        string trimmed = value.Trim();
        return string.Join(
            ' ',
            trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
    }
}
