using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.DrugProblemsFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Repositories;
using DrugsApp.Domain.RegularDrugsFeature.Shared;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;

/// <summary>
/// Handles creation of drug problems.
/// </summary>
internal sealed class CreateDrugProblemHandler(
    IDrugProblemRepository _drugProblemRepository,
    IDrugFormRepository _drugFormRepository)
    : IRequestHandler<CreateDrugProblemCommand, ErrorOr<CreateDrugProblemResponse>>
{
    public async Task<ErrorOr<CreateDrugProblemResponse>> Handle(
        CreateDrugProblemCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Request.DrugFormId is not null)
        {
            return await CreateDatabaseAsync(command, cancellationToken);
        }

        return await CreateCustomAsync(command, cancellationToken);
    }

    private async Task<ErrorOr<CreateDrugProblemResponse>> CreateDatabaseAsync(
        CreateDrugProblemCommand command,
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
            excludeId: null,
            cancellationToken);
        if (hasCatalogDuplicate)
        {
            return Error.Conflict(
                description: "Drug problem already exists for this drug form.",
                metadata: new()
                {
                    ["conflictingResourceId"] = drugFormId.ToString(),
                    ["field"] = "drugFormId",
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
            drugFormId,
            command.Request.ProblemNote);
        if (created.IsError)
        {
            return created.Errors;
        }

        created.Value.AttachDrugForm(drugForm);

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
            command.Request.CustomActiveSubstances ?? string.Empty,
            command.Request.CustomFormulationName ?? string.Empty,
            command.Request.CustomDosage ?? string.Empty,
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
