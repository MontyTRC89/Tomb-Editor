namespace TombLib.LevelData.VisualTriggering
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

}
