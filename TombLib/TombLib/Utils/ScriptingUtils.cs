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

        private static readonly string _reservedFunctionPrefix = "__";
        private static readonly string _metadataPrefix = "!";
        private static readonly string _enumSplitterStart = "[";
        private static readonly string _enumSplitterEnd = "]";
        private static readonly char   _tabChar = '\t';

        private static readonly string _luaSplitter = ".";
        private static readonly string _luaInclude = "require";
        private static readonly string _luaFunc = "function";
        private static readonly string _luaIf = "if";
        private static readonly string _luaThen = "then";
        private static readonly string _luaElse = "else";
        private static readonly string _luaEnd = "end";
        private static readonly string _luaNull = "nil";
        private static readonly string _luaActivator = "activator";
        private static readonly string _luaIs = "=";
        private static readonly string _luaBrkOpen = "(";
        private static readonly string _luaBrkClose = ")";
        private static readonly string _luaSpace = " ";
        private static readonly string _luaSeparator = ",";
        private static readonly string _luaComment = "--";

        private static readonly string[] _reservedNames = { "OnStart", "OnEnd", "OnLoad", "OnSave", "OnControlPhase" };
        private static readonly string _levelFuncPrefix = "LevelFuncs" + _luaSplitter;

        private static readonly string _nodeNameId = _metadataPrefix + "name";
        private static readonly string _nodeTypeId = _metadataPrefix + "condition";
        private static readonly string _nodeArgumentId = _metadataPrefix + "arguments";
        private static readonly string _nodeDescriptionId = _metadataPrefix + "description";
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
                    int cPoint = line.IndexOf(_luaComment);

                    if (cPoint >= 0)
                    {
                        int start = cPoint + _luaComment.Length;
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
                        else if (comment.StartsWith(_nodeDescriptionId, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            nodeFunction.Description = ExtractValues(comment.Substring(_nodeDescriptionId.Length, comment.Length - _nodeDescriptionId.Length)).LastOrDefault();
                            continue;
                        }
                        else if (comment.StartsWith(_nodeArgumentId, System.StringComparison.InvariantCultureIgnoreCase))
                        {
                            var settings = ExtractValues(comment.Substring(_nodeArgumentId.Length, comment.Length - _nodeArgumentId.Length));

                            foreach (var s in settings)
                            {
                                var argLayout = new ArgumentLayout()
                                {
                                    Type = ArgumentType.Numerical,
                                    CustomEnumeration = new List<string>(),
                                    Description = string.Empty,
                                    NewLine = false,
                                    Width = 100.0f
                                };

                                var parameters = s.Split(',').Select(st => st.Trim());
                                foreach (var p in parameters)
                                {
                                    float width = 100.0f;

                                    if (p.Equals(_nodeLayoutNewLine, StringComparison.InvariantCultureIgnoreCase))
                                        argLayout.NewLine = true;
                                    else if (float.TryParse(p, out width))
                                        argLayout.Width = width;
                                    else if (p.StartsWith(_enumSplitterStart) && p.EndsWith(_enumSplitterEnd))
                                        argLayout.CustomEnumeration.AddRange(p.Substring(1, p.Length - 2).Split('|').Select(st => st.Trim()));
                                    else
                                        try { argLayout.Type = (ArgumentType)Enum.Parse(typeof(ArgumentType), p); }
                                        catch { argLayout.Description = p; }
                                }

                                 nodeFunction.Arguments.Add(argLayout);
                            }
                            continue;
                        }
                    }

                    if (cPoint > 0)
                        line = line.Substring(0, cPoint - 1);
                    else if (cPoint == 0)
                        line = string.Empty;

                    if (line.StartsWith(_luaFunc + _luaSpace + _levelFuncPrefix))
                    {
                        int indexStart = line.IndexOf(_luaSplitter) + 1;
                        int indexEnd = line.IndexOf(_luaBrkOpen) - indexStart;
                        nodeFunction.Signature = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.StartsWith(_levelFuncPrefix))
                    {
                        int indexStart = line.IndexOf(_luaSplitter) + 1;
                        int indexEnd = line.IndexOf(_luaIs) - indexStart;
                        nodeFunction.Signature = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.Contains(_luaInclude))
                    {
                        int pos1 = line.IndexOf(_luaInclude) + _luaInclude.Length;
                        int pos2 = line.Length;
                        string subfile = line.Substring(pos1, pos2 - pos1);
                        pos1 = subfile.IndexOf(_luaBrkOpen) + 1;
                        pos2 = subfile.IndexOf(_luaBrkClose);
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

                    if (string.IsNullOrEmpty(nodeFunction.Name) ||
                        string.IsNullOrEmpty(nodeFunction.Signature))
                        continue;

                    if (!result.Contains(nodeFunction))
                        result.Add(nodeFunction);

                    nodeFunction = new NodeFunction();
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

                    int cPoint = line.IndexOf(_luaComment);
                    if (cPoint > 0)
                        line = line.Substring(0, cPoint - 1);
                    else if (cPoint == 0)
                        continue;

                    if (line.StartsWith(_luaFunc + _luaSpace + _levelFuncPrefix))
                    {
                        int indexStart = line.IndexOf(_luaSplitter) + 1;
                        int indexEnd = line.IndexOf(_luaBrkOpen) - indexStart;
                        functionName = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.StartsWith(_levelFuncPrefix))
                    {
                        int indexStart = line.IndexOf(_luaSplitter) + 1;
                        int indexEnd = line.IndexOf(_luaIs) - indexStart;
                        functionName = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.Contains(_luaInclude))
                    {
                        int pos1 = line.IndexOf(_luaInclude) + _luaInclude.Length;
                        int pos2 = line.Length;
                        string subfile = line.Substring(pos1, pos2 - pos1);
                        pos1 = subfile.IndexOf(_luaBrkOpen) + 1;
                        pos2 = subfile.IndexOf(_luaBrkClose);
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

        private static string ParseFunctionString(string function, List<string> arguments)
        {
            string joined = _levelFuncPrefix + function + _luaBrkOpen + _luaActivator;
            arguments.ForEach(arg => joined += _luaSeparator + _luaSpace + (string.IsNullOrEmpty(arg) ? _luaNull : arg));
            joined += _luaBrkClose;
            return joined;
        }

        private static string ParseNode(TriggerNode node, int indent, ref string source)
        {
            source += Environment.NewLine;

            if (node is TriggerNodeAction)
            {
                source += string.Empty.PadLeft(indent, _tabChar) + 
                          ParseFunctionString(node.Function, node.Arguments);

                if (node.Next != null)
                    ParseNode(node.Next, indent, ref source);
            }

            if (node is TriggerNodeCondition)
            {
                var condNode = node as TriggerNodeCondition;

                source += string.Empty.PadLeft(indent, _tabChar) + 
                          _luaIf + _luaSpace + _luaBrkOpen + 
                          ParseFunctionString(condNode.Function, condNode.Arguments) + 
                          _luaBrkClose + _luaSpace + _luaThen;
                
                if (condNode.Next != null)
                    ParseNode(condNode.Next, indent + 1, ref source);

                if (condNode.Else != null)
                {
                    source += Environment.NewLine + string.Empty.PadLeft(indent, _tabChar) + _luaElse;
                    ParseNode(condNode.Else, indent + 1, ref source);
                }

                source += Environment.NewLine + string.Empty.PadLeft(indent, _tabChar) + _luaEnd;
            }

            return source;
        }

        public static string ParseNodes(List<TriggerNode> nodes, string functionName)
        {
            var result = _levelFuncPrefix + functionName + 
                         _luaSpace + _luaIs + _luaSpace + _luaFunc + _luaBrkOpen + _luaActivator + _luaBrkClose;

            nodes.OrderByDescending(node => node.ScreenPosition.Y).ToList().ForEach(node => ParseNode(node, 1, ref result));
            result += Environment.NewLine + _luaEnd;
            return result;
        }
    }
}