using System;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.SupportFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.SupportFeature.Entities;

/// <summary>
/// Localized question and answer content for a logical FAQ entry.
/// </summary>
public sealed class FaqEntryTranslation : BaseEntity
{
    // Attributes

    /// <summary>
    /// Language of the localized FAQ content.
    /// </summary>
    /// <remarks>
    /// Persisted as a two-letter code such as <c>en</c> or <c>pl</c>.
    /// </remarks>
    public FaqLanguageCode LanguageCode { get; private set; }

    /// <summary>
    /// Public display question shown in FAQ lists for the language.
    /// </summary>
    /// <remarks>
    /// The value is trimmed and collapsed into a single-space representation.
    /// </remarks>
    public string Question { get; private set; } = string.Empty;

    /// <summary>
    /// Normalized question value used for uniqueness and search within the language.
    /// </summary>
    /// <remarks>
    /// The value is derived from <see cref="Question"/> and stored in a lower-case collapsed form.
    /// </remarks>
    public string NormalizedQuestion { get; private set; } = string.Empty;

    /// <summary>
    /// Answer content rendered in the FAQ details area for the language.
    /// </summary>
    /// <remarks>
    /// Boundary whitespace is trimmed while meaningful inner formatting is preserved.
    /// </remarks>
    public string Answer { get; private set; } = string.Empty;

    // Relationships

    /// <summary>
    /// Foreign key to the owning logical FAQ entry.
    /// </summary>
    public Guid FaqEntryId { get; private set; }

    /// <summary>
    /// Owning logical FAQ entry aggregate.
    /// </summary>
    public FaqEntry FaqEntry { get; private set; } = null!;

    private FaqEntryTranslation() : base()
    {
    }

    internal static ErrorOr<FaqEntryTranslation> Create(
        FaqLanguageCode languageCode,
        string question,
        string answer)
    {
        ErrorOr<(string DisplayValue, string LookupValue)> normalizedQuestion = NormalizeQuestion(question);
        if (normalizedQuestion.IsError)
        {
            return normalizedQuestion.Errors;
        }

        ErrorOr<string> normalizedAnswer = NormalizeAnswer(answer);
        if (normalizedAnswer.IsError)
        {
            return normalizedAnswer.Errors;
        }

        return new FaqEntryTranslation
        {
            LanguageCode = languageCode,
            Question = normalizedQuestion.Value.DisplayValue,
            NormalizedQuestion = normalizedQuestion.Value.LookupValue,
            Answer = normalizedAnswer.Value
        };
    }

    internal void AttachToFaqEntry(FaqEntry faqEntry)
    {
        FaqEntry = faqEntry;
        FaqEntryId = faqEntry.Id;
    }

    /// <summary>
    /// Updates the localized question while keeping the normalized lookup field in sync.
    /// </summary>
    /// <param name="question">Replacement question text.</param>
    /// <returns>
    /// Success when the question is valid; otherwise validation errors describing the failure.
    /// </returns>
    public ErrorOr<Success> UpdateQuestion(string question)
    {
        ErrorOr<(string DisplayValue, string LookupValue)> normalizedQuestion = NormalizeQuestion(question);
        if (normalizedQuestion.IsError)
        {
            return normalizedQuestion.Errors;
        }

        Question = normalizedQuestion.Value.DisplayValue;
        NormalizedQuestion = normalizedQuestion.Value.LookupValue;

        return Result.Success;
    }

    /// <summary>
    /// Updates the localized answer content.
    /// </summary>
    /// <param name="answer">Replacement answer text.</param>
    /// <returns>
    /// Success when the answer is valid; otherwise validation errors describing the failure.
    /// </returns>
    public ErrorOr<Success> UpdateAnswer(string answer)
    {
        ErrorOr<string> normalizedAnswer = NormalizeAnswer(answer);
        if (normalizedAnswer.IsError)
        {
            return normalizedAnswer.Errors;
        }

        Answer = normalizedAnswer.Value;

        return Result.Success;
    }

    private static ErrorOr<(string DisplayValue, string LookupValue)> NormalizeQuestion(string value)
    {
        string trimmed = value.Trim();
        string collapsed = string.Join(
            ' ',
            trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: "FaqEntryTranslation.QuestionEmpty",
                description: "Question must be provided.",
                metadata: new()
                {
                    ["field"] = "question"
                });
        }

        if (collapsed.Length > 300)
        {
            return Error.Validation(
                code: "FaqEntryTranslation.QuestionTooLong",
                description: "Question must not exceed 300 characters.",
                metadata: new()
                {
                    ["field"] = "question",
                    ["maxLength"] = 300
                });
        }

        return (collapsed, collapsed.ToLowerInvariant());
    }

    private static ErrorOr<string> NormalizeAnswer(string value)
    {
        string trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return Error.Validation(
                code: "FaqEntryTranslation.AnswerEmpty",
                description: "Answer must be provided.",
                metadata: new()
                {
                    ["field"] = "answer"
                });
        }

        return trimmed;
    }
}
