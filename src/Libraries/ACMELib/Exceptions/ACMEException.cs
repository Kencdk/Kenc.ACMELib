namespace Kenc.ACMELib.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class for all exceptions from the ACME protocol.
    /// </summary>
    public class ACMEException : Exception
    {
        public int Status
        {
            get;
            private set;
        }

        public string Descriptor
        {
            get;
            private set;
        }

        private ACMEException()
        {
        }

        private ACMEException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ACMEException(int status, string detail, string descriptor = default) : base(detail)
        {
            Status = status;
            if (string.IsNullOrEmpty(descriptor))
            {
                Type type = GetType();
                var acmeAttributes = type.GetCustomAttributes(typeof(ACMEExceptionAttribute), true);
                if (acmeAttributes != null && acmeAttributes.Length > 0)
                {
                    Descriptor = ((ACMEExceptionAttribute)acmeAttributes[0]).Descriptor;
                }
            }
            else
            {
                Descriptor = descriptor;
            }
        }
    }
}