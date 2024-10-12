using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.LevelData.VisualScripting;

namespace TombLib.Utils
{
    public static class LuaSyntax
    {
        public const string Include = "require";
        public const string Func = "function";
        public const string If = "if";
        public const string Then = "then";
        public const string Else = "else";
        public const string End = "end";
        public const string Null = "nil";
        public const string Is = "=";
        public const string BracketOpen = "(";
        public const string BracketClose = ")";
        public const string Splitter = ".";
        public const string Separator = ",";
        public const string Space = " ";
        public const string Comment = "--";

        public const string Activator = "activator";
        public const string ActivatorNamePrefix = Activator + ":GetName()";
        public const string ColorTypePrefix = "TEN.Color";
        public const string Vec3TypePrefix = "TEN.Vec3";
        public const string ObjectIDPrefix = "TEN.Objects.ObjID";
        public const string ReservedFunctionPrefix = "__";

        public const string LevelFuncPrefix = "LevelFuncs" + Splitter;
        public const string NodeCatalogPrefix = LevelFuncPrefix + "Engine" + Splitter + "Node" + Splitter;
    }

    public static class ScriptingUtils
    {
        private const int _maxRecursionDepth = 32;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] _reservedNames = { "OnStart", "OnEnd", "OnLoad", "OnSave", "OnControlPhase", "OnLoop", "OnUseItem" };

        private const string _metadataPrefix = "!";
        private const string _enumSplitterStart = "[";
        private const string _enumSplitterEnd = "]";
        private const string _defaultValueStart = "{";
        private const string _defaultValueEnd = "}";
        private const char _tabChar = '\t';

        private const string _nodeIgnoreId = _metadataPrefix + "ignore";
        private const string _nodeNameId = _metadataPrefix + "name";
        private const string _nodeSectionId = _metadataPrefix + "section";
        private const string _nodeTypeId = _metadataPrefix + "condition";
        private const string _nodeArgumentId = _metadataPrefix + "arguments";
        private const string _nodeDescriptionId = _metadataPrefix + "description";
        private const string _nodeLayoutNewLine = "newline";

        public static string GameNodeScriptPath = Path.Combine("Scripts", "Engine", "NodeCatalogs");
        public static string NodeScriptPath => Path.Combine(DefaultPaths.CatalogsDirectory, "TEN Node Catalogs");

        public static readonly List<NodeFunction> NodeFunctions = GetAllNodeFunctions(NodeScriptPath);

        private static List<NodeFunction> GetAllNodeFunctions(string path, List<NodeFunction> list = null, int depth = 0)
        {
            var result = list == null ? new List<NodeFunction>() : list;

            if (!Directory.Exists(path))
                return result;

            foreach (var file in Directory.GetFiles(path).Where(p => !p.StartsWith("_") && p.EndsWith(".lua"))) try
            {
                var lines = File.ReadAllLines(file);
                var nodeFunction = new NodeFunction();
                bool ignore = false;

                foreach (string l in lines)
                {
                    string line = l.Trim();
                    int cPoint = line.IndexOf(LuaSyntax.Comment);

                    if (cPoint >= 0)
                    {
                        int start = cPoint + LuaSyntax.Comment.Length;
                        int end = line.Length;

                        var comment = line.Substring(start, end - start).Trim();
                        if (comment.StartsWith(_nodeIgnoreId, StringComparison.InvariantCultureIgnoreCase))
                        { 
                            ignore = true;
                        }
                        else if (comment.StartsWith(_nodeTypeId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            bool cond = false;
                            bool.TryParse(TextExtensions.ExtractValues(comment.Substring(_nodeTypeId.Length, comment.Length - _nodeTypeId.Length)).LastOrDefault(), out cond);
                            nodeFunction.Conditional = cond;
                            continue;
                        }
                        else if (comment.StartsWith(_nodeNameId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            nodeFunction.Name = TextExtensions.ExtractValues(comment.Substring(_nodeNameId.Length, comment.Length - _nodeNameId.Length)).LastOrDefault();
                            continue;
                        }
                        else if (comment.StartsWith(_nodeDescriptionId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var newLine = TextExtensions.ExtractValues(comment.Substring(_nodeDescriptionId.Length, comment.Length - _nodeDescriptionId.Length)).LastOrDefault();
                            if (string.IsNullOrEmpty(nodeFunction.Description))
                                nodeFunction.Description = newLine;
                            else
                                nodeFunction.Description += Environment.NewLine + newLine;
                            continue;
                        }
                        else if (comment.StartsWith(_nodeSectionId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            nodeFunction.Section = TextExtensions.ExtractValues(comment.Substring(_nodeSectionId.Length, comment.Length - _nodeSectionId.Length)).LastOrDefault();
                            continue;
                        }
                        else if (comment.StartsWith(_nodeArgumentId, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var settings = TextExtensions.ExtractValues(comment.Substring(_nodeArgumentId.Length, comment.Length - _nodeArgumentId.Length));

                            foreach (var s in settings)
                            {
                                var argLayout = new ArgumentLayout()
                                {
                                    Type = ArgumentType.Numerical,
                                    CustomEnumeration = new List<string>(),
                                    DefaultValue = string.Empty,
                                    Description = string.Empty,
                                    NewLine = false,
                                    Width = 100.0f
                                };

                                var parameters = s.SplitParenthesis().Select(st => st.Trim());
                                foreach (var p in parameters)
                                {
                                    float width = 100.0f;

                                    if (p.Equals(_nodeLayoutNewLine, StringComparison.InvariantCultureIgnoreCase))
                                        argLayout.NewLine = true;
                                    else if (float.TryParse(p, out width))
                                        argLayout.Width = width;
                                    else if (p.StartsWith(_enumSplitterStart) && p.EndsWith(_enumSplitterEnd))
                                        argLayout.CustomEnumeration.AddRange(p.Substring(1, p.Length - 2).Split('|').Select(st => st.Trim()));
                                    else if (p.StartsWith(_defaultValueStart) && p.EndsWith(_defaultValueEnd))
                                        argLayout.DefaultValue = p.Substring(1, p.Length - 2).Trim();
                                    else if (Enum.TryParse(p, out ArgumentType argType))
                                        argLayout.Type = argType;
                                    else
                                        argLayout.Description = p;
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

                    if (line.StartsWith(LuaSyntax.NodeCatalogPrefix))
                    {
                        if (ignore)
                        {
                            ignore = false;
                            continue;
                        }

                        int indexStart = line.LastIndexOf(LuaSyntax.Splitter) + 1;
                        int indexEnd = line.IndexOf(LuaSyntax.Is) - indexStart;
                        nodeFunction.Signature = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else
                        continue;

                    if (string.IsNullOrEmpty(nodeFunction.Name) ||
                        string.IsNullOrEmpty(nodeFunction.Signature))
                        continue;

                    if (nodeFunction.Signature.StartsWith(LuaSyntax.ReservedFunctionPrefix))
                        continue;

                    if (!result.Contains(nodeFunction))
                        result.Add(nodeFunction);

                    nodeFunction = new NodeFunction();
                }
            }
            catch
            {
                continue;
            }

            // Check for duplicate functions

            var duplicates = result.GroupBy(f => f.Signature).Where(g => g.Count() > 1);
            if (duplicates.Count() > 0)
            {
                var message = "Node catalogs contain a duplicate of function " + duplicates.First().First().Signature + ". " +
                              "Review node catalogs and rename or remove duplicate.";
#if DEBUG
                throw new Exception(message);
#else
                logger.Warn(message);
#endif
            }

            return result;
        }

        public static List<string> GetAllFunctionNames(string path, List<string> list = null, int depth = 0)
        {
            var result = list == null ? new List<string>() : list;

            try
            {
                if (!File.Exists(path))
                    return result;

                var lines = File.ReadAllLines(path);

                foreach (string l in lines)
                {
                    string functionName = string.Empty;

                    string line = l.Trim();
                    bool skip = false;

                    if (string.IsNullOrEmpty(line))
                        continue;

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

                    int cPoint = line.IndexOf(LuaSyntax.Comment);
                    if (cPoint > 0)
                        line = line.Substring(0, cPoint - 1);
                    else if (cPoint == 0)
                        continue;

                    if (line.StartsWith(LuaSyntax.LevelFuncPrefix))
                    {
                        int indexStart = line.IndexOf(LuaSyntax.Splitter) + 1;
                        int indexEnd = line.IndexOf(LuaSyntax.Is) - indexStart;

                        if (indexEnd <= 0)
                            continue;

                        functionName = line.Substring(indexStart, indexEnd).Trim();
                    }
                    else if (line.Contains(LuaSyntax.Include))
                    {
                        int pos1 = line.IndexOf(LuaSyntax.Include) + LuaSyntax.Include.Length;
                        int pos2 = line.Length;
                        string subfile = line.Substring(pos1, pos2 - pos1);
                        pos1 = subfile.IndexOf(LuaSyntax.BracketOpen) + 1;
                        pos2 = subfile.IndexOf(LuaSyntax.BracketClose);
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

                    if (functionName.StartsWith(LuaSyntax.ReservedFunctionPrefix))
                        continue;

                    if (!result.Contains(functionName))
                        result.Add(functionName);
                }
            }
            catch
            {
                return result;
            }

            return result;
        }

        private static string ParseFunctionString(string function, List<string> arguments)
        {
            string joined = LuaSyntax.NodeCatalogPrefix + function + LuaSyntax.BracketOpen;

            for (int i = 0; i < arguments.Count; i++)
            {
                joined += (string.IsNullOrEmpty(arguments[i]) ? LuaSyntax.Null : arguments[i]);
                if (arguments.Count > 1 && i < arguments.Count - 1)
                    joined += (LuaSyntax.Separator + LuaSyntax.Space);
            }

            joined += LuaSyntax.BracketClose;
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
                          LuaSyntax.If + LuaSyntax.Space + LuaSyntax.BracketOpen + 
                          ParseFunctionString(condNode.Function, condNode.Arguments) + 
                          LuaSyntax.BracketClose + LuaSyntax.Space + LuaSyntax.Then;
                
                if (condNode.Next != null)
                    ParseNode(condNode.Next, indent + 1, ref source);

                if (condNode.Else != null)
                {
                    source += Environment.NewLine + string.Empty.PadLeft(indent, _tabChar) + LuaSyntax.Else;
                    ParseNode(condNode.Else, indent + 1, ref source);
                }

                source += Environment.NewLine + string.Empty.PadLeft(indent, _tabChar) + LuaSyntax.End;
            }

            return source;
        }

        public static string ParseNodes(List<TriggerNode> nodes, string functionName)
        {
            if (nodes == null || nodes.Count == 0)
                return string.Empty;

            var result = LuaSyntax.LevelFuncPrefix + functionName + LuaSyntax.Space + LuaSyntax.Is + LuaSyntax.Space + 
                         LuaSyntax.Func + LuaSyntax.BracketOpen + LuaSyntax.Activator + LuaSyntax.BracketClose;

            nodes.OrderByDescending(node => node.ScreenPosition.Y).ToList().ForEach(node => ParseNode(node, 1, ref result));
            result += Environment.NewLine + LuaSyntax.End;
            return result;
        }
    }
}