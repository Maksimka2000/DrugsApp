using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;
using DrugsApp.Features.RegularFoodFeature.CreateRegularFood;
using DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;

/// <summary>
/// Resolves create requests from persisted proposal snapshots when apply_payload is incomplete.
/// </summary>
public sealed class MedicalChangeProposalApplyRequestResolver(
    IDrugFormRepository drugFormRepository,
    IFoodRepository foodRepository,
    ISupplementRepository supplementRepository)
{
    public async Task<ErrorOr<CreateRegularDrugRequest>> ResolveRegularDrugRequestAsync(
        MedicalChangeProposalItem item,
        CancellationToken cancellationToken)
    {
        if (TryDeserializeRegularDrugPayload(item.ApplyPayload, out CreateRegularDrugRequest? fromPayload)
            && fromPayload.DrugId != Guid.Empty
            && fromPayload.DrugFormId != Guid.Empty)
        {
            return MergeRegularDrugSnapshot(fromPayload, item);
        }

        if (item.RelatedCatalogId is not Guid drugFormId || drugFormId == Guid.Empty)
        {
            return Error.Validation(
                description: "Regular drug proposal is missing drug_form_id.",
                metadata: new() { ["field"] = "related_catalog_id" });
        }

        DrugForm? drugForm = await drugFormRepository.GetByIdAsync(drugFormId, cancellationToken);
        if (drugForm is null)
        {
            return Error.NotFound(description: "Drug form not found.");
        }

        var rebuilt = new CreateRegularDrugRequest(
            DrugId: drugForm.DrugId,
            DrugFormId: drugFormId,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            IntakeTimes: null);

        return MergeRegularDrugSnapshot(rebuilt, item);
    }

    public async Task<ErrorOr<CreateRegularFoodRequest>> ResolveRegularFoodRequestAsync(
        MedicalChangeProposalItem item,
        CancellationToken cancellationToken)
    {
        if (TryDeserializeRegularFoodPayload(item.ApplyPayload, out CreateRegularFoodRequest? fromPayload)
            && fromPayload.FoodId != Guid.Empty)
        {
            return MergeRegularFoodSnapshot(fromPayload, item);
        }

        if (item.RelatedCatalogId is not Guid foodId || foodId == Guid.Empty)
        {
            return Error.Validation(
                description: "Regular food proposal is missing food_id.",
                metadata: new() { ["field"] = "related_catalog_id" });
        }

        if (await foodRepository.GetByIdAsync(foodId, cancellationToken) is null)
        {
            return Error.NotFound(description: "Food not found.");
        }

        var rebuilt = new CreateRegularFoodRequest(
            FoodId: foodId,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            IntakeTimes: null);

        return MergeRegularFoodSnapshot(rebuilt, item);
    }

    public async Task<ErrorOr<CreateRegularSupplementRequest>> ResolveRegularSupplementRequestAsync(
        Guid patientUserId,
        MedicalChangeProposalItem item,
        CancellationToken cancellationToken)
    {
        if (TryDeserializeRegularSupplementPayload(item.ApplyPayload, out CreateRegularSupplementRequest? fromPayload)
            && RegularSupplementMappings.MapSourceFromToken(fromPayload.Source) is not null
            && HasValidSupplementTarget(fromPayload))
        {
            return MergeRegularSupplementSnapshot(fromPayload, item);
        }

        RegularSupplementSource? source = item.Source switch
        {
            MedicalChangeProposalSource.Database => RegularSupplementSource.Database,
            _ => null
        };

        if (source is null)
        {
            return Error.Validation(description: "Regular supplement proposal requires source database.");
        }

        if (item.RelatedCatalogId is not Guid catalogSupplementId || catalogSupplementId == Guid.Empty)
        {
            return Error.Validation(
                description: "Regular supplement proposal is missing supplement_id.",
                metadata: new() { ["field"] = "related_catalog_id" });
        }

        if (await supplementRepository.GetByIdAsync(catalogSupplementId, cancellationToken) is null)
        {
            return Error.NotFound(description: "Supplement not found.");
        }

        var rebuilt = new CreateRegularSupplementRequest(
            Source: "database",
            SupplementId: catalogSupplementId,
            OwnSupplementId: null,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            IntakeTimes: null);

        return MergeRegularSupplementSnapshot(rebuilt, item);
    }

    private static bool TryDeserializeRegularDrugPayload(string payload, out CreateRegularDrugRequest request)
    {
        request = default!;
        ErrorOr<CreateRegularDrugRequest> result = MedicalChangeProposalJson.Deserialize<CreateRegularDrugRequest>(
            payload,
            "apply_payload");
        if (result.IsError)
        {
            return false;
        }

        request = result.Value;
        return true;
    }

    private static bool TryDeserializeRegularFoodPayload(string payload, out CreateRegularFoodRequest request)
    {
        request = default!;
        ErrorOr<CreateRegularFoodRequest> result = MedicalChangeProposalJson.Deserialize<CreateRegularFoodRequest>(
            payload,
            "apply_payload");
        if (result.IsError)
        {
            return false;
        }

        request = result.Value;
        return true;
    }

    private static bool TryDeserializeRegularSupplementPayload(string payload, out CreateRegularSupplementRequest request)
    {
        request = default!;
        ErrorOr<CreateRegularSupplementRequest> result = MedicalChangeProposalJson.Deserialize<CreateRegularSupplementRequest>(
            payload,
            "apply_payload");
        if (result.IsError)
        {
            return false;
        }

        request = result.Value;
        return true;
    }

    private static bool HasValidSupplementTarget(CreateRegularSupplementRequest request)
    {
        RegularSupplementSource? source = RegularSupplementMappings.MapSourceFromToken(request.Source);
        return source switch
        {
            RegularSupplementSource.Database => request.SupplementId is Guid supplementId && supplementId != Guid.Empty,
            _ => false
        };
    }

    private static CreateRegularDrugRequest MergeRegularDrugSnapshot(
        CreateRegularDrugRequest request,
        MedicalChangeProposalItem item)
    {
        request = MergeIntakeTimes(request, item);
        return request with
        {
            StartDate = request.StartDate ?? item.StartDate,
            EndDate = request.EndDate ?? item.EndDate
        };
    }

    private static CreateRegularFoodRequest MergeRegularFoodSnapshot(
        CreateRegularFoodRequest request,
        MedicalChangeProposalItem item)
    {
        request = MergeIntakeTimes(request, item);
        return request with
        {
            StartDate = request.StartDate ?? item.StartDate,
            EndDate = request.EndDate ?? item.EndDate
        };
    }

    private static CreateRegularSupplementRequest MergeRegularSupplementSnapshot(
        CreateRegularSupplementRequest request,
        MedicalChangeProposalItem item)
    {
        request = MergeIntakeTimes(request, item);
        return request with
        {
            StartDate = request.StartDate ?? item.StartDate,
            EndDate = request.EndDate ?? item.EndDate
        };
    }

    private static CreateRegularDrugRequest MergeIntakeTimes(
        CreateRegularDrugRequest request,
        MedicalChangeProposalItem item)
    {
        if (request.IntakeTimes is { Count: > 0 })
        {
            return request;
        }

        if (item.IntakeTimes.Count == 0)
        {
            return request;
        }

        return request with
        {
            IntakeTimes = item.IntakeTimes.Select(intake => intake.IntakeTime).ToArray()
        };
    }

    private static CreateRegularFoodRequest MergeIntakeTimes(
        CreateRegularFoodRequest request,
        MedicalChangeProposalItem item)
    {
        if (request.IntakeTimes is { Count: > 0 })
        {
            return request;
        }

        if (item.IntakeTimes.Count == 0)
        {
            return request;
        }

        return request with
        {
            IntakeTimes = item.IntakeTimes.Select(intake => intake.IntakeTime).ToArray()
        };
    }

    private static CreateRegularSupplementRequest MergeIntakeTimes(
        CreateRegularSupplementRequest request,
        MedicalChangeProposalItem item)
    {
        if (request.IntakeTimes is { Count: > 0 })
        {
            return request;
        }

        if (item.IntakeTimes.Count == 0)
        {
            return request;
        }

        return request with
        {
            IntakeTimes = item.IntakeTimes.Select(intake => intake.IntakeTime).ToArray()
        };
    }
}
