using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AllergiesFeature.Entities;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.AllergiesFeature.Repositories;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Features.AllergiesFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.AllergiesFeature.CreateDrugAllergy;

/// <summary>
/// Handles creation of drug allergies.
/// </summary>
internal sealed class CreateDrugAllergyHandler(
    IClientDrugAllergyRepository _drugAllergyRepository,
    IDrugRepository _drugRepository)
    : IRequestHandler<CreateDrugAllergyCommand, ErrorOr<CreatedDrugAllergyResponse>>
{
    public async Task<ErrorOr<CreatedDrugAllergyResponse>> Handle(
        CreateDrugAllergyCommand command,
        CancellationToken cancellationToken)
    {
        AllergyRecordSource? source = AllergySharedMappings.MapSourceFromToken(command.Request.Source);
        if (source is null)
        {
            return Error.Validation(
                description: "Invalid source value.",
                metadata: new()
                {
                    ["field"] = "source",
                });
        }

        return source.Value switch
        {
            AllergyRecordSource.Database => await CreateDatabaseAsync(command, cancellationToken),
            AllergyRecordSource.Custom => await CreateCustomAsync(command, cancellationToken),
            _ => Error.Validation(description: "Invalid source value."),
        };
    }

    private async Task<ErrorOr<CreatedDrugAllergyResponse>> CreateDatabaseAsync(
        CreateDrugAllergyCommand command,
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

        bool existsByDrugId = await _drugAllergyRepository.ExistsByDrugIdAsync(
            command.UserId,
            drugId,
            cancellationToken);
        if (existsByDrugId)
        {
            return Error.Conflict(
                description: "Drug allergy already exists for this drug.",
                metadata: new()
                {
                    ["conflictingResourceId"] = drugId.ToString(),
                    ["field"] = "drugId",
                });
        }

        string normalizedDisplayName = AllergySharedMappings.NormalizeForLookup(drug.CommercialName);

        bool existsCustomCollision = await _drugAllergyRepository.ExistsCustomNameNormalizedAsync(
            command.UserId,
            normalizedDisplayName,
            cancellationToken);
        if (existsCustomCollision)
        {
            return Error.Conflict(
                description: "A custom drug allergy with the same name already exists.",
                metadata: new()
                {
                    ["field"] = "customDrugName",
                    ["conflictingName"] = drug.CommercialName,
                });
        }

        bool hasActiveSubstanceOverlap = await _drugAllergyRepository.HasActiveSubstanceOverlapAsync(
            command.UserId,
            drugId,
            cancellationToken);
        if (hasActiveSubstanceOverlap)
        {
            return Error.Conflict(
                description: "An allergy to a drug with overlapping active substances already exists.",
                metadata: new()
                {
                    ["field"] = "drugId",
                    ["reason"] = "activeSubstanceOverlap",
                });
        }

        ErrorOr<ClientDrugAllergy> created = ClientDrugAllergy.CreateDatabase(
            command.UserId,
            drugId,
            command.Request.Note);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _drugAllergyRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDto(drug).MapToResponse();
    }

    private async Task<ErrorOr<CreatedDrugAllergyResponse>> CreateCustomAsync(
        CreateDrugAllergyCommand command,
        CancellationToken cancellationToken)
    {
        string customName = command.Request.CustomDrugName!;
        string normalizedCustomName = AllergySharedMappings.NormalizeForLookup(customName);

        bool existsCustomDuplicate = await _drugAllergyRepository.ExistsCustomNameNormalizedAsync(
            command.UserId,
            normalizedCustomName,
            cancellationToken);
        if (existsCustomDuplicate)
        {
            return Error.Conflict(
                description: "Custom drug allergy with this name already exists.",
                metadata: new()
                {
                    ["field"] = "customDrugName",
                    ["conflictingName"] = customName,
                });
        }

        bool existsCatalogCollision = await _drugAllergyRepository.ExistsDatabaseDisplayNameNormalizedAsync(
            command.UserId,
            normalizedCustomName,
            cancellationToken);
        if (existsCatalogCollision)
        {
            return Error.Conflict(
                description: "A catalog-backed drug allergy with the same name already exists.",
                metadata: new()
                {
                    ["field"] = "customDrugName",
                    ["conflictingName"] = customName,
                });
        }

        ErrorOr<ClientDrugAllergy> created = ClientDrugAllergy.CreateCustom(
            command.UserId,
            customName,
            command.Request.Note);
        if (created.IsError)
        {
            return created.Errors;
        }

        await _drugAllergyRepository.AddAsync(created.Value, cancellationToken);

        return created.Value.MapToDto().MapToResponse();
    }
}
