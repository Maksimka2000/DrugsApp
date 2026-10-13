using DrugsApp.Domain.DrugFeature.Entities;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Drug form a user takes or plans to take, projected for overlap matching [description].
/// </summary>
/// <param name="Drug">Catalog drug with its active substances loaded.</param>
/// <param name="Form">Selected drug form of the catalog drug.</param>
internal readonly record struct DrugFormIntake(Drug Drug, DrugForm Form);
