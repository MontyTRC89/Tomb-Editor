## TEN Node function script files metadata reference

### Overview

This folder contains lua script files which will be referenced by both TE and TEN to work with node trigger system.
TE uses metadata from comments in these files to build UI, while TEN uses actual functions to execute node scripts.

On level compile, all files from this directory are merged into NodeFunctions.lua file, which is executed on level
start-up, given that level contains any volumes. Therefore, NodeFunctions.lua file should never be modified.

### Format

Lua node scripts should follow this convention: several metadata signatures should be followed by actual function
body which should start with conventional **LevelFuncs.** prefix. Amount of argument metadata signatures should
be the same as actual function arguments, and should be listed in the same order.

### Metadata signature reference

Comment metadata signature reference (metadata block is indicated by a keyword which starts with !):

 - **!Name "NAME"** - NAME will be visible name for this function in node editor.

 - **!Conditional "True"** - this is a condition node, otherwise it's action (or if !Conditional is not specified)

 - **!Description "DESC"** - DESC will be a tooltip for a given function. You can use \n symbol to begin from a 
   new line.

 - **!Arguments "ARGDESC1" "ARGDESC2" "ARGDESC..."** - infinite amount of args, with ARGDESC parameters separated
   by commas as follows:

  - **0-100** - numerical value specifying width of control in percent of node width. For whole-line controls,
    this can be omitted.

  - **NewLine** - keyword which specifies if this and following arguments should appear on new UI line, until
    another argument with another **NewLine** is encountered.

  - **Boolean, Numerical, Vector3, String, Color, LuaScript, Moveables, Statics, Cameras, Sinks, FlybyCameras,
    Volumes, Rooms, SoundEffects, WadSlots, Enumeration, CompareOperand** - keywords which specify argument type
    and its appearance in UI.

  - **[ENUMDESC1 | ENUMDESC2 | ENUMDESC...]** - custom enumeration descriptors for this argument, as follows:
  
     - For numerical value, first and second ENUMDESC values determine min/max UI range of value. Rest is ignored.
     - For Moveables and WadSlots lists, ENUMDESC values will filter out object ID names which contain any of
       ENUMDESCs only.
     - For Enumeration, ENUMDESC values will be displayed as is but converted to numericals on compilation
       in order of appearance.

 - **Any other string value except listed above** - tooltip for a given argument control.
 

Metadata blocks can appear in any order, except !Name - it resets parsing of current function.

Metadata parsing happens until real function block starts (which should start with LevelFuncs. prefix).

ARGDESC parameters can appear in any order, e.g. !Argument "Foo, Numerical, 20" is equal to
!Argument "Numerical, 20, Foo"

ENUMDESC parameters should NOT be quoted, or else parsing will fail miserably.


### Example

```
-- !Name "Check moveable health"
-- !Description "Compares selected moveable health with given value."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Moveable to check" "NewLine, CompareOperand, 70, Kind of check" "Numerical, 30, Hit points value" 

LevelFuncs.CheckEntityHealth = function(moveableName, operand, value)
	local health = TEN.Objects.GetMoveableByName(entityName):GetHP()
	return LevelFuncs.CompareValue(health, value, operand)
end
```

Here, **!Arguments** signature lists 3 parameters, which will be called "Moveable to check", "Kind of check"
and "Hit points value" respectively. First argument, "Moveable to check", will occupy whole second line of a
node UI, because width is not explicitly stated for it. Next two arguments, "Kind of check" and "Hit points
value", both will occupy same next line, because **NewLine** keyword is not specified for third argument.
Second argument, "Kind of check", will occupy 70 percent of line width, while "Hit points value" will occupy
30 percent. Note that arguments sitting on the same line should sum to 100 percent width, otherwise
UI symmetry is not guaranteed.

**LevelFuncs.CheckEntityHealth** function declaration should contain same amount of arguments and in the same
order as metadata argument signature. Therefore, **moveableName** will be read from "Moveable to check"
UI argument, **operand** will be read from "Kind of check", and so on.