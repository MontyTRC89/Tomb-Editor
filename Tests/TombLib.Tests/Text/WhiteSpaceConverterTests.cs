using TombLib.Scripting.Text;

namespace TombLib.Tests.Text;

/// <summary>
/// Tests for <see cref="WhiteSpaceConverter"/>.
/// </summary>
[TestClass]
public class WhiteSpaceConverterTests
{
    // ---------------------------------------------------------------------------
    // tabSize validation
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ConvertSpacesToTabs_ZeroTabSize_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => WhiteSpaceConverter.ConvertSpacesToTabs("code", 0));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_NegativeTabSize_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => WhiteSpaceConverter.ConvertSpacesToTabs("code", -4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_ZeroTabSize_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => WhiteSpaceConverter.ConvertTabsToSpaces("code", 0));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_NegativeTabSize_Throws()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => WhiteSpaceConverter.ConvertTabsToSpaces("code", -4));
    }

    // ---------------------------------------------------------------------------
    // ConvertSpacesToTabs — leading indentation
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ConvertSpacesToTabs_EmptyInput_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, WhiteSpaceConverter.ConvertSpacesToTabs(string.Empty, 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_NoIndentation_ReturnsOriginal()
    {
        const string input = "Legend= 42";

        Assert.AreEqual(input, WhiteSpaceConverter.ConvertSpacesToTabs(input, 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_FourSpaces_BecomesOneTab()
    {
        Assert.AreEqual("\tLegend= 42", WhiteSpaceConverter.ConvertSpacesToTabs("    Legend= 42", 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_EightSpaces_BecomesTwoTabs()
    {
        Assert.AreEqual("\t\tLegend= 42", WhiteSpaceConverter.ConvertSpacesToTabs("        Legend= 42", 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_PartialLeadingSpaces_StayAsSpaces()
    {
        Assert.AreEqual("   Legend= 42", WhiteSpaceConverter.ConvertSpacesToTabs("   Legend= 42", 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_SpacesNotAlignedToTabStop_KeepTrailingPartialGroup()
    {
        // Six leading spaces with tabSize 4: the first four become a tab, the last two stay as spaces.
        Assert.AreEqual("\t  Legend= 42", WhiteSpaceConverter.ConvertSpacesToTabs("      Legend= 42", 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_ExistingTab_IsPreservedAndAdvancesColumn()
    {
        Assert.AreEqual("\t   Legend= 42", WhiteSpaceConverter.ConvertSpacesToTabs("\t   Legend= 42", 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_EmptyLines_StayEmpty()
    {
        Assert.AreEqual("\n\n", WhiteSpaceConverter.ConvertSpacesToTabs("\n\n", 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_ContentIsNeverChanged()
    {
        const string input = "    Legend= \"  hello;world  \" ; 42 >\n  , 43";

        string result = WhiteSpaceConverter.ConvertSpacesToTabs(input, 4);

        // All non-indentation content is preserved byte-for-byte.
        Assert.AreEqual("\tLegend= \"  hello;world  \" ; 42 >\n  , 43", result);
    }

    [TestMethod]
    public void ConvertSpacesToTabs_LargeTabSize_KeepsSpaces()
    {
        // No indentation reaches a tab stop, so nothing is converted and no allocation explodes.
        Assert.AreEqual("    Legend= 42", WhiteSpaceConverter.ConvertSpacesToTabs("    Legend= 42", 1024));
    }

    // ---------------------------------------------------------------------------
    // ConvertSpacesToTabs — line endings
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ConvertSpacesToTabs_LFLineEndings_Preserved()
    {
        Assert.AreEqual("\tA\n\tB\n", WhiteSpaceConverter.ConvertSpacesToTabs("    A\n    B\n", 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_CRLFLineEndings_Preserved()
    {
        Assert.AreEqual("\tA\r\n\tB\r\n", WhiteSpaceConverter.ConvertSpacesToTabs("    A\r\n    B\r\n", 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_MixedLineEndings_Preserved()
    {
        Assert.AreEqual("\tA\r\n\tB\n\tC", WhiteSpaceConverter.ConvertSpacesToTabs("    A\r\n    B\n    C", 4));
    }

    [TestMethod]
    public void ConvertSpacesToTabs_StandaloneCRLineEndings_Preserved()
    {
        Assert.AreEqual("\tA\r\tB\r", WhiteSpaceConverter.ConvertSpacesToTabs("    A\r    B\r", 4));
    }

    // ---------------------------------------------------------------------------
    // ConvertTabsToSpaces — expansion
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ConvertTabsToSpaces_EmptyInput_ReturnsEmpty()
    {
        Assert.AreEqual(string.Empty, WhiteSpaceConverter.ConvertTabsToSpaces(string.Empty, 4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_NoTabs_ReturnsOriginal()
    {
        const string input = "Legend= 42";

        Assert.AreEqual(input, WhiteSpaceConverter.ConvertTabsToSpaces(input, 4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_LeadingTab_BecomesTabSizeSpaces()
    {
        Assert.AreEqual("    Legend= 42", WhiteSpaceConverter.ConvertTabsToSpaces("\tLegend= 42", 4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_TabInsideContent_ReachesNextTabStop()
    {
        // Column before the tab is 1 ('a'), so the tab expands to 3 spaces.
        Assert.AreEqual("a   b", WhiteSpaceConverter.ConvertTabsToSpaces("a\tb", 4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_TabAfterPartialIndent_ReachesNextTabStop()
    {
        // Three spaces then a tab: the tab expands to one space (next stop at column 4).
        Assert.AreEqual("    b", WhiteSpaceConverter.ConvertTabsToSpaces("   \tb", 4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_TabAfterExactTabStop_ExpandsFully()
    {
        // Four spaces then a tab: the tab expands to four spaces.
        Assert.AreEqual("        b", WhiteSpaceConverter.ConvertTabsToSpaces("    \tb", 4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_EmptyLines_StayEmpty()
    {
        Assert.AreEqual("\n\n", WhiteSpaceConverter.ConvertTabsToSpaces("\n\n", 4));
    }

    // ---------------------------------------------------------------------------
    // ConvertTabsToSpaces — line endings
    // ---------------------------------------------------------------------------

    [TestMethod]
    public void ConvertTabsToSpaces_LFLineEndings_Preserved()
    {
        Assert.AreEqual("    A\n    B\n", WhiteSpaceConverter.ConvertTabsToSpaces("\tA\n\tB\n", 4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_CRLFLineEndings_Preserved()
    {
        Assert.AreEqual("    A\r\n    B\r\n", WhiteSpaceConverter.ConvertTabsToSpaces("\tA\r\n\tB\r\n", 4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_MixedLineEndings_Preserved()
    {
        Assert.AreEqual("    A\r\n    B\n    C", WhiteSpaceConverter.ConvertTabsToSpaces("\tA\r\n\tB\n\tC", 4));
    }

    [TestMethod]
    public void ConvertTabsToSpaces_StandaloneCRLineEndings_Preserved()
    {
        Assert.AreEqual("    A\r    B\r", WhiteSpaceConverter.ConvertTabsToSpaces("\tA\r\tB\r", 4));
    }
}
