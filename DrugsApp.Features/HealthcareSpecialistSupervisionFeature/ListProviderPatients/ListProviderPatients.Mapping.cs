using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.PatientSupervisionFeature;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderPatients;

internal static class ListProviderPatientsMapping
{
    public static SearchProviderPatientsQuery MapToQuery(
        this SearchProviderPatientsRequest request,
        Guid providerUserId) =>
        new(
            ProviderUserId: providerUserId,
            SearchQuery: NormalizeQuery(request.SearchQuery),
            Statuses: MapStatuses(request.Statuses),
            SortBy: MapSortBy(request.SortBy),
            Ascending: request.OrderBy.Equals("ascending", StringComparison.OrdinalIgnoreCase),
            Skip: request.Skip,
            Take: request.Take);

    public static ProviderPatientListResponse MapToResponse(
        this SearchProviderPatientsResult result,
        int requestedTake,
        int? totalCount,
        string? matchedFieldToken,
        List<string> matchedFailedFieldTokens)
    {
        var items = result.Items.Select(r => new ProviderPatientListItemDto(
            r.EntryType,
            r.PatientUserId,
            r.RequestId,
            r.ConnectionId,
            r.RelationshipState,
            r.AccessLevelCode,
            r.FirstName,
            r.LastName,
            r.HasProfilePhoto,
            r.RequestedAtUtc,
            r.ApprovedAtUtc)).ToList();

        bool hasMore = items.Count > requestedTake;
        var page = items.Take(requestedTake).ToList();

        return new ProviderPatientListResponse(
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
                ProviderPatientSearchCatalog.TryParseRelationshipStatus(status, out PatientHealthcareProfessionalRelationshipStatus parsed);
                return parsed;
            })
            .Distinct()
            .ToList();

        if (mapped.Count == ProviderPatientSearchCatalog.GetRelationshipStatusTokens().Count)
        {
            return null;
        }

        return mapped.Count == 0 ? null : mapped;
    }

    private static ProviderPatientSortField MapSortBy(string sortBy)
    {
        ProviderPatientSearchCatalog.TryParseSortField(sortBy, out ProviderPatientSortField sortField);
        return sortField;
    }
}
