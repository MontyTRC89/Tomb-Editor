#nullable enable

using Microsoft.Extensions.DependencyInjection;
using System;
using TombIDE.ScriptingStudio.Host;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared;
using TombIDE.Shared.Messaging.Scripting;

namespace TombIDE.ScriptingStudio.Composition;

/// <summary>
/// Creates hostable ScriptingStudio shell instances.
/// This is the only temporary Scripting Studio constructor that accepts <see cref="IDE"/>;
/// it must not pass IDE beyond this boundary.
/// </summary>
public interface IScriptingStudioShellFactory
{
	/// <summary>
	/// Creates a shell instance for the supplied IDE context.
	/// </summary>
	/// <param name="ide">The current TombIDE instance.</param>
	/// <returns>A hostable shell instance.</returns>
	IScriptingStudioShell Create(IDE ide);
}

internal sealed class ScriptingStudioShellFactory : IScriptingStudioShellFactory
{
	private readonly IServiceProvider _serviceProvider;

	public ScriptingStudioShellFactory(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public IScriptingStudioShell Create(IDE ide)
	{
		ArgumentNullException.ThrowIfNull(ide);

		IServiceScope shellScope = _serviceProvider.CreateScope();

		try
		{
			IScriptingProjectContext projectContext = new IdeScriptingProjectContext(ide);
			ScriptingStudioLegacySettingsSnapshot legacySnapshot =
				ScriptingStudioLegacySettingsImport.CreateSnapshot(ide.IDEConfiguration);

			ScriptingStudioShellContext context =
				shellScope.ServiceProvider.GetRequiredService<ScriptingStudioShellContext>();
			context.Initialize(projectContext, legacySnapshot);

			RootShellViewModel viewModel =
				shellScope.ServiceProvider.GetRequiredService<RootShellViewModel>();

			return new ScriptingStudioShell(shellScope, viewModel);
		}
		catch
		{
			shellScope.Dispose();
			throw;
		}
	}
}
