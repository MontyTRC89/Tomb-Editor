-- !Name "If current camera room is..."
-- !Section "View"
-- !Conditional "True"
-- !Description "Check if camera is in particular room."
-- !Arguments "NewLine, Rooms, Room to check"

LevelFuncs.Engine.Node.IsCameraInRoom = function(roomName)
    return TEN.View.GetCameraRoom():GetName() == roomName
end

-- !Name "If current camera room flag is set..."
-- !Section "View"
-- !Conditional "True"
-- !Description "Check if current camera room has a particular flag set."
-- !Arguments "Enumeration, [ Water | Quicksand | Skybox | Wind | Cold | Damage | No lensflare ], 30, Flag type"

LevelFuncs.Engine.Node.IsCameraRoomFlagSet = function(flag)
	local flagIndex = LevelFuncs.Engine.Node.GetRoomFlag(flag)
	return TEN.View.GetCameraRoom():GetFlag(flagIndex)
end

-- !Name "If current camera room tag is present..."
-- !Section "View"
-- !Conditional "True"
-- !Description "Check if current camera room has a particular tag."
-- !Arguments "NewLine, String, Tag to search"

LevelFuncs.Engine.Node.IsCameraRoomTagPresent = function(tag)
	return TEN.View.GetCameraRoom():IsTagPresent(tag)
end

-- !Name "If fade out is complete..."
-- !Section "View"
-- !Conditional "True"
-- !Description "Check if fade out was finished and screen is totally black."

LevelFuncs.Engine.Node.FadeOutComplete = function()
    return TEN.View.FadeOutComplete()
end

-- !Name "Shake camera"
-- !Section "View"
-- !Description "Shakes camera with specified strength.\nStrength also determines how long effect would take place."
-- !Arguments "Numerical, 20, [ 0 | 256 ], Shake strength"

LevelFuncs.Engine.Node.ShakeCamera = function(strength)
    TEN.Effects.MakeEarthquake(strength)
end

-- !Name "Play flyby sequence"
-- !Section "View"
-- !Description "Plays desired flyby sequence."
-- !Arguments "Numerical, 20, [ 0 | 256 ], Flyby sequence index"

LevelFuncs.Engine.Node.PlayFlyBy = function(index)
    TEN.View.PlayFlyBy(index)
end

-- !Name "Activate camera"
-- !Section "View"
-- !Description "Activate camera placed on the map.\nThis camera will be a fixed camera that allows weapons to be fired and the look key to be used."
-- !Arguments "NewLine, Cameras, Choose camera to activate." "NewLine, Moveables, Choose moveable to target (default Lara)"

LevelFuncs.Engine.Node.ActivateCamera = function(camName, moveable)
    local moveableTarget = TEN.Objects.GetMoveableByName(moveable)
    local cam = TEN.Objects.GetCameraByName(camName):PlayCamera(moveableTarget)
end

-- !Name "Set screen field of view"
-- !Section "View"
-- !Description "Change screen field of view."
-- !Arguments "Numerical, 25, [ 10 | 170 | 2 ], Field of view in degrees"

LevelFuncs.Engine.Node.SetFOV = function(fov)
    TEN.View.SetFOV(fov)
end

-- !Name "Flash screen"
-- !Section "View"
-- !Description "Flashes screen with specified color and for specified duration.\nDuration value of 1 takes 1 second to flash."
-- !Arguments "Color, 10, Flash color" "Numerical, 20, [ 0.1 | 10 | 2 | 0.1 | 0.5 ], Flash speed"

LevelFuncs.Engine.Node.FlashScreen = function(color, duration)
    TEN.View.FlashScreen(color, duration)
end

-- !Name "Fade out screen"
-- !Section "View"
-- !Description "Do a screen fade-out to black."
-- !Arguments "Numerical, 25, [0 | 100 | 2 ], Fade speed"

LevelFuncs.Engine.Node.FadeOut = function(speed)
    TEN.View.FadeOut(speed)
end

-- !Name "Fade in screen"
-- !Section "View"
-- !Description "Do a screen fade-in from black."
-- !Arguments "Numerical, 25, [0 | 100 | 2 ], Fade speed"

LevelFuncs.Engine.Node.FadeIn = function(speed)
    TEN.View.FadeIn(speed)
end

-- !Name "Set cinematic bars"
-- !Section "View"
-- !Description "Toggle cinematic bars visibility."
-- !Arguments "Numerical, 15, [0 | 100 ], Cinematic bars height"
-- !Arguments "Numerical, 15, [0 | 100 ], Cinematic bars speed"

LevelFuncs.Engine.Node.SetCineBars = function(height, speed)
    TEN.View.SetCineBars(height, speed)
end

-- !Name "Set display postprocessing mode"
-- !Section "View"
-- !Description "Set the postprocessing mode for all graphics, excluding GUI"
-- !Arguments "NewLine, Enumeration, 50, [ None | Monochrome | Negative | Exclusion ], Sets the postprocessing mode"
-- !Arguments "Numerical, 25, [ 0 | 1 | 2 | 0.05 | 0.1 ], Postprocessing strength"
-- !Arguments "Color, 25, { TEN.Color(128,128,128) }, Set the tint color that overlays over the chosen color mode.\nMay be used with postprocessing mode set to None."

LevelFuncs.Engine.Node.SetPostProcessDisplay = function(postProcessModeEnum, power, tintColor) 

    local postProcessMode = LevelFuncs.Engine.Node.SetPostProcessMode(postProcessModeEnum)
    TEN.View.SetPostProcessMode(postProcessMode)
    TEN.View.SetPostProcessStrength(power)
    TEN.View.SetPostProcessTint(tintColor)
end

-- !Name "Attach camera to moveable"
-- !Section "View"
-- !Description "Attaches game camera to a specific moveable."
-- !Arguments "NewLine, Moveables, 70, Source moveable" , "Number, 30 , [ 0 | 50 | 0 ] , Mesh number of source moveable to attach camera target to"
-- !Arguments "NewLine, Moveables, 70, Target moveable" , "Number, 30 , [ 0 | 50 | 0 ] , Mesh number of target moveable to attach camera target to"

LevelFuncs.Engine.Node.AttachCameraToMoveable = function(source, sourceMesh, target, targetMesh)
    local sourcePos = TEN.Objects.GetMoveableByName(source)
    local targetPos = TEN.Objects.GetMoveableByName(target)
    sourcePos:AttachObjCamera(sourceMesh, targetPos, targetMesh)
end

-- !Name "Reset game camera to default position"
-- !Section "View"
-- !Description "Reset camera if target has been modified."

LevelFuncs.Engine.Node.ResetObjCam = function()
	ResetObjCamera()
end