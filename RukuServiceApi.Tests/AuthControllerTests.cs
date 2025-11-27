using System.Text;
using System.Text.Json;

namespace RukuServiceApi.Tests;

[TestClass]
public sealed class AuthControllerTests
{
    private static readonly HttpClient Client = TestHelpers.GetClient();

    [TestMethod]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var loginRequest = new { email = "admin@rukuit.com", uid = "admin-uid-12345" };

        var content = TestHelpers.CreateJsonContent(loginRequest);
        var response = await Client.PostAsync("/api/auth/login", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseContent);

        Assert.IsTrue(json.RootElement.TryGetProperty("token", out var token));
        Assert.IsTrue(json.RootElement.TryGetProperty("user", out var user));
        Assert.IsFalse(string.IsNullOrEmpty(token.GetString()));
    }

    [TestMethod]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var loginRequest = new { email = "invalid@example.com", uid = "invalid-uid" };

        var content = TestHelpers.CreateJsonContent(loginRequest);
        var response = await Client.PostAsync("/api/auth/login", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Login_WithMissingEmail_ShouldReturnBadRequest()
    {
        var loginRequest = new { email = "", uid = "admin-uid-12345" };

        var content = TestHelpers.CreateJsonContent(loginRequest);
        var response = await Client.PostAsync("/api/auth/login", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task Register_WithNewUser_ShouldReturnToken()
    {
        var uniqueEmail = $"test{Guid.NewGuid()}@example.com";
        var registerRequest = new
        {
            email = uniqueEmail,
            uid = $"test-uid-{Guid.NewGuid()}",
            displayName = "Test User",
            emailVerified = true,
            provider = 1, // Google
        };

        var content = TestHelpers.CreateJsonContent(registerRequest);
        var response = await Client.PostAsync("/api/auth/register", content);

        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseContent);

        Assert.IsTrue(json.RootElement.TryGetProperty("token", out var token));
        Assert.IsTrue(json.RootElement.TryGetProperty("user", out var user));
        Assert.IsFalse(string.IsNullOrEmpty(token.GetString()));
    }

    [TestMethod]
    public async Task Register_WithExistingUser_ShouldReturnConflict()
    {
        var registerRequest = new
        {
            email = "admin@rukuit.com",
            uid = "admin-uid-12345",
            displayName = "Admin User",
            emailVerified = true,
            provider = 1,
        };

        var content = TestHelpers.CreateJsonContent(registerRequest);
        var response = await Client.PostAsync("/api/auth/register", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
}
