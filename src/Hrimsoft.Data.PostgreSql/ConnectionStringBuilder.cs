using System;
using Hrimsoft.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Hrimsoft.Data.PostgreSql;

/// <summary> </summary>
public static class ConnectionStringBuilder
{
    public const string DEFAULT_CONNECTION_STRING_NAME     = "db";
    public const string DEFAULT_CONNECTION_STRING_VAR_NAME = "DB";
    public const string DEFAULT_DB_HOST_VAR_NAME           = "DB_HOST";
    public const string DEFAULT_DB_PORT_VAR_NAME           = "DB_PORT";
    public const string DEFAULT_DB_NAME_VAR_NAME           = "DB_NAME";
    public const string DEFAULT_DB_USER_VAR_NAME           = "DB_USER";
    public const string DEFAULT_DB_PASSWORD_VAR_NAME       = "DB_PWD";
    public const string DEFAULT_HISTORY_TABLE_VAR_NAME     = "DB_HISTORY_TABLE";
    public const string DEFAULT_HISTORY_SCHEMA_VAR_NAME    = "DB_HISTORY_SCHEMA";

    /// <summary>
    /// Builds a connection string and history table name and schema from environment variables
    /// </summary>
    public static (string ConnectionString, string HistoryTable, string HistorySchema) Get(IConfiguration appConfig, DbConfigEnvVariables dbVars = null) {
        if (appConfig == null)
            throw new ArgumentNullException(nameof(appConfig));

        if (dbVars == null) {
            dbVars = new DbConfigEnvVariables(DEFAULT_CONNECTION_STRING_VAR_NAME,
                                              DEFAULT_DB_HOST_VAR_NAME,
                                              DEFAULT_DB_PORT_VAR_NAME,
                                              DEFAULT_DB_NAME_VAR_NAME,
                                              DEFAULT_DB_USER_VAR_NAME,
                                              DEFAULT_DB_PASSWORD_VAR_NAME,
                                              DEFAULT_HISTORY_TABLE_VAR_NAME,
                                              DEFAULT_HISTORY_SCHEMA_VAR_NAME);
        }

        var connectionString = GetConnectionStringValue(appConfig, dbVars.ConnectionVarName);
        var password         = GetDbPassword(appConfig, dbVars.PasswordVarName);
        var userName         = GetDbUserName(appConfig, dbVars.UserVarName);
        var host             = GetDbHost(appConfig, dbVars.HostVarName);
        var port             = GetDbPort(appConfig, dbVars.PortVarName);
        var databaseName     = GetDatabase(appConfig, dbVars.DatabaseVarName);
        var builder          = new NpgsqlConnectionStringBuilder(connectionString);
        if (!string.IsNullOrWhiteSpace(host))
            builder.Host = host;
        if (port > 0)
            builder.Port = port.Value;
        if (!string.IsNullOrWhiteSpace(databaseName))
            builder.Database = databaseName;
        if (!string.IsNullOrWhiteSpace(userName))
            builder.Username = userName;
        if (!string.IsNullOrWhiteSpace(password))
            builder.Password = password;

        var historyTable  = GetHistoryTable(appConfig, dbVars.MigrationHistoryTableVarName);
        var historySchema = GetHistorySchema(appConfig, dbVars.MigrationHistorySchemaVarName);

        return (builder.ConnectionString, historyTable, historySchema);
    }

    private static string GetHistorySchema(IConfiguration appConfig, string historySchemaVarName) {
        // database can be set from environment variable or connection string
        if (string.IsNullOrWhiteSpace(historySchemaVarName))
            return null;
        var schema = appConfig[historySchemaVarName];
        if (string.IsNullOrWhiteSpace(schema) && historySchemaVarName != ConnectionStringBuilder.DEFAULT_HISTORY_SCHEMA_VAR_NAME)
            schema = "public";
        return schema;
    }

    private static string GetHistoryTable(IConfiguration appConfig, string historyTableVarName) {
        // database can be set from environment variable or connection string
        if (string.IsNullOrWhiteSpace(historyTableVarName))
            return null;
        return appConfig[historyTableVarName];
    }

    private static string GetDatabase(IConfiguration appConfig, string databaseVarName) {
        // database can be set from environment variable or connection string
        if (string.IsNullOrWhiteSpace(databaseVarName))
            return null;
        var database = appConfig[databaseVarName];
        if (string.IsNullOrWhiteSpace(database) && databaseVarName != DEFAULT_DB_NAME_VAR_NAME)
            throw new ConfigurationException($"There is no db name in the environment variable '{databaseVarName}'");
        return database;
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