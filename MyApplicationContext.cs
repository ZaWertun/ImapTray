using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Windows.Forms;
using S22.Imap;

namespace ImapTray
{
    class MyApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon = new NotifyIcon();
        private readonly ConfigurationForm _configWindow = new ConfigurationForm();

        public MyApplicationContext()
        {
            _notifyIcon.Icon = Properties.Resources.AppIcon;
            _notifyIcon.DoubleClick += ShowConfig;
            _notifyIcon.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("Test", Test),
                new MenuItem("Configuration", ShowConfig),
                new MenuItem("-"),
                new MenuItem("Exit", Exit)
            });
            _notifyIcon.Visible = true;
        }

        private void Test(object sender, EventArgs e)
        {
            using (var client = new ImapClient("imap.gmail.com", 993, true))
            {
                client.Login("***@gmail.com", "***", AuthMethod.Login);

                IEnumerable<uint> uids = client.Search(SearchCondition.Unseen());
                IEnumerable<MailMessage> messages = client.GetMessages(uids);

                Debug.WriteLine("!!!");
            }
        }

        private void ShowConfig(object sender, EventArgs e)
        {
            if (_configWindow.Visible)
            {
                _configWindow.Activate();
            }
            else
            {
                _configWindow.ShowDialog();
            }
        }

        private void Exit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
