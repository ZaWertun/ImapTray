using System;
using System.Windows.Forms;

namespace ImapTray
{
    static class ListViewUtils
    {
        public static void AutoSizeColumns(object sender, EventArgs e)
        {
            lock (sender)
            {
                var listView = sender as ListView;
                if (listView == null) return;

                int fixedWidth = 0;
                float totalColumnWidth = 0;

                listView.BeginUpdate();
                foreach (ColumnHeader col in listView.Columns)
                {
                    var tag = Convert.ToInt32(col.Tag);
                    totalColumnWidth += tag;
                    if (tag == 0)
                    {
                        fixedWidth += col.Width;
                    }
                }

                foreach (ColumnHeader col in listView.Columns)
                {
                    var tag = Convert.ToInt32(col.Tag);
                    if (tag > 0)
                    {
                        float colPercentage = tag / totalColumnWidth;
                        col.Width = (int)(colPercentage * (listView.ClientRectangle.Width - fixedWidth));
                    }
                }
                listView.EndUpdate();
            }
        }
    }
}
