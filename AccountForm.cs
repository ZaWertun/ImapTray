using System.Windows.Forms;

namespace ImapTray
{
    public partial class AccountForm : Form
    {
        private event OnActionHandler OnAction;

        public AccountForm(OnActionHandler onAction)
        {
            InitializeComponent();
            OnAction += onAction;
        }

        private void btnAction_Click(object sender, System.EventArgs e)
        {
            if (!Validate())
            {
                return;
            }

            string login = txbLogin.Text;
            string password = txbPassword.Text;
            string server = txbServer.Text;
            uint port = decimal.ToUInt16(numPort.Value);
            bool ssl = chbSsl.Checked;
            OnAction(login, password, server, port, ssl);
        }
    }

    public delegate void OnActionHandler(string login, string password, string server, uint port, bool ssl);
}
