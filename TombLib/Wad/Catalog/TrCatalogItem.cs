using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad.Catalog
{
    internal class TrCatalogItem
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Icon { get; private set; }

        public TrCatalogItem(int objectId, string name)
        {
            Id = objectId;
            Name = name;
        }
    }
}
