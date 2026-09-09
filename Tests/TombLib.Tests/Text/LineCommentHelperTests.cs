using TombLib.Scripting.Text;

namespace TombLib.Tests.Text;

/// <summary>
/// Tests for <see cref="LineCommentHelper"/> matching legacy ClassicScript line comment behavior.
/// </summary>
[TestClass]
public class LineCommentHelperTests
{
    private const string Delimiter = ";";

    // ---------------------------------------------------------------------------
    // FindCommentStart
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void FindCommentStart_LineWithComment_ReturnsWhitespaceBeforeSemicolon()
    {
        int result = LineCommentHelper.FindCommentStart("Legend= 42 ; comment", Delimiter);

        // The comment (including leading whitespace) starts at index 10 (the space before ';').
        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void FindCommentStart_NoComment_ReturnsNegative()
    {
        int result = LineCommentHelper.FindCommentStart("Legend= 42", Delimiter);

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void FindCommentStart_CommentOnlyLine_ReturnsZero()
    {
        int result = LineCommentHelper.FindCommentStart("; just a comment", Delimiter);

        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void FindCommentStart_SemicolonInQuotedString_IsTreatedAsComment()
    {
        // Legacy quirk: a semicolon inside quotes is still treated as a comment delimiter.
        int result = LineCommentHelper.FindCommentStart("Legend= \"hello;world\"", Delimiter);

        Assert.IsTrue(result >= 0);
    }

    [TestMethod]
    public void FindCommentStart_NonBreakingSpaceBeforeComment_IncludesWhitespace()
    {
        int result = LineCommentHelper.FindCommentStart("Legend= 42\u00A0; comment", Delimiter);

        Assert.AreEqual(10, result);
    }

    [TestMethod]
    public void FindCommentStart_EmptyString_ReturnsNegative()
    {
        int result = LineCommentHelper.FindCommentStart(string.Empty, Delimiter);

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void FindCommentStart_MultiCharacterDelimiter_FindsDelimiter()
    {
        int result = LineCommentHelper.FindCommentStart("code // comment", "//");

        // The comment (including leading whitespace) starts at index 4 (the space before '//').
        Assert.AreEqual(4, result);
    }

    [TestMethod]
    public void FindCommentStart_DelimiterAtEnd_ReturnsCorrectPosition()
    {
        int result = LineCommentHelper.FindCommentStart("code ;", Delimiter);

        // The comment (including leading whitespace) starts at index 4 (the space before ';').
        Assert.AreEqual(4, result);
    }

    // ---------------------------------------------------------------------------
    // GetCodeRange
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetCodeRange_LineWithComment_ReturnsCodeBeforeWhitespace()
    {
        TextRange range = LineCommentHelper.GetCodeRange("Legend= 42 ; comment", Delimiter);

        Assert.AreEqual(0, range.Offset);
        // Code ends at index 10 (the space before ';').
        Assert.AreEqual(10, range.Length);
        Assert.AreEqual("Legend= 42", range.GetText("Legend= 42 ; comment"));
    }

    [TestMethod]
    public void GetCodeRange_NoComment_ReturnsFullText()
    {
        TextRange range = LineCommentHelper.GetCodeRange("Legend= 42", Delimiter);

        Assert.AreEqual(0, range.Offset);
        Assert.AreEqual(10, range.Length);
        Assert.AreEqual("Legend= 42", range.GetText("Legend= 42"));
    }

    [TestMethod]
    public void GetCodeRange_CommentOnlyLine_ReturnsEmptyRange()
    {
        TextRange range = LineCommentHelper.GetCodeRange("; just a comment", Delimiter);

        Assert.AreEqual(0, range.Offset);
        Assert.AreEqual(0, range.Length);
        Assert.IsTrue(range.IsEmpty);
    }

    [TestMethod]
    public void GetCodeRange_EmptyString_ReturnsEmptyRange()
    {
        TextRange range = LineCommentHelper.GetCodeRange(string.Empty, Delimiter);

        Assert.AreEqual(0, range.Offset);
        Assert.AreEqual(0, range.Length);
    }

    // ---------------------------------------------------------------------------
    // RemoveLineComment — matches legacy LineParser.RemoveComments
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void RemoveLineComment_RemovesCommentAndPrecedingWhitespace()
    {
        string result = LineCommentHelper.RemoveLineComment("Legend= 42 ; comment", Delimiter);

        Assert.AreEqual("Legend= 42", result);
    }

    [TestMethod]
    public void RemoveLineComment_NoComment_ReturnsOriginal()
    {
        string result = LineCommentHelper.RemoveLineComment("Legend= 42", Delimiter);

        Assert.AreEqual("Legend= 42", result);
    }

    [TestMethod]
    public void RemoveLineComment_CommentOnlyLine_ReturnsEmpty()
    {
        string result = LineCommentHelper.RemoveLineComment("; just a comment", Delimiter);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RemoveLineComment_EmptyString_ReturnsEmpty()
    {
        string result = LineCommentHelper.RemoveLineComment(string.Empty, Delimiter);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RemoveLineComment_NullString_ReturnsEmpty()
    {
        string result = LineCommentHelper.RemoveLineComment(null!, Delimiter);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RemoveLineComment_LFLineEnding_PreservesNewline()
    {
        string result = LineCommentHelper.RemoveLineComment("Legend= 42 ; comment\n", Delimiter);

        Assert.AreEqual("Legend= 42\n", result);
    }

    [TestMethod]
    public void RemoveLineComment_CRLFLineEnding_PreservesLFOnly()
    {
        // Legacy regex \s*;.*$ with Multiline consumes \r as part of the .* match
        // because . matches \r (only \n is excluded). The \n is preserved as the
        // line terminator that $ anchors before.
        string result = LineCommentHelper.RemoveLineComment("Legend= 42 ; comment\r\n", Delimiter);

        Assert.AreEqual("Legend= 42\n", result);
    }

    [TestMethod]
    public void RemoveLineComment_CRLineEnding_IsPartOfTheLegacyMatch()
    {
        string result = LineCommentHelper.RemoveLineComment("Legend= 42 ; comment\r", Delimiter);

        Assert.AreEqual("Legend= 42", result);
    }

    [TestMethod]
    public void RemoveLineComment_MultipleLines_RemovesCommentsFromAllLines()
    {
        string input = "Line1 ; comment1\nLine2 ; comment2\nLine3";

        string result = LineCommentHelper.RemoveLineComment(input, Delimiter);

        Assert.AreEqual("Line1\nLine2\nLine3", result);
    }

    [TestMethod]
    public void RemoveLineComment_AllCommentLines_RemovesInterveningNewlines()
    {
        string input = "; a\n; b\n; c";

        string result = LineCommentHelper.RemoveLineComment(input, Delimiter);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void RemoveLineComment_TrailingCommentOnlyLine_RemovesPrecedingLineEnding()
    {
        // The multiline legacy regex consumes the preceding newline before a comment-only line.
        string input = "Line1 ; comment\n; comment";

        string result = LineCommentHelper.RemoveLineComment(input, Delimiter);

        Assert.AreEqual("Line1", result);
    }

    [TestMethod]
    public void RemoveLineComment_CommentOnlyLine_RemovesPrecedingLineEnding()
    {
        string result = LineCommentHelper.RemoveLineComment("Line1\r\n; comment", Delimiter);

        Assert.AreEqual("Line1", result);
    }

    // ---------------------------------------------------------------------------
    // MaskLineComment — matches legacy LineParser.EscapeComments
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void MaskLineComment_ReplacesCommentWithSpaces_PreservesLength()
    {
        string result = LineCommentHelper.MaskLineComment("Legend= 42 ; comment", Delimiter);

        Assert.AreEqual("Legend= 42          ", result);
        Assert.AreEqual(20, result.Length);
    }

    [TestMethod]
    public void MaskLineComment_NoComment_ReturnsOriginal()
    {
        string result = LineCommentHelper.MaskLineComment("Legend= 42", Delimiter);

        Assert.AreEqual("Legend= 42", result);
    }

    [TestMethod]
    public void MaskLineComment_EmptyString_ReturnsEmpty()
    {
        string result = LineCommentHelper.MaskLineComment(string.Empty, Delimiter);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void MaskLineComment_NullString_ReturnsEmpty()
    {
        string result = LineCommentHelper.MaskLineComment(null!, Delimiter);

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void MaskLineComment_WithTrailingNewline_PreservesNewlineAndLength()
    {
        string input = "Legend= 42 ; comment\n";
        string result = LineCommentHelper.MaskLineComment(input, Delimiter);

        Assert.IsTrue(result.EndsWith("\n"));
        Assert.AreEqual(input.Length, result.Length);
    }

    [TestMethod]
    public void MaskLineComment_CRLF_MasksCarriageReturnsMatchedByLegacyRegex()
    {
        string input = "A ; c\r\nB ; d\r\n";
        string result = LineCommentHelper.MaskLineComment(input, Delimiter);

        Assert.IsTrue(result.EndsWith(" \n"));
        Assert.AreEqual(input.Length, result.Length);
        Assert.IsFalse(result.Contains(";"));
    }

    [TestMethod]
    public void MaskLineComment_CommentOnlyLine_MasksPrecedingLineEnding()
    {
        // The legacy regex \s*;.*$ matches \r\n; comment (11 chars) because \s*
        // greedily consumes whitespace before the ; including the \r\n line ending.
        string input = "Line1\r\n; comment";
        string result = LineCommentHelper.MaskLineComment(input, Delimiter);

        Assert.AreEqual("Line1" + new string(' ', 11), result);
        Assert.AreEqual(input.Length, result.Length);
    }

    [TestMethod]
    public void MaskLineComment_MultipleLines_SpacesMatchOriginalCommentLength()
    {
        string input = "Line1 ; abc\nLine2 ; defgh";

        string result = LineCommentHelper.MaskLineComment(input, Delimiter);

        Assert.AreEqual(input.Length, result.Length);
        Assert.IsFalse(result.Contains(";"));
    }

    [TestMethod]
    public void MaskLineComment_CommentOnlyLine_BecomesSpacesOfSameLength()
    {
        string input = "; comment";
        string result = LineCommentHelper.MaskLineComment(input, Delimiter);

        Assert.AreEqual(input.Length, result.Length);
        Assert.IsFalse(result.Contains(";"));
    }

    // ---------------------------------------------------------------------------
    // // delimiter — quote-aware behavior (GameFlow / TRX)
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void FindCommentStart_SlashSlashInsideQuotedString_ReturnsNegative()
    {
        int result = LineCommentHelper.FindCommentStart("\"url\": \"http://example.com\",", "//");

        Assert.AreEqual(-1, result);
    }

    [TestMethod]
    public void FindCommentStart_SlashSlashInsideQuotesThenRealComment_FindsRealComment()
    {
        const string input = "\"url\": \"http://example.com\", // note";
        int result = LineCommentHelper.FindCommentStart(input, "//");

        int realCommentIndex = input.IndexOf("//", 20, StringComparison.Ordinal);
        Assert.AreEqual(realCommentIndex - 1, result);
    }

    [TestMethod]
    public void RemoveLineComment_SlashSlashInsideQuotedString_PreservesUrl()
    {
        string result = LineCommentHelper.RemoveLineComment("\"url\": \"http://example.com\",", "//");

        Assert.AreEqual("\"url\": \"http://example.com\",", result);
    }

    [TestMethod]
    public void RemoveLineComment_UrlThenRealComment_RemovesOnlyComment()
    {
        string result = LineCommentHelper.RemoveLineComment("\"url\": \"http://example.com\", // note", "//");

        Assert.AreEqual("\"url\": \"http://example.com\",", result);
    }

    [TestMethod]
    public void RemoveLineComment_EscapedQuote_StillSkipsQuotedSlashSlash()
    {
        // The \" is an escaped quote, so the // before the closing quote stays inside the string.
        string result = LineCommentHelper.RemoveLineComment("\"path\": \"a\\\"b//c\" // real", "//");

        Assert.AreEqual("\"path\": \"a\\\"b//c\"", result);
    }

    [TestMethod]
    public void RemoveLineComment_QuotedSlashSlashAcrossLines_ResetsQuoteStatePerLine()
    {
        string input = "\"path\": \"a//b\"\n\"title\": \"Caves\" // only this is a comment";

        string result = LineCommentHelper.RemoveLineComment(input, "//");

        Assert.AreEqual("\"path\": \"a//b\"\n\"title\": \"Caves\"", result);
    }
}
