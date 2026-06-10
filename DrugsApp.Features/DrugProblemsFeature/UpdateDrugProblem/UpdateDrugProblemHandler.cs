using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Enums;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblem;

/// <summary>
/// Handles updates to drug problems.
/// </summary>
internal sealed class UpdateDrugProblemHandler(
    IDrugProblemRepository _drugProblemRepository,
    IDrugRepository _drugRepository)
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

        if (entity.Source == DrugProblemSource.Database && command.Request.CustomDrugName is not null)
        {
            return SourceMismatch("customDrugName", "sourceMismatch");
        }

        if (entity.Source == DrugProblemSource.Custom && command.Request.DrugId is not null)
        {
            return SourceMismatch("drugId", "sourceMismatch");
        }

        ErrorOr<Success> noteResult = ApplyProblemNote(entity, command);
        if (noteResult.IsError)
        {
            return noteResult.Errors;
        }

        if (command.Request.DrugId is not null)
        {
            ErrorOr<UpdateDrugProblemResponse> result = await ApplyDatabaseUpdateAsync(entity, command, cancellationToken);
            if (result.IsError)
            {
                return result.Errors;
            }

            return result;
        }

        if (!string.IsNullOrWhiteSpace(command.Request.CustomDrugName))
        {
            ErrorOr<Success> customResult = await ApplyCustomUpdateAsync(entity, command, cancellationToken);
            if (customResult.IsError)
            {
                return customResult.Errors;
            }
        }

        return entity.MapToDetailsDto().MapToResponse();
    }

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
        Guid drugId = command.Request.DrugId!.Value;

        Drug? drug = await _drugRepository.GetByIdAsync(drugId, cancellationToken);
        if (drug is null)
        {
            return Error.NotFound(
                description: "Drug not found.",
                metadata: new()
                {
                    ["resourceType"] = "Drug",
                    ["resourceId"] = drugId.ToString(),
                });
        }

        if (drug.TypeOfDrug != DrugType.Standard ||
            drug.TypeOfProduct.Equals("Weterynaryjny", StringComparison.OrdinalIgnoreCase))
        {
            return Error.Validation(
                description: "Selected drug is not eligible.",
                metadata: new()
                {
                    ["field"] = "drugId",
                    ["reason"] = "eligibility",
                });
        }

        bool hasCatalogDuplicate = await _drugProblemRepository.ExistsForUserWithDrugIdAsync(
            command.UserId,
            drugId,
            excludeId: command.Id,
            cancellationToken);
        if (hasCatalogDuplicate)
        {
            return Error.Conflict(
                description: "Drug problem already exists for this drug.",
                metadata: new()
                {
                    ["field"] = "drugId",
                    ["conflictingResourceId"] = drugId.ToString(),
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

        ErrorOr<Success> updateResult = entity.UpdateDatabaseDrug(drugId);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        entity.AttachDrug(drug);

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

        return entity.UpdateCustomDrugName(customName);
    }

    private static string NormalizeForLookup(string value)
    {
        string trimmed = value.Trim();
        return string.Join(' ', trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
