-- Helper function for value comparisons. Any function which uses
-- CompareOperand arguments should use this helper function for comparison.

LevelFuncs.CompareValue = function(value, reference, operand)

	local result = false
	if (operand == 0 and value == reference) then result = true end
	if (operand == 1 and value ~= reference) then result = true end
	if (operand == 2 and value <  reference) then result = true end
	if (operand == 3 and value <= reference) then result = true end
	if (operand == 4 and value >  reference) then result = true end
	if (operand == 5 and value >= reference) then result = true end	
	return result
end

-- Helper function for easy generation of a display string with all parameters set.

LevelFuncs.GenerateString = function(text, x, y, center, shadow, color)

	local options = { }
	if (shadow == true) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
	if (center == true) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
	local rX, rY = TEN.Misc.PercentToScreen(x, y)
	return TEN.Strings.DisplayString(text, rX, rY, color, false, options)
end