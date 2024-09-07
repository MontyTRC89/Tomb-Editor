-- !Name "If audio track is playing..."
-- !Section "Sound"
-- !Conditional "True"
-- !Description "Checks if specified audio track is playing."
-- !Arguments "NewLine, SoundTracks, Name of the audiotrack to check"

LevelFuncs.Engine.Node.AudioTrackIsPlaying = function(name)
    return TEN.Sound.IsAudioTrackPlaying(name)
end

-- !Name "If audio track loudness is..."
-- !Section "Sound"
-- !Conditional "True"
-- !Description "Checks specified audio track loudness."
-- !Arguments "NewLine, CompareOperator, 33, Compare operation" "Numerical, 33, [ 0 | 1 | 2 | 0.05 | 0.1 ], Loudness"
-- !Arguments "Enumeration, 34, [ One shot | Looped | Voice ], Audiotrack type to check"

LevelFuncs.Engine.Node.CheckAudioTrackLoudness = function(operator, value, mode)
    return LevelFuncs.Engine.Node.CompareValue(
        TEN.Sound.GetAudioTrackLoudness(LevelFuncs.Engine.Node.GetSoundTrackType(mode)), value, operator)
end

-- !Name "Play audio track"
-- !Section "Sound"
-- !Description "Plays specified audio track."
-- !Arguments "NewLine, 75, SoundTracks, Name of the audiotrack to play"
-- !Arguments "Enumeration, 25, [ One shot | Looped | Voice ], Audiotrack type to set"

LevelFuncs.Engine.Node.PlayAudioTrack = function(name, type)
    -- LEGACY: Make pre-1.1.0 nodes compatible with updated audio track enumeration.
    local realType = 0;
    if (type == true) then
        realType = 1
    elseif (type == false) then
        realType = 0
    else
        realType = type
    end

    TEN.Sound.PlayAudioTrack(name, realType)
end

-- !Name "Stop audio track"
-- !Section "Sound"
-- !Description "Stops audio track of the specified type."
-- !Arguments "Enumeration, 25, [ One shot | Looped | Voice ], Audiotrack type to stop"

LevelFuncs.Engine.Node.StopAudioTrack = function(looped)
    TEN.Sound.StopAudioTrack(looped)
end

-- !Name "Stop all audio tracks"
-- !Section "Sound"
-- !Description "Stops audio tracks of all types."

LevelFuncs.Engine.Node.StopAudioTracks = function()
    TEN.Sound.StopAudioTracks()
end

-- !Name "If sound is playing..."
-- !Section "Sound"
-- !Conditional "True"
-- !Description "Checks if specified sound is playing."
-- !Arguments "NewLine, SoundEffects, Sound to check"

LevelFuncs.Engine.Node.SoundIsPlaying = function(soundID)
    return TEN.Sound.IsSoundPlaying(soundID)
end
