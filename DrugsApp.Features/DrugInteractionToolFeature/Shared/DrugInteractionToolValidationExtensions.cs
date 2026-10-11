using FluentValidation;

namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Shared FluentValidation rules for the id lists accepted by the drug interaction tool endpoints [description].
/// </summary>
public static class DrugInteractionToolValidationExtensions
{
  /// <summary>
  /// Adds the standard id list rules: at most <paramref name="maxCount"/> items, no empty Guid, unique values.
  /// </summary>
  /// <typeparam name="T">Validated request type.</typeparam>
  /// <param name="validator">Validator receiving the rules.</param>
  /// <param name="selector">Reads the id list from the request.</param>
  /// <param name="propertyName">Property name used in error messages.</param>
  /// <param name="maxCount">Maximum number of ids.</param>
  public static void AddIdSelectionRules<T>(
    this AbstractValidator<T> validator,
    Func<T, IReadOnlyCollection<Guid>?> selector,
    string propertyName,
    int maxCount)
  {
    validator.When(
      request => selector(request) is { Count: > 0 },
      () =>
      {
        validator.RuleFor(request => selector(request)!)
          .Must(ids => ids.Count <= maxCount)
          .WithMessage($"{propertyName} must not contain more than {maxCount} items.")
          .Must(ids => ids.All(id => id != Guid.Empty))
          .WithMessage($"{propertyName} must not contain empty Guid values.")
          .Must(ids => ids.Distinct().Count() == ids.Count)
          .WithMessage($"{propertyName} must be unique.")
          .OverridePropertyName(propertyName);
      });
  }
}
