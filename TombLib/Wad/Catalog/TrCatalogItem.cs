using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad.Catalog
{
    public class TrCatalogItem
    {
        public int ObjectId { get; private set; }
        public string Name { get; private set; }
        public string Icon { get; private set; }

        public TrCatalogItem(int objectId, string name)
        {
            ObjectId = objectId;
            Name = name;
        }
    }
}
