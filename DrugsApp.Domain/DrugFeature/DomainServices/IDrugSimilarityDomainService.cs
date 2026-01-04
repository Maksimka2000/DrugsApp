using System;
using System.Collections.Generic;
using DrugsApp.Domain.DrugFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.DrugFeature.DomainServices;

/// <summary>
/// Provides deterministic scoring and classification for drug similarity checks.
/// </summary>
public interface IDrugSimilarityDomainService
{
    /// <summary>
    /// Scores and classifies a candidate drug against a review-form snapshot.
    /// </summary>
    /// <param name="snapshot">Normalized review snapshot used for comparison.</param>
    /// <param name="candidateDrug">Candidate drug with drug forms and lookup navigation loaded.</param>
    /// <returns>Similarity result containing the best matching drug form id and classification.</returns>
    ErrorOr<DrugSimilarityResult> ScoreAndClassify(
        DrugSimilaritySnapshot snapshot,
        Drug candidateDrug);
}



