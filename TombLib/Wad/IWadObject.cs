using System;
using TombLib.LevelData;

namespace TombLib.Wad
{
    public interface IWadObjectId : IComparable
    {
        string ToString(TRVersion.Game gameVersion);
    }
    public interface IWadObject
    {
        IWadObjectId Id { get; }
        string ToString(TRVersion.Game gameVersion);
    }
}
