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
            TombEngine = 18
        }

        /// <summary> Wrapper for getting native game version, excluding TombEngine. Equal to deprecated WadGameVersion enum.</summary>
        public static Game Native(this Game ver) => ver == Game.TRNG ? Game.TR4 : ver;

        /// <summary> Wrapper for getting legacy game version, omitting both TRNG and TombEngine. Equal to deprecated TRVersion enum. </summary>
        public static Game Legacy(this Game ver) => ver == Game.TRNG ? Game.TR4 : (ver == Game.TombEngine ? Game.TR5 : ver);

        /// <summary> Wrapper for getting legacy game version, omitting both TRNG and TombEngine. Equal to deprecated TRVersion enum. </summary>
        public static bool UsesMainSfx(this Game ver) => ver == Game.TR2  || ver == Game.TR3;

        /// <summary> Base enumeration. Contains all possible game versions.
        public static List<Game> AllVersions => Enum.GetValues(typeof(Game)).OfType<Game>().ToList();

        /// <summary> Helper native (aka WadGameVersion) enumeration list. Can be used to populate various controls, like listbox. </summary>
        public static List<Game> NativeVersions => AllVersions.Where(item => item != Game.TRNG).ToList();

        /// <summary> Helper legacy (aka TRVersion) enumeration list. Can be used to populate various controls, like listbox. </summary>
        public static List<Game> LegacyVersions => NativeVersions.Where(item => item != Game.TombEngine).ToList();

        /// <summary> Helper compilable version enumeration list. Can be used to populate various controls, like listbox. </summary>
        public static List<Game> CompilableVersions(bool experimental)
        {
            if (experimental)
                return AllVersions.ToList();
            else
                return AllVersions.Where(item => item.Legacy() != Game.TR5).ToList();
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

    // Only for TEN
    public enum ShatterType : short
    {
        None = 0,
        Fragment = 1,
        Explode = 2
    }

    public static class StringEnums
    {
        public static readonly List<string> NGRoomTypes = new List<string>()
        {
            "Rain 1",
            "Rain 2",
            "Rain 3",
            "Rain 4",
            "Snow 1",
            "Snow 2",
            "Snow 3",
            "Snow 4"
        };

        public static readonly List<string> ReverberationTypes = new List<string>()
        {
            "None",
            "Small",
            "Medium",
            "Large",
            "Pipe",
        };

        public static readonly List<string> ExtraReverberationTypes = new List<string>()
        {
            "None",
            "Default",
            "Generic",
            "Padded Cell",
            "Room",
            "Bathroom",
            "Living Room",
            "Stone Room",
            "Auditorium",
            "Concert Hall",
            "Cave",
            "Arena",
            "Hangar",
            "Carpeted Hallway",
            "Hallway",
            "Stone Corridor",
            "Alley",
            "Forest",
            "City",
            "Mountains",
            "Quarry",
            "Plain",
            "Parking Lot",
            "Sewer Pipe",
            "Underwater",
            "Small Room",
            "Medium Room",
            "Large Room",
            "Medium Hall",
            "Large Hall",
            "Plate",
            "Custom 1",
            "Custom 2",
            "Custom 3",
            "Custom 4",
            "Custom 5",
            "Custom 6",
            "Custom 7",
            "Custom 8",
            "Custom 9",
            "Custom 10",
            "Custom 11",
            "Custom 12",
            "Custom 13",
            "Custom 14",
            "Custom 15",
            "Custom 16"
        };

    }
}
