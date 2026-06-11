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

    public async Task<MedicalChangeProposalDetail?> GetDetailForPatientAsync(Guid patientUserId, Guid proposalId, CancellationToken cancellationToken)
    {
        MedicalChangeProposal? proposal = await context.MedicalChangeProposals
            .AsNoTracking()
            .Include(p => p.Items)
            .ThenInclude(i => i.IntakeTimes)
            .SingleOrDefaultAsync(
                x => x.Id == proposalId && x.PatientUserId == patientUserId,
                cancellationToken);

        if (proposal is null)
        {
            return null;
        }

        return MapDetail(proposal);
    }

    public async Task<MedicalChangeProposalDetail?> GetDetailForParticipantAsync(Guid userId, Guid proposalId, CancellationToken cancellationToken)
    {
        MedicalChangeProposal? proposal = await context.MedicalChangeProposals
            .AsNoTracking()
            .Include(p => p.Items)
            .ThenInclude(i => i.IntakeTimes)
            .SingleOrDefaultAsync(
                x => x.Id == proposalId
                     && (x.PatientUserId == userId || x.HealthcareProfessionalUserId == userId),
                cancellationToken);

        if (proposal is null)
        {
            return null;
        }

        return MapDetail(proposal);
    }

    public async Task<MedicalChangeProposal?> GetTrackedDetailForPatientAsync(Guid patientUserId, Guid proposalId, CancellationToken cancellationToken)
    {
        return await context.MedicalChangeProposals
            .Include(p => p.Items)
            .ThenInclude(i => i.IntakeTimes)
            .SingleOrDefaultAsync(
                x => x.Id == proposalId && x.PatientUserId == patientUserId,
                cancellationToken);
    }

    public async Task<MedicalChangeProposal?> GetTrackedByIdForPatientAsync(Guid patientUserId, Guid proposalId, CancellationToken cancellationToken)
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

    public async Task AddAsync(MedicalChangeProposal proposal, CancellationToken cancellationToken)
    {
        await context.MedicalChangeProposals.AddAsync(proposal, cancellationToken);
    }

    private static MedicalChangeProposalDetail MapDetail(MedicalChangeProposal proposal)
    {
        var sections = proposal.Items
            .GroupBy(x => x.CategoryCode.ToString())
            .Select(group => new MedicalChangeProposalSectionDto(
                group.Key,
                group.Select(item => new MedicalChangeProposalSectionItemDto(
                    item.CategoryCode.ToString(),
                    item.Source.ToString(),
                    item.RelatedCatalogId,
                    item.CustomValue,
                    item.DisplayNameSnapshot,
                    item.SecondaryTextSnapshot,
                    item.StartDate,
                    item.EndDate,
                    item.Note,
                    item.IntakeTimes.Select(t => t.IntakeTime).ToList()))
                .ToList()))
            .ToList();

        return new MedicalChangeProposalDetail(
            proposal.Id,
            proposal.SupervisionConnectionId,
            proposal.PatientUserId,
            proposal.HealthcareProfessionalUserId,
            proposal.ProposalState.ToString(),
            proposal.ProposalSummary,
            proposal.SubmittedAtUtc,
            sections);
    }
}
