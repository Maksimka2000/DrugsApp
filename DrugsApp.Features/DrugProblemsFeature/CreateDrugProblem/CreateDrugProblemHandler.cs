using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;

/// <summary>
/// Handles creation of drug problems.
/// </summary>
internal sealed class CreateDrugProblemHandler(
    IDrugProblemRepository _drugProblemRepository,
    IDrugRepository _drugRepository)
    : IRequestHandler<CreateDrugProblemCommand, ErrorOr<CreateDrugProblemResponse>>
{
    public async Task<ErrorOr<CreateDrugProblemResponse>> Handle(
        CreateDrugProblemCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Request.DrugId is not null)
        {
            return await CreateDatabaseAsync(command, cancellationToken);
        }

        return await CreateCustomAsync(command, cancellationToken);
    }

    private async Task<ErrorOr<CreateDrugProblemResponse>> CreateDatabaseAsync(
        CreateDrugProblemCommand command,
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
            excludeId: null,
            cancellationToken);
        if (hasCatalogDuplicate)
        {
            return Error.Conflict(
                description: "Drug problem already exists for this drug.",
                metadata: new()
                {
                    ["conflictingResourceId"] = drugId.ToString(),
                    ["field"] = "drugId",
                });
        }

        string normalizedDrugDisplayName = NormalizeForLookup(drug.CommercialName);

        bool hasCustomCollision = await _drugProblemRepository.ExistsForUserWithCustomNameAsync(
            command.UserId,
            normalizedCustomName: normalizedDrugDisplayName,
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

        ErrorOr<DrugProblem> created = DrugProblem.CreateDatabase(
            command.UserId,
            drugId,
            command.Request.ProblemNote);
        if (created.IsError)
        {
            return created.Errors;
        }

        created.Value.AttachDrug(drug);

        await _drugProblemRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDetailsDto().MapToResponse();
    }

    private async Task<ErrorOr<CreateDrugProblemResponse>> CreateCustomAsync(
        CreateDrugProblemCommand command,
        CancellationToken cancellationToken)
    {
        string customName = command.Request.CustomDrugName ?? string.Empty;

        bool hasCustomDuplicate = await _drugProblemRepository.ExistsForUserWithCustomNameAsync(
            command.UserId,
            normalizedCustomName: NormalizeForLookup(customName),
            excludeId: null,
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
            normalizedDrugDisplayName: NormalizeForLookup(customName),
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

        ErrorOr<DrugProblem> created = DrugProblem.CreateCustom(
            command.UserId,
            customName,
            command.Request.ProblemNote);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _drugProblemRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDetailsDto().MapToResponse();
    }

    private static string NormalizeForLookup(string value)
    {
        string trimmed = value.Trim();
        return string.Join(' ', trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
