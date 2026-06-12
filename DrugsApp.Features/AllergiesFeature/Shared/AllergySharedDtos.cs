using System;
using System.Collections.Generic;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;

namespace DrugsApp.Features.AllergiesFeature.Shared;

/// <summary>
/// Allergy summary response for UI binding.
/// </summary>
public sealed record AllergySummaryResponse(
    Guid Id,
    string? SevereReactionStatus,
    bool? CarriesAllergyMedication,
    string? PollenDustMitesAllergyStatus,
    string? InsectVenomAllergyStatus,
    string? LatexAllergyStatus,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Compact drug form summary for catalog-backed drug allergies.
/// </summary>
public sealed record ClientDrugAllergyDrugFormSummaryDto(
    Guid DrugFormId,
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    IReadOnlyCollection<LookupDto> ActiveSubstances,
    string FormulationName,
    string? Dosage);

/// <summary>
/// Drug allergy list item DTO.
/// </summary>
public sealed record ClientDrugAllergyDto(
    Guid Id,
    string Source,
    Guid? DrugFormId,
    string? CustomDrugName,
    string? CustomActiveSubstances,
    string? CustomFormulationName,
    string? CustomDosage,
    string DisplayName,
    string? Note,
    IReadOnlyCollection<LookupDto> ActiveSubstances,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    ClientDrugAllergyDrugFormSummaryDto? DrugFormSummary);

/// <summary>
/// Supplement allergy list item DTO.
/// </summary>
public sealed record ClientSupplementAllergyDto(
    Guid Id,
    string Source,
    Guid? SupplementId,
    string? CustomSupplementName,
    string DisplayName,
    string? Note,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Food allergy list item DTO.
/// </summary>
public sealed record ClientFoodAllergyDto(
    Guid Id,
    string Source,
    Guid? FoodId,
    string? CustomFoodName,
    string DisplayName,
    string? Note,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Other allergy list item DTO.
/// </summary>
public sealed record ClientOtherAllergyDto(
    Guid Id,
    string AllergyName,
    string? Note,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

/// <summary>
/// Name availability response envelope.
/// </summary>
public sealed record NameAvailabilityResponse(bool IsAvailable);

/// <summary>
/// Paged drug allergies response.
/// </summary>
public sealed record ListDrugAllergiesResponse(
    IReadOnlyCollection<ClientDrugAllergyDto> Items,
    int TotalCount,
    bool HasMore);

/// <summary>
/// Paged supplement allergies response.
/// </summary>
public sealed record ListSupplementAllergiesResponse(
    IReadOnlyCollection<ClientSupplementAllergyDto> Items,
    int TotalCount,
    bool HasMore);

/// <summary>
/// Paged food allergies response.
/// </summary>
public sealed record ListFoodAllergiesResponse(
    IReadOnlyCollection<ClientFoodAllergyDto> Items,
    int TotalCount,
    bool HasMore);

/// <summary>
/// Paged other allergies response.
/// </summary>
public sealed record ListOtherAllergiesResponse(
    IReadOnlyCollection<ClientOtherAllergyDto> Items,
    int TotalCount,
    bool HasMore);

/// <summary>
/// Shared PATCH request for allergy note updates.
/// </summary>
public sealed record UpdateAllergyNoteRequest(string? Note, bool? ClearNote);

/// <summary>
/// Created drug allergy response wrapper.
/// </summary>
public sealed record CreatedDrugAllergyResponse(ClientDrugAllergyDto Item);

/// <summary>
/// Updated drug allergy response wrapper.
/// </summary>
public sealed record UpdatedDrugAllergyResponse(ClientDrugAllergyDto Item);

/// <summary>
/// Created supplement allergy response wrapper.
/// </summary>
public sealed record CreatedSupplementAllergyResponse(ClientSupplementAllergyDto Item);

/// <summary>
/// Updated supplement allergy response wrapper.
/// </summary>
public sealed record UpdatedSupplementAllergyResponse(ClientSupplementAllergyDto Item);

/// <summary>
/// Created food allergy response wrapper.
/// </summary>
public sealed record CreatedFoodAllergyResponse(ClientFoodAllergyDto Item);

/// <summary>
/// Updated food allergy response wrapper.
/// </summary>
public sealed record UpdatedFoodAllergyResponse(ClientFoodAllergyDto Item);

/// <summary>
/// Created other allergy response wrapper.
/// </summary>
public sealed record CreatedOtherAllergyResponse(ClientOtherAllergyDto Item);

/// <summary>
/// Updated other allergy response wrapper.
/// </summary>
public sealed record UpdatedOtherAllergyResponse(ClientOtherAllergyDto Item);
