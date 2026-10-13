using DrugsApp.Domain.StudyModeFeature;
using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.GenerateStudyPatientRandomData;

/// <summary>
/// Validator for the random data request (section token and count).
/// </summary>
public sealed class GenerateStudyPatientRandomDataRequestValidator : AbstractValidator<GenerateStudyPatientRandomDataRequest>
{
    /// <summary>
    /// Initializes section and count rules.
    /// </summary>
    public GenerateStudyPatientRandomDataRequestValidator()
    {
        RuleFor(request => request.Section)
            .Must(section => !string.IsNullOrWhiteSpace(section)
                && GenerateStudyPatientRandomDataMapping.SectionsByToken.ContainsKey(section))
            .WithMessage($"section must be one of: {string.Join(", ", GenerateStudyPatientRandomDataMapping.SectionsByToken.Keys)}.");

        RuleFor(request => request.Count)
            .InclusiveBetween(1, StudyRandomMedicalDataCatalog.MAX_ITEMS_PER_REQUEST)
            .WithMessage($"count must be between 1 and {StudyRandomMedicalDataCatalog.MAX_ITEMS_PER_REQUEST}.");
    }
}
