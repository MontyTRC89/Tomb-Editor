TEN Node function script files metadata reference

This folder contains lua script files which will be referenced by both TE and TEN to work with node trigger system.
TE uses metadata from comments in these files to build UI, while TEN uses actual functions to execute node scripts.

On level compile, all files from this directory are merged into NodeFunctions.lua file, which is executed on level
start-up, given that level contains any volumes. Therefore, NodeFunctions.lua file should never be modified.

Comment metadata signature reference (metadata block is indicated by a keyword which starts with !):

 !Name "NAME" - NAME will be visible name for this function in node editor.

 !Conditional "True" - this is a condition node, otherwise it's action (or if !Conditional is not specified)

 !Description "DESC" - DESC will be a tooltip for a given function. You can use \n symbol to begin from a new line.

 !Arguments "ARGDESC1" "ARGDESC2" "ARGDESC..." - infinite amount of args, with ARGDESC parameters separated by commas as follows:

  - 0-100 - numerical value specifying width of control in percent of node width. For whole-line controls, this can be omitted.

  - NewLine - keyword which specifies if this and following arguments should appear on new UI line, until another argument with
    NewLine is encountered.

  - Boolean, Numerical, Vector3, String, Color, LuaScript, Moveables, Statics, Cameras, Sinks, FlybyCameras, Volumes, 
    Rooms, SoundEffects, WadSlots, Enumeration, CompareOperand - keywords which specify argument type and its appearance in UI.

  - [ENUMDESC1 | ENUMDESC2 | ENUMDESC...] - custom enumeration descriptors for this argument, as follows:
     For numerical value, first and second ENUMDESC values determine min/max UI range of value. Rest is ignored.
     For Moveables and WadSlots lists, ENUMDESC values will filter out object ID names which contain any of ENUMDESCs only.
     For Enumeration, ENUMDESC values will be displayed as is but converted to numericals on compilation in order of appearance.

 - Any other string value except listed above - tooltip for a given argument control.

Metadata blocks can appear in any order, except !Name - it resets parsing of current function.
Metadata parsing happens until real function block starts (which should start with LevelFuncs. prefix).
ARGDESC parameters can appear in any order, e.g. !Argument "Foo, Numerical, 20" is equal to !Argument "Numerical, 20, Foo"
ENUMDESC parameters should NOT be quoted, or else parsing will fail miserably.