--Revolution Nodes by TrainWreck; Code references by Lwmte, Adngel. Shoutout to DaviDMRR, RichardTRLE and Stranger1992 for the help.

local Timer = require("Engine.Timer")

-- Construct timed transform data and start transform
LevelFuncs.Engine.Node.ConstructTimedData = function(objectName, objectcentre, dataType, radius, time, startangle, endangle, isLoop, isCCW, isSmooth, isRotate, isCRotate, SFX)

    local dataName  = objectName .. "_revolve_data"
    
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
    LevelVars[dataName].DataType   = dataType
    LevelVars[dataName].Radius     = radius
    LevelVars[dataName].ObjectName = objectName
    LevelVars[dataName].CentreName = objectcentre
    LevelVars[dataName].Name       = dataName
    LevelVars[dataName].Timer      = Timer.Create(dataName, 1 / 30, true, false, LevelFuncs.Engine.Node.TransformTimedData, dataName)
    LevelVars[dataName].StartAngle = startangle
    LevelVars[dataName].EndAngle   = endangle
    LevelVars[dataName].CCW        = isCCW
    LevelVars[dataName].Loop       = isLoop
    LevelVars[dataName].Smooth     = isSmooth
    LevelVars[dataName].Rotate     = isRotate
    LevelVars[dataName].CRotate    = isCRotate
    LevelVars[dataName].Sfx        = SFX
    LevelVars[dataName].OldValue   = startangle
	LevelVars[dataName].StopAtEnd  = false
    
	LevelVars[dataName].NewValue = LevelVars[dataName].Loop
    and (startangle + (LevelVars[dataName].CCW and -360 or 360))
    or endangle
    
    LevelVars[dataName].Timer:Start()
end

-- Transform object parameter using previously saved timed transform data
LevelFuncs.Engine.Node.TransformTimedData = function(dataName)
   
        -- Smoothly loop the progress value
        LevelVars[dataName].Progress = LevelVars[dataName].Loop
		and (LevelVars[dataName].Progress + LevelVars[dataName].Interval) % 1
		or math.min(LevelVars[dataName].Progress + LevelVars[dataName].Interval, 1)
        -- Stop at 1
	
	--function to normalize the angles to 360
	local function NormalizeAngle(angle)
    return (angle % 360 + 360) % 360
	end
    	
    local factor = LevelVars[dataName].Smooth and LevelFuncs.Engine.Node.Smoothstep(LevelVars[dataName].Progress) or LevelVars[dataName].Progress
    local newValue1 = LevelFuncs.Engine.Node.Lerp(LevelVars[dataName].OldValue, LevelVars[dataName].NewValue, factor)
    local object2 = TEN.Objects.GetMoveableByName(LevelVars[dataName].ObjectName)
    local object2pos = object2:GetPosition()
    local center2 = TEN.Objects.GetMoveableByName(LevelVars[dataName].CentreName)
    local center2pos = center2:GetPosition()
	local center2rot = center2:GetRotation()
    local radius2 = LevelVars[dataName].Radius
    local angle = newValue1 * math.pi / 180
    local x = center2pos.x
    local y = center2pos.y
    local z = center2pos.z
	local normalizedNewValue1 = NormalizeAngle(newValue1)
    local normalizedEndAngle = NormalizeAngle(LevelVars[dataName].EndAngle)
	

    if (LevelVars[dataName].DataType == 0) then
       	if (center2rot.y == 0) then

			local ptx, pty = x + radius2 * math.sin(angle), y - radius2 * math.cos(angle)
			object2:SetPosition(Vec3(ptx, pty, object2pos.z))
			if not IsSoundPlaying(LevelVars[dataName].Sfx) then
            TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
			end
			if LevelVars[dataName].CRotate then    
            center2:SetRotation(Rotation(0, 0, newValue1))
			end
		elseif	(center2rot.y == 90) then
			local ptz, pty = z + radius2 * math.sin(-angle), y - radius2 * math.cos(-angle)
			object2:SetPosition(Vec3(object2pos.x, pty, ptz))
			if not IsSoundPlaying(LevelVars[dataName].Sfx) then
			TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
			end
			if LevelVars[dataName].CRotate then    
			center2:SetRotation(Rotation(0, center2rot.y, newValue1))
			end
		elseif	(center2rot.y == -180) then
			local ptx, pty = x + radius2 * math.sin(-angle), y - radius2 * math.cos(-angle)
			object2:SetPosition(Vec3(ptx, pty, object2pos.z))
			if not IsSoundPlaying(LevelVars[dataName].Sfx) then
			TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
			end
			if LevelVars[dataName].CRotate then    
			center2:SetRotation(Rotation(0, center2rot.y, newValue1))
			end	
		elseif	(center2rot.y == -90) then
			local ptz, pty = z + radius2 * math.sin(angle), y - radius2 * math.cos(angle)
			object2:SetPosition(Vec3(object2pos.x, pty, ptz))
			if not IsSoundPlaying(LevelVars[dataName].Sfx) then
			TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
			end
			if LevelVars[dataName].CRotate then    
			center2:SetRotation(Rotation(0, center2rot.y, newValue1))
			end
		end
    elseif (LevelVars[dataName].DataType == 1) then
        local ptz, ptx = z + radius2 * math.sin(-angle), x + radius2 * math.cos(-angle)
        object2:SetPosition(Vec3(ptx, object2pos.y, ptz))
        if not IsSoundPlaying(LevelVars[dataName].Sfx) then
            TEN.Sound.PlaySound(LevelVars[dataName].Sfx, object2pos)
        end
        if LevelVars[dataName].CRotate then    
            center2:SetRotation(Rotation(0, newValue1, 0))
        end
        if LevelVars[dataName].Rotate then    
            object2:SetRotation(Rotation(0, newValue1, 0))
        end
    end

    if LevelVars[dataName].StopAtEnd and  math.abs(normalizedNewValue1 - normalizedEndAngle) < 5e-1 then
        Timer.Delete(LevelVars[dataName].Name)
        LevelVars[dataName] = nil
    elseif not LevelVars[dataName].Loop and LevelVars[dataName].Progress >= 1 then
        Timer.Delete(LevelVars[dataName].Name)
        LevelVars[dataName] = nil
    end
end

--- Delete timed transform data
LevelFuncs.Engine.Node.DeleteTimedData = function(objectName, endangle)
    local dataName = objectName .. "_revolve_data"

	if LevelVars[dataName] and LevelVars[dataName].Timer and LevelVars[dataName].Timer:IsActive() then
   	LevelVars[dataName].EndAngle = endangle
    	LevelVars[dataName].StopAtEnd = true
	end
end



-- !Name "Revolve an object"
-- !Section "Timespan actions"
-- !Description "Rotate an object around another a moveable over specified timespan."
-- !Arguments "NewLine, Moveables, Moveable to rotate"
-- !Arguments "NewLine, Moveables, CentrePoint"
-- !Arguments "NewLine, Enumeration, [ Vertical | Horizontal], 20, Select Axis of Rotation."
-- !Arguments "Numerical, [ 1 | 65535 | 2 | 1 | 1 ], {1}, 20, Radius of Rotation"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 20, Time (in seconds)"
-- !Arguments "Numerical, [ -360 | 360 | 2 | 1 | 1 ], {0}, 20, Start Angle"
-- !Arguments "Numerical, [ -360 | 360 | 2 | 1 | 1 ], {360}, 20, End Angle. If Loop is ticked this is the angle the object will stop."
-- !Arguments "NewLine, 33, Boolean, Loop"
-- !Arguments "33, Boolean, Flip Loop"
-- !Arguments "33, Boolean, Smooth"
-- !Arguments "NewLine, 65, Boolean, Rotate Object (Y axis when XZ selected)"
-- !Arguments "35, Boolean, Rotate Center Object"
-- !Arguments "NewLine, SoundEffects,{635}",

LevelFuncs.Engine.Node.ChangeMoveableRotationOverTimespan = function(moveableName, centrepoint, option, radius, time, startangle, endangle, isLoop, isCCW, isSmooth, isRotate, isCRotate, SFX)
    -- Wrap another node function call into do/end to prevent wrong parsing
    do LevelFuncs.Engine.Node.ConstructTimedData(moveableName, centrepoint, option, radius, time, startangle, endangle, isLoop, isCCW, isSmooth, isRotate,  isCRotate, SFX) end
end

-- !Name "Stop the revolution of an object"
-- !Section "Timespan actions"
-- !Description "Stop an already active object rotation."
-- !Arguments "NewLine, Moveables, Moveable to stop rotation"
-- !Arguments "NewLine, Numerical, [ -360 | 360 | 2 | 1 | 1 ], {360}, 20, End Angle. If Loop is ticked this is the angle the object will stop."

LevelFuncs.Engine.Node.StopMoveableRotation = function(moveableName,endangle)
    -- Wrap another node function call into do/end to prevent wrong parsing
    do LevelFuncs.Engine.Node.DeleteTimedData(moveableName,endangle) end
end
