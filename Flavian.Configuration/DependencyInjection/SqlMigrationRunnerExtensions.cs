using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Flavian.Persistence.Database.Migrations;

namespace Flavian.Configuration.DependencyInjection;

public static class SqlMigrationRunnerExtensions
{
    public static async Task ApplySqlMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("SqlMigration");

        try
        {
            await SqlMigrationRunner.RunMigrationsAsync(scope.ServiceProvider, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed.");
            Console.WriteLine($"[Migration Error] {ex.Message}");
        }
    }
}
