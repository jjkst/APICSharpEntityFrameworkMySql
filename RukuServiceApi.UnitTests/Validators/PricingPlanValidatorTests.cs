using FluentAssertions;
using FluentValidation.TestHelper;
using RukuServiceApi.Models;
using RukuServiceApi.Validators;

namespace RukuServiceApi.UnitTests.Validators;

[TestClass]
public sealed class PricingPlanValidatorTests
{
    private PricingPlanValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new PricingPlanValidator();
    }

    [TestMethod]
    public void ValidPricingPlan_ShouldPassValidation()
    {
        var plan = new PricingPlan
        {
            Name = "Basic Plan",
            InitialSetupFee = "$100.00",
            MonthlySubscription = "$25.00",
            Features = new List<string> { "Feature 1" }
        };

        var result = _validator.TestValidate(plan);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void EmptyName_ShouldFailValidation()
    {
        var plan = new PricingPlan
        {
            Name = "",
            InitialSetupFee = "$100.00",
            MonthlySubscription = "$25.00"
        };

        var result = _validator.TestValidate(plan);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [TestMethod]
    public void NameTooShort_ShouldFailValidation()
    {
        var plan = new PricingPlan
        {
            Name = "AB",
            InitialSetupFee = "$100.00",
            MonthlySubscription = "$25.00"
        };

        var result = _validator.TestValidate(plan);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [TestMethod]
    public void NameTooLong_ShouldFailValidation()
    {
        var plan = new PricingPlan
        {
            Name = new string('A', 51),
            InitialSetupFee = "$100.00",
            MonthlySubscription = "$25.00"
        };

        var result = _validator.TestValidate(plan);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [TestMethod]
    public void EmptyInitialSetupFee_ShouldFailValidation()
    {
        var plan = new PricingPlan
        {
            Name = "Basic Plan",
            InitialSetupFee = "",
            MonthlySubscription = "$25.00"
        };

        var result = _validator.TestValidate(plan);

        result.ShouldHaveValidationErrorFor(x => x.InitialSetupFee);
    }

    [TestMethod]
    public void InvalidInitialSetupFeeFormat_ShouldFailValidation()
    {
        var plan = new PricingPlan
        {
            Name = "Basic Plan",
            InitialSetupFee = "one hundred",
            MonthlySubscription = "$25.00"
        };

        var result = _validator.TestValidate(plan);

        result.ShouldHaveValidationErrorFor(x => x.InitialSetupFee);
    }

    [TestMethod]
    public void ValidPriceWithoutDollarSign_ShouldPassValidation()
    {
        var plan = new PricingPlan
        {
            Name = "Basic Plan",
            InitialSetupFee = "100.00",
            MonthlySubscription = "25.00"
        };

        var result = _validator.TestValidate(plan);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void EmptyMonthlySubscription_ShouldFailValidation()
    {
        var plan = new PricingPlan
        {
            Name = "Basic Plan",
            InitialSetupFee = "$100.00",
            MonthlySubscription = ""
        };

        var result = _validator.TestValidate(plan);

        result.ShouldHaveValidationErrorFor(x => x.MonthlySubscription);
    }

    [TestMethod]
    public void InvalidMonthlySubscriptionFormat_ShouldFailValidation()
    {
        var plan = new PricingPlan
        {
            Name = "Basic Plan",
            InitialSetupFee = "$100.00",
            MonthlySubscription = "free"
        };

        var result = _validator.TestValidate(plan);

        result.ShouldHaveValidationErrorFor(x => x.MonthlySubscription);
    }

    [TestMethod]
    public void FeatureExceeding200Characters_ShouldFailValidation()
    {
        var plan = new PricingPlan
        {
            Name = "Basic Plan",
            InitialSetupFee = "$100.00",
            MonthlySubscription = "$25.00",
            Features = new List<string> { new string('x', 201) }
        };

        var result = _validator.TestValidate(plan);

        result.ShouldHaveValidationErrorFor("Features[0]");
    }

    [TestMethod]
    public void WholeNumberPrice_ShouldPassValidation()
    {
        var plan = new PricingPlan
        {
            Name = "Basic Plan",
            InitialSetupFee = "$100",
            MonthlySubscription = "25"
        };

        var result = _validator.TestValidate(plan);

        result.IsValid.Should().BeTrue();
    }
}
