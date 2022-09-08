-- !Name "Draw text"
-- !Section "Text"
-- !Description "Draws simple label on a screen for one frame."
-- !Arguments "NewLine, String, Text"
-- !Arguments "NewLine, Numerical, 20, X position, [ 0 | 100 | 0 ]" "Numerical, 20, Y position, [ 0 | 100 | 0 ]" 
-- !Arguments "Boolean, 20, Centered" "Boolean, 20, Shadow" "Color, 20, Text color"

LevelFuncs.DrawText = function(text, x, y, center, shadow, color)

	local string = LevelFuncs.GenerateString(text, x, y, center, shadow, color)
	TEN.Strings.ShowString(string, 1 / 30)
end

-- !Name "Draw text for a time span"
-- !Section "Text"
-- !Description "Draws simple label on a screen for a specified time span."
-- !Arguments "Numerical, 20, [ 0 | 180 | 2 | 0.25 | 1 ], Time span in seconds"
-- !Arguments "NewLine, String, Text"
-- !Arguments "NewLine, Numerical, 20, X position, [ 0 | 100 | 0 ]" "Numerical, 20, Y position, [ 0 | 100 | 0 ]" 
-- !Arguments "Boolean, 20, Centered" "Boolean, 20, Shadow" "Color, 20, Text color"

LevelFuncs.DrawTextForTimespan = function(time, text, x, y, center, shadow, color)

	local string = LevelFuncs.GenerateString(text, x, y, center, shadow, color)
	TEN.Strings.ShowString(string, time)
end