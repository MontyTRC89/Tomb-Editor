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
            // Parse --gapi <name> / --gapi=<name> and propagate via the
            // env var. Must happen before any DeviceManager access. Recognised
            // values: vulkan (default), dx11, directx11, opengl.
            args = ExtractRendererArg(args);

            var services = WPFInitializer.InitializeWPF();
            services.AddSingleton<ICustomGeometrySettingsPresetIOService, CustomGeometrySettingsPresetIOService>();
            ServiceLocator.Configure(services.BuildServiceProvider());

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Load configuration
            var initialEvents = new List<LogEventInfo>();
            var configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);

            // Apply persisted renderer choice. --gapi takes precedence — only
            // fall back to config if env var is empty.
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TOMBEDITOR_GRAPHIC_API")))
                Environment.SetEnvironmentVariable("TOMBEDITOR_GRAPHIC_API",
                    TombLib.Graphics.RendererCatalog.Resolve(configuration.Rendering_GraphicsApi).Id);

            // Update DarkUI configuration
            Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;

            // Setup logging
            using (var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents))
            {
                Application.EnableVisualStyles();
                Application.SetDefaultFont(new System.Drawing.Font("Segoe UI", 8.25f));
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
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

                    using (FormMain form = new FormMain(tool))
                    {
                        form.Show();

                        if (!string.IsNullOrEmpty(refLevel)) WadActions.LoadReferenceLevel(tool, form, refLevel);
                        if (!string.IsNullOrEmpty(startWad)) WadActions.LoadWad(tool, form, true, startWad);

                        Application.Run(form);
                    }
                }
            }
        }

        // Pulls "--gapi <name>" / "--gapi=<name>" out of args (sets the
        // TOMBEDITOR_GRAPHIC_API env var) and returns the remaining args.
        private static string[] ExtractRendererArg(string[] args)
        {
            var kept = new List<string>(args.Length);
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if (a.StartsWith("--gapi=", StringComparison.OrdinalIgnoreCase))
                {
                    Environment.SetEnvironmentVariable("TOMBEDITOR_GRAPHIC_API", a.Substring("--gapi=".Length));
                    continue;
                }
                if (a.Equals("--gapi", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    Environment.SetEnvironmentVariable("TOMBEDITOR_GRAPHIC_API", args[++i]);
                    continue;
                }
                kept.Add(a);
            }
            return kept.ToArray();
        }
    }
}
