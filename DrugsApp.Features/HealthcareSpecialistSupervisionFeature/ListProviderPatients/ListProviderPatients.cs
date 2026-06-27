using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderPatients;

/// <summary>
/// Request payload for searching provider patients.
/// </summary>
/// <param name="SearchQuery">Optional search query.</param>
/// <param name="SortBy">Sort field token.</param>
/// <param name="OrderBy">Order direction.</param>
/// <param name="Statuses">Optional relationship status filters.</param>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
public sealed record SearchProviderPatientsRequest(
    [FromQuery(Name = "search_query")] string? SearchQuery,
    [FromQuery(Name = "sort_by")] string SortBy = "status",
    [FromQuery(Name = "order_by")] string OrderBy = "ascending",
    [FromQuery(Name = "statuses")] List<string>? Statuses = null,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20);

/// <summary>
/// Query for searching provider patients.
/// </summary>
/// <param name="ProviderUserId">Provider user identifier.</param>
/// <param name="SearchQuery">Optional search query.</param>
/// <param name="Statuses">Status filters.</param>
/// <param name="SortBy">Sort field.</param>
/// <param name="Ascending">Order direction flag.</param>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
public sealed record SearchProviderPatientsQuery(
    Guid ProviderUserId,
    string? SearchQuery,
    IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? Statuses,
    ProviderPatientSortField SortBy,
    bool Ascending,
    int Skip,
    int Take) : IRequest<ErrorOr<ProviderPatientListResponse>>;

internal sealed class SearchProviderPatientsHandler(
    ISupervisionReadRepository supervisionReadRepository,
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository)
    : IRequestHandler<SearchProviderPatientsQuery, ErrorOr<ProviderPatientListResponse>>
{
    private static readonly (string Token, ProviderPatientSearchField Field)[] SearchPriorityFields =
    [
        ("email", ProviderPatientSearchField.Email),
        ("display_name", ProviderPatientSearchField.DisplayName)
    ];

    public async Task<ErrorOr<ProviderPatientListResponse>> Handle(
        SearchProviderPatientsQuery query,
        CancellationToken cancellationToken)
    {
        HealthcareProviderProfile? profile = await healthcareProviderProfileRepository.GetTrackedByUserIdAsync(
            query.ProviderUserId,
            cancellationToken);

        if (profile is null)
        {
            return Error.Forbidden(description: "Provider profile required.");
        }

        int takePlusOne = query.Take + 1;
        int? totalCount = null;

        ProviderPatientSearchField appliedSearchField = ProviderPatientSearchField.None;
        string? matchedFieldToken = null;
        List<string> matchedFailedFieldTokens = new();
        SearchProviderPatientsResult result;

        if (string.IsNullOrWhiteSpace(query.SearchQuery))
        {
            result = await supervisionReadRepository.SearchProviderPatientsAsync(
                query.ProviderUserId,
                searchQuery: null,
                searchField: appliedSearchField,
                statuses: query.Statuses,
                sortField: query.SortBy,
                ascending: query.Ascending,
                skip: query.Skip,
                take: takePlusOne,
                cancellationToken);
        }
        else
        {
            SearchProviderPatientsResult? matchedResult = null;

            foreach ((string token, ProviderPatientSearchField field) in SearchPriorityFields)
            {
                SearchProviderPatientsResult attempt = await supervisionReadRepository.SearchProviderPatientsAsync(
                    query.ProviderUserId,
                    searchQuery: query.SearchQuery,
                    searchField: field,
                    statuses: query.Statuses,
                    sortField: query.SortBy,
                    ascending: query.Ascending,
                    skip: query.Skip,
                    take: takePlusOne,
                    cancellationToken);

                if (attempt.Items.Count > 0)
                {
                    matchedResult = attempt;
                    matchedFieldToken = token;
                    appliedSearchField = field;
                    break;
                }

                matchedFailedFieldTokens.Add(token);
            }

            result = matchedResult ?? new SearchProviderPatientsResult([], 0);
        }

        if (query.Skip == 0)
        {
            if (!string.IsNullOrWhiteSpace(query.SearchQuery) && matchedFieldToken is null)
            {
                totalCount = 0;
            }
            else
            {
                totalCount = await supervisionReadRepository.CountProviderPatientsAsync(
                    query.ProviderUserId,
                    searchQuery: query.SearchQuery,
                    searchField: appliedSearchField,
                    statuses: query.Statuses,
                    cancellationToken);
            }
        }

        return result.MapToResponse(query.Take, totalCount, matchedFieldToken, matchedFailedFieldTokens);
    }
}
