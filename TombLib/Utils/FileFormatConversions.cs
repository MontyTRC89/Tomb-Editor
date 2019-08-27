using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.Forms;
using System.Windows.Forms;
using System.IO;
using TombLib.LevelData.IO;
using TombLib.LevelData;
using System.Text.RegularExpressions;

namespace TombLib.Utils
{
    // This class was created for all conversion methos that re-process Wad2 and Prj2 files due to breaking changes
    // Let's hope that we must use this class almost never
    public class FileFormatConversions
    {
        public class SoundInfoConversionRow
        {
            public WadSoundInfo SoundInfo { get; set; }
            public bool SaveToXml { get; set; }
            public bool ExportSamples { get; set; }
            public int NewId { get; set; }
            public string NewName { get; set; }
            public string OldName { get; set; }

            public SoundInfoConversionRow(WadSoundInfo info, string name)
            {
                SoundInfo = info;
                NewId = -1;
                SaveToXml = false;
                NewName = "";
                OldName = name;
            }
        }

        public const string SoundsCatalogPath = "Sounds\\TR4\\Sounds.xml";

        public static bool ConvertWad2ToNewSoundFormat(string src, string dest, string soundsCatalog)
        {
            try
            {
                // Load sounds catalog
                WadSounds sounds = WadSounds.ReadFromFile(soundsCatalog);

                // Load Wad2
                Wad2 wad = Wad2Loader.LoadFromFile(src);

                // Check if the Wad2 needs to be converted
                if (wad.SoundSystem != SoundSystem.Dynamic)
                    return true;

                // Now collect all sound infos from obsolete lists and build a new list
                var soundInfos = wad.AllLoadesSoundInfos.Values.ToList();

                // Loop through each sound info and try to get the classic Id from TrCatalog.xml
                var conversionList = new List<SoundInfoConversionRow>();
                foreach (var soundInfo in soundInfos)
                {
                    var row = new SoundInfoConversionRow(soundInfo, soundInfo.Name);

                    // If user has changed name, result will be -1 and the user will need to manually set the new sound id
                    row.NewId = TrCatalog.TryGetSoundInfoIdByDescription(wad.SuggestedGameVersion, soundInfo.Name);
                    if (row.NewId != -1)
                    {
                        row.SaveToXml = true;
                        row.NewName = TrCatalog.GetOriginalSoundName(wad.SuggestedGameVersion, (uint)row.NewId);
                    }

                    conversionList.Add(row);
                }

                // Now we'll show a dialog with all conversion rows and the user will need to make some choices
                using (var form = new Wad2SoundsConversionDialog(wad.SuggestedGameVersion, conversionList))
                {
                    if (form.ShowDialog() == DialogResult.Cancel)
                        return false;
                }

                // Assign new Id and name
                foreach (var row in conversionList)
                {
                    row.SoundInfo.Id = row.NewId;
                    row.SoundInfo.Name = row.NewName;
                }

                // Remap all sounds in animcommands
                foreach (var row in conversionList)
                    if (row.NewId != -1)
                    {
                        foreach (var moveable in wad.Moveables)
                            foreach (var animation in moveable.Value.Animations)
                                foreach (var cmd in animation.AnimCommands)
                                {
                                    if (cmd.SoundInfoObsolete != null && cmd.SoundInfoObsolete == row.SoundInfo)
                                    {
                                        cmd.Parameter2 = (short)((cmd.Parameter2 & 0xc000) | row.NewId);
                                        cmd.SoundInfoObsolete = null;
                                    }
                                }
                    }

                // Bind samples
                foreach (var row in conversionList)
                    if (row.SaveToXml)
                    {
                        if (row.ExportSamples)
                        {
                            var samples = new List<string>();
                            foreach (var sample in row.SoundInfo.EmbeddedSamples)
                            {
                                if (sample.IsLoaded)
                                {
                                    string sampleName = row.NewName.ToLower() + "_" + row.SoundInfo.EmbeddedSamples.IndexOf(sample) + ".wav";
                                    samples.Add(sampleName);
                                    File.WriteAllBytes(Path.GetDirectoryName(dest) + "\\" + sampleName, sample.Data);
                                }
                            }

                            row.SoundInfo.EmbeddedSamples.Clear();
                            foreach (var sample in samples)
                                row.SoundInfo.EmbeddedSamples.Add(new WadSample(sample));
                        }
                        else
                        {
                            row.SoundInfo.EmbeddedSamples.Clear();
                            var refInfo = sounds.TryGetSoundInfo(row.SoundInfo.Id);
                            if (refInfo != null)
                                row.SoundInfo.EmbeddedSamples.AddRange(refInfo.EmbeddedSamples);
                        }
                    }

                // Create the new sounds archive
                foreach (var row in conversionList)
                    if (row.SaveToXml)
                        wad.Sounds.SoundInfos.Add(row.SoundInfo);

                // Sort sound infos
                wad.Sounds.SoundInfos.Sort((a, b) => a.Id.CompareTo(b.Id));

                // Make a backup copy
                if (src == dest)
                {
                    int index = 0;
                    string backupFilename = "";
                    while (true)
                    {
                        backupFilename = dest + "." + index + ".bak";
                        if (!File.Exists(backupFilename))
                            break;
                        index++;
                    }

                    File.Copy(src, backupFilename, true);
                }

                // Save Wad2 with Xml sounds
                wad.SoundSystem = SoundSystem.Xml;
                Wad2Writer.SaveToFile(wad, dest);

                // Finished!
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool ConvertPrj2ToNewSoundFormat(Level level, string src, string dest, string soundsCatalog, bool save)
        {
            try
            {
                // Load sounds catalog
                WadSounds sounds = WadSounds.ReadFromFile(soundsCatalog);

                // Check for sound system
                if (level.Settings.SoundSystem == SoundSystem.Xml)
                    return true;

                // Infer the wad version from level version
                WadGameVersion version = WadGameVersion.TR4_TRNG;
                switch (level.Settings.GameVersion)
                {
                    case GameVersion.TR2:
                        version = WadGameVersion.TR2;
                        break;
                    case GameVersion.TR3:
                        version = WadGameVersion.TR3;
                        break;
                    case GameVersion.TR4:
                    case GameVersion.TRNG:
                        version = WadGameVersion.TR4_TRNG;
                        break;
                    case GameVersion.TR5:
                        version = WadGameVersion.TR5;
                        break;
                    case GameVersion.TR5Main:
                        version = WadGameVersion.TR5Main;
                        break;
                }

                // Collect all sounds to remap
                var conversionList = new List<SoundInfoConversionRow>();

                // We start from sound id = 602 which is the TR1 sound area of TRNG extended soundmap.
                // This area is reserved for TR1 enemies and so it *** should *** be used rarely
                int lastSoundId = 602;

                foreach (var room in level.Rooms)
                {
                    if (room != null)
                        foreach (var obj in room.Objects)
                        {
                            if (obj is SoundSourceInstance)
                            {
                                SoundSourceInstance soundSource = obj as SoundSourceInstance;
                                if (soundSource.WadReferencedSoundName != null && soundSource.WadReferencedSoundName != "")
                                {
                                    if (!conversionList.Select(f => f.OldName).Contains(soundSource.WadReferencedSoundName))
                                    {
                                        // First try to get sound name from TrCatalog
                                        int newId = TrCatalog.TryGetSoundInfoIdByDescription(version, soundSource.WadReferencedSoundName);

                                        var row = new SoundInfoConversionRow(null, soundSource.WadReferencedSoundName);
                                        if (newId == -1)
                                        {
                                            // If sound was not found in catalog, then assign a generic Id and ask to the user
                                            row.NewName = Regex.Replace(soundSource.WadReferencedSoundName, "[^A-Za-z0-9 _]", "").ToUpper();
                                            row.NewId = lastSoundId++;
                                        }
                                        else
                                        {
                                            // Otherwise, we are lucky, and we can just assign the correct Id
                                            row.NewName = TrCatalog.GetOriginalSoundName(version, (uint)newId);
                                            row.NewId = newId;
                                        }

                                        conversionList.Add(row);
                                    }
                                }
                                else if (soundSource.EmbeddedSoundInfo != null)
                                {
                                    bool found = false;
                                    foreach (var r in conversionList)
                                        if (r.SoundInfo != null && r.SoundInfo == soundSource.EmbeddedSoundInfo)
                                        {
                                            found = true;
                                            break;
                                        }

                                    if (found)
                                        continue;

                                    // In this case we can't do anything, sound is not in catalog for sure and we must 
                                    // ask to the user
                                    var row = new SoundInfoConversionRow(soundSource.EmbeddedSoundInfo,
                                                                         soundSource.EmbeddedSoundInfo.Name);
                                    row.NewName = Regex.Replace(soundSource.EmbeddedSoundInfo.Name, "[^A-Za-z0-9 _]", "").ToUpper(); 
                                    row.NewId = lastSoundId++;

                                    conversionList.Add(row);
                                }
                            }
                        }
                }

                // Now we'll show a dialog with all conversion rows and the user will need to make some choices
                using (var form = new Prj2SoundsConversionDialog(version, conversionList))
                {
                    if (form.ShowDialog() == DialogResult.Cancel)
                        return false;
                }

                // Assign new Id and name
                foreach (var row in conversionList)
                {
                    if (row.SoundInfo != null)
                    {
                        row.SoundInfo.Id = row.NewId;
                        row.SoundInfo.Name = row.NewName;
                    }
                }

                // We'll export only embedded sound sources
                var newSounds = new WadSounds();

                // Remap sound sources
                foreach (var room in level.Rooms)
                {
                    if (room != null)
                        foreach (var obj in room.Objects)
                        {
                            if (obj is SoundSourceInstance)
                            {
                                SoundSourceInstance soundSource = obj as SoundSourceInstance;
                                if (soundSource.WadReferencedSoundName != null && soundSource.WadReferencedSoundName != "")
                                {
                                    soundSource.SoundId = -1;
                                    foreach (var row in conversionList)
                                        if (row.OldName == soundSource.WadReferencedSoundName && row.NewId != -1)
                                        {
                                            soundSource.SoundId = row.NewId;
                                            break;
                                        }

                                    soundSource.WadReferencedSoundName = "";
                                    soundSource.EmbeddedSoundInfo = null;
                                }
                                else if (soundSource.EmbeddedSoundInfo != null)
                                {
                                    // We export embedded sound infos
                                    newSounds.SoundInfos.Add(soundSource.EmbeddedSoundInfo);

                                    soundSource.SoundId = -1;
                                    foreach (var row in conversionList)
                                        if (row.SoundInfo == soundSource.EmbeddedSoundInfo && row.NewId != -1)
                                        {
                                            soundSource.SoundId = row.NewId;
                                            break;
                                        }

                                    soundSource.WadReferencedSoundName = "";
                                    soundSource.EmbeddedSoundInfo = null;
                                }
                            }
                        }
                }

                // Export samples
                foreach (var row in conversionList)
                    if (row.SoundInfo != null)
                    {
                        var samples = new List<string>();
                        foreach (var sample in row.SoundInfo.EmbeddedSamples)
                        {
                            if (sample.IsLoaded)
                            {
                                string sampleName =Path.GetFileNameWithoutExtension(dest) + "_" + row.NewName.ToLower() + "_" + row.SoundInfo.EmbeddedSamples.IndexOf(sample) + ".wav";
                                samples.Add(sampleName);
                                File.WriteAllBytes(Path.GetDirectoryName(dest) + "\\" + sampleName, sample.Data);
                            }
                        }

                        row.SoundInfo.EmbeddedSamples.Clear();
                        foreach (var sample in samples)
                            row.SoundInfo.EmbeddedSamples.Add(new WadSample(sample));
                    }

                // Sort sound infos
                sounds.SoundInfos.Sort((a, b) => a.Id.CompareTo(b.Id));

                // Make a backup copy
                if (save && src == dest)
                {
                    int index = 0;
                    string backupFilename = "";
                    while (true)
                    {
                        backupFilename = dest + "." + index + ".bak";
                        if (!File.Exists(backupFilename))
                            break;
                        index++;
                    }

                    File.Copy(src, backupFilename, true);
                }

                // Save Xml to file
                string xmlFileName = Path.GetDirectoryName(dest) + "\\" + Path.GetFileNameWithoutExtension(dest) + ".xml";
                WadSounds.SaveToXml(xmlFileName, newSounds);

                // Assign Xml to level settings
                level.Settings.SoundsCatalogs.Add(new ReferencedSoundsCatalog(level.Settings,
                    level.Settings.MakeRelative(soundsCatalog, VariableType.LevelDirectory)));
                level.Settings.SoundsCatalogs.Add(new ReferencedSoundsCatalog(level.Settings,
                    level.Settings.MakeRelative(xmlFileName, VariableType.LevelDirectory)));
                level.Settings.SoundSystem = SoundSystem.Xml;

                // Assign sounds if possible
                foreach (var soundRef in level.Settings.SoundsCatalogs)
                    if (soundRef.LoadException == null)
                        foreach (var sound in soundRef.Sounds.SoundInfos)
                            if (sound.Global && !level.Settings.SelectedSounds.Contains(sound.Id))
                                level.Settings.SelectedSounds.Add(sound.Id);

                // Save Prj2 with Xml sounds
                if (save)
                    using (var stream = File.OpenWrite(dest))
                        Prj2Writer.SaveToPrj2(stream, level);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void Log(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
