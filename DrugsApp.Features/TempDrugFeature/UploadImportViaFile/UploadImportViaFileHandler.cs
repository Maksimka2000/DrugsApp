using DrugsApp.Features.TempDrugFeature.ApplicationServices;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.UploadImportViaFile;

/// <summary>
/// Handles file upload import command.
/// </summary>
internal sealed class UploadImportViaFileHandler(ITempDrugImportUploadApplicationService _uploadApplicationService)
    : IRequestHandler<UploadImportViaFileCommand, ErrorOr<CreatedImportBatchResponse>>
{
    
    public async Task<ErrorOr<CreatedImportBatchResponse>> Handle(
        UploadImportViaFileCommand command,
        CancellationToken cancellationToken)
    {
        return await _uploadApplicationService.UploadFromFileAsync(command.Request.File, cancellationToken);
    }
}

