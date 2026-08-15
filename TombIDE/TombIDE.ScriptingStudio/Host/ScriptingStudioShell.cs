#nullable enable

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Shell;

namespace TombIDE.ScriptingStudio.Host;

internal sealed class ScriptingStudioShell : IScriptingStudioShell
{
	private readonly IServiceScope _shellScope;
	private readonly RootShellHost _host;
	private readonly RootShellViewModel _viewModel;
	private bool _disposed;

	public ScriptingStudioShell(IServiceScope shellScope, RootShellViewModel viewModel)
	{
		ArgumentNullException.ThrowIfNull(shellScope);
		ArgumentNullException.ThrowIfNull(viewModel);

		_shellScope = shellScope;
		_viewModel = viewModel;

		var view = new RootShellView
		{
			DataContext = _viewModel
		};

		_host = new RootShellHost(view);
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_host.Dispose();
		_shellScope.Dispose();
	}

	public void Mount(Control hostContainer, Form ownerForm)
	{
		ArgumentNullException.ThrowIfNull(hostContainer);
		ArgumentNullException.ThrowIfNull(ownerForm);

		IWin32DialogOwnerProvider dialogOwnerProvider =
			_shellScope.ServiceProvider.GetRequiredService<IWin32DialogOwnerProvider>();
		dialogOwnerProvider.SetOwner(ownerForm);

		if (!hostContainer.Controls.Contains(_host))
			hostContainer.Controls.Add(_host);

		_host.BringToFront();
	}

	public void NotifyHostTabActivated()
		=> _viewModel.NotifyHostTabActivated();

	public void NotifyMainWindowFocusChanged(bool isFocused)
		=> _viewModel.NotifyMainWindowFocusChanged(isFocused);
}
