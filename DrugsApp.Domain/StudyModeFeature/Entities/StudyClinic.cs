using DrugsApp.Domain.Common;
using DrugsApp.Domain.StudyModeFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.StudyModeFeature.Entities;

/// <summary>
/// Virtual training clinic aggregate root; owns its provisioned study members and enforces member caps,
/// member uniqueness and canvas invariants (positions and doctor-patient association parties).
/// </summary>
public sealed class StudyClinic : BaseEntity
{
    private const int NAME_MIN_LENGTH = 3;
    private const int NAME_MAX_LENGTH = 120;
    private const int SCENARIO_DESCRIPTION_MAX_LENGTH = 2000;
    private const int MAX_DOCTOR_COUNT = 20;
    private const int MAX_PATIENT_COUNT = 60;

    // Attributes

    /// <summary>
    /// Display name of the clinic, trimmed and whitespace-collapsed.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Lowercased invariant form of <see cref="Name"/> used for case-insensitive uniqueness and search.
    /// </summary>
    public string NormalizedName { get; private set; } = string.Empty;

    /// <summary>
    /// Optional teaching scenario text; empty input is stored as null.
    /// </summary>
    public string? ScenarioDescription { get; private set; }

    /// <summary>
    /// Admin user who created the clinic; audit information only, not ownership.
    /// </summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>
    /// Who can see and manage the clinic: every study mode admin (public) or only its creator (private).
    /// </summary>
    public StudyClinicVisibility Visibility { get; private set; }

    // Relationships

    private readonly List<StudyClinicMember> _members = new();

    /// <summary>
    /// Provisioned study accounts placed on the clinic canvas (One-to-Many, mutated only through clinic methods).
    /// </summary>
    public IReadOnlyCollection<StudyClinicMember> Members => _members.AsReadOnly();

    private StudyClinic()
    {
    }

    // Factory and behaviors

    /// <summary>
    /// Creates a new empty study clinic.
    /// </summary>
    /// <param name="name">
    /// Display name; trimmed and whitespace-collapsed, 3–120 characters.
    /// </param>
    /// <param name="scenarioDescription">
    /// Optional scenario text; trimmed, empty becomes null, up to 2000 characters.
    /// </param>
    /// <param name="createdByUserId">
    /// Admin user creating the clinic.
    /// </param>
    /// <param name="visibility">
    /// Initial visibility chosen by the creator.
    /// </param>
    /// <returns>
    /// The new clinic or validation errors.
    /// </returns>
    public static ErrorOr<StudyClinic> Create(
        string name,
        string? scenarioDescription,
        Guid createdByUserId,
        StudyClinicVisibility visibility)
    {
        string collapsedName = CollapseWhitespace(name);
        if (collapsedName.Length < NAME_MIN_LENGTH || collapsedName.Length > NAME_MAX_LENGTH)
        {
            return Error.Validation(
                code: "StudyClinic.NameInvalid",
                description: $"Clinic name must contain between {NAME_MIN_LENGTH} and {NAME_MAX_LENGTH} characters.",
                metadata: new()
                {
                    ["field"] = "name",
                    ["minLength"] = NAME_MIN_LENGTH,
                    ["maxLength"] = NAME_MAX_LENGTH
                });
        }

        string? trimmedScenarioDescription = string.IsNullOrWhiteSpace(scenarioDescription)
            ? null
            : scenarioDescription.Trim();
        if (trimmedScenarioDescription is not null && trimmedScenarioDescription.Length > SCENARIO_DESCRIPTION_MAX_LENGTH)
        {
            return Error.Validation(
                code: "StudyClinic.ScenarioDescriptionTooLong",
                description: $"Scenario description must not exceed {SCENARIO_DESCRIPTION_MAX_LENGTH} characters.",
                metadata: new()
                {
                    ["field"] = "scenarioDescription",
                    ["maxLength"] = SCENARIO_DESCRIPTION_MAX_LENGTH
                });
        }

        if (createdByUserId == Guid.Empty)
        {
            return Error.Validation(
                code: "StudyClinic.CreatedByUserIdEmpty",
                description: "Creating admin user id must be provided.",
                metadata: new()
                {
                    ["field"] = "createdByUserId"
                });
        }

        return new StudyClinic
        {
            Name = collapsedName,
            NormalizedName = collapsedName.ToLowerInvariant(),
            ScenarioDescription = trimmedScenarioDescription,
            CreatedByUserId = createdByUserId,
            Visibility = visibility
        };
    }

    /// <summary>
    /// Tells whether the given admin user may see and manage the clinic.
    /// </summary>
    /// <param name="userId">
    /// Calling admin user identifier.
    /// </param>
    /// <returns>
    /// True for public clinics and for private clinics created by <paramref name="userId"/>.
    /// </returns>
    public bool IsAccessibleBy(Guid userId) =>
        Visibility == StudyClinicVisibility.Public || CreatedByUserId == userId;

    /// <summary>
    /// Changes who can see the clinic; only its creator may do so.
    /// </summary>
    /// <param name="visibility">
    /// New visibility.
    /// </param>
    /// <param name="callerUserId">
    /// Admin user requesting the change.
    /// </param>
    /// <returns>
    /// Success, or forbidden when the caller is not the creator.
    /// </returns>
    public ErrorOr<Success> ChangeVisibility(StudyClinicVisibility visibility, Guid callerUserId)
    {
        if (CreatedByUserId != callerUserId)
        {
            return Error.Forbidden(
                code: "StudyClinic.NotCreator",
                description: "Only the creator of the clinic can change its visibility.",
                metadata: new()
                {
                    ["field"] = "visibility"
                });
        }

        Visibility = visibility;
        return Result.Success;
    }

    /// <summary>
    /// Returns the normalized (trimmed, whitespace-collapsed, lowercased invariant) form of a clinic name.
    /// </summary>
    /// <param name="name">
    /// Raw clinic name or search text.
    /// </param>
    /// <returns>
    /// Normalized value used for uniqueness lookups and search filters.
    /// </returns>
    public static string NormalizeName(string name) => CollapseWhitespace(name).ToLowerInvariant();

    /// <summary>
    /// Returns the maximum number of members of the given kind a clinic may hold.
    /// </summary>
    /// <param name="memberKind">
    /// Member kind whose cap is requested.
    /// </param>
    /// <returns>
    /// 20 for doctors, 60 for patients.
    /// </returns>
    public static int GetMemberCap(StudyMemberKind memberKind) =>
        memberKind == StudyMemberKind.Doctor ? MAX_DOCTOR_COUNT : MAX_PATIENT_COUNT;

    /// <summary>
    /// Ensures the clinic still has room for another member of the given kind.
    /// </summary>
    /// <param name="memberKind">
    /// Kind of the member about to be added.
    /// </param>
    /// <returns>
    /// Success, or a conflict with <c>field=limit</c> and <c>maxCount</c> metadata when the cap is reached.
    /// </returns>
    public ErrorOr<Success> EnsureCapacityFor(StudyMemberKind memberKind)
    {
        int maxCount = GetMemberCap(memberKind);
        int currentCount = _members.Count(member => member.MemberKind == memberKind);
        if (currentCount >= maxCount)
        {
            return Error.Conflict(
                code: "StudyClinic.MemberLimitReached",
                description: $"A study clinic can hold at most {maxCount} {memberKind.ToString().ToLowerInvariant()} accounts.",
                metadata: new()
                {
                    ["field"] = "limit",
                    ["maxCount"] = maxCount,
                    ["memberKind"] = memberKind.ToString()
                });
        }

        return Result.Success;
    }

    /// <summary>
    /// Adds a provisioned study account to the clinic.
    /// </summary>
    /// <param name="userId">
    /// Identity user identifier of the study account.
    /// </param>
    /// <param name="memberKind">
    /// Canvas node type of the study account.
    /// </param>
    /// <param name="loginEmail">
    /// Generated login email.
    /// </param>
    /// <param name="protectedPassword">
    /// Data Protection payload of the generated password.
    /// </param>
    /// <param name="positionX">
    /// Initial horizontal canvas coordinate.
    /// </param>
    /// <param name="positionY">
    /// Initial vertical canvas coordinate.
    /// </param>
    /// <returns>
    /// The added member, a conflict when the cap is reached or the user is already a member, or validation errors.
    /// </returns>
    public ErrorOr<StudyClinicMember> AddMember(
        Guid userId,
        StudyMemberKind memberKind,
        string loginEmail,
        string protectedPassword,
        double positionX,
        double positionY)
    {
        ErrorOr<Success> capacityResult = EnsureCapacityFor(memberKind);
        if (capacityResult.IsError)
        {
            return capacityResult.Errors;
        }

        bool isAlreadyMember = _members.Any(member => member.UserId == userId);
        if (isAlreadyMember)
        {
            return Error.Conflict(
                code: "StudyClinic.MemberAlreadyExists",
                description: "The study account is already a member of this clinic.",
                metadata: new()
                {
                    ["field"] = "userId"
                });
        }

        ErrorOr<StudyClinicMember> memberResult = StudyClinicMember.Create(
            Id,
            userId,
            memberKind,
            loginEmail,
            protectedPassword,
            positionX,
            positionY);
        if (memberResult.IsError)
        {
            return memberResult.Errors;
        }

        _members.Add(memberResult.Value);
        return memberResult.Value;
    }

    /// <summary>
    /// Finds a member by user identifier and, optionally, by member kind.
    /// </summary>
    /// <param name="userId">
    /// Identity user identifier of the study account.
    /// </param>
    /// <param name="memberKind">
    /// Optional kind that the member must also match.
    /// </param>
    /// <returns>
    /// The matching member or null.
    /// </returns>
    public StudyClinicMember? FindMember(Guid userId, StudyMemberKind? memberKind) =>
        _members.FirstOrDefault(member =>
            member.UserId == userId
            && (memberKind is null || member.MemberKind == memberKind));

    /// <summary>
    /// Removes a member from the clinic; EF deletes the orphaned row on commit.
    /// </summary>
    /// <param name="userId">
    /// Identity user identifier of the member to remove.
    /// </param>
    /// <returns>
    /// Success, or not found when the user is not a member of this clinic.
    /// </returns>
    public ErrorOr<Success> RemoveMember(Guid userId)
    {
        StudyClinicMember? member = FindMember(userId, null);
        if (member is null)
        {
            return MemberNotFound(userId, "userId");
        }

        _members.Remove(member);
        return Result.Success;
    }

    /// <summary>
    /// Ensures every association pair references a doctor member and a patient member of this clinic.
    /// </summary>
    /// <param name="pairs">
    /// Desired doctor-patient association pairs.
    /// </param>
    /// <returns>
    /// Success, or a validation error with <c>field=associations</c> and <c>invalidPairs</c> metadata.
    /// </returns>
    public ErrorOr<Success> ValidateAssociations(
        IReadOnlyCollection<(Guid DoctorUserId, Guid PatientUserId)> pairs)
    {
        HashSet<Guid> doctorUserIds = GetMemberUserIds(StudyMemberKind.Doctor);
        HashSet<Guid> patientUserIds = GetMemberUserIds(StudyMemberKind.Patient);

        List<string> invalidPairs = pairs
            .Where(pair => !doctorUserIds.Contains(pair.DoctorUserId) || !patientUserIds.Contains(pair.PatientUserId))
            .Select(pair => $"{pair.DoctorUserId}:{pair.PatientUserId}")
            .ToList();

        if (invalidPairs.Count > 0)
        {
            return Error.Validation(
                code: "StudyClinic.AssociationsInvalid",
                description: "Every association must connect a doctor and a patient of this clinic.",
                metadata: new()
                {
                    ["field"] = "associations",
                    ["invalidPairs"] = string.Join(",", invalidPairs)
                });
        }

        return Result.Success;
    }

    /// <summary>
    /// Applies canvas coordinates to existing members.
    /// </summary>
    /// <param name="positions">
    /// Member user identifiers with their new coordinates.
    /// </param>
    /// <returns>
    /// Success, or a validation error with <c>field=positions</c> when a member is missing or a coordinate is invalid.
    /// </returns>
    public ErrorOr<Success> UpdatePositions(
        IReadOnlyCollection<(Guid UserId, double PositionX, double PositionY)> positions)
    {
        foreach ((Guid userId, double positionX, double positionY) in positions)
        {
            StudyClinicMember? member = FindMember(userId, null);
            if (member is null)
            {
                return Error.Validation(
                    code: "StudyClinic.PositionMemberNotFound",
                    description: "Positions may only reference members of this clinic.",
                    metadata: new()
                    {
                        ["field"] = "positions",
                        ["userId"] = userId.ToString()
                    });
            }

            ErrorOr<Success> moveResult = member.MoveTo(positionX, positionY);
            if (moveResult.IsError)
            {
                return moveResult.Errors;
            }
        }

        return Result.Success;
    }

    /// <summary>
    /// Returns the user identifiers of every member of the given kind.
    /// </summary>
    /// <param name="memberKind">
    /// Member kind to select.
    /// </param>
    /// <returns>
    /// Set of member user identifiers.
    /// </returns>
    public HashSet<Guid> GetMemberUserIds(StudyMemberKind memberKind) =>
        _members
            .Where(member => member.MemberKind == memberKind)
            .Select(member => member.UserId)
            .ToHashSet();

    private static Error MemberNotFound(Guid userId, string field) =>
        Error.NotFound(
            code: "StudyClinicMember.NotFound",
            description: "Study account was not found in this clinic.",
            metadata: new()
            {
                ["field"] = field,
                ["resourceId"] = userId.ToString()
            });

    private static string CollapseWhitespace(string value) =>
        string.Join(
            ' ',
            (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
