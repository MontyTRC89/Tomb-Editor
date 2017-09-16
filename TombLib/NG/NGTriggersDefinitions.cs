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

    public class NGTriggersDefinitions
    {
        public static Dictionary<int, NGTrigger> ActionTriggers { get; private set; }
        public static Dictionary<int, NGTrigger> ConditionTriggers { get; private set; }
        public static Dictionary<int, NGTrigger> FlipEffectTriggers { get; private set; }

        public static bool LoadTriggers(Stream stream)
        {
            ActionTriggers = new Dictionary<int, NGTrigger>();
            ConditionTriggers = new Dictionary<int, NGTrigger>();
            FlipEffectTriggers = new Dictionary<int, NGTrigger>();

            using (var reader = new StreamReader(stream))
            {
                string line;
                NGParameterType parameterType;

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
                            var trigger = new NGTrigger(GetIdFromEntry(line), GetDescriptionFromEntry(line));
                            FlipEffectTriggers.Add(trigger.Id, trigger);
                        }
                    }

                    // Flipeffect parameters
                    if (line.StartsWith("<START_EFFECT"))
                    {
                        string[] tokens = line.Split('_');

                        int flipeffect = Int32.Parse(tokens[1]);

                        if (tokens[2] == "T") parameterType = NGParameterType.Timer;
                        if (tokens[2] == "O") parameterType = NGParameterType.Object;
                        if (tokens[2] == "E") parameterType = NGParameterType.Extra;
                        if (tokens[2] == "B") parameterType = NGParameterType.Button;

                        while (!reader.EndOfStream && (line = reader.ReadLine()).Trim().StartsWith("<END"))
                        {
                            var trigger = new NGTrigger(GetIdFromEntry(line), GetDescriptionFromEntry(line));
                            FlipEffectTriggers.Add(trigger.Id, trigger);
                        }
                    }
                }                
            }

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
    }
}
