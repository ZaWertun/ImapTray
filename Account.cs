using System;
using System.Runtime.Serialization;

namespace ImapTray
{
    [DataContract]
    public class Account
    {
        [DataMember]
        public readonly string username;

        [DataMember]
        public readonly string password;

        [DataMember]
        public readonly string server;

        [DataMember]
        public readonly UInt16 port;

        [DataMember]
        public readonly bool ssl;

        public Account(string username, string password, string server, ushort port, bool ssl)
        {
            this.username = username;
            this.password = password;
            this.server = server;
            this.port = port;
            this.ssl = ssl;
        }
    }
}
