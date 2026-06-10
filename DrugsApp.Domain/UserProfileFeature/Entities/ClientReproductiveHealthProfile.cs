using DrugsApp.Domain.Common;
using DrugsApp.Domain.UserProfileFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.UserProfileFeature.Entities;

/// <summary>
/// Women-specific reproductive health profile attached to a user profile.
/// </summary>
public sealed class ClientReproductiveHealthProfile : BaseEntity
{
    // Attributes

    /// <summary>
    /// Owner user identifier (matches <see cref="UserProfile.Id" />).
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Indicates whether the user currently declares pregnancy.
    /// </summary>
    public bool CurrentlyPregnant { get; private set; }

    /// <summary>
    /// Indicates whether the user plans to get pregnant.
    /// </summary>
    public bool PlanningToGetPregnant { get; private set; }

    /// <summary>
    /// Indicates whether the user declares menopause.
    /// </summary>
    public bool Menopause { get; private set; }

    /// <summary>
    /// Optional contraception method code captured from the allowed list.
    /// </summary>
    public ContraceptionMethodCode? ContraceptionMethodCode { get; private set; }

    // Relationships

    private ClientReproductiveHealthProfile()
    {
    }

    private ClientReproductiveHealthProfile(
        Guid userId,
        bool currentlyPregnant,
        bool planningToGetPregnant,
        bool menopause,
        ContraceptionMethodCode? contraceptionMethodCode) : this()
    {
        UserId = userId;
        CurrentlyPregnant = currentlyPregnant;
        PlanningToGetPregnant = planningToGetPregnant;
        Menopause = menopause;
        ContraceptionMethodCode = contraceptionMethodCode;
    }

    // Factory and behaviors

    /// <summary>
    /// Creates a new reproductive health profile for the specified user.
    /// </summary>
    /// <param name="userId">Profile owner identifier.</param>
    /// <param name="currentlyPregnant">Pregnancy declaration.</param>
    /// <param name="planningToGetPregnant">Pregnancy planning flag.</param>
    /// <param name="menopause">Menopause declaration.</param>
    /// <param name="contraceptionMethodCode">Optional contraception method code.</param>
    /// <returns>Validated <see cref="ClientReproductiveHealthProfile"/> or validation errors.</returns>
    public static ErrorOr<ClientReproductiveHealthProfile> Create(
        Guid userId,
        bool currentlyPregnant,
        bool planningToGetPregnant,
        bool menopause,
        ContraceptionMethodCode? contraceptionMethodCode)
    {
        ErrorOr<(bool CurrentlyPregnant, bool PlanningToGetPregnant, ContraceptionMethodCode? ContraceptionMethodCode)> normalized = NormalizeState(
            userId,
            currentlyPregnant,
            planningToGetPregnant,
            contraceptionMethodCode);

        if (normalized.IsError)
        {
            return normalized.Errors;
        }

        return new ClientReproductiveHealthProfile(
            userId,
            normalized.Value.CurrentlyPregnant,
            normalized.Value.PlanningToGetPregnant,
            menopause,
            normalized.Value.ContraceptionMethodCode);
    }

    /// <summary>
    /// Updates the reproductive health profile values.
    /// </summary>
    /// <param name="currentlyPregnant">Updated pregnancy declaration.</param>
    /// <param name="planningToGetPregnant">Updated planning flag.</param>
    /// <param name="menopause">Updated menopause flag.</param>
    /// <param name="contraceptionMethodCode">Optional contraception method code.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> Update(
        bool currentlyPregnant,
        bool planningToGetPregnant,
        bool menopause,
        ContraceptionMethodCode? contraceptionMethodCode)
    {
        ErrorOr<(bool CurrentlyPregnant, bool PlanningToGetPregnant, ContraceptionMethodCode? ContraceptionMethodCode)> normalized = NormalizeState(
            UserId,
            currentlyPregnant,
            planningToGetPregnant,
            contraceptionMethodCode);

        if (normalized.IsError)
        {
            return normalized.Errors;
        }

        CurrentlyPregnant = normalized.Value.CurrentlyPregnant;
        PlanningToGetPregnant = normalized.Value.PlanningToGetPregnant;
        Menopause = menopause;
        ContraceptionMethodCode = normalized.Value.ContraceptionMethodCode;

        return Result.Success;
    }

    private static ErrorOr<(bool CurrentlyPregnant, bool PlanningToGetPregnant, ContraceptionMethodCode? ContraceptionMethodCode)> NormalizeState(
        Guid userId,
        bool currentlyPregnant,
        bool planningToGetPregnant,
        ContraceptionMethodCode? contraceptionMethodCode)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientReproductiveHealthProfile.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId"
                });
        }

        if (contraceptionMethodCode is not null && !Enum.IsDefined(typeof(ContraceptionMethodCode), contraceptionMethodCode.Value))
        {
            return Error.Validation(
                code: "ClientReproductiveHealthProfile.ContraceptionCodeInvalid",
                description: "Contraception method code is invalid.",
                metadata: new()
                {
                    ["field"] = "contraceptionMethodCode"
                });
        }

        bool effectiveCurrentlyPregnant = currentlyPregnant;
        bool effectivePlanningToGetPregnant = planningToGetPregnant;
        ContraceptionMethodCode? effectiveContraception = contraceptionMethodCode;

        if (effectiveCurrentlyPregnant)
        {
            effectivePlanningToGetPregnant = false;
            effectiveContraception = null;
        }

        return (effectiveCurrentlyPregnant, effectivePlanningToGetPregnant, effectiveContraception);
    }
}
