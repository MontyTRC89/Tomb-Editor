-- !Name "If moveable is active..."
-- !Section "Moveable state"
-- !Description "Checks if moveable is active."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.TestMoveableActivity = function(moveableName)
	return TEN.Objects.GetMoveableByName(moveableName):GetActive()
end

-- !Name "If health of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Compares selected moveable health with given value."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Object to check" "NewLine, CompareOperator, 70, Kind of check"
-- !Arguments "Numerical, 30, Hit points value, [ 0 | 3000 | 0 | 1 | 5 ]" 

LevelFuncs.Engine.Node.TestHitPoints = function(moveableName, operator, value)
	local health = TEN.Objects.GetMoveableByName(moveableName):GetHP()
	return LevelFuncs.Engine.Node.CompareValue(health, value, operator)
end

-- !Name "If ID of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable belongs to a certain slot ID."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Object to check" "NewLine, WadSlots, Object ID to compare to"

LevelFuncs.Engine.Node.TestMoveableId = function(moveableName, objectId)
	return TEN.Objects.GetMoveableByName(moveableName):GetObjectID() == objectId
end

-- !Name "If name of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable's name is the one specified."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, Object to check"
-- !Arguments "NewLine, String, Object name to compare to"

LevelFuncs.Engine.Node.TestMoveableName = function(moveableName, name)
	return TEN.Objects.GetMoveableByName(moveableName):GetName() == name
end

-- !Name "If animation of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable is currently playing specified animation number."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, 80" "Numerical, 20, [ 0 | 1000 ], Animation ID"

LevelFuncs.Engine.Node.TestMoveableAnimation = function(moveableName, animationId)
	return TEN.Objects.GetMoveableByName(moveableName):GetAnim() == animationId
end

-- !Name "If state of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable's current state is the one specified."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, 80" "Numerical, 20, [ 0 | 1000 ], State ID"

LevelFuncs.Engine.Node.TestMoveableCurrentState = function(moveableName, stateId)
	return TEN.Objects.GetMoveableByName(moveableName):GetState() == stateId
end

-- !Name "If position of a moveable is within range..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable's current position is within specified range."
-- !Description "If single-dimension check is needed, set other dimensions to values well out of level bounds."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, Vector3, [ -1000000 | 1000000 ], Upper position bound" "NewLine, Vector3, [ -1000000 | 1000000 ], Lower position bound"

LevelFuncs.Engine.Node.TestMoveablePosition = function(moveableName, pos1, pos2)
	local pos = TEN.Objects.GetMoveableByName(moveableName):GetPosition()
	return (pos.x >= pos1.x and pos.x <= pos2.x and
			pos.y >= pos1.y and pos.y <= pos2.y and
			pos.z >= pos1.z and pos.z <= pos2.z)
end

-- !Name "If rotation of a moveable is within range..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable's current rotation is within specified range."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, 70"
-- !Arguments "Numerical, 15, [ 0 | 359 ], In range (in degrees)" "Numerical, 15, [ 0 | 359 ], Out range (in degrees)"

LevelFuncs.Engine.Node.TestMoveableRotation = function(moveableName, rot1, rot2)
	local rot = TEN.Objects.GetMoveableByName(moveableName):GetRotation().y
	return (rot >= rot1 and rot <= rot2)
end

-- !Name "If speed of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks moveable's current speed."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, 100"
-- !Arguments "NewLine, CompareOperator, 30, Kind of check" "Numerical, 20, [ 0 | 1000 ], Speed value (per frame)"
-- !Arguments "Enumeration, 50, [ Horizontal | Vertical | Combined ], Speed direction to compare"

LevelFuncs.Engine.Node.TestMoveableSpeed = function(moveableName, operator, value, type)
	local spd = TEN.Objects.GetMoveableByName(moveableName):GetVelocity()
	
	if (type == 0) then
		return LevelFuncs.Engine.Node.CompareValue(spd.z, value, operator)
	elseif (type == 1) then
		return LevelFuncs.Engine.Node.CompareValue(spd.y, value, operator)
	else
		local length = math.sqrt(spd.x * spd.x + spd.y * spd.y + spd.z * spd.z)
		return LevelFuncs.Engine.Node.CompareValue(length, value, operator)
	end
end

-- !Name "If OCB of a moveable is..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable's current OCB is equal to specified."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, 70"
-- !Arguments "Numerical, 30, [ -65536 | 65535 ], OCB value"

LevelFuncs.Engine.Node.TestMoveableOCB = function(moveableName, value)
	local ocb = TEN.Objects.GetMoveableByName(moveableName):GetOCB()
	return (ocb == value)
end

-- !Name "If mesh number of a moveable is visible..."
-- !Section "Moveable parameters"
-- !Description "Checks if moveable's mesh index is visible."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables, 70"
-- !Arguments "Numerical, 30, [ 0 | 31 ], Mesh index to check"

LevelFuncs.Engine.Node.TestMoveableMeshVisibility = function(moveableName, value)
	return TEN.Objects.GetMoveableByName(moveableName):MeshIsVisible(value)
end

-- !Name "If moveable is on the line of sight..."
-- !Section "Moveable state"
-- !Description "Checks if one moveable is on the line of sight with another moveable."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.TestMoveableLOS = function(moveableName1, moveableName2)
	local mov1 = TEN.Objects.GetMoveableByName(moveableName1)
	local mov2 = TEN.Objects.GetMoveableByName(moveableName2)
	return TEN.Misc.HasLineOfSight(mov1:GetRoom(), mov1:GetPosition(), mov2:GetPosition())
end

-- !Name "If distance between moveables is..."
-- !Section "Moveable state"
-- !Description "Checks if the distance between moveables complies to specified."
-- !Conditional "True"
-- !Arguments "NewLine, CompareOperator, 30, Kind of check" "Numerical, 20, [ 0 | 1000000 ], Distance value to compare"
-- !Arguments "Enumeration, 50, [ All dimensions | Horizontal only ], Distance type to compare"
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.TestMoveableDistance = function(operator, value, type, moveableName1, moveableName2)
	local mov1 = TEN.Objects.GetMoveableByName(moveableName1)
	local mov2 = TEN.Objects.GetMoveableByName(moveableName2)

	local distance = 0
	if (type == 0) then
		distance = TEN.Misc.CalculateDistance(mov1:GetPosition(), mov2:GetPosition())
	else
		distance = TEN.Misc.CalculateHorizontalDistance(mov1:GetPosition(), mov2:GetPosition())
	end

	return LevelFuncs.Engine.Node.CompareValue(distance, value, operator)
end

-- !Name "If distance between static and moveable is..."
-- !Section "Moveable state"
-- !Description "Checks if the distance between moveable and static mesh complies to specified."
-- !Conditional "True"
-- !Arguments "NewLine, CompareOperator, 30, Kind of check" "Numerical, 20, [ 0 | 1000000 ], Distance value to compare"
-- !Arguments "Enumeration, 50, [ All dimensions | Horizontal only ], Distance type to compare"
-- !Arguments "NewLine, Statics"
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.TestMoveableDistanceWithStatic = function(operator, value, type, staticName, moveableName)
	local stat = TEN.Objects.GetStaticByName(staticName)
	local mov  = TEN.Objects.GetMoveableByName(moveableName)

	local distance = 0
	if (type == 0) then
		distance = TEN.Misc.CalculateDistance(mov:GetPosition(), stat:GetPosition())
	else
		distance = TEN.Misc.CalculateHorizontalDistance(mov:GetPosition(), stat:GetPosition())
	end

	return LevelFuncs.Engine.Node.CompareValue(distance, value, operator)
end

-- !Name "Enable moveable"
-- !Section "Moveable state"
-- !Description "Enables moveable."
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.EnableMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Enable()
end

-- !Name "Disable moveable"
-- !Section "Moveable state"
-- !Description "Disables moveable."
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.DisableMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Disable()
end

-- !Name "Set moveable's animation"
-- !Section "Moveable parameters"
-- !Description "Sets moveable's animation."
-- !Arguments "NewLine, Moveables, 80" "Numerical, 20, [ 0 | 1000 ], Animation ID"

LevelFuncs.Engine.Node.SetMoveableAnimation = function(moveableName, animationId)
    TEN.Objects.GetMoveableByName(moveableName):SetAnim(animationId)
end

-- !Name "Set moveable's state"
-- !Section "Moveable parameters"
-- !Description "Sets moveable's next state."
-- !Arguments "NewLine, Moveables, 80" "Numerical, 20, [ 0 | 1000 ], State ID"

LevelFuncs.Engine.Node.SetMoveableState = function(moveableName, stateId)
    TEN.Objects.GetMoveableByName(moveableName):SetState(stateId)
end

-- !Name "Shatter moveable"
-- !Section "Moveable state"
-- !Description "Shatters object in similar way to shatterable statics."
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.ShatterMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Shatter()
end

-- !Name "Shatter specified moveable mesh"
-- !Section "Moveable state"
-- !Description "Shatters specified moveable mesh."
-- !Arguments "NewLine, Moveables, 85"
-- !Arguments "Numerical, 15, [ 0 | 31 ], Mesh index to shatter"

LevelFuncs.Engine.Node.ShatterMoveableMesh = function(moveableName, value)
	TEN.Objects.GetMoveableByName(moveableName):ShatterMesh(value)
end

-- !Name "Explode moveable"
-- !Section "Moveable state"
-- !Description "Explodes object."
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.ExplodeMoveable = function(moveableName)
    TEN.Objects.GetMoveableByName(moveableName):Explode()
end

-- !Name "Play sound near moveable"
-- !Section "Moveable state"
-- !Description "Plays specified sound ID around specified moveable."
-- !Arguments "NewLine, Moveables, Moveable to play sound around" "NewLine, SoundEffects, Sound to play"

LevelFuncs.Engine.Node.PlaySoundAroundMoveable = function(moveableName, soundID)
    TEN.Misc.PlaySound(soundID, TEN.Objects.GetMoveableByName(moveableName):GetPosition())
end

-- !Name "Modify health of a moveable"
-- !Section "Moveable parameters"
-- !Description "Set given moveable health."
-- !Arguments "Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -1000 | 1000 | 0 | 1 | 5 ], 15, Health value to define", "NewLine, Moveables"

LevelFuncs.Engine.Node.SetMoveableHitPoints = function(operation, value, moveableName)

	if (operation == 0) then
		local moveable = TEN.Objects.GetMoveableByName(moveableName)
		moveable:SetHP(moveable:GetHP() + value)
	else
		TEN.Objects.GetMoveableByName(moveableName):SetHP(value)
	end
end

-- !Name "Modify OCB of a moveable"
-- !Section "Moveable parameters"
-- !Description "Set given moveable OCB."
-- !Arguments "Numerical, [ -65536 | 65535 | 0 | 1 | 5 ], 20, OCB to define", "NewLine, Moveables"

LevelFuncs.Engine.Node.SetMoveableOCB = function(value, moveableName)
	TEN.Objects.GetMoveableByName(moveableName):SetOCB(value)
end

-- !Name "Modify position of a moveable"
-- !Section "Moveable parameters"
-- !Description "Set given moveable position."
-- !Arguments "NewLine, Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 75, Position value to define"
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.SetMoveablePosition = function(operation, value, moveableName)

	local moveable = TEN.Objects.GetMoveableByName(moveableName)

	if (operation == 0) then
		local position = moveable:GetPosition();
		position.x = position.x + value.x
		position.y = position.y + value.y
		position.z = position.z + value.z
		moveable:SetPosition(position)
	else
		moveable:SetPosition(value)
	end
end

-- !Name "Modify rotation of a moveable"
-- !Section "Moveable parameters"
-- !Description "Set given moveable rotation."
-- !Arguments "Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ 0 | 360 | 2 | 1 | 5 ], 15, Rotation value to define", "NewLine, Moveables"

LevelFuncs.Engine.Node.SetMoveableRotation = function(operation, value, moveableName)

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

-- !Name "Set moveable colour"
-- !Section "Moveable parameters"
-- !Description "Sets moveable tint to a given value."
-- !Arguments "NewLine, Moveables, 80" "Color, 20, Moveable colour" 

LevelFuncs.Engine.Node.SetMoveableColor = function(moveableName, color)
    TEN.Objects.GetMoveableByName(moveableName):SetColor(color)
end

-- !Name "Set specified moveable mesh visibility"
-- !Section "Moveable parameters"
-- !Description "Sets specified moveable mesh visibility."
-- !Arguments "NewLine, Moveables, 70"
-- !Arguments "Numerical, 15, [ 0 | 31 ], Mesh index to check" "Boolean, 15, Visible"

LevelFuncs.Engine.Node.SetMoveableMeshVisibility = function(moveableName, value, state)
	if (state == true) then
		TEN.Objects.GetMoveableByName(moveableName):ShowMesh(value)
	else
		TEN.Objects.GetMoveableByName(moveableName):HideMesh(value)
	end
end

-- !Name "Swap specified moveable mesh with another"
-- !Section "Moveable parameters"
-- !Description "Swaps specified moveable mesh with another moveable mesh."
-- !Arguments "NewLine, 80, Moveables, Destination moveable" "Numerical, 20, [ 0 | 31 ], Mesh index to swap"
-- !Arguments "NewLine, 80, WadSlots, Source slot" "Numerical, 20, [ 0 | 31 ], Mesh index to use"

LevelFuncs.Engine.Node.SwapMoveableMesh = function(dest, destIndex, srcSlot, srcIndex)
	
	TEN.Objects.GetMoveableByName(dest):SwapMesh(destIndex, srcSlot, srcIndex)
end

-- !Name "If moveable has effect..."
-- !Section "Moveable state"
-- !Conditional "True"
-- !Description "Checks if moveable currently has an effect attached."
-- !Arguments "Enumeration, 40, [ Fire | Sparks | Smoke | Electric ignite | Red ignite | Custom ], Effect type to compare"
-- !Arguments "NewLine, Moveables, Moveable to check"

LevelFuncs.Engine.Node.TestMoveableEffect = function(effectID, moveableName)

	local effect = TEN.Objects.GetMoveableByName(moveableName):GetEffect()

	if (effect == 0) then
		return false
	else
		return effectID == effect - 1
	end
end

-- !Name "Set moveable effect"
-- !Section "Moveable state"
-- !Description "Assigns specific effect to a moveable."
-- !Arguments "Enumeration, 30, [ Fire | Sparks | Smoke | Electric ignite | Red ignite ], Effect type to set"
-- !Arguments "Numerical, 13, [ -1 | 99 ], Effect timeout (set to -1 for indefinite timeout)"
-- !Arguments "NewLine, Moveables, Moveable to check"


LevelFuncs.Engine.Node.SetMoveableEffect = function(effectID, timeout, moveableName)
    TEN.Objects.GetMoveableByName(moveableName):SetEffect(effectID + 1, timeout)
end

-- !Name "Set custom moveable effect"
-- !Section "Moveable state"
-- !Description "Assigns custom colored burn effect to a moveable."
-- !Arguments "Color, 10, Effect primary colour" 
-- !Arguments "Color, 10, Effect secondary colour" 
-- !Arguments "Numerical, 13, [ -1 | 99 ], Effect timeout (set to -1 for indefinite timeout)"
-- !Arguments "NewLine, Moveables, Moveable to check"


LevelFuncs.Engine.Node.SetCustomMoveableEffect = function(primary, secondary, timeout, moveableName)
    TEN.Objects.GetMoveableByName(moveableName):SetCustomEffect(primary, secondary, timeout)
end