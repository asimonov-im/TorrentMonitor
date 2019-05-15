using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using TorrentMonitorLib;

namespace TorrentMonitorUI
{
    // Source: https://www.codeproject.com/Articles/1191863/Opening-a-specific-file-format-with-a-single-insta
    class SingleInstanceController : WindowsFormsApplicationBase
    {
        // TODO: Should this use AppDomain.CurrentDomain.BaseDirectory instead?
        private static readonly string ConfigFilePath = Path.Combine(Application.StartupPath, "config.json");

        public SingleInstanceController()
        {
            // Run only single instance of this app
            IsSingleInstance = true;

            // Register callback for new instances being started
            StartupNextInstance += StartupAdditionalInstance;
        }

        async void StartupAdditionalInstance(object sender, StartupNextInstanceEventArgs e)
        {
            await AddTorrentAsync(e.CommandLine);
        }

        /// <summary>
        /// This method is called when first instance of the application is started.
        /// </summary>
        protected override async void OnCreateMainForm()
        {
            var monitor = TorrentMonitor.FromSettingsFile(ConfigFilePath);
            MainForm = new MainForm(monitor);
            await AddTorrentAsync(CommandLineArgs);
        }

        private async Task AddTorrentAsync(ReadOnlyCollection<string> args)
        {
            var form = MainForm as MainForm;
            if (args.Count > 0 && Uri.TryCreate(args[0], UriKind.Absolute, out Uri result))
            {
                await form.AddTorrent(result);
            }
        }
    }
}
