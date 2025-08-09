using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Utils;

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
            TR1X = 11, // Temporarily unused, only for TombIDE identification
            TR2X = 12, // Temporarily unused, only for TombIDE identification
            TRNG = 16,
            TombEngine = 18,
        }

        /// <summary> Wrapper for getting native game version, excluding TombEngine. Equal to deprecated WadGameVersion enum.</summary>
        public static Game Native(this Game ver) => ver == Game.TRNG ? Game.TR4 : ver;

        /// <summary> Wrapper for getting legacy game version, omitting both TRNG and TombEngine. Equal to deprecated TRVersion enum. </summary>
        public static Game Legacy(this Game ver) => ver == Game.TRNG ? Game.TR4 : (ver == Game.TombEngine ? Game.TR5 : ver);

        /// <summary> Wrapper for getting legacy game version, omitting both TRNG and TombEngine. Equal to deprecated TRVersion enum. </summary>
        public static bool UsesMainSfx(this Game ver) => ver is Game.TR2 or Game.TR2X or Game.TR3;

        /// <summary> Base enumeration. Contains all possible game versions.
        public static List<Game> AllVersions => Enum.GetValues<Game>()
            .Where(e => e is not Game.TR1X and not Game.TR2X) // Exclude TombIDE versions
            .ToList();

        /// <summary> Helper native (aka WadGameVersion) enumeration list. Can be used to populate various controls, like listbox. </summary>
        public static List<Game> NativeVersions => new()
        {
            Game.TR1, Game.TR2, Game.TR3, Game.TR4, Game.TR5, Game.TombEngine
        };

        /// <summary> Helper legacy (aka TRVersion) enumeration list. Can be used to populate various controls, like listbox. </summary>
        public static List<Game> LegacyVersions => new()
        {
            Game.TR1, Game.TR2, Game.TR3, Game.TR4, Game.TR5, Game.TRNG
        };
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

    public static class TextureFootStep
    {
        public enum Type : byte
        {
            Mud = 0,
            Snow = 1,
            Sand = 2,
            Gravel = 3,
            Ice = 4,
            Water = 5,
            Stone = 6,
            Wood = 7,
            Metal = 8,
            Marble = 9,
            Grass = 10,
            Concrete = 11,
            OldWood = 12,
            OldMetal = 13,

            Custom1 = 14, // Can be used in TRNG with FLEP
            Custom2 = 15,

            Custom3 = 16, // TEN only!
            Custom4 = 17,
            Custom5 = 18,
            Custom6 = 19,
            Custom7 = 20,
            Custom8 = 21,

            Count
        }

        // Helper UI function which gets the names of all available footstep sounds
        // according to selected game version.

        public static List<string> GetNames(TRVersion.Game version) =>
            GetNames(new LevelSettings() { GameVersion = version });

        public static List<string> GetNames(LevelSettings settings)
        {
            var result = new List<string>();

            foreach (Type sound in Enum.GetValues(typeof(Type)))
                result.Add(sound.ToString().SplitCamelcase());

            // Delete new values in case legacy engine is used
            if (settings.GameVersion != TRVersion.Game.TombEngine)
                result = result.Where(e => !e.Contains("Custom") || e == "Custom 1" || e == "Custom 2").ToList();

            return result.Where(e => e != "Count").ToList();
        }
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
