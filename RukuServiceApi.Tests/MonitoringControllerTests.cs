using System.Text.Json;

namespace RukuServiceApi.Tests;

[TestClass]
public sealed class MonitoringControllerTests
{
    private static readonly HttpClient Client = TestHelpers.GetClient();
    private static string? _adminToken;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _adminToken = await TestHelpers.GetAdminTokenAsync();
    }

    [TestMethod]
    public async Task GetSystemInfo_WithAdminAuth_ShouldReturnSystemInfo()
    {
        var request = TestHelpers.CreateAuthenticatedRequest(
            HttpMethod.Get,
            "/api/monitoring/system-info",
            _adminToken
        );
        var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var systemInfo = JsonDocument.Parse(content);

        Assert.IsTrue(systemInfo.RootElement.TryGetProperty("application", out _));
        Assert.IsTrue(systemInfo.RootElement.TryGetProperty("system", out _));
        Assert.IsTrue(systemInfo.RootElement.TryGetProperty("environment", out _));
    }

    [TestMethod]
    public async Task GetSystemInfo_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync("/api/monitoring/system-info");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task GetPerformanceMetrics_WithAdminAuth_ShouldReturnMetrics()
    {
        var request = TestHelpers.CreateAuthenticatedRequest(
            HttpMethod.Get,
            "/api/monitoring/performance",
            _adminToken
        );
        var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var metrics = JsonDocument.Parse(content);

        Assert.IsTrue(metrics.RootElement.TryGetProperty("memory", out _));
        Assert.IsTrue(metrics.RootElement.TryGetProperty("cpu", out _));
        Assert.IsTrue(metrics.RootElement.TryGetProperty("threads", out _));
    }

    [TestMethod]
    public async Task GetRecentLogs_WithAdminAuth_ShouldReturnLogs()
    {
        var request = TestHelpers.CreateAuthenticatedRequest(
            HttpMethod.Get,
            "/api/monitoring/logs?count=10",
            _adminToken
        );
        var response = await Client.SendAsync(request);

        // Could be Ok or NotFound if no log files exist
        Assert.IsTrue(
            response.StatusCode == System.Net.HttpStatusCode.OK
                || response.StatusCode == System.Net.HttpStatusCode.NotFound
        );
    }

    [TestMethod]
    public async Task ForceGarbageCollection_WithAdminAuth_ShouldReturnOk()
    {
        var request = TestHelpers.CreateAuthenticatedRequest(
            HttpMethod.Post,
            "/api/monitoring/gc",
            _adminToken
        );
        var response = await Client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(content);

        Assert.IsTrue(result.RootElement.TryGetProperty("beforeMemoryMB", out _));
        Assert.IsTrue(result.RootElement.TryGetProperty("afterMemoryMB", out _));
    }
}
