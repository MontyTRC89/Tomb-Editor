using System;

namespace TombLib.LevelData
{
    public interface IHasLuaScriptID
    {
        string LuaScriptId { get; set; }
        void AllocateNewLuaScriptId();
        bool TrySetLuaScriptId(string newScriptId);
    }
}
