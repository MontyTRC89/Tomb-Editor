using System;
using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Lua.Utils;

namespace TombLib.Test;

[TestClass]
public class LuaEditorInteractionRulesTests
{
	[TestMethod]
	public void IsValidAutocompleteContext_AllowsMemberTriggerInCode()
	{
		var document = CreateDocument("player.");

		bool result = LuaEditorInteractionRules.IsValidAutocompleteContext(document, document.TextLength, '.');

		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsValidAutocompleteContext_BlocksIdentifierImmediatelyAfterDot()
	{
		var document = CreateDocument("player.a");

		bool result = LuaEditorInteractionRules.IsValidAutocompleteContext(document, document.TextLength, null);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidManualCompletionContext_BlocksCommentText()
	{
		var document = CreateDocument("-- player");

		bool result = LuaEditorInteractionRules.IsValidManualCompletionContext(document, document.TextLength);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidManualCompletionContext_BlocksOpenString()
	{
		var document = CreateDocument("print(\"player");

		bool result = LuaEditorInteractionRules.IsValidManualCompletionContext(document, document.TextLength);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidManualCompletionContext_BlocksOpenLongComment()
	{
		var document = CreateDocument("--[[ player");

		bool result = LuaEditorInteractionRules.IsValidManualCompletionContext(document, document.TextLength);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsValidManualCompletionContext_BlocksOpenLongString()
	{
		var document = CreateDocument("value = [[player");

		bool result = LuaEditorInteractionRules.IsValidManualCompletionContext(document, document.TextLength);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanRequestHover_ReturnsFalseWhenCompletionWindowIsOpen()
	{
		var document = CreateDocument("player");

		bool result = LuaEditorInteractionRules.CanRequestHover(document, document.TextLength, true, false);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void CanRequestHover_BlocksCommentText()
	{
		var document = CreateDocument("value = 1 -- hover target");

		bool result = LuaEditorInteractionRules.CanRequestHover(document, document.TextLength, false, false);

		Assert.IsFalse(result);
	}

	[TestMethod]
	public void TryGetDefinitionStartOffset_ReturnsWordStartFromInsideIdentifier()
	{
		const string identifier = "targetValue";
		string text = "local " + identifier + " = 1";
		var document = CreateDocument(text);
		int identifierStart = text.IndexOf(identifier, StringComparison.Ordinal);
		int probeOffset = identifierStart + 4;

		bool result = LuaEditorInteractionRules.TryGetDefinitionStartOffset(document, probeOffset, out int definitionOffset);

		Assert.IsTrue(result);
		Assert.AreEqual(identifierStart, definitionOffset);
	}

	[TestMethod]
	public void TryGetDefinitionStartOffset_ReturnsWordStartWhenCaretIsAfterIdentifier()
	{
		const string identifier = "targetValue";
		string text = "return " + identifier;
		var document = CreateDocument(text);
		int identifierStart = text.IndexOf(identifier, StringComparison.Ordinal);
		int probeOffset = identifierStart + identifier.Length;

		bool result = LuaEditorInteractionRules.TryGetDefinitionStartOffset(document, probeOffset, out int definitionOffset);

		Assert.IsTrue(result);
		Assert.AreEqual(identifierStart, definitionOffset);
	}

	[TestMethod]
	public void TryGetDefinitionStartOffset_BlocksCommentText()
	{
		var document = CreateDocument("-- targetValue");

		bool result = LuaEditorInteractionRules.TryGetDefinitionStartOffset(document, document.TextLength, out _);

		Assert.IsFalse(result);
	}

	private static TextDocument CreateDocument(params string[] lines)
		=> new(string.Join(Environment.NewLine, lines));
}