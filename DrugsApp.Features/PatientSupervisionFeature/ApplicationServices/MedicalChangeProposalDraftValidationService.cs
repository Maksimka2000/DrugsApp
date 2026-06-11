using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.HealthConditionFeature.Repositories;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.CreateDrugAllergy;
using DrugsApp.Features.AllergiesFeature.CreateFoodAllergy;
using DrugsApp.Features.AllergiesFeature.CreateOtherAllergy;
using DrugsApp.Features.AllergiesFeature.CreateSupplementAllergy;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;
using DrugsApp.Features.HealthConditionFeature.AddUserHealthCondition;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;
using DrugsApp.Features.RegularFoodFeature.CreateRegularFood;
using DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using ErrorOr;

namespace DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;

/// <summary>
/// Validates provider draft items before persistence.
/// </summary>
public sealed class MedicalChangeProposalDraftValidationService(
    IDrugRepository drugRepository,
    IFoodRepository foodRepository,
    ISupplementRepository supplementRepository,
    IHealthConditionRepository healthConditionRepository) : IMedicalChangeProposalDraftValidationService
{
    public async Task<ErrorOr<Success>> ValidateAsync(
        Guid patientUserId,
        IReadOnlyCollection<ProposalDraftItem> draftItems,
        CancellationToken cancellationToken)
    {
        if (draftItems.Count == 0)
        {
            return Error.Validation(description: "At least one draft item is required.");
        }

        foreach (ProposalDraftItem item in draftItems)
        {
            ErrorOr<Success> addOnlyResult = MedicalChangeProposalJson.ValidateAddOnlyPayload(item.ApplyPayload, "apply_payload");
            if (addOnlyResult.IsError)
            {
                return addOnlyResult.Errors;
            }

            ErrorOr<Success> categoryResult = await ValidateCategoryAsync(item, cancellationToken);
            if (categoryResult.IsError)
            {
                return categoryResult.Errors;
            }
        }

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ValidateCategoryAsync(
        ProposalDraftItem item,
        CancellationToken cancellationToken)
    {
        return item.CategoryCode switch
        {
            MedicalChangeProposalCategory.RegularDrug => ValidateRegularDrug(item),
            MedicalChangeProposalCategory.RegularSupplement => ValidateRegularSupplement(item),
            MedicalChangeProposalCategory.RegularFood => ValidateRegularFood(item),
            MedicalChangeProposalCategory.AllergyDrug => ValidateDrugAllergy(item),
            MedicalChangeProposalCategory.AllergySupplement => ValidateSupplementAllergy(item),
            MedicalChangeProposalCategory.AllergyFood => ValidateFoodAllergy(item),
            MedicalChangeProposalCategory.AllergyOther => ValidateOtherAllergy(item),
            MedicalChangeProposalCategory.HealthCondition => await ValidateHealthConditionAsync(item, cancellationToken),
            MedicalChangeProposalCategory.DrugProblem => await ValidateDrugProblemAsync(item, cancellationToken),
            MedicalChangeProposalCategory.WomenSpecific => ValidateWomenSpecific(item),
            _ => Error.Validation(description: $"Unsupported category: {item.CategoryCode}.")
        };
    }

    private static ErrorOr<Success> ValidateRegularDrug(ProposalDraftItem item)
    {
        ErrorOr<CreateRegularDrugRequest> request = MedicalChangeProposalJson.Deserialize<CreateRegularDrugRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (request.IsError)
        {
            return request.Errors;
        }

        if (request.Value.DrugId == Guid.Empty || request.Value.DrugFormId == Guid.Empty)
        {
            return Error.Validation(description: "Regular drug payload requires drug_id and drug_form_id.");
        }

        return Result.Success;
    }

    private static ErrorOr<Success> ValidateRegularSupplement(ProposalDraftItem item)
    {
        ErrorOr<CreateRegularSupplementRequest> request = MedicalChangeProposalJson.Deserialize<CreateRegularSupplementRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (request.IsError)
        {
            return request.Errors;
        }

        if (RegularSupplementMappings.MapSourceFromToken(request.Value.Source) is null)
        {
            return Error.Validation(description: "Invalid supplement source.");
        }

        return Result.Success;
    }

    private static ErrorOr<Success> ValidateRegularFood(ProposalDraftItem item)
    {
        ErrorOr<CreateRegularFoodRequest> request = MedicalChangeProposalJson.Deserialize<CreateRegularFoodRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (request.IsError)
        {
            return request.Errors;
        }

        if (request.Value.FoodId == Guid.Empty)
        {
            return Error.Validation(description: "Regular food payload requires food_id.");
        }

        return Result.Success;
    }

    private static ErrorOr<Success> ValidateDrugAllergy(ProposalDraftItem item)
    {
        ErrorOr<CreateDrugAllergyRequest> request = MedicalChangeProposalJson.Deserialize<CreateDrugAllergyRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (request.IsError)
        {
            return request.Errors;
        }

        if (AllergySharedMappings.MapSourceFromToken(request.Value.Source) is null)
        {
            return Error.Validation(description: "Invalid allergy source.");
        }

        return Result.Success;
    }

    private static ErrorOr<Success> ValidateSupplementAllergy(ProposalDraftItem item)
    {
        ErrorOr<CreateSupplementAllergyRequest> request = MedicalChangeProposalJson.Deserialize<CreateSupplementAllergyRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (request.IsError)
        {
            return request.Errors;
        }

        if (AllergySharedMappings.MapSourceFromToken(request.Value.Source) is null)
        {
            return Error.Validation(description: "Invalid allergy source.");
        }

        return Result.Success;
    }

    private static ErrorOr<Success> ValidateFoodAllergy(ProposalDraftItem item)
    {
        ErrorOr<CreateFoodAllergyRequest> request = MedicalChangeProposalJson.Deserialize<CreateFoodAllergyRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (request.IsError)
        {
            return request.Errors;
        }

        if (AllergySharedMappings.MapSourceFromToken(request.Value.Source) is null)
        {
            return Error.Validation(description: "Invalid allergy source.");
        }

        return Result.Success;
    }

    private static ErrorOr<Success> ValidateOtherAllergy(ProposalDraftItem item)
    {
        ErrorOr<CreateOtherAllergyRequest> request = MedicalChangeProposalJson.Deserialize<CreateOtherAllergyRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (request.IsError)
        {
            return request.Errors;
        }

        if (string.IsNullOrWhiteSpace(request.Value.AllergyName))
        {
            return Error.Validation(description: "Other allergy payload requires allergy_name.");
        }

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ValidateHealthConditionAsync(
        ProposalDraftItem item,
        CancellationToken cancellationToken)
    {
        ErrorOr<AddUserHealthConditionRequest> request = MedicalChangeProposalJson.Deserialize<AddUserHealthConditionRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (request.IsError)
        {
            return request.Errors;
        }

        if (request.Value.HealthConditionId is Guid healthConditionId)
        {
            if (await healthConditionRepository.GetByIdAsync(healthConditionId, cancellationToken) is null)
            {
                return Error.NotFound(description: "Health condition not found.");
            }
        }
        else if (string.IsNullOrWhiteSpace(request.Value.CustomHealthConditionName))
        {
            return Error.Validation(description: "Health condition payload requires health_condition_id or custom_health_condition_name.");
        }

        return Result.Success;
    }

    private async Task<ErrorOr<Success>> ValidateDrugProblemAsync(
        ProposalDraftItem item,
        CancellationToken cancellationToken)
    {
        ErrorOr<CreateDrugProblemRequest> request = MedicalChangeProposalJson.Deserialize<CreateDrugProblemRequest>(
            item.ApplyPayload,
            "apply_payload");
        if (request.IsError)
        {
            return request.Errors;
        }

        if (string.IsNullOrWhiteSpace(request.Value.ProblemNote))
        {
            return Error.Validation(description: "Drug problem payload requires problem_note.");
        }

        if (request.Value.DrugId is Guid drugId)
        {
            if (await drugRepository.GetByIdAsync(drugId, cancellationToken) is null)
            {
                return Error.NotFound(description: "Drug not found.");
            }
        }
        else if (string.IsNullOrWhiteSpace(request.Value.CustomDrugName))
        {
            return Error.Validation(description: "Drug problem payload requires drug_id or custom_drug_name.");
        }

        return Result.Success;
    }

    private static ErrorOr<Success> ValidateWomenSpecific(ProposalDraftItem item)
    {
        ErrorOr<ReproductiveHealthProfileUpsertRequest> request = MedicalChangeProposalJson.Deserialize<ReproductiveHealthProfileUpsertRequest>(
            item.ApplyPayload,
            "apply_payload");
        return request.IsError ? request.Errors : Result.Success;
    }
}
