using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool ConvertWad2ToNewSoundFormat(string src, string dest)
        {
            try
            {
                // Load Wad2
                Wad2 wad = Wad2Loader.LoadFromFile(src, false);

                // Check if the Wad2 needs to be converted
                if (wad.SoundSystem != SoundSystem.Dynamic)
                    return true;

                // Now collect all sound infos from obsolete lists and build a new list
                var soundInfos = wad.AllLoadedSoundInfos.Values.ToList();

                // Loop through each sound info and try to get the classic Id from TrCatalog.xml
                var conversionList = new List<SoundInfoConversionRow>();
                foreach (var soundInfo in soundInfos)
                {
                    var row = new SoundInfoConversionRow(soundInfo, soundInfo.Name);

                    // If user has changed name, result will be -1 and the user will need to manually set the new sound id
                    row.NewId = TrCatalog.TryGetSoundInfoIdByDescription(wad.GameVersion, soundInfo.Name);
                    if (row.NewId != -1)
                    {
                        row.SaveToXml = true;
                        row.NewName = TrCatalog.GetOriginalSoundName(wad.GameVersion, (uint)row.NewId);
                    }

                    conversionList.Add(row);
                }

                // Now we'll show a dialog with all conversion rows and the user will need to make some choices
                WadSounds sounds = null;
                using (var form = new Wad2SoundsConversionDialog(wad.GameVersion, conversionList))
                {
                    if (form.ShowDialog() == DialogResult.Cancel)
                        return false;

                    if (form.Sounds != null)
                        sounds = form.Sounds;
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
                if (sounds != null)
                    foreach (var row in conversionList)
                        if (row.SaveToXml)
                        {
                            if (row.ExportSamples)
                            {
                                var samples = new List<string>();
                                foreach (var sample in row.SoundInfo.Samples)
                                {
                                    if (sample.IsLoaded)
                                    {
                                        string sampleName = row.NewName.ToLower() + "_" + row.SoundInfo.Samples.IndexOf(sample) + ".wav";
                                        samples.Add(sampleName);
                                        File.WriteAllBytes(Path.GetDirectoryName(dest) + "\\" + sampleName, sample.Data);
                                    }
                                }

                                row.SoundInfo.Samples.Clear();
                                foreach (var sample in samples)
                                    row.SoundInfo.Samples.Add(new WadSample(sample));
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
                // Check for sound system
                if (level.Settings.SoundSystem == SoundSystem.Xml)
                    return true;

                // Infer the wad version from level version
                TRVersion.Game version = level.Settings.GameVersion.Native();

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

                                    // Let's first try a search in TrCatalog, maybe we are lucky
                                    // First try to get sound name from TrCatalog
                                    int newId = TrCatalog.TryGetSoundInfoIdByDescription(version, soundSource.EmbeddedSoundInfo.Name);

                                    var row = new SoundInfoConversionRow(soundSource.EmbeddedSoundInfo, soundSource.EmbeddedSoundInfo.Name);
                                    if (newId == -1)
                                    {
                                        // If sound was not found in catalog, then assign a generic Id and ask to the user
                                        row.NewName = Regex.Replace(soundSource.EmbeddedSoundInfo.Name, "[^A-Za-z0-9 _]", "").ToUpper();
                                        row.NewId = lastSoundId++;
                                    }
                                    else
                                    {
                                        // Otherwise, we are lucky, and we can just assign the correct Id
                                        row.NewName = TrCatalog.GetOriginalSoundName(version, (uint)newId);
                                        row.NewId = newId;
                                    }

                                    // This flag is handle by Tomb Editor and set only for embedded sound sources
                                    row.ExportSamples = true;

                                    conversionList.Add(row);
                                }
                            }
                        }
                }

                WadSounds sounds = null;

                // Now we'll show a dialog with all conversion rows and the user will need to make some choices
                if (conversionList.Count != 0)
                    using (var form = new Prj2SoundsConversionDialog(version, conversionList))
                    {
                        if (form.ShowDialog() == DialogResult.Cancel)
                            return false;

                        if (form.Sounds != null)
                            sounds = form.Sounds;
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
                                    if (!newSounds.SoundInfos.Contains(soundSource.EmbeddedSoundInfo))
                                        newSounds.SoundInfos.Add(soundSource.EmbeddedSoundInfo);

                                    soundSource.SoundId = -1;
                                    foreach (var row in conversionList)
                                        if (row.SoundInfo == soundSource.EmbeddedSoundInfo && row.NewId != -1)
                                        {
                                            soundSource.SoundId = row.NewId;

                                            // Try to bind samples from additional catalog, if loaded
                                            if (sounds != null)
                                            {
                                                WadSoundInfo catalogInfo = sounds.TryGetSoundInfo(row.NewId);
                                                if (catalogInfo != null && catalogInfo.Samples.Count > 0)
                                                {
                                                    soundSource.EmbeddedSoundInfo.Samples.Clear();
                                                    soundSource.EmbeddedSoundInfo.Samples.AddRange(catalogInfo.Samples);
                                                    // TODO: in theory if valid samples are found in catalog, we shouldn't need to
                                                    // export them
                                                    row.ExportSamples = false;
                                                }
                                            }

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
                    if (row.SoundInfo != null && row.ExportSamples)
                    {
                        var samples = new List<string>();
                        foreach (var sample in row.SoundInfo.Samples)
                        {
                            if (sample.IsLoaded)
                            {
                                string sampleName = Path.GetFileNameWithoutExtension(dest) + "_" + row.NewName.ToLower() + "_" + row.SoundInfo.Samples.IndexOf(sample) + ".wav";
                                samples.Add(sampleName);
                                File.WriteAllBytes(Path.GetDirectoryName(dest) + "\\" + sampleName, sample.Data);
                            }
                        }

                        row.SoundInfo.Samples.Clear();
                        foreach (var sample in samples)
                            row.SoundInfo.Samples.Add(new WadSample(sample));
                    }

                // Sort sound infos
                newSounds.SoundInfos.Sort((a, b) => a.Id.CompareTo(b.Id));

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
                if (newSounds.SoundInfos.Count != 0)
                {
                    string xmlFileName = Path.GetDirectoryName(dest) + "\\" + Path.GetFileNameWithoutExtension(dest) + ".xml";
                    WadSounds.SaveToXml(xmlFileName, newSounds);

                    // Assign Xml to level settings
                    level.Settings.SoundsCatalogs.Add(new ReferencedSoundsCatalog(level.Settings,
                    level.Settings.MakeRelative(xmlFileName, VariableType.LevelDirectory)));
                }

                level.Settings.SoundSystem = SoundSystem.Xml;

                // Try to get Xml and SFX files
                foreach (var wadRef in level.Settings.Wads)
                    if (wadRef != null && wadRef.LoadException == null)
                    {
                        string wadPath = level.Settings.MakeAbsolute(wadRef.Path);
                        string extension = Path.GetExtension(wadPath).ToLower();

                        if (extension == ".wad")
                        {
                            string sfxPath = Path.GetDirectoryName(wadPath) + "\\" + Path.GetFileNameWithoutExtension(wadPath) + ".sfx";
                            if (File.Exists(sfxPath))
                            {
                                sounds = WadSounds.ReadFromFile(sfxPath);
                                if (sounds != null)
                                    level.Settings.SoundsCatalogs.Add(new ReferencedSoundsCatalog(level.Settings,
                                        level.Settings.MakeRelative(sfxPath, VariableType.LevelDirectory)));
                            }

                        }
                        else if (extension == ".wad2")
                        {
                            string xmlPath = Path.GetDirectoryName(wadPath) + "\\" + Path.GetFileNameWithoutExtension(wadPath) + ".xml";
                            if (File.Exists(xmlPath))
                            {
                                sounds = WadSounds.ReadFromFile(xmlPath);
                                if (sounds != null)
                                    level.Settings.SoundsCatalogs.Add(new ReferencedSoundsCatalog(level.Settings,
                                        level.Settings.MakeRelative(xmlPath, VariableType.LevelDirectory)));
                            }
                        }
                    }

                // Assign sounds if possible
                foreach (var soundRef in level.Settings.SoundsCatalogs)
                    if (soundRef.LoadException == null)
                        foreach (var sound in soundRef.Sounds.SoundInfos)
                            if (!level.Settings.SelectedSounds.Contains(sound.Id))
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
