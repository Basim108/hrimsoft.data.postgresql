using System;
using Hrimsoft.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hrimsoft.Data.PostgreSql;

/// <summary>Custom env variables name for db configuration</summary>
/// <param name="ConnectionVarName">Name of an environment variable that holds a connection string name</param>
/// <param name="HostVarName">Name of an environment variable that holds a db host</param>
/// <param name="PortVarName">Name of an environment variable that holds a db port</param>
/// <param name="UserVarName">Name of an environment variable that holds a db user name</param>
/// <param name="PasswordVarName">Name of an environment variable that holds a db user password</param>
public record DbConfigEnvVariables(string ConnectionVarName,
                                   string HostVarName,
                                   string PortVarName,
                                   string DatabaseVarName,
                                   string UserVarName,
                                   string PasswordVarName,
                                   string MigrationHistoryTableVarName,
                                   string MigrationHistorySchemaVarName);

/// <summary> </summary>
public static class AddNpgsqlContextRegistration
{
    /// <summary>Registers a DbContext. Takes values from connection string and/or environment variables</summary>
    /// <param name="appConfig"></param>
    /// <param name="migrationAssembly">Assembly name where migrations are located</param>
    /// <param name="services"></param>
    /// <param name="dbVars">Custom env variables name for db configuration</param>
    /// <exception cref="ArgumentNullException">Application configuration is required</exception>
    /// <exception cref="ConfigurationException">Throws when connection string is empty or there is no password db environment variable</exception>
    public static IServiceCollection AddNpgsqlContext<TContext>(this IServiceCollection services,
                                                                IConfiguration          appConfig,
                                                                string                  migrationAssembly = null,
                                                                DbConfigEnvVariables    dbVars            = null)
        where TContext : DbContext {
        if (appConfig == null)
            throw new ArgumentNullException(nameof(appConfig));

        services.AddDbContext<TContext>(options => {
            var (connectionString, historyTable, historySchema) = ConnectionStringBuilder.Get(appConfig, dbVars);

            if (string.IsNullOrWhiteSpace(migrationAssembly))
                options.UseNpgsql(connectionString);
            else
                options.UseNpgsql(connectionString,
                                  npgsqlOptionsAction => {
                                      npgsqlOptionsAction.MigrationsAssembly(migrationAssembly);
                                      if (!string.IsNullOrWhiteSpace(historyTable))
                                          npgsqlOptionsAction.MigrationsHistoryTable(historyTable, historySchema);
                                  });
        });
        return services;
    }
}