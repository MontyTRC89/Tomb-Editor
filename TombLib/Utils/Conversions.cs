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

namespace TombLib.Utils
{
    // This class was created for all conversion methos that re-process Wad2 and Prj2 files due to breaking changes
    // Let's hope that we must use this class almost never
    public class Conversions
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
                    throw new InvalidOperationException("This Wad2 file doesn't need to be converted");

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

        public static bool ConvertPrj2ToNewSoundFormat(string src, string dest, string soundsCatalog)
        {
            try
            {
                // Load sounds catalog
                WadSounds sounds = WadSounds.ReadFromFile(soundsCatalog);

                // Load Prj2
                Level level = Prj2Loader.LoadFromPrj2(src, null);

                // Check for sound system
                if (level.Settings.SoundSystem == SoundSystem.Xml)
                    throw new InvalidOperationException("This Prj2 file doesn't need to be converted");

                // Collect all sounds to remap
                var conversionList = new List<SoundInfoConversionRow>();
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
                                        var row = new SoundInfoConversionRow(null, soundSource.WadReferencedSoundName);
                                        row.NewId = -1;
                                        row.NewName = "";
                                        conversionList.Add(row);
                                    }
                                }
                                else if (soundSource.EmbeddedSoundInfo != null)
                                {
                                    var row = new SoundInfoConversionRow(soundSource.EmbeddedSoundInfo, soundSource.EmbeddedSoundInfo.Name);
                                    row.NewId = -1;
                                    row.NewName = "";
                                    conversionList.Add(row);
                                }
                            }
                        }

                    if (room != null)
                        foreach (var trigger in room.Triggers)
                        {
                            /*if (obj is SoundSourceInstance)
                            {
                                SoundSourceInstance soundSource = obj as SoundSourceInstance;
                                if (soundSource.WadReferencedSoundName != null && soundSource.WadReferencedSoundName != "")
                                {
                                    if (!conversionRows.Select(f => f.SoundInfo.Name).Contains(soundSource.WadReferencedSoundName))
                                    {
                                        var info = level.Settings.WadTryGetSoundInfo(soundSource.WadReferencedSoundName);
                                        if (info == null)
                                            continue;

                                        var newInfo = TrCatalog.TryGetSoundInfoIdByDescription(row.SoundInfo.Name);
                                        var row = new SoundInfoConversionRow(info);
                                        row.NewId = newInfo
                                        row.NewName =
                                    }
                                }
                                else if (soundSource.EmbeddedSoundInfo != null)
                                {
                                }
                                //
                            }*/
                        }
                }

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

                // Now we'll show a dialog with all conversion rows and the user will need to make some choices
                using (var form = new Wad2SoundsConversionDialog(version, conversionList))
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
                                    foreach (var row in conversionList)
                                        if (row.OldName == soundSource.WadReferencedSoundName && row.NewId != -1)
                                        {
                                            soundSource.SoundId = row.NewId;
                                            soundSource.WadReferencedSoundName = "";
                                            soundSource.EmbeddedSoundInfo = null;
                                            break;
                                        }
                                }
                                else if (soundSource.EmbeddedSoundInfo != null)
                                {
                                    // We export embedded sound infos
                                    newSounds.SoundInfos.Add(soundSource.EmbeddedSoundInfo);

                                    foreach (var row in conversionList)
                                        if (row.SoundInfo == soundSource.EmbeddedSoundInfo && row.NewId != -1)
                                        {
                                            soundSource.SoundId = row.NewId;
                                            soundSource.WadReferencedSoundName = "";
                                            soundSource.EmbeddedSoundInfo = null;
                                            break;
                                        }
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
                                string sampleName = row.NewName.ToLower() + "_" + row.SoundInfo.EmbeddedSamples.IndexOf(sample) + ".wav";
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

                // Save Xml to file
                string xmlFileName = Path.GetDirectoryName(dest) + "\\" + Path.GetFileNameWithoutExtension(dest) + ".xml";
                WadSounds.SaveToXml(xmlFileName, newSounds);

                // Assign Xml to level settings
                level.Settings.BaseSoundsXmlFilePath = PathC.GetRelativePath(Path.GetDirectoryName(dest), soundsCatalog);
                level.Settings.BaseSounds = sounds;
                level.Settings.CustomSoundsXmlFilePath = Path.GetFileName(xmlFileName);
                level.Settings.CustomSounds = newSounds;
                level.Settings.SoundSystem = SoundSystem.Xml;

                // Save Prj2 with Xml sounds
                using (var stream = File.OpenWrite(dest))
                    Prj2Writer.SaveToPrj2(stream, level);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
