using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace EcommerceApi.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Availability> Availabilities { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var pricingPlansConverter = new ValueConverter<List<PricingPlan>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<PricingPlan>>(v, (JsonSerializerOptions)null)
            );

            modelBuilder.Entity<Service>().HasIndex(p => p.Title).IsUnique();
            modelBuilder
                .Entity<Service>()
                .Property(s => s.PricingPlans)
                .HasConversion(pricingPlansConverter);
            base.OnModelCreating(modelBuilder);
        }
    }
}
