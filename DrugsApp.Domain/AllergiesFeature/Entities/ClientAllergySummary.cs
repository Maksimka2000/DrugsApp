using System;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.AllergiesFeature.Entities;

/// <summary>
/// Per-user allergy summary answers persisted as a single row.
/// </summary>
public sealed class ClientAllergySummary : BaseEntity
{
    // Attributes

    /// <summary>
    /// Owner identifier from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Whether the user experienced severe reactions.
    /// </summary>
    public AllergyAnswerState? SevereReactionStatus { get; private set; }

    /// <summary>
    /// Whether the user carries allergy medication.
    /// </summary>
    public bool? CarriesAllergyMedication { get; private set; }

    /// <summary>
    /// Allergy status for pollen and dust mites.
    /// </summary>
    public AllergyAnswerState? PollenDustMitesAllergyStatus { get; private set; }

    /// <summary>
    /// Allergy status for insect venom.
    /// </summary>
    public AllergyAnswerState? InsectVenomAllergyStatus { get; private set; }

    /// <summary>
    /// Latex allergy status.
    /// </summary>
    public AllergyAnswerState? LatexAllergyStatus { get; private set; }

    private ClientAllergySummary() : base()
    {
    }

    private ClientAllergySummary(Guid userId) : this()
    {
        UserId = userId;
    }

    /// <summary>
    /// Creates an empty allergy summary for a given user.
    /// </summary>
    /// <param name="userId">Authenticated user identifier.</param>
    /// <returns>Validated entity or errors.</returns>
    public static ErrorOr<ClientAllergySummary> CreateForUser(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "AllergySummary.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        return new ClientAllergySummary(userId);
    }

    /// <summary>
    /// Updates all summary answers in a single operation.
    /// </summary>
    /// <param name="severeReactionStatus">Tri-state severe reaction answer.</param>
    /// <param name="carriesAllergyMedication">Whether the user carries medication.</param>
    /// <param name="pollenDustMitesAllergyStatus">Pollen and dust mites allergy answer.</param>
    /// <param name="insectVenomAllergyStatus">Insect venom allergy answer.</param>
    /// <param name="latexAllergyStatus">Latex allergy answer.</param>
    /// <returns>Success result.</returns>
    public ErrorOr<Success> UpsertAnswers(
        AllergyAnswerState? severeReactionStatus,
        bool? carriesAllergyMedication,
        AllergyAnswerState? pollenDustMitesAllergyStatus,
        AllergyAnswerState? insectVenomAllergyStatus,
        AllergyAnswerState? latexAllergyStatus)
    {
        SevereReactionStatus = severeReactionStatus;
        CarriesAllergyMedication = carriesAllergyMedication;
        PollenDustMitesAllergyStatus = pollenDustMitesAllergyStatus;
        InsectVenomAllergyStatus = insectVenomAllergyStatus;
        LatexAllergyStatus = latexAllergyStatus;

        return Result.Success;
    }
}
