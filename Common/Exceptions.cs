using System;

namespace PurpleTreeSoftware.Network.Common
{
   
    public class ListenerAlreadyRunningException : Exception
    {
        public ListenerAlreadyRunningException()
        {
        }

        public ListenerAlreadyRunningException(string message)
            : base(message)
        {
        }

        public ListenerAlreadyRunningException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
