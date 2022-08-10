using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TombLib.Utils;

namespace TombLib.LevelData
{
    // A small XML serializable class used to pass batch-compile list between TIDE and TE.
    // Usage: put a list of all PRJ2 files into "Files" field and OPTIONALLY define 
    // custom path for compiled levels in "Location" field. Then, save it as XML via provided
    // method and launch TE with sole command-line argument pointing to that XML.
    // After compilation, TE will automatically delete batch XML so no traces are left.

    public class BatchCompileList
    {
        public string Location { get; set; }
        public List<string> Files { get; private set; } = new List<string>();

        public static BatchCompileList ReadFromXml(string filename)
        {
            if (!File.Exists(filename))
                return null;

            return XmlUtils.ReadXmlFile<BatchCompileList>(filename);
        }

        public static bool SaveToXml(string filename, BatchCompileList list)
        {
            XmlUtils.WriteXmlFile(filename, list);
            return true; // Why return true? It's not like there will ever be a different result other than an exception. -Nickelony
        }
    }
}
