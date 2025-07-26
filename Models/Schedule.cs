using System;
using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ContactName { get; set; }

        [Required]
        public DateTime SelectedDate { get; set; }

        [Required]
        public List<string> Services { get; set; }

        [Required]
        public List<string> Timeslots { get; set; }

        public string Note { get; set; }

        public string Uid { get; set; }
    }
}