-- !Ignore
-- Construct timed transform data and start transform
LevelFuncs.Engine.Node.ConstructTimedData = function(objectName, isStatic, dataType, newValue, time, smooth)

	local prefix = nil
	local value  = nil
	local object = isStatic and TEN.Objects.GetStaticByName(objectName) or TEN.Objects.GetMoveableByName(objectName)
	
	if (dataType == 0) then 
		prefix = "_translation"
		value = object:GetPosition()
	elseif (dataType == 1) then
		prefix = "_rotation"
		value = object:GetRotation()
	elseif (dataType == 2) then
		prefix = "_scale"
		value = object:GetScale()
	elseif (dataType == 3) then
		prefix = "_color"
		value = object:GetColor()
	end

	local dataName  = objectName .. prefix .. "_transform_data"
	
	if (LevelVars[dataName] ~= nil and LevelVars[dataName].Timer ~= nil) then
		if (LevelVars[dataName].Timer:IsActive()) then
			return
		else
			Timer.Delete(LevelVars[dataName].Name)
			LevelVars[dataName] = nil
		end
	end

	LevelVars[dataName] = {}
	
	LevelVars[dataName].Progress   = 0
	LevelVars[dataName].Interval   = 1 / (time * 30)
	LevelVars[dataName].Smooth     = smooth
	LevelVars[dataName].DataType   = dataType
	LevelVars[dataName].IsStatic   = isStatic
	LevelVars[dataName].ObjectName = objectName
	LevelVars[dataName].Name       = dataName
	LevelVars[dataName].Timer      = Timer.Create(dataName, 1 / 30, true, false, LevelFuncs.Engine.Node.TransformTimedData, dataName)
	LevelVars[dataName].NewValue   = newValue
	LevelVars[dataName].OldValue   = value

	LevelVars[dataName].Timer:Start()
end

-- !Ignore
-- Transform object parameter using previously saved timed transform data
LevelFuncs.Engine.Node.TransformTimedData = function(dataName)

	LevelVars[dataName].Progress = math.min(LevelVars[dataName].Progress + LevelVars[dataName].Interval, 1)
	local factor = LevelVars[dataName].Smooth and LevelFuncs.Engine.Node.Smoothstep(LevelVars[dataName].Progress) or LevelVars[dataName].Progress
	
	local newValue1
	local newValue2
	local newValue3
	local newValue4
	
	if (LevelVars[dataName].DataType == 2) then
		newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue, LevelVars[dataName].NewValue, factor)
	elseif (LevelVars[dataName].DataType == 3) then
		newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue.r, LevelVars[dataName].NewValue.r, factor)
		newValue2 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue.g, LevelVars[dataName].NewValue.g, factor)
	    newValue3 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue.b, LevelVars[dataName].NewValue.b, factor)
	    newValue4 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue.a, LevelVars[dataName].NewValue.a, factor)
	else
		newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue.x, LevelVars[dataName].NewValue.x, factor)
		newValue2 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue.y, LevelVars[dataName].NewValue.y, factor)
	    newValue3 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue.z, LevelVars[dataName].NewValue.z, factor)
	end
	
	local object = LevelVars[dataName].IsStatic and TEN.Objects.GetStaticByName(LevelVars[dataName].ObjectName) or TEN.Objects.GetMoveableByName(LevelVars[dataName].ObjectName)

	if (LevelVars[dataName].DataType == 0) then
		object:SetPosition(Vec3(newValue1, newValue2, newValue3))
	elseif (LevelVars[dataName].DataType == 1) then
		object:SetRotation(Rotation(newValue1, newValue2, newValue3))
	elseif (LevelVars[dataName].DataType == 2) then
		object:SetScale(newValue1)
	elseif (LevelVars[dataName].DataType == 3) then
		object:SetColor(Color(newValue1, newValue2, newValue3, newValue4))
	end
		
	if (LevelVars[dataName].Progress >= 1) then
		Timer.Delete(LevelVars[dataName].Name)
		LevelVars[dataName] = nil
	end

end

-- !Name "Transform position of a moveable"
-- !Section "Timespan actions"
-- !Description "Gradually change position of a moveable over specified timespan."
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 65, New position value to define"
-- !Arguments "35, Boolean, Relative coordinates"
-- !Arguments "NewLine, Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 65, Time (in seconds)" 
-- !Arguments "35, Boolean, Smooth motion"

LevelFuncs.Engine.Node.SetMoveablePositionOverTimespan = function(moveableName, newPosition, relative, time, smooth)
	if (relative) then
		newPosition = TEN.Objects.GetMoveableByName(moveableName):GetPosition() + newPosition
	end
	
	-- Wrap another node function call into do/end to prevent wrong parsing
	do LevelFuncs.Engine.Node.ConstructTimedData(moveableName, false, 0, newPosition, time, smooth) end
end

-- !Name "Transform rotation of a moveable"
-- !Section "Timespan actions"
-- !Description "Gradually change rotation of a moveable over specified timespan."
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, Numerical, [ -360 | 360 | 2 | 1 | 5 ], 20, Rotation value to define" "30, Boolean, Relative angle"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 20, Time (in seconds)"  "30, Boolean, Smooth motion"

LevelFuncs.Engine.Node.SetMoveableRotationOverTimespan = function(moveableName, newRotation, relative, time, smooth)
	local fullNewRotation = TEN.Objects.GetMoveableByName(moveableName):GetRotation()

	if (relative) then
		fullNewRotation.y = fullNewRotation.y + newRotation
	else
		fullNewRotation.y = newRotation
	end
	
	-- Wrap another node function call into do/end to prevent wrong parsing
	do LevelFuncs.Engine.Node.ConstructTimedData(moveableName, false, 1, fullNewRotation, time, smooth) end
end

-- !Name "Transform colour of a moveable"
-- !Section "Timespan actions"
-- !Description "Gradually change colour of a moveable over specified timespan."
-- !Arguments "NewLine, Moveables, 70" "Color, 15, Moveable colour"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 15, Time (in seconds)"

LevelFuncs.Engine.Node.SetMoveableColourOverTimespan = function(moveableName, newColour, time)
	do LevelFuncs.Engine.Node.ConstructTimedData(moveableName, false, 3, newColour, time, true) end
end

-- !Name "Transform position of a static mesh"
-- !Section "Timespan actions"
-- !Description "Gradually change position of a static mesh over specified timespan."
-- !Arguments "NewLine, Statics"
-- !Arguments "NewLine, Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 65, New position value to define"
-- !Arguments "35, Boolean, Relative coordinates"
-- !Arguments "NewLine, Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 65, Time (in seconds)" 
-- !Arguments "35, Boolean, Smooth motion"

LevelFuncs.Engine.Node.SetStaticPositionOverTimespan = function(staticName, newPosition, relative, time, smooth)
	if (relative) then
		newPosition = TEN.Objects.GetStaticByName(staticName):GetPosition() + newPosition
	end
	
	-- Wrap another node function call into do/end to prevent wrong parsing
	do LevelFuncs.Engine.Node.ConstructTimedData(staticName, true, 0, newPosition, time, smooth) end
end

-- !Name "Transform rotation of a static mesh"
-- !Section "Timespan actions"
-- !Description "Gradually change rotation of a static mesh over specified timespan."
-- !Arguments "NewLine, Statics"
-- !Arguments "NewLine, Numerical, [ -360 | 360 | 2 | 1 | 5 ], 20, Rotation value to define" "30, Boolean, Relative angle"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 20, Time (in seconds)"  "30, Boolean, Smooth motion"

LevelFuncs.Engine.Node.SetStaticRotationOverTimespan = function(staticName, newRotation, relative, time, smooth)
	local fullNewRotation = TEN.Objects.GetStaticByName(staticName):GetRotation()

	if (relative) then
		fullNewRotation.y = fullNewRotation.y + newRotation
	else
		fullNewRotation.y = newRotation
	end
	
	-- Wrap another node function call into do/end to prevent wrong parsing
	do LevelFuncs.Engine.Node.ConstructTimedData(staticName, true, 1, fullNewRotation, time, smooth) end
end

-- !Name "Transform scale of a static mesh"
-- !Section "Timespan actions"
-- !Description "Gradually change scale of a static mesh over specified timespan."
-- !Arguments "NewLine, Statics"
-- !Arguments "NewLine, Numerical, [ 0 | 256 | 2 | 1 | 5 ], 20, {1}, Scale value to define" "30, Boolean, Relative scale"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 20, Time (in seconds)"  "30, Boolean, Smooth motion"

LevelFuncs.Engine.Node.SetStaticScaleOverTimespan = function(staticName, newScale, relative, time, smooth)

	if (relative) then
		newScale = TEN.Objects.GetStaticByName(staticName):GetScale() + newScale
	end

	-- Wrap another node function call into do/end to prevent wrong parsing
	do LevelFuncs.Engine.Node.ConstructTimedData(staticName, true, 2, newScale, time, smooth) end
end

-- !Name "Transform colour of a static mesh"
-- !Section "Timespan actions"
-- !Description "Gradually change colour of a static mesh over specified timespan."
-- !Arguments "NewLine, Statics, 70" "Color, 15, Static mesh colour"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 15, Time (in seconds)"

LevelFuncs.Engine.Node.SetStaticColourOverTimespan = function(staticName, newColour, time)
	do LevelFuncs.Engine.Node.ConstructTimedData(staticName, true, 3, newColour, time, true) end
end