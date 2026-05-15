using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ProjectManagementAPI.IntegrationsTests.TestSupport;

internal static class IntegrationTestHelpers
{
    public static async Task<string> GetTokenAsync(HttpClient client, string email = "teacher@example.com", string password = "Senha@123")
    {
        string result;

        var response = await client.PostAsJsonAsync("/api/login", new
        {
            email,
            password
        });
        response.EnsureSuccessStatusCode();

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();
        result = body.GetProperty("token").GetString()!;

        return result;
    }

    public static async Task AuthenticateAsTeacherAsync(HttpClient client)
    {
        string token = await GetTokenAsync(client);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task AuthenticateAsOtherTeacherAsync(HttpClient client)
    {
        string token = await GetTokenAsync(client, "other.teacher@example.com");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
