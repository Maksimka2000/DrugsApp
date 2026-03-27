using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.UploadImportViaUrl;

/// <summary>
/// Handles URL upload import command.
/// </summary>
internal sealed class UploadImportViaUrlHandler(ITempDrugImportUploadApplicationService _uploadApplicationService)
    : IRequestHandler<UploadImportViaUrlCommand, ErrorOr<CreatedImportBatchResponse>>
{
    public async Task<ErrorOr<CreatedImportBatchResponse>> Handle(
        UploadImportViaUrlCommand command,
        CancellationToken cancellationToken)
    {
        return await _uploadApplicationService.UploadFromUrlAsync(command.Request.Url, cancellationToken);
    }
}