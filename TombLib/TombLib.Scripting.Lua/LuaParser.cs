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

	/// <summary>
	/// Gets the list of functions parsed from the Lua code.
	/// </summary>
	public IReadOnlyList<LuaElement> Functions => _functions.AsReadOnly();

	/// <summary>
	/// Gets the list of local variables parsed from the Lua code.
	/// </summary>
	public IReadOnlyList<LuaElement> LocalVariables => _localVariables.AsReadOnly();

	/// <summary>
	/// Gets the set of global variables parsed from the Lua code.
	/// </summary>
	public IReadOnlySet<LuaElement> GlobalVariables => _globalVariables;

	private readonly List<LuaElement> _functions;
	private readonly List<LuaElement> _localVariables;
	private readonly HashSet<LuaElement> _globalVariables;
	private readonly Dictionary<string, ScopeInfo> _scopes;
	private int _conditionalScopeCounter;
	private int _loopScopeCounter;
	private int _blockScopeCounter;

	private static readonly Regex FunctionRegex = new(@"\bfunction\b\s+([\w_]+)", RegexOptions.Compiled);
	private static readonly Regex GlobalVarRegex = new(@"\b(?<!\.)([\w_]+)\s*=[^=]", RegexOptions.Compiled);
	private static readonly Regex LocalVarDeclRegex = new(@"\blocal\s+([a-zA-Z_]\w*)\s*=[^=]", RegexOptions.Compiled);
	private static readonly Regex ConditionalRegex = new(@"\b(if|elseif|else)\b", RegexOptions.Compiled);
	private static readonly Regex LoopRegex = new(@"\b(for|while|repeat)\b", RegexOptions.Compiled);
	private static readonly Regex DoRegex = new(@"\bdo\b", RegexOptions.Compiled);
	private static readonly Regex EndOrUntilRegex = new(@"\b(end|until)\b", RegexOptions.Compiled);

	public LuaParser()
	{
		_functions = new List<LuaElement>();
		_localVariables = new List<LuaElement>();
		_globalVariables = new HashSet<LuaElement>();
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
		ClearPreviousResults();

		Stack<string> scopeStack = new();
		scopeStack.Push("global");

		bool insideMultiLineComment = false;
		bool insideString = false;
		char stringDelimiter = '\0';

		for (int i = 0; i < content.Length; i++)
		{
			char currentChar = content[i];

			// Check if inside a multi-line comment
			if (insideMultiLineComment)
			{
				// Check if the multi-line comment ends
				if (content[i..].StartsWith("]]"))
				{
					insideMultiLineComment = false;
					i++;
				}

				continue;
			}

			// Check if inside a string
			if (insideString)
			{
				// Check if the string ends
				if (currentChar == stringDelimiter)
				{
					insideString = false;
				}

				continue;
			}

			// Check if it is a single-line comment
			if (currentChar == '-' && i + 1 < content.Length && content[i + 1] == '-')
			{
				// Check if it is a multi-line comment
				if (i + 2 < content.Length && content[i + 2] == '[' && content[i + 3] == '[')
				{
					insideMultiLineComment = true;
					i += 3;
				}
				else
				{
					// Skip until the end of the line
					while (i < content.Length && content[i] != '\n')
					{
						i++;
					}
				}

				continue;
			}

			// Check if it is a string delimiter
			if (currentChar is '"' or '\'')
			{
				insideString = true;
				stringDelimiter = currentChar;
				continue;
			}

			// Skip whitespace characters
			if (char.IsWhiteSpace(currentChar))
			{
				continue;
			}

			// Try to process different elements of the Lua code
			if (TryProcessFunctionDefinition(content, i, scopeStack)) continue;
			if (TryProcessEndOrUntil(content, i, scopeStack)) continue;
			if (TryProcessLocalVariableDeclaration(content, ref i, scopeStack)) continue;
			if (TryProcessGlobalVariableAssignment(content, ref i)) continue;
			if (TryProcessConditionalStatement(content, i, scopeStack)) continue;
			if (TryProcessLoopStatement(content, i, scopeStack)) continue;
			if (TryProcessDoBlock(content, i, scopeStack)) continue;
		}
	}

	private void ClearPreviousResults()
	{
		_functions.Clear();
		_localVariables.Clear();
		_globalVariables.Clear();
		_scopes.Clear();
		_conditionalScopeCounter = 0;
		_loopScopeCounter = 0;
		_blockScopeCounter = 0;
	}

	private bool TryProcessFunctionDefinition(string content, int currentIndex, Stack<string> scopeStack)
	{
		Match functionMatch = FunctionRegex.Match(content, currentIndex);

		if (functionMatch.Success && functionMatch.Index == currentIndex)
		{
			// Check if the previous character is definitely a word border
			if (currentIndex > 0 && !char.IsWhiteSpace(content[currentIndex - 1]))
			{
				return false;
			}

			string functionName = functionMatch.Groups[1].Value;
			scopeStack.Push(functionName);

			string currentScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_functions.Add(new LuaElement(functionName, LuaElementType.Function, currentScope));
			_scopes[currentScope] = new ScopeInfo(currentIndex, 0);

			return true;
		}

		return false;
	}

	private bool TryProcessEndOrUntil(string content, int currentIndex, Stack<string> scopeStack)
	{
		Match endOrUntilMatch = EndOrUntilRegex.Match(content, currentIndex);

		if (endOrUntilMatch.Success && endOrUntilMatch.Index == currentIndex)
		{
			// Check if the previous character is definitely a word border
			if (currentIndex > 0 && !char.IsWhiteSpace(content[currentIndex - 1]))
			{
				return false;
			}

			string keyword = endOrUntilMatch.Groups[1].Value;

			if (scopeStack.Count > 1)
			{
				string scope = string.Join(ScopeDelimiter, scopeStack.Reverse());
				scopeStack.Pop();

				int endIndex = currentIndex;

				if (keyword == "until")
				{
					// Find the end of the condition following "until"
					while (endIndex < content.Length && content[endIndex] != '\n' && content[endIndex] != ';')
					{
						endIndex++;
					}
				}

				_scopes[scope] = new ScopeInfo(_scopes[scope].Offset, endIndex - _scopes[scope].Offset);
			}

			return true;
		}

		return false;
	}

	private bool TryProcessLocalVariableDeclaration(string content, ref int currentIndex, Stack<string> scopeStack)
	{
		Match localVarDeclMatch = LocalVarDeclRegex.Match(content, currentIndex);

		if (localVarDeclMatch.Success && localVarDeclMatch.Index == currentIndex)
		{
			// Check if the previous character is definitely a word border
			if (currentIndex > 0 && !char.IsWhiteSpace(content[currentIndex - 1]))
			{
				return false;
			}

			string varName = localVarDeclMatch.Groups[1].Value;
			string currentScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_localVariables.Add(new LuaElement(varName, LuaElementType.LocalVariable, currentScope));

			currentIndex = localVarDeclMatch.Index + localVarDeclMatch.Length;

			return true;
		}

		return false;
	}

	private bool TryProcessGlobalVariableAssignment(string content, ref int currentIndex)
	{
		Match globalVarMatch = GlobalVarRegex.Match(content, currentIndex);

		if (globalVarMatch.Success && globalVarMatch.Index == currentIndex)
		{
			string varName = globalVarMatch.Groups[1].Value;
			_globalVariables.Add(new LuaElement(varName, LuaElementType.GlobalVariable, "global"));

			currentIndex = globalVarMatch.Index + globalVarMatch.Length;

			return true;
		}

		return false;
	}

	private bool TryProcessConditionalStatement(string content, int currentIndex, Stack<string> scopeStack)
	{
		Match conditionalMatch = ConditionalRegex.Match(content, currentIndex);

		if (conditionalMatch.Success && conditionalMatch.Index == currentIndex)
		{
			// Check if the previous character is definitely a word border
			if (currentIndex > 0 && !char.IsWhiteSpace(content[currentIndex - 1]))
			{
				return false;
			}

			string keyword = conditionalMatch.Groups[1].Value;

			if (keyword != "if" && scopeStack.Count > 1)
			{
				string scope = string.Join(ScopeDelimiter, scopeStack.Reverse());
				scopeStack.Pop();

				_scopes[scope] = new ScopeInfo(_scopes[scope].Offset, currentIndex - _scopes[scope].Offset);
			}

			scopeStack.Push($"{keyword}_{_conditionalScopeCounter++}");

			string conditionalScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_scopes[conditionalScope] = new ScopeInfo(currentIndex, 0);

			return true;
		}

		return false;
	}

	private bool TryProcessLoopStatement(string content, int currentIndex, Stack<string> scopeStack)
	{
		Match loopMatch = LoopRegex.Match(content, currentIndex);

		if (loopMatch.Success && loopMatch.Index == currentIndex)
		{
			// Check if the previous character is definitely a word border
			if (currentIndex > 0 && !char.IsWhiteSpace(content[currentIndex - 1]))
			{
				return false;
			}

			scopeStack.Push($"loop_{_loopScopeCounter++}");

			string loopScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_scopes[loopScope] = new ScopeInfo(currentIndex, 0);

			return true;
		}

		return false;
	}

	private bool TryProcessDoBlock(string content, int currentIndex, Stack<string> scopeStack)
	{
		Match doMatch = DoRegex.Match(content, currentIndex);

		if (doMatch.Success && doMatch.Index == currentIndex)
		{
			// Check if the previous character is definitely a word border
			if (currentIndex > 0 && !char.IsWhiteSpace(content[currentIndex - 1]))
			{
				return false;
			}

			scopeStack.Push($"block_{_blockScopeCounter++}");

			string blockScope = string.Join(ScopeDelimiter, scopeStack.Reverse());
			_scopes[blockScope] = new ScopeInfo(currentIndex, 0);

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
		.FirstOrDefault(x => offset <= x.Value.Offset + x.Value.Length).Key ?? "global";

	/// <summary>
	/// Gets the autocomplete items within the specified scope.
	/// </summary>
	/// <param name="scope">The scope to retrieve autocomplete items for.</param>
	/// <returns>The list of autocomplete items within the specified scope.</returns>
	public IReadOnlyList<LuaElement> GetAutocompleteItems(string scope)
	{
		List<LuaElement> autocompleteItems = new();

		// Fetch all items within the range of the given scope
		foreach (LuaElement element in _functions.Concat(_localVariables).Concat(_globalVariables))
		{
			if (scope.StartsWith(element.Scope))
			{
				autocompleteItems.Add(element);
			}
		}

		return autocompleteItems.AsReadOnly();
	}
}
