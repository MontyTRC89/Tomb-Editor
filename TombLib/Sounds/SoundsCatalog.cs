using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.Wad.TrLevels;

namespace TombLib.Sounds
{
    public class SoundsCatalog
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static Dictionary<WadTombRaiderVersion, SortedDictionary<ushort, SoundCatalogInfo>> _catalog = new Dictionary<WadTombRaiderVersion, SortedDictionary<ushort, SoundCatalogInfo>>();

        public static void LoadAllCatalogsFromXml(string path)
        {
            //LoadCatalog(path, WadTombRaiderVersion.TR1);
            LoadCatalogFromXml(path + "\\TR2\\Sounds.xml", WadTombRaiderVersion.TR2);
            LoadCatalogFromXml(path + "\\TR3\\Sounds.xml", WadTombRaiderVersion.TR3);
            LoadCatalogFromXml(path + "\\TR4\\Sounds.xml", WadTombRaiderVersion.TR4);
            LoadCatalogFromXml(path + "\\TR5\\Sounds.xml", WadTombRaiderVersion.TR5);
        }

        public static bool LoadCatalogFromXml(string fileName, WadTombRaiderVersion version)
        {
            var catalog = LoadCatalogFromXml(fileName);
            if (catalog == null)
                return false;
            _catalog[version] = catalog;
            return true;
        }

        internal static SortedDictionary<ushort, SoundCatalogInfo> LoadCatalogFromXml(string fileName)
        {
            var dictionary = new SortedDictionary<ushort, SoundCatalogInfo>();

            try
            {
                var document = new XmlDocument();
                document.Load(fileName);

                XmlNodeList gamesNodes = document.DocumentElement.SelectNodes("/Sounds/Sound");
                foreach (XmlNode soundNode in document.DocumentElement.ChildNodes)
                {
                    if (soundNode.Name != "Sound")
                        continue;

                    var soundId = UInt16.Parse(soundNode.Attributes["Id"].Value);
                    var soundInfo = new SoundCatalogInfo();

                    foreach (XmlNode node in soundNode.ChildNodes)
                    {
                        if (node.Name == "Name")
                            soundInfo.Name = node.InnerText;
                        else if (node.Name == "Volume")
                            soundInfo.Volume = Int16.Parse(node.InnerText);
                        else if (node.Name == "Pitch")
                            soundInfo.Pitch = Int16.Parse(node.InnerText);
                        else if (node.Name == "Range")
                            soundInfo.Range = Int16.Parse(node.InnerText);
                        else if (node.Name == "Chance")
                            soundInfo.Chance = Int16.Parse(node.InnerText);
                        else if (node.Name == "L")
                            soundInfo.FlagL = Boolean.Parse(node.InnerText);
                        else if (node.Name == "N")
                            soundInfo.FlagN = Boolean.Parse(node.InnerText);
                        else if (node.Name == "V")
                            soundInfo.FlagV = Boolean.Parse(node.InnerText);
                        else if (node.Name == "R")
                            soundInfo.FlagR = Boolean.Parse(node.InnerText);
                        else if (node.Name == "P")
                            soundInfo.FlagP = Boolean.Parse(node.InnerText);
                        else if (node.Name == "NgLocked")
                            soundInfo.NgLocked = Boolean.Parse(node.InnerText);
                        else if (node.Name == "Mandatory")
                            soundInfo.MandatorySound = Boolean.Parse(node.InnerText);
                        else if (node.Name == "Samples")
                        {
                            foreach (XmlNode sampleNode in node.ChildNodes)
                            {
                                if (sampleNode.Name != "Sample")
                                    continue;
                                soundInfo.Samples.Add(sampleNode.InnerText);
                            }
                        }
                    }

                    dictionary.Add(soundId, soundInfo);
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "'SoundsCatalog.LoadCatalogFromXml' failed.");
                return null;
            }

            return dictionary;
        }

        public static bool SaveToXml(string fileName, WadTombRaiderVersion version)
        {
            return SaveToXml(fileName, _catalog[version]);
        }

        internal static bool SaveToXml(string fileName, SortedDictionary<ushort, SoundCatalogInfo> catalog)
        {
            try
            {
                using (var writer = new StreamWriter(File.OpenWrite(fileName)))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("\t<Sounds>");

                    var i = 0;
                    foreach (var pair in catalog)
                    {
                        var sound = pair.Value;

                        writer.WriteLine("\t\t<Sound Id=\"" + pair.Key + "\">");

                        writer.WriteLine("\t\t\t<Name>" + sound.Name.Replace("&", "&amp;") + "</Name>");
                        writer.WriteLine("\t\t\t<Volume>" + sound.Volume + "</Volume>");
                        writer.WriteLine("\t\t\t<Pitch>" + sound.Pitch + "</Pitch>");
                        writer.WriteLine("\t\t\t<Range>" + sound.Range + "</Range>");
                        writer.WriteLine("\t\t\t<Chance>" + sound.Chance + "</Chance>");
                        writer.WriteLine("\t\t\t<L>" + sound.FlagL + "</L>");
                        writer.WriteLine("\t\t\t<N>" + sound.FlagN + "</N>");
                        writer.WriteLine("\t\t\t<P>" + sound.FlagP + "</P>");
                        writer.WriteLine("\t\t\t<R>" + sound.FlagR + "</R>");
                        writer.WriteLine("\t\t\t<V>" + sound.FlagV + "</V>");
                        writer.WriteLine("\t\t\t<NgLocked>" + sound.NgLocked + "</NgLocked>");
                        writer.WriteLine("\t\t\t<Mandatory>" + sound.MandatorySound + "</Mandatory>");

                        writer.WriteLine("\t\t\t<Samples>");
                        foreach (var sample in sound.Samples)
                        {
                            writer.WriteLine("\t\t\t\t<Sample>" + sample.Replace("&", "&amp;") + "</Sample>");
                        }
                        writer.WriteLine("\t\t\t</Samples>");

                        writer.WriteLine("\t\t</Sound>");

                        i++;
                    }

                    writer.Write("\t</Sounds>");
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "'SoundsCatalog.SaveToXml' failed.");
                return false;
            }

            return true;
        }

        public static SoundCatalogInfo GetSound(WadTombRaiderVersion version, uint id)
        {
            if (!_catalog.ContainsKey(version))
                return null;
            if (!_catalog[version].ContainsKey((ushort)id))
                return null;
            return _catalog[version][(ushort)id];
        }

        public static int GetSoundMapSize(WadTombRaiderVersion version, bool isNg)
        {
            switch (version)
            {
                case WadTombRaiderVersion.TR1:
                    return 256;
                case WadTombRaiderVersion.TR2:
                case WadTombRaiderVersion.TR3:
                    return 370;
                case WadTombRaiderVersion.TR4:
                    return (isNg ? 4096 : 370);
                default:
                    return 450;
            }
        }

        public static SortedDictionary<ushort, SoundCatalogInfo> GetAllSounds(WadTombRaiderVersion version)
        {
            return _catalog[version];
        }

        public static bool LoadCatalogFromTxt(string fileName, WadTombRaiderVersion version)
        {
            var catalog = LoadCatalogFromTxt(fileName);
            if (catalog == null)
                return false;
            _catalog[version] = catalog;
            return true;
        }

        internal static SortedDictionary<ushort, SoundCatalogInfo> LoadCatalogFromTxt(string fileName)
        {
            var dictionary = new SortedDictionary<ushort, SoundCatalogInfo>();

            try
            {
                using (var reader = new StreamReader(File.OpenRead(fileName)))
                {
                    ushort soundId = 0;
                    while (!reader.EndOfStream)
                    {
                        var s = reader.ReadLine().Trim();

                        var sound = new SoundCatalogInfo();

                        sound.Range = 10;

                        int endOfSoundName = s.IndexOf(':');
                        if (endOfSoundName == -1)
                        {
                            sound.Name = s;
                            sound.Unused = true;
                            dictionary.Add(soundId, sound);
                            soundId++;
                            continue;
                        }

                        sound.Name = s.Substring(0, s.IndexOf(':'));

                        var tokens = s.Split(' ', '\t');
                        if (tokens.Length == 1)
                        {
                            sound.Unused = true;
                            dictionary.Add(soundId, sound);
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
                                    sound.FlagP = true;
                                    sound.Flags |= (0x2000);
                                }

                                if (token == "V")
                                {
                                    sound.FlagV = true;
                                    sound.Flags |= (0x4000);
                                }

                                if (token == "N")
                                {
                                    sound.FlagN = true;
                                    sound.Flags |= (0x1000);
                                }

                                if (token == "L")
                                {
                                    sound.FlagL = true;
                                    sound.Flags |= (0x03);
                                }

                                if (token == "R")
                                {
                                    sound.FlagP = true;
                                    sound.Flags |= (0x02);
                                }

                                if (token == "W")
                                {
                                    sound.FlagP = true;
                                    sound.Flags |= (0x01);
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
                                            if (token[0] == '#')
                                            {
                                                sound.WadLetters.Add(token.Substring(1));
                                                if (token[1] == 'g') sound.MandatorySound = true;
                                            }
                                            else
                                            {
                                                // If I'm here, then it's a sample
                                                sound.Samples.Add(token);
                                            }
                                        }
                                        else
                                        {
                                            sound.Range = Int16.Parse(token.Substring(3));
                                        }
                                    }
                                    else
                                    {
                                        sound.Pitch = Int16.Parse(token.Substring(3));
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

                        dictionary.Add(soundId, sound);
                        soundId++;
                    }
                }
            }
            catch (Exception exc)
            {
                logger.Warn(exc, "'SoundsCatalog.LoadCatalogFromTxt' failed.");
                return null;
            }

            return dictionary;
        }

        public static bool ConvertTxtFileToXml(string input, string output)
        {
            var catalog = LoadCatalogFromTxt(input);
            if (catalog == null)
                return false;
            if (!SaveToXml(output, catalog))
                return false;

            return true;
        }

        public static void TestProcedure(IEnumerable<string> levels)
        {
            var dictionary = new SortedDictionary<ushort, SoundCatalogInfo>();
            var writeSamples = true;
            var samples = new List<tr_sample>();
            var samplesNames = new List<string>();

            foreach (var name in levels)
            {
                if (!name.ToLower().Contains(".tr2"))
                    continue;

                var level = new TrLevel();
                level.LoadLevel(name, "D:\\TR2\\data\\MAIN.SFX", "");

                var k = 0;
                if (writeSamples)
                {
                    foreach (var sample in level.Samples)
                    {
                        samples.Add(sample);
                        samplesNames.Add("sample" + k);

                        k++;

                    }
                    writeSamples = false;
                }

                for (var i = 0; i < 370; i++)
                {
                    if (!dictionary.ContainsKey((ushort)i) && level.SoundMap[i] != -1)
                    {
                        var oldInfo = level.SoundDetails[level.SoundMap[i]];
                        var newInfo = new SoundCatalogInfo();

                        // Fill the new sound info
                        newInfo.Name = TrCatalog.GetSoundName(WadTombRaiderVersion.TR2, (uint)i).Replace(" ", "_").ToUpper().Replace("/", "_")
                            .Replace(";", "_").Replace("(", "").Replace(")", "").Replace("?", "").Replace("!", "")
                            .Replace("-", "_").Replace(":", "").Replace("'", "");
                        newInfo.Name = newInfo.Name.Replace("__", "_");
                        newInfo.Name = newInfo.Name.Trim('_');
                        var sampleName = newInfo.Name.ToLower();

                        /* if (oldLevel.Version >= TrVersion.TR3)
                         {
                             newInfo.Volume = (short)(oldInfo.Volume * 100 / 255);
                             newInfo.Range = oldInfo.Range;
                             newInfo.Chance = (short)(oldInfo.Chance * 100 / 255);
                             newInfo.Pitch = (short)(oldInfo.Pitch * 100 / 127);
                         }
                         else
                         {*/
                        newInfo.Volume = (byte)oldInfo.Volume;
                        newInfo.Range = oldInfo.Range;
                        newInfo.Chance = (byte)oldInfo.Chance;
                        newInfo.Pitch = oldInfo.Pitch;
                        // }

                        newInfo.FlagP = ((oldInfo.Characteristics & 0x2000) != 0); // TODO: loop meaning changed between TR versions
                        newInfo.FlagV = ((oldInfo.Characteristics & 0x4000) != 0);
                        newInfo.FlagN = ((oldInfo.Characteristics & 0x1000) != 0);

                        if ((oldInfo.Characteristics & 0x03) == 0x03)
                            newInfo.FlagL = true;
                        else if ((oldInfo.Characteristics & 0x03) == 0x02)
                            newInfo.FlagR = true;
                        else if ((oldInfo.Characteristics & 0x03) == 0x01)
                            newInfo.FlagW = true;

                        int numSamplesInGroup = (oldInfo.Characteristics & 0x00fc) >> 2;

                        // Read all samples linked to this sound info (for example footstep has 4 samples)
                        // For old TRs, don't load samples, because they are in MAIN.SFX
                        // In theory, sound management for TR2 and TR3 should be done in external tool
                        for (int j = oldInfo.Sample; j < oldInfo.Sample + numSamplesInGroup; j++)
                        {
                            var realIndex = (int)level.SamplesIndices[j];
                            if (samplesNames[realIndex].StartsWith("sample"))
                                samplesNames[realIndex] = sampleName + (j - oldInfo.Sample);
                            newInfo.Samples.Add(samplesNames[realIndex]);
                        }

                        dictionary.Add((ushort)i, newInfo);
                    }
                }
            }

            SaveToXml("D:\\tr2\\data\\test\\Sounds.xml", dictionary);

            // Write samples
            for (var i = 0; i < samples.Count; i++)
            {
                using (BinaryWriter writer = new BinaryWriter(File.OpenWrite("D:\\tr2\\data\\test\\samples\\" + samplesNames[i] + ".wav")))
                    writer.Write(samples[i].Data);
            }
        }
    }
}
