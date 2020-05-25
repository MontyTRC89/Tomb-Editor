using System;
using System.Collections.Generic;
using System.Linq;

namespace TombLib.LevelData
{
    public static class TRVersion
    {
        public enum Game : long
        {
            TR1 = 1,
            TR2 = 2,
            TR3 = 3,
            TR4 = 4,
            TR5 = 5,
            TRNG = 16,
            TR5Main = 18
        }

        /// <summary> Wrapper for getting native game version, excluding TR5Main. Equal to deprecated WadGameVersion enum.</summary>
        public static Game Native(this Game ver) => ver == Game.TRNG ? Game.TR4 : ver;

        /// <summary> Wrapper for getting legacy game version, omitting both TRNG and TR5Main. Equal to deprecated TRVersion enum. </summary>
        public static Game Legacy(this Game ver) => ver == Game.TRNG ? Game.TR4 : (ver == Game.TR5Main ? Game.TR5 : ver);

        /// <summary> Wrapper for getting legacy game version, omitting both TRNG and TR5Main. Equal to deprecated TRVersion enum. </summary>
        public static bool UsesMainSfx(this Game ver) => ver == Game.TR2  || ver == Game.TR3;

        /// <summary> Base enumeration. Contains all possible game versions.
        public static List<Game> AllVersions => Enum.GetValues(typeof(Game)).OfType<Game>().ToList();

        /// <summary> Helper native (aka WadGameVersion) enumeration list. Can be used to populate various controls, like listbox. </summary>
        public static List<Game> NativeVersions => AllVersions.Where(item => item != Game.TRNG).ToList();

        /// <summary> Helper legacy (aka TRVersion) enumeration list. Can be used to populate various controls, like listbox. </summary>
        public static List<Game> LegacyVersions => NativeVersions.Where(item => item != Game.TR5Main).ToList();

        /// <summary> Helper compilable version enumeration list. Can be used to populate various controls, like listbox. </summary>
        public static List<Game> CompilableVersions(bool experimental)
        {
            if (experimental)
                return AllVersions.Where(item => item >= Game.TR2).ToList();
            else
                return AllVersions.Where(item => item >= Game.TR2 && item.Native() <= Game.TR4).ToList();
        }
    }

    /// Only for TR5+
    public enum Tr5LaraType : byte
    {
        Normal = 0,
        Catsuit = 3,
        Divesuit = 4,
        Invisible = 6
    }

    /// Only for TR5+
    public enum Tr5WeatherType : byte
    {
        Normal = 0,
        Rain = 1,
        Snow = 2
    }
}
