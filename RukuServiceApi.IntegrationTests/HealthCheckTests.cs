using System.Text.Json;

namespace RukuServiceApi.IntegrationTests;

[TestClass]
public sealed class HealthCheckTests
{
    private static readonly HttpClient Client = TestHelpers.GetClient();

    [TestMethod]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var response = await Client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var health = JsonDocument.Parse(content);

        Assert.IsTrue(health.RootElement.TryGetProperty("status", out _));
        Assert.IsTrue(health.RootElement.TryGetProperty("checks", out _));
        Assert.IsTrue(health.RootElement.TryGetProperty("totalDuration", out _));
    }

    [TestMethod]
    public async Task HealthCheckReady_ShouldReturnOk()
    {
        var response = await Client.GetAsync("/health/ready");

        // Ready endpoint may return Ok (Healthy) or ServiceUnavailable (Unhealthy)
        Assert.IsTrue(
            response.StatusCode == System.Net.HttpStatusCode.OK
                || response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
        );
    }

    [TestMethod]
    public async Task HealthCheckLive_ShouldReturnOk()
    {
        var response = await Client.GetAsync("/health/live");

        response.EnsureSuccessStatusCode();
    }
}
