using FluentAssertions;
using FluentValidation.TestHelper;
using RukuServiceApi.Models;
using RukuServiceApi.Validators;

namespace RukuServiceApi.UnitTests.Validators;

[TestClass]
public sealed class CreateUserRequestValidatorTests
{
    private CreateUserRequestValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new CreateUserRequestValidator();
    }

    [TestMethod]
    public void ValidRequest_ShouldPassValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            Uid = "user-uid-12345",
            DisplayName = "John Doe"
        };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void EmptyEmail_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "",
            Uid = "user-uid-12345"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [TestMethod]
    public void InvalidEmailFormat_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "not-an-email",
            Uid = "user-uid-12345"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [TestMethod]
    public void EmailTooLong_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Email = new string('a', 250) + "@x.com",
            Uid = "user-uid-12345"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [TestMethod]
    public void EmptyUid_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            Uid = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Uid);
    }

    [TestMethod]
    public void UidTooShort_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            Uid = "abcd"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Uid);
    }

    [TestMethod]
    public void UidTooLong_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            Uid = new string('a', 101)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Uid);
    }

    [TestMethod]
    public void UidWithInvalidCharacters_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            Uid = "user uid !@#"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Uid);
    }

    [TestMethod]
    public void NullDisplayName_ShouldPassValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            Uid = "user-uid-12345",
            DisplayName = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.DisplayName);
    }

    [TestMethod]
    public void DisplayNameTooLong_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            Uid = "user-uid-12345",
            DisplayName = new string('A', 101)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }

    [TestMethod]
    public void DisplayNameWithInvalidCharacters_ShouldFailValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            Uid = "user-uid-12345",
            DisplayName = "John <script>"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DisplayName);
    }
}
