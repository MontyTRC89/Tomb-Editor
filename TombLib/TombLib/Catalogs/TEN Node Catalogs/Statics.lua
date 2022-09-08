-- !Name "If scale of a static mesh is..."
-- !Section "Static mesh parameters"
-- !Description "Compares selected static mesh scale with given value."
-- !Conditional "True"
-- !Arguments "NewLine, Statics, Static mesh to check" "NewLine, CompareOperand, 70, Kind of check"
-- !Arguments "Numerical, 30, Scale value, [ 0 | 256 | 2 ]" 

LevelFuncs.TestStaticScale = function(staticName, operand, value)
	local scale = TEN.Objects.GetStaticByName(staticName):GetScale()
	return LevelFuncs.CompareValue(scale, value, operand)
end

-- !Name "Enable static mesh"
-- !Section "Static mesh state"
-- !Description "Enables static mesh, e.g. after shattering or manually disabling it."
-- !Arguments "NewLine, Statics"

LevelFuncs.EnableStatic = function(staticName)
    TEN.Objects.GetStaticByName(staticName):Enable()
end

-- !Name "Disable static mesh"
-- !Section "Static mesh state"
-- !Description "Disables static mesh."
-- !Arguments "NewLine, Statics"

LevelFuncs.DisableStatic = function(staticName)
    TEN.Objects.GetStaticByName(staticName):Disable()
end

-- !Name "Play sound near static mesh"
-- !Section "Static mesh state"
-- !Description "Plays specified sound ID around specified static mesh."
-- !Arguments "NewLine, Statics, Static mesh to play sound around" "NewLine, SoundEffects, Sound to play"

LevelFuncs.PlaySoundAroundMoveable = function(staticName, soundID)
    TEN.Misc.PlaySound(soundID, TEN.Objects.GetStaticByName(staticName):GetPosition())
end

-- !Name "Modify position of a static mesh"
-- !Section "Static mesh parameters"
-- !Description "Set given static mesh position."
-- !Arguments "NewLine, Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 75, Position value to define"
-- !Arguments "NewLine, Statics"

LevelFuncs.SetStaticPosition = function(operation, value, staticName)

	local stat = TEN.Objects.GetStaticByName(staticName)

	if (operation == 0) then
		local position = stat:GetPosition();
		position.x = position.x + value.x
		position.y = position.y + value.y
		position.z = position.z + value.z
		stat:SetPosition(position)
	else
		stat:SetPosition(value)
	end
end

-- !Name "Modify rotation of a static mesh"
-- !Section "Static mesh parameters"
-- !Description "Set given static mesh rotation."
-- !Arguments "Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ 0 | 360 | 2 | 1 | 5 ], 15, Rotation value to define", "NewLine, Statics"

LevelFuncs.SetStaticRotation = function(operation, value, staticName)

	local stat = TEN.Objects.GetStaticByName(staticName)
	local rotation = stat:GetRotation();

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

	stat:SetRotation(rotation)
end

-- !Name "Modify scale of a static mesh"
-- !Section "Static mesh parameters"
-- !Description "Set given static mesh scale."
-- !Arguments "Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ 0 | 256 | 2 | 1 | 5 ], 25, Scale value to define", "NewLine, Statics"

LevelFuncs.SetStaticScale = function(operation, value, staticName)

	local stat = TEN.Objects.GetStaticByName(staticName)

	if (operation == 0) then
		local scale = stat:GetScale();
		stat:SetScale(scale + value)
	else
		stat:SetScale(value)
	end
end

-- !Name "Set static mesh colour"
-- !Section "Static mesh parameters"
-- !Description "Sets static mesh tint to a given value."
-- !Arguments "NewLine, Statics, 80" "Color, 20, Static mesh colour" 

LevelFuncs.SetStaticColor = function(staticName, color)
    TEN.Objects.GetStaticByName(staticName):SetColor(color)
end

-- !Name "Shatter static mesh"
-- !Section "Static mesh state"
-- !Description "Shatters static mesh.\nAlso applicable to non-shatterable statics."
-- !Arguments "NewLine, Statics"

LevelFuncs.ShatterStatic = function(staticName)
    TEN.Objects.GetStaticByName(staticName):Shatter()
end