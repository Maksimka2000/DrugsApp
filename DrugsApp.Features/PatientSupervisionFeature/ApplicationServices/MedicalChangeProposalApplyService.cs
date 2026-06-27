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
using DrugsApp.Domain.RegularDrugsFeature.Shared;
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
    IClientRegularSupplementRepository clientRegularSupplementRepository,
    IClientDrugAllergyRepository clientDrugAllergyRepository,
    IClientSupplementAllergyRepository clientSupplementAllergyRepository,
    IClientFoodAllergyRepository clientFoodAllergyRepository,
    IClientOtherAllergyRepository clientOtherAllergyRepository,
    IDrugRepository drugRepository,
    IDrugFormRepository drugFormRepository,
    IUserHealthConditionRepository userHealthConditionRepository,
    IHealthConditionRepository healthConditionRepository,
    IDrugProblemRepository drugProblemRepository,
    IUserProfileRepository userProfileRepository,
    IReproductiveHealthProfileApplicationService reproductiveHealthProfileApplicationService,
    MedicalChangeProposalApplyRequestResolver applyRequestResolver) : IMedicalChangeProposalApplyService
{
    private sealed class RegularIntakeApplyCounts
    {
        public int RegularDrug { get; set; }
        public int RegularSupplement { get; set; }
        public int RegularFood { get; set; }
    }

    /// <summary>
    /// Applies an approved medical change proposal into patient data across domains.
    /// </summary>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="proposal">Proposal aggregate being applied.</param>
    /// <param name="items">Proposal items to apply.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Apply result containing created record counts.</returns>
    public async Task<ErrorOr<ApplyProposalResult>> ApplyAsync(
        Guid patientUserId,
        MedicalChangeProposal proposal,
        IReadOnlyCollection<MedicalChangeProposalItem> items,
        CancellationToken cancellationToken)
    {
        _ = drugRepository;
        var counts = new RegularIntakeApplyCounts();

        foreach (MedicalChangeProposalItem item in items)
        {
            ErrorOr<Success> applyResult = item.CategoryCode switch
            {
                MedicalChangeProposalCategory.RegularDrug => await ApplyRegularDrugAsync(patientUserId, item, counts, cancellationToken),
                MedicalChangeProposalCategory.RegularSupplement => await ApplyRegularSupplementAsync(patientUserId, item, counts, cancellationToken),
                MedicalChangeProposalCategory.RegularFood => await ApplyRegularFoodAsync(patientUserId, item, counts, cancellationToken),
                MedicalChangeProposalCategory.AllergyDrug => await ApplyDrugAllergyAsync(patientUserId, item, cancellationToken),
                MedicalChangeProposalCategory.AllergySupplement => await ApplySupplementAllergyAsync(patientUserId, item, cancellationToken),
                MedicalChangeProposalCategory.AllergyFood => await ApplyFoodAllergyAsync(patientUserId, item, cancellationToken),
                MedicalChangeProposalCategory.AllergyOther => await ApplyOtherAllergyAsync(patientUserId, item, cancellationToken),
                MedicalChangeProposalCategory.HealthCondition => await ApplyHealthConditionAsync(patientUserId, item, cancellationToken),
                MedicalChangeProposalCategory.DrugProblem => await ApplyDrugProblemAsync(patientUserId, item, cancellationToken),
                MedicalChangeProposalCategory.WomenSpecific => await ApplyWomenSpecificAsync(patientUserId, item, cancellationToken),
                _ => Error.Validation(description: $"Unsupported proposal category: {item.CategoryCode}.")
            };

            if (applyResult.IsError)
            {
                return applyResult.Errors;
            }
        }

        return new ApplyProposalResult(
            counts.RegularDrug,
            counts.RegularSupplement,
            counts.RegularFood,
            InvalidatedReason: null);
    }

    private async Task<ErrorOr<Success>> ApplyRegularDrugAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        RegularIntakeApplyCounts counts,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateRegularDrugRequest> requestResult = await applyRequestResolver.ResolveRegularDrugRequestAsync(
            item,
            cancellationToken);
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        ErrorOr<RegularDrugsFeature.Shared.RegularDrugListItemDto> result = await regularDrugsApplicationService.CreateAsync(
            patientUserId,
            requestResult.Value,
            cancellationToken);
        if (result.IsError)
        {
            return result.Errors;
        }

        counts.RegularDrug++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyRegularSupplementAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        RegularIntakeApplyCounts counts,
        CancellationToken cancellationToken)
    {
        if (item.Source == MedicalChangeProposalSource.Custom)
        {
            return Error.Validation(
                description: "Custom regular supplement proposals cannot be applied automatically. Use a catalog supplement (source=database).");
        }

        ErrorOr<CreateRegularSupplementRequest> requestResult = await applyRequestResolver.ResolveRegularSupplementRequestAsync(
            patientUserId,
            item,
            cancellationToken);
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        CreateRegularSupplementRequest request = requestResult.Value;
        RegularSupplementSource? source = RegularSupplementMappings.MapSourceFromToken(request.Source);
        if (source is null)
        {
            return Error.Validation(description: "Invalid source value.", metadata: new() { ["field"] = "source" });
        }

        if (source != RegularSupplementSource.Database)
        {
            return Error.Validation(description: "Only catalog supplements can be applied from proposals.");
        }

        Guid supplementId = request.SupplementId!.Value;
        Supplement? supplement = await supplementRepository.GetByIdAsync(supplementId, cancellationToken);
        if (supplement is null)
        {
            return Error.NotFound(description: "Supplement not found.");
        }

        if (await clientRegularSupplementRepository.ExistsLiveDuplicateForUserAsync(
                patientUserId,
                source.Value,
                supplementId,
                ownSupplementId: null,
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
            ownSupplementId: null,
            request.StartDate,
            request.EndDate,
            request.IntakeTimes,
            today);
        if (createdEntity.IsError)
        {
            return createdEntity.Errors;
        }

        createdEntity.Value.AttachCatalogReference(supplement);

        await clientRegularSupplementRepository.AddAsync(createdEntity.Value, cancellationToken);
        counts.RegularSupplement++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyRegularFoodAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        RegularIntakeApplyCounts counts,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateRegularFoodRequest> requestResult = await applyRequestResolver.ResolveRegularFoodRequestAsync(
            item,
            cancellationToken);
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        CreateRegularFoodRequest request = requestResult.Value;
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
        counts.RegularFood++;
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyDrugAllergyAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
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
            Guid drugFormId = requestResult.Value.DrugFormId!.Value;
            DrugForm? drugForm = await drugFormRepository.GetByIdAsync(drugFormId, cancellationToken);
            if (drugForm is null)
            {
                return Error.NotFound(description: "Drug form not found.");
            }

            if (!RegularDrugEligibility.IsEligibleDrug(drugForm.Drug))
            {
                return Error.Validation(description: "Selected drug form is not eligible.");
            }

            if (await clientDrugAllergyRepository.ExistsByDrugFormIdAsync(patientUserId, drugFormId, cancellationToken))
            {
                return Error.Conflict(description: "Drug allergy already exists for this drug form.");
            }

            ErrorOr<ClientDrugAllergy> createdEntity = ClientDrugAllergy.CreateDatabase(
                patientUserId,
                drugFormId,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            createdEntity.Value.AttachDrugForm(drugForm);

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
                requestResult.Value.CustomActiveSubstances ?? string.Empty,
                requestResult.Value.CustomFormulationName ?? string.Empty,
                requestResult.Value.CustomDosage ?? string.Empty,
                requestResult.Value.Note);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await clientDrugAllergyRepository.AddAsync(createdEntity.Value, cancellationToken);
        }

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplySupplementAllergyAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
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

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyFoodAllergyAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
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

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyOtherAllergyAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
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
        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyHealthConditionAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
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

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyDrugProblemAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateDrugProblemRequest> requestResult = MedicalChangeProposalJson.Deserialize<CreateDrugProblemRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (requestResult.IsError)
        {
            return requestResult.Errors;
        }

        if (requestResult.Value.DrugFormId is Guid drugFormId)
        {
            DrugForm? drugForm = await drugFormRepository.GetByIdAsync(drugFormId, cancellationToken);
            if (drugForm is null)
            {
                return Error.NotFound(description: "Drug form not found.");
            }

            if (await drugProblemRepository.ExistsForUserWithDrugFormIdAsync(
                    patientUserId,
                    drugFormId,
                    excludeId: null,
                    cancellationToken))
            {
                return Error.Conflict(description: "Drug problem already exists for this drug form.");
            }

            ErrorOr<DrugProblem> createdEntity = DrugProblem.CreateDatabase(
                patientUserId,
                drugFormId,
                requestResult.Value.ProblemNote);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            createdEntity.Value.AttachDrugForm(drugForm);
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
                requestResult.Value.CustomActiveSubstances ?? string.Empty,
                requestResult.Value.CustomFormulationName ?? string.Empty,
                requestResult.Value.CustomDosage ?? string.Empty,
                requestResult.Value.ProblemNote);
            if (createdEntity.IsError)
            {
                return createdEntity.Errors;
            }

            await drugProblemRepository.AddAsync(createdEntity.Value, cancellationToken);
        }

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ApplyWomenSpecificAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
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

        return Result.Success;
    }

    private static string NormalizeName(string value)
    {
        string trimmed = value.Trim();
        return string.Join(
            ' ',
            trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
    }
}
