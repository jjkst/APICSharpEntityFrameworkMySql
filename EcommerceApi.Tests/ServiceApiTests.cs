using System.Text;

namespace EcommerceApi.Tests;

[TestClass]
public sealed class ServiceApiTests
{
    private static readonly HttpClient client = new ();

    [TestMethod]
    public async Task PostService_FromJsonFile_ShouldReturnSuccess()
    {
        // Read the JSON file
        var projectDir = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;
        var jsonPath = Path.Combine(projectDir, "TestData", "my-services.json");
        var json = File.ReadAllText(jsonPath);

        // Parse the first service object from the array
        var servicesArray = System.Text.Json.JsonDocument.Parse(json).RootElement;
        var firstService = servicesArray[0].GetRawText();

        // POST to the API
        var content = new StringContent(firstService, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("http://localhost:5002/api/services", content);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
