using FluentAssertions;
using FluentValidation.TestHelper;
using RukuServiceApi.Models;
using RukuServiceApi.Validators;

namespace RukuServiceApi.UnitTests.Validators;

[TestClass]
public sealed class CreateServiceRequestValidatorTests
{
    private CreateServiceRequestValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new CreateServiceRequestValidator();
    }

    [TestMethod]
    public void ValidRequest_ShouldPassValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "My Web Service",
            Description = "A description that is long enough to pass validation rules.",
            Features = new List<string> { "Feature 1", "Feature 2" },
            PricingPlans = new List<PricingPlan>
            {
                new PricingPlan
                {
                    Name = "Basic Plan",
                    InitialSetupFee = "$100.00",
                    MonthlySubscription = "$25.00",
                    Features = new List<string> { "Basic support" }
                }
            }
        };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void EmptyTitle_ShouldFailValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "",
            Description = "A valid description that meets minimum length."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [TestMethod]
    public void TitleTooShort_ShouldFailValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "AB",
            Description = "A valid description that meets minimum length."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [TestMethod]
    public void TitleWithInvalidCharacters_ShouldFailValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "My Service <script>",
            Description = "A valid description that meets minimum length."
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [TestMethod]
    public void EmptyDescription_ShouldFailValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "Valid Title",
            Description = ""
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void DescriptionTooShort_ShouldFailValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "Valid Title",
            Description = "Short"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void FileNameWithInvalidCharacters_ShouldFailValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "Valid Title",
            Description = "A valid description that meets minimum length.",
            FileName = "file name with spaces.jpg"
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FileName);
    }

    [TestMethod]
    public void NullFileName_ShouldPassValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "Valid Title",
            Description = "A valid description that meets minimum length.",
            FileName = null
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.FileName);
    }

    [TestMethod]
    public void FeatureExceeding200Characters_ShouldFailValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "Valid Title",
            Description = "A valid description that meets minimum length.",
            Features = new List<string> { new string('a', 201) }
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Features[0]");
    }

    [TestMethod]
    public void InvalidPricingPlan_ShouldFailValidation()
    {
        var request = new CreateServiceRequest
        {
            Title = "Valid Title",
            Description = "A valid description that meets minimum length.",
            PricingPlans = new List<PricingPlan>
            {
                new PricingPlan
                {
                    Name = "",
                    InitialSetupFee = "invalid",
                    MonthlySubscription = "$25.00"
                }
            }
        };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
    }
}
