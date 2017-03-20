using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ImapTray
{
    static class NotificationManager
    {
        private const int PaddingX = 5;
        private const int PaddingY = 5;
        private const int MaxNotifications = 5;
        private static readonly List<NotificationAndPosition> Notifications = new List<NotificationAndPosition>();

        public static void Add(Notification notification)
        {

            lock (Notifications)
            {
                if (Notifications.Count >= MaxNotifications)
                {
                    var first = Notifications.First();
                    first.Notify.Hide();
                    Notifications.Remove(first);
                    Realign(0, first.Height);
                }
                Notifications.Add(new NotificationAndPosition(notification, 0));
            }

            var workingArea = SystemInformation.WorkingArea;
            notification.BeforeShow += delegate(Notification n)
            {
                int selfHeight = n.Height + PaddingY;
                n.Opacity = 0;
                lock (Notifications)
                {
                    int reserved = Notifications.Select(x => x.Height).Sum();
                    n.SetLocation(workingArea.Width - n.Width - PaddingX, workingArea.Height - reserved - selfHeight);
                    try
                    {
                        Notifications.Single(x => x.Notify == n).Height = selfHeight;
                    }
                    catch {}
                }
            };

            notification.Shown += delegate(Notification n)
            {
                var timer = new Timer { Interval = 25 };
                timer.Tick += delegate
                {
                    double opacity = n.Opacity;
                    if (Math.Abs(1 - opacity) > float.Epsilon)
                    {
                        // FIXME: increment should depend on animation time!
                        n.Opacity = opacity + 0.05;
                    }
                    else
                    {
                        timer.Stop();
                        timer.Dispose();
                    }
                };
                timer.Start();
            };

            notification.Closing += delegate(Notification n)
            {
                lock (Notifications)
                {
                    int index = 0;
                    int removedHeight = 0;
                    try
                    {
                        var el = Notifications.Single(x => x.Notify == n);
                        removedHeight = el.Height;
                        index = Notifications.IndexOf(el);
                        Notifications.Remove(el);
                    }
                    catch { }
                    if (removedHeight > 0)
                    {
                        Realign(index, removedHeight);
                    }
                }
            };
            notification.Show();
        }

        private static void Realign(int index, int height)
        {
            lock (Notifications)
            {
                for (var i = 0; i < Notifications.Count; ++i)
                {
                    var el = Notifications[i];
                    el.TargetY = (i < index) ? 0 : el.Notify.Location.Y + height;
                }
            }

            var t1 = new Timer { Interval = 10 };
            t1.Tick += delegate
            {
                lock (Notifications)
                {
                    int finished = 0;
                    foreach (var el in Notifications)
                    {
                        var y = el.Notify.Location.Y;
                        if (el.TargetY == 0 || y >= el.TargetY)
                        {
                            finished += 1;
                        }
                        else
                        {
                            y += 5;
                            el.Notify.SetLocation(el.Notify.Location.X, (y > el.TargetY) ? el.TargetY : y);
                        }
                    }
                    if (finished == Notifications.Count)
                    {
                        t1.Stop();
                        t1.Dispose();
                    }
                }
            };
            t1.Start();
        }
    }

    class NotificationAndPosition
    {
        public readonly Notification Notify;
        public int Height;
        public int TargetY = 0;

        public NotificationAndPosition(Notification notify, int height)
        {
            Notify = notify;
            Height = height;
        }
    }
}
