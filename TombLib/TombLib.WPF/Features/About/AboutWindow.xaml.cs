#nullable enable

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using TombLib.Utils;

namespace TombLib.WPF.Features.About;

/// <summary>
/// WPF counterpart of the WinForms <c>TombLib.Forms.FormAbout</c>, shared by the
/// WPF shells (WadTool still uses the WinForms one). Each application passes its
/// own header artwork; title and version come from the entry application.
/// </summary>
public partial class AboutWindow : Window
{
	public AboutWindow(ImageSource aboutScreen)
	{
		InitializeComponent();

		Title = "About " + System.Windows.Forms.Application.ProductName;

		headerImage.Source = aboutScreen;

		string bitness = Environment.Is64BitProcess ? "64-bit" : "32-bit";
		versionText.Text = "Version " + System.Windows.Forms.Application.ProductVersion +
			" (.NET " + Logging.FrameworkVersion + ", " + bitness + ")";
	}

	private void Ok_Click(object sender, RoutedEventArgs e) => Close();

	private void Link_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not Hyperlink link || link.Tag is not string url)
			return;

		try
		{
			Process.Start(new ProcessStartInfo("http://www." + url + "/") { UseShellExecute = true });
		}
		catch
		{
			// ignored
		}
	}
}
