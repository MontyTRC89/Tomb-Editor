using TombLib.Scripting.Lua.Objects;

namespace TombLib.Test;

[TestClass]
public class LuaCompletionItemTests
{
	[TestMethod]
	public async Task WithRequestContext_PreservesRequestMetadataAcrossResolve()
	{
		var item = new LuaCompletionItem(
			"spawn",
			insertText: "spawn",
			resolveAsync: _ => Task.FromResult(new LuaCompletionItem("spawn", detail: "function", insertCaretOffset: 2)),
			insertCaretOffset: 2)
			.WithRequestContext(4, 7);

		LuaCompletionItem resolvedItem = await item.ResolveAsync();

		Assert.AreEqual(4, resolvedItem.RequestDocumentVersion);
		Assert.AreEqual(7, resolvedItem.RequestGeneration);
		Assert.AreEqual("function", resolvedItem.Detail);
		Assert.AreEqual(2, resolvedItem.InsertCaretOffset);
	}

	[TestMethod]
	public async Task WithFilteredCommitContext_DropsTextEditAndPreservesResolveMetadata()
	{
		LuaCompletionTextEdit textEdit = new(
			new LuaCompletionRange(new LuaCompletionPosition(0, 2), new LuaCompletionPosition(0, 5)));

		var item = new LuaCompletionItem(
			"Color",
			insertText: "Color",
			textEdit: textEdit,
			resolveAsync: _ => Task.FromResult(new LuaCompletionItem("Color", detail: "enum", textEdit: textEdit)))
			.WithFilteredCommitContext(6, 2);

		Assert.AreEqual(6, item.RequestDocumentVersion);
		Assert.AreEqual(2, item.RequestGeneration);
		Assert.IsNull(item.TextEdit);

		LuaCompletionItem resolvedItem = await item.ResolveAsync();

		Assert.AreEqual("enum", resolvedItem.Detail);
		Assert.IsNull(resolvedItem.TextEdit);
		Assert.AreEqual(6, resolvedItem.RequestDocumentVersion);
		Assert.AreEqual(2, resolvedItem.RequestGeneration);
	}
}