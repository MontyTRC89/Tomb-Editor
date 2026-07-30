using TombLib.Scripting.Text;

namespace TombLib.Tests.Text;

/// <summary>
/// Tests for <see cref="ContinuationHelper"/> matching legacy
/// <c>Patterns.NextLineKey</c> (<c>&gt;\s*(;.*)?$</c>) behavior.
/// </summary>
[TestClass]
public class ContinuationHelperTests
{
    private const string CommentDelimiter = ";";
    private const char ContinuationMarker = '>';

    // ---------------------------------------------------------------------------
    // IsValidContinuation
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void IsValidContinuation_WithMarker_ReturnsTrue()
    {
        bool result = ContinuationHelper.IsValidContinuation("Legend= 42 >", CommentDelimiter, ContinuationMarker);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValidContinuation_WithMarkerAndTrailingWhitespace_ReturnsTrue()
    {
        bool result = ContinuationHelper.IsValidContinuation("Legend= 42 > ", CommentDelimiter, ContinuationMarker);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValidContinuation_WithMarkerAndComment_ReturnsTrue()
    {
        // The comment is stripped before checking the marker.
        bool result = ContinuationHelper.IsValidContinuation("Legend= 42 > ; comment", CommentDelimiter, ContinuationMarker);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValidContinuation_WithMarkerWhitespaceAndComment_ReturnsTrue()
    {
        bool result = ContinuationHelper.IsValidContinuation("Legend= 42 >   ; comment", CommentDelimiter, ContinuationMarker);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValidContinuation_DuplicateMarker_ReturnsTrue()
    {
        // Legacy quirk: a final > is sufficient, so >> also continues.
        bool result = ContinuationHelper.IsValidContinuation("Legend= 42 >>", CommentDelimiter, ContinuationMarker);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsValidContinuation_NoMarker_ReturnsFalse()
    {
        bool result = ContinuationHelper.IsValidContinuation("Legend= 42", CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidContinuation_MarkerInComment_ReturnsFalse()
    {
        // The > is inside the comment, so it should not count.
        bool result = ContinuationHelper.IsValidContinuation("Legend= 42 ; > not a marker", CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidContinuation_EmptyString_ReturnsFalse()
    {
        bool result = ContinuationHelper.IsValidContinuation(string.Empty, CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidContinuation_CommentOnlyLine_ReturnsFalse()
    {
        bool result = ContinuationHelper.IsValidContinuation("; just a comment", CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidContinuation_WhitespaceOnlyLine_ReturnsFalse()
    {
        bool result = ContinuationHelper.IsValidContinuation("   ", CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidContinuation_MarkerNotAtEndOfCode_ReturnsFalse()
    {
        // The marker is in the middle of the code, not at the end.
        bool result = ContinuationHelper.IsValidContinuation("Legend= > 42", CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsValidContinuation_OnlyMarker_ReturnsTrue()
    {
        bool result = ContinuationHelper.IsValidContinuation(">", CommentDelimiter, ContinuationMarker);

        Assert.IsTrue(result);
    }

    // ---------------------------------------------------------------------------
    // HasSingleContinuationMarker
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void HasSingleContinuationMarker_SingleMarker_ReturnsTrue()
    {
        bool result = ContinuationHelper.HasSingleContinuationMarker("Legend= 42 >", CommentDelimiter, ContinuationMarker);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasSingleContinuationMarker_SingleMarkerWithComment_ReturnsTrue()
    {
        bool result = ContinuationHelper.HasSingleContinuationMarker("Legend= 42 > ; comment", CommentDelimiter, ContinuationMarker);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HasSingleContinuationMarker_DuplicateMarker_ReturnsFalse()
    {
        bool result = ContinuationHelper.HasSingleContinuationMarker("Legend= 42 >>", CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasSingleContinuationMarker_NoMarker_ReturnsFalse()
    {
        bool result = ContinuationHelper.HasSingleContinuationMarker("Legend= 42", CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasSingleContinuationMarker_CommentOnlyLine_ReturnsFalse()
    {
        bool result = ContinuationHelper.HasSingleContinuationMarker("; comment", CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void HasSingleContinuationMarker_TripleMarker_ReturnsFalse()
    {
        bool result = ContinuationHelper.HasSingleContinuationMarker("Legend= 42 >>>", CommentDelimiter, ContinuationMarker);

        Assert.IsFalse(result);
    }
}
