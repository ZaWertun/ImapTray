using System;
using System.Windows.Forms;

namespace ImapTray
{
    public partial class ConfigurationForm : Form
    {
        public ConfigurationForm()
        {
            InitializeComponent();

            ConfigurationManager.onConfigurationChanged += RefreshList;
            RefreshList(ConfigurationManager.Load());
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

        private void RefreshList(Configuration cfg)
        {
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

        private readonly object _resizing = new Object();

        private void listView1_SizeChanged(object sender, EventArgs e)
        {
            lock (_resizing)
            {
                var listView = sender as ListView;
                if (listView == null) return;

                float totalColumnWidth = 0;
                for (int i = 0; i < listView.Columns.Count; i++)
                {
                    totalColumnWidth += Convert.ToInt32(listView.Columns[i].Tag);
                }

                for (int i = 0; i < listView.Columns.Count; i++)
                {
                    float colPercentage = (Convert.ToInt32(listView.Columns[i].Tag) / totalColumnWidth);
                    listView.Columns[i].Width = (int)(colPercentage * listView.ClientRectangle.Width);
                }
            }
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
    }
}
