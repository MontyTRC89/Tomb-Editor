using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace TombLib.Wad.Catalog
{
    public class TrCatalog
    {
        public struct OriginalNameInfo
        {
            public bool IsStatic { get; set; }
            public uint Id { get; set; }
        }

        private struct Item
        {
            public string Name { get; set; }
        }

        private struct ItemSound
        {
            public string Name { get; set; }
            public bool FixedByDefault { get; set; }
        }

        private class Game
        {
            internal WadGameVersion Version { get; private set; }
            internal Dictionary<uint, Item> Moveables { get; private set; } = new Dictionary<uint, Item>();
            internal Dictionary<uint, Item> SpriteSequences { get; private set; } = new Dictionary<uint, Item>();
            internal Dictionary<uint, Item> Statics { get; private set; } = new Dictionary<uint, Item>();
            internal Dictionary<uint, ItemSound> Sounds { get; private set; } = new Dictionary<uint, ItemSound>();
            internal Dictionary<string, OriginalNameInfo> OriginalNames { get; private set; } = new Dictionary<string, OriginalNameInfo>();
            //internal int SoundMapSize { get; set; }

            public Game(WadGameVersion version)
            {
                Version = version;
            }
        }

        private static Dictionary<WadGameVersion, Game> Games = new Dictionary<WadGameVersion, Game>();

        public static int PredictSoundMapSize(WadGameVersion wadVersion, bool IsNg, int numDemoData)
        {
            switch (wadVersion)
            {
                case WadGameVersion.TR1:
                    return 256;
                case WadGameVersion.TR2:
                case WadGameVersion.TR3:
                    return 370;
                case WadGameVersion.TR4_TRNG:
                    return IsNg && numDemoData != 0 ? numDemoData : 370;
                case WadGameVersion.TR5:
                    return 450;
                default:
                    throw new ArgumentOutOfRangeException("Unknown game version.");
            }
        }

        public static string GetMoveableName(WadGameVersion version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version, out game))
                return "Unknown #" + id;
            Item entry;
            if (!game.Moveables.TryGetValue(id, out entry))
                return "Unknown #" + id;
            return game.Moveables[id].Name;
        }

        public static string GetStaticName(WadGameVersion version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version, out game))
                return "Unknown #" + id;
            Item entry;
            if (!game.Statics.TryGetValue(id, out entry))
                return "Unknown #" + id;
            return game.Statics[id].Name;
        }

        public static string GetOriginalSoundName(WadGameVersion version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version, out game))
                return "Unknown #" + id;
            ItemSound entry;
            if (!game.Sounds.TryGetValue(id, out entry))
                return "Unknown #" + id;
            return game.Sounds[id].Name;
        }

        public static string GetSpriteSequenceName(WadGameVersion version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version, out game))
                return "Unknown #" + id;
            Item entry;
            if (!game.SpriteSequences.TryGetValue(id, out entry))
                return "Unknown #" + id;
            return game.SpriteSequences[id].Name;
        }

        public static OriginalNameInfo? GetSlotFromOriginalName(WadGameVersion version, string name)
        {
            Game game;
            if (!Games.TryGetValue(version, out game))
                return null;
            OriginalNameInfo entry;
            if (!game.OriginalNames.TryGetValue(name, out entry))
                return null;
            return entry;
        }

        public static bool IsSoundFixedByDefault(WadGameVersion version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version, out game))
                return false;
            ItemSound entry;
            if (!game.Sounds.TryGetValue(id, out entry))
                return false;
            return game.Sounds[id].FixedByDefault;
        }

        public static Dictionary<uint, string> GetAllMoveables(WadGameVersion version)
        {
            return Games[version].Moveables.ToDictionary(item => item.Key, item => item.Value.Name);
        }

        public static Dictionary<uint, string> GetAllStatics(WadGameVersion version)
        {
            return Games[version].Statics.ToDictionary(item => item.Key, item => item.Value.Name);
        }

        public static Dictionary<uint, string> GetAllSpriteSequences(WadGameVersion version)
        {
            return Games[version].SpriteSequences.ToDictionary(item => item.Key, item => item.Value.Name);
        }

        public static Dictionary<uint, string> GetAllSounds(WadGameVersion version)
        {
            return Games[version].Sounds.ToDictionary(item => item.Key, item => item.Value.Name);
        }

        public static string GetVersionString(WadGameVersion version)
        {
            switch (version)
            {
                case WadGameVersion.TR1:
                    return "Tomb Raider";
                case WadGameVersion.TR2:
                    return "Tomb Raider 2";
                case WadGameVersion.TR3:
                    return "Tomb Raider 3";
                case WadGameVersion.TR4_TRNG:
                    return "Tomb Raider 4";
                case WadGameVersion.TR5:
                    return "Tomb Raider 5";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void LoadCatalog(string fileName)
        {
            var document = new XmlDocument();
            document.Load(fileName);

            XmlNodeList gamesNodes = document.DocumentElement.SelectNodes("/game");
            foreach (XmlNode gameNode in document.DocumentElement.ChildNodes)
            {
                if (gameNode.Name != "game")
                    continue;

                var stringVersion = gameNode.Attributes["id"].Value;
                WadGameVersion version;
                if (stringVersion == "TR1")
                    version = WadGameVersion.TR1;
                else if (stringVersion == "TR2")
                    version = WadGameVersion.TR2;
                else if (stringVersion == "TR3")
                    version = WadGameVersion.TR3;
                else if (stringVersion == "TR4")
                    version = WadGameVersion.TR4_TRNG;
                else if (stringVersion == "TR5")
                    version = WadGameVersion.TR5;
                else
                    continue;

                Game game = new Game(version);

                // Parse moveables
                XmlNode moveables = gameNode.SelectSingleNode("moveables");
                if (moveables != null)
                    foreach (XmlNode moveableNode in moveables.ChildNodes)
                    {
                        if (moveableNode.Name != "moveable")
                            continue;

                        uint id = uint.Parse(moveableNode.Attributes["id"].Value);
                        string name = moveableNode.Attributes["name"].Value;
                        game.Moveables.Add(id, new Item { Name = name });
                    }

                // Parse statics
                XmlNode statics = gameNode.SelectSingleNode("statics");
                if (statics != null)
                    foreach (XmlNode staticNode in statics.ChildNodes)
                    {
                        if (staticNode.Name != "static")
                            continue;

                        uint id = uint.Parse(staticNode.Attributes["id"].Value);
                        string name = staticNode.Attributes["name"]?.Value ?? "";
                        game.Statics.Add(id, new Item { Name = name });
                    }

                // Parse sprite sequences
                XmlNode sounds = gameNode.SelectSingleNode("sounds");
                if (sounds != null)
                    foreach (XmlNode soundNode in sounds.ChildNodes)
                    {
                        if (soundNode.Name != "sound")
                            continue;

                        uint id = uint.Parse(soundNode.Attributes["id"].Value);
                        string name = soundNode.Attributes["name"]?.Value ?? "";
                        bool fixedByDefault = bool.Parse(soundNode.Attributes["fixed_by_default"]?.Value ?? "false");
                        game.Sounds.Add(id, new ItemSound { Name = name, FixedByDefault = fixedByDefault });
                    }

                // Parse sprite sequences
                XmlNode spriteSequences = gameNode.SelectSingleNode("sprite_sequences");
                if (spriteSequences != null)
                    foreach (XmlNode spriteSequenceNode in spriteSequences.ChildNodes)
                    {
                        if (spriteSequenceNode.Name != "sprite_sequence")
                            continue;

                        uint id = uint.Parse(spriteSequenceNode.Attributes["id"].Value);
                        string name = spriteSequenceNode.Attributes["name"].Value;
                        game.SpriteSequences.Add(id, new Item { Name = name });
                    }

                // Parse original names
                XmlNode originalNames = gameNode.SelectSingleNode("original_names");
                if (originalNames != null)
                    foreach (XmlNode originalNameNode in originalNames.ChildNodes)
                    {
                        if (originalNameNode.Name != "org")
                            continue;

                        bool isStatic = bool.Parse(originalNameNode.Attributes["is_static"].Value);
                        uint id = uint.Parse(originalNameNode.Attributes["id"].Value);
                        string name = originalNameNode.Attributes["name"].Value;
                        game.OriginalNames.Add(name, new OriginalNameInfo { IsStatic = isStatic, Id = id });
                    }
                Games.Add(version, game);
            }
        }
    }
}
