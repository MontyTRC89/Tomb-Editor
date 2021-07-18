using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public class ScriptingUtils
    {
        public static List<string> GetAllFunctionsNames(string path)
        {
            try
            {
                var result = new List<string>();
                var lines = File.ReadAllLines(path);

                foreach (string l in lines)
                {
                    string line = l.Trim();

                    if (line.StartsWith("function"))
                    {
                        if (line.Contains("--"))
                        {
                            line = line.Substring(0, line.IndexOf("--") - 1);
                        }

                        string functionName = line.Substring(line.IndexOf(" "), line.IndexOf("(") - line.IndexOf(" ")).Trim();
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
