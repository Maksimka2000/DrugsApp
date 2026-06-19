using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.ActiveSubstanceFeature.Repositories;
using DrugsApp.Domain.DrugFeature.DomainServices;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugFeature.Repositories;
using DrugsApp.Domain.PharmaceuticalFormFeature.Entities;
using DrugsApp.Domain.PharmaceuticalFormFeature.Repositories;
using DrugsApp.Domain.RouteOfAdministrationFeature.Entities;
using DrugsApp.Domain.RouteOfAdministrationFeature.Repositories;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.DrugFeature.ApplicationServices;


// Drug
//
//
/// <summary>
/// DTO special for the drug review canonicalization service for creation and manipulation with data    
/// </summary>
/// <param name="CommercialName">Commercial name for the drug</param>
/// <param name="TypeOfProduct">Type of product for the drug</param>
/// <param name="TypeOfDrug">Type of drug for the drug</param>
/// <param name="ActiveSubstanceIds">Ids of the active substances for the drug</param>
/// <param name="ProductIdentifier">Product identifier for the drug</param>
/// <param name="AtcCode">ATC code for the drug</param>
/// <param name="Dosage">Dosage for the drug</param>
/// <param name="Leaflet">Leaflet for the drug</param>
/// <param name="EducationalMaterials">Educational materials for the drug</param>
/// <param name="RouteOfAdministrationIds">Ids of the routes of administration for the drug</param>
/// <param name="PharmaceuticalFormIds">Ids of the pharmaceutical forms for the drug</param>
public sealed record CreateDrugFromReviewDrugApplicationServiceDraft(
    string CommercialName,
    string TypeOfProduct,
    DrugType TypeOfDrug,
    IReadOnlyCollection<Guid> ActiveSubstanceIds,
    string ProductIdentifier,
    string? AtcCode,
    string? Dosage,
    string? Leaflet,
    string? EducationalMaterials,
    IReadOnlyCollection<Guid>? RouteOfAdministrationIds,
    IReadOnlyCollection<Guid>? PharmaceuticalFormIds);

// Drug Form
//
//
/// <summary>
/// DTO special for the drug review canonicalization service for creation of a drug form for an existing drug from a review.
/// </summary>
/// <param name="DrugId">The id of the drug.</param>
/// <param name="CommercialName">The commercial name of the drug.</param>
/// <param name="TypeOfProduct">The type of product of the drug.</param>
/// <param name="ActiveSubstanceIds">The ids of the active substances of the drug.</param>
/// <param name="ProductIdentifier">The product identifier of the drug.</param>
/// <param name="AtcCode">The ATC code of the drug.</param>
/// <param name="Dosage">The dosage of the drug.</param>
/// <param name="Leaflet">The leaflet of the drug.</param>
/// <param name="EducationalMaterials">The educational materials of the drug.</param>
/// <param name="RouteOfAdministrationIds">The ids of the routes of administration of the drug.</param>
/// <param name="PharmaceuticalFormIds">The ids of the pharmaceutical forms of the drug.</param>
public sealed record CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft(
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    IReadOnlyCollection<Guid> ActiveSubstanceIds,
    string ProductIdentifier,
    string? AtcCode,
    string? Dosage,
    string? Leaflet,
    string? EducationalMaterials,
    IReadOnlyCollection<Guid>? RouteOfAdministrationIds,
    IReadOnlyCollection<Guid>? PharmaceuticalFormIds);

// Update Drug Form Links From Review
//
//
/// <summary>
/// DTO special for the drug review canonicalization service for updating drug form links from a review.
/// </summary>
/// <param name="DrugId">The id of the drug.</param>
/// <param name="DrugFormId">The id of the drug form.</param>
/// <param name="CommercialName">The commercial name of the drug.</param>
/// <param name="TypeOfProduct">The type of product of the drug.</param>
/// <param name="ActiveSubstanceIds">The ids of the active substances of the drug.</param>
/// <param name="ProductIdentifier">The product identifier of the drug.</param>
/// <param name="AtcCode">The ATC code of the drug.</param>
/// <param name="Dosage">The dosage of the drug.</param>
/// <param name="Leaflet">The leaflet of the drug.</param>
/// <param name="EducationalMaterials">The educational materials of the drug.</param>
/// <param name="RouteOfAdministrationIds">The ids of the routes of administration of the drug.</param>
/// <param name="PharmaceuticalFormIds">The ids of the pharmaceutical forms of the drug.</param>
public sealed record UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft(
    Guid DrugId,
    Guid DrugFormId,
    string CommercialName,
    string TypeOfProduct,
    IReadOnlyCollection<Guid> ActiveSubstanceIds,
    string ProductIdentifier,
    string? AtcCode,
    string? Dosage,
    string? Leaflet,
    string? EducationalMaterials,
    IReadOnlyCollection<Guid>? RouteOfAdministrationIds,
    IReadOnlyCollection<Guid>? PharmaceuticalFormIds);










/// <summary>
/// The canonicalization service for the drug review.
/// </summary>
/// <param name="_drugRepository">The repository for the drug.</param>
/// <param name="_activeSubstanceRepository">The repository for the active substance.</param>
/// <param name="_routeOfAdministrationRepository">The repository for the route of administration.</param>
/// <param name="_pharmaceuticalFormRepository">The repository for the pharmaceutical form.</param>
/// <param name="_drugSimilarityDomainService">The domain service for the drug similarity.</param>
/// <param name="_drugFormRepository">The repository for the drug form.</param>
internal sealed class DrugReviewCanonicalizationService(
    IDrugRepository _drugRepository,
    IActiveSubstanceRepository _activeSubstanceRepository,
    IRouteOfAdministrationRepository _routeOfAdministrationRepository,
    IPharmaceuticalFormRepository _pharmaceuticalFormRepository,
    IDrugSimilarityDomainService _drugSimilarityDomainService,
    IDrugFormRepository _drugFormRepository) : IDrugReviewCanonicalizationService
{
    
    // Create New Drug From Review
    //
    //
    /// <summary>
    /// Creates a new drug from a review.   
    /// </summary>
    /// <param name="drugApplicationServiceDraft">The draft of the drug application service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the creation of the new drug from the review.</returns>
    public async Task<ErrorOr<DrugDto>> CreateNewDrugFromReviewAsync(
        CreateDrugFromReviewDrugApplicationServiceDraft drugApplicationServiceDraft,
        CancellationToken cancellationToken)
    {
        // Parse Product Identifier and convert to int
        ErrorOr<int> productIdentifierResult = ParsePositiveInt(drugApplicationServiceDraft.ProductIdentifier, "productIdentifier");
        if (productIdentifierResult.IsError)
        {
            return productIdentifierResult.Errors;
        }
        int productIdentifier = productIdentifierResult.Value;

        // Resolve Active Substances by their ids, return error if any are not found
        ErrorOr<IReadOnlyCollection<ActiveSubstance>> activeSubstancesResult = await ResolveActiveSubstancesAsync(drugApplicationServiceDraft.ActiveSubstanceIds, cancellationToken);
        if (activeSubstancesResult.IsError)
        {
            return activeSubstancesResult.Errors;
        }
        IReadOnlyCollection<ActiveSubstance> activeSubstances = activeSubstancesResult.Value;

        // Resolve Routes by their ids, return empty collection if none are provided
        ErrorOr<IReadOnlyCollection<RouteOfAdministration>> routesResult = await ResolveRoutesAsync(drugApplicationServiceDraft.RouteOfAdministrationIds, cancellationToken);
        if (routesResult.IsError)
        {
            return routesResult.Errors;
        }
        IReadOnlyCollection<RouteOfAdministration> routes = routesResult.Value;

        // Resolve Pharmaceutical Forms by their ids, return empty collection if none are provided
        ErrorOr<IReadOnlyCollection<PharmaceuticalForm>> pharmaceuticalFormsResult = await ResolvePharmaceuticalFormsAsync(drugApplicationServiceDraft.PharmaceuticalFormIds, cancellationToken);
        if (pharmaceuticalFormsResult.IsError)
        {
            return pharmaceuticalFormsResult.Errors;
        }
        IReadOnlyCollection<PharmaceuticalForm> pharmaceuticalForms = pharmaceuticalFormsResult.Value;

        // Ensure no bypass conflicts, return error if any are found
        ErrorOr<Success> noBypassResult = await EnsureNoBypassConflictsAsync(
            snapshot: new DrugSimilaritySnapshot(
                CommercialName: drugApplicationServiceDraft.CommercialName,
                TypeOfProduct: drugApplicationServiceDraft.TypeOfProduct,
                ActiveSubstanceIds: drugApplicationServiceDraft.ActiveSubstanceIds,
                AtcCode: drugApplicationServiceDraft.AtcCode,
                Dosage: drugApplicationServiceDraft.Dosage,
                Leaflet: drugApplicationServiceDraft.Leaflet,
                EducationalMaterials: drugApplicationServiceDraft.EducationalMaterials,
                RouteOfAdministrationIds: drugApplicationServiceDraft.RouteOfAdministrationIds,
                PharmaceuticalFormIds: drugApplicationServiceDraft.PharmaceuticalFormIds),
            cancellationToken: cancellationToken);
        if (noBypassResult.IsError)
        {
            return noBypassResult.Errors;
        }

        // if no errors, create new drug
        var drug = Drug.Create(
            commercialName: drugApplicationServiceDraft.CommercialName,
            typeOfProduct: drugApplicationServiceDraft.TypeOfProduct,
            typeOfDrug: drugApplicationServiceDraft.TypeOfDrug,
            activeSubstances: activeSubstances,
            drugForms: null);

        // create new drug form
        var drugForm = DrugForm.Create(
            drugId: drug.Id,
            productIdentifier: productIdentifier,
            atcCode: drugApplicationServiceDraft.AtcCode,
            dosage: drugApplicationServiceDraft.Dosage,
            leaflet: drugApplicationServiceDraft.Leaflet,
            educationMaterials: drugApplicationServiceDraft.EducationalMaterials);

        // add routes to drug form
        foreach (RouteOfAdministration routeOfAdministration in routes)
        {
            drugForm.AddRouteOfAdministration(routeOfAdministration);
        }

        // add pharmaceutical forms to drug form
        foreach (PharmaceuticalForm pharmaceuticalForm in pharmaceuticalForms)
        {
            drugForm.AddPharmaceuticalForm(pharmaceuticalForm);
        }

        // add drug form to drug
        drug.AddDrugForm(drugForm);

        // save drug to database
        await _drugRepository.AddAsync(drug, cancellationToken);

        // Return Dto (Its prohibited to return the drug object from the application service because it violates the encapsulation principle)
        return drug.MapToDto();
    }

    // Create Drug Form 
    //
    //
    /// <summary>
    /// Creates a drug form for an existing drug from a review. 
    /// </summary>
    /// <param name="drugApplicationServiceDraft">The draft of the drug application service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the creation of the drug form for the existing drug from the review.</returns>
    public async Task<ErrorOr<DrugFormDto>> CreateDrugFormForExistingDrugFromReviewAsync(
        CreateDrugFormForExistingDrugFromReviewDrugApplicationServiceDraft drugApplicationServiceDraft,
        CancellationToken cancellationToken)
    {
        // Parse Product Identifier and convert to int  
        ErrorOr<int> productIdentifierResult = ParsePositiveInt(drugApplicationServiceDraft.ProductIdentifier, "productIdentifier");
        if (productIdentifierResult.IsError)
        {
            return productIdentifierResult.Errors;
        }
        int productIdentifier = productIdentifierResult.Value;

        // Get the drug and check if it exists
        Drug? drug = await _drugRepository.GetByIdAsync(drugApplicationServiceDraft.DrugId, cancellationToken);
        if (drug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "drug",
                    ["resourceId"] = drugApplicationServiceDraft.DrugId.ToString()
                });
        }

        // Early validation checks to ensure the merge is allowed for the provided drugId.
        if (!IsBasicMatch(drugApplicationServiceDraft.CommercialName, drugApplicationServiceDraft.TypeOfProduct, drug))
        {
            return Error.Validation(description: "Merge is not allowed for the provided drugId.");
        }
        if (!SetEquals(drugApplicationServiceDraft.ActiveSubstanceIds, drug.ActiveSubstances.Select(a => a.Id)))
        {
            return Error.Validation(description: "Merge is not allowed for the provided activeSubstanceIds.");
        }

        // Resolve Routes by their ids, return empty collection if none are provided
        ErrorOr<IReadOnlyCollection<RouteOfAdministration>> routesResult = await ResolveRoutesAsync(drugApplicationServiceDraft.RouteOfAdministrationIds, cancellationToken);
        if (routesResult.IsError)
        {
            return routesResult.Errors;
        }

        // Resolve Pharmaceutical Forms by their ids, return empty collection if none are provided
        ErrorOr<IReadOnlyCollection<PharmaceuticalForm>> pharmaceuticalFormsResult = await ResolvePharmaceuticalFormsAsync(drugApplicationServiceDraft.PharmaceuticalFormIds, cancellationToken);
        if (pharmaceuticalFormsResult.IsError)
        {
            return pharmaceuticalFormsResult.Errors;
        }

        // Ensure the merge is allowed for the provided drugId. Check duplication and update.
        ErrorOr<Success> classificationResult = EnsureMergeAllowed(
            drug,
            new DrugSimilaritySnapshot(
                CommercialName: drugApplicationServiceDraft.CommercialName,
                TypeOfProduct: drugApplicationServiceDraft.TypeOfProduct,
                ActiveSubstanceIds: drugApplicationServiceDraft.ActiveSubstanceIds,
                AtcCode: drugApplicationServiceDraft.AtcCode,
                Dosage: drugApplicationServiceDraft.Dosage,
                Leaflet: drugApplicationServiceDraft.Leaflet,
                EducationalMaterials: drugApplicationServiceDraft.EducationalMaterials,
                RouteOfAdministrationIds: drugApplicationServiceDraft.RouteOfAdministrationIds,
                PharmaceuticalFormIds: drugApplicationServiceDraft.PharmaceuticalFormIds));
        if (classificationResult.IsError)
        {
            return classificationResult.Errors;
        }

        // Create a new drug form for the existing drug
        var drugForm = DrugForm.Create(
            drugId: drug.Id,
            productIdentifier: productIdentifierResult.Value,
            atcCode: drugApplicationServiceDraft.AtcCode,
            dosage: drugApplicationServiceDraft.Dosage,
            leaflet: drugApplicationServiceDraft.Leaflet,
            educationMaterials: drugApplicationServiceDraft.EducationalMaterials);

        foreach (RouteOfAdministration routeOfAdministration in routesResult.Value)
        {
            drugForm.AddRouteOfAdministration(routeOfAdministration);
        }

        foreach (PharmaceuticalForm pharmaceuticalForm in pharmaceuticalFormsResult.Value)
        {
            drugForm.AddPharmaceuticalForm(pharmaceuticalForm);
        }

        drug.AddDrugForm(drugForm);

        // Return Dto (Its prohibited to return the drug form object from the application service because it violates the encapsulation principle)
        return drugForm.MapToDto();
    }


    // Update Drug Form Links From Review
    //
    //
    /// <summary>
    /// Updates drug form links from a review.
    /// </summary>
    /// <param name="drugApplicationServiceDraft">The draft of the drug application service.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the update of the drug form links from the review.</returns>
    public async Task<ErrorOr<DrugFormDto>> UpdateDrugFormLinksFromReviewAsync(
        UpdateDrugFormLinksFromReviewDrugApplicationServiceDraft drugApplicationServiceDraft,
        CancellationToken cancellationToken)
    {
        // Parse Product Identifier and convert to int
        ErrorOr<int> productIdentifierResult = ParsePositiveInt(drugApplicationServiceDraft.ProductIdentifier, "productIdentifier");
        if (productIdentifierResult.IsError)
        {
            return productIdentifierResult.Errors;
        }
        int productIdentifier = productIdentifierResult.Value;

        // Get the drug and check if it exists
        Drug? drug = await _drugRepository.GetByIdAsync(drugApplicationServiceDraft.DrugId, cancellationToken);
        if (drug is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "drug",
                    ["resourceId"] = drugApplicationServiceDraft.DrugId.ToString()
                });
        }

        // Get the drug form and check if it exists
        DrugForm? drugForm = await _drugFormRepository.GetByIdAsync(drugApplicationServiceDraft.DrugFormId, cancellationToken);
        if (drugForm is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found",
                metadata: new()
                {
                    ["resourceType"] = "drugForm",
                    ["resourceId"] = drugApplicationServiceDraft.DrugFormId.ToString()
                });
        }

        // Check if the drug form belongs to the drug
        if (drugForm.DrugId != drug.Id)
        {
            return Error.Validation(description: "The provided drugFormId does not belong to the provided drugId.");
        }

        // Fast validation checks to ensure the update is allowed for the provided drug snapshot.
        if (!IsBasicMatch(drugApplicationServiceDraft.CommercialName, drugApplicationServiceDraft.TypeOfProduct, drug))
        {
            return Error.Validation(description: "Update is not allowed for the provided drug snapshot.");
        }

        // Check if the active substances are the same
        if (!SetEquals(drugApplicationServiceDraft.ActiveSubstanceIds, drug.ActiveSubstances.Select(a => a.Id)))
        {
            return Error.Validation(description: "Update is not allowed for the provided activeSubstanceIds.");
        }

        //Ensure the update is allowed for the provided drug snapshot.
        ErrorOr<Success> classificationResult = EnsureUpdateAllowed(
            drug,
            drugForm,
            new DrugSimilaritySnapshot(
                CommercialName: drugApplicationServiceDraft.CommercialName,
                TypeOfProduct: drugApplicationServiceDraft.TypeOfProduct,
                ActiveSubstanceIds: drugApplicationServiceDraft.ActiveSubstanceIds,
                AtcCode: drugApplicationServiceDraft.AtcCode,
                Dosage: drugApplicationServiceDraft.Dosage,
                Leaflet: drugApplicationServiceDraft.Leaflet,
                EducationalMaterials: drugApplicationServiceDraft.EducationalMaterials,
                RouteOfAdministrationIds: drugApplicationServiceDraft.RouteOfAdministrationIds,
                PharmaceuticalFormIds: drugApplicationServiceDraft.PharmaceuticalFormIds));
        if (classificationResult.IsError)
        {
            return classificationResult.Errors;
        }

        // Update the leaflet and educational materials if they are provided    
        if (drugApplicationServiceDraft.Leaflet is not null)
        {
            drugForm.SetLeaflet(drugApplicationServiceDraft.Leaflet);
        }

        // Update the educational materials if it is provided
        if (drugApplicationServiceDraft.EducationalMaterials is not null)
        {
            drugForm.SetEducationMaterials(drugApplicationServiceDraft.EducationalMaterials);
        }

        // Update the product identifier
        drugForm.SetProductIdentifier(productIdentifier);

        return drugForm.MapToDto();
    }


    /// <summary>
    ///     This method parses the product identifier and returns the integer if it is a positive integer. Part of the validation pipeline used in methods that require a positive integer product identifier.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <param name="errorKey">The error key to use if the value is not a positive integer.</param>
    /// <returns>The integer if it is a positive integer, otherwise an error.</returns>
    private static ErrorOr<int> ParsePositiveInt(string value, string errorKey)
    {
        if (!int.TryParse(value, out int parsed) || parsed <= 0)
        {
            return Error.Validation(
                description: "ProductIdentifier must be a positive integer.",
                metadata: new()
                {
                    ["error"] = errorKey
                });
        }

        return parsed;
    }

    /// <summary>
    ///     This method resolves the active substances by their ids and returns a list of active substances.
    /// </summary>
    /// <param name="activeSubstanceIds">The ids of the active substances to resolve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of active substances.</returns>
    private async Task<ErrorOr<IReadOnlyCollection<ActiveSubstance>>> ResolveActiveSubstancesAsync(
        IReadOnlyCollection<Guid>? activeSubstanceIds,
        CancellationToken cancellationToken)
    {

        if (activeSubstanceIds is null || activeSubstanceIds.Count == 0)
        {
            return Error.Validation(
                description: "ActiveSubstanceIds must not be empty.",
                metadata: new()
                {
                    ["error"] = "activeSubstanceIds"
                });
        }

        IReadOnlyCollection<ActiveSubstance> activeSubstances = await _activeSubstanceRepository.GetByIdsForLinkingAsync(activeSubstanceIds, cancellationToken);

        List<Guid> missingActiveSubstanceIds = GetMissingIds(activeSubstanceIds, activeSubstances.Select(a => a.Id));
        if (missingActiveSubstanceIds.Count > 0)
        {
            return Error.Validation(
                description: "Some activeSubstanceIds were not found.",
                metadata: new()
                {
                    ["missingIds"] = missingActiveSubstanceIds.Select(id => id.ToString()).ToArray()
                });
        }

        return activeSubstances.ToList();
    }

    /// <summary>
    ///     This method resolves the routes of administration by their ids and returns a list of routes of administration.
    /// </summary>
    /// <param name="routeOfAdministrationIds">The ids of the routes of administration to resolve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of routes of administration.</returns>  
    private async Task<ErrorOr<IReadOnlyCollection<RouteOfAdministration>>> ResolveRoutesAsync(
        IReadOnlyCollection<Guid>? routeOfAdministrationIds,
        CancellationToken cancellationToken)
    {
        if (routeOfAdministrationIds is null)
        {
            return new List<RouteOfAdministration>().AsReadOnly();
        }

        IReadOnlyCollection<Guid> ids = routeOfAdministrationIds;
        IReadOnlyCollection<RouteOfAdministration> entities = await _routeOfAdministrationRepository.GetByIdsAsync(ids, cancellationToken);

        List<Guid> missingIds = GetMissingIds(ids, entities.Select(e => e.Id));
        if (missingIds.Count > 0)
        {
            return Error.Validation(
                description: "Some routeOfAdministrationIds were not found.",
                metadata: new()
                {
                    ["missingIds"] = missingIds.Select(id => id.ToString()).ToArray()
                });
        }

        return entities.ToList();
    }

    /// <summary>
    ///     This method resolves the pharmaceutical forms by their ids and returns a list of pharmaceutical forms.
    /// </summary>
    /// <param name="pharmaceuticalFormIds">The ids of the pharmaceutical forms to resolve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of pharmaceutical forms.</returns>
    private async Task<ErrorOr<IReadOnlyCollection<PharmaceuticalForm>>> ResolvePharmaceuticalFormsAsync(
        IReadOnlyCollection<Guid>? pharmaceuticalFormIds,
        CancellationToken cancellationToken)
    {

        if (pharmaceuticalFormIds is null)
        {
            return new List<PharmaceuticalForm>().AsReadOnly();
        }

        IReadOnlyCollection<Guid> ids = pharmaceuticalFormIds;
        IReadOnlyCollection<PharmaceuticalForm> entities = await _pharmaceuticalFormRepository.GetByIdsAsync(ids, cancellationToken);

        List<Guid> missingIds = GetMissingIds(ids, entities.Select(e => e.Id));
        if (missingIds.Count > 0)
        {
            return Error.Validation(
                description: "Some pharmaceuticalFormIds were not found.",
                metadata: new()
                {
                    ["missingIds"] = missingIds.Select(id => id.ToString()).ToArray()
                });
        }

        return entities.ToList();
    }

    /// <summary>
    ///     This method ensures that there are no bypass conflicts with the provided snapshot. Used in the create new drug from review method.
    /// </summary>
    /// <param name="snapshot">The snapshot to ensure no bypass conflicts with.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A success if there are no bypass conflicts.</returns>
    private async Task<ErrorOr<Success>> EnsureNoBypassConflictsAsync(
        DrugSimilaritySnapshot snapshot,
        CancellationToken cancellationToken)
    {
        // Search for similar candidates
        List<Drug> candidates = await _drugRepository.SearchSimilarCandidatesAsync(
            commercialName: snapshot.CommercialName,
            typeOfProduct: snapshot.TypeOfProduct,
            cancellationToken: cancellationToken);

        // Score and classify each candidate by the snapshot
        foreach (Drug candidate in candidates)
        {
            ErrorOr<DrugSimilarityResult> result = _drugSimilarityDomainService.ScoreAndClassify(snapshot, candidate);
            if (result.IsError)
            {
                continue;
            }
            DrugSimilarityResult similarity = result.Value;

            if (similarity.SimilarityClassification == DrugSimilarityClassification.Duplication)
            {
                return Error.Conflict(
                    description: "A matching drug form already exists.",
                    metadata: new()
                    {
                        ["existingResourceId"] = similarity.DrugFormId.ToString()
                    });
            }

            if (similarity.SimilarityClassification == DrugSimilarityClassification.Update)
            {
                return Error.Conflict(
                    description: "Only links differ; use the update-links endpoint.",
                    metadata: new()
                    {
                        ["existingResourceId"] = similarity.DrugFormId.ToString()
                    });
            }

            if (similarity.SimilarityClassification == DrugSimilarityClassification.Merge)
            {
                return Error.Conflict(
                    description: "A matching drug exists; merge is required.",
                    metadata: new()
                    {
                        ["existingResourceId"] = similarity.DrugId.ToString()
                    });
            }
        }

        return Result.Success;
    }


    /// <summary>
    ///     This method ensures that the merge is allowed for the provided snapshot.
    /// </summary>
    /// <param name="candidateDrug">The candidate drug to ensure the merge is allowed for.</param>
    /// <param name="snapshot">The snapshot to ensure the merge is allowed for.</param>
    /// <returns>A success if the merge is allowed.</returns>
    private ErrorOr<Success> EnsureMergeAllowed(
        Drug candidateDrug,
        DrugSimilaritySnapshot snapshot)
    {
        // Score and classify the candidate drug by the snapshot
        ErrorOr<DrugSimilarityResult> similarityResult = _drugSimilarityDomainService.ScoreAndClassify(snapshot, candidateDrug);
        if (similarityResult.IsError)
        {
            return similarityResult.Errors;
        }
        DrugSimilarityResult similarity = similarityResult.Value;

        // Check if the similarity classification is a duplication
        if (similarity.SimilarityClassification == DrugSimilarityClassification.Duplication)
        {
            return Error.Conflict(
                description: "A matching drug form already exists.",
                metadata: new()
                {
                    ["existingResourceId"] = similarity.DrugFormId.ToString()
                });
        }

        // Check if the similarity classification is an update
        if (similarity.SimilarityClassification == DrugSimilarityClassification.Update)
        {
            return Error.Conflict(
                description: "Only links differ; use the update-links endpoint.",
                metadata: new()
                {
                    ["existingResourceId"] = similarity.DrugFormId.ToString()
                });
        }

        // Check if the similarity classification is a merge
        if (similarity.SimilarityClassification != DrugSimilarityClassification.Merge)
        {
            return Error.Validation(description: "Merge is not allowed for the provided snapshot.");
        }

        return Result.Success;
    }


    private ErrorOr<Success> EnsureUpdateAllowed(
        Drug candidateDrug,
        DrugForm candidateDrugForm,
        DrugSimilaritySnapshot snapshot)
    {
        // Score and classify the candidate drug by the snapshot
        ErrorOr<DrugSimilarityResult> similarityResult = _drugSimilarityDomainService.ScoreAndClassify(snapshot, candidateDrug);
        if (similarityResult.IsError)
        {
            return similarityResult.Errors;
        }
        DrugSimilarityResult similarity = similarityResult.Value;

        // Check if the similarity classification is a duplication
        if (similarity.SimilarityClassification == DrugSimilarityClassification.Duplication)
        {
            return Error.Conflict(
                description: "A matching drug form already exists.",
                metadata: new()
                {
                    ["existingResourceId"] = similarity.DrugFormId.ToString()
                });
        }

        // Check if the similarity classification is an update
        if (similarity.SimilarityClassification != DrugSimilarityClassification.Update)
        {
            return Error.Validation(description: "Update is not allowed for the provided snapshot.");
        }

        if (similarity.DrugFormId != candidateDrugForm.Id)
        {
            return Error.Conflict(
                description: "Only links differ; use the update-links endpoint.",
                metadata: new()
                {
                    ["existingResourceId"] = similarity.DrugFormId.ToString()
                });
        }

        return Result.Success;
    }

     /// <summary>
    ///     This method gets the missing ids between the requested ids and the loaded ids.  
    /// </summary>
    /// <param name="requestedIds">The requested ids to get the missing ids from.</param>
    /// <param name="loadedIds">The loaded ids to get the missing ids from.</param>
    /// <returns>A list of missing ids.</returns>
    private static List<Guid> GetMissingIds(IEnumerable<Guid> requestedIds, IEnumerable<Guid> loadedIds)
    {
        return requestedIds.Except(loadedIds).ToList();
    }

    /// <summary>
    ///     This method checks if the basic match is allowed for the provided drug. 
    /// </summary>
    /// <param name="commercialName">The commercial name to check if the basic match is allowed for.</param>
    /// <param name="typeOfProduct">The type of product to check if the basic match is allowed for.</param>
    /// <param name="drug">The drug to check if the basic match is allowed for.</param>
    /// <returns>A boolean if the basic match is allowed for the provided drug.</returns>
    private static bool IsBasicMatch(string commercialName, string typeOfProduct, Drug drug)
        => AreEqualNormalized(commercialName, drug.CommercialName) && AreEqualNormalized(typeOfProduct, drug.TypeOfProduct);


    /// <summary>
    ///     This method checks if the set is equal to the right set.
    /// </summary>
    /// <param name="left">The left set to check if the set is equal to the right set.</param>
    /// <param name="right">The right set to check if the set is equal to the left set.</param>
    /// <returns>A boolean if the set is equal to the right set.</returns>
    private static bool SetEquals(IEnumerable<Guid>? left, IEnumerable<Guid> right)
    {
        if (left is null)
        {
            return false;
        }

        Guid[] leftArr = left.Where(id => id != Guid.Empty).Distinct().OrderBy(id => id).ToArray();
        Guid[] rightArr = right.Where(id => id != Guid.Empty).Distinct().OrderBy(id => id).ToArray();
        return leftArr.SequenceEqual(rightArr);
    }


    /// <summary>
    ///     This method checks if the strings are equal normalized.
    /// </summary>
    private static bool AreEqualNormalized(string? left, string? right)
        => string.Equals(Normalize(left), Normalize(right), StringComparison.Ordinal);

    private static string? Normalize(string? value)
    {
        if (value is null)
        {
            return null;
        }

        string trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        return trimmed.ToUpperInvariant();
    }


}

