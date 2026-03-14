using FluentAssertions;
using FluentValidation.TestHelper;
using RukuServiceApi.Models;
using RukuServiceApi.Validators;

namespace RukuServiceApi.UnitTests.Validators;

[TestClass]
public sealed class ContactRequestValidatorTests
{
    private ContactRequestValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new ContactRequestValidator();
    }

    [TestMethod]
    public void ValidRequest_ShouldPassValidation()
    {
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "+1234567890",
            Questions = "I have a question about your services and pricing."
        };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void EmptyFirstName_ShouldFailValidation()
    {
        var request = new ContactRequest
        {
            FirstName = "",
            LastName = "Doe",
            Email = "john@example.com",
            Questions = "I have a question about your services and pricing."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [TestMethod]
    public void FirstNameWithNumbers_ShouldFailValidation()
    {
        var request = new ContactRequest
        {
            FirstName = "John123",
            LastName = "Doe",
            Email = "john@example.com",
            Questions = "I have a question about your services and pricing."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [TestMethod]
    public void InvalidEmail_ShouldFailValidation()
    {
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "not-an-email",
            Questions = "I have a question about your services and pricing."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [TestMethod]
    public void InvalidPhoneNumber_ShouldFailValidation()
    {
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "abc",
            Questions = "I have a question about your services and pricing."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [TestMethod]
    public void NullPhoneNumber_ShouldPassValidation()
    {
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = null,
            Questions = "I have a question about your services and pricing."
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [TestMethod]
    public void QuestionsTooShort_ShouldFailValidation()
    {
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Questions = "Short"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Questions);
    }

    [TestMethod]
    public void EmptyQuestions_ShouldFailValidation()
    {
        var request = new ContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Questions = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Questions);
    }
}
