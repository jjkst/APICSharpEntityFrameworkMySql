using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RukuServiceApi.Context;

/// <summary>
/// Design-time factory so `dotnet ef` can create the DbContext without the WebApplication host.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        connectionString = ResolveEnvPlaceholder(connectionString);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing. Set the CONNECTIONSTRING environment variable or update appsettings.json."
            );
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 42)));

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string? ResolveEnvPlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (
            value.StartsWith("${", StringComparison.Ordinal)
            && value.EndsWith("}", StringComparison.Ordinal)
        )
        {
            var envName = value[2..^1];
            var envValue = Environment.GetEnvironmentVariable(envName);
            if (string.IsNullOrWhiteSpace(envValue))
            {
                throw new InvalidOperationException(
                    $"Environment variable '{envName}' required to create the DbContext was not found.\n"
                        + $"Please set it before running EF Core commands:\n"
                        + $"  export {envName}=\"your-value\"\n"
                        + $"Or load from .env.local:\n"
                        + $"  export $(grep -v '^#' .env.local | xargs)\n"
                        + $"For Docker, run migrations inside the container:\n"
                        + $"  docker-compose exec ruku-service-api-dev dotnet ef database update"
                );
            }

            return envValue;
        }

        return value;
    }
}
