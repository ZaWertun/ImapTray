using System;
using System.Windows.Forms;

namespace ImapTray
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var credentials = new[]
            {
                new Credential("test@gmail.com", "password", "imap.gmail.com", 993, true)
            };

            Configuration.Save(credentials);
            var tmp = Configuration.Load();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MyApplicationContext());
        }
    }
}
