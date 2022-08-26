using System.Collections.Generic;

namespace TombLib.LevelData.VisualScripting
{
    // Condition type specifies how condition node will check condition.
    // Condition type should be internally processed by lua comparer which
    // should be called in condition function itself. For this reason, 
    // condition function declaration must feature argument which will pass
    // condition type to it.

    public enum ConditionType
    {
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        More,
        MoreOrEqual
    }

    // For action node, argument type specifies how argument will be displayed
    // in UI and how incorrect values will be filtered out.

    public enum ArgumentType
    {
        Boolean,
        Numerical,
        Vector3,
        String,
        Color,
        LuaScript,      // Listable
        Moveables,      // Listable
        Statics,        // Listable
        Cameras,        // Listable
        Sinks,          // Listable
        FlybyCameras,   // Listable
        Volumes,        // Listable
        Rooms,          // Listable
        SoundEffects,   // Listable
        CompareOperand
    }

    public struct ArgumentLayout
    {
        public bool NewLine;
        public float Width;
    }

    public class NodeFunction
    {
        public string Name { get; set; }
        public bool Conditional { get; set; }
        public string Signature { get; set; }
        public List<ArgumentType> Arguments { get; private set; } = new List<ArgumentType>();
        public List<ArgumentLayout> ArgumentLayout { get; private set; } = new List<ArgumentLayout>();

        public override string ToString() => Name;
        public override int GetHashCode() => (Name + Conditional.ToString() + Signature + Arguments.Count.ToString()).GetHashCode();
    }
}
