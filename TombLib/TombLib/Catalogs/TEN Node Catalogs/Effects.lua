-- !Name "Check if fade out is complete"
-- !Section "Effects"
-- !Conditional "True"
-- !Description "Check if fade out was finished and screen is totally black."

LevelFuncs.Engine.Node.FadeOutComplete = function()
    return TEN.Misc.FadeOutComplete()
end

-- !Name "Flash screen"
-- !Section "Effects"
-- !Description "Flashes screen with specified color and for specified duration.\nDuration value of 1 takes 1 second to flash."
-- !Arguments "Color, 10, Flash color" "Numerical, 20, [ 0.1 | 10 | 2 | 0.1 | 0.5 ], Flash speed" 

LevelFuncs.Engine.Node.FlashScreen = function(color, duration)
    TEN.Effects.FlashScreen(color, duration)
end

-- !Name "Fade out screen"
-- !Section "Effects"
-- !Description "Do a screen fade-out to black."
-- !Arguments "Numerical, 25, [0 | 100 | 2 ], Fade speed"

LevelFuncs.Engine.Node.FadeOut = function(speed)
    TEN.Misc.FadeOut(speed)
end

-- !Name "Fade in screen"
-- !Section "Effects"
-- !Description "Do a screen fade-in from black."
-- !Arguments "Numerical, 25, [0 | 100 | 2 ], Fade speed"

LevelFuncs.Engine.Node.FadeIn = function(speed)
    TEN.Misc.FadeIn(speed)
end

-- !Name "Set cinematic bars"
-- !Section "Effects"
-- !Description "Toggle cinematic bars visibility."
-- !Arguments "Numerical, 15, [0 | 100 ], Cinematic bars height"
-- !Arguments "Numerical, 15, [0 | 100 ], Cinematic bars speed"

LevelFuncs.Engine.Node.SetCineBars = function(height, speed)
    TEN.Misc.SetCineBars(height, speed)
end

-- !Name "Set screen field of view"
-- !Section "Effects"
-- !Description "Change screen field of view."
-- !Arguments "Numerical, 25, [ 10 | 170 | 2 ], Field of view in degrees"

LevelFuncs.Engine.Node.SetFOV = function(fov)
    TEN.Misc.SetFOV(fov)
end

-- !Name "Play flyby sequence"
-- !Section "Effects"
-- !Description "Plays desired flyby sequence."
-- !Arguments "Numerical, 20, [ 0 | 256 ], Flyby sequence index" 

LevelFuncs.Engine.Node.PlayFlyBy = function(index)
    TEN.Misc.PlayFlyBy(index)
end

-- !Name "Shake camera"
-- !Section "Effects"
-- !Description "Shakes camera with specified strength.\nStrength also determines how long effect would take place."
-- !Arguments "Numerical, 20, [ 0 | 256 ], Shake strength" 

LevelFuncs.Engine.Node.ShakeCamera = function(strength)
    TEN.Effects.MakeEarthquake(strength)
end

-- !Name "Vibrate game controller"
-- !Section "Effects"
-- !Description "Vibrates game controller, if vibration feature is available and setting is on."
-- !Arguments "Numerical, 15, [ 0 | 1 | 2 | 0.25 | 0.5 ], Vibration strength" 
-- !Arguments "Numerical, 15, [ 0 | 10 | 2 | 0.25 | 0.5 ], Vibration time in seconds" 

LevelFuncs.Engine.Node.Vibrate = function(strength, time)
    TEN.Misc.Vibrate(strength, time)
end

-- !Name "Play audio track"
-- !Section "Effects"
-- !Description "Plays specified audio track."
-- !Arguments "NewLine, 82, SoundTracks, Name of the audiotrack to play" "Boolean, 18, Looped"

LevelFuncs.Engine.Node.PlayAudioTrack = function(name, looped)
	TEN.Misc.PlayAudioTrack(name, looped)
end

-- !Name "Stop audio track"
-- !Section "Effects"
-- !Description "Stops audio track of the specified mode."
-- !Arguments "Boolean, 18, Looped"

LevelFuncs.Engine.Node.StopAudioTrack = function(looped)
	TEN.Misc.StopAudioTrack(looped)
end

-- !Name "Stop both audio tracks"
-- !Section "Effects"
-- !Description "Stops audio tracks of both looped and one-shot types."

LevelFuncs.Engine.Node.StopAudioTracks = function()
	TEN.Misc.StopAudioTracks()
end