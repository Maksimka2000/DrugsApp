using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.Features.StudyModeFeature.CreateStudyClinic;
using DrugsApp.Features.StudyModeFeature.CreateStudyDoctor;
using DrugsApp.Features.StudyModeFeature.CreateStudyPatient;
using DrugsApp.Features.StudyModeFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.StudyModeFeature;

/// <summary>
/// Enumerates every controller action and asserts that the read-only study roles only reach the allow-listed read
/// endpoints; any new member endpoint stays closed to study roles unless it is deliberately allow-listed here.
/// </summary>
public sealed class StudyRoleAuthorizationTests : IntegrationTestBase
{
    private static readonly HashSet<string> MemberReadNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.UserProfileFeature.GetUserProfile",
        "DrugsApp.Features.UserProfileFeature.DownloadUserProfilePhoto",
        "DrugsApp.Features.UserProfileFeature.GetReproductiveHealthProfile",
        "DrugsApp.Features.AllergiesFeature.GetAllergySummary",
        "DrugsApp.Features.AllergiesFeature.ListDrugAllergies",
        "DrugsApp.Features.AllergiesFeature.ListSupplementAllergies",
        "DrugsApp.Features.AllergiesFeature.ListFoodAllergies",
        "DrugsApp.Features.AllergiesFeature.ListOtherAllergies",
        "DrugsApp.Features.HealthConditionFeature.SearchUserHealthConditions",
        "DrugsApp.Features.DrugProblemsFeature.SearchDrugProblems",
        "DrugsApp.Features.RegularDrugsFeature.ListRegularDrugs",
        "DrugsApp.Features.RegularDrugsFeature.GetRegularDrug",
        "DrugsApp.Features.RegularDrugsFeature.ListRegularDrugHistory",
        "DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplements",
        "DrugsApp.Features.RegularSupplementsFeature.GetRegularSupplement",
        "DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplementHistory",
        "DrugsApp.Features.RegularSupplementsFeature.ListOwnSupplements",
        "DrugsApp.Features.RegularSupplementsFeature.GetOwnSupplement",
        "DrugsApp.Features.RegularFoodFeature.ListRegularFood",
        "DrugsApp.Features.RegularFoodFeature.GetRegularFood",
        "DrugsApp.Features.RegularFoodFeature.ListRegularFoodHistory",
        "DrugsApp.Features.MealTimeManagementFeature.ListMealTimes",
        "DrugsApp.Features.MealTimeManagementFeature.RetrieveMealTime",
        "DrugsApp.Features.DrugInteractionToolFeature.ExecuteRegisteredDrugInteractionToolReport",
        "DrugsApp.Features.DrugToolFeature.GetDrugToolDetail",
        "DrugsApp.Features.DrugFeature.SearchDrugSimilarities",
        "DrugsApp.Features.VetDrugFeature.SearchVetDrugs",
        "DrugsApp.Features.VetDrugFeature.RetrieveVetDrug",
        "DrugsApp.Features.ActiveSubstanceFeature.SearchActiveSubstances",
        "DrugsApp.Features.ActiveSubstanceGroupFeature.SearchActiveSubstanceGroups",
        "DrugsApp.Features.FoodFeature.SearchFoods",
        "DrugsApp.Features.FoodFeature.SearchFoodGroups",
        "DrugsApp.Features.SupplementFeature.SearchSupplements",
        "DrugsApp.Features.SupplementFeature.SearchSupplementGroups",
        "DrugsApp.Features.PharmaceuticalFormFeature.SearchPharmaceuticalForms",
        "DrugsApp.Features.RouteOfAdministrationFeature.SearchRouteOfAdministrations",
        "DrugsApp.Features.HealthConditionFeature.SearchHealthConditions",
        "DrugsApp.Features.DrugProblemsFeature.SearchDrugProblemTypes",
    };

    private static readonly HashSet<string> ProviderReadNamespaces = new(StringComparer.Ordinal)
    {
        "DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderPatients",
        "DrugsApp.Features.HealthcareSpecialistSupervisionFeature.GetProviderPatientDetail",
        "DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionPrecheck",
        "DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionReport",
    };

    private static readonly Regex RouteParameterPattern = new(@"\{\*{0,2}([^}:?=]+)(:[^}=?]*)?[^}]*\}", RegexOptions.Compiled);

    [Fact]
    public async Task StudyRoles_ReachOnlyAllowListedEndpoints()
    {
        (string doctorAccessToken, string patientAccessToken) = await ProvisionStudyAccountsAsync();

        List<string> violations = [];
        foreach (ProbedEndpoint endpoint in EnumerateEndpoints())
        {
            HttpStatusCode anonymousStatus = await SendAsync(endpoint, accessToken: null);
            bool isPublicEndpoint = anonymousStatus != HttpStatusCode.Unauthorized;
            if (isPublicEndpoint)
            {
                continue;
            }

            bool isMemberRead = MemberReadNamespaces.Contains(endpoint.ControllerNamespace);
            bool isProviderRead = ProviderReadNamespaces.Contains(endpoint.ControllerNamespace);

            await AssertAccessAsync(endpoint, doctorAccessToken, "StudyDoctor", isMemberRead || isProviderRead, violations);
            await AssertAccessAsync(endpoint, patientAccessToken, "StudyPatient", isMemberRead, violations);
        }

        violations.Should().BeEmpty("study roles must only reach the allow-listed read endpoints");
    }

    private async Task AssertAccessAsync(
        ProbedEndpoint endpoint,
        string accessToken,
        string roleName,
        bool isAllowed,
        List<string> violations)
    {
        HttpStatusCode status = await SendAsync(endpoint, accessToken);
        bool isForbidden = status == HttpStatusCode.Forbidden;

        if (isAllowed == isForbidden)
        {
            violations.Add($"{roleName} {endpoint.Method} {endpoint.Url} -> {(int)status} (expected {(isAllowed ? "not 403" : "403")})");
        }
    }

    private async Task<(string DoctorAccessToken, string PatientAccessToken)> ProvisionStudyAccountsAsync()
    {
        await AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None);

        HttpResponseMessage clinicResponse = await Client.PostAsJsonAsync(
            "/api/admin/study/clinics",
            new CreateStudyClinicRequest($"Authorization ward {Guid.NewGuid():N}", null),
            IntegrationTestJson.Options);
        clinicResponse.EnsureSuccessStatusCode();
        StudyClinicDto clinic = (await clinicResponse.Content.ReadFromJsonAsync<StudyClinicDto>(IntegrationTestJson.Options))!;

        HttpResponseMessage doctorResponse = await Client.PostAsJsonAsync(
            $"/api/admin/study/clinics/{clinic.Id}/doctors",
            new CreateStudyDoctorRequest(
                "Anna",
                "Nowak",
                new DateOnly(1980, 1, 1),
                "female",
                "doctor",
                null,
                null,
                null,
                0,
                0),
            IntegrationTestJson.Options);
        doctorResponse.EnsureSuccessStatusCode();
        CreatedStudyAccountResponse doctor =
            (await doctorResponse.Content.ReadFromJsonAsync<CreatedStudyAccountResponse>(IntegrationTestJson.Options))!;

        HttpResponseMessage patientResponse = await Client.PostAsJsonAsync(
            $"/api/admin/study/clinics/{clinic.Id}/patients",
            new CreateStudyPatientRequest("Jan", "Kowalski", new DateOnly(1990, 5, 5), "male", 100, 100),
            IntegrationTestJson.Options);
        patientResponse.EnsureSuccessStatusCode();
        CreatedStudyAccountResponse patient =
            (await patientResponse.Content.ReadFromJsonAsync<CreatedStudyAccountResponse>(IntegrationTestJson.Options))!;

        TestAuthHelper.ClearBearerToken(Client);

        LoginMemberResponse doctorLogin = await TestAuthHelper.LoginMemberAsync(
            Client,
            doctor.Credentials.LoginEmail,
            doctor.Credentials.Password,
            CancellationToken.None);
        LoginMemberResponse patientLogin = await TestAuthHelper.LoginMemberAsync(
            Client,
            patient.Credentials.LoginEmail,
            patient.Credentials.Password,
            CancellationToken.None);

        return (doctorLogin.AccessToken, patientLogin.AccessToken);
    }

    private IEnumerable<ProbedEndpoint> EnumerateEndpoints()
    {
        IActionDescriptorCollectionProvider provider =
            Factory.Services.GetRequiredService<IActionDescriptorCollectionProvider>();

        foreach (ControllerActionDescriptor action in provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>())
        {
            string? template = action.AttributeRouteInfo?.Template;
            if (template is null)
            {
                continue;
            }

            string url = "/" + RouteParameterPattern.Replace(template, match =>
                match.Groups[2].Value.Contains("guid", StringComparison.OrdinalIgnoreCase)
                    ? Guid.NewGuid().ToString()
                    : "1");

            IEnumerable<string> methods = action.EndpointMetadata
                .OfType<HttpMethodMetadata>()
                .SelectMany(metadata => metadata.HttpMethods)
                .DefaultIfEmpty("GET")
                .Distinct();

            foreach (string method in methods)
            {
                yield return new ProbedEndpoint(action.ControllerTypeInfo.Namespace ?? string.Empty, method, url);
            }
        }
    }

    private async Task<HttpStatusCode> SendAsync(ProbedEndpoint endpoint, string? accessToken)
    {
        using var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), endpoint.Url);
        if (accessToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        bool sendsBody = endpoint.Method is "POST" or "PUT" or "PATCH";
        if (sendsBody)
        {
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        }

        using HttpResponseMessage response = await Client.SendAsync(request);
        return response.StatusCode;
    }

    private sealed record ProbedEndpoint(string ControllerNamespace, string Method, string Url);
}
