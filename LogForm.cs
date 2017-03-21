using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImapTray
{
    public partial class LogForm : Form
    {
        private readonly ImageList _severityIcons = new ImageList();

        public LogForm()
        {
            InitializeComponent();

            listView1.SizeChanged += ListViewUtils.AutoSizeColumns;
            ListViewUtils.AutoSizeColumns(listView1, null);

            _severityIcons.ImageSize = new Size(16, 16);
            _severityIcons.ColorDepth = ColorDepth.Depth32Bit;

            _severityIcons.Images.Add(Properties.Resources.Debug);
            _severityIcons.Images.Add(Properties.Resources.Info);
            _severityIcons.Images.Add(Properties.Resources.Warn);
            _severityIcons.Images.Add(Properties.Resources.Error);

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
                    ImageIndex = (int) m.severity,
                    Text = ""
                };
                item.SubItems.Add(m.when.ToString());
                item.SubItems.Add(m.msg);

                listView1.Items.Add(item);
            }

            listView1.EndUpdate();
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
            Icon = Properties.Resources.AppIcon;
        }
    }
}
