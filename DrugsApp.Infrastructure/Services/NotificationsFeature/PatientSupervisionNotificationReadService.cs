using System;

using System.Collections.Generic;

using System.Linq;

using System.Threading;

using System.Threading.Tasks;

using DrugsApp.Domain.NotificationsFeature.Interfaces;

using DrugsApp.Domain.PatientSupervisionFeature.Entities;

using DrugsApp.Domain.PatientSupervisionFeature.Enums;

using DrugsApp.Domain.PatientSupervisionFeature.Repositories;

using DrugsApp.Domain.UserProfileFeature.Entities;

using DrugsApp.Domain.UserProfileFeature.Repositories;

using ErrorOr;



namespace DrugsApp.Infrastructure.Services.NotificationsFeature;



public sealed class PatientSupervisionNotificationReadService(

    ISupervisionReadRepository supervisionReadRepository,

    ISupervisionRequestRepository supervisionRequestRepository,

    IMedicalChangeProposalRepository proposalRepository,

    IUserProfileRepository userProfileRepository)

    : IPatientSupervisionNotificationReadService

{

    public Task<ErrorOr<SupervisionRequestNotificationDetail>> GetRequestDetailAsync(

        Guid requestId,

        Guid userId,

        CancellationToken cancellationToken)

    {

        return GetRequestDetailInternalAsync(requestId, userId, cancellationToken);

    }



    public Task<ErrorOr<MedicalChangeProposalNotificationDetail>> GetProposalDetailAsync(

        Guid proposalId,

        Guid userId,

        CancellationToken cancellationToken)

    {

        return GetProposalDetailInternalAsync(proposalId, userId, cancellationToken);

    }



    private async Task<ErrorOr<SupervisionRequestNotificationDetail>> GetRequestDetailInternalAsync(

        Guid requestId,

        Guid userId,

        CancellationToken cancellationToken)

    {

        SupervisionRequest? request = await supervisionRequestRepository.GetByIdForPatientAsync(

            userId,

            requestId,

            track: false,

            cancellationToken);



        bool viewerIsPatient = request is not null;

        if (!viewerIsPatient)

        {

            request = await supervisionRequestRepository.GetByIdForProviderAsync(

                userId,

                requestId,

                track: false,

                cancellationToken);

        }



        if (request is null)

        {

            return Error.NotFound(

                description: "Supervision request not found.",

                metadata: new()

                {

                    ["resourceType"] = "SupervisionRequest",

                    ["resourceId"] = requestId.ToString()

                });

        }



        bool isExpired = request.RequestState == SupervisionRequestState.Pending

                         && DateTime.UtcNow >= request.ExpiresAtUtc;



        string counterpartySummary = await ResolveCounterpartySummaryAsync(

            request,

            viewerIsPatient,

            cancellationToken);



        IReadOnlyList<string> accessLevels = viewerIsPatient

            ?

            [

                "view_and_modify",

                "view_only",

                "restricted_view",

                "block_access"

            ]

            : Array.Empty<string>();



        return new SupervisionRequestNotificationDetail(

            RequestId: request.Id,

            HealthcareProfessionalSummary: counterpartySummary,

            RequestMessage: null,

            AvailableAccessLevels: accessLevels,

            SelectedAccessLevelCode: null,

            CurrentRequestState: isExpired

                ? SupervisionRequestState.Expired.ToString()

                : request.RequestState.ToString());

    }



    private async Task<ErrorOr<MedicalChangeProposalNotificationDetail>> GetProposalDetailInternalAsync(

        Guid proposalId,

        Guid userId,

        CancellationToken cancellationToken)

    {

        MedicalChangeProposalDetail? detail = await proposalRepository.GetDetailForParticipantAsync(

            userId,

            proposalId,

            cancellationToken);



        if (detail is null)

        {

            return Error.NotFound(

                description: "Medical change proposal not found.",

                metadata: new()

                {

                    ["resourceType"] = "MedicalChangeProposal",

                    ["resourceId"] = proposalId.ToString()

                });

        }



        bool viewerIsPatient = detail.PatientUserId == userId;

        string counterpartySummary = viewerIsPatient

            ? await ResolveHealthcareProfessionalSummaryAsync(detail.PatientUserId, detail.HealthcareProfessionalUserId, cancellationToken)

            : await ResolvePatientSummaryAsync(detail.PatientUserId, cancellationToken);



        IReadOnlyList<MedicalChangeProposalSectionDetail> sections = detail.DraftItems
            .GroupBy(item => item.CategoryCode)
            .Select(group =>
            {
                string summaryText = string.Join(
                    ", ",
                    group.Select(i => i.DisplayName).Where(n => !string.IsNullOrWhiteSpace(n)));

                if (string.IsNullOrWhiteSpace(summaryText))
                {
                    summaryText = "No items";
                }

                return new MedicalChangeProposalSectionDetail(
                    SectionCode: group.Key,
                    SectionTitle: group.Key,
                    SummaryText: summaryText);
            })
            .ToList();



        return new MedicalChangeProposalNotificationDetail(

            ProposalId: detail.ProposalId,

            HealthcareProfessionalSummary: counterpartySummary,

            ProposalSummary: detail.ProposalSummary,

            Sections: sections,

            ProposalNote: null,

            CurrentProposalState: detail.ProposalState);

    }



    private async Task<string> ResolveCounterpartySummaryAsync(

        SupervisionRequest request,

        bool viewerIsPatient,

        CancellationToken cancellationToken)

    {

        if (viewerIsPatient)

        {

            return await ResolveHealthcareProfessionalSummaryAsync(

                request.PatientUserId,

                request.HealthcareProfessionalUserId,

                cancellationToken);

        }



        return await ResolvePatientSummaryAsync(request.PatientUserId, cancellationToken);

    }



    private async Task<string> ResolveHealthcareProfessionalSummaryAsync(

        Guid patientUserId,

        Guid healthcareProfessionalUserId,

        CancellationToken cancellationToken)

    {

        PatientHealthcareProfessionalDetail? providerDetail = await supervisionReadRepository

            .GetPatientHealthcareProfessionalDetailAsync(

                patientUserId,

                healthcareProfessionalUserId,

                cancellationToken);



        if (providerDetail is not null)

        {

            string summary = $"{providerDetail.FirstName} {providerDetail.LastName}".Trim();

            if (!string.IsNullOrWhiteSpace(summary))

            {

                return summary;

            }

        }



        return await ResolveUserDisplayNameAsync(healthcareProfessionalUserId, "Healthcare professional", cancellationToken);

    }



    private async Task<string> ResolvePatientSummaryAsync(Guid patientUserId, CancellationToken cancellationToken) =>

        await ResolveUserDisplayNameAsync(patientUserId, "Patient", cancellationToken);



    private async Task<string> ResolveUserDisplayNameAsync(

        Guid userId,

        string fallback,

        CancellationToken cancellationToken)

    {

        UserProfile? profile = await userProfileRepository.GetByUserIdAsync(userId, cancellationToken);

        if (profile is null)

        {

            return fallback;

        }



        string displayName = $"{profile.FirstName} {profile.LastName}".Trim();

        return string.IsNullOrWhiteSpace(displayName) ? fallback : displayName;

    }

}


