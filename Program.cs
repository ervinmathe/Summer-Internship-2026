using ScriptRunner.Core;
using ScriptRunner.Engine;

namespace Script_runner {

    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ScriptModule.InitializeScripts("C:\\Nyarigyak_2026\\task1\\Script_runner\\Scripts");
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}