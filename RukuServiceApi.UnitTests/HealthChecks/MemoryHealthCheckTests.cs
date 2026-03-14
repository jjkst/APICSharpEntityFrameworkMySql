using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using RukuServiceApi.HealthChecks;

namespace RukuServiceApi.UnitTests.HealthChecks;

[TestClass]
public sealed class MemoryHealthCheckTests
{
    private Mock<ILogger<MemoryHealthCheck>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<MemoryHealthCheck>>();
    }

    [TestMethod]
    public async Task CheckHealthAsync_ShouldReturnHealthyOrDegraded()
    {
        var healthCheck = new MemoryHealthCheck(_loggerMock.Object);
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
    }

    [TestMethod]
    public async Task CheckHealthAsync_ShouldIncludeMemoryUsageInData()
    {
        var healthCheck = new MemoryHealthCheck(_loggerMock.Object);
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Data.Should().ContainKey("memoryUsageMB");
        result.Data.Should().ContainKey("processId");
        result.Data.Should().ContainKey("timestamp");
    }

    [TestMethod]
    public async Task CheckHealthAsync_ShouldReturnDescriptionWithMemoryUsage()
    {
        var healthCheck = new MemoryHealthCheck(_loggerMock.Object);
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Description.Should().NotBeNullOrEmpty();
        result.Description.Should().Contain("MB");
    }
}
