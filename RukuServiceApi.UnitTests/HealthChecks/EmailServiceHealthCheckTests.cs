using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RukuServiceApi.HealthChecks;
using RukuServiceApi.Models;

namespace RukuServiceApi.UnitTests.HealthChecks;

[TestClass]
public sealed class EmailServiceHealthCheckTests
{
    private Mock<ILogger<EmailServiceHealthCheck>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<EmailServiceHealthCheck>>();
    }

    private EmailServiceHealthCheck CreateHealthCheck(EmailSettings settings)
    {
        var options = Options.Create(settings);
        return new EmailServiceHealthCheck(options, _loggerMock.Object);
    }

    [TestMethod]
    public async Task CheckHealthAsync_WithCompleteSettings_ShouldReturnHealthy()
    {
        var healthCheck = CreateHealthCheck(new EmailSettings
        {
            SmtpServer = "smtp.example.com",
            SmtpPort = 587,
            SmtpUsername = "user@example.com",
            SmtpPassword = "password",
            EnableSsl = true
        });
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("configured");
    }

    [TestMethod]
    public async Task CheckHealthAsync_WithEmptySmtpServer_ShouldReturnDegraded()
    {
        var healthCheck = CreateHealthCheck(new EmailSettings
        {
            SmtpServer = "",
            SmtpUsername = "user@example.com",
            SmtpPassword = "password"
        });
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("incomplete");
    }

    [TestMethod]
    public async Task CheckHealthAsync_WithEmptySmtpUsername_ShouldReturnDegraded()
    {
        var healthCheck = CreateHealthCheck(new EmailSettings
        {
            SmtpServer = "smtp.example.com",
            SmtpUsername = "",
            SmtpPassword = "password"
        });
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [TestMethod]
    public async Task CheckHealthAsync_WithEmptySmtpPassword_ShouldReturnDegraded()
    {
        var healthCheck = CreateHealthCheck(new EmailSettings
        {
            SmtpServer = "smtp.example.com",
            SmtpUsername = "user@example.com",
            SmtpPassword = ""
        });
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [TestMethod]
    public async Task CheckHealthAsync_WithCompleteSettings_ShouldIncludeDataFields()
    {
        var healthCheck = CreateHealthCheck(new EmailSettings
        {
            SmtpServer = "smtp.example.com",
            SmtpPort = 587,
            SmtpUsername = "user@example.com",
            SmtpPassword = "password",
            EnableSsl = true
        });
        var context = new HealthCheckContext();

        var result = await healthCheck.CheckHealthAsync(context);

        result.Data.Should().ContainKey("smtpServer");
        result.Data.Should().ContainKey("smtpPort");
        result.Data.Should().ContainKey("enableSsl");
        result.Data.Should().ContainKey("timestamp");
    }
}
