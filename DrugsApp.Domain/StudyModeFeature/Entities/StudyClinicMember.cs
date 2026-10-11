using DrugsApp.Domain.Common;
using DrugsApp.Domain.StudyModeFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.StudyModeFeature.Entities;

/// <summary>
/// Provisioned study account placed on a study clinic canvas; child of <see cref="StudyClinic"/> that links
/// the Identity user to the clinic and keeps the protected generated credentials.
/// </summary>
public sealed class StudyClinicMember : BaseEntity
{
    private const int LOGIN_EMAIL_MAX_LENGTH = 256;

    // Attributes

    /// <summary>
    /// Identity user identifier of the study account; doubles as the canvas node identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Canvas node type of the study account (doctor or patient).
    /// </summary>
    public StudyMemberKind MemberKind { get; private set; }

    /// <summary>
    /// Generated login email mirrored from the Identity user, stored lowercased.
    /// </summary>
    public string LoginEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Data Protection payload of the generated password; never exposed outside the credentials use case.
    /// </summary>
    public string ProtectedPassword { get; private set; } = string.Empty;

    /// <summary>
    /// Horizontal canvas coordinate of the node.
    /// </summary>
    public double PositionX { get; private set; }

    /// <summary>
    /// Vertical canvas coordinate of the node.
    /// </summary>
    public double PositionY { get; private set; }

    // Relationships

    /// <summary>
    /// Owning study clinic identifier (Many-to-One with <see cref="StudyClinic"/>).
    /// </summary>
    public Guid StudyClinicId { get; private set; }

    private StudyClinicMember()
    {
    }

    // Factory and behaviors

    /// <summary>
    /// Creates a study clinic member; invoked exclusively by <see cref="StudyClinic.AddMember"/>.
    /// </summary>
    /// <param name="studyClinicId">
    /// Owning clinic identifier.
    /// </param>
    /// <param name="userId">
    /// Identity user identifier of the provisioned study account.
    /// </param>
    /// <param name="memberKind">
    /// Canvas node type of the study account.
    /// </param>
    /// <param name="loginEmail">
    /// Generated login email; trimmed and lowercased.
    /// </param>
    /// <param name="protectedPassword">
    /// Data Protection payload of the generated password.
    /// </param>
    /// <param name="positionX">
    /// Initial horizontal canvas coordinate; must be finite.
    /// </param>
    /// <param name="positionY">
    /// Initial vertical canvas coordinate; must be finite.
    /// </param>
    /// <returns>
    /// The validated member or validation errors.
    /// </returns>
    internal static ErrorOr<StudyClinicMember> Create(
        Guid studyClinicId,
        Guid userId,
        StudyMemberKind memberKind,
        string loginEmail,
        string protectedPassword,
        double positionX,
        double positionY)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "StudyClinicMember.UserIdEmpty",
                description: "Study account user id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId"
                });
        }

        string normalizedLoginEmail = (loginEmail ?? string.Empty).Trim().ToLowerInvariant();
        if (normalizedLoginEmail.Length == 0 || normalizedLoginEmail.Length > LOGIN_EMAIL_MAX_LENGTH)
        {
            return Error.Validation(
                code: "StudyClinicMember.LoginEmailInvalid",
                description: $"Study account login email must contain between 1 and {LOGIN_EMAIL_MAX_LENGTH} characters.",
                metadata: new()
                {
                    ["field"] = "loginEmail",
                    ["maxLength"] = LOGIN_EMAIL_MAX_LENGTH
                });
        }

        if (string.IsNullOrWhiteSpace(protectedPassword))
        {
            return Error.Validation(
                code: "StudyClinicMember.ProtectedPasswordEmpty",
                description: "Protected study account password must be provided.",
                metadata: new()
                {
                    ["field"] = "protectedPassword"
                });
        }

        ErrorOr<Success> positionResult = EnsureFinitePosition(positionX, positionY);
        if (positionResult.IsError)
        {
            return positionResult.Errors;
        }

        return new StudyClinicMember
        {
            StudyClinicId = studyClinicId,
            UserId = userId,
            MemberKind = memberKind,
            LoginEmail = normalizedLoginEmail,
            ProtectedPassword = protectedPassword,
            PositionX = positionX,
            PositionY = positionY
        };
    }

    /// <summary>
    /// Moves the canvas node to new coordinates.
    /// </summary>
    /// <param name="positionX">
    /// New horizontal canvas coordinate; must be finite.
    /// </param>
    /// <param name="positionY">
    /// New vertical canvas coordinate; must be finite.
    /// </param>
    /// <returns>
    /// Success, or a validation error when a coordinate is NaN or infinite.
    /// </returns>
    public ErrorOr<Success> MoveTo(double positionX, double positionY)
    {
        ErrorOr<Success> positionResult = EnsureFinitePosition(positionX, positionY);
        if (positionResult.IsError)
        {
            return positionResult.Errors;
        }

        PositionX = positionX;
        PositionY = positionY;
        return Result.Success;
    }

    private static ErrorOr<Success> EnsureFinitePosition(double positionX, double positionY)
    {
        bool hasFiniteCoordinates = double.IsFinite(positionX) && double.IsFinite(positionY);
        if (!hasFiniteCoordinates)
        {
            return Error.Validation(
                code: "StudyClinicMember.PositionInvalid",
                description: "Canvas coordinates must be finite numbers.",
                metadata: new()
                {
                    ["field"] = "positions"
                });
        }

        return Result.Success;
    }
}
