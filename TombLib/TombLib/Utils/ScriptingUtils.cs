using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TombLib.Utils
{
    public class ScriptingUtils
    {
        public static List<string> GetAllFunctionsNames(string path)
        {
            const string tableName = "LevelFuncs";
            string[] reservedNames = { "OnStart", "OnEnd", "OnLoad", "OnSave", "OnControlPhase" };
            try
            {
                var result = new List<string>();
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

                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
