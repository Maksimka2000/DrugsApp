namespace DrugsApp.Features.StudyModeFeature.Shared;

/// <summary>
/// Result of filling a study patient's medical section with random sample data.
/// </summary>
/// <param name="Section">Section token that was filled (for example <c>regular-food</c>).</param>
/// <param name="RequestedCount">Requested number of items.</param>
/// <param name="CreatedCount">Number of items actually created; lower when no further unique candidates existed or the limit was reached.</param>
/// <param name="LimitReached">Whether the per-user limit of the section stopped the generation.</param>
public sealed record StudyRandomDataResponse(
    string Section,
    int RequestedCount,
    int CreatedCount,
    bool LimitReached);

/// <summary>
/// Suggested personal data for a new study patient; nothing is saved by the endpoint that returns it.
/// </summary>
/// <param name="FirstName">Polish first name matching the gender.</param>
/// <param name="LastName">Polish surname in the form matching the gender.</param>
/// <param name="DateOfBirth">Birth date making the patient 18–90 years old.</param>
/// <param name="Gender">Profile gender: <c>female</c> or <c>male</c>.</param>
public sealed record StudyRandomPatientProfileResponse(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender);
