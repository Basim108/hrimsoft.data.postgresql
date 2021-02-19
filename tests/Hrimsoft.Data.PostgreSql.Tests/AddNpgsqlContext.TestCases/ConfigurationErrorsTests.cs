using System;
using Hrimsoft.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Hrimsoft.Data.PostgreSql.Tests
{
    public class ConfigurationErrorsTests
    {
        private Mock<IConfiguration>        _configuration;
        private Mock<IConfigurationSection> _configSection;

        private const string DEFAULT_CONNECTION_STRING =
            "Host=192.168.122.195;Port=5430;Database=test_db;Username=test_user;Password=12345;Pooling=True;CommandTimeout=300;Application Name=Hrimsoft.Data.Tests;";

        [SetUp]
        public void Setup()
        {
            _configuration = new Mock<IConfiguration>();
            _configSection = new Mock<IConfigurationSection>();
            _configuration.Setup(x => x.GetSection("ConnectionStrings"))
                          .Returns(_configSection.Object);
        }

        [Test]
        public void Error_when_there_is_no_default_connection_string()
        {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object);
            var sp       = services.BuildServiceProvider();
            var ex       = Assert.Catch<MissingConnectionStringException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("Missing connection string with name: 'db'", ex.Message);
        }

        [Test]
        public void Error_when_there_is_no_custom_connection_string()
        {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(DEFAULT_CONNECTION_STRING);
            _configSection.Setup(a => a[It.Is<string>(s => s == "custom_db")])
                          .Returns("");
            _configuration.SetupGet(x => x["DB"]).Returns("custom_db");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object);
            var sp       = services.BuildServiceProvider();
            var ex       = Assert.Catch<MissingConnectionStringException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("Missing connection string with name: 'custom_db'", ex.Message);
        }

        [Test]
        public void Error_when_custom_host_var_is_empty()
        {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(DEFAULT_CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_HOST"]).Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               hostVarName: "CUSTOM_HOST");
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is no db host in the environment variable 'CUSTOM_HOST'", ex.Message);
        }

        [Test]
        public void Error_when_custom_port_var_is_empty()
        {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(DEFAULT_CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_PORT"]).Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               portVarName: "CUSTOM_PORT");
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is no db port in the environment variable 'CUSTOM_PORT'", ex.Message);
        }

        [Test]
        public void Error_when_custom_port_var_has_none_int_value()
        {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(DEFAULT_CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_PORT"]).Returns("qwerty");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               portVarName: "CUSTOM_PORT");
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is wrong value in the environment variable 'CUSTOM_PORT'. Must be a positive integer", ex.Message);
            Assert.NotNull(ex.InnerException);
            Assert.IsInstanceOf<FormatException>(ex.InnerException);
        }

        [Test]
        public void Error_when_custom_port_var_has_negative_value()
        {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(DEFAULT_CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_PORT"]).Returns("-10");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               portVarName: "CUSTOM_PORT");
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is wrong value in the environment variable 'CUSTOM_PORT'. Must be a positive integer", ex.Message);
            Assert.Null(ex.InnerException);
        }

        [Test]
        public void Error_when_custom_user_var_is_empty()
        {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(DEFAULT_CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_USER"]).Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               userVarName: "CUSTOM_USER");
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is no db user name in the environment variable 'CUSTOM_USER'", ex.Message);
        }

        [Test]
        public void Error_when_custom_password_var_is_empty()
        {
            _configSection.Setup(a => a[It.Is<string>(s => s == "db")])
                          .Returns(DEFAULT_CONNECTION_STRING);
            _configuration.SetupGet(x => x["CUSTOM_PWD"]).Returns("");
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(_configuration.Object,
                                                                               passwordVarName: "CUSTOM_PWD");
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ConfigurationException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("There is no db password in the environment variable 'CUSTOM_PWD'", ex.Message);
        }
        
        [Test]
        public void Error_when_app_config_is_null()
        {
            var services = new ServiceCollection().AddNpgsqlContext<DbContext>(null);
            var sp = services.BuildServiceProvider();
            var ex = Assert.Catch<ArgumentNullException>(() => sp.GetService<DbContext>());
            Assert.AreEqual("appConfig", ex.ParamName);
        }
    }
}