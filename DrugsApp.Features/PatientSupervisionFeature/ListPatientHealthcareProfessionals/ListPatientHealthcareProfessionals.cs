using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientHealthcareProfessionals;

/// <summary>
/// Request payload for searching patient healthcare professionals.
/// </summary>
/// <param name="SearchQuery">Optional search query.</param>
/// <param name="SortBy">Sort field token.</param>
/// <param name="OrderBy">Order direction.</param>
/// <param name="Statuses">Optional relationship status filters.</param>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
public sealed record SearchPatientHealthcareProfessionalsRequest(
    [FromQuery(Name = "search_query")] string? SearchQuery,
    [FromQuery(Name = "sort_by")] string SortBy = "status",
    [FromQuery(Name = "order_by")] string OrderBy = "ascending",
    [FromQuery(Name = "statuses")] List<string>? Statuses = null,
    [FromQuery(Name = "skip")] int Skip = 0,
    [FromQuery(Name = "take")] int Take = 20);

/// <summary>
/// Query for searching patient healthcare professionals.
/// </summary>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="SearchQuery">Optional search query.</param>
/// <param name="Statuses">Status filters.</param>
/// <param name="SortBy">Sort field.</param>
/// <param name="Ascending">Order direction flag.</param>
/// <param name="Skip">Number of records to skip.</param>
/// <param name="Take">Number of records to take.</param>
public sealed record SearchPatientHealthcareProfessionalsQuery(
    Guid PatientUserId,
    string? SearchQuery,
    IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? Statuses,
    PatientHealthcareProfessionalSortField SortBy,
    bool Ascending,
    int Skip,
    int Take) : IRequest<ErrorOr<PatientHealthcareProfessionalListResponse>>;

internal sealed class SearchPatientHealthcareProfessionalsHandler(ISupervisionReadRepository supervisionReadRepository)
    : IRequestHandler<SearchPatientHealthcareProfessionalsQuery, ErrorOr<PatientHealthcareProfessionalListResponse>>
{
    private static readonly (string Token, PatientHealthcareProfessionalSearchField Field)[] SearchPriorityFields =
    [
        ("email", PatientHealthcareProfessionalSearchField.Email),
        ("display_name", PatientHealthcareProfessionalSearchField.DisplayName),
        ("profession_label", PatientHealthcareProfessionalSearchField.ProfessionLabel),
        ("workplace_label", PatientHealthcareProfessionalSearchField.WorkplaceLabel)
    ];

    public async Task<ErrorOr<PatientHealthcareProfessionalListResponse>> Handle(
        SearchPatientHealthcareProfessionalsQuery query,
        CancellationToken cancellationToken)
    {
        int takePlusOne = query.Take + 1;
        int? totalCount = null;

        PatientHealthcareProfessionalSearchField appliedSearchField = PatientHealthcareProfessionalSearchField.None;
        string? matchedFieldToken = null;
        List<string> matchedFailedFieldTokens = new();
        SearchPatientHealthcareProfessionalsResult result;

        if (string.IsNullOrWhiteSpace(query.SearchQuery))
        {
            result = await supervisionReadRepository.SearchPatientHealthcareProfessionalsAsync(
                query.PatientUserId,
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
            SearchPatientHealthcareProfessionalsResult? matchedResult = null;

            foreach ((string token, PatientHealthcareProfessionalSearchField field) in SearchPriorityFields)
            {
                SearchPatientHealthcareProfessionalsResult attempt =
                    await supervisionReadRepository.SearchPatientHealthcareProfessionalsAsync(
                        query.PatientUserId,
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

            result = matchedResult ?? new SearchPatientHealthcareProfessionalsResult([], 0);
        }

        if (query.Skip == 0)
        {
            if (!string.IsNullOrWhiteSpace(query.SearchQuery) && matchedFieldToken is null)
            {
                totalCount = 0;
            }
            else
            {
                totalCount = await supervisionReadRepository.CountPatientHealthcareProfessionalsAsync(
                    query.PatientUserId,
                    searchQuery: query.SearchQuery,
                    searchField: appliedSearchField,
                    statuses: query.Statuses,
                    cancellationToken);
            }
        }

        return result.MapToResponse(query.Take, totalCount, matchedFieldToken, matchedFailedFieldTokens);
    }
}
