using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TombLib.NG;

namespace NgXmlBuilder
{
    public class NgTempCatalog
    {
        public static NgTriggerNode ActionTrigger { get; set; }
        public static NgTriggerNode ConditionTrigger { get; set; }
        public static NgTriggerNode FlipEffectTrigger { get; set; }
        public static NgTriggerNode TimerFieldTrigger { get; set; }

        public static void LoadCatalog(string fileName)
        {
            var xml = new XmlDocument();
            xml.Load(fileName);

            var triggersNode = xml.ChildNodes[0].ChildNodes[0];
            foreach (XmlNode triggerNode in triggersNode.ChildNodes)
            {
                if (triggerNode.Name == "TimerTrigger")
                {
                    TimerFieldTrigger = new NgTriggerNode(0xffff, "TimerTrigger");

                    var TargetList = triggerNode.ChildNodes[0];
                    foreach (XmlNode targetNode in TargetList.ChildNodes)
                    {
                        var key = ushort.Parse(targetNode.Attributes["K"].Value);
                        var value = targetNode.Attributes["V"].Value;
                        TimerFieldTrigger.TargetList.Add(key, new NgTriggerNode(key, value));
                    }
                }
                else if (triggerNode.Name == "FlipEffectTrigger")
                {
                    FlipEffectTrigger = new NgTriggerNode(0xffff, "FlipEffectTrigger");

                    var TargetList = triggerNode.ChildNodes[0];
                    foreach (XmlNode targetNode in TargetList.ChildNodes)
                    {
                        var key = ushort.Parse(targetNode.Attributes["K"].Value);
                        var value = targetNode.Attributes["V"].Value;
                        var node = new NgTriggerNode(key, value);

                        foreach (XmlNode nodeList in targetNode.ChildNodes)
                        {
                            if (nodeList.Name == "TimerList")
                            {
                                var listKind = (NgParameterKind)Enum.Parse(typeof(NgParameterKind), nodeList.Attributes["Kind"].Value);
                                node.TimerListKind = listKind;

                                foreach (XmlNode timerNode in nodeList.ChildNodes)
                                {
                                    var key2 = ushort.Parse(timerNode.Attributes["K"].Value);
                                    var value2 = timerNode.Attributes["V"].Value;
                                    var node2 = new NgTriggerNode(key2, value2);
                                    node.TimerList.Add(key2, node2);
                                }
                            }
                            else if (nodeList.Name == "ExtraList")
                            {
                                var listKind = (NgParameterKind)Enum.Parse(typeof(NgParameterKind), nodeList.Attributes["Kind"].Value);
                                node.ExtraListKind = listKind;

                                foreach (XmlNode extraNode in nodeList.ChildNodes)
                                {
                                    var key2 = ushort.Parse(extraNode.Attributes["K"].Value);
                                    var value2 = extraNode.Attributes["V"].Value;
                                    var node2 = new NgTriggerNode(key2, value2);
                                    node.ExtraList.Add(key2, node2);
                                }
                            }
                        }

                        FlipEffectTrigger.TargetList.Add(node.Id, node);
                    }
                }
                else if (triggerNode.Name == "ActionTrigger")
                {
                    ActionTrigger = new NgTriggerNode(0xffff, "ActionTrigger");

                    var timerList = triggerNode.ChildNodes[0];
                    foreach (XmlNode timerNode in timerList.ChildNodes)
                    {
                        var key = ushort.Parse(timerNode.Attributes["K"].Value);
                        var value = timerNode.Attributes["V"].Value;
                        var node = new NgTriggerNode(key, value);

                        foreach (XmlNode nodeList in timerNode.ChildNodes)
                        {
                            if (nodeList.Name == "TargetList")
                            {
                                var listKind = (NgParameterKind)Enum.Parse(typeof(NgParameterKind), nodeList.Attributes["Kind"].Value);
                                node.TargetListKind = listKind;

                                foreach (XmlNode targetNode in nodeList.ChildNodes)
                                {
                                    var key2 = ushort.Parse(targetNode.Attributes["K"].Value);
                                    var value2 = targetNode.Attributes["V"].Value;
                                    var node2 = new NgTriggerNode(key2, value2);
                                    node.TargetList.Add(key2, node2);
                                }
                            }
                            else if (nodeList.Name == "ExtraList")
                            {
                                var listKind = (NgParameterKind)Enum.Parse(typeof(NgParameterKind), nodeList.Attributes["Kind"].Value);
                                node.ExtraListKind = listKind;

                                foreach (XmlNode extraNode in nodeList.ChildNodes)
                                {
                                    var key2 = ushort.Parse(extraNode.Attributes["K"].Value);
                                    var value2 = extraNode.Attributes["V"].Value;
                                    var node2 = new NgTriggerNode(key2, value2);
                                    node.ExtraList.Add(key2, node2);
                                }
                            }
                        }

                        ActionTrigger.TimerList.Add(node.Id, node);
                    }
                }
                else if (triggerNode.Name == "ConditionTrigger")
                {
                    ConditionTrigger = new NgTriggerNode(0xffff, "ConditionTrigger");

                    var timerList = triggerNode.ChildNodes[0];
                    foreach (XmlNode timerNode in timerList.ChildNodes)
                    {
                        var key = ushort.Parse(timerNode.Attributes["K"].Value);
                        var value = timerNode.Attributes["V"].Value;
                        var node = new NgTriggerNode(key, value);

                        foreach (XmlNode nodeList in timerNode.ChildNodes)
                        {
                            if (nodeList.Name == "TargetList")
                            {
                                var listKind = (NgParameterKind)Enum.Parse(typeof(NgParameterKind), nodeList.Attributes["Kind"].Value);
                                node.TargetListKind = listKind;

                                foreach (XmlNode targetNode in nodeList.ChildNodes)
                                {
                                    var key2 = ushort.Parse(targetNode.Attributes["K"].Value);
                                    var value2 = targetNode.Attributes["V"].Value;
                                    var node2 = new NgTriggerNode(key2, value2);
                                    node.TargetList.Add(key2, node2);
                                }
                            }
                            /*else if (nodeList.Name == "ButtonList")
                            {
                                var listKind = (NgParameterKind)Enum.Parse(typeof(NgParameterKind), nodeList.Attributes["Kind"].Value);
                                node.ButtonListKind = listKind;

                                foreach (XmlNode buttonNode in nodeList.ChildNodes)
                                {
                                    var key2 = ushort.Parse(buttonNode.Attributes["K"].Value);
                                    var value2 = buttonNode.Attributes["V"].Value;
                                    var node2 = new NgTriggerNode(key2, value2);
                                    node.ButtonList.Add(key2, node2);
                                }
                            }*/
                        }

                        ConditionTrigger.TimerList.Add(node.Id, node);
                    }
                }
            }
        }
    }
}
