using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.SupportFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.SupportFeature.Entities;

/// <summary>
/// Represents a logical support FAQ entry with localized translations.
/// </summary>
public sealed class FaqEntry : BaseEntity
{
    // Relationships

    private readonly List<FaqEntryTranslation> _translations = new();

    /// <summary>
    /// Localized question and answer variants for the FAQ entry.
    /// </summary>
    public IReadOnlyCollection<FaqEntryTranslation> Translations => _translations.AsReadOnly();

    private FaqEntry() : base()
    {
    }

    // Methods

    /// <summary>
    /// Creates a new FAQ entry aggregate with English and Polish translations.
    /// </summary>
    /// <param name="englishQuestion">English question text.</param>
    /// <param name="englishAnswer">English answer text.</param>
    /// <param name="polishQuestion">Polish question text.</param>
    /// <param name="polishAnswer">Polish answer text.</param>
    /// <returns>
    /// A validated <see cref="FaqEntry"/> aggregate or validation errors describing why creation failed.
    /// </returns>
    public static ErrorOr<FaqEntry> Create(
        string englishQuestion,
        string englishAnswer,
        string polishQuestion,
        string polishAnswer)
    {
        FaqEntry faqEntry = new();

        ErrorOr<Success> englishResult = faqEntry.AddTranslation(
            languageCode: FaqLanguageCode.En,
            question: englishQuestion,
            answer: englishAnswer);

        if (englishResult.IsError)
        {
            return englishResult.Errors;
        }

        ErrorOr<Success> polishResult = faqEntry.AddTranslation(
            languageCode: FaqLanguageCode.Pl,
            question: polishQuestion,
            answer: polishAnswer);

        if (polishResult.IsError)
        {
            return polishResult.Errors;
        }

        return faqEntry;
    }

    /// <summary>
    /// Returns the translation for the requested language when it exists.
    /// </summary>
    /// <param name="languageCode">Requested language code.</param>
    /// <returns>The matching translation or <see langword="null"/>.</returns>
    public FaqEntryTranslation? GetTranslation(FaqLanguageCode languageCode) =>
        _translations.FirstOrDefault(translation => translation.LanguageCode == languageCode);

    /// <summary>
    /// Updates the English translation question and/or answer.
    /// </summary>
    /// <param name="question">Replacement English question text when provided.</param>
    /// <param name="answer">Replacement English answer text when provided.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateEnglish(string? question, string? answer) =>
        UpdateTranslation(FaqLanguageCode.En, question, answer);

    /// <summary>
    /// Updates the Polish translation question and/or answer.
    /// </summary>
    /// <param name="question">Replacement Polish question text when provided.</param>
    /// <param name="answer">Replacement Polish answer text when provided.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdatePolish(string? question, string? answer) =>
        UpdateTranslation(FaqLanguageCode.Pl, question, answer);

    private ErrorOr<Success> AddTranslation(
        FaqLanguageCode languageCode,
        string question,
        string answer)
    {
        if (_translations.Any(translation => translation.LanguageCode == languageCode))
        {
            return Error.Validation(
                code: "FaqEntry.TranslationAlreadyExists",
                description: "A translation for the requested language already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["languageCode"] = languageCode.ToString()
                });
        }

        ErrorOr<FaqEntryTranslation> translationResult = FaqEntryTranslation.Create(
            languageCode: languageCode,
            question: question,
            answer: answer);

        if (translationResult.IsError)
        {
            return translationResult.Errors;
        }

        FaqEntryTranslation translation = translationResult.Value;
        translation.AttachToFaqEntry(this);
        _translations.Add(translation);

        return Result.Success;
    }

    private ErrorOr<Success> UpdateTranslation(
        FaqLanguageCode languageCode,
        string? question,
        string? answer)
    {
        FaqEntryTranslation? translation = GetTranslation(languageCode);
        if (translation is null)
        {
            return Error.NotFound(
                code: "FaqEntry.TranslationNotFound",
                description: "The requested FAQ translation was not found.",
                metadata: new Dictionary<string, object>
                {
                    ["languageCode"] = languageCode.ToString()
                });
        }

        if (question is not null)
        {
            ErrorOr<Success> updateQuestionResult = translation.UpdateQuestion(question);
            if (updateQuestionResult.IsError)
            {
                return updateQuestionResult.Errors;
            }
        }

        if (answer is not null)
        {
            ErrorOr<Success> updateAnswerResult = translation.UpdateAnswer(answer);
            if (updateAnswerResult.IsError)
            {
                return updateAnswerResult.Errors;
            }
        }

        return Result.Success;
    }
}
