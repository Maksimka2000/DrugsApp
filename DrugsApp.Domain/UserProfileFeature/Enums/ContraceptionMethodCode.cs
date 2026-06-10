namespace DrugsApp.Domain.UserProfileFeature.Enums;

/// <summary>
/// Allowed contraception method codes captured for reproductive health profiles.
/// </summary>
public enum ContraceptionMethodCode
{
    /// <summary>
    /// No contraception method reported.
    /// </summary>
    none,

    /// <summary>
    /// Oral contraceptive pill.
    /// </summary>
    oral_pill,

    /// <summary>
    /// Intrauterine device.
    /// </summary>
    iud,

    /// <summary>
    /// Implant-based contraception.
    /// </summary>
    implant,

    /// <summary>
    /// Injectable contraception.
    /// </summary>
    injection,

    /// <summary>
    /// Contraceptive patch.
    /// </summary>
    patch,

    /// <summary>
    /// Vaginal ring contraception.
    /// </summary>
    vaginal_ring,

    /// <summary>
    /// Barrier methods (e.g., condoms).
    /// </summary>
    barrier_method,

    /// <summary>
    /// Fertility awareness methods.
    /// </summary>
    fertility_awareness,

    /// <summary>
    /// Sterilization procedures.
    /// </summary>
    sterilization,

    /// <summary>
    /// Other contraception method not captured by predefined codes.
    /// </summary>
    other
}
