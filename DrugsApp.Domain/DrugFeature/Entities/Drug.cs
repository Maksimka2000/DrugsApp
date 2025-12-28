using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.DrugFeature.Entities;

/// <summary>
/// Represents an approved drug aggregate composed of active substances and drug forms.
/// </summary>
public sealed class Drug : BaseEntity
{
    

    private Drug() : base()
    {
    }


    // Navigation properties

    private readonly List<ActiveSubstance> _activeSubstances = new();
    /// <summary>
    /// Active substances defining the composition (minimum one).
    /// </summary>
    public IReadOnlyCollection<ActiveSubstance> ActiveSubstances => _activeSubstances;

    private readonly List<DrugForm> _drugForms = new();
    /// <summary>
    /// Drug forms owned by the aggregate.
    /// </summary>
    public IReadOnlyCollection<DrugForm> DrugForms => _drugForms;





    /// <summary>
    /// Marketed name displayed to administrators.
    /// </summary>
    public string CommercialName { get; private set; } = string.Empty;

    /// <summary>
    /// Product classification string coming from regulatory data.
    /// </summary>
    public string TypeOfProduct { get; private set; } = string.Empty;

    /// <summary>
    /// Drug type flag used for filtering and reporting.
    /// </summary>
    public DrugType TypeOfDrug { get; private set; }

    /// <summary>
    /// Cached count of drug forms owned by this aggregate.
    /// </summary>
    public int DrugFormCounter { get; private set; }

    //

    /// <summary>
    /// Creates a new drug aggregate with optional seed collections.
    /// </summary>
    /// <param name="commercialName">Commercial name for UI display.</param>
    /// <param name="typeOfProduct">Product classification string.</param>
    /// <param name="typeOfDrug">Drug type value.</param>
    /// <param name="activeSubstances">Optional initial active substances set.</param>
    /// <param name="drugForms">Optional initial drug forms set.</param>
    /// <returns>A new <see cref="Drug"/> instance.</returns>
    public static Drug Create(
        string commercialName,
        string typeOfProduct,
        DrugType typeOfDrug,
        IEnumerable<ActiveSubstance>? activeSubstances,
        IEnumerable<DrugForm>? drugForms)
    {
        var drug = new Drug
        {
            CommercialName = commercialName.Trim(),
            TypeOfProduct = typeOfProduct.Trim(),
            TypeOfDrug = typeOfDrug
        };


        if (activeSubstances != null)
        {
            foreach (ActiveSubstance activeSubstance in activeSubstances)
            {
                drug.AddActiveSubstance(activeSubstance);
            }
        }


        if (drugForms != null)
        {
            foreach (DrugForm drugForm in drugForms)
            {
                drug.AddDrugForm(drugForm);
            }
        }


        return drug;
    }

    /// <summary>
    /// Updates the commercial name after trimming input.
    /// </summary>
    /// <param name="commercialName">New commercial name.</param>
    public void UpdateCommercialName(string commercialName)
    {
        string trimmedName = commercialName.Trim();

        if (CommercialName == trimmedName)
        {
            return;
        }

        CommercialName = trimmedName;
    }

    /// <summary>
    /// Updates the drug type when it differs from the current value.
    /// </summary>
    /// <param name="typeOfDrug">New drug type value.</param>
    public void UpdateTypeOfDrug(DrugType typeOfDrug)
    {
        if (TypeOfDrug == typeOfDrug)
        {
            return;
        }

        TypeOfDrug = typeOfDrug;
    }

    /// <summary>
    /// Adds a new active substance to the composition if it is not already linked.
    /// </summary>
    /// <param name="activeSubstance">Active substance to link.</param>
    public void AddActiveSubstance(ActiveSubstance activeSubstance)
    {
        if (_activeSubstances.Any(substance => substance.Id == activeSubstance.Id))
        {
            return;
        }

        _activeSubstances.Add(activeSubstance);
    }

    /// <summary>
    /// Removes an active substance ensuring at least one item remains in the collection.
    /// </summary>
    /// <param name="activeSubstance">Active substance to unlink.</param>
    public void RemoveActiveSubstance(ActiveSubstance activeSubstance)
    {
        ActiveSubstance? existing = _activeSubstances.FirstOrDefault(substance => substance.Id == activeSubstance.Id);
        if (existing is null || _activeSubstances.Count == 1)
        {
            return;
        }

        _activeSubstances.Remove(existing);
    }

    /// <summary>
    /// Registers a new drug form owned by this aggregate and refreshes the counter.
    /// </summary>
    /// <param name="drugForm">Drug form to link.</param>
    public void AddDrugForm(DrugForm drugForm)
    {
        if (drugForm.DrugId != Id)
        {
            return;
        }

        if (_drugForms.Any(form => form.Id == drugForm.Id))
        {
            return;
        }

        _drugForms.Add(drugForm);
        DrugFormCounter = _drugForms.Count;
    }

    /// <summary>
    /// Removes a drug form from the aggregate and refreshes the counter.
    /// </summary>
    /// <param name="drugForm">Drug form to unlink.</param>
    public void RemoveDrugForm(DrugForm drugForm)
    {
        DrugForm? existing = _drugForms.FirstOrDefault(form => form.Id == drugForm.Id);
        if (existing is null || _drugForms.Count == 1)
        {
            return;
        }

        _drugForms.Remove(existing);
        DrugFormCounter = _drugForms.Count;
    }
}

