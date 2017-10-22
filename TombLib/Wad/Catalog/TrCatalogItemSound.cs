using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad.Catalog
{
    internal class TrCatalogItemSound : TrCatalogItem
    {
        public bool Mandatory { get; private set; }

        public TrCatalogItemSound(int id, string name, bool mandatory)
            : base(id, name)
        {
            Mandatory = mandatory;
        }
    }
}
