namespace DrugsApp.Domain.StudyModeFeature.Enums;

/// <summary>
/// Node type of a provisioned study account placed on a study clinic canvas; persisted as a string.
/// </summary>
public enum StudyMemberKind
{
    /// <summary>
    /// Read-only study doctor account that supervises study patients through teacher-defined associations.
    /// </summary>
    Doctor,

    /// <summary>
    /// Read-only study patient account whose medical record is authored by the teacher.
    /// </summary>
    Patient
}
