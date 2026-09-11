using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.InteractionFeature.Entities;
using DrugsApp.Domain.InteractionFeature.Enums;
using DrugsApp.Domain.InteractionFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.InteractionFeature;

public sealed class InteractionRepository : IInteractionRepository
{
    private readonly DrugsAppDbContext _context;

    public InteractionRepository(DrugsAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Interaction>> SearchByPartiesAsync(
        InteractionKind interactionKind,
        InteractionPartyKey? leftPartyKey,
        InteractionPartyKey? rightPartyKey,
        InteractionSortBy sortBy,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<Interaction> query = _context.Interactions
            .AsNoTracking();

        // apply includes
        query = ApplyIncludes(query);

        // apply interaction kind filter
        query = ApplyInteractionKindFilter(query, interactionKind);
        // apply party filters
        query = ApplyPartyFilters(query, leftPartyKey, rightPartyKey);
        // apply sort
        query = ApplySort(query, sortBy, ascending);

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Interaction> ApplyIncludes(IQueryable<Interaction> query) =>
        query
            .Include(interaction => interaction.InteractionSeverity)
            .Include(interaction => interaction.Parties);

    public async Task<int> CountByPartiesAsync(
        InteractionKind interactionKind,
        InteractionPartyKey? leftPartyKey,
        InteractionPartyKey? rightPartyKey,
        CancellationToken cancellationToken)
    {
        IQueryable<Interaction> query = _context.Interactions.AsNoTracking();

        query = ApplyInteractionKindFilter(query, interactionKind);
        query = ApplyPartyFilters(query, leftPartyKey, rightPartyKey);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Interaction?> GetByIdAsync(Guid interactionId, CancellationToken cancellationToken)
    {
        return await _context.Interactions
            .Include(interaction => interaction.InteractionSeverity)
            .Include(interaction => interaction.Parties)
            .Include(interaction => interaction.ExpandedRows)
            .FirstOrDefaultAsync(interaction => interaction.Id == interactionId, cancellationToken);
    }

    public async Task<InteractionSeverity?> GetInteractionSeverityByIdAsync(
        Guid? interactionSeverityId,
        CancellationToken cancellationToken)
    {
        if (interactionSeverityId is null)
        {
            return null;
        }

        return await _context.InteractionSeverities
            .AsNoTracking()
            .FirstOrDefaultAsync(severity => severity.Id == interactionSeverityId, cancellationToken);
    }

    public async Task<Interaction?> GetByCanonicalPairAsync(
        InteractionPartyKey firstParty,
        InteractionPartyKey secondParty,
        CancellationToken cancellationToken)
    {
        IQueryable<Interaction> query = _context.Interactions
            .AsNoTracking()
            .Include(interaction => interaction.Parties);

        query = query
            .Where(BuildInteractionHasPartyPredicate(firstParty))
            .Where(BuildInteractionHasPartyPredicate(secondParty));

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Interaction interaction, CancellationToken cancellationToken)
    {
        await _context.Interactions.AddAsync(interaction, cancellationToken);
    }

    public void Remove(Interaction interaction)
    {
        _context.Interactions.Remove(interaction);
    }

    public async Task<bool> AnyPartyReferencesGroupAsync(
        InteractionType groupPartyType,
        Guid groupId,
        CancellationToken cancellationToken)
    {
        return await _context.InteractionParties
            .AsNoTracking()
            .AnyAsync(BuildGroupPartyPredicate(groupPartyType, groupId), cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> GetInteractionIdsByGroupPartyAsync(
        InteractionType groupPartyType,
        Guid groupId,
        CancellationToken cancellationToken)
    {
        List<Guid> ids = await _context.InteractionParties
            .AsNoTracking()
            .Where(BuildGroupPartyPredicate(groupPartyType, groupId))
            .Select(party => party.InteractionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return ids;
    }

    public async Task<Dictionary<Guid, IReadOnlyCollection<InteractionParty>>> GetPartiesByInteractionIdsAsync(
        IEnumerable<Guid> interactionIds,
        CancellationToken cancellationToken)
    {
        var interactionIdList = interactionIds.ToList();
        if (interactionIdList.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyCollection<InteractionParty>>();
        }

        List<InteractionParty> parties = await _context.InteractionParties
            .AsNoTracking()
            .Where(party => interactionIdList.Contains(party.InteractionId))
            .ToListAsync(cancellationToken);

        return parties
            .GroupBy(party => party.InteractionId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<InteractionParty>)group.ToList());
    }

    public async Task DeleteExpandedRowsByAtomicPairsAsync(
        Guid interactionId,
        IEnumerable<InteractionAtomicPair> atomicPairs,
        CancellationToken cancellationToken)
    {
        var pairList = atomicPairs.ToList();
        if (pairList.Count == 0)
        {
            return;
        }

        Expression<Func<InteractionExpanded, bool>> predicate = BuildAtomicPairsPredicate(interactionId, pairList);

        await _context.InteractionExpandedRows
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task InsertExpandedRowsIgnoreConflictsAsync(
        Guid interactionId,
        IEnumerable<InteractionAtomicPair> atomicPairs,
        CancellationToken cancellationToken)
    {
        var pairList = atomicPairs.ToList();
        if (pairList.Count == 0)
        {
            return;
        }

        List<InteractionAtomicPair> existingPairs = await _context.InteractionExpandedRows
            .AsNoTracking()
            .Where(row => row.InteractionId == interactionId)
            .Select(row => new InteractionAtomicPair(row.PartyTypeA, row.PartyIdA, row.PartyTypeB, row.PartyIdB))
            .ToListAsync(cancellationToken);

        HashSet<InteractionAtomicPair> existingSet = new(existingPairs);

        var rowsToInsert = pairList
            .Where(pair => !existingSet.Contains(pair))
            .Select(pair => InteractionExpanded.Create(
                interactionId,
                pair.PartyTypeA,
                pair.PartyIdA,
                pair.PartyTypeB,
                pair.PartyIdB))
            .ToList();

        if (rowsToInsert.Count == 0)
        {
            return;
        }

        await _context.InteractionExpandedRows.AddRangeAsync(rowsToInsert, cancellationToken);
    }

    private static IQueryable<Interaction> ApplyInteractionKindFilter(
        IQueryable<Interaction> query,
        InteractionKind interactionKind)
    {
        return interactionKind switch
        {
            InteractionKind.Substance => query.Where(interaction =>
                interaction.Parties.All(party =>
                    party.PartyType == InteractionType.Substance ||
                    party.PartyType == InteractionType.SubstanceGroup)),
            InteractionKind.Food => query.Where(interaction =>
                interaction.Parties.Any(party =>
                    party.PartyType == InteractionType.Substance ||
                    party.PartyType == InteractionType.SubstanceGroup) &&
                interaction.Parties.Any(party =>
                    party.PartyType == InteractionType.Food ||
                    party.PartyType == InteractionType.FoodGroup)),
            InteractionKind.Supplement => query.Where(interaction =>
                interaction.Parties.Any(party =>
                    party.PartyType == InteractionType.Substance ||
                    party.PartyType == InteractionType.SubstanceGroup) &&
                interaction.Parties.Any(party =>
                    party.PartyType == InteractionType.Supplement ||
                    party.PartyType == InteractionType.SupplementGroup)),
            _ => query
        };
    }

    private static IQueryable<Interaction> ApplyPartyFilters(
        IQueryable<Interaction> query,
        InteractionPartyKey? leftPartyKey,
        InteractionPartyKey? rightPartyKey)
    {
        // if both parties are null, return the query as is
        if (leftPartyKey is null && rightPartyKey is null)
        {
            return query;
        }

        // if both parties are not null, return the query with the parties filtered
        if (leftPartyKey is not null && rightPartyKey is not null)
        {
            InteractionPartyKey leftKey = leftPartyKey;
            InteractionPartyKey rightKey = rightPartyKey;

            return query
                .Where(BuildInteractionHasPartyPredicate(leftKey))
                .Where(BuildInteractionHasPartyPredicate(rightKey));
        }

        // if one party is not null, return the query with the party filtered
        InteractionPartyKey key = leftPartyKey ?? rightPartyKey!;

        // if one party is not null, return the query with the party filtered
        return query.Where(BuildInteractionHasPartyPredicate(key));
    }

    private static IQueryable<Interaction> ApplySort(
        IQueryable<Interaction> query,
        InteractionSortBy sortBy,
        bool ascending)
    {
        return (sortBy, ascending) switch
        {
            (InteractionSortBy.Severity, true) => query
                .OrderBy(interaction => interaction.InteractionSeverity.SeverityNumber)
                .ThenBy(interaction => interaction.Id),
            (InteractionSortBy.Severity, false) => query
                .OrderByDescending(interaction => interaction.InteractionSeverity.SeverityNumber)
                .ThenBy(interaction => interaction.Id),
            (InteractionSortBy.CreatedAt, true) => query
                .OrderBy(interaction => interaction.CreatedAtUtc)
                .ThenBy(interaction => interaction.Id),
            (InteractionSortBy.CreatedAt, false) => query
                .OrderByDescending(interaction => interaction.CreatedAtUtc)
                .ThenBy(interaction => interaction.Id),
            (InteractionSortBy.UpdatedAt, true) => query
                .OrderBy(interaction => interaction.UpdatedAtUtc)
                .ThenBy(interaction => interaction.Id),
            (InteractionSortBy.UpdatedAt, false) => query
                .OrderByDescending(interaction => interaction.UpdatedAtUtc)
                .ThenBy(interaction => interaction.Id),
            _ => query.OrderByDescending(interaction => interaction.CreatedAtUtc)
                .ThenBy(interaction => interaction.Id)
        };
    }

    private static Expression<Func<Interaction, bool>> BuildInteractionHasPartyPredicate(InteractionPartyKey key)
    {
        return interaction => interaction.Parties.Any(party =>
            party.PartyType == key.PartyType &&
            (
                key.PartyType == InteractionType.Substance && party.ActiveSubstanceId == key.PartyId ||
                key.PartyType == InteractionType.SubstanceGroup && party.ActiveSubstanceGroupId == key.PartyId ||
                key.PartyType == InteractionType.Food && party.FoodId == key.PartyId ||
                key.PartyType == InteractionType.FoodGroup && party.FoodGroupId == key.PartyId ||
                key.PartyType == InteractionType.Supplement && party.SupplementId == key.PartyId ||
                key.PartyType == InteractionType.SupplementGroup && party.SupplementGroupId == key.PartyId
            ));
    }

    private static Expression<Func<InteractionParty, bool>> BuildGroupPartyPredicate(
        InteractionType groupPartyType,
        Guid groupId) =>
        groupPartyType switch
        {
            InteractionType.SubstanceGroup => party =>
                party.PartyType == InteractionType.SubstanceGroup && party.ActiveSubstanceGroupId == groupId,
            InteractionType.FoodGroup => party =>
                party.PartyType == InteractionType.FoodGroup && party.FoodGroupId == groupId,
            InteractionType.SupplementGroup => party =>
                party.PartyType == InteractionType.SupplementGroup && party.SupplementGroupId == groupId,
            _ => party => false
        };

    private static Expression<Func<InteractionExpanded, bool>> BuildAtomicPairsPredicate(
        Guid interactionId,
        IReadOnlyCollection<InteractionAtomicPair> pairs)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(InteractionExpanded), "row");
        Expression interactionMatch = Expression.Equal(
            Expression.Property(parameter, nameof(InteractionExpanded.InteractionId)),
            Expression.Constant(interactionId));

        Expression? pairExpression = null;

        foreach (InteractionAtomicPair pair in pairs)
        {
            Expression current = Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(parameter, nameof(InteractionExpanded.PartyTypeA)),
                    Expression.Constant(pair.PartyTypeA)),
                Expression.AndAlso(
                    Expression.Equal(
                        Expression.Property(parameter, nameof(InteractionExpanded.PartyIdA)),
                        Expression.Constant(pair.PartyIdA)),
                    Expression.AndAlso(
                        Expression.Equal(
                            Expression.Property(parameter, nameof(InteractionExpanded.PartyTypeB)),
                            Expression.Constant(pair.PartyTypeB)),
                        Expression.Equal(
                            Expression.Property(parameter, nameof(InteractionExpanded.PartyIdB)),
                            Expression.Constant(pair.PartyIdB)))));

            pairExpression = pairExpression is null
                ? current
                : Expression.OrElse(pairExpression, current);
        }

        Expression finalExpression = Expression.AndAlso(interactionMatch, pairExpression!);

        return Expression.Lambda<Func<InteractionExpanded, bool>>(finalExpression, parameter);
    }

    public async Task<List<InteractionExpanded>> SearchExpandedRowsByAtomicIdsAsync(
        ShortInteractionType typeA,
        IReadOnlyCollection<Guid> idsA,
        ShortInteractionType typeB,
        IReadOnlyCollection<Guid> idsB,
        CancellationToken cancellationToken)
    {
        if (idsA.Count == 0 || idsB.Count == 0)
        {
            return new List<InteractionExpanded>();
        }

        return await _context.InteractionExpandedRows
            .AsNoTracking()
            .Where(row =>
                row.PartyTypeA == typeA &&
                idsA.Contains(row.PartyIdA) &&
                row.PartyTypeB == typeB &&
                idsB.Contains(row.PartyIdB))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<InteractionExpanded>> SearchExpandedRowsBulkAsync(
        IReadOnlyCollection<Guid> allSubstanceIds,
        bool includeFood,
        bool includeSupplement,
        CancellationToken cancellationToken)
    {
        if (allSubstanceIds.Count == 0)
        {
            return new List<InteractionExpanded>();
        }

        return await _context.InteractionExpandedRows
            .AsNoTracking()
            .Where(row =>
                row.PartyTypeA == ShortInteractionType.Substance &&
                allSubstanceIds.Contains(row.PartyIdA) &&
                (
                    (row.PartyTypeB == ShortInteractionType.Substance && allSubstanceIds.Contains(row.PartyIdB)) ||
                    (includeFood && row.PartyTypeB == ShortInteractionType.Food) ||
                    (includeSupplement && row.PartyTypeB == ShortInteractionType.Supplement)
                ))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Interaction>> GetByIdsAsync(
        IReadOnlyCollection<Guid> interactionIds,
        CancellationToken cancellationToken)
    {
        if (interactionIds.Count == 0)
        {
            return new List<Interaction>();
        }

        return await _context.Interactions
            .AsNoTracking()
            .Include(interaction => interaction.InteractionSeverity)
            .Include(interaction => interaction.Parties)
            .Where(interaction => interactionIds.Contains(interaction.Id))
            .OrderBy(interaction => interaction.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<InteractionExpandedProjection>> ListExpandedByLeftActiveSubstanceIdsAsync(
        InteractionKind interactionKind,
        IReadOnlyCollection<Guid> leftActiveSubstanceIds,
        CancellationToken cancellationToken)
    {
        if (leftActiveSubstanceIds.Count == 0)
        {
            return new List<InteractionExpandedProjection>();
        }

        var leftIds = leftActiveSubstanceIds.ToList();

        List<InteractionExpandedProjection> forwardRows = await BuildExpandedProjectionQueryAsync(
            interactionKind,
            expanded => expanded.PartyTypeA == ShortInteractionType.Substance && leftIds.Contains(expanded.PartyIdA),
            useReverseMapping: false,
            cancellationToken);

        if (interactionKind != InteractionKind.Substance)
        {
            return OrderExpandedProjections(forwardRows);
        }

        List<InteractionExpandedProjection> reverseRows = await BuildExpandedProjectionQueryAsync(
            interactionKind,
            expanded => expanded.PartyTypeB == ShortInteractionType.Substance && leftIds.Contains(expanded.PartyIdB),
            useReverseMapping: true,
            cancellationToken);

        return OrderExpandedProjections(
            forwardRows
                .Concat(reverseRows)
                .GroupBy(row => row.ExpandedRowId)
                .Select(group => group.First())
                .ToList());
    }

    private async Task<List<InteractionExpandedProjection>> BuildExpandedProjectionQueryAsync(
        InteractionKind interactionKind,
        Expression<Func<InteractionExpanded, bool>> expandedFilter,
        bool useReverseMapping,
        CancellationToken cancellationToken)
    {
        IQueryable<InteractionExpanded> expandedQuery = _context.InteractionExpandedRows
            .AsNoTracking()
            .Where(expandedFilter);

        IQueryable<Interaction> interactionQuery = ApplyInteractionKindFilter(
            _context.Interactions.AsNoTracking(),
            interactionKind);

        if (!useReverseMapping)
        {
            return await (
                from expanded in expandedQuery
                join interaction in interactionQuery on expanded.InteractionId equals interaction.Id
                join severity in _context.InteractionSeverities.AsNoTracking()
                    on interaction.InteractionSeverityId equals severity.Id
                select new InteractionExpandedProjection(
                    expanded.Id,
                    expanded.InteractionId,
                    expanded.PartyTypeA,
                    expanded.PartyIdA,
                    expanded.PartyTypeB,
                    expanded.PartyIdB,
                    interaction.DefaultText,
                    interaction.StomachText,
                    severity.Name,
                    severity.SeverityNumber))
                .ToListAsync(cancellationToken);
        }

        return await (
            from expanded in expandedQuery
            join interaction in interactionQuery on expanded.InteractionId equals interaction.Id
            join severity in _context.InteractionSeverities.AsNoTracking()
                on interaction.InteractionSeverityId equals severity.Id
            select new InteractionExpandedProjection(
                expanded.Id,
                expanded.InteractionId,
                expanded.PartyTypeB,
                expanded.PartyIdB,
                expanded.PartyTypeA,
                expanded.PartyIdA,
                interaction.DefaultText,
                interaction.StomachText,
                severity.Name,
                severity.SeverityNumber))
            .ToListAsync(cancellationToken);
    }

    private static List<InteractionExpandedProjection> OrderExpandedProjections(
        List<InteractionExpandedProjection> rows) =>
        rows
            .OrderByDescending(row => row.SeverityNumber)
            .ThenBy(row => row.LeftPartyId)
            .ThenBy(row => row.RightPartyId)
            .ToList();

    public async Task<List<Interaction>> ListByActiveSubstanceIdAsync(
        Guid activeSubstanceId,
        CancellationToken cancellationToken)
    {
        return await _context.Interactions
            .AsNoTracking()
            .Include(interaction => interaction.InteractionSeverity)
            .Include(interaction => interaction.Parties)
            .Where(interaction => interaction.Parties.Any(party =>
                party.PartyType == InteractionType.Substance &&
                party.ActiveSubstanceId == activeSubstanceId))
            .OrderBy(interaction => interaction.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Interaction>> ListBySupplementIdAsync(
        Guid supplementId,
        CancellationToken cancellationToken)
    {
        return await _context.Interactions
            .AsNoTracking()
            .Include(interaction => interaction.InteractionSeverity)
            .Include(interaction => interaction.Parties)
            .Where(interaction => interaction.Parties.Any(party =>
                party.PartyType == InteractionType.Supplement &&
                party.SupplementId == supplementId))
            .OrderBy(interaction => interaction.Id)
            .ToListAsync(cancellationToken);
    }
}
