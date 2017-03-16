using System;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace ImapTray
{
    public partial class LogForm : Form
    {
        private readonly ImageList _severityIcons = new ImageList();

        public LogForm()
        {
            InitializeComponent();

            _severityIcons.ImageSize = new Size(16, 16);
            _severityIcons.Images.Add(Properties.Resources.AppIcon);

            listView1.SmallImageList = _severityIcons;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LogForm_Shown(object sender, EventArgs e)
        {
            RefreshList(Log.ListAll());
            Log.onLogChanged += RefreshList;
        }

        private void RefreshList(Log.Message[] messages)
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();

            foreach (var m in messages)
            {
                var item = new ListViewItem
                {
                    ImageIndex = 0,
                    Text = ""
                };
                item.SubItems.Add(m.when.ToString());
                item.SubItems.Add(m.msg);

                listView1.Items.Add(item);
            }

            listView1.EndUpdate();
        }
    }
}
