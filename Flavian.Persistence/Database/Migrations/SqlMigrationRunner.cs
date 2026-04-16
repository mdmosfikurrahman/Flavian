using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Flavian.Persistence.Data;
using Flavian.Shared.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Flavian.Persistence.Database.Migrations;

public static class SqlMigrationRunner
{
    public static async Task RunMigrationsAsync(IServiceProvider services, ILogger logger)
    {
        var databaseContext = services.GetRequiredService<FlavianDbContext>();
        var dbConnection = databaseContext.Database.GetDbConnection();
        await dbConnection.OpenAsync();

        await EnsureMigrationTrackingTableExistsAsync(dbConnection);

        var appliedMigrations = await LoadAppliedMigrationsAsync(dbConnection);

        var changelogPath = Path.Combine(
            AppContext.BaseDirectory,
            "Database",
            "Changelog.yaml"
        );

        if (!File.Exists(changelogPath))
            throw new FileNotFoundException($"Changelog file not found at: {changelogPath}");

        var changelog = LoadChangelog(changelogPath);

        var migrationsToRun = LoadMigrationsFromChangelog(changelog);

        foreach (var migration in migrationsToRun)
        {
            logger.LogDebug("Checking migration: {MigrationId} - {MigrationFileName}, checksum: {MigrationChecksum}",
                migration.Id, migration.FileName, migration.Checksum);

            if (appliedMigrations.TryGetValue(migration.FileName, out var storedChecksum))
            {
                if (storedChecksum != migration.Checksum)
                {
                    throw new InvalidOperationException(
                        $"Migration file '{migration.FileName}' has been modified after being applied.\n" +
                        $"Expected checksum: {storedChecksum}\n" +
                        $"Current checksum:  {migration.Checksum}\n" +
                        $"Revert changes or create a new migration file.");
                }

                logger.LogInformation("Skipping already applied migration: {MigrationId} - {MigrationFileName}",
                    migration.Id, migration.FileName);
                continue;
            }

            logger.LogInformation("Applying migration: {MigrationId} - {MigrationFileName} by {MigrationAuthor}",
                migration.Id, migration.FileName, migration.Author);
            await ApplyMigrationAsync(dbConnection, migration);
        }

        logger.LogInformation("All pending migrations have been applied successfully.");
    }

    private static async Task EnsureMigrationTrackingTableExistsAsync(DbConnection dbConnection)
    {
        var createTableCommand = dbConnection.CreateCommand();
        createTableCommand.CommandText = """
                                             IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='schema_migrations' AND xtype='U')
                                             CREATE TABLE schema_migrations (
                                                 id NVARCHAR(100) UNIQUE NOT NULL,
                                                 filename NVARCHAR(255) UNIQUE NOT NULL,
                                                 checksum NVARCHAR(128) NOT NULL,
                                                 author NVARCHAR(100) NOT NULL,
                                                 description NVARCHAR(MAX) NOT NULL,
                                                 applied_at DATETIME2 NOT NULL
                                             );
                                         """;
        await createTableCommand.ExecuteNonQueryAsync();
    }

    private static async Task<Dictionary<string, string>> LoadAppliedMigrationsAsync(DbConnection dbConnection)
    {
        var appliedMigrations = new Dictionary<string, string>();
        var selectCommand = dbConnection.CreateCommand();
        selectCommand.CommandText = "SELECT filename, checksum FROM schema_migrations;";

        using var reader = await selectCommand.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var filename = reader.GetString(0);
            var checksum = reader.GetString(1);
            appliedMigrations[filename] = checksum;
        }

        return appliedMigrations;
    }

    private static Changelog LoadChangelog(string filePath)
    {
        var yamlContent = File.ReadAllText(filePath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<Changelog>(yamlContent);
    }

    private static List<SqlMigration> LoadMigrationsFromChangelog(Changelog changelog)
    {
        var sqlDirectoryPath = Path.Combine(
            AppContext.BaseDirectory,
            "Database",
            "Db"
        );
        var result = new List<SqlMigration>();

        if (changelog.Migrations.Count == 0)
            throw new InvalidOperationException("No migrations found in Changelog.yaml.");

        var idSet = new HashSet<string>();
        foreach (var entry in changelog.Migrations.Where(entry => !idSet.Add(entry.Id)))
        {
            throw new InvalidOperationException(
                $"Duplicate migration ID detected: '{entry.Id}'. Migration IDs must be unique.");
        }

        foreach (var entry in changelog.Migrations)
        {
            var filePath = Path.Combine(sqlDirectoryPath, entry.Filename);
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Migration file not found: {entry.Filename}");

            var fileContent = File.ReadAllText(filePath);
            var checksum = CalculateSha256Checksum(fileContent);

            result.Add(new SqlMigration(
                entry.Id,
                entry.Filename,
                fileContent,
                checksum,
                entry.Author,
                entry.Description));
        }

        return result;
    }

    private static async Task ApplyMigrationAsync(DbConnection dbConnection, SqlMigration migration)
    {
        using var transaction = await dbConnection.BeginTransactionAsync();

        try
        {
            var applyCommand = dbConnection.CreateCommand();
            applyCommand.CommandText = migration.Content;
            applyCommand.Transaction = transaction;
            await applyCommand.ExecuteNonQueryAsync();

            var insertCommand = dbConnection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO schema_migrations (id, filename, checksum, author, description, applied_at)
                VALUES (@id, @filename, @checksum, @author, @description, @appliedAt)";
            insertCommand.Transaction = transaction;
            insertCommand.Parameters.Add(CreateParameter(insertCommand, "@id", migration.Id));
            insertCommand.Parameters.Add(CreateParameter(insertCommand, "@filename", migration.FileName));
            insertCommand.Parameters.Add(CreateParameter(insertCommand, "@checksum", migration.Checksum));
            insertCommand.Parameters.Add(CreateParameter(insertCommand, "@author", migration.Author));
            insertCommand.Parameters.Add(CreateParameter(insertCommand, "@description", migration.Description));
            insertCommand.Parameters.Add(CreateParameter(insertCommand, "@appliedAt", TimeUtils.GetUtcNow()));

            await insertCommand.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static DbParameter CreateParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        return parameter;
    }

    private static string CalculateSha256Checksum(string content)
    {
        using var sha256 = SHA256.Create();
        var normalizedContent = content.Replace("\r\n", "\n");
        var bytes = Encoding.UTF8.GetBytes(normalizedContent);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }

    private record SqlMigration(
        string Id,
        string FileName,
        string Content,
        string Checksum,
        string Author,
        string Description);

    private class Changelog
    {
        public List<MigrationEntry> Migrations { get; set; } = new();
    }

    private class MigrationEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
    }
}
