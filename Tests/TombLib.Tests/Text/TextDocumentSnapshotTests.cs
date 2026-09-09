using ICSharpCode.AvalonEdit.Document;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Text;

namespace TombLib.Tests.Text;

/// <summary>
/// Tests proving that <see cref="TextDocumentSnapshot"/> captures an immutable snapshot
/// that does not change when the underlying AvalonEdit document is edited.
/// </summary>
[TestClass]
public class TextDocumentSnapshotTests
{
    [TestMethod]
    public void Snapshot_DoesNotChangeWhenDocumentTextIsReplaced()
    {
        var document = new TextDocument("hello");
        var snapshot = new TextDocumentSnapshot(document);

        document.Text = "world!";

        Assert.AreEqual("hello", snapshot.GetText(0, snapshot.TextLength));
        Assert.AreEqual(5, snapshot.TextLength);
        Assert.AreEqual(1, snapshot.LineCount);
    }

    [TestMethod]
    public void Snapshot_DoesNotChangeWhenDocumentIsEdited()
    {
        var document = new TextDocument("line1\nline2");
        var snapshot = new TextDocumentSnapshot(document);

        document.Insert(6, " inserted");

        Assert.AreEqual("line1\nline2", snapshot.GetText(0, snapshot.TextLength));
        Assert.AreEqual(11, snapshot.TextLength);
        Assert.AreEqual(2, snapshot.LineCount);
    }

    [TestMethod]
    public void Snapshot_Lines_RemainStableAfterDocumentEdit()
    {
        var document = new TextDocument("first\nsecond");
        var snapshot = new TextDocumentSnapshot(document);

        ITextLine line = snapshot.GetLineByNumber(2);
        document.Insert(0, "X");

        Assert.AreEqual(6, line.Offset);
        Assert.AreEqual(6, line.Length);
        Assert.AreEqual(2, line.LineNumber);
        Assert.AreEqual("second", snapshot.GetText(line.Offset, line.Length));
    }

    [TestMethod]
    public void Snapshot_CapturesFileNameAtConstruction()
    {
        var document = new TextDocument("text");
        document.FileName = @"C:\path\original.txt";

        var snapshot = new TextDocumentSnapshot(document);

        document.FileName = @"C:\path\changed.txt";

        Assert.AreEqual(@"C:\path\original.txt", snapshot.FileName);
    }
}
