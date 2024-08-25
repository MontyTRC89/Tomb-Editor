using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.Utils;

namespace NgXmlBuilder
{
    internal enum NgParameterType
    {
        Object,
        Timer,
        Extra
    }

    internal enum NgBlockType
    {
        Trigger,
        FlipEffect,
        Condition,
        Action
    }

    internal class NgBlock
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

    internal static class Program
    {
        private const string outPath = "..\\TombLib\\Catalogs\\NgCatalog.xml";

        private static void Main()
        {
            Console.WriteLine("Parsing TXT files...");
            NgCatalog catalog = LoadTriggersFromFile();

            Console.WriteLine("Writing XML...");
            SaveToXml(catalog, outPath);

            Console.WriteLine("Done");

            // Test
            NgCatalog.LoadCatalog(outPath);

            // Adjust other files...
            /*foreach (string fileName in Directory.EnumerateFiles("..\\ScriptEditor\\References", "*.txt"))
            {
                string content = File.ReadAllText(fileName, Encoding.UTF8);
                string newContent = AdjustCardinalDirections(content);
                File.WriteAllText(fileName, newContent, Encoding.UTF8);
            }*/
        }

        // Earlier entries take priority
        private static readonly KeyValuePair<string, string>[] cardinalReplacementTable = new[]
        {
            new KeyValuePair<string, string>("North-South (in ngle)", "X-axis (east-west)"),
            new KeyValuePair<string, string>("East-West (in ngle)", "Z-axis (north-south)"),
            new KeyValuePair<string, string>("North-South", "X-axis (east-west)"),
            new KeyValuePair<string, string>("East-West", "Z-axis (north-south)"),
            new KeyValuePair<string, string>("East-Ovest", "Z-axis (north-south)"),

            new KeyValuePair<string, string>("North East", "-X,+Z direction (north-west)"),
            new KeyValuePair<string, string>("North West", "-X,-Z direction (south-west)"),
            new KeyValuePair<string, string>("South West", "+X,-Z direction (south-east)"),
            new KeyValuePair<string, string>("South East", "+X,+Z direction (north-east)"),

            new KeyValuePair<string, string>("North-East", "-X,+Z direction (north-west)"),
            new KeyValuePair<string, string>("North-West", "-X,-Z direction (south-west)"),
            new KeyValuePair<string, string>("South-West", "+X,-Z direction (south-east)"),
            new KeyValuePair<string, string>("South-East", "+X,+Z direction (north-east)"),

            new KeyValuePair<string, string>("South (in ngle view)", "+X direction (east)"),
            new KeyValuePair<string, string>("East (in ngle view)", "+Z direction (north)"),
            new KeyValuePair<string, string>("North (in ngle view)", "-X direction (west)"),
            new KeyValuePair<string, string>("West (in ngle view)", "-Z direction (south)"),

            new KeyValuePair<string, string>("South", "+X direction (east)"),
            new KeyValuePair<string, string>("East", "+Z direction (north)"),
            new KeyValuePair<string, string>("North", "-X direction (west)"),
            new KeyValuePair<string, string>("West", "-Z direction (south)"),
        };

        private static bool IsSeparator(char @char)
            => char.IsSeparator(@char)
            || @char is '(' or ')' or '-' or ':' or '.' or '\n' or '\r';

        private static string AdjustCardinalDirections(string name, int startIndex = 0)
        {
            while (true)
            {
                // Search for first occurrence of a word we are going to replace
                int matchStringIndex = -1;
                int matchTableIndex = -1;

                for (int i = 0; i < cardinalReplacementTable.Length; ++i)
                {
                    int newIndex = name.IndexOf(cardinalReplacementTable[i].Key, startIndex, StringComparison.InvariantCultureIgnoreCase);
                    string dbgString = name[Math.Max(0, newIndex - 1)..];

                    if (newIndex == -1)
                        continue;

                    if (newIndex != 0 && !IsSeparator(name[newIndex - 1])) // Must be an entire word
                        continue;

                    if (newIndex != (name.Length - cardinalReplacementTable[i].Key.Length) && !IsSeparator(name[newIndex + cardinalReplacementTable[i].Key.Length])) // Must be an entire word
                        continue;

                    if (matchStringIndex != -1 && matchStringIndex <= newIndex) // Discard matches later in the string for now.
                        continue;

                    matchStringIndex = newIndex;
                    matchTableIndex = i;
                }

                string dbgString2 = name[Math.Max(0, matchStringIndex - 1)..];

                if (matchTableIndex == -1)
                    return name;

                // Replace occurance
                KeyValuePair<string, string> replacement = cardinalReplacementTable[matchTableIndex];
                name = name.Remove(matchStringIndex, replacement.Key.Length).Insert(matchStringIndex, replacement.Value);
                startIndex = matchStringIndex + replacement.Value.Length;
            }
        }

        private static SortedList<ushort, TriggerParameterUshort> GetListFromTxt(string name)
        {
            var result = new SortedList<ushort, TriggerParameterUshort>();

            using (var reader = new StreamReader(File.OpenRead("NG\\" + name + ".txt")))
            {
                while (!reader.EndOfStream)
                {
                    string[] tokens = reader.ReadLine().Trim().Split(':');
                    result.Add(ToU16(int.Parse(tokens[0])), new NgTriggerSubtype(ToU16(int.Parse(tokens[0])), XmlEncodeString(AdjustCardinalDirections(tokens[1].Trim()))));
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
                if (line.StartsWith("<START"))
                    break;
            }

            if (reader.EndOfStream)
                return null;

            string[] tokens = line.Split('_');
            var block = new NgBlock();

            // Block type
            if (tokens[1] is "TRIGGERWHAT" or "TRIGGERTYPE" or "TEXTS")
                block.Type = NgBlockType.Trigger;
            else if (tokens[1] == "EFFECT")
                block.Type = NgBlockType.FlipEffect;
            else if (tokens[1] == "ACTION")
                block.Type = NgBlockType.Action;
            else if (tokens[1] == "CONDITION")
                block.Type = NgBlockType.Condition;
            else
                throw new Exception("Unknown token[1]");

            // Parameter type
            if (tokens[3].StartsWith("O"))
                block.ParameterType = NgParameterType.Object;
            else if (tokens[3].StartsWith("E"))
                block.ParameterType = NgParameterType.Extra;
            else if (tokens[3].StartsWith("T"))
                block.ParameterType = NgParameterType.Timer;
            else if (tokens[3].StartsWith("B"))
                block.ParameterType = NgParameterType.Extra;
            else
                throw new Exception("Unknown token[3]");

            block.Id = ToU16(int.Parse(tokens[2]));

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().Trim();

                if (line.StartsWith(";") || line == "")
                    continue;

                if (line.StartsWith("<END"))
                    break;

                block.Items.Add(AdjustCardinalDirections(line));
            }

            return block;
        }

        private static ushort ToU16(long data)
            => unchecked((ushort)checked((short)data));

        private static ushort GetItemId(string line)
            => ToU16(int.Parse(line.Trim().Split(':')[0]));

        private static string GetItemValue(string line)
            => line.Trim().Split(':')[1];

        private static string XmlEncodeString(string s)
        {
            s = s.Replace("&", "&amp;");
            s = s.Replace("<", "&lt;");
            s = s.Replace(">", "&gt;");
            s = s.Replace("\"", "");

            return s;
        }

        private static readonly char[] numberChars = new[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '+', '-'
        };

        private struct ParsedSubstring
        {
            public string String;
            public decimal? Value;
        }

        private struct Parsed : IComparable<Parsed>
        {
            public List<ParsedSubstring> Substrings;
            public TriggerParameterUshort Original;

            public readonly int CompareTo(Parsed other)
            {
                if (Original.Key < other.Original.Key)
                    return -1;

                if (Original.Key > other.Original.Key)
                    return 1;

                return 0;
            }
        }

        private static bool IsSeparator(char @char, bool start)
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

        private static List<ParsedSubstring> SplitIntoNumbers(string str, int startPos = 0)
        {
            str = str.Trim();
            var list = new List<ParsedSubstring>();

            while (startPos < str.Length)
            {
                int nextStart = str.IndexOfAny(numberChars, startPos);

                if (nextStart == -1)
                    break;

                int nextEnd = str.IndexOf(c => !numberChars.Contains(c), nextStart + 1, str.Length);
                string numberSubstring = str[nextStart..nextEnd];

                if (!decimal.TryParse(numberSubstring, out decimal number))
                {
                    startPos = nextEnd + 1;
                    continue;
                }

                // Preserve exactly one whitespace before the number
                string preStr = str[..nextStart];
                int preStrWhiteSpaceCount = preStr.Reverse().TakeWhile(@char => char.IsWhiteSpace(@char)).Count();

                if (preStrWhiteSpaceCount > 0)
                {
                    if (preStr.Length != preStrWhiteSpaceCount && !IsSeparator(preStr[preStr.Length - preStrWhiteSpaceCount - 1], true))
                        preStrWhiteSpaceCount--;

                    numberSubstring = preStr[^preStrWhiteSpaceCount..] + numberSubstring;
                    preStr = preStr[..^preStrWhiteSpaceCount];
                }

                // Preserve exactly one whitespace after the number
                string postStr = str[nextEnd..];
                int postStrWhiteSpaceCount = postStr.TakeWhile(@char => char.IsWhiteSpace(@char)).Count();

                if (postStrWhiteSpaceCount > 0)
                {
                    if (postStr.Length != postStrWhiteSpaceCount && !IsSeparator(postStr[postStrWhiteSpaceCount], false))
                        postStrWhiteSpaceCount--;

                    numberSubstring += postStr[..postStrWhiteSpaceCount];
                    postStr = postStr[postStrWhiteSpaceCount..];
                }

                // Add parsed values
                if (!string.IsNullOrEmpty(preStr))
                    list.Add(new ParsedSubstring { String = preStr });

                list.Add(new ParsedSubstring { String = numberSubstring, Value = number });
                str = postStr;
                startPos = 0;
            }

            if (!string.IsNullOrEmpty(str))
                list.Add(new ParsedSubstring { String = str });

            return list;
        }

        private static void AddFixed(List<NgParameterRange> choice, IEnumerable<TriggerParameterUshort> enumsToAdd)
        {
            if (!enumsToAdd.Any())
                return;

            for (int i = 0; i < choice.Count; ++i)
            {
                if (choice[i].Kind == NgParameterKind.FixedEnumeration)
                {
                    choice[i] = new NgParameterRange(choice[i].FixedEnumeration.Values.Concat(enumsToAdd).ToDictionary(e => e.Key));
                    return;
                }
            }

            choice.Add(new NgParameterRange(enumsToAdd.ToDictionary(e => e.Key)));
        }

        private static bool DetectLinearity(List<NgParameterRange> outChoice, List<Parsed> unmappedEnumsSorted, NgBlock dbgBlock)
        {
            if (unmappedEnumsSorted.Count <= 5) // It's not worth it if there are at most 5 elements
            {
                AddFixed(outChoice, unmappedEnumsSorted.Select(p => p.Original));
                unmappedEnumsSorted.Clear();
                return false;
            }

            Parsed firstEnum = unmappedEnumsSorted.First();
            ushort idStart = firstEnum.Original.Key;
            int count = 1;

            // Stop if there is a varying amount of numbers in the strings or if different values are fixed.
            for (int i = 1; i < unmappedEnumsSorted.Count; ++i)
            {
                Parsed entry = unmappedEnumsSorted[i];

                // Don't compress certain strings
                if (entry.Original.Name.StartsWith("PUZZLE_ITEM", StringComparison.InvariantCulture) ||
                    entry.Original.Name.StartsWith("KEY_ITEM", StringComparison.InvariantCulture) ||
                    entry.Original.Name.StartsWith("Send command for ", StringComparison.InvariantCulture) ||
                    (entry.Original.Name.Length == 2 && entry.Original.Name.StartsWith("F", StringComparison.InvariantCulture)) || // F keys
                    (entry.Original.Name.Length == 7 && entry.Original.Name.StartsWith("Number", StringComparison.InvariantCulture))) // Number keys
                    break;

                if (entry.Substrings.Count != firstEnum.Substrings.Count)
                    break;

                for (int j = 0; j < entry.Substrings.Count; ++j)
                {
                    if (entry.Substrings[j].Value != null != (firstEnum.Substrings[j].Value != null))
                        goto ExitLoop0;
                }

                for (int j = 0; j < entry.Substrings.Count; ++j)
                {
                    if (entry.Substrings[j].Value == null && entry.Substrings[j].String != firstEnum.Substrings[j].String)
                        goto ExitLoop0;
                }

                // Check if the run is linear
                // There can't be missing keys.
                if (idStart + count != entry.Original.Key)
                    break;

                ++count;
            }

        ExitLoop0:
            if (count <= 5) // It's not worth it if there are at most 5 elements
            {
                AddFixed(outChoice, unmappedEnumsSorted.Take(count).Select(p => p.Original));
                unmappedEnumsSorted.RemoveRange(0, count);
                return true;
            }

            // String format now is perfectly fine.
            // We just have to find some kind of linear pattern
            Parsed secondEnum = unmappedEnumsSorted[1];
            var linearParameters = new List<NgLinearParameter>(firstEnum.Substrings.Count);

            for (int i = 0; i < firstEnum.Substrings.Count; ++i)
            {
                if (firstEnum.Substrings[i].Value == null)
                {
                    linearParameters.Add(new NgLinearParameter { FixedStr = firstEnum.Substrings[i].String });
                }
                else
                {
                    decimal xDistance = secondEnum.Original.Key - firstEnum.Original.Key;
                    decimal yDistance = secondEnum.Substrings[i].Value.Value - firstEnum.Substrings[i].Value.Value;
                    decimal factor = yDistance / xDistance;
                    decimal add = firstEnum.Substrings[i].Value.Value - (firstEnum.Original.Key * factor);
                    linearParameters.Add(new NgLinearParameter { Factor = factor, Add = add });
                }
            }

            // Eliminate unnecessary linear parameters
            // (i.e. linear with 0 slope)
            for (int i = 0; i < linearParameters.Count; ++i)
            {
                if (linearParameters[i].FixedStr != null && linearParameters[i].Factor == 0)
                    linearParameters[i] = new NgLinearParameter { FixedStr = firstEnum.Substrings[i].String };
            }

            for (int i = linearParameters.Count - 1; i >= 1; --i)
            {
                if (linearParameters[i].FixedStr != null && linearParameters[i - 1].FixedStr != null)
                {
                    linearParameters[i - 1] = new NgLinearParameter { FixedStr = linearParameters[i - 1].FixedStr + linearParameters[i].FixedStr };
                    linearParameters.RemoveAt(i);

                    for (int j = 0; j < count; ++j)
                        unmappedEnumsSorted[j].Substrings.RemoveAt(i);
                }
            }

            // Check that the linear pattern is indeed a good fit
            for (int i = 0; i < count; ++i)
            {
                Parsed @enum = unmappedEnumsSorted[i];

                for (int j = 0; j < linearParameters.Count; ++j)
                {
                    if (linearParameters[j].FixedStr == null)
                    {
                        // Calculate prediction with the linear model
                        decimal value = (linearParameters[j].Factor * @enum.Original.Key) + linearParameters[j].Add;

                        // Check prediction
                        if (@enum.Substrings[j].Value.Value != value)
                        {
                            count = i;
                            goto ExitLoop1;
                        }
                    }
                }
            }

        ExitLoop1:
            if (count < 2 + (linearParameters.Count * 2)) // It's not worth it if there are at most 5 elements
            {
                AddFixed(outChoice, unmappedEnumsSorted.Take(count).Select(p => p.Original));
                unmappedEnumsSorted.RemoveRange(0, count);
                return true;
            }

            outChoice.Add(new NgParameterRange(new NgLinearModel
            {
                Start = idStart,
                EndInclusive = unchecked((ushort)(idStart + count - 1)),
                Parameters = linearParameters
            }));

            unmappedEnumsSorted.RemoveRange(0, count);
            return true;
        }

        private static NgParameterRange DetectPattern(NgParameterRange parameter, NgBlock dbgBlock)
        {
            if (parameter.Kind != NgParameterKind.FixedEnumeration)
                return parameter;

            // Parse the numbers inside the strings.
            var unmappedEnums = new List<Parsed>();

            foreach (KeyValuePair<ushort, TriggerParameterUshort> entry in parameter.FixedEnumeration)
                unmappedEnums.Add(new Parsed { Original = entry.Value, Substrings = SplitIntoNumbers(entry.Value.Name) });

            unmappedEnums.Sort();

            var outputChoice = new List<NgParameterRange>();

            while (DetectLinearity(outputChoice, unmappedEnums, dbgBlock))
            { }

            return new NgParameterRange(outputChoice);
        }

        private static NgParameterRange GetList(NgBlock block)
        {
            if (block.Items.Count == 1 && block.Items[0].StartsWith("#"))
            {
                // Special list
                string list = block.Items[0];

                // Dynamic list?
                if (list == "#ROOMS_255#")
                    return new NgParameterRange(NgParameterKind.Rooms255);
                else if (list == "#SOUND_EFFECT_A#")
                    return new NgParameterRange(NgParameterKind.SoundEffectsA);
                else if (list == "#SOUND_EFFECT_B#")
                    return new NgParameterRange(NgParameterKind.SoundEffectsB);
                else if (list == "#SFX_1024#")
                    return new NgParameterRange(NgParameterKind.Sfx1024);
                else if (list == "#NG_STRING_LIST_255#")
                    return new NgParameterRange(NgParameterKind.NgStringsList255);
                else if (list == "#NG_STRING_LIST_ALL#")
                    return new NgParameterRange(NgParameterKind.NgStringsAll);
                else if (list == "#PSX_STRING_LIST#")
                    return new NgParameterRange(NgParameterKind.PsxStringsList);
                else if (list == "#PC_STRING_LIST#")
                    return new NgParameterRange(NgParameterKind.PcStringsList);
                else if (list == "#STRING_LIST_255#")
                    return new NgParameterRange(NgParameterKind.StringsList255);
                else if (list == "#MOVEABLES#")
                    return new NgParameterRange(NgParameterKind.MoveablesInLevel);
                else if (list == "#SINK_LIST#")
                    return new NgParameterRange(NgParameterKind.SinksInLevel);
                else if (list == "#STATIC_LIST#")
                    return new NgParameterRange(NgParameterKind.StaticsInLevel);
                else if (list == "#FLYBY_LIST#")
                    return new NgParameterRange(NgParameterKind.FlybyCamerasInLevel);
                else if (list == "#CAMERA_EFFECTS#")
                    return new NgParameterRange(NgParameterKind.CamerasInLevel);
                else if (list is "#WAD-SLOTS#" or "#LARA_ANIM_SLOT#")
                    return new NgParameterRange(NgParameterKind.WadSlots);
                else if (list == "#STATIC_SLOTS#")
                    return new NgParameterRange(NgParameterKind.StaticsSlots);
                else if (list == "#LARA_POS_OCB#")
                    return new NgParameterRange(NgParameterKind.LaraStartPosOcb);

                // Repeated strings
                if (list.StartsWith("#REPEAT#"))
                {
                    string[] tokens = list.Replace("#REPEAT#", "").Split('#');
                    string radix = tokens[0].Replace("\"", "");
                    int start = int.Parse(tokens[1].Replace(",", ""));
                    int end = int.Parse(tokens[2].Replace(",", ""));
                    var enumeration = new SortedList<ushort, TriggerParameterUshort>();

                    for (int i = start; i < end; i++)
                        enumeration.Add(ToU16(i), new TriggerParameterUshort((ushort)i, radix + i));

                    return DetectPattern(new NgParameterRange(enumeration), block);
                }

                // TXT lists
                string listName = list.Replace("#", "");

                if (File.Exists("NG\\" + listName + ".txt"))
                    return DetectPattern(new NgParameterRange(GetListFromTxt(listName)), block);
                else
                    throw new Exception("Missing NG file");
            }
            else
            {
                // Free list
                var enumeration = new SortedList<ushort, TriggerParameterUshort>();

                foreach (string item in block.Items)
                    enumeration.Add(GetItemId(item), new TriggerParameterUshort(GetItemId(item), GetItemValue(item)));

                return DetectPattern(new NgParameterRange(enumeration), block);
            }
        }

        private static NgCatalog LoadTriggersFromFile(string filePath = "Catalogs\\NG\\NG_Constants.txt")
        {
            var result = new NgCatalog();

            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                while (!reader.EndOfStream)
                {
                    NgBlock block = ReadNgBlock(reader);

                    if (block == null)
                        break;

                    if (block.Type == NgBlockType.Trigger)
                    {
                        if (block.Id == 15)
                        {
                            // Trigger for timer field
                            result.TimerFieldTrigger = DetectPattern(new NgParameterRange(GetListFromTxt("TIMER_SIGNED_LONG")), block);
                        }
                        else if (block.Id == 9)
                        {
                            // Trigger for flip effect
                            foreach (string item in block.Items)
                                result.FlipEffectTrigger.MainList.Add(ToU16(GetItemId(item)), new NgTriggerSubtype(ToU16(GetItemId(item)), GetItemValue(item)));
                        }
                        else if (block.Id == 11)
                        {
                            // Trigger for action
                            foreach (string item in block.Items)
                                result.ActionTrigger.MainList.Add(ToU16(GetItemId(item)), new NgTriggerSubtype(ToU16(GetItemId(item)), GetItemValue(item))
                                { Target = new NgParameterRange(NgParameterKind.MoveablesInLevel) });
                        }
                        else if (block.Id == 12)
                        {
                            // Condition trigger
                            foreach (string item in block.Items)
                                result.ConditionTrigger.MainList.Add(ToU16(GetItemId(item)), new NgTriggerSubtype(ToU16(GetItemId(item)), GetItemValue(item)));
                        }
                    }
                    else if (block.Type == NgBlockType.FlipEffect)
                    {
                        if (block.ParameterType == NgParameterType.Timer)
                            result.FlipEffectTrigger.MainList[block.Id].Timer = GetList(block);
                        else if (block.ParameterType == NgParameterType.Extra)
                            result.FlipEffectTrigger.MainList[block.Id].Extra = GetList(block);
                    }
                    else if (block.Type == NgBlockType.Action)
                    {
                        if (block.ParameterType == NgParameterType.Object)
                            result.ActionTrigger.MainList[block.Id].Timer = GetList(block);
                        else if (block.ParameterType == NgParameterType.Extra)
                            result.ActionTrigger.MainList[block.Id].Extra = GetList(block);
                    }
                    else if (block.Type == NgBlockType.Condition)
                    {
                        if (block.ParameterType == NgParameterType.Object)
                            result.ConditionTrigger.MainList[block.Id].Timer = GetList(block);
                        else if (block.ParameterType == NgParameterType.Extra)
                            result.ConditionTrigger.MainList[block.Id].Extra = GetList(block);
                    }
                }
            }

            // Override some information...
            result.FlipEffectTrigger.MainList[28].Timer = GetList(new NgBlock { Items = new List<string> { "#COLORS" } });
            result.FlipEffectTrigger.MainList[30].Timer = new NgParameterRange(NgParameterKind.AnyNumber);
            result.FlipEffectTrigger.MainList[45].Timer = new NgParameterRange(NgParameterKind.AnyNumber);

            return result;
        }

        private static void SaveToXml(NgCatalog catalog, string path)
        {
            var document = new XDocument();

            document.Add(new XElement("TriggerDescription",
                new XElement("TimerFieldTrigger", WriteNgParameterRange(catalog.TimerFieldTrigger)),
                new XElement("FlipEffectTrigger", WriteNgTriggerSubtype(catalog.FlipEffectTrigger)),
                new XElement("ActionTrigger", WriteNgTriggerSubtype(catalog.ActionTrigger)),
                new XElement("ConditionTrigger", WriteNgTriggerSubtype(catalog.ConditionTrigger))));

            document.Save(path);
        }

        private static XObject[] WriteNgTriggerSubtype(NgTriggerSubtypes triggerSubtypes)
        {
            var elements = new List<XObject>();

            foreach (NgTriggerSubtype triggerSubtype in triggerSubtypes.MainList.Values)
            {
                var triggerSubtypeElement = new XElement("Subtrigger");
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
            var parameterElement = new XElement(parameter.Kind.ToString());

            switch (parameter.Kind)
            {
                case NgParameterKind.FixedEnumeration:
                    foreach (TriggerParameterUshort value in parameter.FixedEnumeration.Values)
                    {
                        parameterElement.Add(new XElement("Enum",
                            new XAttribute("K", value.Key),
                            new XAttribute("V", value.Name)));
                    }

                    break;

                case NgParameterKind.LinearModel:
                    foreach (NgLinearParameter linearParameter in parameter.LinearModel.Value.Parameters)
                    {
                        if (linearParameter.FixedStr != null)
                        {
                            parameterElement.Add(new XElement("Fixed", linearParameter.FixedStr));
                        }
                        else
                        {
                            parameterElement.Add(new XElement("Linear",
                                new XAttribute("Add", linearParameter.Add),
                                new XAttribute("Factor", linearParameter.Factor)));
                        }
                    }

                    parameterElement.Add(new XAttribute("Start", parameter.LinearModel.Value.Start));
                    parameterElement.Add(new XAttribute("End", parameter.LinearModel.Value.EndInclusive));
                    break;

                case NgParameterKind.Choice:
                    foreach (NgParameterRange choice in parameter.Choices)
                        parameterElement.Add(WriteNgParameterRange(choice));

                    break;
            }

            return new XObject[] { parameterElement };
        }
    }
}
