using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string Uid { get; set; }

        public UserRole Role { get; set; }
        public ProviderList Provider { get; set; }
    }

    public enum UserRole
    {
        Admin = 1,
        Owner = 2,
        Subscriber = 3,
    }

    public enum ProviderList
    {
        Google = 1,
        Facebook = 2,
        Apple = 3,
    }
}
