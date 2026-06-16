using DrugsApp.Domain.NotificationsFeature;
using DrugsApp.Domain.NotificationsFeature.Enums;

namespace DrugsApp.Features.PatientSupervisionFeature.Shared;

/// <summary>
/// Notification projection helpers for supervision domain events.
/// </summary>
public static class SupervisionNotificationMappings
{
    public const string SupervisionRequestAggregateType = "supervision_request";

    public const string MedicalChangeProposalAggregateType = "medical_change_proposal";

    public const string SupervisionConnectionAggregateType = "supervision_connection";

    public const string PatientSupervisionRequestsDeepLink = "/supervision/patient/requests";

    public const string PatientHealthcareProfessionalsDeepLink = "/supervision/patient/healthcare-professionals";

    public const string PatientProposalsDeepLink = "/supervision/patient/proposals";

    public const string ProviderPatientsDeepLink = "/supervision/provider/patients";

    public static string BuildPatientRequestDeepLink(Guid requestId) =>
        $"{PatientSupervisionRequestsDeepLink}/{requestId}";

    public static string BuildPatientHealthcareProfessionalDeepLink(Guid healthcareProfessionalUserId) =>
        $"{PatientHealthcareProfessionalsDeepLink}/{healthcareProfessionalUserId}";

    public static string BuildPatientProposalDeepLink(Guid proposalId) =>
        $"{PatientProposalsDeepLink}/{proposalId}";

    public static string BuildProviderPatientDeepLink(Guid patientUserId) =>
        $"{ProviderPatientsDeepLink}/{patientUserId}";

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

    public static string BuildRequestSourceEventKey(Guid requestId) => $"supervision-request:{requestId}";

    public static string BuildProposalSourceEventKey(Guid proposalId) => $"medical-change-proposal:{proposalId}";

    public static string BuildAccessLevelChangedSourceEventKey(Guid connectionId) =>
        $"supervision-connection-access-level:{connectionId}";

    public static string BuildRemovedByPatientSourceEventKey(Guid connectionId) =>
        $"supervision-connection-removed-by-patient:{connectionId}";

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
