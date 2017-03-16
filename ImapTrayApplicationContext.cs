using System;
using System.Windows.Forms;

namespace ImapTray
{
    class ImapTrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon = new NotifyIcon();
        private readonly ConfigurationForm _configWindow = new ConfigurationForm();
        private readonly AccountChecker _checker = new AccountChecker();

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
            throw new NotImplementedException();
        }

        private void ShowCfg(object sender, EventArgs e)
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
