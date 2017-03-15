using System;
using System.Windows.Forms;

namespace ImapTray
{
    public partial class ConfigurationForm : Form
    {
        public ConfigurationForm()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            new AccountForm(
                (login, password, server, port, ssl) =>
                {
                    if (login.Length > 0) { }
                }
            ).ShowDialog(this);
        }

        private void FillList()
        {
            var accounts = Configuration.Load();

            listView1.BeginUpdate();
            listView1.Items.Clear();
            for (var i = 0; i < accounts.Length; ++i)
            {
                var acc = accounts[i];
                var item = new ListViewItem();
                item.Tag = i;
                item.Text = acc.login;
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

            FillList();
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
    }
}
