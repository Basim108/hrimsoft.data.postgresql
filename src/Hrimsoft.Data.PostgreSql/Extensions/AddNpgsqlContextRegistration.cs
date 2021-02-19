using System;
using System.Data;
using Hrimsoft.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Hrimsoft.Data.PostgreSql
{
    /// <summary> </summary>
    public static class AddNpgsqlContextRegistration
    {
        private const string DEFAULT_CONNECTION_STRING_NAME     = "db";
        private const string DEFAULT_CONNECTION_STRING_VAR_NAME = "DB";
        private const string DEFAULT_DB_HOST_VAR_NAME           = "DB_HOST";
        private const string DEFAULT_DB_PORT_VAR_NAME           = "DB_PORT";
        private const string DEFAULT_DB_USER_VAR_NAME           = "DB_USER";
        private const string DEFAULT_DB_PASSWORD_VAR_NAME       = "DB_PWD";

        /// <summary>Registers a DbContext. Takes values from connection string and/or environment variables</summary>
        /// <param name="appConfig"></param>
        /// <param name="migrationAssembly">Assembly name where migrations are located</param>
        /// <param name="connectionVarName">Name of an environment variable that holds a connection string name</param>
        /// <param name="hostVarName">Name of an environment variable that holds a db host</param>
        /// <param name="portVarName">Name of an environment variable that holds a db port</param>
        /// <param name="userVarName">Name of an environment variable that holds a db user name</param>
        /// <param name="passwordVarName">Name of an environment variable that holds a db user password</param>
        /// <param name="services"></param>
        /// <exception cref="ArgumentNullException">Application configuration is required</exception>
        /// <exception cref="ConfigurationException">Throws when connection string is empty or there is no password db environment variable</exception>
        public static IServiceCollection AddNpgsqlContext<TContext>(
            this IServiceCollection services,
            IConfiguration          appConfig,
            string                  migrationAssembly = null,
            string                  connectionVarName = DEFAULT_CONNECTION_STRING_VAR_NAME,
            string                  hostVarName       = DEFAULT_DB_HOST_VAR_NAME,
            string                  portVarName       = DEFAULT_DB_PORT_VAR_NAME,
            string                  userVarName       = DEFAULT_DB_USER_VAR_NAME,
            string                  passwordVarName   = DEFAULT_DB_PASSWORD_VAR_NAME)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(options =>
            {
                if (appConfig == null)
                    throw new ArgumentNullException(nameof(appConfig));
                
                // name of the connection string can be set from environment variable or default name will be used
                var connectionStringName = DEFAULT_CONNECTION_STRING_NAME;
                if (!string.IsNullOrWhiteSpace(connectionVarName))
                    connectionStringName = appConfig[connectionVarName];
                if (string.IsNullOrWhiteSpace(connectionStringName))
                {
                    if (connectionVarName != DEFAULT_CONNECTION_STRING_VAR_NAME)
                        throw new ConfigurationException($"There is no connection string name in the environment variable {connectionVarName}");
                    connectionStringName = DEFAULT_CONNECTION_STRING_NAME;
                }
                var connectionStringValue = appConfig.GetConnectionString(connectionStringName);
                if (string.IsNullOrWhiteSpace(connectionStringValue))
                    throw new MissingConnectionStringException(connectionStringName);
                
                // password can be set from environment variable or connection string
                var password = "";
                if (!string.IsNullOrWhiteSpace(passwordVarName))
                {
                    password = appConfig[passwordVarName];
                    if (string.IsNullOrWhiteSpace(password) && passwordVarName != DEFAULT_DB_PASSWORD_VAR_NAME)
                        throw new ConfigurationException($"There is no db password in the environment variable '{passwordVarName}'");
                }
                
                // username can be set from environment variable or connection string
                var userName = "";
                if (!string.IsNullOrWhiteSpace(userVarName))
                {
                    userName = appConfig[userVarName];
                    if (string.IsNullOrWhiteSpace(userName) && userVarName != DEFAULT_DB_USER_VAR_NAME)
                        throw new ConfigurationException($"There is no db user name in the environment variable '{userVarName}'");
                }
                
                // host can be set from environment variable or connection string
                string host = null;
                if (!string.IsNullOrWhiteSpace(hostVarName))
                {
                    host = appConfig[hostVarName];
                    if (string.IsNullOrWhiteSpace(host) && hostVarName != DEFAULT_DB_HOST_VAR_NAME)
                        throw new ConfigurationException($"There is no db host in the environment variable '{hostVarName}'");
                }
                
                // port can be set from environment variable or connection string
                int? port = null;
                if (!string.IsNullOrWhiteSpace(portVarName))
                {
                    var portStr = appConfig[portVarName];
                    if (string.IsNullOrWhiteSpace(portStr) && portVarName != DEFAULT_DB_PORT_VAR_NAME)
                        throw new ConfigurationException($"There is no db port in the environment variable '{portVarName}'");
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(portStr))
                            port = int.Parse(portStr);
                    }
                    catch (Exception ex)
                    {
                        throw new ConfigurationException($"There is wrong value in the environment variable '{portVarName}'. Must be a positive integer", ex);
                    }
                    if (port <= 0)
                        throw new ConfigurationException($"There is wrong value in the environment variable '{portVarName}'. Must be a positive integer");
                }
                var builder = new NpgsqlConnectionStringBuilder(connectionStringValue);
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
    }
}