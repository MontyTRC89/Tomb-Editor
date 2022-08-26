using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TombLib.LevelData.VisualScripting;

namespace TombLib.Utils
{
    public static class ScriptingUtils
    {
        private static readonly int _maxRecursionDepth = 32;
        private static readonly string[] _reservedNames = { "OnStart", "OnEnd", "OnLoad", "OnSave", "OnControlPhase" };
        private static readonly string _tableName = "LevelFuncs";
        private static readonly string _includeStart = "require";

        private static readonly string _reservedFunctionPrefix = "__";
        private static readonly string _commentPrefix = "--";

        private static readonly string _nodeNameId = "!name";
        private static readonly string _nodeTypeId = "!condition";
        private static readonly string _nodeArgumentId = "!arguments";
        private static readonly string _nodeArgumentLayout = "!argumentlayout";
        private static readonly string _nodeLayoutNewLine = "newline";

        private static List<string> ExtractValues(string source)
        {
            return source.Split('"').Where((item, index) => index % 2 != 0).ToList();
        }

        public static List<NodeFunction> GetAllNodeFunctions(string path, List<NodeFunction> list = null, int depth = 0)
        {
            var result = list == null ? new List<NodeFunction>() : list;

            try
            {
                if (!File.Exists(path))
                    return result;

                var lines = File.ReadAllLines(path, Encoding.GetEncoding(1252));
                var nodeFunction = new NodeFunction();

                foreach (string l in lines)
                {
                    string line = l.Trim();
                    int cPoint = line.IndexOf(_commentPrefix);

                    if (cPoint >= 0)
                    {
                        int start = cPoint + _commentPrefix.Length;
                        int end = line.Length;

                        var comment = line.Substring(start, end - start).Trim();

                        if (comment.StartsWith(_nodeTypeId, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            bool cond = false;
                            bool.TryParse(ExtractValues(comment.Substring(_nodeTypeId.Length, comment.Length - _nodeTypeId.Length)).LastOrDefault(), out cond);
                            nodeFunction.Conditional = cond;
                            continue;
                        }
                        else if (comment.StartsWith(_nodeNameId, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            nodeFunction.Name = ExtractValues(comment.Substring(_nodeNameId.Length, comment.Length - _nodeNameId.Length)).LastOrDefault();
                            continue;
                        }
                        else if (comment.StartsWith(_nodeArgumentId, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            var args = ExtractValues(comment.Substring(_nodeArgumentId.Length, comment.Length - _nodeArgumentId.Length));

                            foreach (var a in args)
                            {
                                var argType = ArgumentType.Numerical;
                                try { argType = (ArgumentType)Enum.Parse(typeof(ArgumentType), a); } catch { }
                                nodeFunction.Arguments.Add(argType);
                            }

                            continue;
                        }
                        else if (comment.StartsWith(_nodeArgumentLayout, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            var settings = ExtractValues(comment.Substring(_nodeNameId.Length, comment.Length - _nodeNameId.Length));

                            foreach (var s in settings)
                            {
                                var argLayout = new ArgumentLayout()
                                {
                                    NewLine = false,
                                    Width = 100.0f
                                };

                                var parameters = s.Split(',').Select(st => st.Trim());
                                foreach (var p in parameters)
                                {
                                    if (p.Equals(_nodeLayoutNewLine, StringComparison.InvariantCultureIgnoreCase))
                                        argLayout.NewLine = true;
                                    else
                                    {
                                        float width = 100.0f;
                                        if (float.TryParse(p, out width))
                                            argLayout.Width = width;
                                    }
                                }

                                if (nodeFunction.ArgumentLayout.Count < nodeFunction.Arguments.Count)
                                    nodeFunction.ArgumentLayout.Add(argLayout);
                            }
                            continue;
                        }
                    }

                    if (cPoint > 0)
                        line = line.Substring(0, cPoint - 1);
                    else if (cPoint == 0)
                        line = string.Empty;

                    if (line.StartsWith("function " + _tableName + "."))
                    {
                        int indexStart = line.IndexOf(".") + 1;
                        int indexEnd = line.IndexOf("(") - indexStart;
                        nodeFunction.Signature = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.StartsWith(_tableName + "."))
                    {
                        int indexStart = line.IndexOf(".") + 1;
                        int indexEnd = line.IndexOf("=") - indexStart;
                        nodeFunction.Signature = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.Contains(_includeStart))
                    {
                        int pos1 = line.IndexOf(_includeStart) + _includeStart.Length;
                        int pos2 = line.Length;
                        string subfile = line.Substring(pos1, pos2 - pos1);
                        pos1 = subfile.IndexOf("(") + 1;
                        pos2 = subfile.IndexOf(")");
                        subfile = subfile.Substring(pos1, pos2 - pos1).Replace('"', ' ').Trim();
                        subfile = Path.Combine(Path.GetDirectoryName(path), subfile + ".lua");

                        depth++;
                        if (depth <= _maxRecursionDepth)
                            GetAllNodeFunctions(subfile, result, depth);
                    }

                    if (string.IsNullOrEmpty(nodeFunction.Signature))
                        continue;

                    if (nodeFunction.Signature.StartsWith(_reservedFunctionPrefix))
                        continue;

                    if (!result.Contains(nodeFunction))
                    {
                        result.Add(nodeFunction);
                        nodeFunction = new NodeFunction();
                    }
                }

                return result;
            }
            catch
            {
                return result;
            }
        }

        public static List<string> GetAllFunctionNames(string path, List<string> list = null, int depth = 0)
        {
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

                    foreach (var name in _reservedNames)
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

                    if (line.StartsWith("function " + _tableName + "."))
                    {
                        int indexStart = line.IndexOf(".") + 1;
                        int indexEnd = line.IndexOf("(") - indexStart;
                        functionName = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.StartsWith(_tableName + "."))
                    {
                        int indexStart = line.IndexOf(".") + 1;
                        int indexEnd = line.IndexOf("=") - indexStart;
                        functionName = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.Contains(_includeStart))
                    {
                        int pos1 = line.IndexOf(_includeStart) + _includeStart.Length;
                        int pos2 = line.Length;
                        string subfile = line.Substring(pos1, pos2 - pos1);
                        pos1 = subfile.IndexOf("(") + 1;
                        pos2 = subfile.IndexOf(")");
                        subfile = subfile.Substring(pos1, pos2 - pos1).Replace('"', ' ').Trim();
                        subfile = Path.Combine(Path.GetDirectoryName(path), subfile + ".lua");

                        depth++;
                        if (depth <= _maxRecursionDepth)
                            GetAllFunctionNames(subfile, result, depth);
                    }
                    else
                        continue;

                    if (string.IsNullOrEmpty(functionName))
                        continue;

                    if (functionName.StartsWith(_reservedFunctionPrefix))
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
