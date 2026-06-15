using DrugsApp.Domain.PatientSupervisionFeature;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.PatientSupervisionFeature;

public sealed class MedicalChangeProposalRepository(DrugsAppDbContext context) : IMedicalChangeProposalRepository
{
    public async Task<IReadOnlyCollection<MedicalChangeProposalListRow>> SearchForPatientAsync(
        Guid patientUserId,
        Guid? providerUserId,
        string? state,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        IQueryable<MedicalChangeProposal> query = context.MedicalChangeProposals
            .AsNoTracking()
            .Where(x => x.PatientUserId == patientUserId);

        if (providerUserId is Guid providerId)
        {
            query = query.Where(x => x.HealthcareProfessionalUserId == providerId);
        }

        if (!string.IsNullOrWhiteSpace(state))
        {
            query = query.Where(x => x.ProposalState.ToString().ToLower() == state.ToLower());
        }

        List<MedicalChangeProposalListRow> rows = await query
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ThenBy(x => x.Id)
            .Skip(skip)
            .Take(take + 1)
            .Select(x => new MedicalChangeProposalListRow(
                x.Id,
                x.HealthcareProfessionalUserId,
                x.ProposalState.ToString(),
                x.ProposalSummary,
                x.SubmittedAtUtc))
            .ToListAsync(cancellationToken);

        return rows;
    }

    public async Task<MedicalChangeProposalDetail?> GetDetailForPatientAsync(
        Guid patientUserId,
        Guid proposalId,
        CancellationToken cancellationToken) =>
        await LoadDetailAsync(patientUserId, proposalId, participantUserId: null, cancellationToken);

    public async Task<MedicalChangeProposalDetail?> GetDetailForParticipantAsync(
        Guid userId,
        Guid proposalId,
        CancellationToken cancellationToken) =>
        await LoadDetailAsync(patientUserId: null, proposalId, participantUserId: userId, cancellationToken);

    public async Task<MedicalChangeProposal?> GetTrackedDetailForPatientAsync(
        Guid patientUserId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        return await context.MedicalChangeProposals
            .Include(p => p.Items)
            .ThenInclude(i => i.IntakeTimes)
            .SingleOrDefaultAsync(
                x => x.Id == proposalId && x.PatientUserId == patientUserId,
                cancellationToken);
    }

    public async Task<MedicalChangeProposal?> GetTrackedByIdForPatientAsync(
        Guid patientUserId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        return await context.MedicalChangeProposals
            .SingleOrDefaultAsync(
                x => x.Id == proposalId && x.PatientUserId == patientUserId,
                cancellationToken);
    }

    public async Task<int> InvalidatePendingByConnectionAsync(Guid connectionId, DateTime resolvedAtUtc, CancellationToken cancellationToken)
    {
        List<MedicalChangeProposal> pending = await context.MedicalChangeProposals
            .Where(x => x.SupervisionConnectionId == connectionId && x.ProposalState == Domain.PatientSupervisionFeature.Enums.MedicalChangeProposalState.Pending)
            .ToListAsync(cancellationToken);

        foreach (MedicalChangeProposal proposal in pending)
        {
            proposal.Invalidate(resolvedAtUtc);
        }

        return pending.Count;
    }

    public async Task<int> DeleteByConnectionAsync(Guid connectionId, CancellationToken cancellationToken)
    {
        List<MedicalChangeProposal> proposals = await context.MedicalChangeProposals
            .Where(x => x.SupervisionConnectionId == connectionId)
            .ToListAsync(cancellationToken);

        if (proposals.Count == 0)
        {
            return 0;
        }

        context.MedicalChangeProposals.RemoveRange(proposals);
        return proposals.Count;
    }

    public async Task AddAsync(MedicalChangeProposal proposal, CancellationToken cancellationToken)
    {
        await context.MedicalChangeProposals.AddAsync(proposal, cancellationToken);
    }

    private async Task<MedicalChangeProposalDetail?> LoadDetailAsync(
        Guid? patientUserId,
        Guid proposalId,
        Guid? participantUserId,
        CancellationToken cancellationToken)
    {
        IQueryable<MedicalChangeProposal> query = context.MedicalChangeProposals.AsNoTracking();

        if (patientUserId is Guid patientId)
        {
            query = query.Where(x => x.Id == proposalId && x.PatientUserId == patientId);
        }
        else if (participantUserId is Guid participantId)
        {
            query = query.Where(x => x.Id == proposalId
                                     && (x.PatientUserId == participantId
                                         || x.HealthcareProfessionalUserId == participantId));
        }
        else
        {
            return null;
        }

        MedicalChangeProposal? proposal = await query.SingleOrDefaultAsync(cancellationToken);
        if (proposal is null)
        {
            return null;
        }

        List<MedicalChangeProposalItem> items = await context.MedicalChangeProposalItems
            .AsNoTracking()
            .Where(x => x.ProposalId == proposal.Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        IReadOnlyCollection<MedicalChangeProposalDraftItemDto> draftItems = await MapDraftItemsAsync(items, cancellationToken);

        return new MedicalChangeProposalDetail(
            proposal.Id,
            proposal.SupervisionConnectionId,
            proposal.PatientUserId,
            proposal.HealthcareProfessionalUserId,
            proposal.ProposalState.ToString(),
            proposal.ProposalSummary,
            proposal.SubmittedAtUtc,
            draftItems);
    }

    private async Task<IReadOnlyCollection<MedicalChangeProposalDraftItemDto>> MapDraftItemsAsync(
        IReadOnlyCollection<MedicalChangeProposalItem> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return Array.Empty<MedicalChangeProposalDraftItemDto>();
        }

        var itemIds = items.Select(x => x.Id).ToList();
        List<MedicalChangeProposalItemIntakeTime> intakeTimes = await context.MedicalChangeProposalItemIntakeTimes
            .AsNoTracking()
            .Where(x => itemIds.Contains(x.ProposalItemId))
            .OrderBy(x => x.IntakeTime)
            .ToListAsync(cancellationToken);

        var intakeLookup = intakeTimes
            .GroupBy(x => x.ProposalItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<TimeOnly>)group.Select(x => x.IntakeTime).ToList());

        return items
            .Select(item => new MedicalChangeProposalDraftItemDto(
                CategoryCode: MedicalChangeProposalCategoryCatalog.ToCategoryToken(item.CategoryCode),
                Source: MedicalChangeProposalCategoryCatalog.ToSourceToken(item.Source),
                RelatedCatalogId: item.RelatedCatalogId,
                CustomValue: item.CustomValue,
                DisplayName: item.DisplayNameSnapshot,
                StartDate: item.StartDate,
                EndDate: item.EndDate,
                Note: item.Note,
                ApplyPayload: item.ApplyPayload,
                IntakeTimes: intakeLookup.TryGetValue(item.Id, out IReadOnlyCollection<TimeOnly>? times)
                    ? times
                    : Array.Empty<TimeOnly>()))
            .ToList();
    }
}
