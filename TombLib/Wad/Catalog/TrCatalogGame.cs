using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad.Catalog
{
    internal class TrCatalogGame
    {
        internal TombRaiderVersion Version { get; private set; }
        internal Dictionary<int, TrCatalogItem> Moveables { get; private set; }
        internal Dictionary<int, TrCatalogItem> Sprites { get; private set; }
        internal Dictionary<int, TrCatalogItem> StaticMeshes { get; private set; }
        internal Dictionary<int, TrCatalogItemSound> Sounds { get; private set; }
        //internal int SoundMapSize { get; set; }

        public TrCatalogGame(TombRaiderVersion version)
        {
            Version = version;
            Moveables = new Dictionary<int, TrCatalogItem>();
            Sprites = new Dictionary<int, TrCatalogItem>();
            StaticMeshes = new Dictionary<int, TrCatalogItem>();
            Sounds = new Dictionary<int, TrCatalogItemSound>();
        }
    }
}
