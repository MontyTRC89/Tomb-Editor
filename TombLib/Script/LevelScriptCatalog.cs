using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Script
{
    public enum LevelScriptCatalogParameterType
    {
        String,
        Boolean,
        SInt8,
        Int8,
        Int16,
        Int32,
        Hex
    }

    public class LevelScriptCommand
    {
        public string Name { get; set; }
        public List<LevelScriptCatalogParameterType> Parameters { get; private set; } = new List<LevelScriptCatalogParameterType>(); 

        public LevelScriptCommand(string name, LevelScriptCatalogParameterType[] parameters)
        {
            Name = name;
            Parameters.AddRange(parameters);
        }
    }

    public class LevelScriptCatalog
    {
        public static Dictionary<string, LevelScriptCommand> Commands { get; private set; }

        public static void LoadCatalog()
        {
            Commands = new Dictionary<string, LevelScriptCommand>();

            Commands.Add("Name",
                         new LevelScriptCommand("Name", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                         }));

            Commands.Add("LoadCamera",
                         new LevelScriptCommand("LoadCamera", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int8
                         }));

            Commands.Add("LensFlare",
                         new LevelScriptCommand("LensFlare", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8
                         }));

            Commands.Add("Level",
                          new LevelScriptCommand("Level", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String,
                             LevelScriptCatalogParameterType.Int8
                          }));

            Commands.Add("Legend",
                         new LevelScriptCommand("Legend", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                         }));

            Commands.Add("Puzzle",
                         new LevelScriptCommand("Puzzle", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.String,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex
                         }));

            Commands.Add("Key",
                         new LevelScriptCommand("Key", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.String,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex
                         }));

            Commands.Add("Pickup",
                         new LevelScriptCommand("Pickup", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.String,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex
                         }));

            Commands.Add("Examine",
                         new LevelScriptCommand("Examine", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.String,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex
                         }));

            Commands.Add("PuzzleCombo",
                         new LevelScriptCommand("PuzzleCombo", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.String,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex
                         }));

            Commands.Add("PickupCombo",
                         new LevelScriptCommand("PickupCombo", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.String,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex
                         }));

            Commands.Add("KeyCombo",
                         new LevelScriptCommand("KeyCombo", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.String,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex,
                             LevelScriptCatalogParameterType.Hex
                         }));

            Commands.Add("Horizon",
                         new LevelScriptCommand("Horizon", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                         }));

            Commands.Add("Starfield",
                         new LevelScriptCommand("Starfield", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                         }));

            Commands.Add("ColAddHorizon",
                         new LevelScriptCommand("ColAddHorizon", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                         }));

            Commands.Add("Timer",
                         new LevelScriptCommand("Timer", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                         }));

            Commands.Add("NoLevel",
                         new LevelScriptCommand("NoLevel", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                         }));

            Commands.Add("Lightning",
                         new LevelScriptCommand("Lightning", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                         }));

            Commands.Add("YoungLara",
                         new LevelScriptCommand("YoungLara", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                         }));

            Commands.Add("ResetHUB",
                        new LevelScriptCommand("ResetHUB", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8
                        }));

            Commands.Add("RemoveAmulet",
                        new LevelScriptCommand("RemoveAmulet", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                        }));

            Commands.Add("Train",
                        new LevelScriptCommand("Train", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                        }));

            Commands.Add("Layer1",
                        new LevelScriptCommand("Layer1", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.SInt8
                        }));

            Commands.Add("Layer2",
                        new LevelScriptCommand("Layer2", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.SInt8
                        }));

            Commands.Add("ResidentCut",
                        new LevelScriptCommand("ResidentCut", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8
                        }));

            Commands.Add("Mirror",
                       new LevelScriptCommand("Mirror", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Hex
                       }));

            Commands.Add("FMV",
                       new LevelScriptCommand("FMV", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8
                       }));

            Commands.Add("Fog",
                       new LevelScriptCommand("Fog", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8
                       }));

            Commands.Add("UVrotate",
                       new LevelScriptCommand("UVrotate", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8
                       }));

        }
    }
}
