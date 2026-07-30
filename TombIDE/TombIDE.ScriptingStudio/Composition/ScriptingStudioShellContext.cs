#nullable enable

using System;
using TombIDE.ScriptingStudio.Settings;
using TombIDE.Shared.Messaging.Scripting;

namespace TombIDE.ScriptingStudio.Composition;

/// <summary>
/// Scoped shell-input bridge initialized exactly once by the shell factory
/// from the IDE-derived project context. Resolving before initialization or
/// initializing twice fails deterministically.
/// </summary>
internal sealed class ScriptingStudioShellContext
{
	private IScriptingProjectContext? _projectContext;
	private ScriptingStudioLegacySettingsSnapshot? _legacySettingsSnapshot;
	private bool _isInitialized;

	/// <summary>
	/// Gets the project context for the current shell scope.
	/// Throws <see cref="InvalidOperationException"/> if the context has not been initialized.
	/// </summary>
	public IScriptingProjectContext ProjectContext
	{
		get
		{
			if (!_isInitialized || _projectContext is null)
				throw new InvalidOperationException(
					"ScriptingStudioShellContext has not been initialized. " +
					"The shell factory must initialize the context before any scoped service resolves it.");

			return _projectContext;
		}
	}

	/// <summary>
	/// Gets the legacy settings snapshot for the current shell scope.
	/// Throws <see cref="InvalidOperationException"/> if the context has not been initialized.
	/// </summary>
	public ScriptingStudioLegacySettingsSnapshot LegacySettingsSnapshot
	{
		get
		{
			if (!_isInitialized || _legacySettingsSnapshot is null)
				throw new InvalidOperationException(
					"ScriptingStudioShellContext has not been initialized. " +
					"The shell factory must initialize the context before any scoped service resolves it.");

			return _legacySettingsSnapshot;
		}
	}

	/// <summary>
	/// Initializes the context exactly once with the given project context and legacy settings.
	/// Throws <see cref="InvalidOperationException"/> if already initialized.
	/// </summary>
	public void Initialize(
		IScriptingProjectContext projectContext,
		ScriptingStudioLegacySettingsSnapshot legacySettingsSnapshot)
	{
		ArgumentNullException.ThrowIfNull(projectContext);
		ArgumentNullException.ThrowIfNull(legacySettingsSnapshot);

		if (_isInitialized)
			throw new InvalidOperationException(
				"ScriptingStudioShellContext has already been initialized. " +
				"Each shell scope must have its own context and must not be re-initialized.");

		_projectContext = projectContext;
		_legacySettingsSnapshot = legacySettingsSnapshot;
		_isInitialized = true;
	}
}
