using System.Collections.Generic;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Domain.PharmaceuticalFormFeature.Entities;

/// <summary>
/// Represents a pharmaceutical form definition used to classify drugs.
/// </summary>
public sealed class PharmaceuticalForm : BaseEntity
{
    

    /// <summary>
    /// 
    /// </summary>
    private PharmaceuticalForm() : base()
    {
    }


    // Navigation properties
    
    private readonly List<DrugForm> _drugForms = new();
    /// <summary>
    /// Drug forms referencing this pharmaceutical form tag.
    /// </summary>
    public IReadOnlyCollection<DrugForm> DrugForms => _drugForms;



    // Properties

    /// <summary>
    /// The formulation name of the pharmaceutical form.
    /// </summary>
    public string FormulationName { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a new pharmaceutical form aggregate.
    /// </summary>
    /// <param name="formulationName">Pharmaceutical form name.</param>
    /// <returns>A new <see cref="PharmaceuticalForm"/> instance.</returns>
    public static PharmaceuticalForm Create(string formulationName) => new()
    {
        FormulationName = formulationName.Trim()
    };

    /// <summary>
    /// Updates the formulation name of the pharmaceutical form.
    /// </summary>
    /// <param name="formulationName">New pharmaceutical form name.</param>
    public void UpdateFormulationName(string formulationName)
    {
        string trimmedName = formulationName.Trim();
        FormulationName = trimmedName;
    }
}


