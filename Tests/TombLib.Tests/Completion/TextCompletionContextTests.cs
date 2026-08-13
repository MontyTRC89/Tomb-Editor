using Nickelony.LanguageServer.Abstractions.Completion;
using System.Collections.Generic;
using TombLib.Scripting.ClassicScript.Completion;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.Completion;

namespace TombLib.Tests.Completion;

/// <summary>
/// Tests for the <see cref="TextCompletionContext"/> validation rules and the
/// ClassicScript provider handling of the <c>-1</c> argument-index sentinel.
/// </summary>
[TestClass]
public class TextCompletionContextTests
{
    private static ITextCompletionProvider CreateClassicScriptProvider()
    {
        var lineService = new ClassicScriptLineService();
        var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
        var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
        var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);

        return new ClassicScriptCompletionProvider(commandService, mnemonicCatalogService);
    }

    [TestMethod]
    public void Default_Context_IsNull_NotValid()
    {
        TextCompletionContext? context = default;

        Assert.IsNull(context);
    }

    [TestMethod]
    public void Constructor_NullDocumentText_Throws()
        => Assert.ThrowsException<ArgumentNullException>(() => new TextCompletionContext(null!, 0));

    [TestMethod]
    public void Constructor_NegativeCaretOffset_Throws()
        => Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TextCompletionContext("text", -1));

    [TestMethod]
    public void Constructor_CaretOffsetBeyondTextLength_Throws()
        => Assert.ThrowsException<ArgumentOutOfRangeException>(() => new TextCompletionContext("text", 5));

    [TestMethod]
    public void Constructor_ArgumentIndexLessThanMinusOne_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => new TextCompletionContext("text", 0, TextCompletionTrigger.Contextual, -2));
    }

    [TestMethod]
    public void Constructor_MinusOneArgumentIndex_IsValidSentinel()
    {
        var context = new TextCompletionContext("text", 0, TextCompletionTrigger.Contextual, -1);

        Assert.AreEqual(-1, context.ArgumentIndex);
    }

    [TestMethod]
    public void Constructor_ZeroArgumentIndex_IsFirstArgument()
    {
        var context = new TextCompletionContext("text", 0, TextCompletionTrigger.Contextual, 0);

        Assert.AreEqual(0, context.ArgumentIndex);
    }

    [TestMethod]
    public void ClassicScriptProvider_MinusOneSentinel_ResolvesArgumentFromCaret()
    {
        ITextCompletionProvider provider = CreateClassicScriptProvider();

        // The caret sits after the comma, in the second argument, so the index resolved
        // from the caret is beyond the single-argument syntax and yields no items.
        IReadOnlyList<TextCompletionItem> items = provider.GetCompletionItems(
            new TextCompletionContext("Horizon= ENABLED, ", 18, TextCompletionTrigger.Contextual, -1));

        Assert.AreEqual(0, items.Count);
    }

    [TestMethod]
    public void ClassicScriptProvider_ZeroArgumentIndex_DoesNotResolveFromCaret()
    {
        ITextCompletionProvider provider = CreateClassicScriptProvider();

        // A forced index of 0 treats the caret as being in the first argument even though
        // it is past a comma, so the first argument's ENABLED/DISABLED items are offered.
        IReadOnlyList<TextCompletionItem> items = provider.GetCompletionItems(
            new TextCompletionContext("Horizon= ENABLED, ", 18, TextCompletionTrigger.Contextual, 0));

        Assert.IsTrue(items.Any(item => item.Label == "ENABLED"));
        Assert.IsTrue(items.Any(item => item.Label == "DISABLED"));
    }

    [TestMethod]
    public void ClassicScriptProvider_NullContext_Throws()
    {
        ITextCompletionProvider provider = CreateClassicScriptProvider();

        Assert.ThrowsException<ArgumentNullException>(() => provider.GetCompletionItems(null!));
    }
}
