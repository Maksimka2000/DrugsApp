using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.SupportFeature.Entities;

namespace DrugsApp.Domain.SupportFeature.Repositories;

public interface IFaqEntryRepository
{
    Task<List<FaqEntry>> SearchPublicAsync(
        string? questionQuery,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountPublicAsync(
        string? questionQuery,
        CancellationToken cancellationToken);

    Task<List<FaqEntry>> SearchAdminAsync(
        string? questionQuery,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountAdminAsync(
        string? questionQuery,
        CancellationToken cancellationToken);

    Task<FaqEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<FaqEntry?> GetByNormalizedQuestionAsync(string question, CancellationToken cancellationToken);

    Task AddAsync(FaqEntry faqEntry, CancellationToken cancellationToken);

    void Remove(FaqEntry faqEntry);
}
