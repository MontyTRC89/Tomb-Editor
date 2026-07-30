using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Documents;

public sealed class ClassicScriptDocumentLookupService
{
	private readonly IClassicScriptCommandService _commandService;

	public ClassicScriptDocumentLookupService(IClassicScriptCommandService commandService)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
	}

	public bool IsLevelScriptDefined(ITextSnapshot source, string levelName)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(levelName);

		return _commandService.IsLevelScriptDefined(source, levelName);
	}

	public bool IsLevelLanguageStringDefined(ITextSnapshot source, string levelName)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(levelName);

		return _commandService.IsLevelLanguageStringDefined(source, levelName);
	}

	public string? TryGetIncludeFilePath(ITextSnapshot source, int offset)
	{
		ArgumentNullException.ThrowIfNull(source);

		string? includeFilePath = _commandService.GetFullIncludePath(source, offset);
		return string.IsNullOrWhiteSpace(includeFilePath) ? null : includeFilePath;
	}
}
