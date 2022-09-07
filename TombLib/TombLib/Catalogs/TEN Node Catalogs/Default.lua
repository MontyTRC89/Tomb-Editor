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

-- !Name "If health of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Compares selected moveable health with given value."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Object to check" "NewLine, CompareOperand, 70, Kind of check"
-- !Arguments "Numerical, 30, Hit points value, [ 0 | 3000 | 0 | 1 | 5 ]" 

LevelFuncs.TestHitPoints = function(moveableName, operand, value)
	local health = TEN.Objects.GetMoveableByName(moveableName):GetHP()
	return LevelFuncs.CompareValue(health, value, operand)
end

-- !Name "If ID of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable belongs to a certain slot ID."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Object to check" "NewLine, WadSlots, Object ID to compare to"

LevelFuncs.TestMoveableId = function(moveableName, objectId)
	return TEN.Objects.GetMoveableByName(moveableName):GetObjectID() == objectId
end

-- !Name "If name of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable's name is the one specified."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Object to check"
-- !Arguments "NewLine, String, Object name to compare to"

LevelFuncs.TestMoveableName = function(moveableName, name)
	return TEN.Objects.GetMoveableByName(moveableName):GetName() == name
end

-- !Name "If animation of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable is currently playing specified animation number."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, 80"  "Numerical, 20, [ 0 | 1000 | 0 ], Animation ID"

LevelFuncs.TestMoveableAnimation = function(moveableName, animationId)
	return TEN.Objects.GetMoveableByName(moveableName):GetAnim() == animationId
end

-- !Name "If state of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable's current state is the one specified."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, 80"  "Numerical, 20, [ 0 | 1000 | 0 ], State ID"

LevelFuncs.TestMoveableCurrentState = function(moveableName, stateId)
	return TEN.Objects.GetMoveableByName(moveableName):GetState() == stateId
end

-- !Name "Flash screen"
-- !Section "Effects"
-- !Description "Flashes screen with specified color and for specified duration.\nDuration value of 1 takes 1 second to flash."
-- !Arguments "Color, 10, Flash colour" "Numerical, 20, [ 0.1 | 10 | 2 | 0.1 | 0.5 ], Flash speed" 

LevelFuncs.FlashScreen = function(color, duration)
    Effects.FlashScreen(color, duration)
end

-- !Name "Enable moveable"
-- !Section "Moveable state"
-- !Description "Enables moveable."
-- !Arguments "NewLine, Moveables"

LevelFuncs.EnableMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Enable()
end

-- !Name "Disable moveable"
-- !Section "Moveable state"
-- !Description "Disables moveable."
-- !Arguments "NewLine, Moveables"

LevelFuncs.DisableMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Disable()
end

-- !Name "Set moveable's animation"
-- !Section "Moveable parameters"
-- !Description "Sets moveable's animation."
-- !Arguments "NewLine, Moveables, 80"  "Numerical, 20, [ 0 | 1000 | 0 ], Animation ID"

LevelFuncs.SetMoveableAnimation = function(moveableName, animationId)
    TEN.Objects.GetMoveableByName(moveableName):SetAnim(animationId)
end

-- !Name "Set moveable's state"
-- !Section "Moveable parameters"
-- !Description "Sets moveable's next state."
-- !Arguments "NewLine, Moveables, 80"  "Numerical, 20, [ 0 | 1000 | 0 ], State ID"

LevelFuncs.SetMoveableState = function(moveableName, stateId)
    TEN.Objects.GetMoveableByName(moveableName):SetState(stateId)
end

-- !Name "Shatter moveable"
-- !Section "Moveable state"
-- !Description "Shatters object in similar way to shatterable statics."
-- !Arguments "NewLine, Moveables"

LevelFuncs.ShatterMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Shatter()
end

-- !Name "Explode moveable"
-- !Section "Moveable state"
-- !Description "Explodes object."
-- !Arguments "NewLine, Moveables"

LevelFuncs.ExplodeMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Explode()
end

-- !Name "Play sound near moveable"
-- !Section "Effects"
-- !Description "Plays specified sound ID around specified object."
-- !Arguments "NewLine, Moveables, Moveable to play sound around" "NewLine, SoundEffects, Sound to play"

LevelFuncs.PlaySoundAroundMoveable = function(moveableName, soundID)
    Misc.PlaySound(soundID, TEN.Objects.GetMoveableByName(moveableName):GetPosition())
end

-- !Name "Modify health of a moveable"
-- !Section "Moveable parameters"
-- !Description "Set given moveable health."
-- !Arguments "Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -1000 | 1000 | 0 | 1 | 5 ], 15, Health value to define", "NewLine, Moveables"

LevelFuncs.SetHitPoints = function(operation, value, moveableName)

	if (operation == 0) then
		local moveable = TEN.Objects.GetMoveableByName(moveableName)
		moveable:SetHP(moveable:GetHP() + value)
	else
		TEN.Objects.GetMoveableByName(moveableName):SetHP(value)
	end
end

-- !Name "Modify rotation of a moveable"
-- !Section "Moveable parameters"
-- !Description "Set given moveable rotation."
-- !Arguments "Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ 0 | 360 | 2 | 1 | 5 ], 15, Rotation value to define", "NewLine, Moveables"

LevelFuncs.SetRotation = function(operation, value, moveableName)

	local moveable = TEN.Objects.GetMoveableByName(moveableName)
	local rotation = moveable:GetRotation();

	if (operation == 0) then
		local rot = rotation.y + value
		if (rot > 360) then
			rotation.y = rot - 360
		elseif (rot < 0) then
			rotation.y = 360 + rot
		else
			rotation.y = rotation.y + value
		end
	else
		rotation.y = value
	end

	moveable:SetRotation(rotation)
end

-- !Name "Set object colour"
-- !Section "Moveable parameters"
-- !Description "Sets moveable tint to a given value."
-- !Arguments "NewLine, Moveables, 80" "Color, 20, Moveable colour" 

LevelFuncs.SetColor = function(moveableName, color)
    TEN.Objects.GetMoveableByName(moveableName):SetColor(color)
end

-- !Name "Play audio track"
-- !Section "Effects"
-- !Description "Plays specified audio track."
-- !Arguments "NewLine, String, Name of the audiotrack to play"
-- !Arguments "NewLine, Enumeration, [ Play once | Play in looped mode ], Sets whether the track should loop or not"

LevelFuncs.PlayAudioTrack = function(name, looped)
	if (looped == 1) then
		TEN.Misc.PlayAudioTrack(moveableName, true)
	else
		TEN.Misc.PlayAudioTrack(moveableName, false)
	end
end

-- !Name "If key is hit..."
-- !Section "Controls"
-- !Description "Checks if specific game key has just been hit (will be true only for 1 frame, until the key is released and hit again)."
-- !Conditional "True"
-- !Arguments "NewLine, Enumeration, [ Forward | Back | Left | Right | Crouch | Sprint | Walk | Jump | Action | Draw | Flare | Look | Roll | Inventory | Pause | Step Left | Step Right ]"

LevelFuncs.KeyIsHit = function(keyCode)
	return TEN.Misc.KeyIsHit(keyCode)
end

-- !Name "If key is held..."
-- !Section "Controls"
-- !Description "Checks if specific game key is being held (will be true as long as the player keeps their finger on that key)."
-- !Conditional "True"
-- !Arguments "NewLine, Enumeration, [ Forward | Back | Left | Right | Crouch | Sprint | Walk | Jump | Action | Draw | Flare | Look | Roll | Inventory | Pause | Step Left | Step Right ]"

LevelFuncs.KeyIsHeld = function(keyCode)
	return TEN.Misc.KeyIsHeld(keyCode)
end