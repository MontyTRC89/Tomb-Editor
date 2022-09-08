-- !Name "Draw text"
-- !Section "Text"
-- !Description "Draws simple label on a screen."
-- !Arguments "NewLine, String, Text"
-- !Arguments "NewLine, Numerical, 20, X position, [ 0 | 100 | 0 ]" "Numerical, 20, Y position, [ 0 | 100 | 0 ]" 
-- !Arguments "Boolean, 20, Centered" "Boolean, 20, Shadow" "Color, 20, Text color"

LevelFuncs.DrawText = function(text, x, y, center, shadow, color)

	local string = LevelFuncs.GenerateString(text, x, y, center, shadow, color)
	TEN.Strings.ShowString(string, 1 / 30)
end