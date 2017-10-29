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
        internal static Dictionary<TombRaiderVersion, TrCatalogGame> Games { get; private set; } = new Dictionary<TombRaiderVersion, TrCatalogGame>();

        public static string GetMoveableName(TombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version)) return "Unknown #" + id;
            if (!Games[version].Moveables.ContainsKey((int)id)) return "Unknown #" + id;
            return Games[version].Moveables[(int)id].Name;
        }

        public static string GetStaticName(TombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version)) return "Unknown #" + id;
            if (!Games[version].StaticMeshes.ContainsKey((int)id)) return "Unknown #" + id;
            return Games[version].StaticMeshes[(int)id].Name;
        }

        public static string GetSoundName(TombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version)) return "Unknown #" + id;
            if (!Games[version].Sounds.ContainsKey((int)id)) return "Unknown #" + id;
            return Games[version].Sounds[(int)id].Name;
        }

        public static string GetSpriteName(TombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version)) return "Unknown #" + id;
            if (!Games[version].Sprites.ContainsKey((int)id)) return "Unknown #" + id;
            return Games[version].Sprites[(int)id].Name;
        }

        public static bool IsSoundMandatory(TombRaiderVersion version, uint id)
        {
            if (!Games.ContainsKey(version)) return false;
            if (!Games[version].StaticMeshes.ContainsKey((int)id)) return false;
            return Games[version].Sounds[(int)id].Mandatory;
        }

        public static Dictionary<int, string> GetAllMoveables(TombRaiderVersion version)
        {
            var result = new Dictionary<int, string>();
            foreach (var item in Games[version].Moveables)
                result.Add(item.Key, item.Value.Name);
            return result;
        }

        public static Dictionary<int, string> GetAllStaticMeshes(TombRaiderVersion version)
        {
            var result = new Dictionary<int, string>();
            foreach (var item in Games[version].StaticMeshes)
                result.Add(item.Key, item.Value.Name);
            return result;
        }

        public static Dictionary<int, string> GetAllSprites(TombRaiderVersion version)
        {
            var result = new Dictionary<int, string>();
            foreach (var item in Games[version].Sprites)
                result.Add(item.Key, item.Value.Name);
            return result;
        }

        public static Dictionary<int, string> GetAllSounds(TombRaiderVersion version)
        {
            var result = new Dictionary<int, string>();
            foreach (var item in Games[version].Sounds)
                result.Add(item.Key, item.Value.Name);
            return result;
        }

        public static int GetSoundMapSize(TombRaiderVersion version, bool isNgWad130)
        {
            switch (version)
            {
                case TombRaiderVersion.TR1:
                    return 256;
                case TombRaiderVersion.TR2:
                case TombRaiderVersion.TR3:
                    return 370;
                case TombRaiderVersion.TR4:
                    return (isNgWad130 ? 2048 : 370);
                default:
                    return 450;
            }
            //return Games[version].SoundMapSize;
        }

        public static string GetVersionString(TombRaiderVersion version)
        {
            if (version == TombRaiderVersion.TR1) return "Tomb Raider I";
            if (version == TombRaiderVersion.TR2) return "Tomb Raider II";
            if (version == TombRaiderVersion.TR3) return "Tomb Raider III";
            if (version == TombRaiderVersion.TR4) return "Tomb Raider The Last Revelation";
            return "Tomb Raider Chronicles";
        }

        public static void LoadCatalog(string fileName)
        {
            var document = new XmlDocument();
            document.Load(fileName);

            XmlNodeList gamesNodes = document.DocumentElement.SelectNodes("/game");
            foreach (XmlNode gameNode in document.DocumentElement.ChildNodes)
            {
                if (gameNode.Name != "game") continue;

                var stringVersion = gameNode.Attributes["id"].Value;

                TombRaiderVersion version;
                if (stringVersion == "TR1")
                    version = TombRaiderVersion.TR1;
                else if (stringVersion == "TR2")
                    version = TombRaiderVersion.TR2;
                else if (stringVersion == "TR3")
                    version = TombRaiderVersion.TR3;
                else if (stringVersion == "TR4")
                    version = TombRaiderVersion.TR4;
                else if (stringVersion == "TR5")
                    version = TombRaiderVersion.TR5;
                else
                    continue;

                var game = new TrCatalogGame(version);

                foreach (XmlNode node in gameNode.ChildNodes)
                {
                    if (node.Name != "moveables") continue;

                    // Parse moveables
                    foreach (XmlNode moveableNode in node.ChildNodes)
                    {
                        if (moveableNode.Name != "moveable") continue;

                        var objectId = Int32.Parse(moveableNode.Attributes["id"].Value);
                        var objectName = moveableNode.Attributes["name"].Value;

                        var moveable = new TrCatalogItem(objectId, objectName);
                        game.Moveables.Add(objectId, moveable);
                    }
                }

                // Add standard static
                for (var i = 0; i < 100; i++)
                {
                    var staticMesh = new TrCatalogItem(i, "Static #" + i);
                    game.StaticMeshes.Add(i, staticMesh);
                }

                foreach (XmlNode node in gameNode.ChildNodes)
                {
                    if (node.Name != "sounds") continue;

                    // Size of the soundmap
                    var soundMapSize = 0;
                    if (version == TombRaiderVersion.TR1) soundMapSize = 256;
                    else if (version == TombRaiderVersion.TR2) soundMapSize = 370;
                    else if (version == TombRaiderVersion.TR3) soundMapSize = 370;
                    else if (version == TombRaiderVersion.TR4) soundMapSize = 370;
                    else if (version == TombRaiderVersion.TR5) soundMapSize = 450;

                    //game.SoundMapSize = soundMapSize;

                    // Parse sounds
                    foreach (XmlNode soundNode in node.ChildNodes)
                    {
                        if (soundNode.Name != "sound") continue;

                        var soundId = Int32.Parse(soundNode.Attributes["id"].Value);
                        var objectName = soundNode.Attributes["name"].Value;
                        var mandatory = (soundNode.Attributes["mandatory"] != null ? Boolean.Parse(soundNode.Attributes["mandatory"].Value) : false);

                        var sound = new TrCatalogItemSound(soundId, objectName, mandatory);
                        game.Sounds.Add(soundId, sound);
                    }
                }

                foreach (XmlNode node in gameNode.ChildNodes)
                {
                    if (node.Name != "sprites") continue;

                    // Parse sounds
                    foreach (XmlNode spriteNode in node.ChildNodes)
                    {
                        if (spriteNode.Name != "sprite") continue;

                        var spriteId = Int32.Parse(spriteNode.Attributes["id"].Value);
                        var objectName = spriteNode.Attributes["name"].Value;
                        
                        var sprite = new TrCatalogItem(spriteId, objectName);
                        game.Sprites.Add(spriteId, sprite);
                    }
                }

                Games.Add(version, game);
            }
        }
    }
}
