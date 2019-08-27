using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TombLib.Utils;
using TombLib.Wad.Catalog;

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
            throw new ArgumentException("Invalid file format");
        }

        public static WadSounds ReadFromXml(string filename)
        {
            var sounds = new WadSounds();

            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var serializer = new XmlSerializer(typeof(WadSounds));
                sounds = (WadSounds)serializer.Deserialize(fileStream);
            }

            foreach (var soundInfo in sounds.SoundInfos)
                soundInfo.Xml = filename;

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

        public static WadSounds ReadFromTxt(string filename)
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
