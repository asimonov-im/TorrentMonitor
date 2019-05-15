using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using TorrentMonitorLib;

namespace TorrentMonitorUI
{
    public partial class MainForm : Form
    {
        private readonly TorrentMonitor monitor;

        public MainForm(TorrentMonitor monitor)
        {
            this.monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
            monitor.Start();

            InitializeComponent();
        }

        public async Task AddTorrent(Uri location)
        {
            await monitor.AddTorrent(location, "External Add Request");
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // If the form is minimized, hide it from the task bar and show the
            // system tray icon (represented by the NotifyIcon control).
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
            }
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to quit?", this.Text, MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                monitor.Stop();
                NLog.LogManager.Shutdown();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }
    }
}
