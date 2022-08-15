using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TombLib.Utils
{
    public class ScriptingUtils
    {
        public static List<string> GetAllFunctionsNames(string path, List<string> list = null)
        {
            const string tableName = "LevelFuncs";
            const string includeStart = "require(\"";
            const string includeEnd = "\")";

            string[] reservedNames = { "OnStart", "OnEnd", "OnLoad", "OnSave", "OnControlPhase" };

            var result = list == null ? new List<string>() : list;

            try
            {
                if (!File.Exists(path))
                    return result;

                var lines = File.ReadAllLines(path, Encoding.GetEncoding(1252));

                foreach (string l in lines)
                {
                    string line = l.Trim();
                    bool skip = false;
                    foreach (string name in reservedNames)
                    {
                        if (line.Contains(name))
                        {
                            skip = true;
                            break;
                        }
                    }

                    if (skip)
                    {
                        continue;
                    }

                    if (line.StartsWith("function " + tableName + "."))
                    {
                        if (line.Contains("--"))
                        {
                            line = line.Substring(0, line.IndexOf("--") - 1);
                        }
                        int indexStart = line.IndexOf(".") + 1;
                        int indexEnd = line.IndexOf("(") - indexStart;
                        string functionName = line.Substring(indexStart, indexEnd).Trim();
                        if (!result.Contains(functionName))
                        {
                            result.Add(functionName);
                        }
                    }
                    else if (line.StartsWith(tableName + "."))
                    {
                        if (line.Contains("--"))
                        {
                            line = line.Substring(0, line.IndexOf("--") - 1);
                        }
                        int indexStart = line.IndexOf(".") + 1;
                        int indexEnd = line.IndexOf("=") - indexStart;
                        string functionName = line.Substring(indexStart, indexEnd).Trim();
                        if (!result.Contains(functionName))
                        {
                            result.Add(functionName);
                        }
                    }
                    else if (line.Contains("require(\""))
                    {
                        string subfile;
                        int pos1 = line.IndexOf(includeStart) + includeStart.Length;
                        int pos2 = line.IndexOf(includeEnd);
                        subfile = line.Substring(pos1, pos2 - pos1);
                        subfile = Path.Combine(Path.GetDirectoryName(path), subfile + ".lua");

                        GetAllFunctionsNames(subfile, result);
                    }

                }

                return result;
            }
            catch
            {
                return result;
            }
        }
    }
}
