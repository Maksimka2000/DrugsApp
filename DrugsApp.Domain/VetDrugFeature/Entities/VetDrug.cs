using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.VetDrugFeature.Entities;

/// <summary>
/// Represents an approved veterinary drug in the system.
/// </summary>
public sealed class VetDrug : BaseEntity
{
    private VetDrug() : base()
    {
    }

    /// <summary>
    /// Numeric identifier for the veterinary product as specified by regulatory authorities.
    /// </summary>
    public int ProductIdentifier { get; private set; }

    /// <summary>
    /// The marketed name of the drug visible to end users.
    /// </summary>
    public string CommercialName { get; private set; } = string.Empty;

    /// <summary>
    /// Classification describing the product category.
    /// </summary>
    public string TypeOfProduct { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a new VetDrug aggregate with required fields.
    /// </summary>
    /// <param name="productIdentifier">Regulatory product identifier (must be positive).</param>
    /// <param name="commercialName">Marketed drug name (cannot be empty).</param>
    /// <param name="typeOfProduct">Product category classification (cannot be empty).</param>
    /// <returns>A new VetDrug instance.</returns>
    public static VetDrug Create(
        int productIdentifier,
        string commercialName,
        string typeOfProduct) => new()
        {
            ProductIdentifier = productIdentifier,
            CommercialName = commercialName.Trim(),
            TypeOfProduct = typeOfProduct.Trim()
        };

    /// <summary>
    /// Updates the commercial name of the drug.
    /// </summary>
    /// <param name="commercialName">New commercial name (cannot be empty).</param>
    public void UpdateCommercialName(string commercialName)
    {
        string trimmedName = commercialName.Trim();

        if (CommercialName == trimmedName)
        {
            return;
        }

        CommercialName = trimmedName;
    }
}

