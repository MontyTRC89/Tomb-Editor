flux = require "Engine/flux"
flux.update(deltatime)
-- !Name "Activate camera"
-- !Section "Camera"
-- !Description "Activate camera"
-- !Arguments "NewLine,Cameras"

LevelFuncs.Engine.Node.ActivateCamera = function(camName)
    local cam = TEN.Objects.GetCameraByName(camName):PlayCamera()
end

-- !Name "Reset game camera to default position"
-- !Section "Camera"
-- !Description "Reset camera if target has been modified."

LevelFuncs.Engine.Node.ResetObjCam = function()
	ResetObjCamera()
	SetFOV(80)
end

-- !Name "Modify position of a camera"
-- !Section "Camera"
-- !Description "Reset camera if target has been modified."
-- !Arguments "NewLine, Enumeration, [ Change | Set ], 25, Change adds/subtracts given value while Set forces it."
-- !Arguments "Vector3, [ -1000000 | 1000000 | 0 | 1 | 32 ], 75, Position value to define"
-- !Arguments "NewLine, Cameras"

LevelFuncs.Engine.Node.SetCameraPosition = function(operation, value, cameraName)

	local camera = TEN.Objects.GetCameraByName(cameraName)

	if (operation == 0) then
		local position = camera:GetPosition();
		position.x = position.x + value.x
		position.y = position.y + value.y
		position.z = position.z + value.z
		camera:SetPosition(position)
	else
		camera:SetPosition(value)
	end
    
end

