using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.DrugFeature.Shared;
using DrugsApp.Features.VetDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.TempDrugFeature.ApplicationServices;


// Drug 
//
//
/// <summary>
/// DTO special for the temp drug application service for creation and manipulation with data
/// </summary>
/// <param name="TempDrugId">part of the temp drug application service, temp drug id needed for operations like setNote, updateStatus, setLink</param>
/// <param name="Note">Note form the client</param>
/// <param name="CanonicalDrugApplicationServiceDraft"></param>
public sealed record CreateDrugFromTempDrugReviewTempDrugApplicationServiceDraft(
    Guid TempDrugId,
    string? Note,
    CreateDrugFromReviewDrugApplicationServiceDraft CanonicalDrugApplicationServiceDraft);

/// <summary>
/// DTO special for the temp drug return data
/// </summary>
/// <param name="TempDrugId"></param>
/// <param name="NewStatus"></param>
/// <param name="LinkedDrugFormId"></param>
/// <param name="Note"></param>
/// <param name="Drug"></param>
public sealed record CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult(
    Guid TempDrugId,
    TempDrugStatus NewStatus,
    Guid LinkedDrugFormId,
    string? Note,
    DrugDto Drug);


// Drug Form 
//
//
/// <summary>
/// DTO special for the temp drug application service for creation of a drug form from a temporary drug review.
/// </summary>
/// <param name="TempDrugId">The id of the temporary drug.</param>
/// <param name="Note">The note of the temporary drug.</param>
/// <param name="CanonicalDraft">The canonical draft of the temporary drug.</param>
public sealed record CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceDraft(
    Guid TempDrugId,
    string? Note,
    CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft CanonicalDraft);


/// <summary>
/// DTO special for the temp drug application service for creation of a drug form from a temporary drug review result.
/// </summary>
/// <param name="TempDrugId">The id of the temporary drug.</param>
/// <param name="NewStatus">The new status of the temporary drug.</param>
/// <param name="LinkedDrugFormId">The id of the linked drug form.</param>
/// <param name="Note">The note of the temporary drug.</param>
/// <param name="DrugForm">The drug form of the temporary drug.</param>
public sealed record CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult(
    Guid TempDrugId,
    TempDrugStatus NewStatus,
    Guid LinkedDrugFormId,
    string? Note,
    DrugFormDto DrugForm);



// Update Drug Form Links From Temp Drug Review
//
//
/// <summary>
/// DTO special for the temp drug application service for updating drug form links from a temporary drug review.
/// </summary>
/// <param name="TempDrugId">The id of the temporary drug.</param>
/// <param name="Note">The note of the temporary drug.</param>
/// <param name="CanonicalDrugApplicationServiceDraft">The canonical draft of the temporary drug.</param>
public sealed record UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceDraft(
    Guid TempDrugId,
    string? Note,
    UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft CanonicalDrugApplicationServiceDraft);

/// <summary>
/// DTO special for the temp drug application service for updating drug form links from a temporary drug review result.
/// </summary>
/// <param name="TempDrugId">The id of the temporary drug.</param>
/// <param name="NewStatus">The new status of the temporary drug.</param>
/// <param name="LinkedDrugFormId">The id of the linked drug form.</param>
/// <param name="Note">The note of the temporary drug.</param>
/// <param name="DrugForm">The drug form of the temporary drug.</param>
public sealed record UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult(
    Guid TempDrugId,
    TempDrugStatus NewStatus,
    Guid LinkedDrugFormId,
    string? Note,
    DrugFormDto DrugForm);

// Create Veterinary Drug From Temp Drug Review
//
//
/// <summary>
/// DTO special for the temp drug application service for creating a veterinary drug from a temporary drug review.
/// </summary>
/// <param name="TempDrugId">The id of the temporary drug.</param>
/// <param name="Note">The note of the temporary drug.</param>
/// <param name="CanonicalDraft">The canonical draft of the temporary drug.</param>
public sealed record CreateVetDrugFromTempDrugReviewDraft(
    Guid TempDrugId,
    string? Note,
    CreateVetDrugFromReviewVetDrugApplicationServiceDraft CanonicalDraft);

/// <summary>
/// DTO special for the temp drug application service for creating a veterinary drug from a temporary drug review result.
/// </summary>
/// <param name="TempDrugId">The id of the temporary drug.</param>
/// <param name="NewStatus">The new status of the temporary drug.</param>
/// <param name="LinkedVetDrugId">The id of the linked veterinary drug.</param>
/// <param name="Note">The note of the temporary drug.</param>
/// <param name="VetDrug">The veterinary drug of the temporary drug.</param>
public sealed record CreateVetDrugFromTempDrugReviewResult(
    Guid TempDrugId,
    TempDrugStatus NewStatus,
    Guid LinkedVetDrugId,
    string? Note,
    VetDrugDto VetDrug);












/// <summary>
/// The application service for the temp drug review.
/// </summary>
/// <param name="_tempDrugRepository">The repository for the temp drug.</param>
/// <param name="_drugReviewCanonicalizationService">The canonicalization service for the drug review.</param>
/// <param name="_vetDrugCanonicalizationService">The canonicalization service for the vet drug review.</param>
internal sealed class TempDrugReviewApplicationService(
    ITempDrugRepository _tempDrugRepository,
    IDrugReviewCanonicalizationService _drugReviewCanonicalizationService,
    IVetDrugCanonicalizationService _vetDrugCanonicalizationService) : ITempDrugReviewApplicationService
{

    // Create Drug From Temp Drug Review
    //
    //
    /// <summary>
    /// Creates a drug from a temporary drug review.
    /// </summary>
    /// <param name="tempDrugApplicationServiceDraft">The draft of the temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the creation of the drug from the temporary drug review.</returns>
    public async Task<ErrorOr<CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult>> CreateDrugAsync(
        CreateDrugFromTempDrugReviewTempDrugApplicationServiceDraft tempDrugApplicationServiceDraft,
        CancellationToken cancellationToken)
    {
        // Get the temp drug and check if it exists
        TempDrug? tempDrug = await _tempDrugRepository.GetByIdAsync(tempDrugApplicationServiceDraft.TempDrugId, cancellationToken);
        if (tempDrug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "tempDrug",
                    ["resourceId"] = tempDrugApplicationServiceDraft.TempDrugId.ToString()
                });
        }

        // Ensure the temp drug is eligible for finalization
        ErrorOr<Success> eligibilityResult = EnsureEligibleForFinalization(tempDrug);
        if (eligibilityResult.IsError)
        {
            return eligibilityResult.Errors;
        }

        // Create the drug from the temp drug review using the CreateDrugFromReviewDrugApplicationServiceDraft Dto
        ErrorOr<DrugDto> drugResult = await _drugReviewCanonicalizationService.CreateNewDrugFromReviewAsync(tempDrugApplicationServiceDraft.CanonicalDrugApplicationServiceDraft, cancellationToken);
        if (drugResult.IsError)
        {
            return drugResult.Errors;
        }
        DrugDto drug = drugResult.Value;

        // Link the temp drug to the drug form
        ErrorOr<Success> linkResult = tempDrug.LinkToDrugForm(drug.DrugForms.First().Id);
        if (linkResult.IsError)
        {
            return linkResult.Errors;
        }

        // Set the note if it is provided
        if (tempDrugApplicationServiceDraft.Note is not null)
        {
            ErrorOr<Success> noteResult = tempDrug.SetNote(tempDrugApplicationServiceDraft.Note);
            if (noteResult.IsError)
                return noteResult.Errors;
        }

        // Change the status to reviewed added
        ErrorOr<Success> statusResult = tempDrug.ChangeStatus(TempDrugStatus.ReviewedAdded);
        if (statusResult.IsError)
        {
            return statusResult.Errors;
        }
        
        // return the result
        return new CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult(
            TempDrugId: tempDrug.Id,
            NewStatus: tempDrug.Status,
            LinkedDrugFormId: tempDrug.LinkedDrugFormId!.Value,
            Note: tempDrug.Note ?? null,
            Drug: drug);
    }

    // Create Drug Form From Temp Drug Review
    //
    //
    /// <summary>
    /// Creates a drug form from a temporary drug review.
    /// </summary>
    /// <param name="tempDrugApplicationServiceDraft">The draft of the temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the creation of the drug form from the temporary drug review.</returns>
    public async Task<ErrorOr<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult>> CreateDrugFormAsync(
        CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceDraft tempDrugApplicationServiceDraft,
        CancellationToken cancellationToken)
    {
        // Get the temp drug and check if it exists
        TempDrug? tempDrug = await _tempDrugRepository.GetByIdAsync(tempDrugApplicationServiceDraft.TempDrugId, cancellationToken);
        if (tempDrug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "tempDrug",
                    ["resourceId"] = tempDrugApplicationServiceDraft.TempDrugId.ToString()
                });
        }

        // Ensure the temp drug is eligible for finalization
        ErrorOr<Success> eligibilityResult = EnsureEligibleForFinalization(tempDrug);
        if (eligibilityResult.IsError)
        {
            return eligibilityResult.Errors;
        }

        // Create the drug form from the temporary drug review using the CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft Dto
        ErrorOr<DrugFormDto> drugFormResult = await _drugReviewCanonicalizationService.CreateDrugFormForExistingDrugFromReviewAsync(tempDrugApplicationServiceDraft.CanonicalDraft, cancellationToken);
        if (drugFormResult.IsError)
        {
            return drugFormResult.Errors;
        }
        DrugFormDto drugForm = drugFormResult.Value;

        // Link the temp drug to the drug form
        ErrorOr<Success> linkResult = tempDrug.LinkToDrugForm(drugForm.Id);
        if (linkResult.IsError)
        {
            return linkResult.Errors;
        }

        // Set the note if it is provided
        if (tempDrugApplicationServiceDraft.Note is not null)
        {
            ErrorOr<Success> noteResult = tempDrug.SetNote(tempDrugApplicationServiceDraft.Note);
            if (noteResult.IsError)
            {
                return noteResult.Errors;
            }
        }

        // Change the status to reviewed added
        ErrorOr<Success> statusResult = tempDrug.ChangeStatus(TempDrugStatus.ReviewedAdded);
        if (statusResult.IsError)
        {
            return statusResult.Errors;
        }

        return new CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult(
            TempDrugId: tempDrug.Id,
            NewStatus: tempDrug.Status,
            LinkedDrugFormId: tempDrug.LinkedDrugFormId!.Value,
            Note: tempDrug.Note ?? null,
            DrugForm: drugForm);
    }

    // Update Drug Form Links From Temp Drug Review
    //
    //
    /// <summary>
    /// Updates drug form links from a temporary drug review.
    /// </summary>
    /// <param name="tempDrugApplicationServiceDraft">The draft of the temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the update of the drug form links from the temporary drug review.</returns>
    public async Task<ErrorOr<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult>> UpdateDrugFormLinksAsync(
        UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceDraft tempDrugApplicationServiceDraft,
        CancellationToken cancellationToken)
    {
        // Get the temp drug and check if it exists
        TempDrug? tempDrug = await _tempDrugRepository.GetByIdAsync(tempDrugApplicationServiceDraft.TempDrugId, cancellationToken);
        if (tempDrug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "tempDrug",
                    ["resourceId"] = tempDrugApplicationServiceDraft.TempDrugId.ToString()
                });
        }

        // Ensure the temp drug is eligible for finalization
        ErrorOr<Success> eligibilityResult = EnsureEligibleForFinalization(tempDrug);
        if (eligibilityResult.IsError)
        {
            return eligibilityResult.Errors;
        }

        // Update the drug form links from the temporary drug review using the UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft Dto
        ErrorOr<DrugFormDto> canonicalResult = await _drugReviewCanonicalizationService.UpdateDrugFormLinksFromReviewAsync(tempDrugApplicationServiceDraft.CanonicalDrugApplicationServiceDraft, cancellationToken);
        if (canonicalResult.IsError)
        {
            return canonicalResult.Errors;
        }
        DrugFormDto drugForm = canonicalResult.Value;

        // Link the temp drug to the drug form
        ErrorOr<Success> linkResult = tempDrug.LinkToDrugForm(canonicalResult.Value.Id);
        if (linkResult.IsError)
        {
            return linkResult.Errors;
        }

        // Set the note if it is provided
        if (tempDrugApplicationServiceDraft.Note is not null)
        {
            ErrorOr<Success> noteResult = tempDrug.SetNote(tempDrugApplicationServiceDraft.Note);
            if (noteResult.IsError)
            {
                return noteResult.Errors;
            }
        }

        // Change the status to reviewed added  
        ErrorOr<Success> statusResult = tempDrug.ChangeStatus(TempDrugStatus.ReviewedAdded);
        if (statusResult.IsError)
        {
            return statusResult.Errors;
        }

        return new UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult(
            TempDrugId: tempDrug.Id,
            NewStatus: tempDrug.Status,
            LinkedDrugFormId: tempDrug.LinkedDrugFormId!.Value,
            Note: tempDrug.Note,
            DrugForm: drugForm);
    }

    // Create Veterinary Drug From Temp Drug Review
    //
    //
    /// <summary>
    /// Creates a veterinary drug from a temporary drug review.
    /// </summary>
    /// <param name="draft">The draft of the temporary drug review.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<ErrorOr<CreateVetDrugFromTempDrugReviewResult>> CreateVetDrugAsync(
        CreateVetDrugFromTempDrugReviewDraft draft,
        CancellationToken cancellationToken)
    {
        // Get the temp drug and check if it exists
        TempDrug? tempDrug = await _tempDrugRepository.GetByIdAsync(draft.TempDrugId, cancellationToken);
        if (tempDrug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "tempDrug",
                    ["resourceId"] = draft.TempDrugId.ToString()
                });
        }

        // Ensure the temp drug is eligible for finalization
        ErrorOr<Success> eligibilityResult = EnsureEligibleForFinalization(tempDrug);
        if (eligibilityResult.IsError)
        {
            return eligibilityResult.Errors;
        }

        // Create the veterinary drug from the temporary drug review using the CreateVetDrugFromReviewVetDrugApplicationServiceDraft Dto
        ErrorOr<VetDrugDto> vetDrugResult = await _vetDrugCanonicalizationService.CreateVetDrugFromReviewAsync(draft.CanonicalDraft, cancellationToken);
        if (vetDrugResult.IsError)
        {
            return vetDrugResult.Errors;
        }
        VetDrugDto vetDrug = vetDrugResult.Value;

        // Link the temp drug to the veterinary dru
        ErrorOr<Success> linkResult = tempDrug.LinkToVetDrug(vetDrug.Id);
        if (linkResult.IsError)
        {
            return linkResult.Errors;
        }

        // Set the note if it is provided
        if (draft.Note is not null)
        {
            ErrorOr<Success> noteResult = tempDrug.SetNote(draft.Note);
            if (noteResult.IsError)
            {
                return noteResult.Errors;
            }
        }

        // Change the status to reviewed added
        ErrorOr<Success> statusResult = tempDrug.ChangeStatus(TempDrugStatus.ReviewedAdded);
        if (statusResult.IsError)
        {
            return statusResult.Errors;
        }

        // return the result
        return new CreateVetDrugFromTempDrugReviewResult(
            TempDrugId: tempDrug.Id,
            NewStatus: tempDrug.Status,
            LinkedVetDrugId: tempDrug.LinkedDrugVetId!.Value,
            Note: tempDrug.Note,
            VetDrug: vetDrug);
    }

    

    /// <summary>
    ///     This method ensures that the temp drug is eligible for finalization.
    /// </summary>
    /// <param name="tempDrug">The temp drug to ensure eligibility for finalization.</param>
    /// <returns>A success if the temp drug is eligible for finalization.</returns>
    private static ErrorOr<Success> EnsureEligibleForFinalization(TempDrug tempDrug)
    {
        if (tempDrug.Status is TempDrugStatus.ReviewedAdded or TempDrugStatus.ReviewedResolved)
        {
            return Error.Conflict(description: "TempDrug is already in a terminal status.");
        }

        if (tempDrug.LinkedDrugFormId is not null || tempDrug.LinkedDrugVetId is not null)
        {
            return Error.Conflict(description: "TempDrug is already linked to a canonical record.");
        }

        return Result.Success;
    }




   
}

