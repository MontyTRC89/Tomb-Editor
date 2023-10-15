﻿-- !Name "If random event occurs..."
-- !Section "Game flow"
-- !Conditional "True"
-- !Description "Happens whether a random event occurs on the range from 1 to 100 percent."
-- !Description "Every time this condition is called, chance is recalculated."
-- !Arguments "Numerical, 15, [ 1 | 100 ], Percentage of event occurence"

LevelFuncs.Engine.Node.TestPercentageChance = function(percentageRange)
	return (math.random() * 100 <= percentageRange)
end

-- !Name "End level"
-- !Section "Game flow"
-- !Description "Ends current level and loads next level according to number. If number is 0, loads next level."
-- !Description "If number is more than level count, loads title."
-- !Arguments "Numerical, 15, [ 0 | 99 ], Next level number"

LevelFuncs.Engine.Node.EndLevel = function(number)
	TEN.Flow.EndLevel(number)
end

-- !Name "Add secret"
-- !Section "Game flow"
-- !Description "Adds one secret to game secret count and plays secret soundtrack."
-- !Arguments "Numerical, 15, [ 0 | 7 ], Level secret index"

LevelFuncs.Engine.Node.AddSecret = function(number)
	TEN.Flow.AddSecret(number)
end

-- !Name "Set secret count"
-- !Section "Game flow"
-- !Description "Overwrites current game secret count with provided one."
-- !Arguments "Numerical, 15, [0 | 99 ], New secret count"

LevelFuncs.Engine.Node.SetSecretCount = function(number)
	TEN.Flow.SetSecretCount(number)
end

-- !Name "If game secret count is..."
-- !Section "Game flow"
-- !Description "Checks current game secret count."
-- !Conditional "True"
-- !Arguments "CompareOperator, 25, Compare operation" "Numerical, 15, [ 0 | 99 ], Secret count"

LevelFuncs.Engine.Node.GetSecretCount = function(operator, number)
	return LevelFuncs.Engine.Node.CompareValue(TEN.Flow.GetSecretCount(), number, operator)
end

-- !Name "Run event from another event set"
-- !Section "Game flow"
-- !Description "Runs an event from another event set."
-- !Arguments "NewLine, 70, EventSets, Target event set"
-- !Arguments "Enumeration, 30, [ On enter | On inside | On leave ], Event to run"
-- !Arguments "NewLine, Moveables, Activator for the event (when necessary)"

LevelFuncs.Engine.Node.RunEventSet = function(setName, eventType, activator)
	if (setName == '' or setName == nil) then
		print("There is no specified event set in level!")
		return
	end

	TEN.Logic.HandleEvent(setName, eventType, TEN.Objects.GetMoveableByName(activator))
end

-- !Name "Run script function"
-- !Section "Game flow"
-- !Description "Runs specified Lua function from level script file."
-- !Description "If needed, arguments should be provided in string form, divided by commas."
-- !Arguments "NewLine, LuaScript, Target Lua script function" "NewLine, String, Arguments"

LevelFuncs.Engine.Node.RunLuaScript = function(funcName, args)
	if (funcName == nil) then
		print("There is no specified function in level script!")
		return
	end

	funcName(table.unpack(LevelFuncs.Engine.Node.SplitString(args, ",")))
end

-- !Name "If script function returns..."
-- !Section "Game flow"
-- !Description "Runs specified conditional Lua function from level script file."
-- !Description "Function should return either numerical or boolean (treated as 0 or 1) value."
-- !Description "If needed, arguments should be provided in string form, divided by commas."
-- !Conditional "True"
-- !Arguments "CompareOperator, 25, Compare operation" "Numerical, 15, [ -65536 | 65535 ]"
-- !Arguments "NewLine, LuaScript, Target Lua script function" "NewLine, String, Arguments"

LevelFuncs.Engine.Node.RunConditionalLuaScript = function(operator, result, funcName, args)
	if (funcName == nil) then
		print("There is no specified function in level script!")
		return 0
	end

	local params = LevelFuncs.Engine.Node.SplitString(args, ",")
	return LevelFuncs.Engine.Node.CompareValue(funcName(table.unpack(params)), result, operator)
end

-- !Name "Save the game"
-- !Conditional "False"
-- !Description "Saves the current game in a specific slot"
-- !Section "Game flow"
-- !Arguments "NewLine, Numerical, 100, [ 0 | 99 | 0 ], Save slots"
LevelFuncs.Engine.Node.SaveGame = function(slot)
	TEN.Flow.SaveGame(slot)
end

-- !Name "Load the game"
-- !Conditional "False"
-- !Description "Load the selected save slot"
-- !Section "Game flow"
-- !Arguments "NewLine, Numerical, 100, [ 0 | 99 | 0 ], Save slots"
LevelFuncs.Engine.Node.LoadGame = function(slot)
	TEN.Flow.LoadGame(slot)
end

-- !Name "Delete Save slot"
-- !Conditional "False"
-- !Description "Delete a specific save slot"
-- !Section "Game flow"
-- !Arguments "NewLine, Numerical, 100, [ 0 | 99 | 0 ], Save slots"
LevelFuncs.Engine.Node.DeleteSaveGame = function(slot)
	TEN.Flow.DeleteSaveGame(slot)
end

-- !Name "If SaveGame exists..."
-- !Conditional "True"
-- !Description "Check if SaveGame exists in a specific slot"
-- !Section "Game flow"
-- !Arguments "NewLine, Numerical, 100, [ 0 | 99 | 0 ], Save slots"
LevelFuncs.Engine.Node.DoesSaveGameExist = function(slot)
	return TEN.Flow.DoesSaveGameExist(slot)
end
