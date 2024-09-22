using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TombLib.Scripting.Lua;

/// <summary>
/// Represents the type of a Lua element.
/// </summary>
public enum LuaElementType
{
	Function,
	LocalVariable,
	GlobalVariable
}

/// <summary>
/// Represents a Lua element with its name, type, scope, and data type.
/// </summary>
public readonly record struct LuaElement(string Name, LuaElementType ElementType, string Scope);

/// <summary>
/// Represents the offset and length of a scope.
/// </summary>
public readonly record struct ScopeInfo(int Offset, int Length);

/// <summary>
/// Parses Lua code and extracts functions and variables.
/// </summary>
public sealed class LuaParser
{
	private const string ScopeDelimiter = "::";

	private static readonly List<string> SegmentationKeywords = new()
	{
		"if", "elseif", "else", "end", "for", "while", "repeat", "until", "do", "then"
	};

	/// <summary>
	/// Gets the list of functions parsed from the Lua code.
	/// </summary>
	public IReadOnlyList<LuaElement> Functions => _functions.AsReadOnly();

	/// <summary>
	/// Gets the list of variables parsed from the Lua code.
	/// </summary>
	public IReadOnlyList<LuaElement> Variables => _variables.AsReadOnly();

	private readonly List<LuaElement> _functions;
	private readonly List<LuaElement> _variables;
	private readonly Dictionary<string, ScopeInfo> _scopes;
	private int _conditionalScopeCounter;
	private int _loopScopeCounter;
	private int _blockScopeCounter;

	private static readonly Regex SingleLineCommentRegex = new(@"--.*", RegexOptions.Compiled);
	private static readonly Regex FunctionRegex = new(@"function\s+([\w_]+)", RegexOptions.Compiled);
	private static readonly Regex GlobalVarRegex = new(@"^([\w_]+)\s*=\s*(.+)$", RegexOptions.Compiled);
	private static readonly Regex LocalVarDeclRegex = new(@"local\s+([\w_]+)\s*(?:=\s*(.+))?", RegexOptions.Compiled);
	private static readonly Regex IfRegex = new(@"\bif\b", RegexOptions.Compiled);
	private static readonly Regex ElseIfRegex = new(@"\belseif\b", RegexOptions.Compiled);
	private static readonly Regex ElseRegex = new(@"\belse\b", RegexOptions.Compiled);
	private static readonly Regex EndRegex = new(@"\bend\b", RegexOptions.Compiled);
	private static readonly Regex ForRegex = new(@"\bfor\b", RegexOptions.Compiled);
	private static readonly Regex WhileRegex = new(@"\bwhile\b", RegexOptions.Compiled);
	private static readonly Regex RepeatRegex = new(@"\brepeat\b", RegexOptions.Compiled);
	private static readonly Regex UntilRegex = new(@"\buntil\b", RegexOptions.Compiled);
	private static readonly Regex DoRegex = new(@"\bdo\b", RegexOptions.Compiled);

	public LuaParser()
	{
		_functions = new List<LuaElement>();
		_variables = new List<LuaElement>();
		_scopes = new Dictionary<string, ScopeInfo>();
		_conditionalScopeCounter = 0;
		_loopScopeCounter = 0;
		_blockScopeCounter = 0;
	}

	/// <summary>
	/// Parses the given Lua content.
	/// </summary>
	/// <param name="content">The Lua content to parse.</param>
	public void ParseLuaContent(string content)
	{
		// Clear previous results
		ClearPreviousResults();

		// Stack to keep track of scopes
		Stack<string> scopeStack = new();
		scopeStack.Push("global");

		// Replace multi-line comments with spaces
		MatchCollection multiLineCommentMatches = Regex.Matches(content, @"--\[\[(.|[\r\n])*?\]\]");

		foreach (Match match in multiLineCommentMatches)
			content = content.Replace(match.Value, " ".PadRight(match.Length));

		string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

		int currentOffset = 0;

		foreach (string line in lines)
			ProcessLine(line, ref currentOffset, scopeStack);
	}

	private void ClearPreviousResults()
	{
		_functions.Clear();
		_variables.Clear();
		_scopes.Clear();
		_conditionalScopeCounter = 0;
		_loopScopeCounter = 0;
		_blockScopeCounter = 0;
	}

	private void ProcessLine(string line, ref int currentOffset, Stack<string> scopeStack)
	{
		// Replace single-line comments with spaces
		line = SingleLineCommentRegex.Replace(line, " ".PadRight(line.Length));

		int lineStartOffset = currentOffset;
		currentOffset += line.Length + 2;

		// Split the line into segments based on Lua keywords
		string[] segments = Regex.Split(line, $@"\b({string.Join("|", SegmentationKeywords)})\b");

		foreach (string segment in segments)
		{
			string trimmedSegment = segment.Trim();

			if (string.IsNullOrEmpty(trimmedSegment))
				continue;

			if (TryProcessFunctionDefinition(trimmedSegment, scopeStack, lineStartOffset)) continue;
			if (TryProcessEndOfScope(trimmedSegment, scopeStack, currentOffset)) continue;
			if (TryProcessLocalVariableDeclaration(trimmedSegment, scopeStack)) continue;
			if (TryProcessGlobalVariableAssignment(trimmedSegment, scopeStack)) continue;
			if (TryProcessConditionalStatement(trimmedSegment, scopeStack, lineStartOffset)) continue;
			if (TryProcessLoopStatement(trimmedSegment, scopeStack, lineStartOffset)) continue;
			if (TryProcessDoBlock(trimmedSegment, scopeStack, lineStartOffset)) continue;
			if (TryProcessUntilStatement(trimmedSegment, scopeStack, currentOffset)) continue;
		}
	}

	private bool TryProcessFunctionDefinition(string line, Stack<string> scopeStack, int currentOffset)
	{
		Match functionMatch = FunctionRegex.Match(line);

		if (functionMatch.Success)
		{
			string functionName = functionMatch.Groups[1].Value;
			scopeStack.Push(functionName);

			string currentScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_functions.Add(new LuaElement(functionName, LuaElementType.Function, currentScope));
			_scopes[currentScope] = new ScopeInfo(currentOffset, 0);

			return true;
		}

		return false;
	}

	private bool TryProcessEndOfScope(string line, Stack<string> scopeStack, int currentOffset)
	{
		if (EndRegex.IsMatch(line))
		{
			if (scopeStack.Count > 1)
			{
				string scope = string.Join(ScopeDelimiter, scopeStack.Reverse());
				scopeStack.Pop();

				_scopes[scope] = new ScopeInfo(_scopes[scope].Offset, currentOffset - _scopes[scope].Offset);
			}

			return true;
		}

		return false;
	}

	private bool TryProcessLocalVariableDeclaration(string line, Stack<string> scopeStack)
	{
		Match localVarDeclMatch = LocalVarDeclRegex.Match(line);

		if (localVarDeclMatch.Success)
		{
			string varName = localVarDeclMatch.Groups[1].Value;
			string currentScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_variables.Add(new LuaElement(varName, LuaElementType.LocalVariable, currentScope));

			return true;
		}

		return false;
	}

	private bool TryProcessGlobalVariableAssignment(string line, Stack<string> scopeStack)
	{
		Match globalVarMatch = GlobalVarRegex.Match(line);

		if (globalVarMatch.Success)
		{
			string varName = globalVarMatch.Groups[1].Value;

			if (varName.Contains('.'))
				return false;

			string currentScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_variables.Add(new LuaElement(varName, LuaElementType.GlobalVariable, currentScope));

			return true;
		}

		return false;
	}

	private bool TryProcessConditionalStatement(string line, Stack<string> scopeStack, int startOffset)
	{
		if (IfRegex.IsMatch(line))
		{
			scopeStack.Push($"if_{_conditionalScopeCounter++}");

			string conditionalScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_scopes[conditionalScope] = new ScopeInfo(startOffset, 0);

			return true;
		}

		if (ElseIfRegex.IsMatch(line))
		{
			if (scopeStack.Count > 1)
			{
				string scope = string.Join(ScopeDelimiter, scopeStack.Reverse());
				scopeStack.Pop();

				_scopes[scope] = new ScopeInfo(_scopes[scope].Offset, startOffset - _scopes[scope].Offset);
			}

			scopeStack.Push($"elseif_{_conditionalScopeCounter++}");

			string conditionalScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_scopes[conditionalScope] = new ScopeInfo(startOffset, 0);

			return true;
		}

		if (ElseRegex.IsMatch(line))
		{
			if (scopeStack.Count > 1)
			{
				string scope = string.Join(ScopeDelimiter, scopeStack.Reverse());
				scopeStack.Pop();

				_scopes[scope] = new ScopeInfo(_scopes[scope].Offset, startOffset - _scopes[scope].Offset);
			}

			scopeStack.Push($"else_{_conditionalScopeCounter++}");

			string conditionalScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_scopes[conditionalScope] = new ScopeInfo(startOffset, 0);

			return true;
		}

		return false;
	}

	private bool TryProcessLoopStatement(string line, Stack<string> scopeStack, int startOffset)
	{
		if (ForRegex.IsMatch(line) || WhileRegex.IsMatch(line) || RepeatRegex.IsMatch(line))
		{
			scopeStack.Push($"loop_{_loopScopeCounter++}");

			string loopScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_scopes[loopScope] = new ScopeInfo(startOffset, 0);

			return true;
		}

		return false;
	}

	private bool TryProcessDoBlock(string line, Stack<string> scopeStack, int startOffset)
	{
		if (DoRegex.IsMatch(line))
		{
			scopeStack.Push($"block_{_blockScopeCounter++}");

			string blockScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_scopes[blockScope] = new ScopeInfo(startOffset, 0);

			return true;
		}

		return false;
	}

	private bool TryProcessUntilStatement(string line, Stack<string> scopeStack, int endOffset)
	{
		if (UntilRegex.IsMatch(line))
		{
			if (scopeStack.Count > 1)
			{
				string scope = string.Join(ScopeDelimiter, scopeStack.Reverse());
				scopeStack.Pop();

				_scopes[scope] = new ScopeInfo(_scopes[scope].Offset, endOffset - _scopes[scope].Offset);
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Gets the scope path from the given offset.
	/// </summary>
	public string GetScopeFromOffset(int offset) => _scopes
		.Where(x => x.Value.Offset <= offset)
		.OrderByDescending(x => x.Value.Offset)
		.FirstOrDefault(x => offset <= x.Value.Offset + x.Value.Length).Key;

	/// <summary>
	/// Gets the autocomplete items within the specified scope.
	/// </summary>
	/// <param name="scope">The scope to retrieve autocomplete items for.</param>
	/// <returns>The list of autocomplete items within the specified scope.</returns>
	public IReadOnlyList<LuaElement> GetAutocompleteItems(string scope)
	{
		List<LuaElement> autocompleteItems = new();

		// Fetch all items within the range of the given scope
		foreach (LuaElement element in _functions.Concat(_variables))
		{
			if (scope.StartsWith(element.Scope))
			{
				autocompleteItems.Add(element);
			}
		}

		return autocompleteItems.AsReadOnly();
	}
}
