using System;
using System.Runtime.Serialization;

namespace Hrimsoft.Data.PostgreSql
{
    /// <summary>
    /// Error of a data access layer
    /// </summary>
    [Serializable]
    public class DataModelException : Exception
    {
        /// <inheritdoc />
        public DataModelException() { }

        /// <inheritdoc />
        public DataModelException(string message) : base(message) { }

        /// <inheritdoc />
        public DataModelException(string message, Exception innerException) : base(message, innerException) { }

        /// <inheritdoc />
        protected DataModelException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}