using System.Linq;
using System.Reflection;
using TombLib.LevelData;
using TombLib.Wad;

namespace TombLib.Test;

[TestClass]
public class LevelSettingsTests
{
    [TestMethod]
    public void AutoStaticMeshMergeEntry_StaticMesh_FallsBackToStaticIdNameWhenMissing()
    {
        var settings = new LevelSettings
        {
            GameVersion = TRVersion.Game.TR4
        };

        var entry = new AutoStaticMeshMergeEntry(123u, false, false, false, false, settings);

        Assert.AreEqual(new WadStaticId(123u).ToString(settings.GameVersion), entry.StaticMesh);
    }

    [TestMethod]
    public void Clone_ReparentsAutoStaticMeshMergeEntries()
    {
        var settings = new LevelSettings();
        settings.AutoStaticMeshMerges.Add(new AutoStaticMeshMergeEntry(321u, true, false, false, false, settings));

        var clonedSettings = settings.Clone();
        var parentField = typeof(AutoStaticMeshMergeEntry).GetField("parent", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsNotNull(parentField);
        Assert.AreSame(clonedSettings, parentField.GetValue(clonedSettings.AutoStaticMeshMerges.Single()));
    }
}
