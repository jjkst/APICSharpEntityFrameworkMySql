using System.Text.Json;

namespace RukuServiceApi.IntegrationTests;

[TestClass]
public sealed class UsersControllerTests
{
    private static readonly HttpClient Client = TestHelpers.GetClient();
    private static string? _adminToken;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _adminToken = await TestHelpers.GetAdminTokenAsync();
    }

    [TestMethod]
    public async Task GetAllUsers_WithAuth_ShouldReturnUsers()
    {
        var request = TestHelpers.CreateAuthenticatedRequest(
            HttpMethod.Get,
            "/api/users",
            _adminToken
        );
        var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var users = JsonDocument.Parse(content);

        Assert.IsNotNull(users);
    }

    [TestMethod]
    public async Task GetUserById_WithValidId_ShouldReturnUser()
    {
        // First get all users to find a valid ID
        var getAllRequest = TestHelpers.CreateAuthenticatedRequest(
            HttpMethod.Get,
            "/api/users",
            _adminToken
        );
        var getAllResponse = await Client.SendAsync(getAllRequest);
        getAllResponse.EnsureSuccessStatusCode();

        var allUsersContent = await getAllResponse.Content.ReadAsStringAsync();
        var allUsers = JsonDocument.Parse(allUsersContent).RootElement;

        if (allUsers.GetArrayLength() > 0)
        {
            var firstUser = allUsers[0];
            var id = firstUser.GetProperty("id").GetInt32();

            var getRequest = TestHelpers.CreateAuthenticatedRequest(
                HttpMethod.Get,
                $"/api/users/{id}",
                _adminToken
            );
            var getResponse = await Client.SendAsync(getRequest);

            getResponse.EnsureSuccessStatusCode();
            var content = await getResponse.Content.ReadAsStringAsync();
            var user = JsonDocument.Parse(content);

            Assert.AreEqual(id, user.RootElement.GetProperty("id").GetInt32());
        }
    }

    [TestMethod]
    public async Task UpdateUserRole_WithAdminToken_ShouldReturnOk()
    {
        // First register a new user
        var uniqueEmail = $"testuser{Guid.NewGuid()}@example.com";
        var registerRequest = new
        {
            email = uniqueEmail,
            uid = $"test-uid-{Guid.NewGuid()}",
            displayName = "Test User",
            emailVerified = true,
            provider = 1,
        };

        var registerContent = TestHelpers.CreateJsonContent(registerRequest);
        var registerResponse = await Client.PostAsync("/api/auth/register", registerContent);
        registerResponse.EnsureSuccessStatusCode();

        var registerResponseContent = await registerResponse.Content.ReadAsStringAsync();
        var registeredUser = JsonDocument.Parse(registerResponseContent);
        var userId = registeredUser.RootElement.GetProperty("user").GetProperty("id").GetInt32();

        // Now update the role
        var roleContent = new StringContent(
            "\"Owner\"",
            System.Text.Encoding.UTF8,
            "application/json"
        );
        var updateRequest = TestHelpers.CreateAuthenticatedRequest(
            HttpMethod.Put,
            $"/api/users/{userId}/role",
            _adminToken
        );
        updateRequest.Content = roleContent;
        var updateResponse = await Client.SendAsync(updateRequest);

        updateResponse.EnsureSuccessStatusCode();
        var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        var updatedUser = JsonDocument.Parse(updateResponseContent);

        // Role is serialized as a number (enum), Owner = 2
        var roleValue = updatedUser.RootElement.GetProperty("role").GetInt32();
        Assert.AreEqual(2, roleValue); // Owner = 2 in UserRole enum
    }

    [TestMethod]
    public async Task UpdateUserRole_WithoutAuth_ShouldReturnUnauthorized()
    {
        var roleContent = new StringContent(
            "\"Owner\"",
            System.Text.Encoding.UTF8,
            "application/json"
        );
        var response = await Client.PutAsync("/api/users/1/role", roleContent);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteUser_WithAdminAuth_ShouldReturnNoContent()
    {
        // Register a throwaway user
        var uniqueEmail = $"deleteuser{Guid.NewGuid()}@example.com";
        var registerRequest = new
        {
            email = uniqueEmail,
            uid = $"delete-uid-{Guid.NewGuid()}",
            displayName = "User To Delete",
            emailVerified = true,
            provider = 1,
        };

        var registerContent = TestHelpers.CreateJsonContent(registerRequest);
        var registerResponse = await Client.PostAsync("/api/auth/register", registerContent);
        registerResponse.EnsureSuccessStatusCode();

        var registerResponseContent = await registerResponse.Content.ReadAsStringAsync();
        var registeredUser = JsonDocument.Parse(registerResponseContent);
        var userId = registeredUser
            .RootElement.GetProperty("user")
            .GetProperty("id")
            .GetInt32();

        // Delete the user
        var deleteRequest = TestHelpers.CreateAuthenticatedRequest(
            HttpMethod.Delete,
            $"/api/users/{userId}",
            _adminToken
        );
        var deleteResponse = await Client.SendAsync(deleteRequest);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [TestMethod]
    public async Task GetAllUsers_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync("/api/users");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetUserById_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync("/api/users/1");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteUser_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await Client.DeleteAsync("/api/users/1");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
