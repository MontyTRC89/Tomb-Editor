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

            Commands.Add("Background",
                         new LevelScriptCommand("Background", new LevelScriptCatalogParameterType[] {
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

            Commands.Add("Sky",
                         new LevelScriptCommand("Sky", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                         }));

            Commands.Add("ColAddHorizon",
                         new LevelScriptCommand("ColAddHorizon", new LevelScriptCatalogParameterType[] {
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

            Commands.Add("Timer",
                        new LevelScriptCommand("Timer", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                        }));

            Commands.Add("DistantFog",
                       new LevelScriptCommand("DistantFog", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8,
                             LevelScriptCatalogParameterType.Int8
                       }));

            Commands.Add("ResetInventory",
                        new LevelScriptCommand("ResetInventory", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                        }));

            Commands.Add("UnlimitedAir",
                        new LevelScriptCommand("UnlimitedAir", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                        }));

            Commands.Add("Weather",
                        new LevelScriptCommand("Weather", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                        }));

            Commands.Add("Rumble",
                        new LevelScriptCommand("Rumble", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Boolean
                        }));

            Commands.Add("LaraType",
                       new LevelScriptCommand("LaraType", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                       }));

            Commands.Add("AmbientTrack",
                      new LevelScriptCommand("AmbientTrack", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int16
                      }));

            Commands.Add("LoadScreen",
                   new LevelScriptCommand("LoadScreen", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                   }));

            Commands.Add("OnLevelStart",
                   new LevelScriptCommand("OnLevelStart", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                   }));

            Commands.Add("OnLevelFinish",
                 new LevelScriptCommand("OnLevelFinish", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                 }));

            Commands.Add("OnLoadGame",
                 new LevelScriptCommand("OnLoadGame", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                 }));

            Commands.Add("OnSaveGame",
                 new LevelScriptCommand("OnSaveGame", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                 }));

            Commands.Add("OnLaraDeath",
                 new LevelScriptCommand("OnLaraDeath", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                 }));

            Commands.Add("OnLevelControl",
                 new LevelScriptCommand("OnLevelControl", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                 }));

            Commands.Add("OnBeginFrame",
                 new LevelScriptCommand("OnBeginFrame", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                 }));

            Commands.Add("OnEndFrame",
                new LevelScriptCommand("OnEndFrame", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
                }));

            Commands.Add("LevelFile",
               new LevelScriptCommand("LevelFile", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.String
               }));

            Commands.Add("LaraStartPos",
                    new LevelScriptCommand("LaraStartPos", new LevelScriptCatalogParameterType[] {
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int32,
                             LevelScriptCatalogParameterType.Int16
                    }));
        }
    }
}
