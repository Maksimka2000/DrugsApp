using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;

namespace DrugsApp.Domain.DrugFeature.Entities;

/// <summary>
/// Represents a concrete composition of a drug including dosage and administration data.
/// </summary>
public sealed class DrugForm : BaseEntity
{
    

    private DrugForm() : base()
    {
    }

    // Navigation properties

    private readonly List<RouteOfAdministration> _routesOfAdministration = new();
    /// <summary>
    /// Routes of administration associated with the form.
    /// </summary>
    public IReadOnlyCollection<RouteOfAdministration> RoutesOfAdministration => _routesOfAdministration;

    private readonly List<PharmaceuticalForm> _pharmaceuticalForms = new();
    /// <summary>
    /// Pharmaceutical formulation tags associated with the form.
    /// </summary>
    public IReadOnlyCollection<PharmaceuticalForm> PharmaceuticalForms => _pharmaceuticalForms;

    /// <summary>
    /// Identifier of the owning drug aggregate.
    /// </summary>
    public Guid DrugId { get; private set; }

    /// <summary>
    /// Navigation to the owning drug (set by EF Core).
    /// </summary>
    public Drug Drug { get; private set; } = null!;







    /// <summary>
    /// Regulatory product identifier for this form.
    /// </summary>
    public int ProductIdentifier { get; private set; }

    /// <summary>
    /// Optional ATC code describing the form.
    /// </summary>
    public string? AtcCode { get; private set; }

    /// <summary>
    /// Optional dosage instructions.
    /// </summary>
    public string? Dosage { get; private set; } = string.Empty;

    /// <summary>
    /// Optional leaflet reference (URL).
    /// </summary>
    public string? Leaflet { get; private set; } = string.Empty;

    /// <summary>
    /// Optional educational materials reference (URL).
    /// </summary>
    public string? EducationMaterials { get; private set; } = string.Empty;

    /// <summary>
    /// Flag derived from routes of administration describing if the form is oral.
    /// </summary>
    public bool IsOral { get; private set; } = false;



    /// <summary>
    /// Creates a new drug form instance for the given drug.
    /// </summary>
    /// <param name="drugId">Owning drug identifier.</param>
    /// <param name="productIdentifier">Product identifier value.</param>
    /// <param name="atcCode">Optional ATC code.</param>
    /// <param name="dosage">Optional dosage text.</param>
    /// <param name="leaflet">Optional leaflet reference.</param>
    /// <param name="educationMaterials">Optional education materials reference.</param>
    /// <returns>A new <see cref="DrugForm"/> instance.</returns>
    public static DrugForm Create(
        Guid drugId,
        int productIdentifier,
        string? atcCode = null,
        string? dosage = null,
        string? leaflet = null,
        string? educationMaterials = null)
    {
        var form = new DrugForm
        {
            DrugId = drugId,
            ProductIdentifier = productIdentifier
        };

        form.SetAtcCode(atcCode);
        form.SetDosage(dosage);
        form.SetLeaflet(leaflet);
        form.SetEducationMaterials(educationMaterials);

        return form;
    }

    /// <summary>
    /// Updates the product identifier when a positive value is provided.
    /// </summary>
    /// <param name="productIdentifier">New product identifier value.</param>
    public void SetProductIdentifier(int productIdentifier)
    {
        if (productIdentifier <= 0)
        {
            return;
        }

        if (ProductIdentifier == productIdentifier)
        {
            return;
        }

        ProductIdentifier = productIdentifier;
    }

    /// <summary>
    /// Sets the ATC code when a non-empty value is provided.
    /// </summary>
    /// <param name="atcCode">ATC code value.</param>
    public void SetAtcCode(string? atcCode)
    {
        if (string.IsNullOrWhiteSpace(atcCode))
        {
            return;
        }

        string trimmedValue = atcCode.Trim();
        if (AtcCode == trimmedValue)
        {
            return;
        }

        AtcCode = trimmedValue;
    }

    /// <summary>
    /// Clears the ATC code.
    /// </summary>
    public void ClearAtcCode()
    {
        AtcCode = null;
    }

    /// <summary>
    /// Sets the dosage when a non-empty value is provided.
    /// </summary>
    /// <param name="dosage">Dosage text.</param>
    public void SetDosage(string? dosage)
    {
        if (string.IsNullOrWhiteSpace(dosage))
        {
            return;
        }

        string trimmedValue = dosage.Trim();
        if (Dosage == trimmedValue)
        {
            return;
        }

        Dosage = trimmedValue;
    }

    /// <summary>
    /// Clears the dosage value.
    /// </summary>
    public void ClearDosage()
    {
        Dosage = null;
    }

    /// <summary>
    /// Sets the leaflet reference when provided.
    /// </summary>
    /// <param name="leaflet">Leaflet reference.</param>
    public void SetLeaflet(string? leaflet)
    {
        if (string.IsNullOrWhiteSpace(leaflet))
        {
            return;
        }

        string trimmedValue = leaflet.Trim();
        if (Leaflet == trimmedValue)
        {
            return;
        }

        Leaflet = trimmedValue;
    }

    /// <summary>
    /// Clears the leaflet reference.
    /// </summary>
    public void ClearLeaflet()
    {
        Leaflet = null;
    }

    /// <summary>
    /// Sets the education materials reference when provided.
    /// </summary>
    /// <param name="educationMaterials">Education materials reference.</param>
    public void SetEducationMaterials(string? educationMaterials)
    {
        if (string.IsNullOrWhiteSpace(educationMaterials))
        {
            return;
        }

        string trimmedValue = educationMaterials.Trim();
        if (EducationMaterials == trimmedValue)
        {
            return;
        }

        EducationMaterials = trimmedValue;
    }

    /// <summary>
    /// Clears the education materials reference.
    /// </summary>
    public void ClearEducationMaterials()
    {
        EducationMaterials = null;
    }

    /// <summary>
    /// Adds a route of administration when it is not already assigned.
    /// </summary>
    /// <param name="routeOfAdministration">Route to attach.</param>
    public void AddRouteOfAdministration(RouteOfAdministration routeOfAdministration)
    {
        if (_routesOfAdministration.Any(route => route.Id == routeOfAdministration.Id))
        {
            return;
        }

        _routesOfAdministration.Add(routeOfAdministration);
        RecalculateIsOral();
    }

    /// <summary>
    /// Removes a route of administration if it is currently assigned.
    /// </summary>
    /// <param name="routeOfAdministration">Route to remove.</param>
    public void RemoveRouteOfAdministration(RouteOfAdministration routeOfAdministration)
    {
        RouteOfAdministration? existing = _routesOfAdministration.FirstOrDefault(route => route.Id == routeOfAdministration.Id);
        if (existing is null)
        {
            return;
        }

        _routesOfAdministration.Remove(existing);
        RecalculateIsOral();
    }

    /// <summary>
    /// Adds a pharmaceutical form tag when not already present.
    /// </summary>
    /// <param name="pharmaceuticalForm">Form to attach.</param>
    public void AddPharmaceuticalForm(PharmaceuticalForm pharmaceuticalForm)
    {
        if (_pharmaceuticalForms.Any(form => form.Id == pharmaceuticalForm.Id))
        {
            return;
        }

        _pharmaceuticalForms.Add(pharmaceuticalForm);
    }

    /// <summary>
    /// Removes an existing pharmaceutical form tag.
    /// </summary>
    /// <param name="pharmaceuticalForm">Form to remove.</param>
    public void RemovePharmaceuticalForm(PharmaceuticalForm pharmaceuticalForm)
    {
        PharmaceuticalForm? existing = _pharmaceuticalForms.FirstOrDefault(form => form.Id == pharmaceuticalForm.Id);
        if (existing is null)
        {
            return;
        }

        _pharmaceuticalForms.Remove(existing);
    }

    /// <summary>
    /// Recomputes the <see cref="IsOral"/> flag using the current routes.
    /// </summary>
    public void RecalculateIsOral()
    {
        IsOral = _routesOfAdministration.Any(route => route.IsOralIntake);
    }
}

