using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Utils;

namespace TombLib.LevelData.VisualTriggering
{
    // Condition type specifies how condition node will check condition.
    // Boolean means strict "is", numerical allows all kinds of operators,
    // such as =, !=, >=, <=, > or <. Range allows to check values between two
    // specific numericals.

    public enum ConditionType
    {
        Boolean,
        Numerical,
        Range
    }

    // For action node, argument type specifies how argument will be displayed
    // in UI and how incorrect values will be filtered out.

    public enum ArgumentType
    {
        Boolean,        // Listable
        Numerical,
        Vector3,
        String,
        Color,
        LuaScript,      // Listable
        Moveables,      // Listable
        Statics,        // Listable
        Cameras,        // Listable
        Sinks,          // Listable
        FlybyCameras,   // Listable
        Volumes,        // Listable
        Rooms,
        SoundEffects
    }

    public static class TriggerNodeEnumerations
    {
        public static IEnumerable<string> BuildList(Level level, ArgumentType type)
        {
            Func<int, string> formatSounds = (i) =>
            {
                var info = level?.Settings?.WadTryGetSoundInfo(i);
                if (info != null)
                    return info.Name;
                else
                    return i + ": --- Not present ---";
            };

            switch (type)
            {
                case ArgumentType.Boolean:
                    return new List<string>() { "true", "false" };

                case ArgumentType.Moveables:
                    return level.ExistingRooms
                        .SelectMany(room => room.Objects)
                        .OfType<MoveableInstance>()
                        .Where(o => !string.IsNullOrEmpty(o.LuaName))
                        .Select(o => o.LuaName);

                case ArgumentType.Statics:
                    return level.ExistingRooms
                        .SelectMany(room => room.Objects)
                        .OfType<StaticInstance>()
                        .Where(o => !string.IsNullOrEmpty(o.LuaName))
                        .Select(o => o.LuaName);

                case ArgumentType.Cameras:
                    return level.ExistingRooms
                        .SelectMany(room => room.Objects)
                        .OfType<CameraInstance>()
                        .Where(o => !string.IsNullOrEmpty(o.LuaName))
                        .Select(o => o.LuaName);

                case ArgumentType.Sinks:
                    return level.ExistingRooms
                        .SelectMany(room => room.Objects)
                        .OfType<SinkInstance>()
                        .Where(o => !string.IsNullOrEmpty(o.LuaName))
                        .Select(o => o.LuaName);

                case ArgumentType.FlybyCameras:
                    return level.ExistingRooms
                        .SelectMany(room => room.Objects)
                        .OfType<FlybyCameraInstance>()
                        .Where(o => !string.IsNullOrEmpty(o.LuaName))
                        .Select(o => o.LuaName);

                case ArgumentType.Volumes:
                    return level.ExistingRooms
                        .SelectMany(room => room.Objects)
                        .OfType<VolumeInstance>()
                        .Where(o => !string.IsNullOrEmpty(o.LuaName))
                        .Select(o => o.LuaName);

                case ArgumentType.Rooms:
                    return level.ExistingRooms.Select(r => r.Name);

                case ArgumentType.LuaScript:
                    return ScriptingUtils.GetAllFunctionsNames(level.Settings.MakeAbsolute(level.Settings.TenLuaScriptFile));

                case ArgumentType.SoundEffects:
                    return Enumerable.Range(0, level.Settings.GlobalSoundMap.Count()).Select(formatSounds);

                default:
                    return null;
            }
        }
    }
}
