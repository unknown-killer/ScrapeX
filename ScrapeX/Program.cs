using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScrapeX
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "--service")
            {
                System.ServiceProcess.ServiceBase.Run(new ScrapeXWindowsService());
                return;
            }
            if (args.Length != 0) { MessageBox.Show("Use ScrapeX without arguments for the desktop, or --service through Windows Service Control Manager.", "ScrapeX"); return; }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
