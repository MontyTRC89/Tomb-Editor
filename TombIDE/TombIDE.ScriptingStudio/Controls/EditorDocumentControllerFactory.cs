#nullable enable

using System;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombIDE.Shared.Messaging.Scripting;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.Controls;

/// <summary>
/// Default implementation of <see cref="IEditorDocumentControllerFactory"/>.
/// Creates an <see cref="EditorDocumentController"/> and registers editors
/// from the workspace profile.
/// </summary>
internal sealed class EditorDocumentControllerFactory : IEditorDocumentControllerFactory
{
	public IEditorDocumentController Create(
		ScriptingWorkspaceProfile profile,
		IScriptingProjectContext projectContext,
		IMessageService messageService,
		IClassicScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(profile);
		ArgumentNullException.ThrowIfNull(projectContext);
		ArgumentNullException.ThrowIfNull(messageService);
		ArgumentNullException.ThrowIfNull(lineService);

		var controller = new EditorDocumentController(
			projectContext.Project.GetCurrentEngineVersion(),
			projectContext.ScriptRootDirectoryPath,
			messageService,
			lineService);

		profile.RegisterEditors(controller);

		return controller;
	}
}
