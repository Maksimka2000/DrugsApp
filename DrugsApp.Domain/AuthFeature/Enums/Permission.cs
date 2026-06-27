using System;

namespace DrugsApp.Domain.AuthFeature.Enums;

/// <summary>
/// Defines the permissions used to protect backend endpoints.
/// </summary>
public enum Permission
{
    /// <summary>
    /// Allows searching catalog entities such as drugs, foods, and supplements.
    /// </summary>
    SearchCatalog = 100,

    /// <summary>
    /// Allows searching reference data such as pharmaceutical forms and routes of administration.
    /// </summary>
    SearchReferenceData = 110,

    /// <summary>
    /// Allows searching health reference catalogs such as health conditions and drug problem types.
    /// </summary>
    SearchHealthReference = 120,

    /// <summary>
    /// Allows healthcare providers to manage patient supervision workflows.
    /// </summary>
    ManagePatientSupervision = 130,

    /// <summary>
    /// Allows administration of drugs, active substances, foods, and supplements.
    /// </summary>
    ManageCatalog = 200,

    /// <summary>
    /// Allows administration of substance, food, supplement, and alcohol interactions.
    /// </summary>
    ManageInteractions = 210,

    /// <summary>
    /// Allows administration of pharmaceutical forms and routes of administration.
    /// </summary>
    ManageReferenceData = 220,

    /// <summary>
    /// Allows import pipeline actions for drug data.
    /// </summary>
    ManageImports = 230,

    /// <summary>
    /// Allows review actions over imported temporary drug data.
    /// </summary>
    ManageReviews = 231,

    /// <summary>
    /// Allows management of auxiliary reference data lists.
    /// </summary>
    ManageOther = 240,

    /// <summary>
    /// Allows management of public website content such as FAQs.
    /// </summary>
    ManageWebsiteContent = 250,

    /// <summary>
    /// Allows administration of client users.
    /// </summary>
    ManageClientUsers = 300,

    /// <summary>
    /// Allows administration of admin users.
    /// </summary>
    ManageAdminUsers = 301,

    /// <summary>
    /// Allows administration of roles and their permission assignments.
    /// </summary>
    ManageRoles = 302,
}
