using System.Text.Json;

namespace RukuServiceApi.IntegrationTests;

[TestClass]
public sealed class ServicesControllerTests
{
    private static readonly HttpClient Client = TestHelpers.GetClient();
    private static string? _adminToken;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _adminToken = await TestHelpers.GetAdminTokenAsync();
    }

    [TestMethod]
    public async Task GetAllServices_WithAuth_ShouldReturnServices()
    {
        var request = TestHelpers.CreateAuthenticatedRequest(HttpMethod.Get, "/api/services", _adminToken);
        var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var services = JsonDocument.Parse(content);

        Assert.IsNotNull(services);
    }

    [TestMethod]
    public async Task GetServiceById_WithValidId_ShouldReturnService()
    {
        // First create a service
        var uniqueId = Guid.NewGuid();
        var createRequest = new
        {
            title = $"Test Service {uniqueId}",
            description = $"Test Description for Service {uniqueId}",
            features = new[] { "Feature 1", "Feature 2" },
            pricingPlans = new[]
            {
                new
                {
                    name = "Basic",
                    initialSetupFee = "$100",
                    monthlySubscription = "$50",
                    features = new[] { "Basic Feature" }
                }
            }
        };

        var createContent = TestHelpers.CreateJsonContent(createRequest);
        var createHttpRequest = TestHelpers.CreateAuthenticatedRequest(HttpMethod.Post, "/api/services/create", _adminToken);
        createHttpRequest.Content = createContent;
        var createResponse = await Client.SendAsync(createHttpRequest);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdService = JsonDocument.Parse(createResponseContent);
        var serviceId = createdService.RootElement.GetProperty("id").GetInt32();

        // Now get it by ID
        var getRequest = TestHelpers.CreateAuthenticatedRequest(HttpMethod.Get, $"/api/services/{serviceId}", _adminToken);
        var getResponse = await Client.SendAsync(getRequest);

        getResponse.EnsureSuccessStatusCode();
        var getContent = await getResponse.Content.ReadAsStringAsync();
        var service = JsonDocument.Parse(getContent);

        Assert.AreEqual(serviceId, service.RootElement.GetProperty("id").GetInt32());
    }

    [TestMethod]
    public async Task CreateService_WithValidData_ShouldReturnCreated()
    {
        var uniqueId = Guid.NewGuid();
        var createRequest = new
        {
            title = $"New Service {uniqueId}",
            description = $"A new test service description that is long enough {uniqueId}",
            features = new[] { "Feature 1", "Feature 2", "Feature 3" },
            pricingPlans = new[]
            {
                new
                {
                    name = "Starter",
                    initialSetupFee = "$200",
                    monthlySubscription = "$100",
                    features = new[] { "Starter Feature 1", "Starter Feature 2" }
                }
            }
        };

        var content = TestHelpers.CreateJsonContent(createRequest);
        var request = TestHelpers.CreateAuthenticatedRequest(HttpMethod.Post, "/api/services/create", _adminToken);
        request.Content = content;
        var response = await Client.SendAsync(request);

        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var service = JsonDocument.Parse(responseContent);

        Assert.IsTrue(service.RootElement.TryGetProperty("id", out _));
        Assert.AreEqual(createRequest.title, service.RootElement.GetProperty("title").GetString());
    }

    [TestMethod]
    public async Task CreateService_WithoutAuth_ShouldReturnUnauthorized()
    {
        var createRequest = new
        {
            title = "Unauthorized Service",
            description = "This should fail without auth"
        };

        var content = TestHelpers.CreateJsonContent(createRequest);
        var response = await Client.PostAsync("/api/services/create", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdateService_WithValidData_ShouldReturnOk()
    {
        // First create a service
        var uniqueId = Guid.NewGuid();
        var createRequest = new
        {
            title = $"Service to Update {uniqueId}",
            description = $"Original description that is long enough for validation {uniqueId}",
            features = new[] { "Original Feature" }
        };

        var createContent = TestHelpers.CreateJsonContent(createRequest);
        var createHttpRequest = TestHelpers.CreateAuthenticatedRequest(HttpMethod.Post, "/api/services/create", _adminToken);
        createHttpRequest.Content = createContent;
        var createResponse = await Client.SendAsync(createHttpRequest);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdService = JsonDocument.Parse(createResponseContent);
        var serviceId = createdService.RootElement.GetProperty("id").GetInt32();

        // Now update it
        var updateUniqueId = Guid.NewGuid();
        var updateRequest = new
        {
            title = $"Updated Service {updateUniqueId}",
            description = $"Updated description that is long enough for validation {updateUniqueId}",
            features = new[] { "Updated Feature 1", "Updated Feature 2" }
        };

        var updateContent = TestHelpers.CreateJsonContent(updateRequest);
        var updateHttpRequest = TestHelpers.CreateAuthenticatedRequest(HttpMethod.Put, $"/api/services/update/{serviceId}", _adminToken);
        updateHttpRequest.Content = updateContent;
        var updateResponse = await Client.SendAsync(updateHttpRequest);

        updateResponse.EnsureSuccessStatusCode();
        var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        var updatedService = JsonDocument.Parse(updateResponseContent);

        Assert.AreEqual(updateRequest.title, updatedService.RootElement.GetProperty("title").GetString());
    }

    [TestMethod]
    public async Task GetAllServices_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync("/api/services");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task DeleteService_WithValidId_ShouldReturnNoContent()
    {
        // First create a service
        var uniqueId = Guid.NewGuid();
        var createRequest = new
        {
            title = $"Service to Delete {uniqueId}",
            description = $"This service will be deleted and is long enough {uniqueId}",
            features = new[] { "Feature" }
        };

        var createContent = TestHelpers.CreateJsonContent(createRequest);
        var createHttpRequest = TestHelpers.CreateAuthenticatedRequest(HttpMethod.Post, "/api/services/create", _adminToken);
        createHttpRequest.Content = createContent;
        var createResponse = await Client.SendAsync(createHttpRequest);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdService = JsonDocument.Parse(createResponseContent);
        var serviceId = createdService.RootElement.GetProperty("id").GetInt32();

        // Now delete it
        var deleteRequest = TestHelpers.CreateAuthenticatedRequest(HttpMethod.Delete, $"/api/services/{serviceId}", _adminToken);
        var deleteResponse = await Client.SendAsync(deleteRequest);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
