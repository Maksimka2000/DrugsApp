using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.DrugFeature.ApplicationServices;
using DrugsApp.Features.DrugFeature.Shared;
using DrugsApp.Features.VetDrugFeature.ApplicationServices;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.TempDrugFeature.ApplicationServices;

/// <summary>
/// 
/// </summary>
public interface ITempDrugReviewApplicationService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tempDrugApplicationServiceDraft"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<CreateDrugFromTempDrugReviewTempDrugApplicationServiceResult>> CreateDrugAsync(
        CreateDrugFromTempDrugReviewTempDrugApplicationServiceDraft tempDrugApplicationServiceDraft,
        CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="draft"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<CreateVetDrugFromTempDrugReviewResult>> CreateVetDrugAsync(
        CreateVetDrugFromTempDrugReviewDraft draft,
        CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tempDrugApplicationServiceDraft"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceResult>> CreateDrugFormAsync(
        CreateDrugFormFromTempDrugReviewTempDrugApplicationServiceDraft tempDrugApplicationServiceDraft,
        CancellationToken cancellationToken);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tempDrugApplicationServiceDraft"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceResult>> UpdateDrugFormLinksAsync(
        UpdateDrugFormLinksFromTempDrugReviewTempDrugApplicationServiceDraft tempDrugApplicationServiceDraft,
        CancellationToken cancellationToken);
}


