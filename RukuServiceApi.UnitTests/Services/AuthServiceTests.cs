using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RukuServiceApi.Models;
using RukuServiceApi.Services;

namespace RukuServiceApi.UnitTests.Services;

[TestClass]
public sealed class AuthServiceTests
{
    private Mock<ILogger<AuthService>> _loggerMock = null!;
    private const string ValidSecretKey = "ThisIsAVeryLongSecretKeyForTesting123456!";

    private AuthService CreateService(JwtSettings settings)
    {
        _loggerMock = new Mock<ILogger<AuthService>>();
        var options = Options.Create(settings);
        return new AuthService(options, _loggerMock.Object);
    }

    private static User CreateTestUser() => new()
    {
        Id = 1,
        Email = "test@example.com",
        DisplayName = "Test User",
        Uid = "test-uid-12345",
        Role = UserRole.Admin
    };

    [TestMethod]
    public void GenerateJwtToken_WithValidSettings_ShouldReturnToken()
    {
        var service = CreateService(new JwtSettings
        {
            SecretKey = ValidSecretKey,
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 60
        });
        var user = CreateTestUser();

        var token = service.GenerateJwtToken(user);

        token.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void GenerateJwtToken_ShouldContainExpectedClaims()
    {
        var service = CreateService(new JwtSettings
        {
            SecretKey = ValidSecretKey,
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 60
        });
        var user = CreateTestUser();

        var token = service.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
        jwt.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "Test User");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
        jwt.Claims.Should().Contain(c => c.Type == "uid" && c.Value == "test-uid-12345");
        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
    }

    [TestMethod]
    public void GenerateJwtToken_WithEmptySecretKey_ShouldThrowInvalidOperationException()
    {
        var service = CreateService(new JwtSettings
        {
            SecretKey = "",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 60
        });
        var user = CreateTestUser();

        var act = () => service.GenerateJwtToken(user);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SecretKey is not configured*");
    }

    [TestMethod]
    public void GenerateJwtToken_WithShortSecretKey_ShouldThrowInvalidOperationException()
    {
        var service = CreateService(new JwtSettings
        {
            SecretKey = "short",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = 60
        });
        var user = CreateTestUser();

        var act = () => service.GenerateJwtToken(user);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*at least 32 characters*");
    }

    [TestMethod]
    public void GenerateJwtToken_ShouldSetCorrectExpiration()
    {
        var expirationMinutes = 30;
        var service = CreateService(new JwtSettings
        {
            SecretKey = ValidSecretKey,
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationMinutes = expirationMinutes
        });
        var user = CreateTestUser();

        var token = service.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var expectedExpiry = DateTime.UtcNow.AddMinutes(expirationMinutes);

        jwt.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [TestMethod]
    public void ValidateUser_WithValidInputs_ShouldReturnTrue()
    {
        var service = CreateService(new JwtSettings { SecretKey = ValidSecretKey });

        var result = service.ValidateUser("test@example.com", "test-uid");

        result.Should().BeTrue();
    }

    [TestMethod]
    public void ValidateUser_WithEmptyEmail_ShouldReturnFalse()
    {
        var service = CreateService(new JwtSettings { SecretKey = ValidSecretKey });

        var result = service.ValidateUser("", "test-uid");

        result.Should().BeFalse();
    }

    [TestMethod]
    public void ValidateUser_WithEmptyUid_ShouldReturnFalse()
    {
        var service = CreateService(new JwtSettings { SecretKey = ValidSecretKey });

        var result = service.ValidateUser("test@example.com", "");

        result.Should().BeFalse();
    }
}
