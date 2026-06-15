using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.PatientSupervisionFeature;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientHealthcareProfessionals;

internal static class ListPatientHealthcareProfessionalsMapping
{
    public static SearchPatientHealthcareProfessionalsQuery MapToQuery(
        this SearchPatientHealthcareProfessionalsRequest request,
        Guid patientUserId) =>
        new(
            PatientUserId: patientUserId,
            SearchQuery: NormalizeQuery(request.SearchQuery),
            Statuses: MapStatuses(request.Statuses),
            SortBy: MapSortBy(request.SortBy),
            Ascending: request.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase),
            Skip: request.Skip,
            Take: request.Take);

    public static PatientHealthcareProfessionalListResponse MapToResponse(
        this SearchPatientHealthcareProfessionalsResult result,
        int requestedTake,
        int? totalCount,
        string? matchedFieldToken,
        List<string> matchedFailedFieldTokens)
    {
        var items = result.Items.Select(row => new PatientHealthcareProfessionalListItemDto(
            row.EntryType,
            row.HealthcareProfessionalUserId,
            row.RequestId,
            row.ConnectionId,
            row.RelationshipState,
            row.AccessLevelCode,
            row.FirstName,
            row.LastName,
            row.ProfessionLabel,
            row.WorkplaceLabel,
            row.ShortDescription,
            row.HasPendingProposal,
            row.PendingProposalCount,
            row.RequestedAtUtc,
            row.ApprovedAtUtc,
            MemberProfilePhotoUrls.Build(row.HealthcareProfessionalUserId, row.ProfilePhotoStorageKey))).ToList();

        bool hasMore = items.Count > requestedTake;
        var page = items.Take(requestedTake).ToList();

        return new PatientHealthcareProfessionalListResponse(
            Items: page,
            TotalCount: totalCount ?? result.TotalCount,
            HasMore: hasMore,
            MatchedFieldToken: matchedFieldToken,
            MatchedFailedFieldTokens: matchedFailedFieldTokens);
    }

    private static string? NormalizeQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? MapStatuses(
        IReadOnlyCollection<string>? statuses)
    {
        if (statuses is null || statuses.Count == 0)
        {
            return null;
        }

        var mapped = statuses
            .Select(status =>
            {
                PatientHealthcareProfessionalSearchCatalog.TryParseRelationshipStatus(status, out PatientHealthcareProfessionalRelationshipStatus parsed);
                return parsed;
            })
            .Distinct()
            .ToList();

        if (mapped.Count == PatientHealthcareProfessionalSearchCatalog.GetRelationshipStatusTokens().Count)
        {
            return null;
        }

        return mapped.Count == 0 ? null : mapped;
    }

    private static PatientHealthcareProfessionalSortField MapSortBy(string sortBy)
    {
        PatientHealthcareProfessionalSearchCatalog.TryParseSortField(sortBy, out PatientHealthcareProfessionalSortField sortField);
        return sortField;
    }
}
