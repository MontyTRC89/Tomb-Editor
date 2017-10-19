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
        public static Dictionary<TombRaiderVersion, TrCatalogGame> Games { get; private set; } = new Dictionary<TombRaiderVersion, TrCatalogGame>();

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

                Games.Add(version, game);
            }
        }
    }
}
