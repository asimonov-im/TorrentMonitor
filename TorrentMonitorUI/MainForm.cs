using System;
using System.ComponentModel;
using System.Drawing;
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

        private async void Form1_Closing(object sender, CancelEventArgs e)
        {
            // Always cancel, so we can wait for StopAsync() to finish, when actually shutting down
            e.Cancel = true;

            var result = MessageBox.Show("Are you sure you want to quit?", this.Text, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // Disable UI so user can't click on anything while we're waiting for tasks
                Enabled = false;

                // Stop the monitor
                await monitor.StopAsync();
                NLog.LogManager.Shutdown();

                // Unregister from the callback and manually close the form
                FormClosing -= Form1_Closing;
                Close();
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void PatternListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            patternTextbox.BackColor = Color.Empty;
            if (e.IsSelected)
            {
                patternTextbox.Text = e.Item.Text;
                updatePatternButton.Enabled = true;
                removePatternButton.Enabled = true;
            }
            else
            {
                patternTextbox.Text = string.Empty;
                updatePatternButton.Enabled = false;
                removePatternButton.Enabled = false;
            }
        }

        private void AddPatternButton_Click(object sender, EventArgs e)
        {
            var pattern = GetPatternAndHighlightError();
            if (pattern != null)
            {
                patternListView.Items.Add(pattern.Pattern);
                monitor.Patterns = monitor.Patterns.Add(pattern);

                patternListView.SelectedItems.Clear();
                patternTextbox.Clear();
                patternTextbox.Focus();
            }
        }

        private void UpdatePatternButton_Click(object sender, EventArgs e)
        {
            var pattern = GetPatternAndHighlightError();
            if (pattern != null)
            {
                int selectedIndex = patternListView.SelectedItems[0].Index;
                patternListView.Items[selectedIndex].Text = pattern.Pattern;
                monitor.Patterns = monitor.Patterns.RemoveAt(selectedIndex);
                monitor.Patterns = monitor.Patterns.Insert(selectedIndex, pattern);

                patternListView.SelectedItems.Clear();
                patternTextbox.Focus();
            }
        }

        private void RemovePatternButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = patternListView.SelectedItems[0].Index;
            patternListView.Items.RemoveAt(selectedIndex);
            monitor.Patterns = monitor.Patterns.RemoveAt(selectedIndex);
        }

        private MatchPattern GetPatternAndHighlightError()
        {
            var pattern = MatchPattern.TryCreate(patternTextbox.Text);
            if (pattern == null)
            {
                patternTextbox.BackColor = Color.Pink;
                patternTextbox.Focus();
            }
            else
            {
                patternTextbox.BackColor = Color.Empty;
            }

            return pattern;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            monitor.Start();

            patternListView.Columns.Add("Pattern", -2, HorizontalAlignment.Left);
            patternListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            patternListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            foreach (var pattern in monitor.Patterns)
            {
                patternListView.Items.Add(pattern.Pattern);
            }
        }
    }
}
