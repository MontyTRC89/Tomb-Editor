using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using TombLib.LevelData;

namespace TombLib.NG
{
    public class NgCatalog
    {
        public static NgParameterRange TimerFieldTrigger { get; private set; }
        public static NgTriggerSubtypes FlipEffectTrigger { get; private set; }
        public static NgTriggerSubtypes ActionTrigger { get; private set; }
        public static NgTriggerSubtypes ConditionTrigger { get; private set; }

        public static void LoadCatalog(string fileName)
        {
            var xml = new XmlDocument();
            xml.Load(fileName);

            var triggersNode = xml.ChildNodes[0].ChildNodes[0];
            foreach (XmlNode triggerNode in triggersNode.ChildNodes)
                switch (triggerNode.Name)
                {
                    case "TimerTrigger":
                        TimerFieldTrigger = ReadNgParameter(triggerNode.ChildNodes[0]);
                        break;
                    case "FlipEffectTrigger":
                        FlipEffectTrigger = ReadNgTriggerSubtypes(triggerNode.ChildNodes[0]);
                        break;
                    case "ActionTrigger":
                        ActionTrigger = ReadNgTriggerSubtypes(triggerNode.ChildNodes[0]);
                        break;
                    case "ConditionTrigger":
                        ConditionTrigger = ReadNgTriggerSubtypes(triggerNode.ChildNodes[0]);
                        break;
                }
        }

        private static NgTriggerSubtypes ReadNgTriggerSubtypes(XmlNode parentNode)
        {
            var result = new NgTriggerSubtypes();
            foreach (XmlNode timerNode in parentNode.ChildNodes)
            {
                var key = ushort.Parse(timerNode.Attributes["K"].Value, CultureInfo.InvariantCulture);
                var value = timerNode.Attributes["V"].Value;
                var triggerSubtype = new NgTriggerSubtype(key, value);

                foreach (XmlNode nodeList in timerNode.ChildNodes)
                    switch (nodeList.Name)
                    {
                        case "TargetList":
                            triggerSubtype.ObjectList = ReadNgParameter(nodeList);
                            break;
                        case "ExtraList":
                            triggerSubtype.ExtraList = ReadNgParameter(nodeList);
                            break;
                        case "TimerList":
                            triggerSubtype.TimerList = ReadNgParameter(nodeList);
                            break;
                    }

                result.MainList.Add(triggerSubtype.Key, triggerSubtype);
            }
            return result;
        }

        private static NgParameterRange ReadNgParameter(XmlNode parentNode)
        {
            var listKind = (NgParameterKind)Enum.Parse(typeof(NgParameterKind), parentNode.Attributes["Kind"].Value);
            switch (listKind)
            {
                case NgParameterKind.Fixed:
                    var fixedList = new SortedList<ushort, TriggerParameterUshort>(parentNode.ChildNodes.Count);
                    foreach (XmlNode objectNode in parentNode.ChildNodes)
                    {
                        var key = ushort.Parse(objectNode.Attributes["K"].Value, CultureInfo.InvariantCulture);
                        var name = objectNode.Attributes["V"].Value;
                        fixedList.Add(key, new TriggerParameterUshort(key, name));
                    }
                    return new NgParameterRange(fixedList);
                default:
                    return new NgParameterRange(listKind);
            }
        }
    }
}