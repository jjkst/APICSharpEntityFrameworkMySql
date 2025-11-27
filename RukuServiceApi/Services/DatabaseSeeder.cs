using Microsoft.EntityFrameworkCore;
using RukuServiceApi.Context;
using RukuServiceApi.Models;

namespace RukuServiceApi.Services
{
    public interface IDatabaseSeeder
    {
        Task SeedAsync();
    }

    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedUsersAsync();
                await _context.SaveChangesAsync();
                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database seeding");
                throw;
            }
        }

        private async Task SeedUsersAsync()
        {
            if (await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Users already exist, skipping user seeding");
                return;
            }

            var adminUser = new User
            {
                Email = "admin@rukuit.com",
                Uid = "admin-uid-12345",
                DisplayName = "System Administrator",
                EmailVerified = true,
                Role = UserRole.Admin,
                Provider = ProviderList.Google,
            };

            var ownerUser = new User
            {
                Email = "owner@rukuit.com",
                Uid = "owner-uid-67890",
                DisplayName = "Business Owner",
                EmailVerified = true,
                Role = UserRole.Owner,
                Provider = ProviderList.Google,
            };

            _context.Users.AddRange(adminUser, ownerUser);
            _logger.LogInformation("Seeded admin and owner users");
        }
    }
}
