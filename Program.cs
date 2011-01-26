using System;

namespace acDumperAgentMain
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new acDumperAgentForm());
        }
    }
}
