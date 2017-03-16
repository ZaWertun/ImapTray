using System;
using System.Collections.Generic;
using System.Threading;
using S22.Imap;

namespace ImapTray
{
    class AccountChecker : IDisposable
    {
        private Thread _workerThread = null;
        private readonly AutoResetEvent _stopEvent = new AutoResetEvent(false);

        public void Start(Configuration configuration)
        {
            _workerThread = new Thread(Work);
            _workerThread.Start(configuration);
        }

        public void Stop()
        {
            _stopEvent.Set();
            _workerThread.Join();
        }

        private void Work(object data)
        {
            var configuration = (Configuration) data;
            List<ImapClient> clients = new List<ImapClient>(configuration.Accounts.Length);
            Array.ForEach(configuration.Accounts, acc =>
            {
                try
                {
                    var client = new ImapClient(acc.server, acc.port, acc.username, acc.password, AuthMethod.Login, acc.ssl);
                    if (client.Supports("IDLE"))
                    {
                        client.NewMessage += ClientOnNewMessage;
                    }
                    clients.Add(client);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            });

            while (true)
            {
                if (_stopEvent.WaitOne(TimeSpan.FromMilliseconds(1000)))
                {
                    break;
                }
            }

            clients.ForEach(client =>
            {
                client.NewMessage -= ClientOnNewMessage;
                client.Logout();
                client.Dispose();
            });
        }

        private void ClientOnNewMessage(object o, IdleMessageEventArgs e)
        {
            string from;
            string subject;
            using (var message = e.Client.GetMessage(e.MessageUID, FetchOptions.HeadersOnly))
            {
                from = message.From.Address;
                subject = message.Subject;
            }
            if (from.Length > 0 && subject.Length > 0) { }
        }

        public void Dispose()
        {
            ((IDisposable) _stopEvent).Dispose();
        }
    }
}
