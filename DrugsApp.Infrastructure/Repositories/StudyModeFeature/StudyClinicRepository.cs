using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.StudyModeFeature;

public sealed class StudyClinicRepository(DrugsAppDbContext _context) : IStudyClinicRepository
{
    public async Task<List<StudyClinicSummary>> SearchSummariesAsync(
        string? search,
        Guid callerUserId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        // build query
        IQueryable<StudyClinic> query = _context.StudyClinics.AsNoTracking();

        // apply visibility and search
        query = ApplyVisibility(query, callerUserId);
        query = ApplySearch(query, search);

        // apply sort and order, project member counts in the same statement, apply paging
        return await query
            .OrderByDescending(clinic => clinic.CreatedAtUtc)
            .ThenBy(clinic => clinic.Name)
            .Select(clinic => new StudyClinicSummary(
                clinic.Id,
                clinic.Name,
                clinic.ScenarioDescription,
                clinic.Members.Count(member => member.MemberKind == StudyMemberKind.Doctor),
                clinic.Members.Count(member => member.MemberKind == StudyMemberKind.Patient),
                clinic.CreatedAtUtc,
                clinic.Visibility,
                clinic.CreatedByUserId))
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? search, Guid callerUserId, CancellationToken cancellationToken)
    {
        // build query
        IQueryable<StudyClinic> query = _context.StudyClinics.AsNoTracking();

        // apply visibility and search
        query = ApplyVisibility(query, callerUserId);
        query = ApplySearch(query, search);

        // apply counting
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> IsAccessibleAsync(Guid clinicId, Guid callerUserId, CancellationToken cancellationToken)
    {
        return await ApplyVisibility(_context.StudyClinics.AsNoTracking(), callerUserId)
            .AnyAsync(clinic => clinic.Id == clinicId, cancellationToken);
    }

    public async Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken)
    {
        return await _context.StudyClinics
            .AsNoTracking()
            .AnyAsync(clinic => clinic.NormalizedName == normalizedName, cancellationToken);
    }

    public async Task<StudyClinic?> GetByIdAsync(Guid clinicId, bool track, CancellationToken cancellationToken)
    {
        IQueryable<StudyClinic> query = _context.StudyClinics.Include(clinic => clinic.Members);
        if (!track)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(clinic => clinic.Id == clinicId, cancellationToken);
    }

    public async Task<StudyClinic?> GetByIdWithLockAsync(Guid clinicId, CancellationToken cancellationToken)
    {
        // row lock held until the surrounding transaction ends, so concurrent member creations
        // in the same clinic run one after another and see each other's members before the cap check
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM study_clinics WHERE \"Id\" = {clinicId} FOR UPDATE",
            cancellationToken);

        return await _context.StudyClinics
            .Include(clinic => clinic.Members)
            .FirstOrDefaultAsync(clinic => clinic.Id == clinicId, cancellationToken);
    }

    public async Task<List<StudyClinic>> ListPrivateByCreatorAsync(Guid creatorUserId, CancellationToken cancellationToken)
    {
        return await _context.StudyClinics
            .Include(clinic => clinic.Members)
            .Where(clinic => clinic.CreatedByUserId == creatorUserId
                && clinic.Visibility == StudyClinicVisibility.Private)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudyClinicMember?> GetMemberAsync(Guid clinicId, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.StudyClinicMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                member => member.StudyClinicId == clinicId && member.UserId == userId,
                cancellationToken);
    }

    public async Task<bool> ExistsMemberAsync(
        Guid clinicId,
        Guid userId,
        StudyMemberKind memberKind,
        CancellationToken cancellationToken)
    {
        return await _context.StudyClinicMembers
            .AsNoTracking()
            .AnyAsync(
                member => member.StudyClinicId == clinicId
                    && member.UserId == userId
                    && member.MemberKind == memberKind,
                cancellationToken);
    }

    public async Task AddAsync(StudyClinic clinic, CancellationToken cancellationToken)
    {
        await _context.StudyClinics.AddAsync(clinic, cancellationToken);
    }

    public void Remove(StudyClinic clinic)
    {
        _context.StudyClinics.Remove(clinic);
    }

    private static IQueryable<StudyClinic> ApplyVisibility(IQueryable<StudyClinic> query, Guid callerUserId) =>
        query.Where(clinic => clinic.Visibility == StudyClinicVisibility.Public
            || clinic.CreatedByUserId == callerUserId);

    private static IQueryable<StudyClinic> ApplySearch(IQueryable<StudyClinic> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        string normalizedSearch = StudyClinic.NormalizeName(search);
        return query.Where(clinic => clinic.NormalizedName.Contains(normalizedSearch));
    }
}
