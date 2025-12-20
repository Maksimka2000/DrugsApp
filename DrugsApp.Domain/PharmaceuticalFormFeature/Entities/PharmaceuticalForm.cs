using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.PharmaceuticalFormFeature.Entities;

/// <summary>
/// Represents a pharmaceutical form definition used to classify drugs.
/// </summary>
public sealed class PharmaceuticalForm : BaseEntity
{
    private PharmaceuticalForm() : base()
    {
    }

    /// <summary>
    /// The formulation name of the pharmaceutical form.
    /// </summary>
    public string FormulationName { get; private set; }

    /// <summary>
    /// Creates a new pharmaceutical form aggregate.
    /// </summary>
    /// <param name="formulationName">Pharmaceutical form name.</param>
    /// <returns>A new <see cref="PharmaceuticalForm"/> instance.</returns>
    public static PharmaceuticalForm Create(string formulationName)
    {
        return new PharmaceuticalForm
        {
            FormulationName = formulationName.Trim()
        };
    }

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


