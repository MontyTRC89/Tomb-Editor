using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace ProjectConverter
{
    class Program
    {
        static bool ConvertProject(string source, string destPath)
        {
            try
            {
                // Load level and all related resources
                var level = Prj2Loader.LoadFromPrj2(source, null);
                if (level == null)
                {
                    Console.WriteLine("Error while loading level");
                    return false;
                }

                // Now convert resources to new format
                foreach (var wadRef in level.Settings.Wads)
                {
                    var wad = wadRef.Wad;
                    var newWad = new Wad2
                    {
                        GameVersion = TRVersion.Game.TR5Main
                    };

                    // Copy all objects to new wad
                    foreach (var moveable in wad.Moveables)
                    {
                        var newId=TrCatalog.
                        newWad.Add()
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        static void Main(string[] args)
        {
        }
    }
}
