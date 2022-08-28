using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.LevelData.VisualScripting
{
    // Every node in visual trigger has this set of parameters. Name and color are
    // merely UI properties, while Previous/Next and ScreenPosition determines the
    // order of compilation. Every node may have or may have no any previous or
    // next nodes. If node or group of nodes is orphaned, it's treated as a whole
    // code block. If visual trigger consists of several orphaned nodes or node
    // groups, they will be compiled into single function body in order determined
    // from their screen position: top to bottom.

    // Function determines an internal lua function which is called to perform certain
    // action based on node setup. These functions are not meant to be directly called
    // from level script, but they use similar notation. Such functions may have
    // several arguments, which are boxed to string values from UI controls of a node.

    public abstract class TriggerNode : ICloneable
    {
        public string Name { get; set; } = string.Empty;
        public Vector2 ScreenPosition { get; set; } = Vector2.Zero;
        public Vector3 Color { get; set; } = Vector3.Zero;

        public string Function { get; set; } = string.Empty;
        public List<string> Arguments { get; private set; } = new List<string>();

        public TriggerNode Previous { get; set; }
        public TriggerNode Next { get; set; }

        public virtual TriggerNode Clone()
        {
            var node = (TriggerNode)MemberwiseClone();

            node.Arguments = new List<string>(Arguments);

            if (Next != null)
            {
                node.Next = Next.Clone();
                node.Next.Previous = node;
            }

            return node;
        }
        object ICloneable.Clone() => Clone();

        public override int GetHashCode()
        {
            var hash = Name.GetHashCode() ^ ScreenPosition.GetHashCode() ^ Color.GetHashCode();

            if (Function != null)
                hash ^= Function.GetHashCode();

            Arguments.ForEach(a => { if (a != null) hash ^= a.GetHashCode(); });
            if (Next != null)
                hash ^= Next.GetHashCode();

            return hash;
        }
    }

    // TriggerNodeAction implementation is similar to base one

    public class TriggerNodeAction : TriggerNode
    {
        
    }

    // Condition node uses function as a bool test and uses arguments to determine
    // value and operand to compare gotten value. Therefore, functions bound to
    // condition nodes must have boolean return value and accept at least two
    // arguments, one of which being a compared value and another being an operand
    // type. Else node is optional and allows to divert script to other branch.

    public class TriggerNodeCondition : TriggerNode
    {
        public TriggerNode Else { get; set; }

        public override TriggerNode Clone()
        {
            var node = new TriggerNodeCondition()
            {
                Color = Color,
                Function = Function,
                Name = Name,
                ScreenPosition = ScreenPosition
            };

            node.Arguments.AddRange(Arguments);

            if (Next != null)
            {
                node.Next = Next.Clone();
                node.Next.Previous = node;
            }

            if (Else != null)
            {
                node.Else = Else.Clone();
                node.Else.Previous = node;
            }

            return node;

        }
    }
}
