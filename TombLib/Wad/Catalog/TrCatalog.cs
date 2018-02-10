using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TombLib.Wad.Catalog
{
    public class TrCatalog
    {
        private class Game
        {
            internal WadTombRaiderVersion Version { get; private set; }
            internal Dictionary<int, Item> Moveables { get; private set; }
            internal Dictionary<int, Item> Sprites { get; private set; }
            internal Dictionary<int, Item> StaticMeshes { get; private set; }
            internal Dictionary<int, ItemSound> Sounds { get; private set; }
            //internal int SoundMapSize { get; set; }

            public Game(WadTombRaiderVersion version)
            {
                Version = version;
                Moveables = new Dictionary<int, Item>();
                Sprites = new Dictionary<int, Item>();
                StaticMeshes = new Dictionary<int, Item>();
                Sounds = new Dictionary<int, ItemSound>();
            }
        }

        private class Item
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public string Icon { get; private set; }

            public Item(int objectId, string name)
            {
                Id = objectId;
                Name = name;
            }
        }

        private class ItemSound : Item
        {
            public bool Mandatory { get; private set; }

            public ItemSound(int id, string name, bool mandatory)
            : base(id, name)
        {
                Mandatory = mandatory;
            }
        }

        private static Dictionary<WadTombRaiderVersion, Game> Games = new Dictionary<WadTombRaiderVersion, Game>();

        public static string GetMoveableName(WadTombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version))
                return "Unknown #" + id;
            if (!Games[version].Moveables.ContainsKey((int)id))
                return "Unknown #" + id;
            return Games[version].Moveables[(int)id].Name;
        }

        public static string GetStaticName(WadTombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version))
                return "Unknown #" + id;
            if (!Games[version].StaticMeshes.ContainsKey((int)id))
                return "Unknown #" + id;
            return Games[version].StaticMeshes[(int)id].Name;
        }

        public static string GetSoundName(WadTombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version))
                return "Unknown #" + id;
            if (!Games[version].Sounds.ContainsKey((int)id))
                return "Unknown #" + id;
            return Games[version].Sounds[(int)id].Name;
        }

        public static string GetSpriteName(WadTombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version))
                return "Unknown #" + id;
            if (!Games[version].Sprites.ContainsKey((int)id))
                return "Unknown #" + id;
            return Games[version].Sprites[(int)id].Name;
        }

        public static bool IsSoundMandatory(WadTombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version))
                return false;
            if (!Games[version].StaticMeshes.ContainsKey((int)id))
                return false;
            return Games[version].Sounds[(int)id].Mandatory;
        }

        public static Dictionary<int, string> GetAllMoveables(WadTombRaiderVersion version)
        {
            var result = new Dictionary<int, string>();
            foreach (var item in Games[version].Moveables)
                result.Add(item.Key, item.Value.Name);
            return result;
        }

        public static Dictionary<int, string> GetAllStaticMeshes(WadTombRaiderVersion version)
        {
            var result = new Dictionary<int, string>();
            foreach (var item in Games[version].StaticMeshes)
                result.Add(item.Key, item.Value.Name);
            return result;
        }

        public static Dictionary<int, string> GetAllSprites(WadTombRaiderVersion version)
        {
            var result = new Dictionary<int, string>();
            foreach (var item in Games[version].Sprites)
                result.Add(item.Key, item.Value.Name);
            return result;
        }

        /*public static Dictionary<int, string> GetAllSounds(WadTombRaiderVersion version)
        {
            var result = new Dictionary<int, string>();
            foreach (var item in Games[version].Sounds)
                result.Add(item.Key, item.Value.Name);
            return result;
        }

        public static int GetSoundMapSize(WadTombRaiderVersion version, bool isNgWad130)
        {
            switch (version)
            {
                case WadTombRaiderVersion.TR1:
                    return 256;
                case WadTombRaiderVersion.TR2:
                case WadTombRaiderVersion.TR3:
                    return 370;
                case WadTombRaiderVersion.TR4:
                    return (isNgWad130 ? 2048 : 370);
                case WadTombRaiderVersion.TR5:
                    return 450;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }*/

        public static string GetVersionString(WadTombRaiderVersion version)
        {
            switch (version)
            {
                case WadTombRaiderVersion.TR1:
                    return "Tomb Raider I";
                case WadTombRaiderVersion.TR2:
                    return "Tomb Raider II";
                case WadTombRaiderVersion.TR3:
                    return "Tomb Raider III";
                case WadTombRaiderVersion.TR4:
                    return "Tomb Raider The Last Revelation";
                case WadTombRaiderVersion.TR5:
                    return "Tomb Raider Chronicles";
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

                WadTombRaiderVersion version;
                if (stringVersion == "TR1")
                    version = WadTombRaiderVersion.TR1;
                else if (stringVersion == "TR2")
                    version = WadTombRaiderVersion.TR2;
                else if (stringVersion == "TR3")
                    version = WadTombRaiderVersion.TR3;
                else if (stringVersion == "TR4")
                    version = WadTombRaiderVersion.TR4;
                else if (stringVersion == "TR5")
                    version = WadTombRaiderVersion.TR5;
                else
                    continue;

                var game = new Game(version);

                foreach (XmlNode node in gameNode.ChildNodes)
                {
                    if (node.Name != "moveables")
                        continue;

                    // Parse moveables
                    foreach (XmlNode moveableNode in node.ChildNodes)
                    {
                        if (moveableNode.Name != "moveable")
                            continue;

                        var objectId = Int32.Parse(moveableNode.Attributes["id"].Value);
                        var objectName = moveableNode.Attributes["name"].Value;

                        var moveable = new Item(objectId, objectName);
                        game.Moveables.Add(objectId, moveable);
                    }
                }

                // Add standard static
                for (var i = 0; i < 100; i++)
                {
                    var staticMesh = new Item(i, "Static #" + i);
                    game.StaticMeshes.Add(i, staticMesh);
                }

                foreach (XmlNode node in gameNode.ChildNodes)
                {
                    if (node.Name != "sounds")
                        continue;

                    // Parse sounds
                    foreach (XmlNode soundNode in node.ChildNodes)
                    {
                        if (soundNode.Name != "sound")
                            continue;

                        var soundId = Int32.Parse(soundNode.Attributes["id"].Value);
                        var objectName = soundNode.Attributes["name"].Value;
                        var mandatory = (soundNode.Attributes["mandatory"] != null ? Boolean.Parse(soundNode.Attributes["mandatory"].Value) : false);

                        var sound = new ItemSound(soundId, objectName, mandatory);
                        game.Sounds.Add(soundId, sound);
                    }
                }

                foreach (XmlNode node in gameNode.ChildNodes)
                {
                    if (node.Name != "sprites")
                        continue;

                    // Parse sounds
                    foreach (XmlNode spriteNode in node.ChildNodes)
                    {
                        if (spriteNode.Name != "sprite")
                            continue;

                        var spriteId = Int32.Parse(spriteNode.Attributes["id"].Value);
                        var objectName = spriteNode.Attributes["name"].Value;

                        var sprite = new Item(spriteId, objectName);
                        game.Sprites.Add(spriteId, sprite);
                    }
                }

                Games.Add(version, game);
            }
        }
    }
}
