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

    public const string PatientSupervisionRequestsDeepLink = "/supervision/patient/healthcare-professionals";
    public const string PatientProposalsDeepLink = "/supervision/patient/proposals";
    public const string ProviderPatientsDeepLink = "/supervision/provider/patients";

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
            DeepLinkPath: PatientSupervisionRequestsDeepLink,
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
            DeepLinkPath: PatientProposalsDeepLink,
            RequiresAction: true,
            ActionState: NotificationActionState.Pending);

    public static NotificationProjectionDraft BuildAccessLevelChangedDraft(
        Guid providerUserId,
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
            RelatedAggregateId: connectionId,
            DeepLinkPath: $"{ProviderPatientsDeepLink}/{connectionId}",
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
            DeepLinkPath: ProviderPatientsDeepLink,
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
            DeepLinkPath: PatientSupervisionRequestsDeepLink,
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
            "view_and_modify" => "view and modify",
            "view_only" => "view only",
            "restricted_view" => "restricted view",
            "block_access" => "block access",
            _ => accessLevelCode
        };
}
