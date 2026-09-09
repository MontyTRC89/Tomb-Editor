using DarkUI.Controls;
using TombLib.Scripting.TRX.ContentNodes;
using TombLib.Scripting.TRX.Services;

namespace TombLib.Tests.TRX;

/// <summary>
/// Tests for <see cref="TRXNodesProvider"/> level-name node extraction.
/// </summary>
[TestClass]
public class TRXNodesProviderTests
{
    private readonly ITRXLineService _lineService = new TRXLineService();

    private static IReadOnlyList<DarkTreeNode> GetNodes(string content, string filter = "")
        => new TRXNodesProvider(new TRXLineService()).GetNodes(content, filter);

    // ---------------------------------------------------------------------------
    // Title properties
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetNodes_TitleProperty_ReturnsLevelNode()
    {
        IReadOnlyList<DarkTreeNode> nodes = GetNodes("\"title\": \"Caves\",\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Caves", nodes[0].Text);
    }

    [TestMethod]
    public void GetNodes_TitlePropertyWithComment_ReturnsLevelNode()
    {
        IReadOnlyList<DarkTreeNode> nodes = GetNodes("\"title\": \"Caves\", // level title\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Caves", nodes[0].Text);
    }

    [TestMethod]
    public void GetNodes_TitlePropertyWithUrlValue_ReturnsFullValue()
    {
        IReadOnlyList<DarkTreeNode> nodes = GetNodes("\"title\": \"http://example.com\",\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("http://example.com", nodes[0].Text);
    }

    [TestMethod]
    public void GetNodes_EmptyTitleValue_ReturnsNoNode()
    {
        IReadOnlyList<DarkTreeNode> nodes = GetNodes("\"title\": \"\",\n");

        Assert.AreEqual(0, nodes.Count);
    }

    [TestMethod]
    public void GetNodes_MalformedTitleValue_ReturnsNoNode()
    {
        IReadOnlyList<DarkTreeNode> nodes = GetNodes("\"title\": ,\n");

        Assert.AreEqual(0, nodes.Count);
    }

    // ---------------------------------------------------------------------------
    // // Level N: Name fallback lines
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetNodes_LevelCommentLine_ReturnsFallbackNode()
    {
        IReadOnlyList<DarkTreeNode> nodes = GetNodes("// Level 1: Caves\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Caves", nodes[0].Text);
    }

    [TestMethod]
    public void GetNodes_LevelCommentWithDotSeparator_ReturnsFallbackNode()
    {
        IReadOnlyList<DarkTreeNode> nodes = GetNodes("// Level 2. Venice\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Venice", nodes[0].Text);
    }

    [TestMethod]
    public void GetNodes_CommentLineContainingTitle_StillUsesRawFallback()
    {
        // Malformed mixed input: the line is a comment that also contains a title property.
        // The fallback must run against the raw line text, not the comment-stripped text.
        IReadOnlyList<DarkTreeNode> nodes = GetNodes("// Level 1: \"title\": \"Caves\"\n");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("\"title\": \"Caves\"", nodes[0].Text);
    }

    // ---------------------------------------------------------------------------
    // Filtering
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void GetNodes_Filter_ReturnsOnlyMatchingNodes()
    {
        const string content = "\"title\": \"Caves\",\n\"title\": \"Venice\",\n\"title\": \"City\",\n";

        IReadOnlyList<DarkTreeNode> nodes = GetNodes(content, "ven");

        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("Venice", nodes[0].Text);
    }
}
