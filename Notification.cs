using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImapTray
{
    internal class Notification : IDisposable
    {
        public Image Icon = null;
        public int MaxWidth = 250;
        public string Text = "";
        public Color TextBackgorundColor = Color.White;
        public Color TextColor = Color.Black;
        public int Timeout = 5000;
        public string Title = "";
        public Color TitleBackgroundColor = Color.White;
        public Color TitleColor = Color.Black;

        private static readonly Padding ZeroPadding = new Padding(0);
        private readonly Form _form;
        private readonly Panel _pnlIcon;
        private readonly TableLayoutPanel _layout;
        private readonly Label _lblTitle;
        private readonly Label _lblText;

        public Notification()
        {
            _form = new Form
            {
                Width = MaxWidth,
                Height = 50,
                TopMost = true,
                TopLevel = true,
                ShowIcon = false,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                SizeGripStyle = SizeGripStyle.Hide,
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None
            };

            _layout = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 2,
                Dock = DockStyle.Fill
            };

            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _pnlIcon = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = ZeroPadding
            };

            _lblTitle = new Label
            {
                //Anchor = AnchorStyles.Left,
                Margin = ZeroPadding,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            _lblTitle.Click += OnClick;

            var titleFont = new Font(_lblTitle.Font.FontFamily, _lblTitle.Font.Size + 1, FontStyle.Bold);
            _lblTitle.Font = titleFont;

            _lblText = new Label
            {
                Margin = ZeroPadding,
                Padding = new Padding(0, 3, 0, 0),
                Dock = DockStyle.Fill
            };
            _lblText.Click += OnClick;

            _layout.SuspendLayout();
            _layout.Controls.Add(_pnlIcon, 0, 0);
            _layout.Controls.Add(_lblTitle, 1, 0);
            _layout.Controls.Add(_lblText, 1, 1);
            _layout.SetColumnSpan(_lblText, 2);
            _layout.ResumeLayout(true);

            _form.Controls.Add(_layout);

            _form.Closed += FormOnClosed;
            _form.Shown += FormOnShown;
        }

        public delegate void NotificationHandler(Notification n);

        public event NotificationHandler BeforeShow;
        public event NotificationHandler Closing;
        public event NotificationHandler Shown;

        public int Height
        {
            get { return _form.Height; }
        }

        public Point Location
        {
            get { return _form.DesktopLocation; }
        }

        public int Width
        {
            get { return _form.Width; }
        }

        public double Opacity
        {
            get { return _form.Opacity; }
            set { _form.Opacity = value; }
        }

        public void Dispose()
        {
            _pnlIcon.Dispose();
            _lblTitle.Font.Dispose();
            _lblTitle.Dispose();
            _lblText.Dispose();
            _layout.Dispose();
            _form.Dispose();
        }

        public void Hide()
        {
            Close();
        }

        public void SetLocation(int x, int y)
        {
            _form.SetDesktopLocation(x, y);
        }

        public void Show()
        {
            if (Icon != null)
            {
                _pnlIcon.BackgroundImage = Properties.Resources.AppIconImage;
                _pnlIcon.BackgroundImageLayout = ImageLayout.Center;
            }
            _pnlIcon.BackColor = TitleBackgroundColor;

            _lblTitle.Text = Title;
            _lblTitle.BackColor = TitleBackgroundColor;
            _lblTitle.ForeColor = TitleColor;

            _lblText.Text = Text;
            _lblText.BackColor = TextBackgorundColor;
            _lblText.ForeColor = TextColor;

            var maxSize = new Size(MaxWidth, SystemInformation.WorkingArea.Height);
            var titleSize = _lblTitle.GetPreferredSize(maxSize);
            var textSize = _lblText.GetPreferredSize(maxSize);
            _form.Size = new Size(MaxWidth, (titleSize.Height + textSize.Height) + 10);

            if (BeforeShow != null)
            {
                BeforeShow(this);
            }
            _form.Show();

            if (Timeout > 0)
            {
                Timer timer = new Timer { Interval = Timeout };
                timer.Tick += delegate
                {
                    Close();
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        private void Close()
        {
            if (Closing != null)
                Closing(this);
            _form.Close();
            Dispose();
        }

        private void FormOnClosed(object sender, EventArgs eventArgs)
        {
            _form.Dispose();
        }

        private void FormOnShown(object sender, EventArgs eventArgs)
        {
            if (Shown != null)
            {
                Shown(this);
            }
        }

        private void OnClick(object sender, EventArgs eventArgs)
        {
            Hide();
        }
    }
}