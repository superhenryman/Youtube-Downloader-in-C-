using System.Runtime.InteropServices;

namespace YTDownloader_GUI
{
    internal static class Program
    {
        //[DllImport("kernel32.dll")]
        //private static extern bool AllocConsole();
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //DialogResult result = MessageBox.Show("Would you like to run in debug mode?", "Debug", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (result == DialogResult.Yes)
            //{
            //    AllocConsole();
            //}
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}