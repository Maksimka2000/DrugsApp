using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.GetStudyAccountCredentials;

/// <summary>
/// Handles credential reveal: member lookup and password unprotection.
/// </summary>
internal sealed class GetStudyAccountCredentialsHandler(
    IStudyClinicRepository _studyClinicRepository,
    IStudyCredentialProtector _studyCredentialProtector)
    : IRequestHandler<GetStudyAccountCredentialsQuery, ErrorOr<StudyAccountCredentialsDto>>
{
    /// <summary>
    /// Loads the member and decrypts its stored password.
    /// </summary>
    /// <param name="query">Credentials query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Credentials, not found, or <c>StudyMode.CredentialsUnavailable</c>.</returns>
    public async Task<ErrorOr<StudyAccountCredentialsDto>> Handle(
        GetStudyAccountCredentialsQuery query,
        CancellationToken cancellationToken)
    {
        StudyClinicMember? member = await _studyClinicRepository.GetMemberAsync(
            query.ClinicId,
            query.UserId,
            cancellationToken);
        if (member is null)
        {
            return StudyModeErrors.MemberNotFound(query.UserId, "userId");
        }

        ErrorOr<string> passwordResult = _studyCredentialProtector.Unprotect(member.ProtectedPassword);
        if (passwordResult.IsError)
        {
            return passwordResult.Errors;
        }

        return member.MapToCredentialsDto(passwordResult.Value);
    }
}
