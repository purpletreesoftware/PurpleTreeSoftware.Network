using System;
using System.Runtime.Serialization;


namespace PurpleTreeSoftware.Network.Common
{
    [DataContract]
    public class Message
    {
        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public string Content { get; set; }

    }
}
