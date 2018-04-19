using System;
using TombLib.Utils;

namespace TombLib.Wad
{
    public interface IWadObjectId : IComparable
    {
        string ToString(WadGameVersion gameVersion);
    }
    public interface IWadObject
    {
        IWadObjectId Id { get; }
        DataVersion Version { get; }
        string ToString(WadGameVersion gameVersion);
    }
}
