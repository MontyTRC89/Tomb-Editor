using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Wad.Catalog
{
    public enum Limit
    {
        TexPages,
        TexInfos,
        SoundBitsPerSample,
        SoundSampleRate,
        SoundSampleSize,
        SoundSampleCount,
        SoundMapSize,
        ItemMaxCount,
        ItemSafeCount,
        BoxLimit,
        BoxMinCount,
        OverlapLimit,
        RoomVertexCount,
        RoomFaceCount,
        RoomLightCount,
        RoomDimensions,
        RoomMaxCount,
        RoomSafeCount,
        FloorHeight,
        CornerHeight,

        NG_SoundMapSize
    }

    public class TrCatalog
    {
        private struct Item
        {
            public List<string> Names { get; set; }
            public string Description { get; set; }
            public uint SkinId { get; set; }
            public int SubstituteId { get; set; }
            public bool AIObject { get; set; }
            public bool Shatterable { get; set; }
            public string TR5MainSlot { get; set; }
        }

        private struct ItemSound
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool FixedByDefault { get; set; }
        }

        private struct ItemAnimation
        {
            public uint Item { get; set; }
            public uint Animation { get; set; }
            public string Name { get; set; }
        }

        private struct ItemState
        {
            public uint Item { get; set; }
            public uint State { get; set; }
            public string Name { get; set; }
        }

        private class Game
        {
            internal TRVersion.Game Version { get; private set; }
            internal SortedList<Limit, int> Limits { get; private set; } = new SortedList<Limit, int>();
            internal SortedList<uint, Item> Moveables { get; private set; } = new SortedList<uint, Item>();
            internal SortedList<uint, Item> SpriteSequences { get; private set; } = new SortedList<uint, Item>();
            internal SortedList<uint, Item> Statics { get; private set; } = new SortedList<uint, Item>();
            internal SortedList<uint, ItemSound> Sounds { get; private set; } = new SortedList<uint, ItemSound>();
            internal List<ItemAnimation> Animations { get; private set; } = new List<ItemAnimation>();
            internal List<ItemState> States { get; private set; } = new List<ItemState>();

            public Game(TRVersion.Game version)
            {
                Version = version;
            }
        }

        private static readonly Dictionary<TRVersion.Game, Game> Games = new Dictionary<TRVersion.Game, Game>();

        public static int PredictSoundMapSize(TRVersion.Game version, bool IsNg, int numDemoData)
        {
            switch (version.Native())
            {
                case TRVersion.Game.TR1:
                    return 256;
                case TRVersion.Game.TR2:
                case TRVersion.Game.TR3:
                    return 370;
                case TRVersion.Game.TR4:
                    return IsNg && numDemoData != 0 ? numDemoData : 370;
                case TRVersion.Game.TR5:
                case TRVersion.Game.TR5Main:
                    return 450;
                default:
                    throw new ArgumentOutOfRangeException("Unknown game version.");
            }
        }

        public static string GetMoveableName(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return "Unknown #" + id;
            Item entry;
            if (!game.Moveables.TryGetValue(id, out entry))
                return "Unknown #" + id;
            return game.Moveables[id].Names.LastOrDefault();
        }

        public static string GetMoveableTR5MainSlot(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return "";
            Item entry;
            if (!game.Moveables.TryGetValue(id, out entry))
                return "";
            return game.Moveables[id].TR5MainSlot;
        }

        public static string GetSpriteSequenceTR5MainSlot(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return "";
            Item entry;
            if (!game.SpriteSequences.TryGetValue(id, out entry))
                return "";
            return game.SpriteSequences[id].TR5MainSlot;
        }

        public static int GetTR5MainSoundMapStart(TRVersion.Game version)
        {
            switch (version)
            {
                case TRVersion.Game.TR1:
                    return 450;
                case TRVersion.Game.TR2:
                    return (450 + 256);
                case TRVersion.Game.TR3:
                    return (450 + 256 + 370);
                case TRVersion.Game.TR4:
                case TRVersion.Game.TRNG:
                    return (450 + 256 + 370 + 370);
                case TRVersion.Game.TR5:
                    return 0;
                default:
                    return 0;
            }
        }

        public static uint GetMoveableSkin(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return id;
            Item entry;
            if (!game.Moveables.TryGetValue(id, out entry))
                return id;
            return game.Moveables[id].SkinId;
        }

        public static uint GetSubstituteID(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return id;
            Item entry;
            if (!game.Moveables.TryGetValue(id, out entry))
                return id;
            if (game.Moveables[id].SubstituteId == -1)
                return id;
            return (uint)game.Moveables[id].SubstituteId;
        }

        public static bool IsMoveableAI(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return false;
            Item entry;
            if (!game.Moveables.TryGetValue(id, out entry))
                return false;

            return entry.AIObject;
        }

        public static string GetStaticName(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return "Unknown #" + id;
            Item entry;
            if (!game.Statics.TryGetValue(id, out entry))
                return "Unknown #" + id;
            return game.Statics[id].Names.LastOrDefault();
        }

        public static bool IsStaticShatterable(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return false;
            Item entry;
            if (!game.Statics.TryGetValue(id, out entry))
                return false;

            return entry.Shatterable;
        }

        public static uint? GetItemIndex(TRVersion.Game version, string name, out bool isMoveable)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
            {
                isMoveable = false;
                return null;
            }

            var entry = game.Moveables.FirstOrDefault(item => item.Value.Names.Any(possibleName => possibleName.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
            if (entry.Value.Names != null)
            {
                isMoveable = true;
                return entry.Key;
            }

            entry = game.Statics.FirstOrDefault(item => item.Value.Names.Any(possibleName => possibleName.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
            if (entry.Value.Names != null)
            {
                isMoveable = false;
                return entry.Key;
            }

            entry = game.SpriteSequences.FirstOrDefault(item => item.Value.Names.Any(possibleName => possibleName.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
            if (entry.Value.Names != null)
            {
                isMoveable = false;
                return entry.Key;
            }

            isMoveable = false;
            return null;
        }

        public static string GetOriginalSoundName(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return "UNKNOWN_SOUND_" + id;
            ItemSound entry;
            if (!game.Sounds.TryGetValue(id, out entry))
                return "UNKNOWN_SOUND_" + id;
            return game.Sounds[id].Name;
        }

        public static int TryGetSoundInfoIdByDescription(TRVersion.Game version, string name)
        {
            var sounds = Games[version.Native()].Sounds;
            foreach (var pair in sounds)
                if (pair.Value.Description.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return (int)pair.Key;
            return -1;
        }

        public static string GetSpriteSequenceName(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return "Unknown #" + id;
            Item entry;
            if (!game.SpriteSequences.TryGetValue(id, out entry))
                return "Unknown #" + id;
            return game.SpriteSequences[id].Names.LastOrDefault();
        }

        public static bool IsSoundFixedByDefault(TRVersion.Game version, uint id)
        {
            Game game;
            if (!Games.TryGetValue(version.Native(), out game))
                return false;
            ItemSound entry;
            if (!game.Sounds.TryGetValue(id, out entry))
                return false;
            return game.Sounds[id].FixedByDefault;
        }

        public static string GetAnimationName(TRVersion.Game version, uint objectId, uint animId)
        {
            Game game;
            ItemAnimation entry = new ItemAnimation();
            Games.TryGetValue(version.Native(), out game);

            if (game != null)
                entry = game.Animations.FirstOrDefault(item => item.Item == objectId && item.Animation == animId);
            
            if (entry.Name == null)
            {
                var otherGames = Games.Where(g => g.Key != version.Native()).ToList();
                otherGames.Reverse();

                foreach (var otherGame in otherGames)
                {
                    entry = otherGame.Value.Animations.FirstOrDefault(item => item.Item == objectId && item.Animation == animId);
                    if (entry.Name != null) break;
                }
            }

            if (entry.Name == null) return "Animation " + animId;
            else return entry.Name;
        }

        public static string GetStateName(TRVersion.Game version, uint objectId, uint stateId)
        {
            Game game;
            ItemState entry = new ItemState();
            Games.TryGetValue(version.Native(), out game);

            if (game != null)
                entry = game.States.FirstOrDefault(item => item.Item == objectId && item.State == stateId);

            if (entry.Name == null)
            {
                var otherGames = Games.Where(g => g.Key != version.Native()).ToList();
                otherGames.Reverse();

                foreach (var otherGame in otherGames)
                {
                    entry = otherGame.Value.States.FirstOrDefault(item => item.Item == objectId && item.State == stateId);
                    if (entry.Name != null) break;
                }
            }

            if (entry.Name == null) return "Unknown state " + stateId;
            else return entry.Name;
        }

        public static int TryToGetStateID(TRVersion.Game version, uint objectId, string stateName)
        {
            Game game;
            ItemState entry = new ItemState();
            Games.TryGetValue(version.Native(), out game);
            
            if (game != null)
                entry = game.States.FirstOrDefault(item => item.Item == objectId && item.Name.ToLower().Contains(stateName.ToLower()));

            if (entry.Name == null)
                foreach (var otherGame in Games.Where(g => g.Key != version.Native()))
                {
                    entry = otherGame.Value.States.FirstOrDefault(item => item.Item == objectId && item.Name.ToLower().Contains(stateName.ToLower()));
                    if (entry.Name != null) break;
                }

            if (entry.Name == null) return -1;
            else return (int)entry.State;
        }

        public static int GetLimit(TRVersion.Game version, Limit limit)
        {
            Game game;
            int value = int.MinValue;
            Games.TryGetValue(version.Native(), out game);

            if (game != null)
                value = game.Limits.FirstOrDefault(item => item.Key == limit).Value;

            if (value == int.MinValue)
            {
                var otherGames = Games.Where(g => g.Key != version.Native()).ToList();
                otherGames.Reverse();

                foreach (var otherGame in otherGames)
                {
                    value = game.Limits.FirstOrDefault(item => item.Key == limit).Value;
                    if (value != int.MinValue) break;
                }
            }

            return value;
        }

        public static IDictionary<uint, string> GetAllMoveables(TRVersion.Game version)
        {
            return Games[version.Native()].Moveables.DicSelect(item => item.Value.Names.LastOrDefault());
        }

        public static IDictionary<uint, string> GetAllStatics(TRVersion.Game version)
        {
            return Games[version.Native()].Statics.DicSelect(item => item.Value.Names.LastOrDefault());
        }

        public static IDictionary<uint, string> GetAllSpriteSequences(TRVersion.Game version)
        {
            return Games[version.Native()].SpriteSequences.DicSelect(item => item.Value.Names.LastOrDefault());
        }

        public static IDictionary<uint, string> GetAllSounds(TRVersion.Game version)
        {
            return Games[version.Native()].Sounds.DicSelect(item => item.Value.Name);
        }

        public static IDictionary<uint, string> GetAllFixedByDefaultSounds(TRVersion.Game version)
        {
            return Games[version.Native()].Sounds
                .DicWhere(sound => sound.Value.FixedByDefault)
                .DicSelect(item => item.Value.Name);
        }

        public static string GetVersionString(TRVersion.Game version)
        {
            switch (version)
            {
                case TRVersion.Game.TR1:
                    return "Tomb Raider";
                case TRVersion.Game.TR2:
                    return "Tomb Raider 2";
                case TRVersion.Game.TR3:
                    return "Tomb Raider 3";
                case TRVersion.Game.TR4:
                    return "Tomb Raider 4";
                case TRVersion.Game.TRNG:
                    return "TRNG";
                case TRVersion.Game.TR5:
                    return "Tomb Raider 5";
                case TRVersion.Game.TR5Main:
                    return "TR5Main";
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
                TRVersion.Game version;
                if (stringVersion == "TR1")
                    version = TRVersion.Game.TR1;
                else if (stringVersion == "TR2")
                    version = TRVersion.Game.TR2;
                else if (stringVersion == "TR3")
                    version = TRVersion.Game.TR3;
                else if (stringVersion == "TR4")
                    version = TRVersion.Game.TR4;
                else if (stringVersion == "TR5")
                    version = TRVersion.Game.TR5;
                else if (stringVersion == "TR5Main")
                    version = TRVersion.Game.TR5Main;
                else
                    continue;

                Game game = new Game(version);

                // Parse limits
                XmlNode limits = gameNode.SelectSingleNode("limits");
                if (limits != null)
                {
                    var names = Enum.GetNames(typeof(Limit));

                    foreach (XmlNode limitNode in limits.ChildNodes)
                    {
                        if (limitNode.Name != "limit")
                            continue;

                        var name = string.Empty;
                        if (limitNode.Attributes["name"] != null)
                            name = limitNode.Attributes["name"].Value;

                        var value = int.MinValue;
                        if (limitNode.Attributes["value"] != null)
                            value = int.Parse(limitNode.Attributes["value"].Value);

                        if (string.IsNullOrEmpty(name) || !names.Any(n => n == name) || value == int.MinValue)
                            continue;

                        game.Limits.Add((Limit)Enum.Parse(typeof(Limit), name), value);
                    }
                }

                // Parse moveables
                XmlNode moveables = gameNode.SelectSingleNode("moveables");
                if (moveables != null)
                    foreach (XmlNode moveableNode in moveables.ChildNodes)
                    {
                        if (moveableNode.Name != "moveable")
                            continue;

                        uint id = uint.Parse(moveableNode.Attributes["id"].Value);
                        var names = (moveableNode.Attributes["name"]?.Value ?? "").Split('|');

                        var skinId = id;
                        if (moveableNode.Attributes["use_body_from"] != null)
                            skinId = uint.Parse(moveableNode.Attributes["use_body_from"].Value);

                        var substituteId = -1;
                        if (moveableNode.Attributes["id2"] != null)
                            substituteId = int.Parse(moveableNode.Attributes["id2"].Value);

                        bool isAI = bool.Parse(moveableNode.Attributes["ai"]?.Value ?? "false");

                        var tr5MainSlot = "";
                        if (moveableNode.Attributes["t5m"] != null)
                            tr5MainSlot = moveableNode.Attributes["t5m"].Value;

                        game.Moveables.Add(id, new Item { Names = new List<string>(names), 
                            SkinId = skinId, SubstituteId = substituteId, AIObject = isAI, TR5MainSlot = tr5MainSlot });
                    }

                // Parse statics
                XmlNode statics = gameNode.SelectSingleNode("statics");
                if (statics != null)
                    foreach (XmlNode staticNode in statics.ChildNodes)
                    {
                        if (staticNode.Name != "static")
                            continue;

                        uint id = uint.Parse(staticNode.Attributes["id"].Value);
                        var names = (staticNode.Attributes["name"]?.Value ?? "").Split('|');
                        bool shatter = bool.Parse(staticNode.Attributes["shatter"]?.Value ?? "false");
                        game.Statics.Add(id, new Item { Names = new List<string>(names), Shatterable = shatter });
                    }

                // Parse sounds
                XmlNode sounds = gameNode.SelectSingleNode("sounds");
                if (sounds != null)
                    foreach (XmlNode soundNode in sounds.ChildNodes)
                    {
                        if (soundNode.Name != "sound")
                            continue;

                        uint id = uint.Parse(soundNode.Attributes["id"].Value);
                        string name = soundNode.Attributes["name"]?.Value ?? "";
                        string description = soundNode.Attributes["description"]?.Value ?? "";
                        bool fixedByDefault = bool.Parse(soundNode.Attributes["hardcoded"]?.Value ?? "false");
                        game.Sounds.Add(id, new ItemSound { Name = name, FixedByDefault = fixedByDefault, Description = description });
                    }

                // Parse sprite sequences
                XmlNode spriteSequences = gameNode.SelectSingleNode("sprite_sequences");
                if (spriteSequences != null)
                    foreach (XmlNode spriteSequenceNode in spriteSequences.ChildNodes)
                    {
                        if (spriteSequenceNode.Name != "sprite_sequence")
                            continue;

                        var tr5MainSlot = "";
                        if (spriteSequenceNode.Attributes["t5m"] != null)
                            tr5MainSlot = spriteSequenceNode.Attributes["t5m"].Value;

                        uint id = uint.Parse(spriteSequenceNode.Attributes["id"].Value);
                        var names = (spriteSequenceNode.Attributes["name"]?.Value ?? "").Split('|');
                        game.SpriteSequences.Add(id, new Item { Names = new List<string>(names), TR5MainSlot = tr5MainSlot });
                    }

                // Parse animations
                XmlNode animations = gameNode.SelectSingleNode("animations");
                if (animations != null)
                    foreach (XmlNode originalNameNode in animations.ChildNodes)
                    {
                        if (originalNameNode.Name != "anim")
                            continue;

                        uint item = uint.Parse(originalNameNode.Attributes["item"].Value);
                        uint id = uint.Parse(originalNameNode.Attributes["id"].Value);
                        string name = originalNameNode.Attributes["name"].Value;

                        game.Animations.Add(new ItemAnimation { Name = name, Animation = id, Item = item });
                    }

                // Parse states
                XmlNode states = gameNode.SelectSingleNode("states");
                if (states != null)
                    foreach (XmlNode originalNameNode in states.ChildNodes)
                    {
                        if (originalNameNode.Name != "state")
                            continue;

                        uint item = uint.Parse(originalNameNode.Attributes["item"].Value);
                        uint id = uint.Parse(originalNameNode.Attributes["id"].Value);
                        string name = originalNameNode.Attributes["name"].Value;

                        game.States.Add(new ItemState { Name = name, State = id, Item = item });
                    }
                Games.Add(version, game);
            }
        }
    }
}
