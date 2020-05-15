namespace Kenc.ACMELib.Exceptions
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ACMEExceptionAttribute : Attribute
    {
        public string Descriptor
        {
            get;
            set;
        }

        public ACMEExceptionAttribute(string descriptor)
        {
            Descriptor = descriptor;
        }
    }
}