using System;
using System.Collections.Generic;
using System.Threading;
using S22.Imap;

namespace ImapTray
{
    class AccountChecker : IDisposable
    {
        private long _unread;
        private Thread _workerThread = null;
        private readonly AutoResetEvent _stopEvent = new AutoResetEvent(false);
        private readonly Dictionary<ImapClient, Account> _clients = new Dictionary<ImapClient, Account>();

        public event OnNewMessage onNewMessage;
        public delegate void OnNewMessage(string username, string subject, string from);

        public event OnUnreadChanged onUnreadChanged;
        public delegate void OnUnreadChanged(long unread);

        public void Start(Configuration configuration)
        {
            if (_workerThread != null)
            {
                return;
            }

            _unread = 0;

            _workerThread = new Thread(Work);
            _workerThread.Start(configuration);
        }

        public void Stop()
        {
            _stopEvent.Set();
            _workerThread.Join();
            _workerThread = null;
            _clients.Clear();
        }

        private void Work(object data)
        {
            var configuration = (Configuration) data;
            Array.ForEach(configuration.Accounts, delegate(Account acc)
            {
                try
                {
                    var client = new ImapClient(acc.server, acc.port, acc.username, acc.password, AuthMethod.Login, acc.ssl);
                    if (client.Supports("IDLE"))
                    {
                        client.NewMessage += ClientOnNewMessage;
                        client.IdleError += ClientOnIdleError;
                    }
                    _clients.Add(client, acc);
                }
                catch (InvalidCredentialsException)
                {
                    Log.Warn("Account `{0}`: invalid credentials", acc.username);
                }
                catch (Exception ex)
                {
                    Log.Error("Account `{0}`: {1}", acc.username, ex.Message);
                }
            });

            bool once = true;
            while (true)
            {
                long unread = 0;
                foreach (var client in _clients.Keys)
                {
                    var info = client.GetMailboxInfo();
                    unread += info.Unread;
                }

                if (once || _unread != unread)
                {
                    onUnreadChanged(unread);
                    _unread = unread;
                    if (once)
                    {
                        once = false;
                    }
                }

                if (_stopEvent.WaitOne(TimeSpan.FromMilliseconds(60 * 1000)))
                {
                    break;
                }
            }

            foreach (var client in _clients.Keys)
            {
                client.NewMessage -= ClientOnNewMessage;
                client.IdleError -= ClientOnIdleError;
                client.Logout();
                client.Dispose();
            }
        }

        private void ClientOnNewMessage(object sender, IdleMessageEventArgs e)
        {
            string from;
            string subject;

            var client = (ImapClient) sender;
            var account = _clients[client];
            using (var message = e.Client.GetMessage(e.MessageUID, FetchOptions.HeadersOnly))
            {
                from = message.From.Address;
                subject = message.Subject;
                // keep email unread:
                client.RemoveMessageFlags(e.MessageUID, null, MessageFlag.Seen);
            }
            onNewMessage(account.username, subject, from);

            _unread += 1;
            onUnreadChanged(_unread);
        }

        private void ClientOnIdleError(object sender, IdleErrorEventArgs e)
        {
            var client = (ImapClient) sender;
            var account = _clients[client];
            Log.Error("Account `{0}`: IDLE error - {1}", account.username, e.Exception.Message);
        }

        public void Dispose()
        {
            ((IDisposable) _stopEvent).Dispose();
        }
    }
}
