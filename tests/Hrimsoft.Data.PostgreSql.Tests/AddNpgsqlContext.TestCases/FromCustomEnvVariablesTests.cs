using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Hrimsoft.Data.PostgreSql.Tests
{
    public class FromCustomEnvVariablesTests
    {
        private Mock<IConfiguration> _configuration;
        private DbConfigEnvVariables _dbVars;
        
        private const string CONNECTION_STRING =
            "Host=192.168.122.195;Port=5430;Database=test_db;Username=test_user;Password=12345;Pooling=True;CommandTimeout=300;Application Name=Hrimsoft.Data.Tests;";

        [SetUp]
        public void Setup()
        {
            _dbVars = new DbConfigEnvVariables(AddNpgsqlContextRegistration.DEFAULT_CONNECTION_STRING_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_DB_HOST_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_DB_PORT_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_DB_USER_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_DB_PASSWORD_VAR_NAME);
            _configuration = new Mock<IConfiguration>();
            var mockConfSection = new Mock<IConfigurationSection>();
            mockConfSection.Setup(a => a[It.Is<string>(s => s == "db")])
                           .Returns(CONNECTION_STRING);
            _configuration.Setup(x => x.GetSection("ConnectionStrings"))
                          .Returns(mockConfSection.Object);
        }

        [Test]
        public void Should_use_host_from_custom_environment()
        {
            _configuration.SetupGet(x => x["CUSTOM_DB_HOST"]).Returns("localhost");

            var services = new ServiceCollection()
               .AddNpgsqlContext<DbContext>(_configuration.Object, 
                                            null,
                                            _dbVars with { HostVarName = "CUSTOM_DB_HOST" });
            var sp       = services.BuildServiceProvider();
            var resolved = sp.GetService<DbContext>();
            Assert.NotNull(resolved);
            var connection = resolved.Database.GetDbConnection();
            Assert.NotNull(connection);
            Assert.IsTrue(connection.ConnectionString.Contains("Host=localhost"));
        }
        
        [Test]
        public void Should_use_port_from_custom_environment()
        {
            _configuration.SetupGet(x => x["CUSTOM_DB_PORT"]).Returns("123");

            var services = new ServiceCollection()
               .AddNpgsqlContext<DbContext>(_configuration.Object, 
                                            null,
                                            _dbVars with { PortVarName = "CUSTOM_DB_PORT" });
            var sp       = services.BuildServiceProvider();
            var resolved = sp.GetService<DbContext>();
            Assert.NotNull(resolved);
            var connection = resolved.Database.GetDbConnection();
            Assert.NotNull(connection);
            Assert.IsTrue(connection.ConnectionString.Contains("Port=123"));
        }

        [Test]
        public void Should_use_user_name_from_custom_environment()
        {
            _configuration.SetupGet(x => x["CUSTOM_DB_USER"]).Returns("custom_user");

            var services = new ServiceCollection()
               .AddNpgsqlContext<DbContext>(_configuration.Object, 
                                            null,
                                            _dbVars with { UserVarName = "CUSTOM_DB_USER" });
            var sp       = services.BuildServiceProvider();
            var resolved = sp.GetService<DbContext>();
            Assert.NotNull(resolved);
            var connection = resolved.Database.GetDbConnection();
            Assert.NotNull(connection);
            Assert.IsTrue(connection.ConnectionString.Contains("Username=custom_user"));
            Assert.IsFalse(connection.ConnectionString.Contains("Username=test_user"));
        }
        
        [Test]
        public void Should_use_password_from_custom_environment()
        {
            _configuration.SetupGet(x => x["CUSTOM_DB_PWD"]).Returns("user_secret_password");

            var services = new ServiceCollection()
               .AddNpgsqlContext<DbContext>(_configuration.Object, null,
                                            _dbVars with { PasswordVarName = "CUSTOM_DB_PWD" });
            var sp       = services.BuildServiceProvider();
            var resolved = sp.GetService<DbContext>();
            Assert.NotNull(resolved);
            var connection = resolved.Database.GetDbConnection();
            Assert.NotNull(connection);
            Assert.IsTrue(connection.ConnectionString.Contains("Password=user_secret_password"));
            Assert.IsFalse(connection.ConnectionString.Contains("Password=12345"));
        }
    }
}