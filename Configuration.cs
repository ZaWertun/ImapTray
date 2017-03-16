using System;
using System.Linq;
using System.Runtime.Serialization;

namespace ImapTray
{
    [DataContract]
    class Configuration
    {
        [DataMember(Name = "accounts")]
        private Account[] _accounts;

        public Account[] Accounts
        {
            get { return _accounts; }
        }

        [DataMember(Name = "emailClientPath")]
        private string _emailClientPath;

        public string EmailClientPath
        {
            get { return _emailClientPath; }
        }

        public Configuration(Account[] accounts)
        {
            _accounts = accounts;
        }

        public Configuration AddAccount(Account account)
        {
            Array.Resize(ref _accounts, _accounts.Length + 1);
            _accounts[_accounts.Length - 1] = account;
            return this;
        }

        public Configuration SetAccount(int at, Account account)
        {
            _accounts[at] = account;
            return this;
        }

        public Configuration RemoveAccount(int at)
        {
            _accounts = _accounts.Where((_, index) => index != at).ToArray();
            return this;
        }

        public Configuration SetEmailClientPath(string path)
        {
            _emailClientPath = path;
            return this;
        }
    }
}
