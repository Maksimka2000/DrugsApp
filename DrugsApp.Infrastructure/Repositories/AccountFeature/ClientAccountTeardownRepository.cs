using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AccountFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.AccountFeature;

/// <summary>
/// Removes client-owned rows across medical and supervision domains.
/// </summary>
public sealed class ClientAccountTeardownRepository(DrugsAppDbContext _context) : IClientAccountTeardownRepository
{
    public async Task DeleteAllOwnedClientDataAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await _context.MedicalChangeProposalItemIntakeTimes
            .Where(row => _context.MedicalChangeProposalItems
                .Where(item => _context.MedicalChangeProposals.Any(proposal =>
                    proposal.Id == item.ProposalId
                    && (proposal.PatientUserId == userId || proposal.HealthcareProfessionalUserId == userId)))
                .Select(item => item.Id)
                .Contains(row.ProposalItemId))
            .ExecuteDeleteAsync(cancellationToken);

        await _context.MedicalChangeProposalItems
            .Where(item => _context.MedicalChangeProposals.Any(proposal =>
                proposal.Id == item.ProposalId
                && (proposal.PatientUserId == userId || proposal.HealthcareProfessionalUserId == userId)))
            .ExecuteDeleteAsync(cancellationToken);

        await _context.MedicalChangeProposals
            .Where(proposal => proposal.PatientUserId == userId || proposal.HealthcareProfessionalUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.SupervisionConnections
            .Where(connection => connection.PatientUserId == userId || connection.HealthcareProfessionalUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.SupervisionRequests
            .Where(request => request.PatientUserId == userId || request.HealthcareProfessionalUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.UserNotifications
            .Where(notification => notification.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientRegularDrugIntakeTimes
            .Where(row => _context.ClientRegularDrugs
                .Where(drug => drug.UserId == userId)
                .Select(drug => drug.Id)
                .Contains(row.RegularDrugId))
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientRegularDrugs
            .Where(drug => drug.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientRegularSupplementIntakeTimes
            .Where(row => _context.ClientRegularSupplements
                .Where(supplement => supplement.UserId == userId)
                .Select(supplement => supplement.Id)
                .Contains(row.RegularSupplementId))
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientRegularSupplements
            .Where(supplement => supplement.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientOwnSupplementItems
            .Where(item => _context.ClientOwnSupplements
                .Where(own => own.UserId == userId)
                .Select(own => own.Id)
                .Contains(item.OwnSupplementId))
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientOwnSupplements
            .Where(own => own.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientRegularFoodIntakeTimes
            .Where(row => _context.ClientRegularFoods
                .Where(food => food.UserId == userId)
                .Select(food => food.Id)
                .Contains(row.RegularFoodId))
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientRegularFoods
            .Where(food => food.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientMealTimes
            .Where(mealTime => mealTime.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientDrugAllergies
            .Where(allergy => allergy.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientSupplementAllergies
            .Where(allergy => allergy.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientFoodAllergies
            .Where(allergy => allergy.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientOtherAllergies
            .Where(allergy => allergy.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.ClientAllergySummaries
            .Where(summary => summary.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.DrugProblems
            .Where(problem => problem.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        await _context.UserHealthConditions
            .Where(condition => condition.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
