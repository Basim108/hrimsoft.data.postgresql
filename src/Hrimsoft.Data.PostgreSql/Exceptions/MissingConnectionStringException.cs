using System;
using System.Runtime.Serialization;
using Hrimsoft.Core.Exceptions;

namespace Hrimsoft.Data.PostgreSql
{
    /// <summary>
    /// Any error related to missed connection string configuration
    /// </summary>
    [Serializable]
    public class MissingConnectionStringException: ConfigurationException
    {
        private const string DEFAULT_SECTION_NAME = "ConnectionStrings";
        private const string DEFAULT_KEY_NAME = "DefaultConnection";

        /// <summary>Create an exception for missing default connection string</summary>
        public MissingConnectionStringException()
            : base("Missing default connection string")
        {
            this.ConfigurationSection = DEFAULT_SECTION_NAME;
            this.ConfigurationKey = DEFAULT_KEY_NAME;
        }

        /// <summary>Create an exception with custom error message</summary>
        public MissingConnectionStringException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.ConfigurationSection = DEFAULT_SECTION_NAME;
            this.ConfigurationKey = DEFAULT_KEY_NAME;
        }

        /// <summary>Create an exception for customly named connection string</summary>
        /// <param name="configurationName">Name of the missing connection string</param>
        public MissingConnectionStringException(string configurationName)
            : base($"Missing connection string with name: '{configurationName ?? "undefined"}'")
        {
            this.ConfigurationSection = DEFAULT_SECTION_NAME;
            this.ConfigurationKey = configurationName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.ApplicationException"></see> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected MissingConnectionStringException(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            this.ConfigurationSection = DEFAULT_SECTION_NAME;
            this.ConfigurationKey = info.GetString(nameof(this.Key));

            if(string.IsNullOrWhiteSpace(this.ConfigurationKey))
                this.ConfigurationKey = DEFAULT_KEY_NAME;
        }
    }

}
