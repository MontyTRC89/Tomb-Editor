using TombLib.Scripting.Text;

namespace TombLib.Tests.Text;

/// <summary>
/// Tests for <see cref="ScriptLexer"/> tokenization using ClassicScript options.
/// </summary>
[TestClass]
public class ScriptLexerTests
{
    private static readonly ScriptLexerOptions Options = ScriptLexerOptions.ClassicScript;

    // ---------------------------------------------------------------------------
    // TokenizeLine — basic token types
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_EmptyLine_ProducesNoTokens()
    {
        var tokens = EnumerateLine("");

        Assert.AreEqual(0, tokens.Count);
    }

    [TestMethod]
    public void TokenizeLine_WhitespaceOnly_ProducesWhitespaceToken()
    {
        var tokens = EnumerateLine("   ");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Whitespace, tokens[0].Type);
        Assert.AreEqual(3, tokens[0].Length);
    }

    [TestMethod]
    public void TokenizeLine_CommentOnly_ProducesCommentToken()
    {
        var tokens = EnumerateLine("; comment text");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Comment, tokens[0].Type);
        Assert.AreEqual(14, tokens[0].Length);
    }

    [TestMethod]
    public void TokenizeLine_CommentOnlyNoText_ProducesCommentToken()
    {
        var tokens = EnumerateLine(";");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Comment, tokens[0].Type);
        Assert.AreEqual(1, tokens[0].Length);
    }

    [TestMethod]
    public void TokenizeLine_SectionHeader_ProducesSectionHeaderToken()
    {
        var tokens = EnumerateLine("[Level]");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.SectionHeader, tokens[0].Type);
        Assert.AreEqual(0, tokens[0].Offset);
        Assert.AreEqual(7, tokens[0].Length);
    }

    [TestMethod]
    public void TokenizeLine_SectionHeaderWithWhitespace_ProducesWhitespaceAndHeader()
    {
        var tokens = EnumerateLine("  [Title]");

        Assert.AreEqual(2, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Whitespace, tokens[0].Type);
        Assert.AreEqual(ScriptTokenType.SectionHeader, tokens[1].Type);
        Assert.AreEqual("[Title]", tokens[1].GetText("  [Title]"));
    }

    [TestMethod]
    public void TokenizeLine_Directive_ProducesDirectiveToken()
    {
        var tokens = EnumerateLine("#INCLUDE");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Directive, tokens[0].Type);
        Assert.AreEqual("#INCLUDE", tokens[0].GetText("#INCLUDE"));
    }

    [TestMethod]
    public void TokenizeLine_DirectiveWithPath_ProducesDirectiveAndPathTokens()
    {
        var tokens = EnumerateLine("#INCLUDE \"path\\file.txt\"");

        Assert.AreEqual(3, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Directive, tokens[0].Type);
        Assert.AreEqual(ScriptTokenType.Whitespace, tokens[1].Type);
        Assert.AreEqual(ScriptTokenType.StringLiteral, tokens[2].Type);
        Assert.AreEqual("\"path\\file.txt\"", tokens[2].GetText("#INCLUDE \"path\\file.txt\""));
    }

    [TestMethod]
    public void TokenizeLine_DirectiveWithComment_StopsAtComment()
    {
        var tokens = EnumerateLine("#DEFINE FOO 1 ; comment");

        Assert.IsTrue(tokens.Count >= 4);
        Assert.AreEqual(ScriptTokenType.Directive, tokens[0].Type);
        Assert.AreEqual(ScriptTokenType.Comment, tokens[^1].Type);
    }

    // ---------------------------------------------------------------------------
    // TokenizeLine — identifiers and mnemonics
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_SimpleIdentifier_ProducesIdentifierToken()
    {
        var tokens = EnumerateLine("Legend");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Identifier, tokens[0].Type);
        Assert.AreEqual("Legend", tokens[0].GetText("Legend"));
    }

    [TestMethod]
    public void TokenizeLine_MnemonicLikeIdentifier_ProducesMnemonicLikeIdentifierToken()
    {
        var tokens = EnumerateLine("CUST_BEHAVIOR");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.MnemonicLikeIdentifier, tokens[0].Type);
        Assert.AreEqual("CUST_BEHAVIOR", tokens[0].GetText("CUST_BEHAVIOR"));
    }

    [TestMethod]
    public void TokenizeLine_AllCapsNoUnderscore_ProducesIdentifierNotMnemonic()
    {
        var tokens = EnumerateLine("NAME");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Identifier, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_IdentifierWithDigits_ProducesIdentifier()
    {
        var tokens = EnumerateLine("item42");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Identifier, tokens[0].Type);
        Assert.AreEqual("item42", tokens[0].GetText("item42"));
    }

    [TestMethod]
    public void TokenizeLine_IdentifierStartingWithUnderscore_ProducesIdentifier()
    {
        var tokens = EnumerateLine("_private");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Identifier, tokens[0].Type);
    }

    // ---------------------------------------------------------------------------
    // TokenizeLine — numeric tokens
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_DecimalValue_ProducesDecimalValueToken()
    {
        var tokens = EnumerateLine("42");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.DecimalValue, tokens[0].Type);
        Assert.AreEqual("42", tokens[0].GetText("42"));
    }

    [TestMethod]
    public void TokenizeLine_HexValue_ProducesHexValueToken()
    {
        var tokens = EnumerateLine("$FF00AB");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.HexValue, tokens[0].Type);
        Assert.AreEqual("$FF00AB", tokens[0].GetText("$FF00AB"));
    }

    [TestMethod]
    public void TokenizeLine_HexValueLowercase_ProducesHexValueToken()
    {
        var tokens = EnumerateLine("$ff");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.HexValue, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_DollarSignAlone_ProducesUnknownToken()
    {
        var tokens = EnumerateLine("$");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Unknown, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_DollarSignFollowedByNonHex_ProducesHexValueThenIdentifier()
    {
        // Greedy hex matching: '$f' is a valid hex value, 'oo' is an identifier.
        var tokens = EnumerateLine("$foo");

        Assert.AreEqual(2, tokens.Count);
        Assert.AreEqual(ScriptTokenType.HexValue, tokens[0].Type); // '$f'
        Assert.AreEqual(ScriptTokenType.Identifier, tokens[1].Type); // 'oo'
    }

    // ---------------------------------------------------------------------------
    // TokenizeLine — operators and punctuation
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_Equals_ProducesEqualsToken()
    {
        var tokens = EnumerateLine("=");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Equals, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_Comma_ProducesCommaToken()
    {
        var tokens = EnumerateLine(",");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Comma, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_Plus_ProducesPlusToken()
    {
        var tokens = EnumerateLine("+");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Plus, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_Minus_ProducesMinusToken()
    {
        var tokens = EnumerateLine("-");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Minus, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_Asterisk_ProducesAsteriskToken()
    {
        var tokens = EnumerateLine("*");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Asterisk, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_Slash_ProducesSlashToken()
    {
        var tokens = EnumerateLine("/");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Slash, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_OpenParen_ProducesOpenParenToken()
    {
        var tokens = EnumerateLine("(");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.OpenParen, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_CloseParen_ProducesCloseParenToken()
    {
        var tokens = EnumerateLine(")");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.CloseParen, tokens[0].Type);
    }

    // ---------------------------------------------------------------------------
    // TokenizeLine — string literals
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_StringLiteral_ProducesStringLiteralToken()
    {
        var tokens = EnumerateLine("\"hello world\"");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.StringLiteral, tokens[0].Type);
        Assert.AreEqual("\"hello world\"", tokens[0].GetText("\"hello world\""));
    }

    [TestMethod]
    public void TokenizeLine_StringLiteralWithEscape_HandlesBackslash()
    {
        var tokens = EnumerateLine("\"hello\\nworld\"");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.StringLiteral, tokens[0].Type);
        Assert.AreEqual(14, tokens[0].Length);
    }

    [TestMethod]
    public void TokenizeLine_SemicolonInQuotedString_StartsComment()
    {
        var tokens = EnumerateLine("\"hello;world\"");

        Assert.AreEqual(2, tokens.Count);
        Assert.AreEqual(ScriptTokenType.StringLiteral, tokens[0].Type);
        Assert.AreEqual("\"hello", tokens[0].GetText("\"hello;world\""));
        Assert.AreEqual(ScriptTokenType.Comment, tokens[1].Type);
        Assert.AreEqual(";world\"", tokens[1].GetText("\"hello;world\""));
    }

    [TestMethod]
    public void TokenizeLine_UnclosedString_ConsumesToEndOfLine()
    {
        var tokens = EnumerateLine("\"unclosed");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(ScriptTokenType.StringLiteral, tokens[0].Type);
        Assert.AreEqual(9, tokens[0].Length);
    }

    // ---------------------------------------------------------------------------
    // TokenizeLine — continuation markers
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_ContinuationAtEndOfLine_ProducesContinuationMarker()
    {
        var tokens = EnumerateLine("Legend= 42 >");

        // Tokens: Identifier, Equals, Whitespace, DecimalValue, Whitespace, ContinuationMarker
        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.ContinuationMarker));
        var marker = tokens.Last(t => t.Type != ScriptTokenType.Whitespace);
        Assert.AreEqual(ScriptTokenType.ContinuationMarker, marker.Type);
    }

    [TestMethod]
    public void TokenizeLine_ContinuationWithTrailingWhitespace_ProducesContinuationMarker()
    {
        var tokens = EnumerateLine("Legend= 42 >   ");

        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.ContinuationMarker));
    }

    [TestMethod]
    public void TokenizeLine_ContinuationWithComment_ProducesContinuationAndComment()
    {
        var tokens = EnumerateLine("Legend= 42 > ; continue");

        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.ContinuationMarker));
        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.Comment));
    }

    [TestMethod]
    public void TokenizeLine_GreaterThanMidLine_ProducesUnknown()
    {
        var tokens = EnumerateLine("a > b");

        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.Unknown && t.GetText("a > b") == ">"));
        Assert.IsFalse(tokens.Any(t => t.Type == ScriptTokenType.ContinuationMarker));
    }

    [TestMethod]
    public void TokenizeLine_DoubleGreaterThanAtEnd_ProducesContinuationAndUnknown()
    {
        // >> at end: first > is unknown (not the last non-ws char), second > is continuation.
        var tokens = EnumerateLine("value >>");

        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.ContinuationMarker));
        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.Unknown));
    }

    // ---------------------------------------------------------------------------
    // TokenizeLine — malformed and edge cases
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_MalformedSectionNoCloseBracket_ProducesUnknownAndIdentifier()
    {
        var tokens = EnumerateLine("[incomplete");

        // Should not be a SectionHeader since there's no ']'.
        Assert.IsFalse(tokens.Any(t => t.Type == ScriptTokenType.SectionHeader));
        Assert.AreEqual(ScriptTokenType.Unknown, tokens[0].Type); // '['
        Assert.AreEqual(ScriptTokenType.Identifier, tokens[1].Type); // 'incomplete'
    }

    [TestMethod]
    public void TokenizeLine_EmptyBrackets_ProducesSectionHeader()
    {
        var tokens = EnumerateLine("[]");

        Assert.AreEqual(1, tokens.Count);
        // [] is technically a section header by the lexer's structural rules.
        Assert.AreEqual(ScriptTokenType.SectionHeader, tokens[0].Type);
    }

    [TestMethod]
    public void TokenizeLine_HashNotFollowedByIdentifier_ProducesUnknownThenWhitespaceThenDecimal()
    {
        // '#' at line start with no keyword after it is Unknown.
        var tokens = EnumerateLine("# 123");

        Assert.AreEqual(3, tokens.Count);
        Assert.AreEqual(ScriptTokenType.Unknown, tokens[0].Type); // '#'
        Assert.AreEqual(ScriptTokenType.Whitespace, tokens[1].Type);
        Assert.AreEqual(ScriptTokenType.DecimalValue, tokens[2].Type); // '123'
    }

    // ---------------------------------------------------------------------------
    // TokenizeLine — complex command lines
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_FullCommand_ProducesCorrectSequence()
    {
        var tokens = EnumerateLine("Legend= 42, CUST_BEHAVIOR, $FF ; comment");

        Assert.AreEqual(ScriptTokenType.Identifier, tokens[0].Type);        // Legend
        Assert.AreEqual(ScriptTokenType.Equals, tokens[1].Type);            // =
        Assert.AreEqual(ScriptTokenType.Whitespace, tokens[2].Type);        // ' '
        Assert.AreEqual(ScriptTokenType.DecimalValue, tokens[3].Type);      // 42
        Assert.AreEqual(ScriptTokenType.Comma, tokens[4].Type);             // ,
        Assert.AreEqual(ScriptTokenType.Whitespace, tokens[5].Type);        // ' '
        Assert.AreEqual(ScriptTokenType.MnemonicLikeIdentifier, tokens[6].Type); // CUST_BEHAVIOR
        Assert.AreEqual(ScriptTokenType.Comma, tokens[7].Type);             // ,
        Assert.AreEqual(ScriptTokenType.Whitespace, tokens[8].Type);        // ' '
        Assert.AreEqual(ScriptTokenType.HexValue, tokens[9].Type);          // $FF
        Assert.AreEqual(ScriptTokenType.Whitespace, tokens[10].Type);       // ' '
        Assert.AreEqual(ScriptTokenType.Comment, tokens[11].Type);          // ; comment
    }

    [TestMethod]
    public void TokenizeLine_FullCommand_VerifyCount()
    {
        var tokens = EnumerateLine("Legend= 42, CUST_BEHAVIOR, $FF ; comment");

        // Legend, =, ' ', 42, ,, ' ', CUST_BEHAVIOR, ,, ' ', $FF, ' ', ; comment
        Assert.AreEqual(12, tokens.Count);
    }

    // ---------------------------------------------------------------------------
    // TokenizeLine — offsets and line numbers
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_OffsetsIncludeLineOffset()
    {
        var tokens = EnumerateLine("ab cd", lineOffset: 100);

        Assert.AreEqual(3, tokens.Count);
        Assert.AreEqual(100, tokens[0].Offset); // "ab"
        Assert.AreEqual(2, tokens[0].Length);
        Assert.AreEqual(102, tokens[1].Offset); // " "
        Assert.AreEqual(103, tokens[2].Offset); // "cd"
    }

    [TestMethod]
    public void TokenizeLine_LineNumberIsPreserved()
    {
        var tokens = EnumerateLine("text", lineNumber: 7);

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(7, tokens[0].LineNumber);
    }

    [TestMethod]
    public void TokenizeLine_DefaultLineNumberIsOne()
    {
        var tokens = EnumerateLine("text");

        Assert.AreEqual(1, tokens.Count);
        Assert.AreEqual(1, tokens[0].LineNumber);
    }

    // ---------------------------------------------------------------------------
    // TokenizeDocument — multi-line document tests
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeDocument_EmptyDocument_ProducesNoTokens()
    {
        var source = new StringTextSnapshot("");

        var tokens = ScriptLexer.TokenizeDocument(source, Options);

        Assert.AreEqual(0, tokens.Count);
    }

    [TestMethod]
    public void TokenizeDocument_TwoLines_AssignsCorrectLineNumbers()
    {
        var source = new StringTextSnapshot("First\nSecond");

        var tokens = ScriptLexer.TokenizeDocument(source, Options);

        Assert.IsTrue(tokens.Count > 0);

        // All tokens on line 1 should have LineNumber == 1.
        var line1Tokens = tokens.Where(t => t.LineNumber == 1).ToList();
        Assert.IsTrue(line1Tokens.Count > 0);
        Assert.IsTrue(line1Tokens.All(t => t.LineNumber == 1));

        // All tokens on line 2 should have LineNumber == 2.
        var line2Tokens = tokens.Where(t => t.LineNumber == 2).ToList();
        Assert.IsTrue(line2Tokens.Count > 0);
        Assert.IsTrue(line2Tokens.All(t => t.LineNumber == 2));

        Assert.AreEqual("First", line1Tokens[0].GetText("First\nSecond"));
        Assert.AreEqual("Second", line2Tokens[0].GetText("First\nSecond"));
    }

    [TestMethod]
    public void TokenizeDocument_CRLFDocument_AssignsCorrectLineNumbers()
    {
        var source = new StringTextSnapshot("Line1\r\nLine2\r\nLine3");

        var tokens = ScriptLexer.TokenizeDocument(source, Options);

        Assert.IsTrue(tokens.Any(t => t.LineNumber == 1));
        Assert.IsTrue(tokens.Any(t => t.LineNumber == 2));
        Assert.IsTrue(tokens.Any(t => t.LineNumber == 3));
        Assert.IsFalse(tokens.Any(t => t.LineNumber == 4));
    }

    [TestMethod]
    public void TokenizeDocument_MultiLineSectionAndCommand_CorrectOffsets()
    {
        var source = new StringTextSnapshot("[Level]\nLegend= 42\n");

        var tokens = ScriptLexer.TokenizeDocument(source, Options);

        // [Level] token should be at offset 0.
        var header = tokens.First(t => t.Type == ScriptTokenType.SectionHeader);
        Assert.AreEqual(0, header.Offset);
        Assert.AreEqual("[Level]", header.GetText(source.GetText(0, source.TextLength)));

        // Legend token should be on line 2.
        var legend = tokens.First(t => t.Type == ScriptTokenType.Identifier && t.GetText(source.GetText(0, source.TextLength)) == "Legend");
        Assert.AreEqual(2, legend.LineNumber);
    }

    [TestMethod]
    public void TokenizeDocument_CommentOnlyLines_TokenizedCorrectly()
    {
        var source = new StringTextSnapshot("; first comment\n; second comment\n");

        var tokens = ScriptLexer.TokenizeDocument(source, Options);

        var comments = tokens.Where(t => t.Type == ScriptTokenType.Comment).ToList();
        Assert.AreEqual(2, comments.Count);
        Assert.AreEqual(1, comments[0].LineNumber);
        Assert.AreEqual(2, comments[1].LineNumber);
    }

    [TestMethod]
    public void TokenizeDocument_ContinuationLines_TokenizedCorrectly()
    {
        var source = new StringTextSnapshot("Legend= 42 >\nCUST_BEHAVIOR, $FF\n");

        var tokens = ScriptLexer.TokenizeDocument(source, Options);

        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.ContinuationMarker));
        Assert.AreEqual(1, tokens.First(t => t.Type == ScriptTokenType.ContinuationMarker).LineNumber);
    }

    [TestMethod]
    public void TokenizeDocument_ExpressionTokens_RecognizedCorrectly()
    {
        var source = new StringTextSnapshot("value = (1 + 2) * 3 - 4 / 2\n");

        var tokens = ScriptLexer.TokenizeDocument(source, Options);

        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.OpenParen));
        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.CloseParen));
        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.Plus));
        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.Asterisk));
        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.Minus));
        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.Slash));
    }

    // ---------------------------------------------------------------------------
    // TokenizeLine — edge case: hash alone
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void TokenizeLine_HashNotAtStartOfLine_ProducesUnknown()
    {
        var tokens = EnumerateLine("a # b");

        Assert.IsTrue(tokens.Any(t => t.Type == ScriptTokenType.Unknown && t.GetText("a # b") == "#"));
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static List<ScriptToken> EnumerateLine(
        string line, int lineOffset = 0, int lineNumber = 1)
    {
        var tokens = new List<ScriptToken>();
        var enumerator = ScriptLexer.TokenizeLine(line.AsSpan(), lineOffset, lineNumber, Options);

        while (enumerator.MoveNext())
            tokens.Add(enumerator.Current);

        return tokens;
    }
}
