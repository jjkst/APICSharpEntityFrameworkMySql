using FluentAssertions;
using FluentValidation.TestHelper;
using RukuServiceApi.Models;
using RukuServiceApi.Validators;

namespace RukuServiceApi.UnitTests.Validators;

[TestClass]
public sealed class UpdateServiceRequestValidatorTests
{
    private UpdateServiceRequestValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new UpdateServiceRequestValidator();
    }

    [TestMethod]
    public void ValidRequest_ShouldPassValidation()
    {
        var request = new UpdateServiceRequest
        {
            Title = "Updated Service",
            Description = "A valid description that meets the minimum length requirement.",
            Features = new List<string> { "Feature A" },
            PricingPlans = new List<PricingPlan>
            {
                new PricingPlan
                {
                    Name = "Standard",
                    InitialSetupFee = "$200.00",
                    MonthlySubscription = "$50.00",
                    Features = new List<string> { "Support" }
                }
            }
        };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void EmptyTitle_ShouldFailValidation()
    {
        var request = new UpdateServiceRequest
        {
            Title = "",
            Description = "A valid description that meets minimum length."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [TestMethod]
    public void TitleTooLong_ShouldFailValidation()
    {
        var request = new UpdateServiceRequest
        {
            Title = new string('A', 101),
            Description = "A valid description that meets minimum length."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [TestMethod]
    public void TitleWithSpecialCharacters_ShouldFailValidation()
    {
        var request = new UpdateServiceRequest
        {
            Title = "Service @#$%",
            Description = "A valid description that meets minimum length."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [TestMethod]
    public void EmptyDescription_ShouldFailValidation()
    {
        var request = new UpdateServiceRequest
        {
            Title = "Valid Title",
            Description = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void DescriptionTooLong_ShouldFailValidation()
    {
        var request = new UpdateServiceRequest
        {
            Title = "Valid Title",
            Description = new string('A', 1001)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void FileNameWithSpaces_ShouldFailValidation()
    {
        var request = new UpdateServiceRequest
        {
            Title = "Valid Title",
            Description = "A valid description that meets minimum length.",
            FileName = "file with spaces.jpg"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [TestMethod]
    public void ValidFileName_ShouldPassValidation()
    {
        var request = new UpdateServiceRequest
        {
            Title = "Valid Title",
            Description = "A valid description that meets minimum length.",
            FileName = "valid-file_name.jpg"
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
    }

    [TestMethod]
    public void FileNameTooLong_ShouldFailValidation()
    {
        var request = new UpdateServiceRequest
        {
            Title = "Valid Title",
            Description = "A valid description that meets minimum length.",
            FileName = new string('a', 256)
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }
}
