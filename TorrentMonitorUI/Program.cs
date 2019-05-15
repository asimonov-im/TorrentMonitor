using System;
using System.Windows.Forms;
using TorrentMonitorLib;

namespace TorrentMonitorUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(params string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SingleInstanceController controller = new SingleInstanceController();
            controller.Run(args);
        }
    }
}
