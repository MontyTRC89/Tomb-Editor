using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad.Catalog
{
    public class TrCatalogGame
    {
        public TombRaiderVersion Version { get; private set; }
        public Dictionary<int, TrCatalogItem> Moveables { get; private set; }
        public Dictionary<int, TrCatalogItem> Sprites { get; private set; }
        public Dictionary<int, TrCatalogItem> StaticMeshes { get; private set; }

        public TrCatalogGame(TombRaiderVersion version)
        {
            Version = version;
            Moveables = new Dictionary<int, TrCatalogItem>();
            Sprites = new Dictionary<int, TrCatalogItem>();
            StaticMeshes = new Dictionary<int, TrCatalogItem>();
        }
    }
}
