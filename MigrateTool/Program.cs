using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MigrateTool;

class Program
{
    private static readonly string ProjectDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "RukuServiceApi")
    );
    private static readonly string EnvFile = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env.local")
    );

    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        LoadEnvironmentVariables();

        var command = args[0].ToLower();
        var remainingArgs = args.Skip(1).ToArray();

        try
        {
            await EnsureEfToolsInstalledAsync();

            switch (command)
            {
                case "create":
                    await CreateMigrationAsync(remainingArgs);
                    break;
                case "update":
                    await UpdateDatabaseAsync(remainingArgs);
                    break;
                case "script":
                    await GenerateSqlScriptAsync(remainingArgs);
                    break;
                case "rollback":
                    await RollbackMigrationAsync(remainingArgs);
                    break;
                case "remove":
                    await RemoveMigrationAsync();
                    break;
                case "status":
                    await ShowStatusAsync();
                    break;
                case "drop":
                    await DropDatabaseAsync(remainingArgs);
                    break;
                case "reset":
                    await ResetDatabaseAsync(remainingArgs);
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowUsage();
                    Environment.Exit(1);
                    break;
            }
        }
        catch (Exception ex)
        {
            WriteError($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static void LoadEnvironmentVariables()
    {
        if (!File.Exists(EnvFile))
        {
            WriteWarning(EnvFile);
            WriteWarning(".env.local not found. Make sure environment variables are set.");
            return;
        }

        WriteInfo("Loading environment variables from .env.local");

        var lines = File.ReadAllLines(EnvFile);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Skip comments and empty lines
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                continue;

            // Parse KEY=VALUE or KEY="VALUE" or KEY='VALUE'
            var match = Regex.Match(trimmed, @"^\s*([^=]+)=(.*)$");
            if (match.Success)
            {
                var key = match.Groups[1].Value.Trim();
                var value = match.Groups[2].Value.Trim();

                // Remove surrounding quotes if present
                if (
                    (value.StartsWith('"') && value.EndsWith('"'))
                    || (value.StartsWith('\'') && value.EndsWith('\''))
                )
                {
                    value = value.Substring(1, value.Length - 2);
                }

                Environment.SetEnvironmentVariable(key, value);
            }
        }

        var connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRING");
        if (string.IsNullOrEmpty(connectionString))
        {
            WriteWarning("CONNECTIONSTRING not found in .env.local");
        }
        else
        {
            // Show a masked version for security
            var masked =
                connectionString.Length > 20 ? connectionString.Substring(0, 20) + "..." : "***";
            WriteInfo($"Environment variables loaded successfully (CONNECTIONSTRING: {masked})");
        }
    }

    private static async Task EnsureEfToolsInstalledAsync()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "tool list --global",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (!output.Contains("dotnet-ef"))
        {
            WriteError("dotnet-ef tools not found. Installing...");
            await RunCommandAsync("dotnet", "tool install --global dotnet-ef");
            WriteInfo("dotnet-ef tools installed");
        }
        else
        {
            WriteInfo("dotnet-ef tools found");
        }
    }

    private static async Task CreateMigrationAsync(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            WriteError("Migration name is required");
            Console.WriteLine("Usage: migrate create <migration_name>");
            Environment.Exit(1);
            return;
        }

        var migrationName = args[0];
        WriteInfo($"Creating migration: {migrationName}");

        await RunCommandAsync("dotnet", $"ef migrations add {migrationName}", ProjectDir);
        WriteInfo("Migration created successfully");
    }

    private static async Task UpdateDatabaseAsync(string[] args)
    {
        var environment = args.Length > 0 ? args[0] : "Development";
        WriteInfo($"Updating database for environment: {environment}");

        if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            WriteWarning(
                "Updating production database. Make sure you have the correct connection string!"
            );
        }

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        await RunCommandAsync("dotnet", "ef database update", ProjectDir);
        WriteInfo("Database updated successfully");
    }

    private static async Task DropDatabaseAsync(string[] args)
    {
        var environment = args.Length > 0 ? args[0] : "Development";
        WriteWarning($"Dropping database for environment: {environment}");

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        await RunCommandAsync("dotnet", "ef database drop --force --no-build", ProjectDir);
        WriteInfo("Database dropped");
    }

    private static async Task ResetDatabaseAsync(string[] args)
    {
        var environment = args.Length > 0 ? args[0] : "Development";
        await DropDatabaseAsync(new[] { environment });
        await UpdateDatabaseAsync(new[] { environment });
    }

    private static async Task GenerateSqlScriptAsync(string[] args)
    {
        var fromMigration = args.Length > 0 ? args[0] : null;
        var outputFile = args.Length > 1 ? args[1] : "migration_script.sql";

        WriteInfo("Generating SQL script...");

        var arguments =
            fromMigration != null
                ? $"ef migrations script {fromMigration} --output ../{outputFile}"
                : $"ef migrations script --output ../{outputFile}";

        await RunCommandAsync("dotnet", arguments, ProjectDir);
        WriteInfo($"SQL script generated: {outputFile}");
    }

    private static async Task RollbackMigrationAsync(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
        {
            WriteError("Target migration name is required");
            Console.WriteLine("Usage: migrate rollback <target_migration_name>");
            Environment.Exit(1);
            return;
        }

        var targetMigration = args[0];
        WriteWarning($"Rolling back to migration: {targetMigration}");

        await RunCommandAsync("dotnet", $"ef database update {targetMigration}", ProjectDir);
        WriteInfo("Rollback completed");
    }

    private static async Task RemoveMigrationAsync()
    {
        WriteWarning("Removing last migration (not applied to database)");
        await RunCommandAsync("dotnet", "ef migrations remove", ProjectDir);
        WriteInfo("Migration removed");
    }

    private static async Task ShowStatusAsync()
    {
        WriteInfo("Current migration status:");
        await RunCommandAsync("dotnet", "ef migrations list", ProjectDir);
    }

    private static async Task RunCommandAsync(
        string fileName,
        string arguments,
        string? workingDirectory = null
    )
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            },
        };

        // Copy all environment variables to the child process
        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            process.StartInfo.EnvironmentVariables[entry.Key.ToString()!] = entry.Value?.ToString();
        }

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                Console.WriteLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                Console.Error.WriteLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Command failed with exit code {process.ExitCode}");
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("RukuServiceApi Database Migration Tool");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run --project MigrateTool -- <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  create <name>     Create a new migration");
        Console.WriteLine("  update [env]      Update database (Development/Production)");
        Console.WriteLine("  script [from] [output] Generate SQL script");
        Console.WriteLine("  rollback <name>   Rollback to specific migration");
        Console.WriteLine("  remove           Remove last migration (not applied)");
        Console.WriteLine("  status           Show migration status");
        Console.WriteLine("  drop [env]       Drop database for environment (default Development)");
        Console.WriteLine("  reset [env]      Drop then apply migrations for environment");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run --project MigrateTool -- create AddUserTable");
        Console.WriteLine("  dotnet run --project MigrateTool -- update Development");
        Console.WriteLine("  dotnet run --project MigrateTool -- update Production");
        Console.WriteLine(
            "  dotnet run --project MigrateTool -- script InitialCreate migration.sql"
        );
        Console.WriteLine("  dotnet run --project MigrateTool -- rollback InitialCreate");
        Console.WriteLine("  dotnet run --project MigrateTool -- status");
        Console.WriteLine("  dotnet run --project MigrateTool -- drop Development");
        Console.WriteLine("  dotnet run --project MigrateTool -- reset Development");
    }

    private static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[INFO] {message}");
        Console.ResetColor();
    }

    private static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARNING] {message}");
        Console.ResetColor();
    }

    private static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }
}
