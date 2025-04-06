-- !Name "If background video is playing..."
-- !Section "Video"
-- !Conditional "True"
-- !Description "Check if any video is currently playing in background mode."

LevelFuncs.Engine.Node.IsVideoPlaying = function()
    return TEN.View.IsVideoPlaying()
end

-- !Name "If specified background video is playing..."
-- !Section "Video"
-- !Conditional "True"
-- !Arguments "NewLine, Videos, Video file to check"
-- !Description "Check if specified video is currently playing in background mode."

LevelFuncs.Engine.Node.IsSpecifiedVideoPlaying = function(fileName)
    return TEN.View.IsVideoPlaying(fileName)
end

-- !Name "Play video"
-- !Section "Video"
-- !Description "Plays desired video file. If played in background, video can be used for streaming to textures and sprites."
-- !Arguments "NewLine, Videos, Video file to play"
-- !Arguments "NewLine, Boolean, 60, Background mode for streaming" "Boolean, 20, Mute", "Boolean, 20, Loop"

LevelFuncs.Engine.Node.PlayVideo = function(fileName, background, mute, loop)
    TEN.View.PlayVideo(fileName, background, mute, loop)
end

-- !Name "Stop video"
-- !Section "Video"
-- !Description "Stops any background video file that is currently playing."

LevelFuncs.Engine.Node.StopVideo = function()
    TEN.View.StopVideo()
end