using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ImapTray
{
    public partial class ConfigurationForm : Form
    {
        public ConfigurationForm()
        {
            InitializeComponent();

            listView1.SizeChanged += ListViewUtils.AutoSizeColumns;
            ListViewUtils.AutoSizeColumns(listView1, null);

            ConfigurationManager.onConfigurationChanged += ConfigurationChanged;
            ConfigurationChanged(ConfigurationManager.Load());
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            new AccountForm(
                "Add account…",
                "Add",
                null,
                (form, account) =>
                {
                    form.Close();
                    ConfigurationManager.Save(ConfigurationManager.Load().AddAccount(account));
                }
            ).ShowDialog(this);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var cfg = ConfigurationManager.Load();
            var selected = listView1.SelectedItems;
            foreach (ListViewItem s in selected)
            {
                cfg.RemoveAccount((int)s.Tag);
            }
            ConfigurationManager.Save(cfg);
        }

        private void ConfigurationChanged(Configuration cfg)
        {
            txbEmailClientPath.Text = cfg.EmailClientPath;

            listView1.BeginUpdate();
            listView1.Items.Clear();

            for (var i = 0; i < cfg.Accounts.Length; ++i)
            {
                var acc = cfg.Accounts[i];
                var item = new ListViewItem
                {
                    Tag = i,
                    Text = acc.username
                };
                item.SubItems.Add(acc.server);
                item.SubItems.Add(acc.port.ToString());
                item.SubItems.Add(acc.ssl ? "✔" : "");
                listView1.Items.Add(item);
            }

            listView1.EndUpdate();
        }

        private void ConfigurationForm_Shown(object sender, EventArgs e)
        {
            listView1.Width += 1;
            listView1.Width -= 1;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var listView = (ListView)sender;
            var item = listView.GetItemAt(e.X, e.Y);
            var index = (int) item.Tag;
            var account = ConfigurationManager.Load().Accounts[index];
            var form = new AccountForm(
                "Change account…",
                "Save",
                account,
                (accountForm, changedAccount) =>
                {
                    accountForm.Close();
                    ConfigurationManager.Save(ConfigurationManager.Load().SetAccount(index, changedAccount));
                }
            );
            form.ShowDialog(this);
        }

        private void btnSelectEmailClientPath_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select email client executable";
            dialog.Filter = "Executable (*.exe)|*.exe";
            dialog.FileOk += delegate(object o, CancelEventArgs args)
            {
                var path = ((OpenFileDialog) o).FileName;
                ConfigurationManager.Save(ConfigurationManager.Load().SetEmailClientPath(path));
            };
            dialog.Multiselect = false;
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            dialog.CheckFileExists = true;
            dialog.ShowDialog(this);
        }

        private void ConfigurationForm_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.AppIcon;
        }
    }
}
