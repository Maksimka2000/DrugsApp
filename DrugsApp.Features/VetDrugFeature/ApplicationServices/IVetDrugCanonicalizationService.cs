using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.VetDrugFeature.ApplicationServices;

/// <summary>
/// 
/// </summary>
public interface IVetDrugCanonicalizationService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="vetDrugApplicationServiceDraft"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<VetDrugDto>> CreateVetDrugFromReviewAsync(
        CreateVetDrugFromReviewVetDrugApplicationServiceDraft vetDrugApplicationServiceDraft,
        CancellationToken cancellationToken);
}
