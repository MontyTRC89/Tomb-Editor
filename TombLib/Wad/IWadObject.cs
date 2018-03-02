using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public interface IWadObjectId : IComparable
    {
        string ToString(WadGameVersion gameVersion);
    }
    public interface IWadObject
    {
        IWadObjectId Id { get; }
        string ToString(WadGameVersion gameVersion);
    }
}
