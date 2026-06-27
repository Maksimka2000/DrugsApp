using System.Text.Json;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.FoodFeature.Entities;
using DrugsApp.Domain.FoodFeature.Repositories;
using DrugsApp.Domain.PatientSupervisionFeature;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Domain.SupplementFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;
using DrugsApp.Features.RegularFoodFeature.CreateRegularFood;
using DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;
using ErrorOr;

namespace DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;

/// <summary>
/// Builds proposal draft items and apply payloads from structured provider input.
/// </summary>
/// <summary>
/// Builds proposal draft items and apply payloads from structured provider input.
/// </summary>
public sealed class ProposalDraftItemComposer(
    IDrugFormRepository drugFormRepository,
    IFoodRepository foodRepository,
    ISupplementRepository supplementRepository) : IProposalDraftItemComposer
{
    private static readonly JsonSerializerOptions ApplyPayloadSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Composes proposal draft items for a patient from inputs.
    /// </summary>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="draftItems">Draft items to compose.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Composed draft items with display names.</returns>
    public async Task<ErrorOr<IReadOnlyCollection<ComposedProposalDraftItem>>> ComposeAsync(
        Guid patientUserId,
        IReadOnlyCollection<ProposalDraftItemInput> draftItems,
        CancellationToken cancellationToken)
    {
        if (draftItems.Count == 0)
        {
            return Error.Validation(description: "At least one draft item is required.");
        }

        List<ComposedProposalDraftItem> composed = new();

        foreach (ProposalDraftItemInput item in draftItems)
        {
            ErrorOr<ComposedProposalDraftItem> result = await ComposeItemAsync(item, cancellationToken);
            if (result.IsError)
            {
                return result.Errors;
            }

            composed.Add(result.Value);
        }

        return composed;
    }

    private async Task<ErrorOr<ComposedProposalDraftItem>> ComposeItemAsync(
        ProposalDraftItemInput item,
        CancellationToken cancellationToken)
    {
        if (!MedicalChangeProposalCategoryCatalog.TryParseCategory(item.CategoryCode, out MedicalChangeProposalCategory category))
        {
            return Error.Validation(
                description: "Invalid category_code.",
                metadata: new() { ["field"] = "category_code" });
        }

        return category switch
        {
            MedicalChangeProposalCategory.RegularDrug => await ComposeRegularDrugAsync(item, cancellationToken),
            MedicalChangeProposalCategory.RegularFood => await ComposeRegularFoodAsync(item, cancellationToken),
            MedicalChangeProposalCategory.RegularSupplement => await ComposeRegularSupplementAsync(item, cancellationToken),
            _ => Error.Validation(
                description: "category_code must be one of: regular_drug, regular_food, regular_supplement.",
                metadata: new() { ["field"] = "category_code" })
        };
    }

    private async Task<ErrorOr<ComposedProposalDraftItem>> ComposeRegularDrugAsync(
        ProposalDraftItemInput item,
        CancellationToken cancellationToken)
    {
        if (item.RelatedCatalogId is not Guid drugFormId || drugFormId == Guid.Empty)
        {
            return Error.Validation(
                description: "regular_drug requires related_catalog_id (drug_form_id).",
                metadata: new() { ["field"] = "related_catalog_id" });
        }

        DrugForm? drugForm = await drugFormRepository.GetByIdAsync(drugFormId, cancellationToken);
        if (drugForm is null)
        {
            return Error.NotFound(description: "Drug form not found.");
        }

        var request = new CreateRegularDrugRequest(
            DrugId: drugForm.DrugId,
            DrugFormId: drugFormId,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            IntakeTimes: item.IntakeTimes);

        string applyPayload = JsonSerializer.Serialize(request, ApplyPayloadSerializerOptions);
        string displayName = drugForm.Drug.CommercialName;

        var draft = new ProposalDraftItem(
            CategoryCode: MedicalChangeProposalCategory.RegularDrug,
            Source: MedicalChangeProposalSource.Database,
            RelatedCatalogId: drugFormId,
            CustomValue: null,
            ApplyPayload: applyPayload,
            IntakeTimes: item.IntakeTimes,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            Note: item.Note);

        return new ComposedProposalDraftItem(draft, displayName);
    }

    private async Task<ErrorOr<ComposedProposalDraftItem>> ComposeRegularFoodAsync(
        ProposalDraftItemInput item,
        CancellationToken cancellationToken)
    {
        if (item.RelatedCatalogId is not Guid foodId || foodId == Guid.Empty)
        {
            return Error.Validation(
                description: "regular_food requires related_catalog_id (food_id).",
                metadata: new() { ["field"] = "related_catalog_id" });
        }

        Food? food = await foodRepository.GetByIdAsync(foodId, cancellationToken);
        if (food is null)
        {
            return Error.NotFound(description: "Food not found.");
        }

        var request = new CreateRegularFoodRequest(
            FoodId: foodId,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            IntakeTimes: item.IntakeTimes);

        string applyPayload = JsonSerializer.Serialize(request, ApplyPayloadSerializerOptions);

        var draft = new ProposalDraftItem(
            CategoryCode: MedicalChangeProposalCategory.RegularFood,
            Source: MedicalChangeProposalSource.Database,
            RelatedCatalogId: foodId,
            CustomValue: null,
            ApplyPayload: applyPayload,
            IntakeTimes: item.IntakeTimes,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            Note: item.Note);

        return new ComposedProposalDraftItem(draft, food.Name);
    }

    private async Task<ErrorOr<ComposedProposalDraftItem>> ComposeRegularSupplementAsync(
        ProposalDraftItemInput item,
        CancellationToken cancellationToken)
    {
        if (!MedicalChangeProposalCategoryCatalog.TryParseSource(item.Source, out MedicalChangeProposalSource source))
        {
            return Error.Validation(
                description: "regular_supplement requires source (database or custom).",
                metadata: new() { ["field"] = "source" });
        }

        if (source == MedicalChangeProposalSource.Database)
        {
            if (item.RelatedCatalogId is not Guid catalogSupplementId || catalogSupplementId == Guid.Empty)
            {
                return Error.Validation(
                    description: "regular_supplement with source=database requires related_catalog_id (supplement_id).",
                    metadata: new() { ["field"] = "related_catalog_id" });
            }

            Supplement? supplement = await supplementRepository.GetByIdAsync(catalogSupplementId, cancellationToken);
            if (supplement is null)
            {
                return Error.NotFound(description: "Supplement not found.");
            }

            var request = new CreateRegularSupplementRequest(
                Source: "database",
                SupplementId: catalogSupplementId,
                OwnSupplementId: null,
                StartDate: item.StartDate,
                EndDate: item.EndDate,
                IntakeTimes: item.IntakeTimes);

            string applyPayload = JsonSerializer.Serialize(request, ApplyPayloadSerializerOptions);

            var draft = new ProposalDraftItem(
                CategoryCode: MedicalChangeProposalCategory.RegularSupplement,
                Source: MedicalChangeProposalSource.Database,
                RelatedCatalogId: catalogSupplementId,
                CustomValue: null,
                ApplyPayload: applyPayload,
                IntakeTimes: item.IntakeTimes,
                StartDate: item.StartDate,
                EndDate: item.EndDate,
                Note: item.Note);

            return new ComposedProposalDraftItem(draft, supplement.Name);
        }

        string customName = item.CustomValue?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(customName))
        {
            return Error.Validation(
                description: "regular_supplement with source=custom requires custom_value (supplement name).",
                metadata: new() { ["field"] = "custom_value" });
        }

        var customPayload = new ProposedCustomRegularSupplementApplyPayload(
            CustomSupplementName: customName,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            IntakeTimes: item.IntakeTimes);

        string customApplyPayload = JsonSerializer.Serialize(customPayload, ApplyPayloadSerializerOptions);

        var customDraft = new ProposalDraftItem(
            CategoryCode: MedicalChangeProposalCategory.RegularSupplement,
            Source: MedicalChangeProposalSource.Custom,
            RelatedCatalogId: null,
            CustomValue: customName,
            ApplyPayload: customApplyPayload,
            IntakeTimes: item.IntakeTimes,
            StartDate: item.StartDate,
            EndDate: item.EndDate,
            Note: item.Note);

        return new ComposedProposalDraftItem(customDraft, customName);
    }
}
