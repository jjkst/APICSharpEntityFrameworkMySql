using FluentAssertions;
using FluentValidation.TestHelper;
using RukuServiceApi.Models;
using RukuServiceApi.Validators;

namespace RukuServiceApi.UnitTests.Validators;

[TestClass]
public sealed class UpdateUserRoleRequestValidatorTests
{
    private UpdateUserRoleRequestValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new UpdateUserRoleRequestValidator();
    }

    [TestMethod]
    public void ValidAdminRole_ShouldPassValidation()
    {
        var request = new UpdateUserRoleRequest { Role = "Admin" };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void ValidOwnerRole_ShouldPassValidation()
    {
        var request = new UpdateUserRoleRequest { Role = "Owner" };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void ValidSubscriberRole_ShouldPassValidation()
    {
        var request = new UpdateUserRoleRequest { Role = "Subscriber" };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void CaseInsensitiveRole_ShouldPassValidation()
    {
        var request = new UpdateUserRoleRequest { Role = "admin" };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void EmptyRole_ShouldFailValidation()
    {
        var request = new UpdateUserRoleRequest { Role = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [TestMethod]
    public void InvalidRole_ShouldFailValidation()
    {
        var request = new UpdateUserRoleRequest { Role = "SuperAdmin" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [TestMethod]
    public void NumericRole_ShouldPassValidation_BecauseEnumTryParseAcceptsIntegers()
    {
        // .NET's Enum.TryParse accepts numeric strings as valid enum values
        var request = new UpdateUserRoleRequest { Role = "123" };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [TestMethod]
    public void RandomString_ShouldFailValidation()
    {
        var request = new UpdateUserRoleRequest { Role = "NotARole" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Role);
    }
}
