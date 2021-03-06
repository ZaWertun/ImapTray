﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ImapTray
{
    class ImapTrayApplicationContext : ApplicationContext
    {
        private readonly Timer _clickTimer = new Timer();
        private readonly NotifyIcon _notifyIcon = new NotifyIcon();
        private readonly AccountChecker _checker = new AccountChecker();
        private readonly Dictionary<string, int> _unread = new Dictionary<string, int>();

        private bool _singleClick;
        private LogForm _logWindow = null;
        private ConfigurationForm _configWindow = null;

        public ImapTrayApplicationContext()
        {
            MainForm = new Form
            {
                WindowState = FormWindowState.Minimized,
                ShowInTaskbar = false
            };
            MainForm.Shown += (sender, args) => ((Form) sender).Hide();

            _clickTimer.Tick += ClickTimerOnTick;
            _clickTimer.Interval = SystemInformation.DoubleClickTime;

            _notifyIcon.Icon = Properties.Resources.AppIcon;
            _notifyIcon.MouseDown += NotifyIconOnMouseUp;
            _notifyIcon.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("Check now", CheckNow),
                new MenuItem("-"),
                new MenuItem("Configuration", ShowCfg),
                new MenuItem("Log", ShowLog),
                new MenuItem("-"),
                new MenuItem("Exit", Exit)
            });
            _notifyIcon.Visible = true;

            _checker.onUnreadChanged += delegate(Account account, int unread)
            {
                _unread[account.username] = unread;
            };

            _checker.onNewMessage += delegate(string username, string subject, string from)
            {
                Debug.WriteLine("Account `{0}`: new message: {1} from {2}", username, subject, from);

                MainForm.BeginInvoke(new Action(delegate
                {
                    var notify = new Notification
                    {
                        Icon = Properties.Resources.AppIconImage,
                        Title = username,
                        TitleBackgroundColor = Color.Gray,
                        Text = String.Format("Subject: {0}\nFrom: {1}", subject, from),
                        Timeout = 15 * 1000
                    };
                    NotificationManager.Add(notify);
                }));
            };

            _checker.Start(ConfigurationManager.Load());

            ConfigurationManager.onConfigurationChanged += delegate(Configuration cfg)
            {
                _checker.Stop();
                _checker.Start(cfg);
            };
        }

        private void ClickTimerOnTick(object sender, EventArgs eventArgs)
        {
            if (_singleClick)
            {
                _clickTimer.Stop();
                _singleClick = false;
                
                ShowUnreadCount();
            }
        }

        private void NotifyIconOnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (e.Clicks < 2)
            {
                _singleClick = true;
                _clickTimer.Start();
            }
            else
            {
                _singleClick = false;
                _clickTimer.Stop();

                OpenEmailClient();
            }
        }

        private void ShowUnreadCount()
        {
            var text = "";
            foreach (var keyValue in _unread)
            {
                text += keyValue.Key + ": " + keyValue.Value + '\n';
            }

            var notify = new Notification
            {
                Icon = Properties.Resources.AppIconImage,
                Title = "Unread emails",
                TitleBackgroundColor = Color.Gray,
                Text = text,
                Timeout = 5 * 1000
            };
            NotificationManager.Add(notify);
        }

        private void OpenEmailClient()
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

        private void CheckNow(object sender, EventArgs e)
        {
            AccountChecker.CheckNow();
        }

        private void ShowCfg(object sender, EventArgs e)
        {
            if (_configWindow == null)
            {
                _configWindow = new ConfigurationForm();
                _configWindow.Closed += ConfigWindowOnClosed;
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

        private void ConfigWindowOnClosed(object sender, EventArgs eventArgs)
        {
            _configWindow.Dispose();
            _configWindow = null;
        }

        private void ShowLog(object sender, EventArgs e)
        {
            if (_logWindow == null)
            {
                _logWindow = new LogForm();
                _logWindow.Closed += LogWindowOnClosed;
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

        private void LogWindowOnClosed(object sender, EventArgs eventArgs)
        {
            _logWindow.Dispose();
            _logWindow = null;
        }

        private void Exit(object sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            _checker.Stop();
            MainForm.Close();
            Application.Exit();
        }
    }
}
