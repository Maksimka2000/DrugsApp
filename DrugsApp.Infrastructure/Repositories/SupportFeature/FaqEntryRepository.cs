using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SupportFeature;
using DrugsApp.Domain.SupportFeature.Entities;
using DrugsApp.Domain.SupportFeature.Enums;
using DrugsApp.Domain.SupportFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.SupportFeature;

public sealed class FaqEntryRepository : IFaqEntryRepository
{
    private const double SimilarityThreshold = 0.35;

    private readonly DrugsAppDbContext _context;

    public FaqEntryRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FaqEntry>> SearchPublicAsync(
        FaqLanguageCode languageCode,
        string? questionQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<FaqEntry> query = BuildPublicQuery(languageCode, questionQuery);
        query = ApplyPublicSort(query, languageCode);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountPublicAsync(
        FaqLanguageCode languageCode,
        string? questionQuery,
        CancellationToken cancellationToken)
    {
        IQueryable<FaqEntry> query = BuildPublicQuery(languageCode, questionQuery);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<List<FaqEntry>> SearchAdminAsync(
        string? questionQuery,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<FaqEntry> query = BuildAdminQuery(questionQuery);
        query = ApplyAdminSort(query);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAdminAsync(
        string? questionQuery,
        CancellationToken cancellationToken)
    {
        IQueryable<FaqEntry> query = BuildAdminQuery(questionQuery);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<FaqEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.FaqEntries
            .Include(faqEntry => faqEntry.Translations)
            .FirstOrDefaultAsync(faqEntry => faqEntry.Id == id, cancellationToken);
    }

    public async Task<FaqEntry?> GetByNormalizedQuestionAsync(
        string question,
        FaqLanguageCode languageCode,
        CancellationToken cancellationToken)
    {
        string normalizedQuestion = NormalizeQuestionQuery(question);
        if (normalizedQuestion.Length == 0)
        {
            return null;
        }

        return await _context.FaqEntries
            .AsNoTracking()
            .Where(faqEntry =>
                faqEntry.Translations.Any(translation =>
                    translation.LanguageCode == languageCode &&
                    translation.NormalizedQuestion == normalizedQuestion))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(FaqEntry faqEntry, CancellationToken cancellationToken)
    {
        await _context.FaqEntries.AddAsync(faqEntry, cancellationToken);
    }

    public void Remove(FaqEntry faqEntry)
    {
        _context.FaqEntries.Remove(faqEntry);
    }

    private IQueryable<FaqEntry> BuildPublicQuery(
        FaqLanguageCode languageCode,
        string? questionQuery)
    {
        IQueryable<FaqEntry> query = _context.FaqEntries
            .AsNoTracking()
            .Include(faqEntry => faqEntry.Translations)
            .Where(faqEntry => faqEntry.Translations.Any(
                translation => translation.LanguageCode == languageCode));

        return ApplyTranslationQuestionFilter(query, languageCode, questionQuery);
    }

    private IQueryable<FaqEntry> BuildAdminQuery(string? questionQuery)
    {
        IQueryable<FaqEntry> query = _context.FaqEntries
            .AsNoTracking()
            .Include(faqEntry => faqEntry.Translations)
            .Where(faqEntry =>
                faqEntry.Translations.Any(translation => translation.LanguageCode == FaqLanguageCode.En) &&
                faqEntry.Translations.Any(translation => translation.LanguageCode == FaqLanguageCode.Pl));

        return ApplyAnyTranslationQuestionFilter(query, questionQuery);
    }

    private static IQueryable<FaqEntry> ApplyTranslationQuestionFilter(
        IQueryable<FaqEntry> query,
        FaqLanguageCode languageCode,
        string? questionQuery)
    {
        string normalizedQuestionQuery = NormalizeQuestionQuery(questionQuery);
        if (normalizedQuestionQuery.Length == 0)
        {
            return query;
        }

        string prefixPattern = $"{normalizedQuestionQuery}%";
        string containsPattern = $"%{normalizedQuestionQuery}%";
        bool useTrigram = normalizedQuestionQuery.Length >= 3;

        return query.Where(faqEntry =>
            faqEntry.Translations.Any(translation =>
                translation.LanguageCode == languageCode &&
                (
                    EF.Functions.ILike(translation.NormalizedQuestion, prefixPattern) ||
                    EF.Functions.ILike(translation.NormalizedQuestion, containsPattern) ||
                    useTrigram &&
                    EF.Functions.TrigramsSimilarity(
                        translation.NormalizedQuestion,
                        normalizedQuestionQuery) >= SimilarityThreshold)));
    }

    private static IQueryable<FaqEntry> ApplyAnyTranslationQuestionFilter(
        IQueryable<FaqEntry> query,
        string? questionQuery)
    {
        string normalizedQuestionQuery = NormalizeQuestionQuery(questionQuery);
        if (normalizedQuestionQuery.Length == 0)
        {
            return query;
        }

        string prefixPattern = $"{normalizedQuestionQuery}%";
        string containsPattern = $"%{normalizedQuestionQuery}%";
        bool useTrigram = normalizedQuestionQuery.Length >= 3;

        return query.Where(faqEntry =>
            faqEntry.Translations.Any(translation =>
                EF.Functions.ILike(translation.NormalizedQuestion, prefixPattern) ||
                EF.Functions.ILike(translation.NormalizedQuestion, containsPattern) ||
                useTrigram &&
                EF.Functions.TrigramsSimilarity(
                    translation.NormalizedQuestion,
                    normalizedQuestionQuery) >= SimilarityThreshold));
    }

    private static IQueryable<FaqEntry> ApplyPublicSort(
        IQueryable<FaqEntry> query,
        FaqLanguageCode languageCode) =>
        query
            .OrderBy(faqEntry => faqEntry.Translations
                .Where(translation => translation.LanguageCode == languageCode)
                .Select(translation => translation.Question)
                .FirstOrDefault())
            .ThenBy(faqEntry => faqEntry.CreatedAtUtc)
            .ThenBy(faqEntry => faqEntry.Id);

    private static IQueryable<FaqEntry> ApplyAdminSort(IQueryable<FaqEntry> query) =>
        query
            .OrderByDescending(faqEntry => faqEntry.UpdatedAtUtc.HasValue)
            .ThenByDescending(faqEntry => faqEntry.UpdatedAtUtc)
            .ThenByDescending(faqEntry => faqEntry.CreatedAtUtc)
            .ThenByDescending(faqEntry => faqEntry.Id);

    private static string NormalizeQuestionQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string trimmed = value.Trim();
        string collapsed = string.Join(
            ' ',
            trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        return collapsed.ToLowerInvariant();
    }
}
