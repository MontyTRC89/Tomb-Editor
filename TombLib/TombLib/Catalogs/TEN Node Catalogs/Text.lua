-- !Name "Draw text"
-- !Section "Text"
-- !Description "Draws simple label on a screen for one frame."
-- !Arguments "NewLine, String, Text"
-- !Arguments "NewLine, Numerical, 20, X position, [ 0 | 100 ]" "Numerical, 20, Y position, [ 0 | 100 ]" 
-- !Arguments "Boolean, 20, Centered" "Boolean, 20, Shadow" "Color, 20, Text color"

LevelFuncs.Engine.Node.DrawText = function(text, x, y, center, shadow, color)

	local str = LevelFuncs.Engine.Node.GenerateString(text, x, y, center, shadow, color)
	TEN.Strings.ShowString(str, 1 / 30)
end

-- !Name "Draw text for a time span"
-- !Section "Text"
-- !Description "Draws simple label on a screen for a specified time span."
-- !Arguments "Numerical, 20, [ 0 | 180 | 2 | 0.25 | 1 ], Time span in seconds"
-- !Arguments "NewLine, String, Text"
-- !Arguments "NewLine, Numerical, 20, X position, [ 0 | 100 ]" "Numerical, 20, Y position, [ 0 | 100 ]" 
-- !Arguments "Boolean, 20, Centered" "Boolean, 20, Shadow" "Color, 20, Text color"

LevelFuncs.Engine.Node.DrawTextForTimespan = function(time, text, x, y, center, shadow, color)

	local str = LevelFuncs.Engine.Node.GenerateString(text, x, y, center, shadow, color)
	TEN.Strings.ShowString(str, time)
end

-- !Name "Draw subtitle for the voice track"
-- !Section "Text"
-- !Description "If voice track is active and subtitle file with the same name as voice track exists in same folder, try to display a subtitle."
-- !Arguments "NewLine, Numerical, 20, X position, [ 0 | 100 ]" "Numerical, 20, Y position, [ 0 | 100 ]" 
-- !Arguments "Boolean, 20, Centered" "Boolean, 20, Shadow" "Color, 20, Text color"

LevelFuncs.Engine.Node.DrawSubtitle = function(x, y, center, shadow, color)

	local text = Misc.GetCurrentSubtitle()
	
	if (text ~= nil) then
		local str = LevelFuncs.Engine.Node.GenerateString(text, x, y, center, shadow, color)
		TEN.Strings.ShowString(str, 1 / 30)
	end
	
end