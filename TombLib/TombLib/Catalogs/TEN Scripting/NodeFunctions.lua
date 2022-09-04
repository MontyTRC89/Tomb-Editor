-- TEN Node function script file

-- Any functions listed here, given that metadata signature is provided,
-- will be visible in node editor in TE. This file will be automatically
-- copied to TEN script folder on every level build, so it should not be
-- updated anywhere else but in TE /Catalogs subfolder.

-- Metadata signature reference:

-- | !Name "NAME" - NAME will be visible name for this function in node editor.
-- | !Conditional "True" - this function is a condition node, otherwise it's action (also if !Conditional is not specified)
-- | !Description "DESC" - DESC will be a tooltip for a given function.
-- | !Arguments "ARG1DESC" "ARG2DESC" "ARG...DESC" - infinite amount of args, with description parameters separated by commas:
--   - [Numerical value 0-100] - specifies width of control in percent of node width.
--   - NewLine - specifies if this argument should appear on new UI line.
--   - Boolean, Numerical, Vector3, String, Color, 
--   - LuaScript, Moveables, Statics, Cameras, Sinks, FlybyCameras, Volumes, 
--   - Rooms, SoundEffects, WadSlots, Enumeration, CompareOperand - specifies argument type and its appearance in UI.
--   - [ENUMDESC1 | ENUMDESC2 | ENUMDESC...] - custom descriptions for "Enumeration" argument type. They will be converted to
--     numerical value on compilation.
--   - Any other string value except listed above - tooltip for a given argument control.


-- Helper function for value comparisons. Any function which uses
-- CompareOperand arguments should use this helper function for comparison.

LevelFuncs.CompareValue = function(value, reference, operand)
	local result = false

	if (operand == 0 and value == reference) then result = true end
	if (operand == 1 and value != reference) then result = true end
	if (operand == 2 and value <  reference) then result = true end
	if (operand == 3 and value <= reference) then result = true end
	if (operand == 4 and value >  reference) then result = true end
	if (operand == 5 and value >= reference) then result = true end
	
	return result
end

-- !Name "Check moveable health"
-- !Description "Compares selected moveable health with given value."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Moveable to check" "NewLine, CompareOperand, 70, Kind of check" "Numerical, 30, Hit points value" 

LevelFuncs.CheckEntityHealth = function(entityName, operand, value)
	local health = TEN.Objects.GetMoveableByName(entityName):GetHP()
	return LevelFuncs.CompareValue(value, health, operand)
end

-- !Name "Flash screen"
-- !Description "Flashes screen with specified color and for specified duration0\nDuration value of 1 takes 1 second to flash."
-- !Arguments "Color, 10" "Numerical, 20" 

LevelFuncs.FlashScreen = function(color, duration)
    Effects.Flash(color, duration)
end

-- !Name "Set hit points"
-- !Description "Set given entity's hitpoints."
-- !Arguments "NewLine, 80, Moveables" "Numerical, 20" 

LevelFuncs.SetHitPoints = function(entityName, value)
	TEN.Objects.GetMoveableByName(entityName):SetHP(value)
end