using System;
using System.Threading;
using System.Windows.Forms;

namespace ImapTray
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            bool result;
            var mutex = new Mutex(true, "ImapTray", out result);
            if (!result)
            {
                return;
            }

            Log.Info("Application starting up...");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ImapTrayApplicationContext());

            GC.KeepAlive(mutex);
        }
    }
}
