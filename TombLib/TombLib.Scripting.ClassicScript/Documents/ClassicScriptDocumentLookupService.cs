using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Documents;

/// <summary>
/// Boundary facade over <see cref="IClassicScriptCommandService"/> that answers
/// document-level lookups (level-script existence, language-string existence and
/// include-file resolution) on behalf of host consumers such as TombIDE.
/// Retained as a thin abstraction so host consumers do not depend on the command
/// service surface directly; it is consumed by TombIDE and tests.
/// </summary>
public sealed class ClassicScriptDocumentLookupService
{
	private readonly IClassicScriptCommandService _commandService;

	public ClassicScriptDocumentLookupService(IClassicScriptCommandService commandService)
	{
		ArgumentNullException.ThrowIfNull(commandService);
		_commandService = commandService;
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
