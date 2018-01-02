using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using TombLib.NG;
using System.Net;
using TombLib.Utils;
using TombLib.LevelData;
using System.Xml.Linq;

namespace NgXmlBuilder
{
    enum NgParameterType
    {
        Object,
        Timer,
        Extra
    }

    enum NgBlockType
    {
        Trigger,
        FlipEffect,
        Condition,
        Action
    }

    class NgBlock
    {
        public NgBlockType Type;
        public NgParameterType ParameterType;
        public ushort Id;
        public List<string> Items;

        public NgBlock()
        {
            Items = new List<string>();
        }
    }

    class Program
    {
        private const string outPath = "..\\TombEditor\\Editor\\Misc\\NgCatalog.xml";

        static void Main(string[] args)
        {
            Console.WriteLine("Parsing TXT files...");
            LoadTriggersFromTxt();

            Console.WriteLine("Writing XML...");
            SaveToXml(outPath);

            Console.WriteLine("Done");

            // Test
            NgCatalog.LoadCatalog(outPath);
        }

        private static SortedList<ushort, TriggerParameterUshort> GetListFromTxt(string name)
        {
            var result = new SortedList<ushort, TriggerParameterUshort>();
            using (var reader = new StreamReader(File.OpenRead("NG\\" + name + ".txt")))
            {
                while (!reader.EndOfStream)
                {
                    var tokens = reader.ReadLine().Trim().Split(':');
                    result.Add(ToU16(int.Parse(tokens[0])), new NgTriggerSubtype(ToU16(int.Parse(tokens[0])), XmlEncodeString(tokens[1].Trim())));
                }
            }
            return result;
        }

        private static NgBlock ReadNgBlock(StreamReader reader)
        {
            string line = "";

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().Trim();
                if (line.StartsWith("<START")) break;
            }

            if (reader.EndOfStream) return null;

            var tokens = line.Split('_');
            var block = new NgBlock();

            // Block type
            if (tokens[1] == "TRIGGERWHAT" || tokens[1] == "TRIGGERTYPE" || tokens[1] == "TEXTS") block.Type = NgBlockType.Trigger;
            else if (tokens[1] == "EFFECT") block.Type = NgBlockType.FlipEffect;
            else if (tokens[1] == "ACTION") block.Type = NgBlockType.Action;
            else if (tokens[1] == "CONDITION") block.Type = NgBlockType.Condition;
            else throw new Exception("Unknown token[1]");

            // Parameter type
            if (tokens[3].StartsWith("O")) block.ParameterType = NgParameterType.Object;
            else if (tokens[3].StartsWith("E")) block.ParameterType = NgParameterType.Extra;
            else if (tokens[3].StartsWith("T")) block.ParameterType = NgParameterType.Timer;
            else if (tokens[3].StartsWith("B")) block.ParameterType = NgParameterType.Extra;
            else throw new Exception("Unknown token[3]");

            block.Id = ToU16(int.Parse(tokens[2]));

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().Trim();
                if (line.StartsWith(";") || line == "") continue;
                if (line.StartsWith("<END")) break;
                block.Items.Add(line);
            }

            return block;
        }

        private static ushort ToU16(long data)
        {
            return unchecked((ushort)checked((short)data));
        }

        private static ushort GetItemId(string line)
        {
            return ToU16(int.Parse(line.Trim().Split(':')[0]));
        }

        private static string GetItemValue(string line)
        {
            return line.Trim().Split(':')[1];
        }

        private static string XmlEncodeString(string s)
        {
            s = s.Replace("&", "&amp;");
            s = s.Replace("<", "&lt;");
            s = s.Replace(">", "&gt;");
            s = s.Replace("\"", "");

            return s;
        }

        static readonly char[] numberChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '+', '-' };

        private struct Parsed
        {
            public string String;
            public decimal? Value;
        }

        private static bool IsSeperator(char @char, bool start)
        {
            if (@char == (start ? ']' : '[') ||
                @char == (start ? '}' : '{') ||
                @char == (start ? ')' : '('))
                return false;
            if (char.IsSymbol(@char))
                return true;
            if (char.IsPunctuation(@char))
                return true;
            return false;
        }

        private static List<Parsed> SplitIntoNumbers(string str, int startPos = 0)
        {
            str = str.Trim();
            List<Parsed> list = new List<Parsed>();
            while (startPos < str.Length)
            {
                int nextStart = str.IndexOfAny(numberChars, startPos);
                if (nextStart == -1)
                    break;

                int nextEnd = str.IndexOf(c => !numberChars.Contains(c), nextStart + 1, str.Length);
                string numberSubstring = str.Substring(nextStart, nextEnd - nextStart);
                decimal number;
                if (!decimal.TryParse(numberSubstring, out number))
                {
                    startPos = nextEnd + 1;
                    continue;
                }

                // Preserve exactly one whitespace before the number
                string preStr = str.Substring(0, nextStart);
                int preStrWhiteSpaceCount = preStr.Reverse().TakeWhile(@char => char.IsWhiteSpace(@char)).Count();
                if (preStrWhiteSpaceCount > 0)
                {
                    if (preStr.Length != preStrWhiteSpaceCount &&
                        !IsSeperator(preStr[preStr.Length - preStrWhiteSpaceCount - 1], true))
                        preStrWhiteSpaceCount -= 1;
                    numberSubstring = preStr.Substring(preStr.Length - preStrWhiteSpaceCount) + numberSubstring;
                    preStr = preStr.Substring(0, preStr.Length - preStrWhiteSpaceCount);
                }

                // Preserve exactly one whitespace after the number
                string postStr = str.Substring(nextEnd);
                int postStrWhiteSpaceCount = postStr.TakeWhile(@char => char.IsWhiteSpace(@char)).Count();
                if (postStrWhiteSpaceCount > 0)
                {
                    if (postStr.Length != postStrWhiteSpaceCount &&
                        !IsSeperator(postStr[postStrWhiteSpaceCount], false))
                        postStrWhiteSpaceCount -= 1;
                    numberSubstring += postStr.Substring(0, postStrWhiteSpaceCount);
                    postStr = postStr.Substring(postStrWhiteSpaceCount);
                }

                // Add parsed values
                if (!string.IsNullOrEmpty(preStr))
                    list.Add(new Parsed { String = preStr });
                list.Add(new Parsed { String = numberSubstring, Value = number });
                str = postStr;
                startPos = 0;
            }

            if (!string.IsNullOrEmpty(str))
                list.Add(new Parsed { String = str });
            return list;
        }

        private static NgParameterRange DetectLinearity(NgParameterRange parameter, NgBlock dbgBlock)
        {
            if (parameter.Kind != NgParameterKind.FixedEnumeration)
                return parameter;
            if (parameter.FixedEnumeration.Count <= 5) // It's not worth it if there are at most 5 elements
                return parameter;

            // Parse the numbers inside the strings.
            // Stop if there is a varying amount of numbers in the strings or if different values are fixed.
            var firstParsedStr = SplitIntoNumbers(parameter.FixedEnumeration.First().Value.Name);
            var parsedStrs = new List<KeyValuePair<ushort, List<Parsed>>>(parameter.FixedEnumeration.Count);
            foreach (var entry in parameter.FixedEnumeration)
            {
                var currentParsedStr = SplitIntoNumbers(entry.Value.Name);
                if (currentParsedStr.Count != firstParsedStr.Count)
                    return parameter;
                for (int i = 0; i < currentParsedStr.Count; ++i)
                    if ((currentParsedStr[i].Value != null) != (firstParsedStr[i].Value != null))
                        return parameter;
                for (int i = 0; i < currentParsedStr.Count; ++i)
                    if (currentParsedStr[i].Value == null)
                        if (currentParsedStr[i].String != firstParsedStr[i].String)
                            return parameter;
                parsedStrs.Add(new KeyValuePair<ushort, List<Parsed>>(entry.Key, currentParsedStr));
            }

            // Check if there exists a linear run
            // There can't be missing keys.
            ushort idStart = parsedStrs.OrderBy(e => e.Key).First().Key;
            ushort idEnd = parsedStrs.OrderByDescending(e => e.Key).First().Key;
            var idLookup = new HashSet<ushort>(parsedStrs.Select(e => e.Key));
            for (ushort i = idStart; i < idEnd; ++i)
                if (!idLookup.Contains(i))
                {
                    Console.WriteLine("Linear holes found. Should be rare with NG trigger definitions.");
                    return parameter;
                }

            // String format now is perfectly fine.
            // We just have to propose some kind of linear pattern
            ushort firstId = parameter.FixedEnumeration.First().Key;
            ushort secondId = parameter.FixedEnumeration.ElementAt(1).Key;
            var secondParsedStr = parsedStrs[1].Value;
            List<NgLinearParameter> linearParameters = new List<NgLinearParameter>(firstParsedStr.Count);
            for (int i = 0; i < firstParsedStr.Count; ++i)
                if (firstParsedStr[i].Value == null)
                    linearParameters.Add(new NgLinearParameter { FixedStr = firstParsedStr[i].String });
                else
                {

                    decimal xDistance = secondId - firstId;
                    decimal yDistance = secondParsedStr[i].Value.Value - firstParsedStr[i].Value.Value;
                    decimal factor = yDistance / xDistance;
                    decimal add = firstParsedStr[i].Value.Value - firstId * factor;
                    linearParameters.Add(new NgLinearParameter { Factor = factor, Add = add });
                }

            // Eliminate unnecessary linear parameters
            // (i.e. linear with 0 slope)
            for (int i = 0; i < linearParameters.Count; ++i)
                if (linearParameters[i].FixedStr != null)
                    if (linearParameters[i].Factor == 0)
                        linearParameters[i] = new NgLinearParameter { FixedStr = firstParsedStr[i].String };
            for (int i = linearParameters.Count - 1; i >= 1; --i)
                if ((linearParameters[i].FixedStr != null) && (linearParameters[i - 1].FixedStr != null))
                {
                    linearParameters[i - 1] = new NgLinearParameter { FixedStr = linearParameters[i - 1].FixedStr + linearParameters[i].FixedStr };
                    linearParameters.RemoveAt(i);
                    for (int j = 0; j < parsedStrs.Count; ++j)
                        parsedStrs[j].Value.RemoveAt(i);
                }

            // Check that the linear pattern is indeed a good fit
            foreach (var parsedStr in parsedStrs)
                for (int i = 0; i < linearParameters.Count; ++i)
                    if (linearParameters[i].FixedStr == null)
                    {
                        // Calculate prediction with the linear model
                        decimal value = linearParameters[i].Factor * parsedStr.Key + linearParameters[i].Add;

                        // Check prediction
                        if (parsedStr.Value[i].Value.Value != value)
                        {
                            Console.WriteLine("Linear modelling failed. Should be rare with NG trigger definitions.");
                            return parameter;
                        }
                    }

            return new NgParameterRange(new NgLinearModel
            {
                Start = unchecked((ushort)idStart),
                End = unchecked((ushort)idEnd),
                Parameters = linearParameters
            });
        }

        private static NgParameterRange GetList(NgBlock block)
        {
            if (block.Items.Count == 1 && block.Items[0].StartsWith("#"))
            {
                // Special list
                var list = block.Items[0];
                // Dynamic list?
                if (list == "#ROOMS_255#") return new NgParameterRange(NgParameterKind.Rooms255);
                else if (list == "#SOUND_EFFECT_A#") return new NgParameterRange(NgParameterKind.SoundEffectsA);
                else if (list == "#SOUND_EFFECT_B#") return new NgParameterRange(NgParameterKind.SoundEffectsB);
                else if (list == "#SFX_1024#") return new NgParameterRange(NgParameterKind.Sfx1024);
                else if (list == "#NG_STRING_LIST_255#") return new NgParameterRange(NgParameterKind.NgStringsList255);
                else if (list == "#NG_STRING_LIST_ALL#") return new NgParameterRange(NgParameterKind.NgStringsAll);
                else if (list == "#PSX_STRING_LIST#") return new NgParameterRange(NgParameterKind.PsxStringsList);
                else if (list == "#PC_STRING_LIST#") return new NgParameterRange(NgParameterKind.PcStringsList);
                else if (list == "#STRING_LIST_255#") return new NgParameterRange(NgParameterKind.StringsList255);
                else if (list == "#MOVEABLES#") return new NgParameterRange(NgParameterKind.MoveablesInLevel);
                else if (list == "#SINK_LIST#") return new NgParameterRange(NgParameterKind.SinksInLevel);
                else if (list == "#STATIC_LIST#") return new NgParameterRange(NgParameterKind.StaticsInLevel);
                else if (list == "#FLYBY_LIST#") return new NgParameterRange(NgParameterKind.FlybyCamerasInLevel);
                else if (list == "#CAMERA_EFFECTS#") return new NgParameterRange(NgParameterKind.CamerasInLevel);
                else if (list == "#WAD-SLOTS#") return new NgParameterRange(NgParameterKind.WadSlots);
                else if (list == "#STATIC_SLOTS#") return new NgParameterRange(NgParameterKind.StaticsSlots);
                else if (list == "#LARA_POS_OCB#") return new NgParameterRange(NgParameterKind.LaraStartPosOcb);

                // Repeated strings
                if (list.StartsWith("#REPEAT#"))
                {
                    var tokens = list.Replace("#REPEAT#", "").Split('#');
                    var radix = tokens[0].Replace("\"", "");
                    var start = int.Parse(tokens[1].Replace(",", ""));
                    var end = int.Parse(tokens[2].Replace(",", ""));
                    var enumeration = new SortedList<ushort, TriggerParameterUshort>();
                    for (var i = start; i < end; i++)
                        enumeration.Add(ToU16(i), new TriggerParameterUshort((ushort)i, radix + i));
                    return DetectLinearity(new NgParameterRange(enumeration), block);
                }

                // TXT lists
                var listName = list.Replace("#", "");
                if (File.Exists("NG\\" + listName + ".txt"))
                    return DetectLinearity(new NgParameterRange(GetListFromTxt(listName)), block);
                else
                    throw new Exception("Missing NG file");
            }
            else
            {
                // Free list
                var enumeration = new SortedList<ushort, TriggerParameterUshort>();
                foreach (var item in block.Items)
                    enumeration.Add(GetItemId(item), new TriggerParameterUshort(GetItemId(item), GetItemValue(item)));
                return DetectLinearity(new NgParameterRange(enumeration), block);
            }
        }

        private static void LoadTriggersFromTxt()
        {
            using (var reader = new StreamReader(File.OpenRead("NG\\NG_Constants.txt")))
            {
                while (!reader.EndOfStream)
                {
                    var block = ReadNgBlock(reader);
                    if (block == null)
                        break;

                    if (block.Type == NgBlockType.Trigger)
                    {
                        if (block.Id == 15)
                        {
                            // Trigger for timer field
                            NgCatalog.TimerFieldTrigger = new NgParameterRange(GetListFromTxt("TIMER_SIGNED_LONG"));
                        }
                        else if (block.Id == 9)
                        {
                            // Trigger for flip effect
                            foreach (var item in block.Items)
                                NgCatalog.FlipEffectTrigger.MainList.Add(ToU16(GetItemId(item)), new NgTriggerSubtype(ToU16(GetItemId(item)), GetItemValue(item)));
                        }
                        else if (block.Id == 11)
                        {
                            // Trigger for action
                            foreach (var item in block.Items)
                                NgCatalog.ActionTrigger.MainList.Add(ToU16(GetItemId(item)), new NgTriggerSubtype(ToU16(GetItemId(item)), GetItemValue(item))
                                    { Target = new NgParameterRange(NgParameterKind.MoveablesInLevel) });
                        }
                        else if (block.Id == 12)
                        {
                            // Condition trigger
                            foreach (var item in block.Items)
                                NgCatalog.ConditionTrigger.MainList.Add(ToU16(GetItemId(item)), new NgTriggerSubtype(ToU16(GetItemId(item)), GetItemValue(item)));
                        }
                    }
                    else if (block.Type == NgBlockType.FlipEffect)
                    {
                        if (block.ParameterType == NgParameterType.Timer)
                            NgCatalog.FlipEffectTrigger.MainList[block.Id].Timer = GetList(block);
                        else if (block.ParameterType == NgParameterType.Extra)
                            NgCatalog.FlipEffectTrigger.MainList[block.Id].Extra = GetList(block);
                    }
                    else if (block.Type == NgBlockType.Action)
                    {
                        if (block.ParameterType == NgParameterType.Object)
                            NgCatalog.ActionTrigger.MainList[block.Id].Timer = GetList(block);
                        else if (block.ParameterType == NgParameterType.Extra)
                            NgCatalog.ActionTrigger.MainList[block.Id].Extra = GetList(block);
                    }
                    else if (block.Type == NgBlockType.Condition)
                    {
                        if (block.ParameterType == NgParameterType.Object)
                            NgCatalog.ConditionTrigger.MainList[block.Id].Timer = GetList(block);
                        else if (block.ParameterType == NgParameterType.Extra)
                            NgCatalog.ConditionTrigger.MainList[block.Id].Extra = GetList(block);
                        /*else if (block.ParameterType == NgParameterType.Button)
                            NgCatalog.ConditionTrigger.MainList[block.Id].ButtonList = GetList(block);
                        */
                    }
                }
            }

            NgCatalog.FlipEffectTrigger.MainList[28].Timer =
                GetList(new NgBlock { Items = new List<string> { "#COLORS" } });
        }

        private static void SaveToXml(string path)
        {
            XDocument document = new XDocument();
            document.Add(new XElement("TriggerDescription",
                new XElement("TimerFieldTrigger", WriteNgParameterRange(NgCatalog.TimerFieldTrigger)),
                new XElement("FlipEffectTrigger", WriteNgTriggerSubtype(NgCatalog.FlipEffectTrigger)),
                new XElement("ActionTrigger", WriteNgTriggerSubtype(NgCatalog.ActionTrigger)),
                new XElement("ConditionTrigger", WriteNgTriggerSubtype(NgCatalog.ConditionTrigger))));
            document.Save(path);
        }

        private static XObject[] WriteNgTriggerSubtype(NgTriggerSubtypes triggerSubtypes)
        {
            List<XObject> elements = new List<XObject>();
            foreach (NgTriggerSubtype triggerSubtype in triggerSubtypes.MainList.Values)
            {
                XElement triggerSubtypeElement = new XElement("Subtrigger");
                triggerSubtypeElement.Add(new XAttribute("K", triggerSubtype.Key));
                triggerSubtypeElement.Add(new XAttribute("V", triggerSubtype.Name));

                if (!triggerSubtype.Timer.IsEmpty)
                    triggerSubtypeElement.Add(new XElement("Timer", WriteNgParameterRange(triggerSubtype.Timer)));
                if (!triggerSubtype.Target.IsEmpty)
                    triggerSubtypeElement.Add(new XElement("Target", WriteNgParameterRange(triggerSubtype.Target)));
                if (!triggerSubtype.Extra.IsEmpty)
                    triggerSubtypeElement.Add(new XElement("Extra", WriteNgParameterRange(triggerSubtype.Extra)));
                elements.Add(triggerSubtypeElement);
            }

            return elements.ToArray();
        }

        private static XObject[] WriteNgParameterRange(NgParameterRange parameter)
        {
            XElement parameterElement = new XElement(parameter.Kind.ToString());
            switch (parameter.Kind)
            {
                case NgParameterKind.FixedEnumeration:
                    foreach (TriggerParameterUshort value in parameter.FixedEnumeration.Values)
                        parameterElement.Add(new XElement("Enum",
                            new XAttribute("K", value.Key),
                            new XAttribute("V", value.Name)));
                    break;
                case NgParameterKind.LinearModel:
                    foreach (NgLinearParameter linearParameter in parameter.LinearModel.Value.Parameters)
                        if (linearParameter.FixedStr != null)
                            parameterElement.Add(new XElement("Fixed", linearParameter.FixedStr));
                        else
                            parameterElement.Add(new XElement("Linear",
                                new XAttribute("Add", linearParameter.Add),
                                new XAttribute("Factor", linearParameter.Factor)));
                    parameterElement.Add(new XAttribute("Start", parameter.LinearModel.Value.Start));
                    parameterElement.Add(new XAttribute("End", parameter.LinearModel.Value.End));
                    break;
            }
            return new XObject[] { parameterElement };
        }
    }
}