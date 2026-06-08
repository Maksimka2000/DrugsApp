using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.SupportFeature.Entities;

/// <summary>
/// Represents a support FAQ entry exposed to public and admin clients.
/// </summary>
public sealed class FaqEntry : BaseEntity
{
    // Attributes

    /// <summary>
    /// Public display question shown in FAQ lists.
    /// </summary>
    /// <remarks>
    /// The value is trimmed and collapsed into a single-space representation.
    /// </remarks>
    public string Question { get; private set; } = string.Empty;

    /// <summary>
    /// Normalized question value used for uniqueness and search.
    /// </summary>
    /// <remarks>
    /// The value is derived from <see cref="Question"/> and stored in a lower-case collapsed form.
    /// </remarks>
    public string NormalizedQuestion { get; private set; } = string.Empty;

    /// <summary>
    /// Answer content rendered in the FAQ details area.
    /// </summary>
    /// <remarks>
    /// Boundary whitespace is trimmed while meaningful inner formatting is preserved.
    /// </remarks>
    public string Answer { get; private set; } = string.Empty;

    // Relationships

    private FaqEntry() : base()
    {
    }

    private FaqEntry(
        string question,
        string normalizedQuestion,
        string answer) : this()
    {
        Question = question;
        NormalizedQuestion = normalizedQuestion;
        Answer = answer;
    }

    // Methods

    /// <summary>
    /// Creates a new FAQ entry aggregate.
    /// </summary>
    /// <param name="question">Question text to display publicly.</param>
    /// <param name="answer">Answer text shown for the FAQ item.</param>
    /// <returns>
    /// A validated <see cref="FaqEntry"/> aggregate or validation errors describing why creation failed.
    /// </returns>
    public static ErrorOr<FaqEntry> Create(string question, string answer)
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

        return new FaqEntry(
            question: normalizedQuestion.Value.DisplayValue,
            normalizedQuestion: normalizedQuestion.Value.LookupValue,
            answer: normalizedAnswer.Value);
    }

    /// <summary>
    /// Updates the FAQ question while keeping the normalized lookup field in sync.
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
    /// Updates the FAQ answer content.
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
                code: "FaqEntry.QuestionEmpty",
                description: "Question must be provided.",
                metadata: new()
                {
                    ["field"] = "question"
                });
        }

        if (collapsed.Length > 300)
        {
            return Error.Validation(
                code: "FaqEntry.QuestionTooLong",
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
                code: "FaqEntry.AnswerEmpty",
                description: "Answer must be provided.",
                metadata: new()
                {
                    ["field"] = "answer"
                });
        }

        return trimmed;
    }
}
