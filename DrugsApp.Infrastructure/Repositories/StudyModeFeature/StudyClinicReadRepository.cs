using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.RegularDrugsFeature.Enums;
using DrugsApp.Domain.RegularFoodFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Repositories.StudyModeFeature;

public sealed class StudyClinicReadRepository(DrugsAppDbContext _context) : IStudyClinicReadRepository
{
    public async Task<StudyClinicCanvasReadModel?> GetCanvasAsync(Guid clinicId, CancellationToken cancellationToken)
    {
        // load clinic header
        StudyClinicHeaderReadModel? header = await _context.StudyClinics
            .AsNoTracking()
            .Where(clinic => clinic.Id == clinicId)
            .Select(clinic => new StudyClinicHeaderReadModel(
                clinic.Id,
                clinic.Name,
                clinic.ScenarioDescription,
                clinic.CreatedAtUtc,
                clinic.Visibility,
                clinic.CreatedByUserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null)
        {
            return null;
        }

        // load members joined with profiles in one query (passwords are never selected)
        List<MemberRow> memberRows = await BuildMemberRowsQuery(clinicId, null, null)
            .ToListAsync(cancellationToken);

        List<Guid> doctorUserIds = memberRows
            .Where(row => row.MemberKind == StudyMemberKind.Doctor)
            .Select(row => row.UserId)
            .ToList();

        List<Guid> patientUserIds = memberRows
            .Where(row => row.MemberKind == StudyMemberKind.Patient)
            .Select(row => row.UserId)
            .ToList();

        // load patient medical counters with one grouped query per table, independent of member count
        Dictionary<Guid, StudyPatientMedicalSummaryReadModel> summaries =
            await LoadMedicalSummariesAsync(patientUserIds, cancellationToken);

        // load active associations between the clinic's doctors and patients
        List<StudyAssociationReadModel> associations = await LoadAssociationsAsync(
            doctorUserIds,
            patientUserIds,
            cancellationToken);

        List<StudyDoctorNodeReadModel> doctors = memberRows
            .Where(row => row.MemberKind == StudyMemberKind.Doctor)
            .OrderBy(row => row.CreatedAtUtc)
            .ThenBy(row => row.UserId)
            .Select(row => new StudyDoctorNodeReadModel(
                row.UserId,
                row.FirstName,
                row.LastName,
                row.ProfessionCode,
                row.OtherProfessionLabel,
                row.WorkplaceLabel,
                row.LoginEmail,
                row.PositionX,
                row.PositionY,
                row.CreatedAtUtc))
            .ToList();

        List<StudyPatientNodeReadModel> patients = memberRows
            .Where(row => row.MemberKind == StudyMemberKind.Patient)
            .OrderBy(row => row.CreatedAtUtc)
            .ThenBy(row => row.UserId)
            .Select(row => new StudyPatientNodeReadModel(
                row.UserId,
                row.FirstName,
                row.LastName,
                row.DateOfBirth,
                row.Gender,
                row.LoginEmail,
                row.PositionX,
                row.PositionY,
                summaries.GetValueOrDefault(row.UserId, StudyPatientMedicalSummaryReadModel.Empty),
                row.CreatedAtUtc))
            .ToList();

        return new StudyClinicCanvasReadModel(header, doctors, patients, associations);
    }

    public async Task<StudyMemberDetailReadModel?> GetMemberDetailAsync(
        Guid clinicId,
        Guid userId,
        StudyMemberKind memberKind,
        CancellationToken cancellationToken)
    {
        MemberRow? row = await BuildMemberRowsQuery(clinicId, userId, memberKind)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        StudyPatientMedicalSummaryReadModel? medicalSummary = null;
        if (memberKind == StudyMemberKind.Patient)
        {
            Dictionary<Guid, StudyPatientMedicalSummaryReadModel> summaries =
                await LoadMedicalSummariesAsync([userId], cancellationToken);
            medicalSummary = summaries.GetValueOrDefault(userId, StudyPatientMedicalSummaryReadModel.Empty);
        }

        return new StudyMemberDetailReadModel(
            row.UserId,
            row.MemberKind,
            row.LoginEmail,
            row.PositionX,
            row.PositionY,
            row.FirstName,
            row.LastName,
            row.DateOfBirth,
            row.Gender,
            row.ProfessionCode,
            row.OtherProfessionLabel,
            row.WorkplaceLabel,
            row.ShortDescription,
            medicalSummary);
    }

    private IQueryable<MemberRow> BuildMemberRowsQuery(Guid clinicId, Guid? userId, StudyMemberKind? memberKind)
    {
        IQueryable<StudyClinicMember> members = _context.StudyClinicMembers
            .AsNoTracking()
            .Where(member => member.StudyClinicId == clinicId);

        if (userId is not null)
        {
            members = members.Where(member => member.UserId == userId.Value);
        }

        if (memberKind is not null)
        {
            members = members.Where(member => member.MemberKind == memberKind.Value);
        }

        return
            from member in members
            join profile in _context.UserProfiles.AsNoTracking() on member.UserId equals profile.Id
            join providerProfile in _context.HealthcareProviderProfiles.AsNoTracking()
                on member.UserId equals providerProfile.UserId into providerProfiles
            from providerProfile in providerProfiles.DefaultIfEmpty()
            select new MemberRow(
                member.UserId,
                member.MemberKind,
                member.LoginEmail,
                member.PositionX,
                member.PositionY,
                member.CreatedAtUtc,
                profile.FirstName,
                profile.LastName,
                profile.DateOfBirth,
                profile.Gender,
                providerProfile != null ? providerProfile.ProfessionCode : null,
                providerProfile != null ? providerProfile.OtherProfessionLabel : null,
                providerProfile != null ? providerProfile.WorkplaceLabel : null,
                providerProfile != null ? providerProfile.ShortDescription : null);
    }

    private async Task<List<StudyAssociationReadModel>> LoadAssociationsAsync(
        List<Guid> doctorUserIds,
        List<Guid> patientUserIds,
        CancellationToken cancellationToken)
    {
        if (doctorUserIds.Count == 0 || patientUserIds.Count == 0)
        {
            return [];
        }

        return await _context.SupervisionConnections
            .AsNoTracking()
            .Where(connection => connection.ConnectionState == SupervisionConnectionState.Active
                && doctorUserIds.Contains(connection.HealthcareProfessionalUserId)
                && patientUserIds.Contains(connection.PatientUserId))
            .OrderBy(connection => connection.HealthcareProfessionalUserId)
            .ThenBy(connection => connection.PatientUserId)
            .Select(connection => new StudyAssociationReadModel(
                connection.HealthcareProfessionalUserId,
                connection.PatientUserId))
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, StudyPatientMedicalSummaryReadModel>> LoadMedicalSummariesAsync(
        List<Guid> patientUserIds,
        CancellationToken cancellationToken)
    {
        if (patientUserIds.Count == 0)
        {
            return [];
        }

        Dictionary<Guid, int> drugAllergyCounts = await CountPerUserAsync(
            _context.ClientDrugAllergies.Where(row => patientUserIds.Contains(row.UserId)).Select(row => row.UserId),
            cancellationToken);
        Dictionary<Guid, int> supplementAllergyCounts = await CountPerUserAsync(
            _context.ClientSupplementAllergies.Where(row => patientUserIds.Contains(row.UserId)).Select(row => row.UserId),
            cancellationToken);
        Dictionary<Guid, int> foodAllergyCounts = await CountPerUserAsync(
            _context.ClientFoodAllergies.Where(row => patientUserIds.Contains(row.UserId)).Select(row => row.UserId),
            cancellationToken);
        Dictionary<Guid, int> otherAllergyCounts = await CountPerUserAsync(
            _context.ClientOtherAllergies.Where(row => patientUserIds.Contains(row.UserId)).Select(row => row.UserId),
            cancellationToken);
        Dictionary<Guid, int> healthConditionCounts = await CountPerUserAsync(
            _context.UserHealthConditions.Where(row => patientUserIds.Contains(row.UserId)).Select(row => row.UserId),
            cancellationToken);
        Dictionary<Guid, int> drugProblemCounts = await CountPerUserAsync(
            _context.DrugProblems.Where(row => patientUserIds.Contains(row.UserId)).Select(row => row.UserId),
            cancellationToken);
        Dictionary<Guid, int> regularDrugCounts = await CountPerUserAsync(
            _context.ClientRegularDrugs
                .Where(row => patientUserIds.Contains(row.UserId)
                    && (row.Status == RegularDrugStatus.Scheduled || row.Status == RegularDrugStatus.Active))
                .Select(row => row.UserId),
            cancellationToken);
        Dictionary<Guid, int> regularSupplementCounts = await CountPerUserAsync(
            _context.ClientRegularSupplements
                .Where(row => patientUserIds.Contains(row.UserId)
                    && (row.Status == RegularSupplementStatus.Scheduled || row.Status == RegularSupplementStatus.Active))
                .Select(row => row.UserId),
            cancellationToken);
        Dictionary<Guid, int> regularFoodCounts = await CountPerUserAsync(
            _context.ClientRegularFoods
                .Where(row => patientUserIds.Contains(row.UserId)
                    && (row.Status == RegularFoodStatus.Scheduled || row.Status == RegularFoodStatus.Active))
                .Select(row => row.UserId),
            cancellationToken);

        return patientUserIds.ToDictionary(
            patientUserId => patientUserId,
            patientUserId => new StudyPatientMedicalSummaryReadModel(
                drugAllergyCounts.GetValueOrDefault(patientUserId)
                    + supplementAllergyCounts.GetValueOrDefault(patientUserId)
                    + foodAllergyCounts.GetValueOrDefault(patientUserId)
                    + otherAllergyCounts.GetValueOrDefault(patientUserId),
                healthConditionCounts.GetValueOrDefault(patientUserId),
                drugProblemCounts.GetValueOrDefault(patientUserId),
                regularDrugCounts.GetValueOrDefault(patientUserId),
                regularSupplementCounts.GetValueOrDefault(patientUserId),
                regularFoodCounts.GetValueOrDefault(patientUserId)));
    }

    private static async Task<Dictionary<Guid, int>> CountPerUserAsync(
        IQueryable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        return await userIds
            .GroupBy(userId => userId)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.UserId, row => row.Count, cancellationToken);
    }

    private sealed record MemberRow(
        Guid UserId,
        StudyMemberKind MemberKind,
        string LoginEmail,
        double PositionX,
        double PositionY,
        DateTime CreatedAtUtc,
        string FirstName,
        string LastName,
        DateOnly DateOfBirth,
        ProfileGender Gender,
        HealthcareProviderProfession? ProfessionCode,
        string? OtherProfessionLabel,
        string? WorkplaceLabel,
        string? ShortDescription);
}
