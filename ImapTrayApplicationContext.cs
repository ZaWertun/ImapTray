using System;
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
            _notifyIcon.DoubleClick += ShowCfg;
            _notifyIcon.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("Configuration", ShowCfg),
                new MenuItem("Log", ShowLog),
                new MenuItem("-"),
                new MenuItem("Exit", Exit)
            });
            _notifyIcon.Visible = true;

            _checker.Start(ConfigurationManager.Load());
            ConfigurationManager.onConfigurationChanged += delegate(Configuration cfg)
            {
                _checker.Stop();
                _checker.Start(cfg);
            };
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
