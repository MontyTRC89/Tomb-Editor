using System;

namespace TombLib.LevelData
{
    public interface IHasLuaName
    {
        string LuaName { get; set; }
        void AllocateNewLuaName();
        bool TrySetLuaName(string newScriptId);
    }
}
