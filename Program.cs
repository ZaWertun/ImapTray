using System;
using System.Windows.Forms;

namespace ImapTray
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Log.Info("Application starting up...");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ImapTrayApplicationContext());
        }
    }
}
