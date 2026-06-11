using DrugsApp.Domain.NotificationsFeature;
using DrugsApp.Domain.NotificationsFeature.Enums;

namespace DrugsApp.Features.PatientSupervisionFeature.Shared;

/// <summary>
/// Notification projection helpers for supervision requests and medical change proposals only.
/// </summary>
public static class SupervisionNotificationMappings
{
    public const string SupervisionRequestAggregateType = "supervision_request";
    public const string MedicalChangeProposalAggregateType = "medical_change_proposal";

    public const string PatientSupervisionRequestsDeepLink = "/supervision/patient/healthcare-professionals";
    public const string PatientProposalsDeepLink = "/supervision/patient/proposals";

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

    public static string BuildRequestSourceEventKey(Guid requestId) => $"supervision-request:{requestId}";

    public static string BuildProposalSourceEventKey(Guid proposalId) => $"medical-change-proposal:{proposalId}";
}
