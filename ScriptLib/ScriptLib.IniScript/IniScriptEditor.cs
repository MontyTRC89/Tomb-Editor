using ICSharpCode.AvalonEdit.Rendering;
using ScriptLib.Core;
using ScriptLib.IniScript.Renderers;

namespace ScriptLib.IniScript;

public sealed class IniScriptEditor : TextEditorBase
{
	public override string DefaultFileExtension => ".txt";
	public override string CommentPrefix => ";";

	private bool _showSectionSeparators;
	public bool ShowSectionSeparators
	{
		get => _showSectionSeparators;
		set
		{
			_showSectionSeparators = value;

			if (_showSectionSeparators)
			{
				if (!TextArea.TextView.BackgroundRenderers.Contains(_sectionRenderer))
					TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
			}
			else
			{
				if (TextArea.TextView.BackgroundRenderers.Contains(_sectionRenderer))
					TextArea.TextView.BackgroundRenderers.Remove(_sectionRenderer);
			}

			TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		}
	}

	private readonly IBackgroundRenderer _sectionRenderer;

	public IniScriptEditor(Version gameEngineVersion, string? filePath, bool isSilentSession = false)
		: base(gameEngineVersion, filePath, isSilentSession)
	{
		_sectionRenderer = new SectionRenderer(this);

		if (ShowSectionSeparators)
			TextArea.TextView.BackgroundRenderers.Add(_sectionRenderer);
	}

	public override void GoToObject(string objectNamePattern, bool isCaseSensitive, bool isExactMatch, bool isRegex) => throw new NotImplementedException();
}
