using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.TempDrugFeature.Entities;

/// <summary>
/// Represents an imported temporary drug record pending administrative review.
/// </summary>
public sealed class TempDrug : BaseEntity
{
    private TempDrug() : base()
    {
    }

    /// <summary>
    /// Unique ingest identifier for the imported record.
    /// </summary>
    public string ProductIdentifier { get; private set; } = null!;

    /// <summary>
    /// Commercial name stored as imported (trimmed at ends only).
    /// </summary>
    public string? CommercialName { get; private set; }

    /// <summary>
    /// Active substance text stored as imported (trimmed at ends only).
    /// </summary>
    public string? ActiveSubstance { get; private set; }

    /// <summary>
    /// Type of product text (expected values: "Ludzki", "Weterynaryjny").
    /// </summary>
    public string? TypeOfProduct { get; private set; }

    /// <summary>
    /// Route of administration text stored as imported.
    /// </summary>
    public string? RouteOfAdministration { get; private set; }

    /// <summary>
    /// Pharmaceutical formulation text stored as imported.
    /// </summary>
    public string? PharmaceuticalFormulation { get; private set; }

    /// <summary>
    /// ATC code stored as imported.
    /// </summary>
    public string? ATCCode { get; private set; }

    /// <summary>
    /// Leaflet reference (often an URL) stored as imported.
    /// </summary>
    public string? Leaflet { get; private set; }

    /// <summary>
    /// Educational materials reference (often an URL) stored as imported.
    /// </summary>
    public string? EducationalMaterials { get; private set; }

    /// <summary>
    /// Dosage text stored as imported.
    /// </summary>
    public string? Dosage { get; private set; }

    /// <summary>
    /// Optional administrator note explaining how the record was reviewed or resolved.
    /// </summary>
    public string? Note { get; private set; }

    /// <summary>
    /// Linked canonical drug form identifier (XOR with <see cref="LinkedDrugVetId"/>).
    /// </summary>
    public Guid? LinkedDrugFormId { get; private set; }

    /// <summary>
    /// Linked veterinary drug identifier (XOR with <see cref="LinkedDrugFormId"/>).
    /// </summary>
    public Guid? LinkedDrugVetId { get; private set; }

    /// <summary>
    /// Identifier of the import batch that produced this record.
    /// </summary>
    public Guid LinkedImportId { get; private set; }

    /// <summary>
    /// Current review state.
    /// </summary>
    public TempDrugStatus Status { get; private set; }

    /// <summary>
    /// Creates a new temporary drug instance from imported raw values.
    /// </summary>
    /// <param name="productIdentifier">Unique ingest identifier.</param>
    /// <param name="commercialName">Optional commercial name.</param>
    /// <param name="activeSubstance">Optional active substance text.</param>
    /// <param name="typeOfProduct">Optional type of product text.</param>
    /// <param name="routeOfAdministration">Optional route of administration text.</param>
    /// <param name="pharmaceuticalFormulation">Optional pharmaceutical formulation text.</param>
    /// <param name="atcCode">Optional ATC code.</param>
    /// <param name="leaflet">Optional leaflet reference.</param>
    /// <param name="educationalMaterials">Optional educational materials reference.</param>
    /// <param name="dosage">Optional dosage text.</param>
    /// <param name="linkedImportId">Required import batch id.</param>
    /// <returns>Created <see cref="TempDrug"/> instance or validation error.</returns>
    public static ErrorOr<TempDrug> CreateImported(
        string productIdentifier,
        string? commercialName,
        string? activeSubstance,
        string? typeOfProduct,
        string? routeOfAdministration,
        string? pharmaceuticalFormulation,
        string? atcCode,
        string? leaflet,
        string? educationalMaterials,
        string? dosage,
        Guid linkedImportId)
    {
        if (string.IsNullOrWhiteSpace(productIdentifier))
        {
            return Error.Validation(description: "ProductIdentifier is required.");
        }

        if (linkedImportId == Guid.Empty)
        {
            return Error.Validation(
                description: "LinkedImportId must not be empty.",
                metadata: new()
                {
                    ["error"] = "linkedImportId"
                });
        }

       

        return new TempDrug
        {
            ProductIdentifier = productIdentifier.Trim(),
            CommercialName = TrimAtEndsOrNull(commercialName),
            ActiveSubstance = TrimAtEndsOrNull(activeSubstance),
            TypeOfProduct = TrimAtEndsOrNull(typeOfProduct),
            RouteOfAdministration = TrimAtEndsOrNull(routeOfAdministration),
            PharmaceuticalFormulation = TrimAtEndsOrNull(pharmaceuticalFormulation),
            ATCCode = TrimAtEndsOrNull(atcCode),
            Leaflet = TrimAtEndsOrNull(leaflet),
            EducationalMaterials = TrimAtEndsOrNull(educationalMaterials),
            Dosage = TrimAtEndsOrNull(dosage),
            LinkedImportId = linkedImportId,
            Status = TempDrugStatus.PendingReview
        };
    }

    /// <summary>
    /// Changes the review status for this record.
    /// </summary>
    /// <param name="newStatus">New status value.</param>
    /// <returns>Success or validation error.</returns>
    public ErrorOr<Success> ChangeStatus(TempDrugStatus newStatus)
    {
        if (Status == newStatus)
        {
            return Result.Success;
        }

        Status = newStatus;
        return Result.Success;
    }

    /// <summary>
    /// Links this record to a canonical drug form and clears veterinary linkage to enforce XOR semantics.
    /// </summary>
    /// <param name="drugFormId">Drug form identifier.</param>
    /// <returns>Success or validation error.</returns>
    public ErrorOr<Success> LinkToDrugForm(Guid drugFormId)
    {

        if(LinkedDrugVetId is not null)
        {
            return Error.Validation(description: "LinkedDrugVetId must be null when linking to a drug form.");
        }

        if (drugFormId == Guid.Empty)
        {
            return Error.Validation(description: "DrugFormId must not be empty.");
        }

        if (LinkedDrugFormId == drugFormId)
        {
            return Result.Success;
        }

        LinkedDrugFormId = drugFormId;
        return Result.Success;
    }

    /// <summary>
    /// Links this record to a veterinary drug and clears drug-form linkage to enforce XOR semantics.
    /// </summary>
    /// <param name="vetDrugId">Veterinary drug identifier.</param>
    /// <returns>Success or validation error.</returns>
    public ErrorOr<Success> LinkToVetDrug(Guid vetDrugId)
    {

        if(LinkedDrugFormId is not null)
        {
            return Error.Validation(description: "LinkedDrugFormId must be null when linking to a veterinary drug.");
        }

        if (vetDrugId == Guid.Empty)
        {
            return Error.Validation(description: "VetDrugId must not be empty.");
        }

        if (LinkedDrugVetId == vetDrugId)
        {
            return Result.Success;
        }

        LinkedDrugVetId = vetDrugId;
        return Result.Success;
    }

    /// <summary>
    /// Sets or clears the administrator note for this record.
    /// </summary>
    /// <param name="note">Note text; null/whitespace clears the note.</param>
    /// <returns>Success or validation error.</returns>
    public ErrorOr<Success> SetNote(string? note)
    {
        string? normalized = note?.Trim();

        if (Note == normalized)
        {
            return Result.Success;
        }

        Note = normalized;
        return Result.Success;
    }



    private static string? TrimAtEndsOrNull(string? value)
    {
        if (value is null)
        {
            return null;
        }

        return value.Trim();
    }
}


