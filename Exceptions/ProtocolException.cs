using System;

namespace VpnProtocol.Exceptions
{
    public class ProtocolException : Exception
    {
        public ProtocolException() { }
        public ProtocolException(string message) : base(message) { }
        public ProtocolException(string message, Exception inner) : base(message, inner) { }
    }
}