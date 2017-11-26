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
    enum NgBlockType
    {
        Trigger,
        Effect,
        Condition,
        Action
    }

    class NgBlock
    {
        public NgBlockType Type;
        public NgParameterType ParameterType;
        public int Id;
        public List<string> Items;

        public NgBlock()
        {
            Items = new List<string>();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Parsing TXT files...");
            LoadTriggersFromTxt();

            Console.WriteLine("Writing XML...");
            SaveToXml();

            Console.WriteLine("Done");

            // Test
            NgCatalog.LoadCatalog("NG\\NgCatalog.xml");
        }

        private static Dictionary<int, NgTriggerNode> GetListFromTxt(string name)
        {
            var result = new Dictionary<int, NgTriggerNode>();
            using (var reader = new StreamReader(File.OpenRead("NG\\" + name + ".txt")))
            {
                while (!reader.EndOfStream)
                {
                    var tokens = reader.ReadLine().Trim().Split(':');
                    result.Add(int.Parse(tokens[0]), new NgTriggerNode(int.Parse(tokens[0]), XmlEncodeString(tokens[1])));
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
            if (tokens[1] == "TRIGGERWHAT") block.Type = NgBlockType.Trigger;
            else if (tokens[1] == "EFFECT") block.Type = NgBlockType.Effect;
            else if (tokens[1] == "ACTION") block.Type = NgBlockType.Action;
            else if (tokens[1] == "CONDITION") block.Type = NgBlockType.Condition;

            // Parameter type
            if (tokens[3] == "O") block.ParameterType = NgParameterType.Object;
            else if (tokens[3] == "E") block.ParameterType = NgParameterType.Extra;
            else if (tokens[3] == "T") block.ParameterType = NgParameterType.Timer;
            else if (tokens[3] == "B") block.ParameterType = NgParameterType.Extra;

            block.Id = int.Parse(tokens[2]);

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().Trim();
                if (line.StartsWith(";") || line == "") continue;
                if (line.StartsWith("<END")) break;
                block.Items.Add(line);
            }

            return block;
        }

        private static int GetItemId(string line)
        {
            return int.Parse(line.Trim().Split(':')[0]);
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

        private static void GetList(NgBlock block, out Dictionary<int, NgTriggerNode> result, out NgListKind kind)
        {
            result = new Dictionary<int, NgTriggerNode>();
            kind = NgListKind.Unknown;

            if (block.Items.Count == 1 && block.Items[0].StartsWith("#"))
            {
                // Special list
                var list = block.Items[0];

                // Dynamic list?
                if (list == "#ROOMS_255#") { kind = NgListKind.Rooms255; return; }
                else if (list == "#SOUND_EFFECT_A#") { kind = NgListKind.SoundEffectsA; return; }
                else if (list == "#SOUND_EFFECT_B#") { kind = NgListKind.SoundEffectsB; return; }
                else if (list == "#SFX_1024#") { kind = NgListKind.Sfx1024; return; }
                else if (list == "#NG_STRING_LIST_255#") { kind = NgListKind.NgStringsList255; return; }
                else if (list == "#NG_STRING_LIST_ALL#") { kind = NgListKind.NgStringsAll; return; }
                else if (list == "#PSX_STRING_LIST#") { kind = NgListKind.PsxStringsList; return; }
                else if (list == "#PC_STRING_LIST#") { kind = NgListKind.PcStringsList; return; }
                else if (list == "#STRING_LIST_255#") { kind = NgListKind.StringsList255; return; }
                else if (list == "#MOVEABLES#") { kind = NgListKind.MoveablesInLevel; return; }
                else if (list == "#SINK_LIST#") { kind = NgListKind.SinksInLevel; return; }
                else if (list == "#STATIC_LIST#") { kind = NgListKind.StaticsInLevel; return; }
                else if (list == "#FLYBY_LIST#") { kind = NgListKind.FlybyCamerasInLevel; return; }
                else if (list == "#CAMERA_EFFECTS#") { kind = NgListKind.CamerasInLevel; return; }
                else if (list == "#WAD-SLOTS#") { kind = NgListKind.WadSlots; return; }
                else if (list == "#STATIC_SLOTS#") { kind = NgListKind.StaticsSlots; return; }

                // Repeated strings
                if (list.StartsWith("#REPEAT#"))
                {
                    var tokens = list.Replace("#REPEAT#", "").Split('#');
                    var radix = tokens[0].Replace("\"", "");
                    var start = int.Parse(tokens[1].Replace(",", ""));
                    var end = int.Parse(tokens[2].Replace(",", ""));
                    for (var i = start; i < end; i++)
                        result.Add(i, new NgTriggerNode(i, XmlEncodeString(radix + i)));
                    kind = (result.Count != 0 ? NgListKind.Fixed : NgListKind.Empty);
                    return;
                }

                // TXT lists
                var listName = list.Replace("#", "");
                if (File.Exists("NG\\" + listName + ".txt"))
                {
                    result = GetListFromTxt(listName);
                    kind = (result.Count != 0 ? NgListKind.Fixed : NgListKind.Empty);
                    return;
                }
            }
            else
            {
                // Free list
                foreach (var item in block.Items)
                    result.Add(GetItemId(item), new NgTriggerNode(GetItemId(item), XmlEncodeString(GetItemValue(item))));
                kind = (result.Count != 0 ? NgListKind.Fixed : NgListKind.Empty);
                return;
            }
        }

        private static void LoadTriggersFromTxt()
        {
            using (var reader = new StreamReader(File.OpenRead("NG\\NG_Constants.txt")))
            {
                string line;

                while (!reader.EndOfStream)
                {
                    var block = ReadNgBlock(reader);
                    if (block == null)
                        return;

                    if (block.Type == NgBlockType.Trigger)
                    {
                        if (block.Id == 15)
                        {
                            // Trigger for timer field
                            NgTempCatalog.TimerFieldTrigger = new NgTriggerNode(-1, "Timer Field");
                            NgTempCatalog.TimerFieldTrigger.ObjectList = GetListFromTxt("TIMER_SIGNED_LONG");
                        }
                        else if (block.Id == 9)
                        {
                            // Trigger for flip effect
                            NgTempCatalog.FlipEffectTrigger = new NgTriggerNode(-1, "Flip Effect");
                            foreach (var item in block.Items)
                            {
                                NgTempCatalog.FlipEffectTrigger.ObjectList.Add(GetItemId(item),
                                    new NgTriggerNode(GetItemId(item), XmlEncodeString(GetItemValue(item))));
                            }
                        }
                        else if (block.Id == 11)
                        {
                            // Trigger for action
                            NgTempCatalog.ActionTrigger = new NgTriggerNode(-1, "Action");
                            foreach (var item in block.Items)
                            {
                                NgTempCatalog.ActionTrigger.TimerList.Add(GetItemId(item),
                                    new NgTriggerNode(GetItemId(item), XmlEncodeString(GetItemValue(item))));
                            }
                        }
                        else if (block.Id == 12)
                        {
                            // Condition trigger
                            NgTempCatalog.ConditionTrigger = new NgTriggerNode(-1, "Condition");
                            foreach (var item in block.Items)
                            {
                                NgTempCatalog.ConditionTrigger.TimerList.Add(GetItemId(item),
                                    new NgTriggerNode(GetItemId(item), XmlEncodeString(GetItemValue(item))));
                            }
                        }
                    }
                    else if (block.Type == NgBlockType.Effect)
                    {
                        if (block.ParameterType == NgParameterType.Timer)
                        {
                            var result = new Dictionary<int, NgTriggerNode>();
                            var kind = NgListKind.Unknown;
                            GetList(block, out result, out kind);
                            NgTempCatalog.FlipEffectTrigger.ObjectList[block.Id].TimerList = result;
                            NgTempCatalog.FlipEffectTrigger.ObjectList[block.Id].TimerListKind = kind;
                        }
                        else if (block.ParameterType == NgParameterType.Extra)
                        {
                            var result = new Dictionary<int, NgTriggerNode>();
                            var kind = NgListKind.Unknown;
                            GetList(block, out result, out kind);
                            NgTempCatalog.FlipEffectTrigger.ObjectList[block.Id].ExtraList = result;
                            NgTempCatalog.FlipEffectTrigger.ObjectList[block.Id].ExtraListKind = kind;
                        }
                    }
                    else if (block.Type == NgBlockType.Action)
                    {
                        if (block.ParameterType == NgParameterType.Object)
                        {
                            var result = new Dictionary<int, NgTriggerNode>();
                            var kind = NgListKind.Unknown;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ActionTrigger.TimerList[block.Id].ObjectList = result;
                            NgTempCatalog.ActionTrigger.TimerList[block.Id].ObjectListKind = kind;
                        }
                        else if (block.ParameterType == NgParameterType.Extra)
                        {
                            var result = new Dictionary<int, NgTriggerNode>();
                            var kind = NgListKind.Unknown;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ActionTrigger.TimerList[block.Id].ExtraList = result;
                            NgTempCatalog.ActionTrigger.TimerList[block.Id].ExtraListKind = kind;
                        }
                    }
                    else if (block.Type == NgBlockType.Condition)
                    {
                        if (block.ParameterType == NgParameterType.Object)
                        {
                            var result = new Dictionary<int, NgTriggerNode>();
                            var kind = NgListKind.Unknown;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ObjectList = result;
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ObjectListKind = kind;
                        }
                        else if (block.ParameterType == NgParameterType.Extra)
                        {
                            var result = new Dictionary<int, NgTriggerNode>();
                            var kind = NgListKind.Unknown;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ExtraList = result;
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ExtraListKind = kind;
                        }
                        /*else if (block.ParameterType == NgParameterType.Button)
                        {
                            var result = new Dictionary<int, NgTriggerNode>();
                            var kind = NgListKind.Unknown;
                            GetList(block, out result, out kind);
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ButtonList = result;
                            NgTempCatalog.ConditionTrigger.TimerList[block.Id].ButtonListKind = kind;
                        }*/
                    }
                }
            }
        }

        private static void SaveToXml()
        {
            if (File.Exists("NG\\NgCatalog.xml")) File.Delete("NG\\NgCatalog.xml");

            using (var stream = File.OpenWrite("NG\\NgCatalog.xml"))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.WriteLine("<xml>");
                    writer.WriteLine("<Triggers>");

                    // Write timer trigger
                    writer.WriteLine("<TimerTrigger>");
                    writer.WriteLine("<ObjectList Kind=\"Fixed\">");
                    for (var i = 0; i < NgTempCatalog.TimerFieldTrigger.ObjectList.Count; i++)
                    {
                        var current = NgTempCatalog.TimerFieldTrigger.ObjectList.ElementAt(i);
                        writer.WriteLine("<Object K=\"" + current.Key + "\" " +
                                         "V=\"" + current.Value.Value + "\" />");
                    }
                    writer.WriteLine("</ObjectList>");
                    writer.WriteLine("</TimerTrigger>");

                    // Write flip effect trigger
                    writer.WriteLine("<FlipEffectTrigger>");
                    writer.WriteLine("<ObjectList Kind=\"Fixed\">");
                    for (var i = 0; i < NgTempCatalog.FlipEffectTrigger.ObjectList.Count; i++)
                    {
                        var current = NgTempCatalog.FlipEffectTrigger.ObjectList.ElementAt(i);
                        writer.WriteLine("<Object K=\"" + current.Key + "\" " +
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

                        writer.WriteLine("</Object>");
                    }
                    writer.WriteLine("</ObjectList>");
                    writer.WriteLine("</FlipEffectTrigger>");

                    // Write action trigger
                    writer.WriteLine("<ActionTrigger>");
                    writer.WriteLine("<TimerList Kind=\"Fixed\">");
                    for (var i = 0; i < NgTempCatalog.ActionTrigger.TimerList.Count; i++)
                    {
                        var current = NgTempCatalog.ActionTrigger.TimerList.ElementAt(i);
                        writer.WriteLine("<Timer K=\"" + current.Key + "\" " +
                                         "V=\"" + current.Value.Value + "\">");

                        writer.WriteLine("<ObjectList Kind=\"" + current.Value.ObjectListKind + "\">");
                        for (var j = 0; j < current.Value.ObjectList.Count; j++)
                        {
                            var currentObject = current.Value.ObjectList.ElementAt(j);
                            writer.WriteLine("<Object K=\"" + currentObject.Key + "\" " +
                                             "V=\"" + currentObject.Value.Value + "\" />");
                        }
                        writer.WriteLine("</ObjectList>");

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

                        writer.WriteLine("<ObjectList Kind=\"" + current.Value.ObjectListKind + "\">");
                        for (var j = 0; j < current.Value.ObjectList.Count; j++)
                        {
                            var currentObject = current.Value.ObjectList.ElementAt(j);
                            writer.WriteLine("<Object K=\"" + currentObject.Key + "\" " +
                                             "V=\"" + currentObject.Value.Value + "\" />");
                        }
                        writer.WriteLine("</ObjectList>");

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
            xml.Load("NG\\NgCatalog.xml");

            var xmlWriter = new XmlTextWriter("NG\\NgCatalog.xml", Encoding.UTF8);
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