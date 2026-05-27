using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.SupplementFeature.Shared;
using ErrorOr;
using MediatR;
using DrugsApp.Domain.SupplementFeature.Entities;


namespace DrugsApp.Features.SupplementFeature.SearchSupplementGroups;


/// <summary>
/// Handles searching supplement groups with prioritized group-name then membership.
/// </summary>
public sealed class SearchSupplementGroupsHandler(ISupplementGroupRepository _supplementGroupRepository) : IRequestHandler<SearchSupplementGroupsQuery, ErrorOr<SearchSupplementGroupsResponse>>
{

     private static readonly Dictionary<string, SupplementGroupSearchField> SearchPriorityFields = new()
    {
        ["group_name"] = SupplementGroupSearchField.GroupName,
        ["supplement_name"] = SupplementGroupSearchField.SuplementName
    };


    /// <summary>
    /// Handles the search operation for supplement groups based on the provided query.
    /// </summary>
    /// <param name="query">The query containing search parameters such as supplement name, skip, and take.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the search response, which includes a collection of supplement groups,
    /// metadata about the search operation, and additional information about matched fields.</returns>
    public async Task<ErrorOr<SearchSupplementGroupsResponse>> Handle(
        SearchSupplementGroupsQuery query,
        CancellationToken cancellationToken)
    {
        // normalize supplement name query
        string? normalizedSupplementNameQuery = string.IsNullOrWhiteSpace(query.SupplementNameQuery)
            ? null
            : query.SupplementNameQuery!.Trim();

        // initialize take plus one
        int takePlusOne = query.Take + 1;
        // initialize total count
        int? totalCount = null;

        // initialize search result list
        List<SupplementGroup> rows = new();
        // initialize applied search field
        SupplementGroupSearchField appliedSearchField = SupplementGroupSearchField.None;
        // initialize matched field token
        string? matchedFieldToken = null;
        // initialize matched field tokens list
        List<string> matchedFailedFieldTokens = new();

        // if supplement name query is empty, search by group name
        if (string.IsNullOrWhiteSpace(normalizedSupplementNameQuery)){
            rows = await _supplementGroupRepository.SearchAsync(
                supplementNameQuery: null,
                searchField: appliedSearchField,
                skip: query.Skip,
                take: takePlusOne,
                cancellationToken: cancellationToken);

            matchedFieldToken = appliedSearchField.ToString();
        }
        // if supplement name query is not empty, search by supplement name
        else {
            // search by priority fields
            foreach ((string token, SupplementGroupSearchField field) in SearchPriorityFields){
                // search by field
                List<SupplementGroup> attempt = await _supplementGroupRepository.SearchAsync(
                    supplementNameQuery: normalizedSupplementNameQuery,
                    searchField: field,
                    skip: query.Skip,
                    take: takePlusOne,
                    cancellationToken: cancellationToken);
                // if attempt is not empty, set rows to attempt
                if (attempt.Count > 0){
                    rows = attempt;
                    matchedFieldToken = token;
                    appliedSearchField = field;
                    break;
                } else {
                    matchedFailedFieldTokens.Add(token);
                }
            }
        }


        // determine if there are more items
        bool hasMore = rows.Count > query.Take;
        
        // get page
        var page = rows.Take(query.Take).ToList();
        

        // count total items
        if (query.Skip == 0)
        {
            if (rows.Count == 0) 
            {
                totalCount = 0;
            }
            else
            {
                totalCount = await _supplementGroupRepository.CountAsync(
                    supplementNameQuery: normalizedSupplementNameQuery,
                    searchField: appliedSearchField,
                    cancellationToken: cancellationToken);
            }
            
        }

        return page.MapToResponse(totalCount, hasMore, matchedFieldToken, matchedFailedFieldTokens);
    }
}
