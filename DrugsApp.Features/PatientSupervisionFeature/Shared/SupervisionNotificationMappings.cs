using DrugsApp.Domain.NotificationsFeature;
using DrugsApp.Domain.NotificationsFeature.Enums;

namespace DrugsApp.Features.PatientSupervisionFeature.Shared;

/// <summary>
/// Notification projection helpers for supervision domain events.
/// </summary>
public static class SupervisionNotificationMappings
{
    /// <summary>
    /// Aggregate type key for supervision requests.
    /// </summary>
    public const string SupervisionRequestAggregateType = "supervision_request";

    /// <summary>
    /// Aggregate type key for medical change proposals.
    /// </summary>
    public const string MedicalChangeProposalAggregateType = "medical_change_proposal";

    /// <summary>
    /// Aggregate type key for supervision connections.
    /// </summary>
    public const string SupervisionConnectionAggregateType = "supervision_connection";

    /// <summary>
    /// Patient deep link base for supervision requests.
    /// </summary>
    public const string PatientSupervisionRequestsDeepLink = "/supervision/patient/requests";

    /// <summary>
    /// Patient deep link base for healthcare professionals.
    /// </summary>
    public const string PatientHealthcareProfessionalsDeepLink = "/supervision/patient/healthcare-professionals";

    /// <summary>
    /// Patient deep link base for proposals.
    /// </summary>
    public const string PatientProposalsDeepLink = "/supervision/patient/proposals";

    /// <summary>
    /// Provider deep link base for patient list.
    /// </summary>
    public const string ProviderPatientsDeepLink = "/supervision/provider/patients";

    /// <summary>Builds a patient deep link to a supervision request.</summary>
    /// <param name="requestId">Request identifier.</param>
    /// <returns>Deep link path.</returns>
    public static string BuildPatientRequestDeepLink(Guid requestId) =>
        $"{PatientSupervisionRequestsDeepLink}/{requestId}";

    /// <summary>Builds a patient deep link to a healthcare professional.</summary>
    /// <param name="healthcareProfessionalUserId">Healthcare professional user id.</param>
    /// <returns>Deep link path.</returns>
    public static string BuildPatientHealthcareProfessionalDeepLink(Guid healthcareProfessionalUserId) =>
        $"{PatientHealthcareProfessionalsDeepLink}/{healthcareProfessionalUserId}";

    /// <summary>Builds a patient deep link to a proposal.</summary>
    /// <param name="proposalId">Proposal identifier.</param>
    /// <returns>Deep link path.</returns>
    public static string BuildPatientProposalDeepLink(Guid proposalId) =>
        $"{PatientProposalsDeepLink}/{proposalId}";

    /// <summary>Builds a provider deep link to a patient.</summary>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <returns>Deep link path.</returns>
    public static string BuildProviderPatientDeepLink(Guid patientUserId) =>
        $"{ProviderPatientsDeepLink}/{patientUserId}";

    /// <summary>Creates a projection draft for a new supervision request.</summary>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="requestId">Request identifier.</param>
    /// <param name="healthcareProfessionalSummary">Summary of the healthcare professional.</param>
    /// <returns>Notification projection draft.</returns>
    public static NotificationProjectionDraft BuildRequestCreatedDraft(
        Guid patientUserId,
        Guid requestId,
        string healthcareProfessionalSummary) =>
        new(
            UserId: patientUserId,
            SystemAnnouncementId: null,
            TypeCode: UserNotificationType.SupervisionRequest,
            PrimaryTagCode: NotificationTag.DoctorPatient,
            AudienceScopeCode: NotificationAudienceScope.PatientOnly,
            Title: "New supervision request",
            SummaryText: $"{healthcareProfessionalSummary} sent you a supervision request.",
            BodyText: null,
            SourceEventKey: BuildRequestSourceEventKey(requestId),
            RelatedAggregateType: SupervisionRequestAggregateType,
            RelatedAggregateId: requestId,
            DeepLinkPath: BuildPatientRequestDeepLink(requestId),
            RequiresAction: true,
            ActionState: NotificationActionState.Pending);

    /// <summary>Creates a projection draft for a submitted medical change proposal.</summary>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="proposalId">Proposal identifier.</param>
    /// <param name="proposalSummary">Proposal summary.</param>
    /// <returns>Notification projection draft.</returns>
    public static NotificationProjectionDraft BuildProposalSubmittedDraft(
        Guid patientUserId,
        Guid proposalId,
        string proposalSummary) =>
        new(
            UserId: patientUserId,
            SystemAnnouncementId: null,
            TypeCode: UserNotificationType.MedicalChangeProposal,
            PrimaryTagCode: NotificationTag.MedicalData,
            AudienceScopeCode: NotificationAudienceScope.PatientOnly,
            Title: "New medical change proposal",
            SummaryText: proposalSummary,
            BodyText: null,
            SourceEventKey: BuildProposalSourceEventKey(proposalId),
            RelatedAggregateType: MedicalChangeProposalAggregateType,
            RelatedAggregateId: proposalId,
            DeepLinkPath: BuildPatientProposalDeepLink(proposalId),
            RequiresAction: true,
            ActionState: NotificationActionState.Pending);

    /// <summary>Creates a projection draft when a patient changes access level.</summary>
    /// <param name="providerUserId">Provider user identifier.</param>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="connectionId">Connection identifier.</param>
    /// <param name="patientSummary">Patient summary text.</param>
    /// <param name="accessLevelCode">New access level code.</param>
    /// <returns>Notification projection draft.</returns>
    public static NotificationProjectionDraft BuildAccessLevelChangedDraft(
        Guid providerUserId,
        Guid patientUserId,
        Guid connectionId,
        string patientSummary,
        string accessLevelCode) =>
        new(
            UserId: providerUserId,
            SystemAnnouncementId: null,
            TypeCode: UserNotificationType.SupervisionAccessLevelChanged,
            PrimaryTagCode: NotificationTag.DoctorPatient,
            AudienceScopeCode: NotificationAudienceScope.ProviderOnly,
            Title: "Patient updated access level",
            SummaryText: $"{patientSummary} changed your access level to {FormatAccessLevelLabel(accessLevelCode)}.",
            BodyText: null,
            SourceEventKey: BuildAccessLevelChangedSourceEventKey(connectionId),
            RelatedAggregateType: SupervisionConnectionAggregateType,
            RelatedAggregateId: patientUserId,
            DeepLinkPath: BuildProviderPatientDeepLink(patientUserId),
            RequiresAction: false,
            ActionState: NotificationActionState.Informational);

    /// <summary>Creates a projection draft when a patient removes supervision.</summary>
    /// <param name="providerUserId">Provider user identifier.</param>
    /// <param name="connectionId">Connection identifier.</param>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="patientSummary">Patient summary text.</param>
    /// <returns>Notification projection draft.</returns>
    public static NotificationProjectionDraft BuildRemovedByPatientDraft(
        Guid providerUserId,
        Guid connectionId,
        Guid patientUserId,
        string patientSummary) =>
        new(
            UserId: providerUserId,
            SystemAnnouncementId: null,
            TypeCode: UserNotificationType.SupervisionRemovedByPatient,
            PrimaryTagCode: NotificationTag.DoctorPatient,
            AudienceScopeCode: NotificationAudienceScope.ProviderOnly,
            Title: "Patient removed supervision",
            SummaryText: $"{patientSummary} ended the supervision relationship.",
            BodyText: null,
            SourceEventKey: BuildRemovedByPatientSourceEventKey(connectionId),
            RelatedAggregateType: SupervisionConnectionAggregateType,
            RelatedAggregateId: patientUserId,
            DeepLinkPath: BuildProviderPatientDeepLink(patientUserId),
            RequiresAction: false,
            ActionState: NotificationActionState.Informational);

    /// <summary>Creates a projection draft when a provider removes supervision.</summary>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="connectionId">Connection identifier.</param>
    /// <param name="providerUserId">Provider user identifier.</param>
    /// <param name="providerSummary">Provider summary text.</param>
    /// <returns>Notification projection draft.</returns>
    public static NotificationProjectionDraft BuildRemovedByDoctorDraft(
        Guid patientUserId,
        Guid connectionId,
        Guid providerUserId,
        string providerSummary) =>
        new(
            UserId: patientUserId,
            SystemAnnouncementId: null,
            TypeCode: UserNotificationType.SupervisionRemovedByDoctor,
            PrimaryTagCode: NotificationTag.DoctorPatient,
            AudienceScopeCode: NotificationAudienceScope.PatientOnly,
            Title: "Healthcare provider removed supervision",
            SummaryText: $"{providerSummary} ended the supervision relationship.",
            BodyText: null,
            SourceEventKey: BuildRemovedByDoctorSourceEventKey(connectionId),
            RelatedAggregateType: SupervisionConnectionAggregateType,
            RelatedAggregateId: providerUserId,
            DeepLinkPath: BuildPatientHealthcareProfessionalDeepLink(providerUserId),
            RequiresAction: false,
            ActionState: NotificationActionState.Informational);

    /// <summary>
    /// Builds a source event key for a supervision request.
    /// </summary>
    /// <param name="requestId">Request identifier.</param>
    /// <returns>Source event key.</returns>
    public static string BuildRequestSourceEventKey(Guid requestId) => $"supervision-request:{requestId}";

    /// <summary>
    /// Builds a source event key for a medical change proposal.
    /// </summary>
    /// <param name="proposalId">Proposal identifier.</param>
    /// <returns>Source event key.</returns>
    public static string BuildProposalSourceEventKey(Guid proposalId) => $"medical-change-proposal:{proposalId}";

    /// <summary>
    /// Builds a source event key for an access level change.
    /// </summary>
    /// <param name="connectionId">Connection identifier.</param>
    /// <returns>Source event key.</returns>
    public static string BuildAccessLevelChangedSourceEventKey(Guid connectionId) =>
        $"supervision-connection-access-level:{connectionId}";

    /// <summary>
    /// Builds a source event key for patient-initiated removal.
    /// </summary>
    /// <param name="connectionId">Connection identifier.</param>
    /// <returns>Source event key.</returns>
    public static string BuildRemovedByPatientSourceEventKey(Guid connectionId) =>
        $"supervision-connection-removed-by-patient:{connectionId}";

    /// <summary>
    /// Builds a source event key for provider-initiated removal.
    /// </summary>
    /// <param name="connectionId">Connection identifier.</param>
    /// <returns>Source event key.</returns>
    public static string BuildRemovedByDoctorSourceEventKey(Guid connectionId) =>
        $"supervision-connection-removed-by-doctor:{connectionId}";

    private static string FormatAccessLevelLabel(string accessLevelCode) =>
        accessLevelCode.ToLowerInvariant() switch
        {
            "viewandmodify" or "view_and_modify" => "view and modify",
            "viewonly" or "view_only" => "view only",
            "restrictedview" or "restricted_view" => "restricted view",
            "blockaccess" or "block_access" => "block access",
            _ => accessLevelCode
        };
}
