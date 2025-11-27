using System.ComponentModel.DataAnnotations;

namespace RukuServiceApi.Models
{
    public class CreateServiceRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Title must be between 3 and 100 characters"
        )]
        public string Title { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "FileName cannot exceed 255 characters")]
        public string? FileName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage = "Description must be between 10 and 1000 characters"
        )]
        public string Description { get; set; } = string.Empty;

        public List<string>? Features { get; set; }
        public List<PricingPlan>? PricingPlans { get; set; }
    }

    public class UpdateServiceRequest
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(
            100,
            MinimumLength = 3,
            ErrorMessage = "Title must be between 3 and 100 characters"
        )]
        public string Title { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "FileName cannot exceed 255 characters")]
        public string? FileName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage = "Description must be between 10 and 1000 characters"
        )]
        public string Description { get; set; } = string.Empty;

        public List<string>? Features { get; set; }
        public List<PricingPlan>? PricingPlans { get; set; }
    }

    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "UID is required")]
        [StringLength(
            100,
            MinimumLength = 5,
            ErrorMessage = "UID must be between 5 and 100 characters"
        )]
        public string Uid { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters")]
        public string? DisplayName { get; set; }

        public bool EmailVerified { get; set; }
        public ProviderList Provider { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;
    }

    public class ContactRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(
            50,
            MinimumLength = 2,
            ErrorMessage = "First name must be between 2 and 50 characters"
        )]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(
            50,
            MinimumLength = 2,
            ErrorMessage = "Last name must be between 2 and 50 characters"
        )]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Questions are required")]
        [StringLength(
            1000,
            MinimumLength = 10,
            ErrorMessage = "Questions must be between 10 and 1000 characters"
        )]
        public string Questions { get; set; } = string.Empty;
    }
}
