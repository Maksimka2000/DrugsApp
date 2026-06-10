using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.UpsertAllergySummary;

/// <summary>
/// Validator for upserting allergy summary answers.
/// </summary>
public sealed class UpsertAllergySummaryRequestValidator : AbstractValidator<UpsertAllergySummaryRequest>
{
    private static readonly string[] AllowedAnswerTokens = ["yes", "no", "dont_know"];

    public UpsertAllergySummaryRequestValidator()
    {
        When(request => !string.IsNullOrWhiteSpace(request.SevereReactionStatus), () =>
        {
            RuleFor(request => request.SevereReactionStatus!)
                .Must(token => AllowedAnswerTokens.Contains(token.Trim().ToLowerInvariant()))
                .WithMessage("severeReactionStatus must be one of: yes, no, dont_know.");
        });

        When(request => !string.IsNullOrWhiteSpace(request.PollenDustMitesAllergyStatus), () =>
        {
            RuleFor(request => request.PollenDustMitesAllergyStatus!)
                .Must(token => AllowedAnswerTokens.Contains(token.Trim().ToLowerInvariant()))
                .WithMessage("pollenDustMitesAllergyStatus must be one of: yes, no, dont_know.");
        });

        When(request => !string.IsNullOrWhiteSpace(request.InsectVenomAllergyStatus), () =>
        {
            RuleFor(request => request.InsectVenomAllergyStatus!)
                .Must(token => AllowedAnswerTokens.Contains(token.Trim().ToLowerInvariant()))
                .WithMessage("insectVenomAllergyStatus must be one of: yes, no, dont_know.");
        });

        When(request => !string.IsNullOrWhiteSpace(request.LatexAllergyStatus), () =>
        {
            RuleFor(request => request.LatexAllergyStatus!)
                .Must(token => AllowedAnswerTokens.Contains(token.Trim().ToLowerInvariant()))
                .WithMessage("latexAllergyStatus must be one of: yes, no, dont_know.");
        });
    }
}
