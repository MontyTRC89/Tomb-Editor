-- !Name "Flash screen"
-- !Section "Effects"
-- !Description "Flashes screen with specified color and for specified duration.\nDuration value of 1 takes 1 second to flash."
-- !Arguments "Color, 10, Flash colour" "Numerical, 20, [ 0.1 | 10 | 2 | 0.1 | 0.5 ], Flash speed" 

LevelFuncs.FlashScreen = function(color, duration)
    Effects.FlashScreen(color, duration)
end

-- !Name "Play sound near moveable"
-- !Section "Effects"
-- !Description "Plays specified sound ID around specified object."
-- !Arguments "NewLine, Moveables, Moveable to play sound around" "NewLine, SoundEffects, Sound to play"

LevelFuncs.PlaySoundAroundMoveable = function(moveableName, soundID)
    Misc.PlaySound(soundID, TEN.Objects.GetMoveableByName(moveableName):GetPosition())
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