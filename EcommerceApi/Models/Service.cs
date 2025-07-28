using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models
{
    public class PricingPlan
    {
        public string Name { get; set; }
        public string InitialSetupFee { get; set; }
        public string MonthlySubscription { get; set; }
        public List<string> Features { get; set; }
    }

    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string FileName { get; set; }

        [Required]
        public string Description { get; set; }

        public List<string> Features { get; set; }

        public List<PricingPlan> PricingPlans { get; set; }
    }
}
