using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProjectManagementAPI.IntegrationsTests.TestSupport;

namespace ProjectManagementAPI.IntegrationsTests;

public sealed class ProjectsIntegrationTests : IClassFixture<ProjectManagementApiFactory>
{
    private readonly HttpClient _client;
    private readonly ProjectManagementApiFactory _factory;

    public ProjectsIntegrationTests(ProjectManagementApiFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProjects_WhenProjectsExist_ReturnsProjects()
    {
        var response = await _client.GetAsync("/api/Projects");
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Array, body.ValueKind);
        Assert.True(body.GetArrayLength() >= 2);
    }

    [Fact]
    public async Task GetProject_WhenProjectExists_ReturnsDetailedProject()
    {
        var response = await _client.GetAsync("/api/Projects/100");
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Seeded Project", body.GetProperty("name").GetString());
        Assert.Equal(1, body.GetProperty("students").GetArrayLength());
    }

    [Fact]
    public async Task PostProject_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/Projects", new
        {
            name = "Unauthorized Project",
            description = "Should fail"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostProject_WhenTeacherIsAuthenticated_CreatesProject()
    {
        await IntegrationTestHelpers.AuthenticateAsTeacherAsync(_client);

        var response = await _client.PostAsJsonAsync("/api/Projects", new
        {
            name = "Integration Project",
            description = "Created through HTTP"
        });
        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Integration Project", body.GetProperty("name").GetString());
        Assert.Equal(1, body.GetProperty("teacherId").GetInt32());
    }

    [Fact]
    public async Task PutProject_WhenProjectExists_UpdatesProject()
    {
        await IntegrationTestHelpers.AuthenticateAsTeacherAsync(_client);

        var response = await _client.PutAsJsonAsync("/api/Projects/100", new
        {
            name = "Updated Project",
            description = "Updated through HTTP"
        });
        var getResponse = await _client.GetAsync("/api/Projects/100");
        JsonElement body = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Updated Project", body.GetProperty("name").GetString());
        Assert.Equal("Updated through HTTP", body.GetProperty("description").GetString());
    }

    [Fact]
    public async Task DeleteProject_WhenTeacherDoesNotOwnProject_ReturnsForbidden()
    {
        await IntegrationTestHelpers.AuthenticateAsOtherTeacherAsync(_client);

        var response = await _client.DeleteAsync("/api/Projects/100");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProject_WhenTeacherOwnsProject_DeletesProject()
    {
        await IntegrationTestHelpers.AuthenticateAsTeacherAsync(_client);

        var response = await _client.DeleteAsync("/api/Projects/100");
        var getResponse = await _client.GetAsync("/api/Projects/100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task AddStudentToProject_WhenStudentIsNotLinked_AddsRelation()
    {
        await IntegrationTestHelpers.AuthenticateAsTeacherAsync(_client);

        var response = await _client.PostAsJsonAsync("/api/Projects/link/100/students", new
        {
            studentId = 11,
            role = "Frontend"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await _client.GetAsync("/api/Projects/100");
        JsonElement body = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(2, body.GetProperty("students").GetArrayLength());
    }

    [Fact]
    public async Task AddStudentToProject_WhenStudentIsAlreadyLinked_ReturnsBadRequest()
    {
        await IntegrationTestHelpers.AuthenticateAsTeacherAsync(_client);

        var response = await _client.PostAsJsonAsync("/api/Projects/link/100/students", new
        {
            studentId = 10,
            role = "Backend"
        });

        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Student already in Project", body);
    }

    [Fact]
    public async Task DeleteStudentFromProject_WhenStudentIsLinked_RemovesRelation()
    {
        await IntegrationTestHelpers.AuthenticateAsTeacherAsync(_client);

        var response = await _client.DeleteAsync("/api/Projects/100/unlink/10");
        var getResponse = await _client.GetAsync("/api/Projects/100");
        JsonElement body = await getResponse.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(0, body.GetProperty("students").GetArrayLength());
    }
}
