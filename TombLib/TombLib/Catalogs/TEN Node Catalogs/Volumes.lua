-- !Name "If volume is enabled..."
-- !Section "Volumes"
-- !Description "Checks if volume is enabled."
-- !Conditional "True"
-- !Arguments "NewLine, Volumes"

LevelFuncs.Engine.Node.TestVolumeActivity = function(volumeName)
	return TEN.Objects.GetVolumeByName(volumeName):GetActive()
end

-- !Name "If volume contains moveable..."
-- !Section "Volumes"
-- !Description "Checks if volume contains specified moveable."
-- !Conditional "True"
-- !Arguments "NewLine, Volumes"
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.TestVolumeContainsMoveable = function(volumeName, moveableName)
	local vol = TEN.Objects.GetVolumeByName(volumeName)
	local mov = TEN.Objects.GetMoveableByName(moveableName)
	return vol:IsMoveableInside(mov)
end

-- !Name "Enable volume"
-- !Section "Volumes"
-- !Description "Enables volume."
-- !Arguments "NewLine, Volumes"

LevelFuncs.Engine.Node.EnableVolume = function(volumeName)
	TEN.Objects.GetVolumeByName(volumeName):Enable()
end

-- !Name "Disable volume"
-- !Section "Volumes"
-- !Description "Disables volume."
-- !Arguments "NewLine, Volumes"

LevelFuncs.Engine.Node.DisableVolume = function(volumeName)
	TEN.Objects.GetVolumeByName(volumeName):Disable()
end

-- !Name "Clear volume activators"
-- !Section "Volumes"
-- !Description "Clears all current volume activators and make them retrigger all the events."
-- !Arguments "NewLine, Volumes"

LevelFuncs.Engine.Node.ClearVolumeActivators = function(volumeName)
	TEN.Objects.GetVolumeByName(volumeName):ClearActivators()
end

-- !Name "Modify position of a volume"
-- !Section "Volumes"
-- !Description "Set given volume position."
-- !Arguments "NewLine, Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 75, Position value to define"
-- !Arguments "NewLine, Volumes"

LevelFuncs.Engine.Node.SetVolumePosition = function(operation, value, volumeName)
	local vol = TEN.Objects.GetVolumeByName(volumeName)

	if (operation == 0) then
		local position = vol:GetPosition();
		position.x = position.x + value.x
		position.y = position.y + value.y
		position.z = position.z + value.z
		vol:SetPosition(position)
	else
		vol:SetPosition(value)
	end
end

-- !Name "Modify rotation of a volume"
-- !Section "Volumes"
-- !Description "Set given volume rotation."
-- !Arguments "NewLine, Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -360 | 360 | 2 | 1 | 5 ], 25, X axis rotation value to define"
-- !Arguments "Numerical, [ -360 | 360 | 2 | 1 | 5 ], 25, Y axis rotation value to define"
-- !Arguments "Numerical, [ -360 | 360 | 2 | 1 | 5 ], 25, Z axis rotation value to define"
-- !Arguments "NewLine, Volumes"

LevelFuncs.Engine.Node.SetVolumeRotation = function(operation, x, y, z, volumeName)
	local vol = TEN.Objects.GetVolumeByName(volumeName)
	local rotation = vol:GetRotation();

	if (operation == 0) then
		rotation.x = LevelFuncs.Engine.Node.WrapRotation(rotation.x, x)
		rotation.y = LevelFuncs.Engine.Node.WrapRotation(rotation.y, y)
		rotation.z = LevelFuncs.Engine.Node.WrapRotation(rotation.z, z)
	else
		rotation.x = value.x
		rotation.y = value.y
		rotation.z = value.z
	end

	vol:SetRotation(rotation)
end

-- !Name "Modify scale of a volume"
-- !Section "Volumes"
-- !Description "Set given volume scale."
-- !Arguments "NewLine, Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Vector3, [ 0 | 256 | 2 | 1 | 5 ], 75, Scale value to define"
-- !Arguments "NewLine, Volumes"

LevelFuncs.Engine.Node.SetVolumeScale = function(operation, value, volumeName)
	local vol = TEN.Objects.GetVolumeByName(volumeName)

	if (operation == 0) then
		local scale = vol:GetScale();
		scale.x = scale.x + value.x
		scale.y = scale.y + value.y
		scale.z = scale.z + value.z
		vol:SetScale(scale)
	else
		vol:SetScale(value)
	end
end
