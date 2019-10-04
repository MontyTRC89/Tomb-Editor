using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using TombLib.Wad.Tr4Wad;

namespace TombLib.Wad
{
    public class SamplePathInfo
    {
        public string Name { get; set; }
        public string FullPath { get; set; } = null;
        public bool Found { get { return (!string.IsNullOrEmpty(FullPath)) && File.Exists(FullPath); } }
    }

    public class WadSounds
    {
        public List<WadSoundInfo> SoundInfos { get; set; }

        public WadSounds()
        {
            SoundInfos = new List<WadSoundInfo>();
        }

        public WadSounds(IEnumerable<WadSoundInfo> soundInfos)
        {
            SoundInfos = soundInfos.Where(s => s != null).ToList();
        }

        public static WadSounds ReadFromFile(string filename)
        {
            string extension = Path.GetExtension(filename).ToLower();
            if (extension == ".xml")
                return ReadFromXml(filename);
            else if (extension == ".txt")
                return ReadFromTxt(filename);
            else if (extension == ".sfx")
                return ReadFromSfx(filename);
            throw new ArgumentException("Invalid file format");
        }

        private static WadSounds ReadFromXml(string filename)
        {
            var sounds = new WadSounds();

            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var serializer = new XmlSerializer(typeof(WadSounds));
                sounds = (WadSounds)serializer.Deserialize(fileStream);
            }

            foreach (var soundInfo in sounds.SoundInfos)
                soundInfo.SoundCatalog = filename;

            return sounds;
        }

        public static bool SaveToXml(string filename, WadSounds sounds)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var serializer = new XmlSerializer(typeof(WadSounds));
                serializer.Serialize(fileStream, sounds);
            }

            return true;
        }

        public WadSoundInfo TryGetSoundInfo(int id)
        {
            foreach (var soundInfo in SoundInfos)
                if (soundInfo.Id == id)
                    return soundInfo;
            return null;
        }

        public static string TryGetSamplePath(Level level, string name)
        {
            // Search the sample in all registered paths, in descending order
            for (int p = 0; p < level.Settings.OldWadSoundPaths.Count; p++)
            {
                string newPath = level.Settings.MakeAbsolute(level.Settings.OldWadSoundPaths[p].Path);
                if (newPath == null)
                    continue;

                newPath = Path.Combine(newPath, name);

                if (File.Exists(newPath))
                    return newPath;
            }

            return null;
        }

        private static WadSounds ReadFromSfx(string filename)
        {
            var samples = new List<string>();

            // Read version
            int version = 129;
            using (var readerVersion = new BinaryReaderEx(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                version = readerVersion.ReadInt32();

            // Read samples
            var samPath = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + ".sam";
            using (var readerSounds = new StreamReader(new FileStream(samPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                while (!readerSounds.EndOfStream)
                    samples.Add(readerSounds.ReadLine());

            // Read sounds
            int soundMapSize = 0;
            short[] soundMap; 
            var wadSoundInfos = new List<wad_sound_info>();
            var soundInfos = new List<WadSoundInfo>();

            var sfxPath = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + ".sfx";
            using (var readerSfx = new BinaryReaderEx(new FileStream(sfxPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                // Try to guess the WAD version
                readerSfx.BaseStream.Seek(740, SeekOrigin.Begin);
                short first = readerSfx.ReadInt16();
                short second = readerSfx.ReadInt16();
                version = ((first == -1 || second == (first + 1)) ? 130 : 129);
                readerSfx.BaseStream.Seek(0, SeekOrigin.Begin);

                soundMapSize = (version == 130 ? 2048 : 370);
                soundMap = new short[soundMapSize];
                for (var i = 0; i < soundMapSize; i++)
                {
                    soundMap[i] = readerSfx.ReadInt16();
                }

                var numSounds = readerSfx.ReadUInt32();

                for (var i = 0; i < numSounds; i++)
                {
                    var info = new wad_sound_info();
                    info.Sample = readerSfx.ReadUInt16();
                    info.Volume = readerSfx.ReadByte();
                    info.Range = readerSfx.ReadByte();
                    info.Chance = readerSfx.ReadByte();
                    info.Pitch = readerSfx.ReadByte();
                    info.Characteristics = readerSfx.ReadUInt16();
                    wadSoundInfos.Add(info);
                }
            }

            // Convert old data to new format
            for (int i = 0; i < soundMapSize; i++)
            {
                // Check if sound is defined at all
                if (soundMap[i] == -1 || soundMap[i] >= wadSoundInfos.Count)
                    continue;

                // Fill the new sound info
                var oldInfo = wadSoundInfos[soundMap[i]];
                var newInfo = new WadSoundInfo(i);
                newInfo.Name = TrCatalog.GetOriginalSoundName(TRVersion.Game.TR4, (uint)i);
                newInfo.Volume = (int)Math.Round(oldInfo.Volume * 100.0f / 255.0f);
                newInfo.RangeInSectors = oldInfo.Range;
                newInfo.Chance = (int)Math.Round(oldInfo.Chance * 100.0f / 255.0f);
                newInfo.PitchFactor = (int)Math.Round((oldInfo.Pitch > 127 ? oldInfo.Pitch - 256 : oldInfo.Pitch) * 100.0f / 128.0f);
                newInfo.RandomizePitch = ((oldInfo.Characteristics & 0x2000) != 0);
                newInfo.RandomizeVolume = ((oldInfo.Characteristics & 0x4000) != 0);
                newInfo.DisablePanning = ((oldInfo.Characteristics & 0x1000) != 0);
                newInfo.LoopBehaviour = (WadSoundLoopBehaviour)(oldInfo.Characteristics & 0x03);
                newInfo.SoundCatalog = sfxPath;

                // Read all samples linked to this sound info (for example footstep has 4 samples)
                int numSamplesInGroup = (oldInfo.Characteristics & 0x00fc) >> 2;
                for (int j = oldInfo.Sample; j < oldInfo.Sample + numSamplesInGroup; j++)
                {
                    newInfo.EmbeddedSamples.Add(new WadSample(samples[j]));
                }
                soundInfos.Add(newInfo);
            }

            var sounds = new WadSounds(soundInfos);

            return sounds;
        }

        private static WadSounds ReadFromTxt(string filename)
        {
            var sounds = new WadSounds();

            try
            {
                using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (var reader = new StreamReader(fileStream))
                    {
                        ushort soundId = 0;
                        while (!reader.EndOfStream)
                        {
                            var s = reader.ReadLine().Trim();

                            var sound = new WadSoundInfo(soundId);
                            sound.RangeInSectors = 10;
                            sound.SoundCatalog = filename;

                            int endOfSoundName = s.IndexOf(':');
                            if (endOfSoundName == -1)
                            {
                                soundId++;
                                continue;
                            }

                            sound.Name = s.Substring(0, s.IndexOf(':'));

                            var tokens = s.Split(' ', '\t');
                            if (tokens.Length == 1)
                            {
                                soundId++;
                                continue;
                            }

                            int currentToken = 1;

                            do
                            {
                                var token = tokens[currentToken];

                                if (token == "")
                                {
                                    currentToken++;
                                    continue;
                                }

                                short volume;
                                short chance;

                                if (token.Length == 1)
                                {
                                    if (token == "P")
                                    {
                                        sound.RandomizePitch = true;
                                    }

                                    if (token == "V")
                                    {
                                        sound.RandomizeVolume = true;
                                    }

                                    if (token == "N")
                                    {
                                        sound.DisablePanning = true;
                                    }

                                    if (token == "L")
                                    {
                                        sound.LoopBehaviour = WadSoundLoopBehaviour.Looped;
                                    }

                                    if (token == "R")
                                    {
                                        sound.LoopBehaviour = WadSoundLoopBehaviour.OneShotRewound;
                                    }

                                    if (token == "W")
                                    {
                                        sound.LoopBehaviour = WadSoundLoopBehaviour.OneShotWait;
                                    }
                                }
                                else if (!token.StartsWith("VOL") || !Int16.TryParse(token.Substring(3), out volume))
                                {
                                    if (!token.StartsWith("CH") || !Int16.TryParse(token.Substring(2), out chance))
                                    {
                                        if (!token.StartsWith("PIT"))
                                        {
                                            if (!token.StartsWith("RAD"))
                                            {
                                                if (token[0] != '#')
                                                {
                                                    // If I'm here, then it's a sample
                                                    // Let's save the name for loading it later
                                                    sound.EmbeddedSamples.Add(new WadSample(token + ".wav"));
                                                }
                                                else if (token == "#g")
                                                {
                                                    // Global sounds must be automatically checked on
                                                    sound.Global = true;
                                                }
                                            }
                                            else
                                            {
                                                sound.RangeInSectors = Int16.Parse(token.Substring(3));
                                            }
                                        }
                                        else
                                        {
                                            sound.PitchFactor = Int16.Parse(token.Substring(3));
                                        }
                                    }
                                    else
                                    {
                                        sound.Chance = Int16.Parse(token.Substring(2));
                                    }
                                }
                                else
                                {
                                    sound.Volume = Int16.Parse(token.Substring(3));
                                }

                                currentToken++;
                            }
                            while (currentToken < tokens.Length);

                            sounds.SoundInfos.Add(sound);
                            soundId++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return sounds;
        }

        public static IReadOnlyList<FileFormat> FormatExtensions { get; } = new List<FileFormat>()
        {
            new FileFormat("TRLE Txt format", "txt"),
            new FileFormat("Tomb Editor Xml format", "xml"),
            new FileFormat("Compiled TRLE Sfx/Sam", "sfx")
        };
    }
}
