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

-- !Name "If health of an object is..."
-- !Description "Compares selected moveable health with given value."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Moveable to check" "NewLine, CompareOperand, 70, Kind of check"
-- !Arguments "Numerical, 30, Hit points value, [ 0 | 3000 ]" 

LevelFuncs.TestHitPoints = function(moveableName, operand, value)
	local health = TEN.Objects.GetMoveableByName(moveableName):GetHP()
	return LevelFuncs.CompareValue(health, value, operand)
end

-- !Name "If object ID is..."
-- !Description "Checks if moveable belongs to a certain slot ID."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Moveable to check" "NewLine, WadSlots, Object ID to compare to"

LevelFuncs.TestMoveableId = function(moveableName, objectId)
	return TEN.Objects.GetMoveableByName(moveableName):GetObjectID() == objectId
end

-- !Name "Flash screen"
-- !Description "Flashes screen with specified color and for specified duration.\nDuration value of 1 takes 1 second to flash."
-- !Arguments "Color, 10, Flash colour" "Numerical, 20, Flash speed" 

LevelFuncs.FlashScreen = function(color, duration)
    Effects.FlashScreen(color, duration)
end

-- !Name "Play sound near moveable"
-- !Description "Plays specified sound ID around specified moveable."
-- !Arguments "NewLine, Moveables, Moveable to play sound around" "NewLine, SoundEffects, Sound to play"

LevelFuncs.PlaySoundAroundMoveable = function(moveableName, soundID)
    Misc.PlaySound(soundID, TEN.Objects.GetMoveableByName(moveableName):GetPosition())
end

-- !Name "Modify health of an object"
-- !Description "Set given entity's hitpoints."
-- !Arguments "Enumeration, [ Change | Set ], 30, Change adds/subtracts given value, while Set forces it."
-- !Arguments "Numerical, [ -1000 | 1000 ], 15, Health value to define", "NewLine, Moveables"

LevelFuncs.SetHitPoints = function(operation, value, moveableName)

	if (operation == 0) then
		local moveable = TEN.Objects.GetMoveableByName(moveableName)
		moveable:SetHP(moveable:GetHP() + value)
	else
		TEN.Objects.GetMoveableByName(moveableName):SetHP(value)
	end
end

-- !Name "Set object colour"
-- !Description "Sets moveable tint to a given value."
-- !Arguments "NewLine, Moveables, 80" "Color, 20, Moveable colour" 

LevelFuncs.SetColor = function(moveableName, color)
    TEN.Objects.GetMoveableByName(moveableName):SetColor(color)
end

-- !Name "Shatter object"
-- !Description "Shatters moveable in similar way to shatterable statics."
-- !Arguments "NewLine, Moveables"

LevelFuncs.ShatterMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Shatter()
end

-- !Name "Explode object"
-- !Description "Explodes moveable."
-- !Arguments "NewLine, Moveables"

LevelFuncs.ExplodeMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Explode()
end