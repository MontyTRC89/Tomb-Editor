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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Load configuration
            var initialEvents = new List<LogEventInfo>();
            var configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);

            // Update DarkUI configuration
            Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;

            var services = WPFInitializer.InitializeWPF();
            services.AddSingleton<ICustomGeometrySettingsPresetIOService, CustomGeometrySettingsPresetIOService>();
            ServiceLocator.Configure(services.BuildServiceProvider());

            // Setup logging
            using (var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents))
            {
                Application.EnableVisualStyles();
                Application.SetDefaultFont(new System.Drawing.Font("Segoe UI", 8.25f));
                // PerMonitorV2 keeps hosted WinForms controls in sync with the per-monitor-aware
                // WPF shell; with SystemAware they render at the system DPI scale and overflow
                // their WindowsFormsHost bounds.
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                Application.SetCompatibleTextRenderingDefault(false);
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

                // WadToolClass.RaiseEvent marshals editor events through
                // SynchronizationContext.Current.Send(). The WPF shell has no WinForms message
                // loop, so a WindowsFormsSynchronizationContext would deadlock Send(); bind to
                // the WPF dispatcher (already created by WPFInitializer) instead.
                var dispatcher = System.Windows.Application.Current?.Dispatcher
                    ?? System.Windows.Threading.Dispatcher.CurrentDispatcher;
                SynchronizationContext.SetSynchronizationContext(
                    new System.Windows.Threading.DispatcherSynchronizationContext(dispatcher));

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
                                    continue; // No file and no valid argument, don't even try to load anything

                                if (loadAsRefLevel)
                                {
                                    if (arg.EndsWith("prj2", StringComparison.InvariantCultureIgnoreCase))
                                        refLevel = arg;
                                }
                                else
                                    startWad = arg;

                                loadAsRefLevel = false; // Reset arg mode if no expected path was found next to it
                            }
                        }
                    }

                    var wpfApp = System.Windows.Application.Current;

                    // WadActions shows DarkMessageBox (WinForms); route it onto the WPF
                    // CMessageBox so message boxes match the shell.
                    WpfMessageBoxBridge.Install();

                    // Without this handler, exceptions thrown during MainWindow construction or
                    // any later WPF-dispatched callback are swallowed silently. Route them
                    // through the standard logger and surface them so we can actually debug them.
                    wpfApp.DispatcherUnhandledException += (sender, e) =>
                    {
                        log.HandleException(e.Exception);
                        MessageBox.Show(e.Exception.ToString(), "WadTool — unhandled exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Handled = true;
                    };

                    try
                    {
                        var mainWindow = new MainWindow(tool);

                        // Open the files passed on the command line once the window has
                        // rendered, so progress/error dialogs have a live owner.
                        mainWindow.ContentRendered += (_, _) =>
                        {
                            if (!string.IsNullOrEmpty(refLevel)) WadActions.LoadReferenceLevel(tool, mainWindow.GetWin32Window(), refLevel);
                            if (!string.IsNullOrEmpty(startWad)) WadActions.LoadWad(tool, mainWindow.GetWin32Window(), true, startWad);
                        };

                        wpfApp.Run(mainWindow);
                    }
                    catch (Exception ex)
                    {
                        log.HandleException(ex);
                        MessageBox.Show(ex.ToString(), "WadTool — startup failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    wpfApp.Shutdown();
                }
            }

            // The hosted native rendering device (DeviceManager singleton) keeps a foreground thread
            // alive, so the process would otherwise linger after the main window closes. Configuration
            // and logs are already flushed at this point, so terminate now.
            Environment.Exit(0);
        }
    }
}
