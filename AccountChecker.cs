using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using S22.Imap;

namespace ImapTray
{
    class AccountChecker : IDisposable
    {
        private class ClientWithAccount
        {
            public readonly ImapClient Client;
            public readonly Account Account;
            public int Unread = 0;

            public ClientWithAccount(ImapClient client, Account account)
            {
                Client = client;
                Account = account;
            }
        }

        private Thread _workerThread = null;
        private static readonly AutoResetEvent CheckNowEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _stopEvent = new AutoResetEvent(false);
        private readonly List<ClientWithAccount> _clients = new List<ClientWithAccount>();

        public event OnNewMessage onNewMessage;
        public delegate void OnNewMessage(string username, string subject, string from);

        public event OnUnreadChanged onUnreadChanged;
        public delegate void OnUnreadChanged(Account account, int unread);

        public void Start(Configuration configuration)
        {
            if (_workerThread != null)
            {
                return;
            }

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

        public static void CheckNow()
        {
            CheckNowEvent.Set();
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
                    _clients.Add(new ClientWithAccount(client, acc));
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

            Stopwatch stopwatch = new Stopwatch(); // FIXME: remove
            while (true)
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                    Debug.WriteLine("Stopwatch: " + stopwatch.Elapsed.Seconds);
                    stopwatch.Reset();
                }

                if (!stopwatch.IsRunning)
                {
                    stopwatch.Start();
                }

                foreach (var el in _clients)
                {
                    var info = el.Client.GetMailboxInfo();
                    int unread = el.Unread;
                    el.Unread = info.Unread;
                    if (unread != el.Unread)
                    {
                        onUnreadChanged(el.Account, unread);
                    }
                }

                if (CheckNowEvent.WaitOne(TimeSpan.FromMilliseconds(60 * 1000)))
                {
                    CheckNowEvent.Reset();
                    continue;
                }

                if (_stopEvent.WaitOne(0))
                {
                    break;
                }
            }

            foreach (var el in _clients)
            {
                el.Client.NewMessage -= ClientOnNewMessage;
                el.Client.IdleError -= ClientOnIdleError;
                el.Client.Logout();
                el.Client.Dispose();
            }
        }

        private void ClientOnNewMessage(object sender, IdleMessageEventArgs e)
        {
            string from;
            string subject;

            var client = (ImapClient) sender;
            var el = _clients.Find(x => x.Client == client);
            if (el != null)
            {
                using (var message = e.Client.GetMessage(e.MessageUID, FetchOptions.HeadersOnly))
                {
                    from = message.From.Address;
                    subject = message.Subject;
                    // keep email unread:
                    client.RemoveMessageFlags(e.MessageUID, null, MessageFlag.Seen);
                }
                onNewMessage(el.Account.username, subject, from);

                el.Unread += 1;
                onUnreadChanged(el.Account, el.Unread);
            }
        }

        private void ClientOnIdleError(object sender, IdleErrorEventArgs e)
        {
            var client = (ImapClient) sender;
            var el = _clients.Find(x => x.Client == client);
            if (el != null)
            {
                Log.Error("Account `{0}`: IDLE error - {1}", el.Account.username, e.Exception.Message);
            }
        }

        public void Dispose()
        {
            ((IDisposable) _stopEvent).Dispose();
        }
    }
}
