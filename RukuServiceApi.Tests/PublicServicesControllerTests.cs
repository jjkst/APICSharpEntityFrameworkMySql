using System.Text.Json;

namespace RukuServiceApi.Tests;

[TestClass]
public sealed class PublicServicesControllerTests
{
    private static readonly HttpClient Client = TestHelpers.GetClient();

    [TestMethod]
    public async Task GetServices_ShouldReturnListOfServices()
    {
        var response = await Client.GetAsync("/api/publicservices");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var services = JsonSerializer.Deserialize<List<object>>(content);

        Assert.IsNotNull(services);
    }

    [TestMethod]
    public async Task GetService_WithValidId_ShouldReturnService()
    {
        // First get all services to find a valid ID
        var getAllResponse = await Client.GetAsync("/api/publicservices");
        getAllResponse.EnsureSuccessStatusCode();
        var allServicesContent = await getAllResponse.Content.ReadAsStringAsync();
        var allServices = JsonDocument.Parse(allServicesContent).RootElement;

        if (allServices.GetArrayLength() > 0)
        {
            var firstService = allServices[0];
            var id = firstService.GetProperty("id").GetInt32();

            var response = await Client.GetAsync($"/api/publicservices/{id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var service = JsonDocument.Parse(content);

            Assert.IsTrue(service.RootElement.TryGetProperty("id", out var serviceId));
            Assert.AreEqual(id, serviceId.GetInt32());
        }
    }

    [TestMethod]
    public async Task GetService_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await Client.GetAsync("/api/publicservices/99999");

        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
