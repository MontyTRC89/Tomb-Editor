using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TombLib.Utils
{
    public class ScriptingUtils
    {
        const int _maxRecursionDepth = 32;

        public static List<string> GetAllFunctionsNames(string path, List<string> list = null, int depth = 0)
        {
            const string tableName = "LevelFuncs";
            const string includeStart = "require";
            const string reservedFunctionPrefix = "__";

            string[] reservedNames = { "OnStart", "OnEnd", "OnLoad", "OnSave", "OnControlPhase" };

            var result = list == null ? new List<string>() : list;

            try
            {
                if (!File.Exists(path))
                    return result;

                var lines = File.ReadAllLines(path, Encoding.GetEncoding(1252));

                foreach (string l in lines)
                {
                    string functionName = string.Empty;

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
                        continue;

                    int cPoint = line.IndexOf("--");
                    if (cPoint > 0)
                        line = line.Substring(0, cPoint - 1);
                    else if (cPoint == 0)
                        continue;

                    if (line.StartsWith("function " + tableName + "."))
                    {
                        int indexStart = line.IndexOf(".") + 1;
                        int indexEnd = line.IndexOf("(") - indexStart;
                        functionName = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.StartsWith(tableName + "."))
                    {
                        int indexStart = line.IndexOf(".") + 1;
                        int indexEnd = line.IndexOf("=") - indexStart;
                        functionName = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.Contains(includeStart))
                    {
                        int pos1 = line.IndexOf(includeStart) + includeStart.Length;
                        int pos2 = line.Length;
                        string subfile = line.Substring(pos1, pos2 - pos1);
                        pos1 = subfile.IndexOf("(") + 1;
                        pos2 = subfile.IndexOf(")");
                        subfile = subfile.Substring(pos1, pos2 - pos1).Replace('"', ' ').Trim();
                        subfile = Path.Combine(Path.GetDirectoryName(path), subfile + ".lua");

                        depth++;
                        if (depth <= _maxRecursionDepth)
                            GetAllFunctionsNames(subfile, result, depth);
                    }

                    if (functionName.StartsWith(reservedFunctionPrefix))
                        continue;

                    if (!result.Contains(functionName))
                        result.Add(functionName);
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
