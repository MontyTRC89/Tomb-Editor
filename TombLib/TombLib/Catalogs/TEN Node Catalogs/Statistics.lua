local Statistics = require("Engine.RingInventory.Statistics")
local count = 0

-- !Name "Shows end of level statistics"
-- !Section "User interface"
-- !Description "Show end of level statistics."
-- !Arguments "NewLine, Numerical, 20, [ 0 | 256 ], Next level"
-- !Arguments "80, SoundTracks, Name of the audiotrack to play"
LevelFuncs.Engine.Node.ShowEndLevelStatistics = function(level, track)
	
    if count == 0 then
        Statistics.SetType(false)
        TEN.Sound.PlayAudioTrack(track)
        Statistics.SetEndStatistics(true, level)
    end
	
end

-- !Name "Shows end of game statistics"
-- !Section "User interface"
-- !Description "Show end of game statistics."
-- !Arguments "NewLine, 100, SoundTracks, Name of the audiotrack to play"
LevelFuncs.Engine.Node.ShowEndGameStatistics = function(track)
	
    if count == 0 then
        Statistics.SetType(true)
        TEN.Sound.PlayAudioTrack(track)
        Statistics.SetEndStatistics(true, 999)
    end

end