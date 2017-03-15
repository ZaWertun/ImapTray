using System;
using System.Runtime.Serialization;

namespace ImapTray
{
    [DataContract]
    class Credential
    {
        [DataMember]
        public readonly string login;

        [DataMember]
        public readonly string password;

        [DataMember]
        public readonly string server;

        [DataMember]
        public readonly UInt16 port;

        [DataMember]
        public readonly bool ssl;

        public Credential(string login, string password, string server, ushort port, bool ssl)
        {
            this.login = login;
            this.password = password;
            this.server = server;
            this.port = port;
            this.ssl = ssl;
        }
    }
}
