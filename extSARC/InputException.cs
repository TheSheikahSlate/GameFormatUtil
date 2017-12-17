using System;
using System.Runtime.Serialization;

namespace extSARC
{
    [Serializable]
    public class InputException : Exception
    {
        public InputException(string message) : base(message) { }
        protected InputException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) { }
    }
}
