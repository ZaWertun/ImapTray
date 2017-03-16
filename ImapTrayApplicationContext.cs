using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ImapTray
{
    class ImapTrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon = new NotifyIcon();
        private readonly AccountChecker _checker = new AccountChecker();

        private LogForm _logWindow = null;
        private ConfigurationForm _configWindow = null;

        public ImapTrayApplicationContext()
        {
            _notifyIcon.Icon = Properties.Resources.AppIcon;
            _notifyIcon.DoubleClick += OpenEmailClient;
            _notifyIcon.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("Configuration", ShowCfg),
                new MenuItem("Log", ShowLog),
                new MenuItem("-"),
                new MenuItem("Exit", Exit)
            });
            _notifyIcon.Visible = true;

            _checker.onUnreadChanged += delegate(long unread)
            {
                if (unread == 0)
                {
                    _notifyIcon.Text = "No new emails";
                }
                else
                {
                    _notifyIcon.Text = String.Format("Unread emails: {0}", unread);
                }
            };

            _checker.onNewMessage += delegate(string username, string subject, string from)
            {
                _notifyIcon.BalloonTipTitle = username;
                _notifyIcon.BalloonTipText = String.Format("`{0}` from {1}", subject, from);
                _notifyIcon.ShowBalloonTip(15 * 1000);
            };

            _checker.Start(ConfigurationManager.Load());

            ConfigurationManager.onConfigurationChanged += delegate(Configuration cfg)
            {
                _checker.Stop();
                _checker.Start(cfg);
            };
        }

        private void OpenEmailClient(object sender, EventArgs e)
        {
            var clientPath = ConfigurationManager.Load().EmailClientPath;
            if (String.IsNullOrEmpty(clientPath))
            {
                return;
            }

            try
            {
                Process.Start(clientPath);
            }
            catch (Exception ex)
            {
                Log.Error("Error when starting process: {0}", ex.Message);
            }
        }

        private void ShowLog(object sender, EventArgs e)
        {
            if (_logWindow == null)
            {
                _logWindow = new LogForm();
            }

            if (_logWindow.Visible)
            {
                _logWindow.Activate();
            }
            else
            {
                _logWindow.ShowDialog();
            }
        }

        private void ShowCfg(object sender, EventArgs e)
        {
            if (_configWindow == null)
            {
                _configWindow = new ConfigurationForm();
            }

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
            _checker.Stop();
            Application.Exit();
        }
    }
}
