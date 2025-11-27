using FluentValidation;
using RukuServiceApi.Models;

namespace RukuServiceApi.Validators
{
    public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
    {
        public CreateServiceRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .Length(3, 100)
                .WithMessage("Title must be between 3 and 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_]+$")
                .WithMessage("Title contains invalid characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .Length(10, 1000)
                .WithMessage("Description must be between 10 and 1000 characters");

            RuleFor(x => x.FileName)
                .MaximumLength(255)
                .WithMessage("FileName cannot exceed 255 characters")
                .Matches(@"^[a-zA-Z0-9._-]+$")
                .When(x => !string.IsNullOrEmpty(x.FileName))
                .WithMessage("FileName contains invalid characters");

            RuleForEach(x => x.Features)
                .MaximumLength(200)
                .WithMessage("Each feature cannot exceed 200 characters");

            RuleForEach(x => x.PricingPlans).SetValidator(new PricingPlanValidator());
        }
    }

    public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
    {
        public UpdateServiceRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .Length(3, 100)
                .WithMessage("Title must be between 3 and 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_]+$")
                .WithMessage("Title contains invalid characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .Length(10, 1000)
                .WithMessage("Description must be between 10 and 1000 characters");

            RuleFor(x => x.FileName)
                .MaximumLength(255)
                .WithMessage("FileName cannot exceed 255 characters")
                .Matches(@"^[a-zA-Z0-9._-]+$")
                .When(x => !string.IsNullOrEmpty(x.FileName))
                .WithMessage("FileName contains invalid characters");

            RuleForEach(x => x.Features)
                .MaximumLength(200)
                .WithMessage("Each feature cannot exceed 200 characters");

            RuleForEach(x => x.PricingPlans).SetValidator(new PricingPlanValidator());
        }
    }

    public class PricingPlanValidator : AbstractValidator<PricingPlan>
    {
        public PricingPlanValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Plan name is required")
                .Length(3, 50)
                .WithMessage("Plan name must be between 3 and 50 characters");

            RuleFor(x => x.InitialSetupFee)
                .NotEmpty()
                .WithMessage("Initial setup fee is required")
                .Matches(@"^\$?\d+(\.\d{2})?$")
                .WithMessage("Invalid price format");

            RuleFor(x => x.MonthlySubscription)
                .NotEmpty()
                .WithMessage("Monthly subscription is required")
                .Matches(@"^\$?\d+(\.\d{2})?$")
                .WithMessage("Invalid price format");

            RuleForEach(x => x.Features)
                .MaximumLength(200)
                .WithMessage("Each feature cannot exceed 200 characters");
        }
    }

    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters");

            RuleFor(x => x.Uid)
                .NotEmpty()
                .WithMessage("UID is required")
                .Length(5, 100)
                .WithMessage("UID must be between 5 and 100 characters")
                .Matches(@"^[a-zA-Z0-9_-]+$")
                .WithMessage("UID contains invalid characters");

            RuleFor(x => x.DisplayName)
                .MaximumLength(100)
                .WithMessage("Display name cannot exceed 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_.]+$")
                .When(x => !string.IsNullOrEmpty(x.DisplayName))
                .WithMessage("Display name contains invalid characters");
        }
    }

    public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
    {
        public UpdateUserRoleRequestValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Role is required")
                .Must(BeValidRole)
                .WithMessage("Invalid role specified");
        }

        private bool BeValidRole(string role)
        {
            return Enum.TryParse<UserRole>(role, true, out _);
        }
    }

    public class ContactRequestValidator : AbstractValidator<ContactRequest>
    {
        public ContactRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .Length(2, 50)
                .WithMessage("First name must be between 2 and 50 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("First name contains invalid characters");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .Length(2, 50)
                .WithMessage("Last name must be between 2 and 50 characters")
                .Matches(@"^[a-zA-Z\s]+$")
                .WithMessage("Last name contains invalid characters");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^[\+]?[1-9][\d]{0,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format");

            RuleFor(x => x.Questions)
                .NotEmpty()
                .WithMessage("Questions are required")
                .Length(10, 1000)
                .WithMessage("Questions must be between 10 and 1000 characters");
        }
    }
}
