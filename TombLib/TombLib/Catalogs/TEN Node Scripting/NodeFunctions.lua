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
--   - [ENUMDESC1 | ENUMDESC2 | ENUMDESC...] - custom descriptions for "Enumeration" argument type or range for numerical type.
--     For numerical value, first and second ENUMDESC parameters determine min/max UI range of value. 
--     For moveable list, ENUMDESC parameters will filter out object ID names which contain any of ENUMDESCs only.
--     For enumeration, ENUMDESC values will be converted to numericals on compilation in order of appearance.
--   - Any other string value except listed above - tooltip for a given argument control.

-- Helper function for value comparisons. Any function which uses
-- CompareOperand arguments should use this helper function for comparison.

LevelFuncs.CompareValue = function(value, reference, operand)
	local result = false

	if (operand == 0 and value == reference) then result = true end
	if (operand == 1 and value ~= reference) then result = true end
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
	return LevelFuncs.CompareValue(health, value, operand)
end

-- !Name "Flash screen"
-- !Description "Flashes screen with specified color and for specified duration.\nDuration value of 1 takes 1 second to flash."
-- !Arguments "Color, 10, Flash colour" "Numerical, 20, Flash speed" 

LevelFuncs.FlashScreen = function(color, duration)
    Effects.FlashScreen(color, duration)
end

-- !Name "Modify health points"
-- !Description "Set given entity's hitpoints."
-- !Arguments "Enumeration, [ Change | Set ], 30, Change adds/subtracts given value, while Set forces it."
-- !Arguments "Numerical, [ -1000 | 1000 ], 15, Health value to define", "NewLine, Moveables"

LevelFuncs.SetHitPoints = function(operation, value, entityName)

	if (operation == 0) then
		local moveable = TEN.Objects.GetMoveableByName(entityName)
		moveable:SetHP(moveable:GetHP() + value)
	else
		TEN.Objects.GetMoveableByName(entityName):SetHP(value)
	end
end

-- !Name "Set moveable colour"
-- !Description "Sets moveable tint to a given value."
-- !Arguments "NewLine, Moveables, 80" "Color, 20, Moveable colour" 

LevelFuncs.SetColor = function(moveableName, color)
    TEN.Objects.GetMoveableByName(moveableName):SetColor(color)
end