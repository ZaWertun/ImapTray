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

        public void RemoveAccount(int at)
        {
            _accounts = _accounts.Where((_, index) => index != at).ToArray();
        }
    }
}
