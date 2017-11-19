using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    internal enum CurrentSectionType
    {
        None,
        FlipEffectTrigger,
        ConditionTrigger,
        ActionTrigger,
    }

    public class NgTriggersDefinitions
    {
        public static Dictionary<int, NgTrigger> ActionTriggers { get; private set; }
        public static Dictionary<int, NgTrigger> ConditionTriggers { get; private set; }
        public static Dictionary<int, NgTrigger> FlipEffectTriggers { get; private set; }

        public static bool LoadTriggers(Stream stream)
        {
            ActionTriggers = new Dictionary<int, NgTrigger>();
            ConditionTriggers = new Dictionary<int, NgTrigger>();
            FlipEffectTriggers = new Dictionary<int, NgTrigger>();

            /*using (var reader = new StreamReader(stream))
            {
                string line;
                //NGParameterType parameterType;

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine().Trim();

                    // Skip comments and blank lines
                    if (line.StartsWith(";") || line == "") continue;

                    // Flipeffects
                    if (line.StartsWith("<START_TRIGGERWHAT_9"))
                    {
                        while (!reader.EndOfStream && (line = reader.ReadLine()).Trim().StartsWith("<END"))
                        {
                            var trigger = new NgTrigger(GetIdFromEntry(line), GetDescriptionFromEntry(line));
                            FlipEffectTriggers.Add(trigger.Id, trigger);
                        }
                    }

                    // Flipeffect parameters
                    if (line.StartsWith("<START_EFFECT"))
                    {
                        string[] tokens = line.Split('_');

                        int flipeffect = Int32.Parse(tokens[1]);

                        //if (tokens[2] == "T") parameterType = NGParameterType.Timer;
                        //if (tokens[2] == "O") parameterType = NGParameterType.Object;
                        //if (tokens[2] == "E") parameterType = NGParameterType.Extra;
                        //if (tokens[2] == "B") parameterType = NGParameterType.Button;

                        while (!reader.EndOfStream && (line = reader.ReadLine()).Trim().StartsWith("<END"))
                        {
                            var trigger = new NgTrigger(GetIdFromEntry(line), GetDescriptionFromEntry(line));
                            FlipEffectTriggers.Add(trigger.Id, trigger);
                        }
                    }
                }
            }*/

            return true;
        }

        private static int GetIdFromEntry(string line)
        {
            string strId = line.Substring(0, line.IndexOf(':'));
            return Int32.Parse(strId);
        }

        private static string GetDescriptionFromEntry(string line)
        {
            return (line.Substring(line.IndexOf(':') + 1, line.Length - line.IndexOf(':') - 1));
        }

        private static List<NgVectorPair> GetList(NgListKind kind)
        {
            var dict = new List<NgVectorPair>();

            switch (kind)
            {
                case NgListKind.Percentage:
                    for (var i = 1; i < 10; i++)
                        dict.Add(new NgVectorPair(i / 10.0f, "0." + i));
                    for (var i = 1; i < 10; i++)
                        dict.Add(new NgVectorPair(i * 1.0f, i.ToString()));
                    for (var i = 0; i < 10; i++)
                        dict.Add(new NgVectorPair(10.0f * i, (10 * i).ToString()));
                    break;

                case NgListKind.Slots:
                    break;

                case NgListKind.StaticSlots:
                    break;

                case NgListKind.Transparency32:
                    for (var i = 0; i < 31; i++)
                        dict.Add(new NgVectorPair(i * 4, "Trasparency = " + i));
                    dict.Add(new NgVectorPair(126, "Trasparency = " + 126));
                    break;

                case NgListKind.BitList:
                    for (var i = 0; i < 32; i++)
                        dict.Add(new NgVectorPair(1 << i, "Bit " + i + " (" + (1 << 1).ToString("X") + " ; " + (1 << 1)));
                    break;

                case NgListKind.NegativeNumbers:
                    for (var i = 0; i < 128; i++)
                        dict.Add(new NgVectorPair(i - 128, (i - 128).ToString()));
                    break;

                case NgListKind.CollisionFloor:
                    dict.Add(new NgVectorPair(0, "No change"));
                    for (var i = 1; i < 64; i++)
                        dict.Add(new NgVectorPair(i, "Decrease floor collision by " + i + " clicks"));
                    for (var i = 1; i < 64; i++)
                        dict.Add(new NgVectorPair(i + 64, "Increase floor collision by " + i + " clicks"));
                    break;

                case NgListKind.CollisionCeiling:
                    dict.Add(new NgVectorPair(0, "No change"));
                    for (var i = 1; i < 64; i++)
                        dict.Add(new NgVectorPair(i, "Decrease ceiling collision by " + i + " clicks"));
                    for (var i = 1; i < 64; i++)
                        dict.Add(new NgVectorPair(i + 64, "Increase ceiling collision by " + i + " clicks"));
                    break;

                case NgListKind.MemoryInventoryIndices:
                    dict = ReadVectorFileInt("MemoryInventoryIndices");
                    break;

                case NgListKind.MicroClicks:
                    for (var i = 0; i < 128; i++)
                        dict.Add(new NgVectorPair((i + 1) * 8, "Units = " + ((i + 1) * 8)));
                    break;

                case NgListKind.MemoryAnimation:
                    dict = ReadVectorFileInt("MemoryAnimation");
                    break;

                case NgListKind.MemoryInventory:
                    dict = ReadVectorFileInt("MemoryInventory");
                    break;

                case NgListKind.MemorySlot:
                    dict = ReadVectorFileInt("MemorySlot");
                    break;

                case NgListKind.MemoryCode:
                    dict = ReadVectorFileInt("MemoryCode");
                    break;

                case NgListKind.MemoryItem:
                    dict = ReadVectorFileInt("MemoryItem");
                    break;

                case NgListKind.MemorySave:
                    dict = ReadVectorFileInt("MemorySave");
                    break;

                //case NgListKind.VarText:

            }

            return dict;
        }

        private static List<NgVectorPair> ReadVectorFileInt(string filename)
        {
            var result = new List<NgVectorPair>();
            using (var reader = new StreamReader(File.OpenRead("Resources\\" + filename + ".txt")))
            {
                while (!reader.EndOfStream)
                {
                    var tokens = reader.ReadLine().Trim().Split(':');
                    result.Add(new NgVectorPair(int.Parse(tokens[0]), tokens[1]));
                }
            }
            return result;
        }
    }
}
