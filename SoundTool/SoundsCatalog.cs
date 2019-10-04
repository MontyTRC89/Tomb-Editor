﻿using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TombLib.LevelData;

namespace SoundTool
{
    public class SoundsCatalog
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<TRVersion.Game, SortedDictionary<ushort, SoundCatalogInfo>> _catalog = new Dictionary<TRVersion.Game, SortedDictionary<ushort, SoundCatalogInfo>>();

        public static void LoadAllCatalogsFromXml(string path)
        {
            //LoadCatalog(path, TRVersion.Game.TR1);
            LoadCatalogFromXml(path + "\\TR2\\Sounds.xml", TRVersion.Game.TR2);
            LoadCatalogFromXml(path + "\\TR3\\Sounds.xml", TRVersion.Game.TR3);
            LoadCatalogFromXml(path + "\\TR4\\Sounds.xml", TRVersion.Game.TR4);
            LoadCatalogFromXml(path + "\\TR5\\Sounds.xml", TRVersion.Game.TR5);
        }

        public static bool LoadCatalogFromXml(string fileName, TRVersion.Game version)
        {
            var catalog = LoadCatalogFromXml(fileName);
            if (catalog == null)
                return false;
            _catalog[version.Native()] = catalog;
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

        public static bool SaveToXml(string fileName, TRVersion.Game version)
        {
            return SaveToXml(fileName, _catalog[version.Native()]);
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

        public static SoundCatalogInfo GetSound(TRVersion.Game version, uint id)
        {
            if (!_catalog.ContainsKey(version.Native()))
                return null;
            if (!_catalog[version.Native()].ContainsKey((ushort)id))
                return null;
            return _catalog[version.Native()][(ushort)id];
        }

        public static int GetSoundMapSize(TRVersion.Game version, bool isNg)
        {
            switch (version.Native())
            {
                case TRVersion.Game.TR1:
                    return 256;
                case TRVersion.Game.TR2:
                case TRVersion.Game.TR3:
                    return 370;
                case TRVersion.Game.TR4:
                    return isNg ? 4096 : 370;
                default:
                    return 450;
            }
        }

        public static SortedDictionary<ushort, SoundCatalogInfo> GetAllSounds(TRVersion.Game version)
        {
            return _catalog[version];
        }

        public static bool LoadCatalogFromTxt(string fileName, TRVersion.Game version)
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
                                    sound.Flags |= 0x2000;
                                }

                                if (token == "V")
                                {
                                    sound.FlagV = true;
                                    sound.Flags |= 0x4000;
                                }

                                if (token == "N")
                                {
                                    sound.FlagN = true;
                                    sound.Flags |= 0x1000;
                                }

                                if (token == "L")
                                {
                                    sound.FlagL = true;
                                    sound.Flags |= 0x03;
                                }

                                if (token == "R")
                                {
                                    sound.FlagP = true;
                                    sound.Flags |= 0x02;
                                }

                                if (token == "W")
                                {
                                    sound.FlagP = true;
                                    sound.Flags |= 0x01;
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
    }
}
