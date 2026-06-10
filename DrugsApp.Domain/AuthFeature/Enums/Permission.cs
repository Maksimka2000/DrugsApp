using System;

namespace DrugsApp.Domain.AuthFeature.Enums;

/// <summary>
/// Defines the permissions used to protect backend endpoints.
/// </summary>
public enum Permission
{
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
    /// Allows management of auxiliary reference datalists.
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
