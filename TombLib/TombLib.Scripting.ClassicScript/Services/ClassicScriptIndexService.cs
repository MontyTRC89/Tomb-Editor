using NCalc;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.Extensions;
using TombLib.Scripting.Text;

namespace TombLib.Scripting.ClassicScript.Services;

/// <summary>
/// Default implementation of <see cref="IClassicScriptIndexService"/>.
/// Uses a per-call <see cref="EvaluationContext"/> and CoreCLR-NCalc for expression evaluation.
/// </summary>
public class ClassicScriptIndexService : IClassicScriptIndexService
{
    private readonly IClassicScriptCommandService _commandService;
    private readonly IClassicScriptLineService _lineService;
    private readonly ClassicScriptMnemonicCatalogService _mnemonicCatalogService;

    // Commands that support sequential free-index discovery.
    private static readonly string[] IndexedCommands =
    [
        "AddEffect", "ColorRGB", "GlobalTrigger", "Image", "ItemGroup",
        "MultiEnvCondition", "Organizer", "Parameters", "TestPosition",
        "TriggerGroup", "Plugin"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassicScriptIndexService"/> class.
    /// </summary>
    public ClassicScriptIndexService(
        IClassicScriptCommandService commandService,
        IClassicScriptLineService lineService,
        ClassicScriptMnemonicCatalogService mnemonicCatalogService)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _lineService = lineService ?? throw new ArgumentNullException(nameof(lineService));
        _mnemonicCatalogService = mnemonicCatalogService ?? throw new ArgumentNullException(nameof(mnemonicCatalogService));
    }

    /// <inheritdoc />
    public int GetNextFreeIndex(ITextSnapshot source, int offset)
    {
        string? commandKey = _commandService.GetCommandKey(source, offset);
        return GetNextFreeIndex(source, offset, commandKey);
    }

    /// <inheritdoc />
    public int GetNextFreeIndex(ITextSnapshot source, int offset, string? commandKey)
    {
        if (string.IsNullOrEmpty(commandKey))
            return -1;

        if (!commandKey.IgnoreCaseEqualsAny(IndexedCommands))
            return -1;

        int loopStartLine = 1;

        if (_commandService.DocumentContainsSections(source))
        {
            int? sectionStartLineNumber = _commandService.GetStartLineOfCurrentSection(source, offset);

            if (sectionStartLineNumber is null)
                return -1;

            loopStartLine = sectionStartLineNumber.Value + 1;
        }

        var context = new EvaluationContext();
        int firstId = GetFirstId(source, commandKey, loopStartLine, context);
        IEnumerable<int> takenIndicesList = GetTakenIndicesList(source, commandKey, loopStartLine);

        int nextFreeIndex = firstId;

        while (takenIndicesList.Contains(nextFreeIndex))
            nextFreeIndex++;

        return nextFreeIndex;
    }

    // ------------------------------------------------------------------
    // #FIRST_ID evaluation
    // ------------------------------------------------------------------

    private int GetFirstId(ITextSnapshot source, string commandKey, int loopStartLine, EvaluationContext context)
    {
        int result = 0;
        var firstIdRegex = new Regex(
            $@"^\s*#FIRST_ID\s+{Regex.Escape(commandKey)}\s*=\s*(.*)\s*(;.*)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        try
        {
            for (int i = loopStartLine; i <= source.LineCount; i++)
            {
                ITextLine line = source.GetLineByNumber(i);
                string? lineText = _commandService.GetWholeCommandLineText(source, line.Offset);

                if (lineText is null)
                    continue;

                lineText = _lineService.EscapeCommentsAndNewLines(lineText);
                Match match = firstIdRegex.Match(lineText);

                if (match.Success)
                {
                    string expressionString = match.Groups[1].Value;
                    expressionString = ResolveExpressionVariables(source, expressionString, context);
                    result = EvaluateExpression(expressionString);
                }
            }
        }
        catch
        {
            // Legacy behavior: silently swallow evaluation errors.
        }

        return result < 1 ? 1 : result;
    }

    // ------------------------------------------------------------------
    // Variable resolution
    // ------------------------------------------------------------------

    private string ResolveExpressionVariables(ITextSnapshot source, string expressionString, EvaluationContext context)
    {
        IEnumerable<string> variables = expressionString
            .Split('+', '-', '*', '/')
            .Select(x => x.Replace('(', ' ').Replace(')', ' ').Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x));

        foreach (string variable in variables)
        {
            if (int.TryParse(variable, out int _))
                continue;

            if (_mnemonicCatalogService.TryGetDecimalValue(variable, out int rowValue))
                expressionString = expressionString.Replace(variable, rowValue.ToString());
            else
                expressionString = expressionString.Replace(
                    variable,
                    GetVariableValue(source, variable, context).ToString());
        }

        return expressionString;
    }

    private int GetVariableValue(ITextSnapshot source, string variable, EvaluationContext context)
    {
        if (!context.VisitedVariables.Add(variable))
            return 0;

        int result = 0;
        var defineRegex = new Regex(
            $@"^\s*#DEFINE\s+{Regex.Escape(variable)}\s+(.*)\s*(;.*)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        foreach (ITextLine line in source.Lines)
        {
            string? lineText = _commandService.GetWholeCommandLineText(source, line.Offset);

            if (lineText is null)
                continue;

            lineText = _lineService.EscapeCommentsAndNewLines(lineText);
            Match match = defineRegex.Match(lineText);

            if (match.Success)
            {
                string expressionString = match.Groups[1].Value;
                expressionString = ResolveExpressionVariables(source, expressionString, context);
                result = EvaluateExpression(expressionString);
            }
        }

        return result;
    }

    // ------------------------------------------------------------------
    // NCalc expression evaluation
    // ------------------------------------------------------------------

    private static int EvaluateExpression(string expressionString)
    {
        var expression = new Expression(expressionString);

        if (expression.HasErrors())
            return 0;

        if (expressionString.Contains('/'))
            return expression.HasErrors() ? 0 : (int)Math.Floor((double)expression.Evaluate());
        else
            return expression.HasErrors() ? 0 : (int)expression.Evaluate();
    }

    // ------------------------------------------------------------------
    // Taken indices
    // ------------------------------------------------------------------

    private IEnumerable<int> GetTakenIndicesList(ITextSnapshot source, string commandKey, int loopStartLine)
    {
        for (int i = loopStartLine; i <= source.LineCount; i++)
        {
            ITextLine processedLine = source.GetLineByNumber(i);
            string processedLineText = source.GetText(processedLine.Offset, processedLine.Length);
            processedLineText = _lineService.EscapeComments(processedLineText);

            string? command = _commandService.GetCommandKey(source, processedLine.Offset);

            if (string.IsNullOrEmpty(command) || !processedLineText.Contains('='))
                continue;

            if (command.Equals(commandKey, StringComparison.OrdinalIgnoreCase))
                if (int.TryParse(processedLineText.Split('=')[1].Split(',')[0].Trim(), out int takenIndex))
                    yield return takenIndex;
        }
    }

    // ------------------------------------------------------------------
    // Per-call context for cyclic define detection
    // ------------------------------------------------------------------

    /// <summary>
    /// Holds per-call state for #DEFINE variable resolution to detect cycles.
    /// Replaces the legacy static <c>_alreadyVisitedVariables</c> stack.
    /// Each call to <see cref="GetNextFreeIndex(ITextSnapshot, int, string?)"/>
    /// creates a fresh instance, eliminating mutable singleton state.
    /// </summary>
    private sealed class EvaluationContext
    {
        /// <summary>
        /// Gets the set of variable names already visited in the current resolution chain.
        /// Case-insensitive comparison matches legacy behavior.
        /// </summary>
        public HashSet<string> VisitedVariables { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
