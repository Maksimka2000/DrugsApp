using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Enums;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Domain.RegularDrugsFeature.Shared;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblem;

/// <summary>
/// Handles updates to drug problems.
/// </summary>
internal sealed class UpdateDrugProblemHandler(
    IDrugProblemRepository _drugProblemRepository,
    IDrugFormRepository _drugFormRepository)
    : IRequestHandler<UpdateDrugProblemCommand, ErrorOr<UpdateDrugProblemResponse>>
{
    public async Task<ErrorOr<UpdateDrugProblemResponse>> Handle(
        UpdateDrugProblemCommand command,
        CancellationToken cancellationToken)
    {
        DrugProblem? entity = await _drugProblemRepository.GetByIdForUserAsync(
            command.UserId,
            command.Id,
            track: true,
            cancellationToken);

        if (entity is null)
        {
            return Error.NotFound(
                description: "Drug problem not found.",
                metadata: new()
                {
                    ["resourceType"] = "DrugProblem",
                    ["resourceId"] = command.Id.ToString(),
                });
        }

        if (entity.Source == DrugProblemSource.Database && HasAnyCustomDrugField(command.Request))
        {
            return SourceMismatch("customDrugName", "sourceMismatch");
        }

        if (entity.Source == DrugProblemSource.Custom && command.Request.DrugFormId is not null)
        {
            return SourceMismatch("drugFormId", "sourceMismatch");
        }

        ErrorOr<Success> noteResult = ApplyProblemNote(entity, command);
        if (noteResult.IsError)
        {
            return noteResult.Errors;
        }

        if (command.Request.DrugFormId is not null)
        {
            ErrorOr<UpdateDrugProblemResponse> result = await ApplyDatabaseUpdateAsync(entity, command, cancellationToken);
            if (result.IsError)
            {
                return result.Errors;
            }

            return result;
        }

        if (HasAnyCustomDrugField(command.Request))
        {
            ErrorOr<Success> customResult = await ApplyCustomUpdateAsync(entity, command, cancellationToken);
            if (customResult.IsError)
            {
                return customResult.Errors;
            }
        }

        return entity.MapToDetailsDto().MapToResponse();
    }

    private static bool HasAnyCustomDrugField(UpdateDrugProblemRequest request) =>
        !string.IsNullOrWhiteSpace(request.CustomDrugName) ||
        !string.IsNullOrWhiteSpace(request.CustomActiveSubstances) ||
        !string.IsNullOrWhiteSpace(request.CustomFormulationName) ||
        !string.IsNullOrWhiteSpace(request.CustomDosage);

    private static Error SourceMismatch(string field, string reason) =>
        Error.Validation(
            description: "Provided field does not match record source.",
            metadata: new()
            {
                ["field"] = field,
                ["reason"] = reason,
            });

    private static ErrorOr<Success> ApplyProblemNote(
        DrugProblem entity,
        UpdateDrugProblemCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Request.ProblemNote))
        {
            return Result.Success;
        }

        return entity.UpdateProblemNote(command.Request.ProblemNote);
    }

    private async Task<ErrorOr<UpdateDrugProblemResponse>> ApplyDatabaseUpdateAsync(
        DrugProblem entity,
        UpdateDrugProblemCommand command,
        CancellationToken cancellationToken)
    {
        Guid drugFormId = command.Request.DrugFormId!.Value;

        DrugForm? drugForm = await _drugFormRepository.GetByIdAsync(drugFormId, cancellationToken);
        if (drugForm is null)
        {
            return Error.NotFound(
                description: "Drug form not found.",
                metadata: new()
                {
                    ["resourceType"] = "DrugForm",
                    ["resourceId"] = drugFormId.ToString(),
                });
        }

        Drug drug = drugForm.Drug;
        if (!RegularDrugEligibility.IsEligibleDrug(drug))
        {
            return Error.Validation(
                description: "Selected drug form is not eligible.",
                metadata: new()
                {
                    ["field"] = "drugFormId",
                    ["reason"] = "eligibility",
                });
        }

        bool hasCatalogDuplicate = await _drugProblemRepository.ExistsForUserWithDrugFormIdAsync(
            command.UserId,
            drugFormId,
            excludeId: command.Id,
            cancellationToken);
        if (hasCatalogDuplicate)
        {
            return Error.Conflict(
                description: "Drug problem already exists for this drug form.",
                metadata: new()
                {
                    ["field"] = "drugFormId",
                    ["conflictingResourceId"] = drugFormId.ToString(),
                });
        }

        string normalizedDrugName = NormalizeForLookup(drug.CommercialName);
        bool hasCustomCollision = await _drugProblemRepository.ExistsForUserWithCustomNameAsync(
            command.UserId,
            normalizedCustomName: normalizedDrugName,
            excludeId: null,
            cancellationToken);
        if (hasCustomCollision)
        {
            return Error.Conflict(
                description: "A custom drug problem with the same name already exists.",
                metadata: new()
                {
                    ["field"] = "customDrugName",
                    ["conflictingName"] = drug.CommercialName,
                });
        }

        ErrorOr<Success> updateResult = entity.UpdateDatabaseDrug(drugFormId);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        entity.AttachDrugForm(drugForm);

        return entity.MapToDetailsDto().MapToResponse();
    }

    private async Task<ErrorOr<Success>> ApplyCustomUpdateAsync(
        DrugProblem entity,
        UpdateDrugProblemCommand command,
        CancellationToken cancellationToken)
    {
        string customName = command.Request.CustomDrugName!;
        string normalizedCustomName = NormalizeForLookup(customName);

        bool hasCustomDuplicate = await _drugProblemRepository.ExistsForUserWithCustomNameAsync(
            command.UserId,
            normalizedCustomName,
            excludeId: command.Id,
            cancellationToken);
        if (hasCustomDuplicate)
        {
            return Error.Conflict(
                description: "Custom drug problem with this name already exists.",
                metadata: new()
                {
                    ["field"] = "customDrugName",
                    ["conflictingName"] = customName,
                });
        }

        bool hasCatalogCollision = await _drugProblemRepository.ExistsForUserWithAnyDrugDisplayNameAsync(
            command.UserId,
            normalizedDrugDisplayName: normalizedCustomName,
            excludeId: null,
            cancellationToken);
        if (hasCatalogCollision)
        {
            return Error.Conflict(
                description: "A catalog-backed drug problem with the same name already exists.",
                metadata: new()
                {
                    ["field"] = "customDrugName",
                    ["conflictingName"] = customName,
                });
        }

        return entity.UpdateCustomDrugName(
            customName,
            command.Request.CustomActiveSubstances!,
            command.Request.CustomFormulationName!,
            command.Request.CustomDosage!);
    }

    private static string NormalizeForLookup(string value)
    {
        string trimmed = value.Trim();
        return string.Join(' ', trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
