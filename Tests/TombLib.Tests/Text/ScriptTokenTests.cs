using TombLib.Scripting.Text;

namespace TombLib.Tests.Text;

/// <summary>
/// Direct tests for <see cref="ScriptToken"/> construction, equality, hashing,
/// text extraction, and invalid ranges.
/// </summary>
[TestClass]
public class ScriptTokenTests
{
    [TestMethod]
    public void Constructor_ValidValues_CreatesToken()
    {
        var token = new ScriptToken(ScriptTokenType.Identifier, 5, 3, 2);

        Assert.AreEqual(ScriptTokenType.Identifier, token.Type);
        Assert.AreEqual(5, token.Offset);
        Assert.AreEqual(3, token.Length);
        Assert.AreEqual(2, token.LineNumber);
        Assert.AreEqual(8, token.EndOffset);
    }

    [TestMethod]
    public void Constructor_NegativeOffset_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ScriptToken(ScriptTokenType.Identifier, -1, 3, 1));
    }

    [TestMethod]
    public void Constructor_NegativeLength_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ScriptToken(ScriptTokenType.Identifier, 0, -1, 1));
    }

    [TestMethod]
    public void Constructor_ZeroLength_CreatesEmptyToken()
    {
        var token = new ScriptToken(ScriptTokenType.Whitespace, 4, 0, 1);

        Assert.AreEqual(4, token.EndOffset);
    }

    [TestMethod]
    public void GetText_ReturnsSubstring()
    {
        var token = new ScriptToken(ScriptTokenType.Identifier, 2, 3, 1);

        Assert.AreEqual("llo", token.GetText("hello"));
    }

    [TestMethod]
    public void GetText_EmptyToken_ReturnsEmpty()
    {
        var token = new ScriptToken(ScriptTokenType.Whitespace, 2, 0, 1);

        Assert.AreEqual(string.Empty, token.GetText("hello"));
    }

    [TestMethod]
    public void GetText_BeyondLength_Throws()
    {
        var token = new ScriptToken(ScriptTokenType.Identifier, 3, 5, 1);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => token.GetText("hello"));
    }

    [TestMethod]
    public void GetText_NullSource_Throws()
    {
        var token = new ScriptToken(ScriptTokenType.Identifier, 0, 1, 1);

        Assert.ThrowsException<ArgumentNullException>(() => token.GetText(null!));
    }

    [TestMethod]
    public void GetText_OffsetBeyondLength_NoOverflow_Throws()
    {
        var token = new ScriptToken(ScriptTokenType.Identifier, int.MaxValue, 0, 1);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => token.GetText("x"));
    }

    [TestMethod]
    public void GetText_OffsetPlusLengthWouldOverflow_Throws()
    {
        var token = new ScriptToken(ScriptTokenType.Identifier, int.MaxValue - 1, int.MaxValue, 1);

        Assert.ThrowsException<ArgumentOutOfRangeException>(() => token.GetText("x"));
    }

    [TestMethod]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = new ScriptToken(ScriptTokenType.Identifier, 1, 2, 3);
        var b = new ScriptToken(ScriptTokenType.Identifier, 1, 2, 3);

        Assert.IsTrue(a.Equals(b));
    }

    [TestMethod]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a = new ScriptToken(ScriptTokenType.Identifier, 1, 2, 3);
        var b = new ScriptToken(ScriptTokenType.Identifier, 1, 3, 3);

        Assert.IsFalse(a.Equals(b));
    }

    [TestMethod]
    public void Equals_Object_ReturnsCorrectResult()
    {
        var token = new ScriptToken(ScriptTokenType.Identifier, 1, 2, 3);

        Assert.IsTrue(token.Equals((object)new ScriptToken(ScriptTokenType.Identifier, 1, 2, 3)));
        Assert.IsFalse(token.Equals("not a token"));
        Assert.IsFalse(token.Equals(null));
    }

    [TestMethod]
    public void GetHashCode_SameValues_HaveSameHashCode()
    {
        var a = new ScriptToken(ScriptTokenType.Identifier, 5, 3, 2);
        var b = new ScriptToken(ScriptTokenType.Identifier, 5, 3, 2);

        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void ToString_ReturnsReadableFormat()
    {
        var token = new ScriptToken(ScriptTokenType.Identifier, 3, 7, 2);
        string result = token.ToString();

        StringAssert.Contains(result, "Identifier");
        StringAssert.Contains(result, "3");
        StringAssert.Contains(result, "7");
        StringAssert.Contains(result, "2");
    }
}
