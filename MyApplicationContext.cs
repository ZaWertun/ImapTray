using System;
using System.Windows.Forms;

namespace ImapTray
{
    class MyApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon = new NotifyIcon();
        private readonly Configuration _configWindow = new Configuration();

        public MyApplicationContext()
        {
            _notifyIcon.Icon = Properties.Resources.AppIcon;
            _notifyIcon.DoubleClick += ShowConfig;
            _notifyIcon.ContextMenu = new ContextMenu(new[] { new MenuItem("Configuration", ShowConfig), new MenuItem("Exit", Exit) });
            _notifyIcon.Visible = true;
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
