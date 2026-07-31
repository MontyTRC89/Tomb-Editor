using System;
using TombIDE.ScriptingStudio.CommandSurface;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.ScriptingStudio.UI;
using Nickelony.LanguageServer.Core.Editing;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.Lua;

internal sealed class LuaDocumentCommandStatusProvider : IStudioDocumentCommandStatusProvider
{
	private readonly ITextFormattingProvider _formattingProvider;
	private readonly LuaReferenceSearchService _referenceSearchService;
	private readonly TextWorkspaceCommandService _workspaceCommandService;

	public LuaDocumentCommandStatusProvider(
		ITextFormattingProvider formattingProvider,
		LuaReferenceSearchService referenceSearchService,
		TextWorkspaceCommandService workspaceCommandService)
	{
		_formattingProvider = formattingProvider ?? throw new ArgumentNullException(nameof(formattingProvider));
		_referenceSearchService = referenceSearchService ?? throw new ArgumentNullException(nameof(referenceSearchService));
		_workspaceCommandService = workspaceCommandService ?? throw new ArgumentNullException(nameof(workspaceCommandService));
	}

	public bool TryGetEnabled(IEditorControl editor, UICommand command, out bool isEnabled)
	{
		bool hasTextEditor = editor is TextEditorBase;
		bool hasLuaEditor = editor is LuaEditor;

		switch (command)
		{
			case UICommand.GoToDefinition:
				isEnabled = hasLuaEditor;
				return true;

			case UICommand.FindReferences:
				isEnabled = hasLuaEditor && _referenceSearchService.SupportsReferences;
				return true;

			case UICommand.RenameSymbol:
				isEnabled = hasLuaEditor && _workspaceCommandService.SupportsRename;
				return true;

			case UICommand.Reindent:
				isEnabled = hasLuaEditor ? _formattingProvider.SupportsFormatting : hasTextEditor;
				return true;

			default:
				isEnabled = false;
				return false;
		}
	}
}
