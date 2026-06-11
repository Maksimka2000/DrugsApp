using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.PatientSupervisionFeature;

public sealed class SupervisionReadRepository(DrugsAppDbContext context) : ISupervisionReadRepository
{
    public async Task<SearchPatientHealthcareProfessionalsResult> SearchPatientHealthcareProfessionalsAsync(
        Guid patientUserId,
        string? relationshipState,
        string? sortBy,
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

        var pendingProposalCounts = await context.MedicalChangeProposals
            .AsNoTracking()
            .Where(p => p.PatientUserId == patientUserId && p.ProposalState == MedicalChangeProposalState.Pending)
            .GroupBy(p => p.HealthcareProfessionalUserId)
            .Select(g => new { ProviderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProviderId, x => x.Count, cancellationToken);

        var requestRows = await context.SupervisionRequests
            .AsNoTracking()
            .Where(x => x.PatientUserId == patientUserId)
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
            .Where(x => x.PatientUserId == patientUserId)
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
                FirstName: profile?.FirstName ?? string.Empty,
                LastName: profile?.LastName ?? string.Empty,
                ProfessionLabel: profile?.ProfessionCode?.ToString() ?? profile?.OtherProfessionLabel,
                WorkplaceLabel: profile?.WorkplaceLabel,
                ShortDescription: profile?.ShortDescription,
                HasPendingProposal: pendingProposals > 0,
                PendingProposalCount: pendingProposals == 0 ? null : pendingProposals,
                RequestedAtUtc: r.RequestedAtUtc,
                ApprovedAtUtc: null,
                HasProfilePhoto: profile?.ProfilePhotoStorageKey is not null));
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
                FirstName: profile?.FirstName ?? string.Empty,
                LastName: profile?.LastName ?? string.Empty,
                ProfessionLabel: profile?.ProfessionCode?.ToString() ?? profile?.OtherProfessionLabel,
                WorkplaceLabel: profile?.WorkplaceLabel,
                ShortDescription: profile?.ShortDescription,
                HasPendingProposal: pendingProposals > 0,
                PendingProposalCount: pendingProposals == 0 ? null : pendingProposals,
                RequestedAtUtc: c.ApprovedAtUtc,
                ApprovedAtUtc: c.ApprovedAtUtc,
                HasProfilePhoto: profile?.ProfilePhotoStorageKey is not null));
        }

        IEnumerable<PatientHealthcareProfessionalListRow> filtered = combined;
        if (!string.IsNullOrWhiteSpace(relationshipState) && !relationshipState.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            string state = relationshipState.ToLower();
            filtered = filtered.Where(x => x.RelationshipState.ToLower() == state);
        }

        int totalCount = filtered.Count();

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

        filtered = (sortBy ?? "status") switch
        {
            "date_added" => ascending
                ? filtered.OrderBy(x => x.RequestedAtUtc).ThenBy(x => x.HealthcareProfessionalUserId)
                : filtered.OrderByDescending(x => x.RequestedAtUtc).ThenBy(x => x.HealthcareProfessionalUserId),
            "display_name" => ascending
                ? filtered.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
                : filtered.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName),
            _ => ascending
                ? filtered.OrderBy(statusPriority).ThenBy(x => x.LastName).ThenBy(x => x.FirstName)
                : filtered.OrderByDescending(statusPriority).ThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName)
        };

        var page = filtered
            .Skip(skip)
            .Take(take + 1)
            .ToList();

        return new SearchPatientHealthcareProfessionalsResult(page, totalCount);
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

        var request = await context.SupervisionRequests.AsNoTracking()
            .Where(x => x.PatientUserId == patientUserId && x.HealthcareProfessionalUserId == healthcareProfessionalUserId)
            .OrderByDescending(x => x.RequestedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var connection = await context.SupervisionConnections.AsNoTracking()
            .Where(x => x.PatientUserId == patientUserId && x.HealthcareProfessionalUserId == healthcareProfessionalUserId)
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
            HasProfilePhoto: profileJoin?.ProfilePhotoStorageKey is not null,
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
        string? relationshipState,
        string? sortBy,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var patientProfiles = await context.UserProfiles.AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.ProfilePhotoStorageKey
            })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var requests = await context.SupervisionRequests.AsNoTracking()
            .Where(x => x.HealthcareProfessionalUserId == providerUserId)
            .Select(r => new
            {
                r.Id,
                r.PatientUserId,
                r.RequestState,
                r.RequestedAtUtc
            })
            .ToListAsync(cancellationToken);

        var connections = await context.SupervisionConnections.AsNoTracking()
            .Where(x => x.HealthcareProfessionalUserId == providerUserId)
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
                FirstName: profile?.FirstName ?? string.Empty,
                LastName: profile?.LastName ?? string.Empty,
                HasProfilePhoto: profile?.ProfilePhotoStorageKey is not null,
                RequestedAtUtc: c.ApprovedAtUtc,
                ApprovedAtUtc: c.ApprovedAtUtc));
        }

        IEnumerable<ProviderPatientListRow> filtered = combined;
        if (!string.IsNullOrWhiteSpace(relationshipState) && !relationshipState.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            string state = relationshipState.ToLower();
            filtered = filtered.Where(x => x.RelationshipState.ToLower() == state);
        }

        int totalCount = filtered.Count();

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

        filtered = (sortBy ?? "status") switch
        {
            "date_added" => ascending
                ? filtered.OrderBy(x => x.RequestedAtUtc).ThenBy(x => x.PatientUserId)
                : filtered.OrderByDescending(x => x.RequestedAtUtc).ThenBy(x => x.PatientUserId),
            "patient_name" => ascending
                ? filtered.OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
                : filtered.OrderByDescending(x => x.LastName).ThenByDescending(x => x.FirstName),
            _ => ascending
                ? filtered.OrderBy(statusPriority).ThenBy(x => x.LastName).ThenBy(x => x.FirstName)
                : filtered.OrderByDescending(statusPriority).ThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName)
        };

        var page = filtered
            .Skip(skip)
            .Take(take + 1)
            .ToList();

        return new SearchProviderPatientsResult(page, totalCount);
    }
}
