using System;
using Hrimsoft.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Hrimsoft.Data.PostgreSql.Tests {
    public class ConfigurationErrorsTests {
        private Mock<IConfiguration>        _configuration;
        private Mock<IConfigurationSection> _configSection;
        private DbConfigEnvVariables        _dbVars;

        private const string CONNECTION_STRING =
            "Host=192.168.122.195;Port=5430;Database=test_db;Username=test_user;Password=12345;Pooling=True;CommandTimeout=300;Application Name=Hrimsoft.Data.Tests;";

        [SetUp]
        public void Setup() {
            _dbVars = new DbConfigEnvVariables(AddNpgsqlContextRegistration.DEFAULT_CONNECTION_STRING_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_DB_HOST_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_DB_PORT_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_DB_NAME_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_DB_USER_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_DB_PASSWORD_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_HISTORY_TABLE_VAR_NAME,
                                               AddNpgsqlContextRegistration.DEFAULT_HISTORY_SCHEMA_VAR_NAME);
            _configuration = new Mock<IConfiguration>();
            _configSection = new Mock<IConfigurationSection>();
            _configuration.Setup(x => x.GetSection("ConnectionStrings"))
                          .Returns(_configSection.Object);
        }

        [Test]
        public void Error_when_there_is_no_default_connection_string() {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object);
            var sp       = services.BuildServiceProvider();
            var ex       = Assert.Catch<MissingConnectionStringException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("Missing connection string with name: 'db'", ex!.Message);
        }

        [Test]
        public void Error_when_there_is_no_custom_connection_string() {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(CONNECTION_STRING);
            _configSection.Setup(a => a[It.Is<string>(s => s == "custom_db")])
                          .Returns("");
            _configuration.SetupGet(x => x["DB"]).Returns("custom_db");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object);
            var sp       = services.BuildServiceProvider();
            var ex       = Assert.Catch<MissingConnectionStringException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("Missing connection string with name: 'custom_db'", ex!.Message);
        }

        [Test]
        public void Error_when_custom_host_var_is_empty() {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_HOST"]).Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               null,
                                                                               _dbVars with { HostVarName = "CUSTOM_HOST" });
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is no db host in the environment variable 'CUSTOM_HOST'", ex!.Message);
        }

        [Test]
        public void Error_when_custom_port_var_is_empty() {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_PORT"]).Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               null,
                                                                               _dbVars with { PortVarName = "CUSTOM_PORT" });
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is no db port in the environment variable 'CUSTOM_PORT'", ex!.Message);
        }

        [Test]
        public void Error_when_custom_port_var_has_none_int_value() {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_PORT"]).Returns("qwerty");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               null,
                                                                               _dbVars with { PortVarName = "CUSTOM_PORT" });
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is wrong value in the environment variable 'CUSTOM_PORT'. Must be a positive integer", ex!.Message);
            Assert.NotNull(ex.InnerException);
            Assert.IsInstanceOf<FormatException>(ex.InnerException);
        }

        [Test]
        public void Error_when_custom_port_var_has_negative_value() {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_PORT"]).Returns("-10");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               null,
                                                                               _dbVars with { PortVarName = "CUSTOM_PORT" });
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is wrong value in the environment variable 'CUSTOM_PORT'. Must be a positive integer", ex!.Message);
            Assert.Null(ex.InnerException);
        }

        [Test]
        public void Error_when_custom_user_var_is_empty() {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_USER"]).Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               null,
                                                                               _dbVars with { UserVarName = "CUSTOM_USER" });
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is no db user name in the environment variable 'CUSTOM_USER'", ex!.Message);
        }

        [Test]
        public void Error_when_custom_password_var_is_empty() {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_PWD"]).Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               null,
                                                                               _dbVars with { PasswordVarName = "CUSTOM_PWD" });
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is no db password in the environment variable 'CUSTOM_PWD'", ex!.Message);
        }
    }
}