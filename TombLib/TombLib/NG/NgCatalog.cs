﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using TombLib.LevelData;

namespace TombLib.NG
{
    public class NgCatalog
    {
        public static NgTriggerSubtypes FlipEffectTrigger { get; set; } = new NgTriggerSubtypes();
        public static NgTriggerSubtypes ActionTrigger { get; set; } = new NgTriggerSubtypes();
        public static NgTriggerSubtypes ConditionTrigger { get; set; } = new NgTriggerSubtypes();
        public static NgParameterRange TimerFieldTrigger { get; set; }

        public static void LoadCatalog(string fileName)
        {
            XmlDocument document = new XmlDocument();
            document.Load(fileName);
            XmlNode documentElement = document.GetElementsByTagName("TriggerDescription").Item(0);
            foreach (XmlNode triggerNode in documentElement.ChildNodes)
                switch (triggerNode.Name)
                {
                    case "TimerFieldTrigger":
                        TimerFieldTrigger = ReadNgParameterRange(triggerNode.ChildNodes[0]);
                        break;
                    case "FlipEffectTrigger":
                        FlipEffectTrigger = ReadNgTriggerSubtypes(triggerNode, "F");
                        break;
                    case "ActionTrigger":
                        ActionTrigger = ReadNgTriggerSubtypes(triggerNode, "A");
                        break;
                    case "ConditionTrigger":
                        ConditionTrigger = ReadNgTriggerSubtypes(triggerNode, "C");
                        break;
                }
        }

        private static NgTriggerSubtypes ReadNgTriggerSubtypes(XmlNode parentNode, string postfix = null)
        {
            var result = new NgTriggerSubtypes();
            foreach (XmlNode timerNode in parentNode.ChildNodes)
            {
                var key = ushort.Parse(timerNode.Attributes["K"].Value, CultureInfo.InvariantCulture);
                var value = timerNode.Attributes["V"].Value + ((postfix != null) ? (" (" + postfix + key.ToString() + ")") : string.Empty);
                var triggerSubtype = new NgTriggerSubtype(key, value);

                foreach (XmlNode nodeList in timerNode.ChildNodes)
                    switch (nodeList.Name)
                    {
                        case "Target":
                            triggerSubtype.Target = ReadNgParameterRange(nodeList.ChildNodes[0]);
                            break;
                        case "Extra":
                            triggerSubtype.Extra = ReadNgParameterRange(nodeList.ChildNodes[0]);
                            break;
                        case "Timer":
                            triggerSubtype.Timer = ReadNgParameterRange(nodeList.ChildNodes[0]);
                            break;
                    }

                result.MainList.Add(triggerSubtype.Key, triggerSubtype);
            }
            return result;
        }

        private static NgParameterRange ReadNgParameterRange(XmlNode node)
        {
            var listKind = (NgParameterKind)Enum.Parse(typeof(NgParameterKind), node.Name);
            switch (listKind)
            {
                case NgParameterKind.FixedEnumeration:
                    var fixedList = new SortedList<ushort, TriggerParameterUshort>(node.ChildNodes.Count);
                    foreach (XmlNode objectNode in node.ChildNodes)
                    {
                        var key = ushort.Parse(objectNode.Attributes["K"].Value, CultureInfo.InvariantCulture);
                        var name = objectNode.Attributes["V"].Value;
                        fixedList.Add(key, new TriggerParameterUshort(key, name));
                    }
                    return new NgParameterRange(fixedList);
                case NgParameterKind.LinearModel:
                    var linearParameters = new List<NgLinearParameter>(node.ChildNodes.Count);
                    foreach (XmlNode objectNode in node.ChildNodes)
                    {
                        switch (objectNode.Name)
                        {
                            case "Fixed":
                                linearParameters.Add(new NgLinearParameter { FixedStr = objectNode.InnerText });
                                break;
                            case "Linear":
                                linearParameters.Add(new NgLinearParameter
                                {
                                    Add = decimal.Parse(objectNode.Attributes["Add"].Value),
                                    Factor = decimal.Parse(objectNode.Attributes["Factor"].Value)
                                });
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    NgLinearModel linearModel = new NgLinearModel
                    {
                        Parameters = linearParameters,
                        Start = ushort.Parse(node.Attributes["Start"].Value),
                        EndInclusive = ushort.Parse(node.Attributes["End"].Value)
                    };
                    return new NgParameterRange(linearModel);

                case NgParameterKind.Choice:
                    var choices = new List<NgParameterRange>(node.ChildNodes.Count);
                    foreach (XmlNode objectNode in node.ChildNodes)
                        choices.Add(ReadNgParameterRange(objectNode));
                    return new NgParameterRange(choices);

                default:
                    return new NgParameterRange(listKind);
            }
        }
    }
}