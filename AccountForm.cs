using System.Windows.Forms;

namespace ImapTray
{
    public sealed partial class AccountForm : Form
    {
        private event OnActionHandler OnAction;

        public AccountForm(string title, string buttonText, Account account, OnActionHandler onAction)
        {
            InitializeComponent();

            Text = title;
            btnAction.Text = buttonText;

            if (account != null)
            {
                txbLogin.Text = account.username;
                txbPassword.Text = account.password;
                txbServer.Text = account.server;
                numPort.Value = account.port;
                chbSsl.Checked = account.ssl;
            }

            OnAction += onAction;
        }

        private void btnAction_Click(object sender, System.EventArgs e)
        {
            if (!Validate())
            {
                return;
            }

            string username = txbLogin.Text;
            string password = txbPassword.Text;
            string server = txbServer.Text;
            ushort port = decimal.ToUInt16(numPort.Value);
            bool ssl = chbSsl.Checked;
            OnAction(this, new Account(username, password, server, port, ssl));
        }
    }

    public delegate void OnActionHandler(AccountForm form, Account account);
}
