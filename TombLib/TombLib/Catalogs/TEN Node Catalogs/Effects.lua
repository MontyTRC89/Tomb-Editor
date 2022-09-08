-- !Name "Flash screen"
-- !Section "Effects"
-- !Description "Flashes screen with specified color and for specified duration.\nDuration value of 1 takes 1 second to flash."
-- !Arguments "Color, 10, Flash color" "Numerical, 20, [ 0.1 | 10 | 2 | 0.1 | 0.5 ], Flash speed" 

LevelFuncs.FlashScreen = function(color, duration)
    TEN.Effects.FlashScreen(color, duration)
end

-- !Name "Fade out screen"
-- !Section "Effects"
-- !Description "Do a screen fade-out to black."
-- !Arguments "Numerical, 25, [0 | 100 | 2 ], Fade speed"

LevelFuncs.FadeOut = function(speed)
    TEN.Misc.FadeOut(speed)
end

-- !Name "Fade in screen"
-- !Section "Effects"
-- !Description "Do a screen fade-in from black."
-- !Arguments "Numerical, 25, [0 | 100 | 2 ], Fade speed"

LevelFuncs.FadeIn = function(speed)
    TEN.Misc.FadeIn(speed)
end

-- !Name "Set cinematic bars"
-- !Section "Effects"
-- !Description "Toggle cinematic bars visibility."
-- !Arguments "Numerical, 15, [0 | 100 | 0 ], Cinematic bars height"
-- !Arguments "Numerical, 15, [0 | 100 | 0 ], Cinematic bars speed"

LevelFuncs.SetCineBars = function(height, speed)
    TEN.Misc.SetCineBars(height, speed)
end

-- !Name "Set screen field of view"
-- !Section "Effects"
-- !Description "Change screen field of view."
-- !Arguments "Numerical, 25, [ 10 | 170 | 2 ], Field of view in degrees"

LevelFuncs.SetFOV = function(fov)
    TEN.Misc.SetFOV(fov)
end

-- !Name "Shake camera"
-- !Section "Effects"
-- !Description "Shakes camera with specified strength.\nStrength also determines how long effect would take place."
-- !Arguments "Numerical, 20, [ 0 | 256 | 0 ], Shake strength" 

LevelFuncs.ShakeCamera = function(strength)
    TEN.Effects.MakeEarthquake(strength)
end

-- !Name "Vibrate game controller"
-- !Section "Effects"
-- !Description "Vibrates game controller, if vibration feature is available and setting is on."
-- !Arguments "Numerical, 15, [ 0 | 1 | 2 | 0.25 | 0.5 ], Vibration strength" 
-- !Arguments "Numerical, 15, [ 0 | 10 | 2 | 0.25 | 0.5 ], Vibration time in seconds" 

LevelFuncs.Vibrate = function(strength, time)
    TEN.Misc.Vibrate(strength, time)
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