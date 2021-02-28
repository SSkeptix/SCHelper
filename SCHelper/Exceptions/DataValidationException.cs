using System;
using System.Runtime.Serialization;

namespace SCHelper.Exceptions
{
    public class DataValidationException : Exception
    {
        public DataValidationException()
        {
        }

        public DataValidationException(string message)
            : base(message)
        {
        }

        public DataValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DataValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
