using System.Linq;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace DrugsApp.Host.Auth.Authorization;


// catatlog include: 
// - drug feature
// - active substance feature
// - food features 
// - supplment features

// interaction include:
// - interaction feature
// - alcohol feature 
// 
// reference data include:
// - pharmaceutical form feature
// - route of administration feature

// import include:
// - temp drug feature usecases and review usecases

// review include:
// - temp drug import batch usecases

// other include:
// - problem feature 
// - health condition feature

// website content  include:
// - faq feature

// .ManageClientUsers:
// - client user usecase
        
// ManageAdminUsers
// - admin user usecase

// ManageRoles
// - role on admin users usecase

public sealed class ApiAuthorizationConvention : IApplicationModelConvention
{
    private static readonly HashSet<string> ClientCatalogReadNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.DrugFeature.SearchDrugs",
        "DrugsApp.Features.DrugFeature.RetrieveDrug",
    };

    private static readonly HashSet<string> CatalogReadNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.VetDrugFeature.SearchVetDrugs",
        "DrugsApp.Features.VetDrugFeature.RetrieveVetDrug",
        "DrugsApp.Features.ActiveSubstanceFeature.SearchActiveSubstances",
        "DrugsApp.Features.ActiveSubstanceGroupFeature.SearchActiveSubstanceGroups",
        "DrugsApp.Features.FoodFeature.SearchFoods",
        "DrugsApp.Features.FoodFeature.SearchFoodGroups",
        "DrugsApp.Features.SupplementFeature.SearchSupplements",
        "DrugsApp.Features.SupplementFeature.SearchSupplementGroups"
    };

    private static readonly HashSet<string> CatalogWriteNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.DrugFeature.UpdateDrug",
        "DrugsApp.Features.DrugFeature.UpdateDrugForm",
        "DrugsApp.Features.VetDrugFeature.UpdateVetDrug",
        "DrugsApp.Features.ActiveSubstanceFeature.AddActiveSubstance",
        "DrugsApp.Features.ActiveSubstanceFeature.UpdateActiveSubstance",
        "DrugsApp.Features.ActiveSubstanceFeature.AddOralMedicationIntakeGuidelines",
        "DrugsApp.Features.ActiveSubstanceFeature.CheckActiveSubstanceNameAvailability",
        "DrugsApp.Features.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup",
        "DrugsApp.Features.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup",
        "DrugsApp.Features.ActiveSubstanceGroupFeature.DeleteActiveSubstanceGroup",
        "DrugsApp.Features.ActiveSubstanceGroupFeature.CheckActiveSubstanceGroupNameAvailability",
        "DrugsApp.Features.FoodFeature.AddFood",
        "DrugsApp.Features.FoodFeature.UpdateFood",
        "DrugsApp.Features.FoodFeature.RemoveFood",
        "DrugsApp.Features.FoodFeature.CheckFoodNameAvailability",
        "DrugsApp.Features.FoodFeature.AddFoodGroup",
        "DrugsApp.Features.FoodFeature.UpdateFoodGroup",
        "DrugsApp.Features.FoodFeature.RemoveFoodGroup",
        "DrugsApp.Features.FoodFeature.CheckFoodGroupNameAvailability",
        "DrugsApp.Features.SupplementFeature.AddSupplement",
        "DrugsApp.Features.SupplementFeature.UpdateSupplement",
        "DrugsApp.Features.SupplementFeature.DeleteSupplement",
        "DrugsApp.Features.SupplementFeature.CheckSupplementNameAvailability",
        "DrugsApp.Features.SupplementFeature.AddSupplementGroup",
        "DrugsApp.Features.SupplementFeature.UpdateSupplementGroup",
        "DrugsApp.Features.SupplementFeature.DeleteSupplementGroup",
        "DrugsApp.Features.SupplementFeature.CheckSupplementGroupNameAvailability"
    };

    private static readonly HashSet<string> ReferenceDataReadNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.PharmaceuticalFormFeature.SearchPharmaceuticalForms",
        "DrugsApp.Features.RouteOfAdministrationFeature.SearchRouteOfAdministrations"
    };

    private static readonly HashSet<string> ReferenceDataWriteNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.PharmaceuticalFormFeature.AddPharmaceuticalForm",
        "DrugsApp.Features.PharmaceuticalFormFeature.UpdatePharmaceuticalForm",
        "DrugsApp.Features.PharmaceuticalFormFeature.CheckPharmaceuticalFormNameAvailability",
        "DrugsApp.Features.RouteOfAdministrationFeature.AddRouteOfAdministration",
        "DrugsApp.Features.RouteOfAdministrationFeature.UpdateRouteOfAdministration",
        "DrugsApp.Features.RouteOfAdministrationFeature.CheckRouteOfAdministrationNameAvailability"
    };

    private static readonly HashSet<string> ImportNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.TempDrugFeature.UploadImportViaUrl",
        "DrugsApp.Features.TempDrugFeature.UploadImportViaFile",
        "DrugsApp.Features.TempDrugFeature.StartImport",
        "DrugsApp.Features.TempDrugFeature.SearchImportBatches",
        "DrugsApp.Features.TempDrugFeature.GetImportBatch",
        "DrugsApp.Features.TempDrugFeature.GetImportBatchDetails",
        "DrugsApp.Features.TempDrugFeature.RevertImport",
        "DrugsApp.Features.TempDrugFeature.CancelImport",
        "DrugsApp.Features.TempDrugFeature.DownloadImportFile"
    };

    private static readonly HashSet<string> ReviewNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.TempDrugFeature.SearchTempDrugs",
        "DrugsApp.Features.TempDrugFeature.UpdateTempDrugStatus",
        "DrugsApp.Features.TempDrugFeature.CreateDrugFromTempDrugReview",
        "DrugsApp.Features.TempDrugFeature.CreateVetDrugFromTempDrugReview",
        "DrugsApp.Features.TempDrugFeature.CreateDrugFormFromTempDrugReview",
        "DrugsApp.Features.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview"
    };

    private static readonly HashSet<string> DrugProblemTypeManageNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.DrugProblemsFeature.CreateDrugProblemType",
        "DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblemType",
        "DrugsApp.Features.DrugProblemsFeature.DeleteDrugProblemType",
        "DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemTypeNameAvailability"
    };

    private static readonly HashSet<string> AdminSelfServiceNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.AdminUserManagementFeature.ChangeAdminUserPassword",
        "DrugsApp.Features.AdminUserManagementFeature.ChangeAdminUserLoginEmail"
    };

    public void Apply(ApplicationModel application)
    {
        foreach (ControllerModel controller in application.Controllers)
        {
            string? controllerNamespace = controller.ControllerType.Namespace;
            if (controllerNamespace is null)
            {
                continue;
            }

            TryApplyAuthorization(controllerNamespace, controller);
        }
    }

    private static void TryApplyAuthorization(string controllerNamespace, ControllerModel controller)
    {
        if (controllerNamespace.StartsWith("DrugsApp.Features.AuthFeature.", StringComparison.Ordinal))
        {
            AllowAnonymous(controller);
            return;
        }

        if (controllerNamespace == "DrugsApp.Features.SupportFeature.SearchPublicFaqEntries")
        {
            AllowAnonymous(controller);
            return;
        }

        if (controllerNamespace.StartsWith("DrugsApp.Features.SupportFeature.", StringComparison.Ordinal))
        {
            RequirePermission(controller, Permission.ManageWebsiteContent);
            return;
        }

        if (controllerNamespace.StartsWith("DrugsApp.Features.SystemTextFeature.", StringComparison.Ordinal))
        {
            RequirePermission(controller, Permission.ManageInteractions);
            return;
        }

        if (controllerNamespace.StartsWith("DrugsApp.Features.InteractionFeature.", StringComparison.Ordinal))
        {
            RequirePermission(controller, Permission.ManageInteractions);
            return;
        }

        if (ReferenceDataReadNamespaces.Contains(controllerNamespace))
        {
            RequireAnyPermission(
                controller,
                Permission.ManageReferenceData,
                Permission.ManageReviews);
            return;
        }

        if (ReferenceDataWriteNamespaces.Contains(controllerNamespace))
        {
            RequirePermission(controller, Permission.ManageReferenceData);
            return;
        }

        if (ImportNamespaces.Contains(controllerNamespace))
        {
            RequirePermission(controller, Permission.ManageImports);
            return;
        }

        if (ReviewNamespaces.Contains(controllerNamespace))
        {
            RequirePermission(controller, Permission.ManageReviews);
            return;
        }

        if (DrugProblemTypeManageNamespaces.Contains(controllerNamespace))
        {
            RequirePermission(controller, Permission.ManageOther);
            return;
        }

        if (controllerNamespace == "DrugsApp.Features.DrugFeature.SearchDrugSimilarities")
        {
            RequireAnyPermission(
                controller,
                Permission.ManageCatalog,
                Permission.ManageReviews);
            return;
        }

        if (ClientCatalogReadNamespaces.Contains(controllerNamespace))
        {
            RequireAnyPermissionOrClientRole(
                controller,
                Permission.ManageCatalog,
                Permission.ManageReviews,
                Permission.ManageInteractions);
            return;
        }

        if (CatalogReadNamespaces.Contains(controllerNamespace))
        {
            RequireAnyPermission(
                controller,
                Permission.ManageCatalog,
                Permission.ManageReviews,
                Permission.ManageInteractions);
            return;
        }

        if (CatalogWriteNamespaces.Contains(controllerNamespace))
        {
            RequirePermission(controller, Permission.ManageCatalog);
            return;
        }

        if (AdminSelfServiceNamespaces.Contains(controllerNamespace))
        {
            RequireAnyAdminRole(controller);
            return;
        }

        if (controllerNamespace.StartsWith("DrugsApp.Features.AdminUserManagementFeature.", StringComparison.Ordinal))
        {
            RequirePermission(controller, Permission.ManageAdminUsers);
            return;
        }

        if (controllerNamespace.StartsWith("DrugsApp.Features.ClientUserManagementFeature.", StringComparison.Ordinal))
        {
            RequirePermission(controller, Permission.ManageClientUsers);
        }
    }

    private static void AllowAnonymous(ControllerModel controller)
    {
        controller.Filters.Add(new AllowAnonymousFilter());
    }

    private static void RequirePermission(ControllerModel controller, Permission permission)
    {
        controller.Filters.Add(new AuthorizeFilter([new HasPermissionAttribute(permission)]));
    }

    private static void RequireAnyPermission(ControllerModel controller, params Permission[] permissions)
    {
        controller.Filters.Add(new AuthorizeFilter([new HasAnyPermissionAttribute(permissions)]));
    }

    private static void RequireAnyPermissionOrClientRole(ControllerModel controller, params Permission[] permissions)
    {
        string[] orderedPermissionNames = permissions
            .Select(permission => permission.ToString())
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        string policy = "AnyPermissionOrClient:" + AuthRoles.Client + "|" + string.Join('|', orderedPermissionNames);
        controller.Filters.Add(new AuthorizeFilter(policy));
    }

    private static void RequireAnyAdminRole(ControllerModel controller)
    {
        AuthorizationPolicy policy = new AuthorizationPolicyBuilder()
            .RequireRole(AuthRoles.AdminRoleNames.ToArray())
            .Build();

        controller.Filters.Add(new AuthorizeFilter(policy));
    }
}

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(Permission permission)
    {
        Policy = permission.ToString();
    }
}

public sealed class HasAnyPermissionAttribute : AuthorizeAttribute
{
    public HasAnyPermissionAttribute(params Permission[] permissions)
    {
        string[] orderedPermissionNames = permissions
            .Select(permission => permission.ToString())
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Policy = "AnyPermission:" + string.Join('|', orderedPermissionNames);
    }
}
