using System.ComponentModel.DataAnnotations;

namespace RukuServiceApi.Models
{
    public class RukuService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? FileName { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public List<string> Features { get; set; } = new();

        public List<PricingPlan> PricingPlans { get; set; } = new();
    }
}
