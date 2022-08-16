using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using NUnit.Framework;

namespace Hrimsoft.Data.PostgreSql.Tests {
    public class FromConnectionStringTests {
        private IConfiguration   _configuration;
        private IServiceProvider _serviceProvider;

        private const string DEFAULT_CONNECTION_STRING =
            "Host=192.168.122.195;Port=5430;Database=test_db;Username=test_user;Password=12345;Pooling=True;CommandTimeout=300;Application Name=Hrimsoft.Data.Tests;";

        [SetUp]
        public void Setup() {
            _configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(new Dictionary<string, string>() {
                                 { "ConnectionStrings:db", DEFAULT_CONNECTION_STRING }
                             }).Build();
            var services = new ServiceCollection()
               .AddNpgsqlContext<DbContext>(_configuration);
            _serviceProvider = services.BuildServiceProvider();
        }

        [Test]
        public void Should_use_host_from_connection_string() {
            var resolved = _serviceProvider.GetService<DbContext>();
            Assert.NotNull(resolved);
            var connection = resolved.Database.GetDbConnection() as NpgsqlConnection;
            Assert.NotNull(connection);
            Assert.IsNull(connection.Host);
            Assert.IsTrue(connection.ConnectionString.Contains("192.168.122.195"));
        }

        [Test]
        public void Should_use_port_from_connection_string() {
            var resolved = _serviceProvider.GetService<DbContext>();
            Assert.NotNull(resolved);
            var connection = resolved.Database.GetDbConnection() as NpgsqlConnection;
            Assert.NotNull(connection);
            Assert.AreEqual(0, connection.Port);
            Assert.IsTrue(connection.ConnectionString.Contains("=5430"));
        }

        [Test]
        public void Should_use_user_name_from_connection_string() {
            var resolved = _serviceProvider.GetService<DbContext>();
            Assert.NotNull(resolved);
            var connection = resolved.Database.GetDbConnection();
            Assert.NotNull(connection);
            Assert.IsTrue(connection.ConnectionString.Contains("Username=test_user"));
        }

        [Test]
        public void Should_use_password_from_connection_string() {
            var resolved = _serviceProvider.GetService<DbContext>();
            Assert.NotNull(resolved);
            var connection = resolved.Database.GetDbConnection();
            Assert.NotNull(connection);
            Assert.IsTrue(connection.ConnectionString.Contains("Password=12345"));
        }
    }
}