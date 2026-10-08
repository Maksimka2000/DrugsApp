namespace DrugsApp.Features.DrugInteractionToolFeature.Shared;

/// <summary>
/// Default render-ready headline strings for interaction tool report sections [description].
/// </summary>
internal static class DrugInteractionToolHeadlines
{
    internal const string AlcoholSection =
        "THIS DRUG IS INTERACTED WITH ALCHOHOL SEE DETAILS";

    internal const string FoodAllOtherPublic =
        "THIS DRUG IS INTERACTED WITH SOME FOODS WHICH USER DONT TAKE";

    internal const string SupplementAllOtherPublic =
        "THIS DRUG IS INTERACTED WITH SOME SUPPLEMENTS WHICH USER DONT TAKE";

    internal const string FoodTakenNow =
        "THIS DRUG IS INTERACTED WITH SOME FOOD YOU ARE TAKING RIGHT NOW";

    internal const string FoodAllOtherRegistered =
        "THIS DRUG IS INTERACTED WITH SOME FOODS WHICH USER DONT TAKE";

    internal const string SupplementTakenNow =
        "THIS DRUG IS INTERACTED WITH SOME SUPPLEMENTS YOU ARE TAKING RIGHT NOW";

    internal const string SupplementAllOtherRegistered =
        "THIS DRUG IS INTERACTED WITH SOME SUPPLEMENTS WHICH USER DONT TAKE";

    internal const string OralGuidanceMealBlock =
        "RECOMENDATIONS ON THE INTAKE TIME OF THE DRUG IN RELATION TO MEALS";

    internal const string HealthConditionsDatabase =
        "THIS USER/YOU HAVE SOME HEALTH CONDITIONS WHICH ARE IN THE DATABASE";

    internal const string HealthConditionsCustom =
        "THIS USER/YOU HAVE ENTERED SOME HEALTH CONDITION WHICH IS NOT IN THE DATABASE";

    internal const string ProblematicDrugsDatabase =
        "THIS USER/YOU HAVE PROBLEMS WITH SOME DRUGS";

    internal const string ProblematicDrugsCustom =
        "THIS USER/YOU HAVE ENTERED A PROBLEMATIC DRUG WHICH IS NOT IN THE DATABASE";

    internal const string ProblematicDrugsTakenNowOverlap =
        "THIS USER/YOU HAVE OVERLAP IN PROBLEMATIC DRUGS TAKEN NOW";

    internal const string ProblematicDrugsPlanTakingOverlap =
        "THIS USER/YOU HAVE OVERLAP IN PROBLEMATIC DRUGS YOU PLAN TO TAKE";

    internal const string DrugAllergiesDatabase =
        "THIS USER/YOU HAVE DRUG ALLERGIES FROM THE DATABASE";

    internal const string DrugAllergiesCustom =
        "THIS USER/YOU HAVE CUSTOM DRUG ALLERGIES";

    internal const string DrugAllergiesTakenNowOverlap =
        "THIS USER/YOU HAVE OVERLAP IN DRUGS TAKEN NOW";

    internal const string DrugAllergiesPlanTakingOverlap =
        "THIS USER/YOU HAVE OVERLAP IN DRUGS YOU PLAN TO TAKE";

    internal const string DrugTakenNowOverlapSameDrugName =
        "The same drug name overlap";

    internal const string DrugTakenNowOverlapSharedActiveSubstance =
        "The same active substance name overlap";

    internal const string SupplementAllergiesDatabase =
        "THIS USER/YOU HAVE SUPPLEMENT ALLERGIES FROM THE DATABASE";

    internal const string SupplementAllergiesCustom =
        "THIS USER/YOU HAVE CUSTOM SUPPLEMENT ALLERGIES";

    internal const string SupplementAllergiesTakenNowOverlap =
        "SUPPLEMENTS YOU TAKE NOW OVERLAP WITH YOUR SUPPLEMENT ALLERGIES";

    internal const string FoodAllergiesDatabase =
        "THIS USER/YOU HAVE FOOD ALLERGIES FROM THE DATABASE";

    internal const string FoodAllergiesCustom =
        "THIS USER/YOU HAVE CUSTOM FOOD ALLERGIES";

    internal const string FoodAllergiesTakenNowOverlap =
        "FOODS YOU TAKE NOW OVERLAP WITH YOUR FOOD ALLERGIES";

    internal const string OtherAllergiesDatabase =
        "THIS USER/YOU HAVE OTHER ALLERGIES FROM THE DATABASE";

    internal const string OtherAllergiesCustom =
        "THIS USER/YOU HAVE CUSTOM OTHER ALLERGIES";

    internal const string ExactDuplication =
        "ACTIVE SUBSTANCE DUPLICATIONS BETWEEN SELECTED DRUGS";

    internal const string GroupDuplication =
        "ACTIVE SUBSTANCE GROUP DUPLICATIONS BETWEEN SELECTED DRUGS";

    internal const string DrugDuplication =
        "THE SAME DRUG IS SELECTED OR TAKEN IN MORE THAN ONE FORM";

    internal const string DrugFormDuplication =
        "SELECTED DRUG FORM IS ALREADY TAKEN REGULARLY";

    internal const string MainWarningCurrentlyPregnant =
        "The user is currently pregnant. Consult a healthcare professional before taking any medication.";

    internal const string MainWarningPlanningPregnancy =
        "The user is planning to get pregnant. Consult a healthcare professional before taking any medication.";
}
