using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.DrugFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.DrugFeature.DomainServices;



/// <summary>
/// Review-form snapshot used to compute similarity classification.
/// </summary>
public sealed record DrugSimilaritySnapshot(
    string CommercialName,
    string TypeOfProduct,
    IReadOnlyCollection<Guid>? ActiveSubstanceIds,
    string? AtcCode,
    string? Dosage,
    string? Leaflet,
    string? EducationalMaterials,
    IReadOnlyCollection<Guid>? RouteOfAdministrationIds,
    IReadOnlyCollection<Guid>? PharmaceuticalFormIds);


/// <summary>
/// Enumerates supported drug similarity classifications.
/// </summary>
/// 
public enum DrugSimilarityClassification
{   
    /// <summary>
    /// Duplication.
    /// </summary>
    Duplication = 0,
    /// <summary>
    /// Update.
    /// </summary>
    Update = 1,
    /// <summary>
    /// Merge.
    /// </summary>
    Merge = 2,
    /// <summary>
    /// Name only.
    /// </summary>
    NameOnly = 3
}

/// <summary>
/// Similarity scoring outcome for a single drug candidate.
/// </summary>
public sealed record DrugSimilarityResult(
    Guid DrugId,
    Guid DrugFormId,
    int MatchScorePercent,
    DrugSimilarityClassification SimilarityClassification,
    bool IsMergeAllowed,
    bool IsUpdateAllowed);



/// <summary>
/// Implements similarity scoring and classification rules for approved drugs.
/// </summary>
public sealed class DrugSimilarityDomainService : IDrugSimilarityDomainService
{
    private const int BASIC_WEIGHT = 20;
    private const int ACTIVE_SUBSTANCE_WEIGHT = 25;
    private const int ATC_WEIGHT = 10;
    private const int DOSAGE_WEIGHT = 10;
    private const int LINKS_WEIGHT = 15;
    private const int ROUTE_SET_WEIGHT = 10;
    private const int PHARMACEUTICAL_FORM_SET_WEIGHT = 10;

    private sealed record SimilarityComputation(
        Guid CandidateDrugId,
        Guid CandidateFormId,
        int MatchScorePercent,
        DrugSimilarityClassification Classification,
        bool IsMergeAllowed,
        bool IsUpdateAllowed);

    /// <summary>
    ///     This method scores and classifies the similarity between a snapshot and a candidate drug.
    /// </summary>
    /// <param name="snapshot">The snapshot to score and classify.</param>
    /// <param name="candidateDrug">The candidate drug to score and classify.</param>
    /// <returns>The similarity result.</returns>
    public ErrorOr<DrugSimilarityResult> ScoreAndClassify(
        DrugSimilaritySnapshot snapshot,
        Drug candidateDrug)
    {

        // Chekc basic matches (commercial name and type of product)
        string? normalizedCommercialName = Normalize(snapshot.CommercialName);
        string? normalizedTypeOfProduct = Normalize(snapshot.TypeOfProduct);

        string? candidateCommercialName = Normalize(candidateDrug.CommercialName);
        string? candidateTypeOfProduct = Normalize(candidateDrug.TypeOfProduct);

        bool isBasicMatch = normalizedCommercialName == candidateCommercialName
            && normalizedTypeOfProduct == candidateTypeOfProduct;


        // Check active substances match (merge)
        Guid[]? snapshotActiveSubstanceIds = snapshot.ActiveSubstanceIds?.Where(id => id != Guid.Empty).ToArray();
        Guid[] candidateActiveSubstanceIds = candidateDrug.ActiveSubstances.Select(s => s.Id).ToArray();

        bool isActiveSubstancesMatch = SetEquals(snapshotActiveSubstanceIds, candidateActiveSubstanceIds);

        // for each form, compute the similarity (update)
        DrugForm bestForm = candidateDrug.DrugForms
            .OrderBy(form => form.Id)
            .First();

        SimilarityComputation bestComputation = ComputeFormSimilarity(
            snapshot: snapshot,
            candidateDrugId: candidateDrug.Id,
            candidateForm: bestForm,
            isBasicMatch: isBasicMatch,
            isActiveSubstancesMatch: isActiveSubstancesMatch);

        foreach (DrugForm form in candidateDrug.DrugForms)
        {
            SimilarityComputation computation = ComputeFormSimilarity(
                snapshot: snapshot,
                candidateDrugId: candidateDrug.Id,
                candidateForm: form,
                isBasicMatch: isBasicMatch,
                isActiveSubstancesMatch: isActiveSubstancesMatch);

            if (computation.MatchScorePercent > bestComputation.MatchScorePercent)
            {
                bestComputation = computation;
                bestForm = form;
                continue;
            }

            if (computation.MatchScorePercent == bestComputation.MatchScorePercent
                && form.Id.CompareTo(bestForm.Id) < 0)
            {
                bestComputation = computation;
                bestForm = form;
            }
        }

        return new DrugSimilarityResult(
            DrugId: candidateDrug.Id,
            DrugFormId: bestForm.Id,
            MatchScorePercent: bestComputation.MatchScorePercent,
            SimilarityClassification: bestComputation.Classification,
            IsMergeAllowed: bestComputation.IsMergeAllowed,
            IsUpdateAllowed: bestComputation.IsUpdateAllowed);
    }

    private static SimilarityComputation ComputeFormSimilarity(
        DrugSimilaritySnapshot snapshot,
        Guid candidateDrugId,
        DrugForm candidateForm,
        bool isBasicMatch,
        bool isActiveSubstancesMatch)
    {
        bool isAtcMatch = AreEqual(Normalize(snapshot.AtcCode), Normalize(candidateForm.AtcCode));
        bool isDosageMatch = AreEqual(Normalize(snapshot.Dosage), Normalize(candidateForm.Dosage));
        bool isLeafletMatch = AreEqual(Normalize(snapshot.Leaflet), Normalize(candidateForm.Leaflet));
        bool isEducationMatch = AreEqual(Normalize(snapshot.EducationalMaterials), Normalize(candidateForm.EducationMaterials));
        bool isLinksMatch = isLeafletMatch && isEducationMatch;

        Guid[]? snapshotRouteIds = snapshot.RouteOfAdministrationIds?.Where(id => id != Guid.Empty).ToArray();
        Guid[] candidateRouteIds = candidateForm.RoutesOfAdministration.Select(r => r.Id).ToArray();
        bool isRouteSetMatch = SetEquals(snapshotRouteIds, candidateRouteIds);

        Guid[]? snapshotPharmaceuticalFormIds = snapshot.PharmaceuticalFormIds?.Where(id => id != Guid.Empty).ToArray();
        Guid[] candidatePharmaceuticalFormIds = candidateForm.PharmaceuticalForms.Select(f => f.Id).ToArray();
        bool isPharmaceuticalFormSetMatch = SetEquals(snapshotPharmaceuticalFormIds, candidatePharmaceuticalFormIds);

        int points = 0;
        if (isBasicMatch)
        {
            points += BASIC_WEIGHT;
        }

        if (isActiveSubstancesMatch)
        {
            points += ACTIVE_SUBSTANCE_WEIGHT;
        }

        if (isAtcMatch)
        {
            points += ATC_WEIGHT;
        }

        if (isDosageMatch)
        {
            points += DOSAGE_WEIGHT;
        }

        if (isLinksMatch)
        {
            points += LINKS_WEIGHT;
        }

        if (isRouteSetMatch)
        {
            points += ROUTE_SET_WEIGHT;
        }

        if (isPharmaceuticalFormSetMatch)
        {
            points += PHARMACEUTICAL_FORM_SET_WEIGHT;
        }

        DrugSimilarityClassification classification = DrugSimilarityClassification.NameOnly;
        bool isMergeAllowed = false;
        bool isUpdateAllowed = false;

        if (isBasicMatch && isActiveSubstancesMatch)
        {
            bool isDuplication = isAtcMatch
                && isDosageMatch
                && isLinksMatch
                && isRouteSetMatch
                && isPharmaceuticalFormSetMatch;

            if (isDuplication)
            {
                return new SimilarityComputation(
                    CandidateDrugId: candidateDrugId,
                    CandidateFormId: candidateForm.Id,
                    MatchScorePercent: 100,
                    Classification: DrugSimilarityClassification.Duplication,
                    IsMergeAllowed: false,
                    IsUpdateAllowed: false);
            }

            bool isUpdate = isAtcMatch
                && isDosageMatch
                && isRouteSetMatch
                && isPharmaceuticalFormSetMatch
                && !isLinksMatch;

            if (isUpdate)
            {
                return new SimilarityComputation(
                    CandidateDrugId: candidateDrugId,
                    CandidateFormId: candidateForm.Id,
                    MatchScorePercent: Clamp(points, 0, 99),
                    Classification: DrugSimilarityClassification.Update,
                    IsMergeAllowed: false,
                    IsUpdateAllowed: true);
            }

            bool isMerge = !isAtcMatch || !isDosageMatch || !isRouteSetMatch || !isPharmaceuticalFormSetMatch;
            if (isMerge)
            {
                return new SimilarityComputation(
                    CandidateDrugId: candidateDrugId,
                    CandidateFormId: candidateForm.Id,
                    MatchScorePercent: Clamp(points, 20, 95),
                    Classification: DrugSimilarityClassification.Merge,
                    IsMergeAllowed: true,
                    IsUpdateAllowed: false);
            }
        }

        return new SimilarityComputation(
            CandidateDrugId: candidateDrugId,
            CandidateFormId: candidateForm.Id,
            MatchScorePercent: Clamp(points, 0, 99),
            Classification: classification,
            IsMergeAllowed: isMergeAllowed,
            IsUpdateAllowed: isUpdateAllowed);
    }

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

    private static bool AreEqual(string? left, string? right)
        => string.Equals(left, right, StringComparison.Ordinal);

    private static bool SetEquals(Guid[]? left, Guid[] right)
    {
        if (left is null)
        {
            return false;
        }

        if (right.Length != left.Length)
        {
            return false;
        }

        return left.OrderBy(x => x).SequenceEqual(right.OrderBy(x => x));
    }

    private static int Clamp(int value, int min, int max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

  
}

