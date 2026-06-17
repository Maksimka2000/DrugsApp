using DrugsApp.Domain.PatientSupervisionFeature;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.PatientSupervisionFeature;

public sealed class SupervisionReadRepository(DrugsAppDbContext context) : ISupervisionReadRepository
{
    public async Task<SearchPatientHealthcareProfessionalsResult> SearchPatientHealthcareProfessionalsAsync(
        Guid patientUserId,
        string? searchQuery,
        PatientHealthcareProfessionalSearchField searchField,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses,
        PatientHealthcareProfessionalSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        List<PatientHealthcareProfessionalListRow> combined = await BuildPatientHealthcareProfessionalRowsAsync(
            patientUserId,
            cancellationToken);

        IEnumerable<PatientHealthcareProfessionalListRow> filtered = ApplyPatientHealthcareProfessionalStatusFilter(
            combined,
            statuses);
        filtered = ApplyPatientHealthcareProfessionalSearch(filtered, searchQuery, searchField);
        filtered = ApplyPatientHealthcareProfessionalSort(filtered, sortField, ascending);

        int totalCount = filtered.Count();

        var page = filtered
            .Skip(skip)
            .Take(take)
            .ToList();

        return new SearchPatientHealthcareProfessionalsResult(page, totalCount);
    }

    public async Task<int> CountPatientHealthcareProfessionalsAsync(
        Guid patientUserId,
        string? searchQuery,
        PatientHealthcareProfessionalSearchField searchField,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses,
        CancellationToken cancellationToken)
    {
        List<PatientHealthcareProfessionalListRow> combined = await BuildPatientHealthcareProfessionalRowsAsync(
            patientUserId,
            cancellationToken);

        IEnumerable<PatientHealthcareProfessionalListRow> filtered = ApplyPatientHealthcareProfessionalStatusFilter(
            combined,
            statuses);
        filtered = ApplyPatientHealthcareProfessionalSearch(filtered, searchQuery, searchField);

        return filtered.Count();
    }

    private async Task<List<PatientHealthcareProfessionalListRow>> BuildPatientHealthcareProfessionalRowsAsync(
        Guid patientUserId,
        CancellationToken cancellationToken)
    {
        var providerProfiles = await context.HealthcareProviderProfiles
            .AsNoTracking()
            .Join(context.UserProfiles.AsNoTracking(),
                provider => provider.UserId,
                profile => profile.Id,
                (provider, profile) => new
                {
                    provider.UserId,
                    profile.Email,
                    profile.FirstName,
                    profile.LastName,
                    profile.ProfilePhotoStorageKey,
                    provider.ProfessionCode,
                    provider.OtherProfessionLabel,
                    provider.WorkplaceLabel,
                    provider.ShortDescription
                })
            .ToDictionaryAsync(x => x.UserId, cancellationToken);

        Dictionary<Guid, int> pendingProposalCounts = await context.MedicalChangeProposals
            .AsNoTracking()
            .Where(p => p.PatientUserId == patientUserId && p.ProposalState == MedicalChangeProposalState.Pending)
            .GroupBy(p => p.HealthcareProfessionalUserId)
            .Select(g => new { ProviderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProviderId, x => x.Count, cancellationToken);

        var requestRows = await context.SupervisionRequests
            .AsNoTracking()
            .Where(x => x.PatientUserId == patientUserId && x.RequestState == SupervisionRequestState.Pending)
            .Select(r => new
            {
                r.Id,
                r.HealthcareProfessionalUserId,
                r.RequestState,
                r.RequestedAtUtc
            })
            .ToListAsync(cancellationToken);

        var connectionRows = await context.SupervisionConnections
            .AsNoTracking()
            .Where(x => x.PatientUserId == patientUserId
                        && x.ConnectionState == SupervisionConnectionState.Active)
            .Select(c => new
            {
                c.Id,
                c.HealthcareProfessionalUserId,
                c.ConnectionState,
                c.AccessLevelCode,
                c.ApprovedAtUtc
            })
            .ToListAsync(cancellationToken);

        var combined = new List<PatientHealthcareProfessionalListRow>();

        foreach (var r in requestRows)
        {
            providerProfiles.TryGetValue(r.HealthcareProfessionalUserId, out var profile);
            pendingProposalCounts.TryGetValue(r.HealthcareProfessionalUserId, out int pendingProposals);

            combined.Add(new PatientHealthcareProfessionalListRow(
                EntryType: "pending_request",
                HealthcareProfessionalUserId: r.HealthcareProfessionalUserId,
                RequestId: r.Id,
                ConnectionId: null,
                RelationshipState: r.RequestState.ToString(),
                AccessLevelCode: null,
                Email: profile?.Email ?? string.Empty,
                FirstName: profile?.FirstName ?? string.Empty,
                LastName: profile?.LastName ?? string.Empty,
                ProfessionLabel: profile?.ProfessionCode?.ToString() ?? profile?.OtherProfessionLabel,
                WorkplaceLabel: profile?.WorkplaceLabel,
                ShortDescription: profile?.ShortDescription,
                HasPendingProposal: pendingProposals > 0,
                PendingProposalCount: pendingProposals == 0 ? null : pendingProposals,
                RequestedAtUtc: r.RequestedAtUtc,
                ApprovedAtUtc: null,
                ProfilePhotoStorageKey: profile?.ProfilePhotoStorageKey));
        }

        foreach (var c in connectionRows)
        {
            providerProfiles.TryGetValue(c.HealthcareProfessionalUserId, out var profile);
            pendingProposalCounts.TryGetValue(c.HealthcareProfessionalUserId, out int pendingProposals);

            combined.Add(new PatientHealthcareProfessionalListRow(
                EntryType: "connection",
                HealthcareProfessionalUserId: c.HealthcareProfessionalUserId,
                RequestId: null,
                ConnectionId: c.Id,
                RelationshipState: c.ConnectionState.ToString(),
                AccessLevelCode: c.AccessLevelCode.ToString(),
                Email: profile?.Email ?? string.Empty,
                FirstName: profile?.FirstName ?? string.Empty,
                LastName: profile?.LastName ?? string.Empty,
                ProfessionLabel: profile?.ProfessionCode?.ToString() ?? profile?.OtherProfessionLabel,
                WorkplaceLabel: profile?.WorkplaceLabel,
                ShortDescription: profile?.ShortDescription,
                HasPendingProposal: pendingProposals > 0,
                PendingProposalCount: pendingProposals == 0 ? null : pendingProposals,
                RequestedAtUtc: c.ApprovedAtUtc,
                ApprovedAtUtc: c.ApprovedAtUtc,
                ProfilePhotoStorageKey: profile?.ProfilePhotoStorageKey));
        }

        return combined;
    }

    private static IEnumerable<PatientHealthcareProfessionalListRow> ApplyPatientHealthcareProfessionalStatusFilter(
        IEnumerable<PatientHealthcareProfessionalListRow> rows,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses)
    {
        if (statuses is null || statuses.Count == 0)
        {
            return rows;
        }

        var selected = statuses.ToHashSet();
        return rows.Where(row =>
            selected.Contains(PatientHealthcareProfessionalSearchCatalog.MapRelationshipState(row.RelationshipState)));
    }

    private static IEnumerable<PatientHealthcareProfessionalListRow> ApplyPatientHealthcareProfessionalSearch(
        IEnumerable<PatientHealthcareProfessionalListRow> rows,
        string? searchQuery,
        PatientHealthcareProfessionalSearchField searchField)
    {
        if (string.IsNullOrWhiteSpace(searchQuery) || searchField == PatientHealthcareProfessionalSearchField.None)
        {
            return rows;
        }

        string trimmedQuery = searchQuery.Trim();
        return rows.Where(row => MatchesPatientHealthcareProfessionalSearch(row, trimmedQuery, searchField));
    }

    private static bool MatchesPatientHealthcareProfessionalSearch(
        PatientHealthcareProfessionalListRow row,
        string searchQuery,
        PatientHealthcareProfessionalSearchField searchField)
    {
        return searchField switch
        {
            PatientHealthcareProfessionalSearchField.Email =>
                row.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase),
            PatientHealthcareProfessionalSearchField.DisplayName =>
                row.FirstName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                || row.LastName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                || $"{row.FirstName} {row.LastName}".Contains(searchQuery, StringComparison.OrdinalIgnoreCase),
            PatientHealthcareProfessionalSearchField.ProfessionLabel =>
                row.ProfessionLabel?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ?? false,
            PatientHealthcareProfessionalSearchField.WorkplaceLabel =>
                row.WorkplaceLabel?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ?? false,
            _ => true
        };
    }

    private static IEnumerable<PatientHealthcareProfessionalListRow> ApplyPatientHealthcareProfessionalSort(
        IEnumerable<PatientHealthcareProfessionalListRow> rows,
        PatientHealthcareProfessionalSortField sortField,
        bool ascending)
    {
        Func<PatientHealthcareProfessionalListRow, int> statusPriority = row =>
        {
            if (row.EntryType == "pending_request")
            {
                return 0;
            }

            return row.AccessLevelCode?.ToLower() switch
            {
                "view_and_modify" => 1,
                "view_only" => 2,
                "restricted_view" => 3,
                "block_access" => 4,
                _ => 5
            };
        };

        return sortField switch
        {
            PatientHealthcareProfessionalSortField.DateAdded => ascending
                ? rows.OrderBy(x => x.RequestedAtUtc).ThenBy(x => x.HealthcareProfessionalUserId)
                : rows.OrderByDescending(x => x.RequestedAtUtc).ThenBy(x => x.HealthcareProfessionalUserId),
            PatientHealthcareProfessionalSortField.DisplayName => ascending
                ? rows.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.HealthcareProfessionalUserId)
                : rows.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName)
                    .ThenBy(x => x.HealthcareProfessionalUserId),
            _ => ascending
                ? rows.OrderBy(statusPriority).ThenBy(x => x.LastName).ThenBy(x => x.FirstName)
                : rows.OrderByDescending(statusPriority).ThenByDescending(x => x.LastName)
                    .ThenByDescending(x => x.FirstName)
        };
    }

    public async Task<PatientHealthcareProfessionalDetail?> GetPatientHealthcareProfessionalDetailAsync(Guid patientUserId, Guid healthcareProfessionalUserId, CancellationToken cancellationToken)
    {
        var profileJoin = await context.HealthcareProviderProfiles.AsNoTracking()
            .Join(context.UserProfiles.AsNoTracking(),
                provider => provider.UserId,
                profile => profile.Id,
                (provider, profile) => new
                {
                    provider.UserId,
                    profile.FirstName,
                    profile.LastName,
                    profile.ProfilePhotoStorageKey,
                    provider.ProfessionCode,
                    provider.OtherProfessionLabel,
                    provider.WorkplaceLabel,
                    provider.ShortDescription
                })
            .SingleOrDefaultAsync(x => x.UserId == healthcareProfessionalUserId, cancellationToken);

        SupervisionRequest? request = await context.SupervisionRequests.AsNoTracking()
            .Where(x => x.PatientUserId == patientUserId
                        && x.HealthcareProfessionalUserId == healthcareProfessionalUserId
                        && x.RequestState == SupervisionRequestState.Pending)
            .OrderByDescending(x => x.RequestedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        SupervisionConnection? connection = await context.SupervisionConnections.AsNoTracking()
            .Where(x => x.PatientUserId == patientUserId
                        && x.HealthcareProfessionalUserId == healthcareProfessionalUserId
                        && x.ConnectionState == SupervisionConnectionState.Active)
            .OrderByDescending(x => x.ApprovedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (request is null && connection is null)
        {
            return null;
        }

        int pendingProposalCount = await context.MedicalChangeProposals.AsNoTracking()
            .Where(p => p.PatientUserId == patientUserId
                        && p.HealthcareProfessionalUserId == healthcareProfessionalUserId
                        && p.ProposalState == MedicalChangeProposalState.Pending)
            .CountAsync(cancellationToken);

        return new PatientHealthcareProfessionalDetail(
            HealthcareProfessionalUserId: healthcareProfessionalUserId,
            ProfilePhotoStorageKey: profileJoin?.ProfilePhotoStorageKey,
            FirstName: profileJoin?.FirstName ?? string.Empty,
            LastName: profileJoin?.LastName ?? string.Empty,
            ProfessionLabel: profileJoin?.ProfessionCode?.ToString() ?? profileJoin?.OtherProfessionLabel,
            WorkplaceLabel: profileJoin?.WorkplaceLabel,
            ShortDescription: profileJoin?.ShortDescription,
            RelationshipState: connection?.ConnectionState.ToString() ?? request!.RequestState.ToString(),
            AccessLevelCode: connection?.AccessLevelCode.ToString(),
            HasPendingProposal: pendingProposalCount > 0,
            PendingProposalCount: pendingProposalCount == 0 ? null : pendingProposalCount,
            ActiveRequestId: request?.Id,
            ActiveConnectionId: connection?.Id);
    }

    public async Task<SearchProviderPatientsResult> SearchProviderPatientsAsync(
        Guid providerUserId,
        string? searchQuery,
        ProviderPatientSearchField searchField,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses,
        ProviderPatientSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        List<ProviderPatientListRow> combined = await BuildProviderPatientRowsAsync(providerUserId, cancellationToken);

        IEnumerable<ProviderPatientListRow> filtered = ApplyProviderPatientStatusFilter(combined, statuses);
        filtered = ApplyProviderPatientSearch(filtered, searchQuery, searchField);
        filtered = ApplyProviderPatientSort(filtered, sortField, ascending);

        int totalCount = filtered.Count();

        var page = filtered
            .Skip(skip)
            .Take(take)
            .ToList();

        return new SearchProviderPatientsResult(page, totalCount);
    }

    public async Task<int> CountProviderPatientsAsync(
        Guid providerUserId,
        string? searchQuery,
        ProviderPatientSearchField searchField,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses,
        CancellationToken cancellationToken)
    {
        List<ProviderPatientListRow> combined = await BuildProviderPatientRowsAsync(providerUserId, cancellationToken);

        IEnumerable<ProviderPatientListRow> filtered = ApplyProviderPatientStatusFilter(combined, statuses);
        filtered = ApplyProviderPatientSearch(filtered, searchQuery, searchField);

        return filtered.Count();
    }

    private async Task<List<ProviderPatientListRow>> BuildProviderPatientRowsAsync(
        Guid providerUserId,
        CancellationToken cancellationToken)
    {
        var patientProfiles = await context.UserProfiles.AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Email,
                p.FirstName,
                p.LastName,
                p.ProfilePhotoStorageKey
            })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var requests = await context.SupervisionRequests
            .AsNoTracking()
            .Where(x => x.HealthcareProfessionalUserId == providerUserId
                        && x.RequestState == SupervisionRequestState.Pending)
            .Select(r => new
            {
                r.Id,
                r.PatientUserId,
                r.RequestState,
                r.RequestedAtUtc
            })
            .ToListAsync(cancellationToken);

        var connections = await context.SupervisionConnections
            .AsNoTracking()
            .Where(x => x.HealthcareProfessionalUserId == providerUserId
                        && x.ConnectionState == SupervisionConnectionState.Active)
            .Select(c => new
            {
                c.Id,
                c.PatientUserId,
                c.ConnectionState,
                c.AccessLevelCode,
                c.ApprovedAtUtc
            })
            .ToListAsync(cancellationToken);

        var combined = new List<ProviderPatientListRow>();

        foreach (var r in requests)
        {
            patientProfiles.TryGetValue(r.PatientUserId, out var profile);

            combined.Add(new ProviderPatientListRow(
                EntryType: "pending_request",
                PatientUserId: r.PatientUserId,
                RequestId: r.Id,
                ConnectionId: null,
                RelationshipState: r.RequestState.ToString(),
                AccessLevelCode: null,
                Email: profile?.Email ?? string.Empty,
                FirstName: profile?.FirstName ?? string.Empty,
                LastName: profile?.LastName ?? string.Empty,
                HasProfilePhoto: profile?.ProfilePhotoStorageKey is not null,
                RequestedAtUtc: r.RequestedAtUtc,
                ApprovedAtUtc: null));
        }

        foreach (var c in connections)
        {
            patientProfiles.TryGetValue(c.PatientUserId, out var profile);

            combined.Add(new ProviderPatientListRow(
                EntryType: "connection",
                PatientUserId: c.PatientUserId,
                RequestId: null,
                ConnectionId: c.Id,
                RelationshipState: c.ConnectionState.ToString(),
                AccessLevelCode: c.AccessLevelCode.ToString(),
                Email: profile?.Email ?? string.Empty,
                FirstName: profile?.FirstName ?? string.Empty,
                LastName: profile?.LastName ?? string.Empty,
                HasProfilePhoto: profile?.ProfilePhotoStorageKey is not null,
                RequestedAtUtc: c.ApprovedAtUtc,
                ApprovedAtUtc: c.ApprovedAtUtc));
        }

        return combined;
    }

    private static IEnumerable<ProviderPatientListRow> ApplyProviderPatientStatusFilter(
        IEnumerable<ProviderPatientListRow> rows,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses)
    {
        if (statuses is null || statuses.Count == 0)
        {
            return rows;
        }

        var selected = statuses.ToHashSet();
        return rows.Where(row =>
            selected.Contains(ProviderPatientSearchCatalog.MapRelationshipState(row.RelationshipState)));
    }

    private static IEnumerable<ProviderPatientListRow> ApplyProviderPatientSearch(
        IEnumerable<ProviderPatientListRow> rows,
        string? searchQuery,
        ProviderPatientSearchField searchField)
    {
        if (string.IsNullOrWhiteSpace(searchQuery) || searchField == ProviderPatientSearchField.None)
        {
            return rows;
        }

        string trimmedQuery = searchQuery.Trim();
        return rows.Where(row => MatchesProviderPatientSearch(row, trimmedQuery, searchField));
    }

    private static bool MatchesProviderPatientSearch(
        ProviderPatientListRow row,
        string searchQuery,
        ProviderPatientSearchField searchField)
    {
        return searchField switch
        {
            ProviderPatientSearchField.Email =>
                row.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase),
            ProviderPatientSearchField.DisplayName =>
                row.FirstName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                || row.LastName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                || $"{row.FirstName} {row.LastName}".Contains(searchQuery, StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }

    private static IEnumerable<ProviderPatientListRow> ApplyProviderPatientSort(
        IEnumerable<ProviderPatientListRow> rows,
        ProviderPatientSortField sortField,
        bool ascending)
    {
        Func<ProviderPatientListRow, int> statusPriority = row =>
        {
            if (row.EntryType == "pending_request")
            {
                return 0;
            }

            return row.AccessLevelCode?.ToLower() switch
            {
                "view_and_modify" => 1,
                "view_only" => 2,
                "restricted_view" => 3,
                "block_access" => 4,
                _ => 5
            };
        };

        return sortField switch
        {
            ProviderPatientSortField.DateAdded => ascending
                ? rows.OrderBy(x => x.RequestedAtUtc).ThenBy(x => x.PatientUserId)
                : rows.OrderByDescending(x => x.RequestedAtUtc).ThenBy(x => x.PatientUserId),
            ProviderPatientSortField.DisplayName => ascending
                ? rows.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.PatientUserId)
                : rows.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName)
                    .ThenBy(x => x.PatientUserId),
            _ => ascending
                ? rows.OrderBy(statusPriority).ThenBy(x => x.LastName).ThenBy(x => x.FirstName)
                : rows.OrderByDescending(statusPriority).ThenByDescending(x => x.LastName)
                    .ThenByDescending(x => x.FirstName)
        };
    }

    public async Task<IReadOnlyCollection<PatientSupervisionRequestListRow>> ListPatientSupervisionRequestsAsync(
        Guid patientUserId,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var providerProfiles = await context.HealthcareProviderProfiles
            .AsNoTracking()
            .Join(context.UserProfiles.AsNoTracking(),
                provider => provider.UserId,
                profile => profile.Id,
                (provider, profile) => new
                {
                    provider.UserId,
                    profile.FirstName,
                    profile.LastName,
                    profile.ProfilePhotoStorageKey,
                    provider.ProfessionCode,
                    provider.OtherProfessionLabel,
                    provider.WorkplaceLabel,
                    provider.ShortDescription
                })
            .ToDictionaryAsync(x => x.UserId, cancellationToken);

        IQueryable<SupervisionRequest> query = context.SupervisionRequests
            .AsNoTracking()
            .Where(x => x.PatientUserId == patientUserId);

        query = ascending
            ? query.OrderBy(x => x.RequestedAtUtc).ThenBy(x => x.Id)
            : query.OrderByDescending(x => x.RequestedAtUtc).ThenByDescending(x => x.Id);

        var requests = await query
            .Skip(skip)
            .Take(take + 1)
            .Select(r => new
            {
                r.Id,
                r.HealthcareProfessionalUserId,
                r.RequestState,
                r.RequestedAtUtc
            })
            .ToListAsync(cancellationToken);

        return requests
            .Select(r =>
            {
                providerProfiles.TryGetValue(r.HealthcareProfessionalUserId, out var profile);

                return new PatientSupervisionRequestListRow(
                    RequestId: r.Id,
                    HealthcareProfessionalUserId: r.HealthcareProfessionalUserId,
                    RequestState: r.RequestState,
                    RequestedAtUtc: r.RequestedAtUtc,
                    ProfilePhotoStorageKey: profile?.ProfilePhotoStorageKey,
                    FirstName: profile?.FirstName ?? string.Empty,
                    LastName: profile?.LastName ?? string.Empty,
                    ProfessionLabel: profile?.ProfessionCode?.ToString() ?? profile?.OtherProfessionLabel,
                    WorkplaceLabel: profile?.WorkplaceLabel,
                    ShortDescription: profile?.ShortDescription);
            })
            .ToList();
    }

    public Task<int> CountPatientSupervisionRequestsAsync(
        Guid patientUserId,
        CancellationToken cancellationToken) =>
        context.SupervisionRequests
            .AsNoTracking()
            .CountAsync(x => x.PatientUserId == patientUserId, cancellationToken);

    public async Task<IReadOnlyCollection<ProviderSupervisionRequestListRow>> ListProviderSupervisionRequestsAsync(
        Guid providerUserId,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<SupervisionRequest> query = context.SupervisionRequests
            .AsNoTracking()
            .Where(x => x.HealthcareProfessionalUserId == providerUserId
                        && x.RequestState == SupervisionRequestState.Pending);

        query = ascending
            ? query.OrderBy(x => x.RequestedAtUtc).ThenBy(x => x.Id)
            : query.OrderByDescending(x => x.RequestedAtUtc).ThenByDescending(x => x.Id);

        var requests = await query
            .Skip(skip)
            .Take(take + 1)
            .Select(r => new
            {
                r.Id,
                r.PatientUserId,
                r.RequestState,
                r.RequestedAtUtc,
                r.ExpiresAtUtc
            })
            .ToListAsync(cancellationToken);

        if (requests.Count == 0)
        {
            return Array.Empty<ProviderSupervisionRequestListRow>();
        }

        var patientIds = requests.Select(r => r.PatientUserId).Distinct().ToList();
        var patientProfiles = await context.UserProfiles
            .AsNoTracking()
            .Where(profile => patientIds.Contains(profile.Id))
            .Select(profile => new
            {
                profile.Id,
                profile.FirstName,
                profile.LastName,
                profile.ProfilePhotoStorageKey
            })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return requests
            .Select(r =>
            {
                patientProfiles.TryGetValue(r.PatientUserId, out var profile);

                return new ProviderSupervisionRequestListRow(
                    RequestId: r.Id,
                    PatientUserId: r.PatientUserId,
                    RequestState: r.RequestState,
                    RequestedAtUtc: r.RequestedAtUtc,
                    ExpiresAtUtc: r.ExpiresAtUtc,
                    ProfilePhotoStorageKey: profile?.ProfilePhotoStorageKey,
                    FirstName: profile?.FirstName ?? string.Empty,
                    LastName: profile?.LastName ?? string.Empty);
            })
            .ToList();
    }

    public Task<int> CountProviderSupervisionRequestsAsync(
        Guid providerUserId,
        CancellationToken cancellationToken) =>
        context.SupervisionRequests
            .AsNoTracking()
            .CountAsync(
                x => x.HealthcareProfessionalUserId == providerUserId
                     && x.RequestState == SupervisionRequestState.Pending,
                cancellationToken);
}
