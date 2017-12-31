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
            SaveToXml();

            Console.WriteLine("Done");

            // Test
            NgCatalog.LoadCatalog(outPath);
        }

        private static Dictionary<ushort, NgTriggerNode> GetListFromTxt(string name)
        {
            var result = new Dictionary<ushort, NgTriggerNode>();
            using (var reader = new StreamReader(File.OpenRead("NG\\" + name + ".txt")))
            {
                while (!reader.EndOfStream)
                {
                    var tokens = reader.ReadLine().Trim().Split(':');
                    result.Add(ToU16(int.Parse(tokens[0])), new NgTriggerNode(ToU16(int.Parse(tokens[0])), XmlEncodeString(tokens[1].Trim())));
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

        private static void GetList(NgBlock block, out Dictionary<ushort, NgTriggerNode> result, out NgParameterKind kind)
        {
            result = new Dictionary<ushort, NgTriggerNode>();
            kind = NgParameterKind.AnyNumber;

            if (block.Items.Count == 1 && block.Items[0].StartsWith("#"))
            {
                // Special list
                var list = block.Items[0];

                // Dynamic list?
                if (list == "#ROOMS_255#") { kind = NgParameterKind.Rooms255; return; }
                else if (list == "#SOUND_EFFECT_A#") { kind = NgParameterKind.SoundEffectsA; return; }
                else if (list == "#SOUND_EFFECT_B#") { kind = NgParameterKind.SoundEffectsB; return; }
                else if (list == "#SFX_1024#") { kind = NgParameterKind.Sfx1024; return; }
                else if (list == "#NG_STRING_LIST_255#") { kind = NgParameterKind.NgStringsList255; return; }
                else if (list == "#NG_STRING_LIST_ALL#") { kind = NgParameterKind.NgStringsAll; return; }
                else if (list == "#PSX_STRING_LIST#") { kind = NgParameterKind.PsxStringsList; return; }
                else if (list == "#PC_STRING_LIST#") { kind = NgParameterKind.PcStringsList; return; }
                else if (list == "#STRING_LIST_255#") { kind = NgParameterKind.StringsList255; return; }
                else if (list == "#MOVEABLES#") { kind = NgParameterKind.MoveablesInLevel; return; }
                else if (list == "#SINK_LIST#") { kind = NgParameterKind.SinksInLevel; return; }
                else if (list == "#STATIC_LIST#") { kind = NgParameterKind.StaticsInLevel; return; }
                else if (list == "#FLYBY_LIST#") { kind = NgParameterKind.FlybyCamerasInLevel; return; }
                else if (list == "#CAMERA_EFFECTS#") { kind = NgParameterKind.CamerasInLevel; return; }
                else if (list == "#WAD-SLOTS#") { kind = NgParameterKind.WadSlots; return; }
                else if (list == "#STATIC_SLOTS#") { kind = NgParameterKind.StaticsSlots; return; }
                else if (list == "#LARA_POS_OCB#") { kind = NgParameterKind.LaraStartPosOcb; return; }

                // Repeated strings
                if (list.StartsWith("#REPEAT#"))
                {
                    var tokens = list.Replace("#REPEAT#", "").Split('#');
                    var radix = tokens[0].Replace("\"", "");
                    var start = int.Parse(tokens[1].Replace(",", ""));
                    var end = int.Parse(tokens[2].Replace(",", ""));
                    for (var i = start; i < end; i++)
                        result.Add(ToU16(i), new NgTriggerNode((ushort)i, XmlEncodeString(radix + i)));
                    kind = (result.Count != 0 ? NgParameterKind.Fixed : NgParameterKind.Empty);
                    return;
                }

                // TXT lists
                var listName = list.Replace("#", "");
                if (File.Exists("NG\\" + listName + ".txt"))
                {
                    result = GetListFromTxt(listName);
                    kind = (result.Count != 0 ? NgParameterKind.Fixed : NgParameterKind.Empty);
                    return;
                }
                else
                    throw new Exception("Missing NG file");
            }
            else
            {
                // Free list
                foreach (var item in block.Items)
                    result.Add(GetItemId(item), new NgTriggerNode(GetItemId(item), XmlEncodeString(GetItemValue(item))));
                kind = (result.Count != 0 ? NgParameterKind.Fixed : NgParameterKind.Empty);
                return;
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
                            NgTempCatalog.TimerFieldTrigger = new NgTriggerNode(0xffff, "Timer Field");
                            NgTempCatalog.TimerFieldTrigger.TargetList = GetListFromTxt("TIMER_SIGNED_LONG");
                        }
                        else if (block.Id == 9)
                        {
                            // Trigger for flip effect
                            NgTempCatalog.FlipEffectTrigger = new NgTriggerNode(0xffff, "Flip Effect");
                            foreach (var item in block.Items)
                            {
                                NgTempCatalog.FlipEffectTrigger.TargetList.Add(GetItemId(item),
                                    new NgTriggerNode(GetItemId(item), XmlEncodeString(GetItemValue(item))));
                            }
                        }
                        else if (block.Id == 11)
                        {
                            // Trigger for action
                            NgTempCatalog.ActionTrigger = new NgTriggerNode(0xffff, "Action");
                            foreach (var item in block.Items)
                            {
                                NgTempCatalog.ActionTrigger.TimerList.Add(GetItemId(item),
                                    new NgTriggerNode(GetItemId(item), XmlEncodeString(GetItemValue(item))) { TargetListKind = NgParameterKind.MoveablesInLevel });
                            }
                        }
                        else if (block.Id == 12)
                        {
                            // Condition trigger
                            NgTempCatalog.ConditionTrigger = new NgTriggerNode(0xffff, "Condition");
                            foreach (var item in block.Items)
                            {
                                NgTempCatalog.ConditionTrigger.TimerList.Add(GetItemId(item),
                                    new NgTriggerNode(GetItemId(item), XmlEncodeString(GetItemValue(item))));
                            }
                        }
                    }
                    else if (block.Type == NgBlockType.FlipEffect)
                    {
                        if (block.ParameterType == NgParameterType.Timer)
                        {
                            var result = new Dictionary<ushort, NgTriggerNode>();
                            var kind = NgParameterKind.AnyNumber;
                            GetList(block, out result, out kind);
                            NgTempCatalog.FlipEffectTrigger.TargetList[block.Id].TimerList = result;
                            NgTempCatalog.FlipEffectTrigger.TargetList[block.Id].TimerListKind = kind;
                        }
                        else if (block.ParameterType == NgParameterType.Extra)
                        {
                            var result = new Dictionary<ushort, NgTriggerNode>();
                            var kind = NgParameterKind.AnyNumber;
                            GetList(block, out result, out kind);
                            NgTempCatalog.FlipEffectTrigger.TargetList[block.Id].ExtraList = result;
                            NgTempCatalog.FlipEffectTrigger.TargetList[block.Id].ExtraListKind = kind;
                        }
                    }
                    else if (block.Type == NgBlockType.Action)
                    {
                        if (block.ParameterType == NgParameterType.Object)
                        {
                            var result = new Dictionary<ushort, NgTriggerNode>();
                            var kind = NgParameterKind.AnyNumber;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ActionTrigger.TimerList[block.Id].TargetList = result;
                            NgTempCatalog.ActionTrigger.TimerList[block.Id].TargetListKind = kind;
                        }
                        else if (block.ParameterType == NgParameterType.Extra)
                        {
                            var result = new Dictionary<ushort, NgTriggerNode>();
                            var kind = NgParameterKind.AnyNumber;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ActionTrigger.TimerList[block.Id].ExtraList = result;
                            NgTempCatalog.ActionTrigger.TimerList[block.Id].ExtraListKind = kind;
                        }
                    }
                    else if (block.Type == NgBlockType.Condition)
                    {
                        if (block.ParameterType == NgParameterType.Object)
                        {
                            var result = new Dictionary<ushort, NgTriggerNode>();
                            var kind = NgParameterKind.AnyNumber;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].TargetList = result;
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].TargetListKind = kind;
                        }
                        else if (block.ParameterType == NgParameterType.Extra)
                        {
                            var result = new Dictionary<ushort, NgTriggerNode>();
                            var kind = NgParameterKind.AnyNumber;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ExtraList = result;
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ExtraListKind = kind;
                        }
                        /*else if (block.ParameterType == NgParameterType.Button)
                        {
                            var result = new Dictionary<ushort, NgTriggerNode>();
                            var kind = NgParameterKind.AnyNumber;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ButtonList = result;
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ButtonListKind = kind;
                        }*/
                    }
                }
            }

            {
                var result = new Dictionary<ushort, NgTriggerNode>();
                var kind = NgParameterKind.AnyNumber;
                GetList(new NgBlock { Items = new List<string> { "#COLORS" } }, out result, out kind);
                NgTempCatalog.FlipEffectTrigger.TargetList[28].TimerList = result;
                NgTempCatalog.FlipEffectTrigger.TargetList[28].TimerListKind = kind;
            }
        }

        private static void SaveToXml()
        {
            if (File.Exists(outPath)) File.Delete(outPath);

            using (var stream = File.OpenWrite(outPath))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine("<xml>");
                    writer.WriteLine("<Triggers>");

                    // Write timer trigger
                    writer.WriteLine("<TimerTrigger>");
                    writer.WriteLine("<TargetList Kind=\"Fixed\">");
                    for (var i = 0; i < NgTempCatalog.TimerFieldTrigger.TargetList.Count; i++)
                    {
                        var current = NgTempCatalog.TimerFieldTrigger.TargetList.ElementAt(i);
                        writer.WriteLine("<Target K=\"" + current.Key + "\" " +
                                         "V=\"" + current.Value.Value + "\" />");
                    }
                    writer.WriteLine("</TargetList>");
                    writer.WriteLine("</TimerTrigger>");

                    // Write flip effect trigger
                    writer.WriteLine("<FlipEffectTrigger>");
                    writer.WriteLine("<TargetList Kind=\"Fixed\">");
                    for (var i = 0; i < NgTempCatalog.FlipEffectTrigger.TargetList.Count; i++)
                    {
                        var current = NgTempCatalog.FlipEffectTrigger.TargetList.ElementAt(i);
                        writer.WriteLine("<Target K=\"" + current.Key + "\" " +
                                         "V=\"" + current.Value.Value + "\">");

                        writer.WriteLine("<TimerList Kind=\"" + current.Value.TimerListKind + "\">");
                        for (var j = 0; j < current.Value.TimerList.Count; j++)
                        {
                            var currentTimer = current.Value.TimerList.ElementAt(j);
                            writer.WriteLine("<Timer K=\"" + currentTimer.Key + "\" " +
                                             "V=\"" + currentTimer.Value.Value + "\" />");
                        }
                        writer.WriteLine("</TimerList>");

                        writer.WriteLine("<ExtraList Kind=\"" + current.Value.ExtraListKind + "\">");
                        for (var j = 0; j < current.Value.ExtraList.Count; j++)
                        {
                            var currentExtra = current.Value.ExtraList.ElementAt(j);
                            writer.WriteLine("<Extra K=\"" + currentExtra.Key + "\" " +
                                             "V=\"" + currentExtra.Value.Value + "\" />");
                        }
                        writer.WriteLine("</ExtraList>");

                        writer.WriteLine("</Target>");
                    }
                    writer.WriteLine("</TargetList>");
                    writer.WriteLine("</FlipEffectTrigger>");

                    // Write action trigger
                    writer.WriteLine("<ActionTrigger>");
                    writer.WriteLine("<TimerList Kind=\"Fixed\">");
                    for (var i = 0; i < NgTempCatalog.ActionTrigger.TimerList.Count; i++)
                    {
                        var current = NgTempCatalog.ActionTrigger.TimerList.ElementAt(i);
                        writer.WriteLine("<Timer K=\"" + current.Key + "\" " +
                                         "V=\"" + current.Value.Value + "\">");

                        writer.WriteLine("<TargetList Kind=\"" + current.Value.TargetListKind + "\">");
                        for (var j = 0; j < current.Value.TargetList.Count; j++)
                        {
                            var currentTarget = current.Value.TargetList.ElementAt(j);
                            writer.WriteLine("<Target K=\"" + currentTarget.Key + "\" " +
                                             "V=\"" + currentTarget.Value.Value + "\" />");
                        }
                        writer.WriteLine("</TargetList>");

                        writer.WriteLine("<ExtraList Kind=\"" + current.Value.ExtraListKind + "\">");
                        for (var j = 0; j < current.Value.ExtraList.Count; j++)
                        {
                            var currentExtra = current.Value.ExtraList.ElementAt(j);
                            writer.WriteLine("<Extra K=\"" + currentExtra.Key + "\" " +
                                             "V=\"" + currentExtra.Value.Value + "\" />");
                        }
                        writer.WriteLine("</ExtraList>");

                        writer.WriteLine("</Timer>");
                    }
                    writer.WriteLine("</TimerList>");
                    writer.WriteLine("</ActionTrigger>");

                    // Write condition trigger
                    writer.WriteLine("<ConditionTrigger>");
                    writer.WriteLine("<TimerList Kind=\"Fixed\">");
                    for (var i = 0; i < NgTempCatalog.ConditionTrigger.TimerList.Count; i++)
                    {
                        var current = NgTempCatalog.ConditionTrigger.TimerList.ElementAt(i);
                        writer.WriteLine("<Timer K=\"" + current.Key + "\" " +
                                         "V=\"" + current.Value.Value + "\">");

                        writer.WriteLine("<TargetList Kind=\"" + current.Value.TargetListKind + "\">");
                        for (var j = 0; j < current.Value.TargetList.Count; j++)
                        {
                            var currentTarget = current.Value.TargetList.ElementAt(j);
                            writer.WriteLine("<Target K=\"" + currentTarget.Key + "\" " +
                                             "V=\"" + currentTarget.Value.Value + "\" />");
                        }
                        writer.WriteLine("</TargetList>");

                        writer.WriteLine("<ExtraList Kind=\"" + current.Value.ExtraListKind + "\">");
                        for (var j = 0; j < current.Value.ExtraList.Count; j++)
                        {
                            var currentButton = current.Value.ExtraList.ElementAt(j);
                            writer.WriteLine("<Extra K=\"" + currentButton.Key + "\" " +
                                             "V=\"" + currentButton.Value.Value + "\" />");
                        }
                        writer.WriteLine("</ExtraList>");

                        writer.WriteLine("</Timer>");
                    }
                    writer.WriteLine("</TimerList>");
                    writer.WriteLine("</ConditionTrigger>");

                    writer.WriteLine("</Triggers>");
                    writer.WriteLine("</xml>");
                }
            }

            var xml = new XmlDocument();
            xml.Load(outPath);

            var xmlWriter = new XmlTextWriter(outPath, Encoding.UTF8);
            xmlWriter.Formatting = Formatting.Indented;

            // Write the XML into a formatting XmlTextWriter
            xml.WriteContentTo(xmlWriter);
            xmlWriter.Flush();
            xmlWriter.Close();
        }

        private static XmlElement CreateNodeForListItem(XmlDocument xml, string nodeName, int index, int key, string value)
        {
            var node = xml.CreateElement(nodeName);

            var attribute = xml.CreateAttribute("Index");
            attribute.Value = index.ToString();
            node.Attributes.Append(attribute);

            attribute = xml.CreateAttribute("Key");
            attribute.Value = key.ToString();
            node.Attributes.Append(attribute);

            attribute = xml.CreateAttribute("Text");
            attribute.Value = value;
            node.Attributes.Append(attribute);

            return node;
        }
    }
}