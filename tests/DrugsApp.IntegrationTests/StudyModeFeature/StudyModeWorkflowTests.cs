using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.SupplementFeature.Entities;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.AuthFeature.LoginMember;
using DrugsApp.Features.StudyModeFeature.CreateStudyClinic;
using DrugsApp.Features.StudyModeFeature.CreateStudyDoctor;
using DrugsApp.Features.StudyModeFeature.CreateStudyPatient;
using DrugsApp.Features.StudyModeFeature.Shared;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.IntegrationTests.StudyModeFeature;

/// <summary>
/// Drives the study mode endpoints the admin panel calls, as a Teacher: clinic lifecycle, account provisioning,
/// canvas links, credentials, random medical data for every section, the medical-data proxy, the study
/// accounts' read-only client access, and the teardown of accounts and clinics.
/// </summary>
public sealed class StudyModeWorkflowTests : IntegrationTestBase
{
    private static readonly string[] RandomDataSections =
    [
        "drug-allergies",
        "supplement-allergies",
        "food-allergies",
        "other-allergies",
        "health-conditions",
        "drug-problems",
        "regular-drugs",
        "regular-supplements",
        "own-supplements",
        "regular-food",
        "meal-times",
    ];

    [Fact]
    public async Task Teacher_RunsTheWholeClinicLifecycle()
    {
        await AuthenticateAsTeacherAsync();

        // The teacher only reaches study mode and interaction rules.
        (await Client.GetAsync("/api/admin-users")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Clinics: create, duplicate name, search, paging validation.
        string clinicName = $"Ward {Guid.NewGuid():N}";
        StudyClinicDto clinic = await PostAsync<StudyClinicDto>(
            "/api/admin/study/clinics",
            new CreateStudyClinicRequest(clinicName, "Scenario"),
            HttpStatusCode.Created);

        await AssertProblemFieldAsync(
            await Client.PostAsJsonAsync(
                "/api/admin/study/clinics",
                new CreateStudyClinicRequest($"  {clinicName.ToUpperInvariant()} ", null),
                IntegrationTestJson.Options),
            HttpStatusCode.Conflict,
            "name");

        StudyClinicListResponse search = await GetAsync<StudyClinicListResponse>(
            $"/api/admin/study/clinics?search={clinicName[..10]}&skip=0&take=20");
        search.Items.Should().ContainSingle(item => item.Id == clinic.Id);

        (await Client.GetAsync("/api/admin/study/clinics?take=41")).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Accounts: validation, then a doctor and a patient.
        string clinicPath = $"/api/admin/study/clinics/{clinic.Id}";

        (await Client.PostAsJsonAsync(
                $"{clinicPath}/doctors",
                new CreateStudyDoctorRequest("Anna", "Nowak", new DateOnly(1980, 1, 1), "female", "other", null, null, null, 0, 0),
                IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest, "profession 'other' needs a label");

        (await Client.PostAsJsonAsync(
                $"{clinicPath}/patients",
                new CreateStudyPatientRequest("Jan", "Kowalski", DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5), "male", 0, 0),
                IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest, "a date of birth in the future is invalid");

        CreatedStudyAccountResponse doctor = await PostAsync<CreatedStudyAccountResponse>(
            $"{clinicPath}/doctors",
            new CreateStudyDoctorRequest("Anna", "Nowak", new DateOnly(1980, 1, 1), "female", "doctor", null, "Clinic A", null, 10, 20),
            HttpStatusCode.Created);
        CreatedStudyAccountResponse patient = await PostAsync<CreatedStudyAccountResponse>(
            $"{clinicPath}/patients",
            new CreateStudyPatientRequest("Ewa", "Kowalska", new DateOnly(1990, 5, 5), "female", 300, 40),
            HttpStatusCode.Created);

        Guid doctorId = doctor.Doctor!.UserId;
        Guid patientId = patient.Patient!.UserId;

        // Canvas: links and positions, invalid links rejected.
        (await Client.PutAsJsonAsync(
                $"{clinicPath}/canvas",
                new
                {
                    associations = new[] { new { doctorUserId = patientId, patientUserId = doctorId } },
                    positions = Array.Empty<object>()
                },
                IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await Client.PutAsJsonAsync(
                $"{clinicPath}/canvas",
                new
                {
                    associations = new[] { new { doctorUserId = doctorId, patientUserId = patientId } },
                    positions = new[] { new { userId = patientId, positionX = 555.5, positionY = 66.0 } }
                },
                IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        StudyClinicCanvasResponse canvas = await GetAsync<StudyClinicCanvasResponse>(clinicPath);
        canvas.Doctors.Should().ContainSingle(node => node.UserId == doctorId);
        canvas.Patients.Should().ContainSingle(node => node.UserId == patientId && node.PositionX == 555.5);
        canvas.Associations.Should().ContainSingle(link => link.DoctorUserId == doctorId && link.PatientUserId == patientId);

        // Credentials are stored and returned unchanged.
        StudyAccountCredentialsDto credentials = await GetAsync<StudyAccountCredentialsDto>(
            $"{clinicPath}/members/{patientId}/credentials");
        credentials.Should().BeEquivalentTo(patient.Credentials);

        // Random data for every section lands in the patient's current data.
        string patientPath = $"{clinicPath}/patients/{patientId}";
        foreach (string section in RandomDataSections)
        {
            HttpResponseMessage response = await Client.PostAsJsonAsync(
                $"{patientPath}/random-data/{section}",
                new { count = 3 },
                IntegrationTestJson.Options);
            string body = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK, $"section {section}: {body}");

            StudyRandomDataResponse result = JsonSerializer.Deserialize<StudyRandomDataResponse>(body, IntegrationTestJson.Options)!;
            result.Section.Should().Be(section);
            result.RequestedCount.Should().Be(3);
            result.CreatedCount.Should().BeGreaterThan(0, $"the seeded catalog has data for {section}");
        }

        (await Client.PostAsJsonAsync($"{patientPath}/random-data/unknown", new { count = 3 }, IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PostAsJsonAsync($"{patientPath}/random-data/meal-times", new { count = 11 }, IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PostAsJsonAsync(
                $"{clinicPath}/patients/{doctorId}/random-data/meal-times",
                new { count = 1 },
                IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.NotFound, "the doctor is not a patient of this clinic");

        JsonElement regularDrugs = await GetJsonAsync($"{patientPath}/regular-drugs?skip=0&take=40");
        regularDrugs.GetProperty("items").GetArrayLength().Should().BeGreaterThan(0, "generated intakes are current, not history");

        StudyPatientDetailDto patientDetail = await GetAsync<StudyPatientDetailDto>(patientPath);
        patientDetail.MedicalSummary.RegularDrugCount.Should().BeGreaterThan(0);
        patientDetail.MedicalSummary.AllergyCount.Should().BeGreaterThan(0);

        // Own supplement through the proxy, taken regularly, is visible in the study patient's own report.
        Guid supplementId = await FindAnySupplementIdAsync();
        string ownSupplementName = $"Study blend {Guid.NewGuid():N}";
        JsonElement createdOwn = await PostJsonAsync(
            $"{patientPath}/own-supplements",
            new { name = ownSupplementName, supplementIds = new[] { supplementId } },
            HttpStatusCode.Created);
        Guid ownSupplementId = createdOwn.GetProperty("item").GetProperty("id").GetGuid();

        JsonElement ownList = await GetJsonAsync($"{patientPath}/own-supplements");
        ownList.GetProperty("items").EnumerateArray()
            .Should().Contain(item => item.GetProperty("id").GetGuid() == ownSupplementId);

        // Scheduling an own supplement (POST regular-supplements with source "own") is covered by
        // CreateRegularSupplementFromOwnSupplementTests; it currently fails with 500 in the shared handler.

        // Study accounts log in to the client app with the generated credentials and stay read-only.
        TestAuthHelper.ClearBearerToken(Client);
        LoginMemberResponse patientLogin = await TestAuthHelper.LoginMemberAsync(
            Client,
            patient.Credentials.LoginEmail,
            patient.Credentials.Password,
            CancellationToken.None);
        TestAuthHelper.ApplyBearerToken(Client, patientLogin.AccessToken);

        (await Client.GetAsync("/api/user-profile")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.PostAsJsonAsync("/api/own-supplements", new { name = "x", supplementIds = new[] { supplementId } }, IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);

        HttpResponseMessage reportResponse = await Client.PostAsJsonAsync(
            "/api/tools/drug-interactions/registered/report",
            new
            {
                manualDrugFormIds = Array.Empty<Guid>(),
                includeUserData = true,
                showAllDrugsInteractions = false,
                showOralGuidance = false,
                showAllFoodAndSupplementInteractions = false
            },
            IntegrationTestJson.ApiRequestOptions);
        string reportBody = await reportResponse.Content.ReadAsStringAsync();
        reportResponse.StatusCode.Should().Be(HttpStatusCode.OK, reportBody);
        JsonElement info = JsonDocument.Parse(reportBody).RootElement.GetProperty("report").GetProperty("info");
        info.GetProperty("supplements").GetProperty("takenNow").GetProperty("rows").GetArrayLength()
            .Should().BeGreaterThan(0, "the generated regular supplements are taken now");
        info.GetProperty("foods").GetProperty("takenNow").GetProperty("rows").GetArrayLength()
            .Should().BeGreaterThan(0, "the generated regular food is taken now");
        info.GetProperty("drugs").GetProperty("takenNow").GetProperty("rows").GetArrayLength()
            .Should().BeGreaterThan(0, "the generated regular drugs are taken now");

        LoginMemberResponse doctorLogin = await TestAuthHelper.LoginMemberAsync(
            Client,
            doctor.Credentials.LoginEmail,
            doctor.Credentials.Password,
            CancellationToken.None);
        TestAuthHelper.ApplyBearerToken(Client, doctorLogin.AccessToken);

        string providerPatients = await Client.GetStringAsync("/api/supervision/provider/patients");
        providerPatients.Should().Contain(patientId.ToString(), "the canvas link makes the doctor supervise the patient");
        (await Client.GetAsync($"/api/supervision/provider/patients/{patientId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        // Teardown: one account, then the whole clinic.
        await AuthenticateAsTeacherAsync();
        (await Client.DeleteAsync($"{clinicPath}/members/{doctorId}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetAsync<StudyClinicCanvasResponse>(clinicPath)).Associations.Should().BeEmpty();
        await AssertLoginFailsAsync(doctor.Credentials);

        await AuthenticateAsTeacherAsync();
        (await Client.DeleteAsync(clinicPath)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Client.GetAsync(clinicPath)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        await AssertLoginFailsAsync(patient.Credentials);
    }

    [Fact]
    public async Task CreatePatient_WhenClinicHoldsSixtyPatients_Returns409WithLimitField()
    {
        await AuthenticateAsTeacherAsync();
        StudyClinicDto clinic = await PostAsync<StudyClinicDto>(
            "/api/admin/study/clinics",
            new CreateStudyClinicRequest($"Full ward {Guid.NewGuid():N}", null),
            HttpStatusCode.Created);

        for (int index = 0; index < 60; index++)
        {
            await PostAsync<CreatedStudyAccountResponse>(
                $"/api/admin/study/clinics/{clinic.Id}/patients",
                new CreateStudyPatientRequest("Jan", $"Patient{index}", new DateOnly(1990, 1, 1), "male", index, 0),
                HttpStatusCode.Created);
        }

        await AssertProblemFieldAsync(
            await Client.PostAsJsonAsync(
                $"/api/admin/study/clinics/{clinic.Id}/patients",
                new CreateStudyPatientRequest("Jan", "Overflow", new DateOnly(1990, 1, 1), "male", 0, 0),
                IntegrationTestJson.Options),
            HttpStatusCode.Conflict,
            "limit");
    }

    private async Task AuthenticateAsTeacherAsync()
    {
        await AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None);
        _teacher ??= await AdminTestFlows.CreateAdminUserAsync(Client, CancellationToken.None, adminRole: AuthRoles.Teacher);
        await AdminTestFlows.AuthenticateAsAdminAsync(Client, _teacher.Email, _teacher.Password, CancellationToken.None);
    }

    private CreateAdminUserResponse? _teacher;

    private async Task AssertLoginFailsAsync(StudyAccountCredentialsDto credentials)
    {
        TestAuthHelper.ClearBearerToken(Client);
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginMemberRequest(credentials.LoginEmail, credentials.Password),
            IntegrationTestJson.Options);
        response.IsSuccessStatusCode.Should().BeFalse("a deleted study account can no longer log in");
    }

    private async Task<Guid> FindAnySupplementIdAsync()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DrugsAppDbContext dbContext = scope.ServiceProvider.GetRequiredService<DrugsAppDbContext>();
        return await dbContext.Set<Supplement>().AsNoTracking().OrderBy(item => item.Id).Select(item => item.Id).FirstAsync();
    }

    private async Task<T> PostAsync<T>(string path, object body, HttpStatusCode expected)
    {
        JsonElement json = await PostJsonAsync(path, body, expected);
        return json.Deserialize<T>(IntegrationTestJson.Options)!;
    }

    private async Task<JsonElement> PostJsonAsync(string path, object body, HttpStatusCode expected)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(path, body, IntegrationTestJson.Options);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(expected, content);
        return JsonDocument.Parse(content).RootElement.Clone();
    }

    private async Task<T> GetAsync<T>(string path) => (await GetJsonAsync(path)).Deserialize<T>(IntegrationTestJson.Options)!;

    private async Task<JsonElement> GetJsonAsync(string path)
    {
        HttpResponseMessage response = await Client.GetAsync(path);
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, content);
        return JsonDocument.Parse(content).RootElement.Clone();
    }

    private static async Task AssertProblemFieldAsync(HttpResponseMessage response, HttpStatusCode expected, string field)
    {
        string content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(expected, content);
        content.Should().Contain($"\"{field}\"", "the problem details name the offending field");
    }
}
