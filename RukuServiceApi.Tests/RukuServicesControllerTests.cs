using System.Text.Json;

namespace RukuServiceApi.Tests;

[TestClass]
public sealed class RukuServicesControllerTests
{
    private static readonly HttpClient Client = TestHelpers.GetClient();
    private static string? _adminToken;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _adminToken = await TestHelpers.GetAdminTokenAsync();
    }

    [TestMethod]
    public async Task GetAllRukuServices_ShouldReturnList()
    {
        var response = await Client.GetAsync("/api/rukuservices");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var services = JsonDocument.Parse(content);

        Assert.IsNotNull(services);
    }

    [TestMethod]
    public async Task CreateRukuService_WithValidData_ShouldReturnCreated()
    {
        var uniqueId = Guid.NewGuid();
        var createRequest = new
        {
            title = $"Ruku Service {uniqueId}",
            description = $"A new Ruku service description that is long enough {uniqueId}",
            features = new[] { "Feature 1", "Feature 2" },
            pricingPlans = new[]
            {
                new
                {
                    name = "Plan 1",
                    initialSetupFee = "$300",
                    monthlySubscription = "$150",
                    features = new[] { "Plan Feature" },
                },
            },
        };

        var content = TestHelpers.CreateJsonContent(createRequest);
        var response = await Client.PostAsync("/api/rukuservices", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var service = JsonDocument.Parse(responseContent);

        Assert.IsTrue(service.RootElement.TryGetProperty("id", out _));
    }

    [TestMethod]
    public async Task GetRukuServiceById_WithValidId_ShouldReturnService()
    {
        // First create a service
        var uniqueId = Guid.NewGuid();
        var createRequest = new
        {
            title = $"Ruku Service for Get {uniqueId}",
            description = $"Description for getting service that is long enough {uniqueId}",
            features = new[] { "Feature" },
        };

        var createContent = TestHelpers.CreateJsonContent(createRequest);
        var createResponse = await Client.PostAsync("/api/rukuservices", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdService = JsonDocument.Parse(createResponseContent);
        var serviceId = createdService.RootElement.GetProperty("id").GetInt32();

        // Now get it
        var getResponse = await Client.GetAsync($"/api/rukuservices/{serviceId}");
        getResponse.EnsureSuccessStatusCode();

        var getContent = await getResponse.Content.ReadAsStringAsync();
        var service = JsonDocument.Parse(getContent);

        Assert.AreEqual(serviceId, service.RootElement.GetProperty("id").GetInt32());
    }

    [TestMethod]
    public async Task UpdateRukuService_WithValidData_ShouldReturnOk()
    {
        // First create a service
        var uniqueId = Guid.NewGuid();
        var createRequest = new
        {
            title = $"Ruku Service to Update {uniqueId}",
            description = $"Original description that is long enough for validation {uniqueId}",
            features = new[] { "Original" },
        };

        var createContent = TestHelpers.CreateJsonContent(createRequest);
        var createResponse = await Client.PostAsync("/api/rukuservices", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdService = JsonDocument.Parse(createResponseContent);
        var serviceId = createdService.RootElement.GetProperty("id").GetInt32();

        // Now update it
        var updateUniqueId = Guid.NewGuid();
        var updateRequest = new
        {
            id = serviceId,
            title = $"Updated Ruku Service {updateUniqueId}",
            description = $"Updated description that is long enough for validation {updateUniqueId}",
            features = new[] { "Updated" },
        };

        var updateContent = TestHelpers.CreateJsonContent(updateRequest);
        var updateResponse = await Client.PutAsync($"/api/rukuservices/{serviceId}", updateContent);

        updateResponse.EnsureSuccessStatusCode();
    }

    [TestMethod]
    public async Task DeleteRukuService_WithValidId_ShouldReturnNoContent()
    {
        // First create a service
        var uniqueId = Guid.NewGuid();
        var createRequest = new
        {
            title = $"Ruku Service to Delete {uniqueId}",
            description = $"This service will be deleted and is long enough {uniqueId}",
            features = new[] { "Feature" },
        };

        var createContent = TestHelpers.CreateJsonContent(createRequest);
        var createResponse = await Client.PostAsync("/api/rukuservices", createContent);
        createResponse.EnsureSuccessStatusCode();

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdService = JsonDocument.Parse(createResponseContent);
        var serviceId = createdService.RootElement.GetProperty("id").GetInt32();

        // Now delete it
        var deleteResponse = await Client.DeleteAsync($"/api/rukuservices/{serviceId}");

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
