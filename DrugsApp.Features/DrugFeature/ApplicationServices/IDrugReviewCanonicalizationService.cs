using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.DrugFeature.ApplicationServices;

/// <summary>
/// 
/// </summary>
public interface IDrugReviewCanonicalizationService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="drugApplicationServiceDraft"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<DrugDto>> CreateNewDrugFromReviewAsync(
        CreateDrugFromReviewDrugApplicationServiceDraft drugApplicationServiceDraft,
        CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="drugApplicationServiceDraft"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<DrugFormDto>> CreateDrugFormForExistingDrugFromReviewAsync(
        CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft drugApplicationServiceDraft,
        CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="drugApplicationServiceDraft"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<DrugFormDto>> UpdateDrugFormLinksFromReviewAsync(
        UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft drugApplicationServiceDraft,
        CancellationToken cancellationToken);
}



