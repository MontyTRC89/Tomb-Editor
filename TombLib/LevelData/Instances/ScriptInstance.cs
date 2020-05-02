using System;

namespace TombLib.LevelData
{
    // This class should be always used as a base class for any script entries
    // used for any kinds of objects in TE or WT. Eventually we may collect all
    // script types in proposed script IDE and sort them in tree based on their
    // child class.

    public abstract class ScriptInstance
    {
        public string Name = string.Empty;

        // Global script assembly, e.g. activation mask, additional temp values etc must be put here.
        // If subclassed script instance doesn't require any particular events, this should be main and only script code snippet.

        public string Environment = string.Empty; 
    }
}
