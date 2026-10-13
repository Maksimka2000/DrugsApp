using System.Net;
using System.Net.Http.Json;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AdminUserManagementFeature.Shared;
using DrugsApp.Features.StudyModeFeature.CreateStudyClinic;
using DrugsApp.Features.StudyModeFeature.CreateStudyDoctor;
using DrugsApp.Features.StudyModeFeature.CreateStudyPatient;
using DrugsApp.Features.StudyModeFeature.Shared;
using DrugsApp.IntegrationTests.Common;
using FluentAssertions;

namespace DrugsApp.IntegrationTests.StudyModeFeature;

/// <summary>
/// Guards the public/private clinic rules: a private clinic is invisible to everyone but its creator (admins
/// included) on every route, only the creator changes visibility, deleting a teacher removes their private
/// clinics with the study accounts, and parallel creates cannot exceed the member caps.
/// </summary>
public sealed class StudyClinicVisibilityTests : IntegrationTestBase
{
    [Fact]
    public async Task PrivateClinic_IsInvisibleToOtherTeachersAndAdmins_OnEveryRoute()
    {
        CreateAdminUserResponse owner = await CreateTeacherAsync();
        CreateAdminUserResponse other = await CreateTeacherAsync();

        await LoginAsync(owner);
        StudyClinicDto clinic = await CreateClinicAsync("private-one", visibility: null);
        clinic.Visibility.Should().BeEquivalentTo("private", "a new clinic is private by default");
        clinic.IsOwner.Should().BeTrue();
        CreatedStudyAccountResponse patient = await CreatePatientAsync(clinic.Id, "Eve");
        Guid patientId = patient.Patient!.UserId;

        foreach (Func<Task> loginAsViewer in new Func<Task>[]
                 {
                     () => LoginAsync(other),
                     () => AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None)
                 })
        {
            await loginAsViewer();

            StudyClinicListResponse list = await Client.GetFromJsonAsync<StudyClinicListResponse>(
                "/api/admin/study/clinics?take=40", IntegrationTestJson.Options) ?? throw new InvalidOperationException();
            list.Items.Should().NotContain(item => item.Id == clinic.Id);

            string root = $"/api/admin/study/clinics/{clinic.Id}";
            (await Client.GetAsync(root)).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await Client.GetAsync($"{root}/patients/{patientId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await Client.GetAsync($"{root}/members/{patientId}/credentials")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await Client.GetAsync($"{root}/patients/{patientId}/regular-drugs")).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await Client.PostAsJsonAsync($"{root}/patients/{patientId}/random-data/meal-times", new { count = 1 }, IntegrationTestJson.Options))
                .StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await Client.DeleteAsync(root)).StatusCode.Should().Be(HttpStatusCode.NotFound);
            (await Client.PutAsJsonAsync($"{root}/visibility", new { visibility = "public" }, IntegrationTestJson.Options))
                .StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task Visibility_OnlyCreatorChangesIt_AndPublicBecomesVisibleToOthers()
    {
        CreateAdminUserResponse owner = await CreateTeacherAsync();
        CreateAdminUserResponse other = await CreateTeacherAsync();

        await LoginAsync(owner);
        StudyClinicDto clinic = await CreateClinicAsync("shared-one", visibility: "private");
        string root = $"/api/admin/study/clinics/{clinic.Id}";

        (await Client.PutAsJsonAsync($"{root}/visibility", new { visibility = "bogus" }, IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Client.PutAsJsonAsync($"{root}/visibility", new { visibility = "public" }, IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        await LoginAsync(other);
        (await Client.GetAsync(root)).StatusCode.Should().Be(HttpStatusCode.OK, "a public clinic is visible to every teacher");
        StudyClinicDto asOther = (await Client.GetFromJsonAsync<StudyClinicCanvasResponse>(root, IntegrationTestJson.Options))!.Clinic;
        asOther.IsOwner.Should().BeFalse();
        (await Client.PutAsJsonAsync($"{root}/visibility", new { visibility = "private" }, IntegrationTestJson.Options))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden, "only the creator changes visibility");
    }

    [Fact]
    public async Task DeleteTeacher_RemovesPrivateClinicsAndTheirAccounts_AndKeepsPublicOnes()
    {
        CreateAdminUserResponse teacher = await CreateTeacherAsync();

        await LoginAsync(teacher);
        StudyClinicDto privateClinic = await CreateClinicAsync("doomed", visibility: "private");
        StudyClinicDto publicClinic = await CreateClinicAsync("survivor", visibility: "public");
        CreatedStudyAccountResponse doomedPatient = await CreatePatientAsync(privateClinic.Id, "Doomed");

        await AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None);
        (await Client.DeleteAsync($"/api/admin-users/{teacher.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await Client.GetAsync($"/api/admin/study/clinics/{publicClinic.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);

        TestAuthHelper.ClearBearerToken(Client);
        HttpResponseMessage login = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = doomedPatient.Credentials.LoginEmail, password = doomedPatient.Credentials.Password },
            IntegrationTestJson.ApiRequestOptions);
        login.IsSuccessStatusCode.Should().BeFalse("the study account of a deleted private clinic is gone");

        CreateAdminUserResponse other = await CreateTeacherAsync();
        await LoginAsync(other);
        (await Client.GetAsync($"/api/admin/study/clinics/{privateClinic.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateDoctor_WhenParallelRequestsRaceForTheLastSlot_OnlyOneSucceeds()
    {
        CreateAdminUserResponse teacher = await CreateTeacherAsync();
        await LoginAsync(teacher);
        StudyClinicDto clinic = await CreateClinicAsync("race", visibility: "private");

        for (int index = 0; index < 19; index++)
        {
            await CreateDoctorAsync(clinic.Id, $"Doc{index}");
        }

        HttpResponseMessage[] responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(index =>
            Client.PostAsJsonAsync(
                $"/api/admin/study/clinics/{clinic.Id}/doctors",
                new CreateStudyDoctorRequest("Race", $"Racer{index}", new DateOnly(1980, 1, 1), "male", "doctor", null, null, null, 0, 0),
                IntegrationTestJson.Options)));

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).Should().Be(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.Conflict).Should().Be(3);

        StudyClinicCanvasResponse canvas = (await Client.GetFromJsonAsync<StudyClinicCanvasResponse>(
            $"/api/admin/study/clinics/{clinic.Id}", IntegrationTestJson.Options))!;
        canvas.Doctors.Should().HaveCount(20);
    }

    private async Task<CreateAdminUserResponse> CreateTeacherAsync()
    {
        await AdminTestFlows.AuthenticateAsSuperAdminAsync(Client, CancellationToken.None);
        return await AdminTestFlows.CreateAdminUserAsync(Client, CancellationToken.None, adminRole: AuthRoles.Teacher);
    }

    private Task LoginAsync(CreateAdminUserResponse teacher) =>
        AdminTestFlows.AuthenticateAsAdminAsync(Client, teacher.Email, teacher.Password, CancellationToken.None);

    private async Task<StudyClinicDto> CreateClinicAsync(string prefix, string? visibility)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/admin/study/clinics",
            new CreateStudyClinicRequest($"{prefix} {Guid.NewGuid():N}", null, visibility),
            IntegrationTestJson.Options);
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<StudyClinicDto>(IntegrationTestJson.Options))!;
    }

    private async Task<CreatedStudyAccountResponse> CreatePatientAsync(Guid clinicId, string firstName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/admin/study/clinics/{clinicId}/patients",
            new CreateStudyPatientRequest(firstName, "Tester", new DateOnly(1990, 1, 1), "female", 0, 0),
            IntegrationTestJson.Options);
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<CreatedStudyAccountResponse>(IntegrationTestJson.Options))!;
    }

    private async Task CreateDoctorAsync(Guid clinicId, string lastName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/api/admin/study/clinics/{clinicId}/doctors",
            new CreateStudyDoctorRequest("Doc", lastName, new DateOnly(1980, 1, 1), "male", "doctor", null, null, null, 0, 0),
            IntegrationTestJson.Options);
        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
    }
}
