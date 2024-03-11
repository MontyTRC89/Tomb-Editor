using DarkUI.Config;
using DarkUI.Win32;
using MvvmDialogs;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using TombEditor.WPF.Forms;
using TombEditor.WPF.ViewModels;
//using TombEditor.WPF.Views;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using WinFormsApp = System.Windows.Forms.Application;

namespace TombEditor.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private static readonly Mutex mutex = new(true, "{84867F76-232B-442B-9B10-DC72C8288839}");

	protected override void OnStartup(StartupEventArgs e)
	{
		Localizer.Instance.LoadLanguage("en");
		Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
		Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

		string[] args = e.Args;

		string? startFile = null;
		string? batchFile = null;
		bool doBatchCompile = false;
		BatchCompileList? batchList = null;

		if (args.Length >= 1)
		{
			// Open files on start
			if (args[0].EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase) ||
				args[0].EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
				startFile = args[0];

			// Batch-compile levels
			if (args[0].EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase))
			{
				batchFile = args[0];
				batchList = BatchCompileList.ReadFromXml(batchFile);
				doBatchCompile = batchList?.Files.Count > 0;
			}
		}

		// Load configuration
		var initialEvents = new List<LogEventInfo>();
		Configuration configuration = new Configuration().LoadOrUseDefault<Configuration>(initialEvents);

		// Update DarkUI configuration
		Colors.Brightness = configuration.UI_FormColor_Brightness / 100.0f;

		if (configuration.Editor_AllowMultipleInstances || doBatchCompile || mutex.WaitOne(TimeSpan.Zero, true))
		{
			// Setup logging
			using var log = new Logging(configuration.Log_MinLevel, configuration.Log_WriteToFile, configuration.Log_ArchiveN, initialEvents);

			// Create configuration file
			configuration.SaveTry();

			// Setup application
			WinFormsApp.EnableVisualStyles();
			WinFormsApp.SetDefaultFont(new System.Drawing.Font("Segoe UI", 8.25f));
			WinFormsApp.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware);
			WinFormsApp.SetCompatibleTextRenderingDefault(false);
			WinFormsApp.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);

			WinFormsApp.ThreadException += (sender, e) =>
			{
				log.HandleException(e.Exception);
				using var dialog = new System.Windows.Forms.ThreadExceptionDialog(e.Exception);

				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Abort)
					Environment.Exit(1);
			};

			WinFormsApp.AddMessageFilter(new ControlScrollFilter());
			SynchronizationContext.SetSynchronizationContext(new System.Windows.Forms.WindowsFormsSynchronizationContext());

			if (!DefaultPaths.CheckCatalog(DefaultPaths.EngineCatalogsDirectory))
				Environment.Exit(1);

			// Load catalogs
			try
			{
				TrCatalog.LoadCatalog(WinFormsApp.StartupPath + "\\Catalogs\\TrCatalog.xml");
				NgCatalog.LoadCatalog(WinFormsApp.StartupPath + "\\Catalogs\\NgCatalog.xml");
			}
			catch (Exception)
			{
				MessageBox.Show("An error occured while loading one of the catalog files.\nFile may be corrupted. Check the log file for details.");
				Environment.Exit(1);
			}

			var editor = new Editor(SynchronizationContext.Current, configuration)
			{
				DialogService = new DialogService()
			};

			Editor.Instance = editor;

			//TEST WINDOWS
			//var mainWindow = new FindTexturesWindow { };
			//Current.MainWindow = mainWindow;
			//Current.MainWindow.Show();

			// Run editor normally if no batch compile is pending.
			// Otherwise, don't load main form and jump straight to batch-compiling levels.

			if (!doBatchCompile)
			{

				var mainWindow = new MainWindow
				{
					ViewModel = new MainWindowViewModel()
				};

				Current.MainWindow = mainWindow;
				Current.MainWindow.Show();

				if (!string.IsNullOrEmpty(startFile)) // Open files on start
				{
					if (startFile.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase))
						EditorActions.OpenLevelPrj(mainWindow.ViewModel, startFile);
					else
						EditorActions.OpenLevel(mainWindow.ViewModel, startFile);
				}
				else if (editor.Configuration.Editor_OpenLastProjectOnStartup)
				{
					if (TombEditor.Properties.Settings.Default.RecentProjects != null && TombEditor.Properties.Settings.Default.RecentProjects.Count > 0 &&
						File.Exists(TombEditor.Properties.Settings.Default.RecentProjects[0]))
						EditorActions.OpenLevel(mainWindow.ViewModel, TombEditor.Properties.Settings.Default.RecentProjects[0]);
				}
			}
			else
				EditorActions.BuildInBatch(editor, batchList, batchFile);
		}
        else if (startFile != null) // Send opening file to existing editor instance
			SingleInstanceManagement.Send(Process.GetCurrentProcess(), new List<string>() { ".prj2" }, startFile);
		else // Just bring editor to top, if user tries to launch another copy
			SingleInstanceManagement.Bump(Process.GetCurrentProcess());

		base.OnStartup(e);
	}
}
