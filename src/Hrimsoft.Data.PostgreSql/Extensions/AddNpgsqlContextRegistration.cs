using System;
using Hrimsoft.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

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
                                   string UserVarName,
                                   string PasswordVarName);

/// <summary> </summary>
public static class AddNpgsqlContextRegistration {
    public const string DEFAULT_CONNECTION_STRING_NAME     = "db";
    public const string DEFAULT_CONNECTION_STRING_VAR_NAME = "DB";
    public const string DEFAULT_DB_HOST_VAR_NAME           = "DB_HOST";
    public const string DEFAULT_DB_PORT_VAR_NAME           = "DB_PORT";
    public const string DEFAULT_DB_USER_VAR_NAME           = "DB_USER";
    public const string DEFAULT_DB_PASSWORD_VAR_NAME       = "DB_PWD";

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
        if (dbVars == null) {
            dbVars = new DbConfigEnvVariables(DEFAULT_CONNECTION_STRING_VAR_NAME,
                                              DEFAULT_DB_HOST_VAR_NAME,
                                              DEFAULT_DB_PORT_VAR_NAME,
                                              DEFAULT_DB_USER_VAR_NAME,
                                              DEFAULT_DB_PASSWORD_VAR_NAME);
        }
        services.AddDbContext<TContext>(options => {
            var connectionString = GetConnectionStringValue(appConfig, dbVars.ConnectionVarName);
            var password         = GetDbPassword(appConfig, dbVars.PasswordVarName);
            var userName         = GetDbUserName(appConfig, dbVars.UserVarName);
            var host             = GetDbHost(appConfig, dbVars.HostVarName);
            var port             = GetDbPort(appConfig, dbVars.PortVarName);

            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrWhiteSpace(host))
                builder.Host = host;
            if (port > 0)
                builder.Port = port.Value;
            if (!string.IsNullOrWhiteSpace(userName))
                builder.Username = userName;
            if (!string.IsNullOrWhiteSpace(password))
                builder.Password = password;

            if (string.IsNullOrWhiteSpace(migrationAssembly))
                options.UseNpgsql(builder.ConnectionString);
            else
                options.UseNpgsql(builder.ConnectionString, npgsqlOptionsAction => npgsqlOptionsAction.MigrationsAssembly(migrationAssembly));
        });
        return services;
    }

    private static int? GetDbPort(IConfiguration appConfig, string portVarName) {
        // port can be set from environment variable or connection string
        int? port = null;
        if (string.IsNullOrWhiteSpace(portVarName))
            return null;
        var portStr = appConfig[portVarName];
        if (string.IsNullOrWhiteSpace(portStr) && portVarName != DEFAULT_DB_PORT_VAR_NAME)
            throw new ConfigurationException($"There is no db port in the environment variable '{portVarName}'");
        try {
            if (!string.IsNullOrWhiteSpace(portStr))
                port = int.Parse(portStr);
        }
        catch (Exception ex) {
            throw new ConfigurationException($"There is wrong value in the environment variable '{portVarName}'. Must be a positive integer", ex);
        }
        if (port <= 0)
            throw new ConfigurationException($"There is wrong value in the environment variable '{portVarName}'. Must be a positive integer");
        return port;
    }

    private static string GetDbHost(IConfiguration appConfig, string hostVarName) {
        // host can be set from environment variable or connection string
        if (string.IsNullOrWhiteSpace(hostVarName))
            return null;
        var host = appConfig[hostVarName];
        if (string.IsNullOrWhiteSpace(host) && hostVarName != DEFAULT_DB_HOST_VAR_NAME)
            throw new ConfigurationException($"There is no db host in the environment variable '{hostVarName}'");
        return host;
    }

    private static string GetDbUserName(IConfiguration appConfig, string userVarName) {
        // username can be set from environment variable or connection string
        if (string.IsNullOrWhiteSpace(userVarName))
            return "";
        var userName = appConfig[userVarName];
        if (string.IsNullOrWhiteSpace(userName) && userVarName != DEFAULT_DB_USER_VAR_NAME)
            throw new ConfigurationException($"There is no db user name in the environment variable '{userVarName}'");
        return userName;
    }

    private static string GetDbPassword(IConfiguration appConfig, string passwordVarName) {
        // password can be set from environment variable or connection string
        if (string.IsNullOrWhiteSpace(passwordVarName))
            return "";
        var password = appConfig[passwordVarName];
        if (string.IsNullOrWhiteSpace(password) && passwordVarName != DEFAULT_DB_PASSWORD_VAR_NAME)
            throw new ConfigurationException($"There is no db password in the environment variable '{passwordVarName}'");
        return password;
    }

    private static string GetConnectionStringValue(IConfiguration appConfig, string connectionVarName) {
        // name of the connection string can be set from environment variable or default name will be used
        var connectionStringName = DEFAULT_CONNECTION_STRING_NAME;
        if (!string.IsNullOrWhiteSpace(connectionVarName))
            connectionStringName = appConfig[connectionVarName];
        if (string.IsNullOrWhiteSpace(connectionStringName)) {
            if (connectionVarName != DEFAULT_CONNECTION_STRING_VAR_NAME)
                throw new ConfigurationException($"There is no connection string name in the environment variable {connectionVarName}");
            connectionStringName = DEFAULT_CONNECTION_STRING_NAME;
        }
        var connectionStringValue = appConfig.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionStringValue))
            throw new MissingConnectionStringException(connectionStringName);
        return connectionStringValue;
    }
}