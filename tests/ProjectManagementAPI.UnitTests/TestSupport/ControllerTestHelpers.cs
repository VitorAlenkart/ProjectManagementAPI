using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ProjectManagementAPI.UnitTests.TestSupport;

internal static class ControllerTestHelpers
{
    public static IConfiguration CreateJwtConfiguration()
    {
        IConfiguration result = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "this-is-a-test-secret-key-with-32-chars",
                ["Jwt:Issuer"] = "ProjectManagementAPI.Tests",
                ["Jwt:Audience"] = "ProjectManagementAPI.Tests"
            })
            .Build();

        return result;
    }

    public static JsonElement ToJsonElement(object value)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(value));
        JsonElement result = document.RootElement.Clone();

        return result;
    }

    public static void SetAuthenticatedUser(ControllerBase controller, int userId, string role = "Teacher")
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, $"user{userId}@example.com"),
                new Claim(ClaimTypes.Role, role)
            },
            "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
