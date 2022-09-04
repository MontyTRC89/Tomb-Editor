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