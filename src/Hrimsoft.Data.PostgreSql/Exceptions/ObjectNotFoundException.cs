using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;

namespace Hrimsoft.Data.PostgreSql
{
    /// <summary>
    /// Object not found in a set
    /// </summary>
    [Serializable]
    public class ObjectNotFoundException<TKey> : DataModelException
    {
        /// <summary> Object Id </summary>
        public TKey Id { get; }
        
        /// <summary>
        /// Name of a table where we tried to find an object 
        /// </summary>
        public string SetName { get; }
        
        /// <summary>
        /// Key value collection of properties and values that we tried to find an object with
        /// Key: Property Name,
        /// Value: Property Value
        /// </summary>
        public NameValueCollection SearchProperties { get; }

        /// <inheritdoc />
        public ObjectNotFoundException()
        {
        }

        /// <inheritdoc />
        public ObjectNotFoundException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public ObjectNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary> </summary>
        public ObjectNotFoundException(string customMessage, string setName, TKey id)
            : base(customMessage)
        {
            this.Id = id;
            this.SetName = setName;
        }

        /// <summary> </summary>
        public ObjectNotFoundException(string setName, TKey id)
            : base($"Object with id '{id}' hasn't found in '{setName}' set.")
        {
            this.Id = id;
            this.SetName = setName;
        }

        /// <summary> </summary>
        public ObjectNotFoundException(string setName, NameValueCollection searchProperties)
            : base(GenerateErrorMessage(setName, searchProperties))
        {
            this.SetName = setName;
            this.SearchProperties = searchProperties;
        }

        /// <summary> Deserialization an instance of an exception</summary>
        protected ObjectNotFoundException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            this.Id = (TKey) info.GetValue(nameof(Id), typeof(TKey));
            this.SetName = info.GetString(nameof(SetName));
            this.SearchProperties = info.GetValue(nameof(SearchProperties), typeof(NameValueCollection)) as NameValueCollection;
        }

        /// <summary> Serialization an instance of an exception </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);

            info.AddValue(nameof(Id), this.Id);
            info.AddValue(nameof(SetName), this.SetName);
            info.AddValue(nameof(SearchProperties), this.SearchProperties);
        }
        
        
        /// <summary> Error message generator </summary>
        private static string GenerateErrorMessage(string setName, NameValueCollection searchProperties)
        {
            if (searchProperties == null)
                throw new ArgumentNullException(nameof(searchProperties));

            StringBuilder resultMessage = new StringBuilder($"Can't find object in '{setName ?? ""}' , by properties:");
            if (searchProperties.Count > 0)
                foreach (string propertyName in searchProperties.Keys)
                {
                    var separator = resultMessage.Length == 0 ? "" : ", ";
                    resultMessage.AppendFormat($"{separator} '{propertyName}'= '{searchProperties[propertyName]}'");
                }
            else
                resultMessage.Append("collection of properties is empty");

            return resultMessage.ToString();
        }
    }
}