using DarkUI.Config;
using DarkUI.Win32;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TombLib.Services;
using TombLib.Services.Abstract;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.Services;

namespace WadTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point of the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            // Initialize WinForms subsystem for hosted controls and legacy dialog forms.
            Application.SetHighDpiMode(HighDpiMode.DpiUnawareGdiScaled);
            Application.EnableVisualStyles();
            Application.SetDefaultFont(new System.Drawing.Font("Segoe UI", 8.25f));
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize WPF and DI services.
            var services = WPFInitializer.InitializeWPF();
            services.AddSingleton<ICustomGeometrySettingsPresetIOService, CustomGeometrySettingsPresetIOService>();
            ServiceLocator.Configure(services.BuildServiceProvider());

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Load configuration.
            var initialEvents = new List<LogEventInfo>();
            var configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);

            // Update DarkUI configuration.
            Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;

            // Setup logging.
            using (var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents))
            {
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += (sender, e) =>
                {
                    log.HandleException(e.Exception);
                    using (var dialog = new ThreadExceptionDialog(e.Exception))
                        if (dialog.ShowDialog() == DialogResult.Abort)
                            Environment.Exit(1);
                };

                configuration.SaveTry();

                if (!DefaultPaths.CheckCatalog(DefaultPaths.EngineCatalogsDirectory))
                    Environment.Exit(1);

                TrCatalog.LoadCatalog(DefaultPaths.EngineCatalogsDirectory);

                Application.AddMessageFilter(new ControlScrollFilter());
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

                using (WadToolClass tool = new WadToolClass(configuration))
                {
                    string startWad = null;
                    string refLevel = null;

                    if (args.Length > 0)
                    {
                        bool loadAsRefLevel = false;

                        foreach (var arg in args)
                        {
                            if (arg.Equals("-r", StringComparison.InvariantCultureIgnoreCase))
                                loadAsRefLevel = true;
                            else
                            {
                                if (!File.Exists(arg))
                                    continue;

                                if (loadAsRefLevel)
                                {
                                    if (arg.EndsWith("prj2", StringComparison.InvariantCultureIgnoreCase))
                                        refLevel = arg;
                                }
                                else
                                    startWad = arg;

                                loadAsRefLevel = false;
                            }
                        }
                    }

                    // Configure WPF Application for standalone use.
                    System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;

                    // Launch WPF main window.
                    var window = new MainWindow(tool);
                    System.Windows.Application.Current.MainWindow = window;
                    window.Show();

                    if (!string.IsNullOrEmpty(refLevel))
                        WadActions.LoadReferenceLevel(tool, window, refLevel);
                    if (!string.IsNullOrEmpty(startWad))
                        WadActions.LoadWad(tool, window, true, startWad);

                    // Run WPF message loop, which also pumps WinForms messages.
                    System.Windows.Threading.Dispatcher.Run();
                }
            }
        }
    }
}
