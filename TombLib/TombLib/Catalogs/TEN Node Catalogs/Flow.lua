-- !Name "If random event occurs..."
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
-- !Description "If number is more than level count, loads title. Optionally, an OCB for target start position object may be provided."
-- !Arguments "Numerical, 15, [ 0 | 99 ], Next level number"
-- !Arguments "Numerical, 15, [ 0 | 99 ], Start position OCB index"

LevelFuncs.Engine.Node.EndLevel = function(number, startPosIndex)
	TEN.Flow.EndLevel(number, startPosIndex)
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

-- !Name "Run volume event"
-- !Section "Game flow"
-- !Description "Runs a volume event from another event set."
-- !Arguments "NewLine, 65, VolumeEventSets, Target event set"
-- !Arguments "VolumeEvents, 35, Event to run"
-- !Arguments "NewLine, Moveables, Activator for the event (when necessary)"

LevelFuncs.Engine.Node.RunEventSet = function(setName, eventType, activator)
	if (LevelFuncs.Engine.Node.StringIsEmpty(setName)) then
		print("There is no specified event set in level!")
		return
	end

	TEN.Logic.HandleEvent(setName, eventType, TEN.Objects.GetMoveableByName(activator))
end

-- !Name "Enable volume event"
-- !Section "Game flow"
-- !Description "Enables an event in a specified volume event set."
-- !Arguments "NewLine, 65, VolumeEventSets, Target event set"
-- !Arguments "VolumeEvents, 35, Event to enable"

LevelFuncs.Engine.Node.EnableEvent = function(setName, eventType)
	if (LevelFuncs.Engine.Node.StringIsEmpty(setName)) then
		print("There is no specified event set in level!")
		return
	end

	TEN.Logic.EnableEvent(setName, eventType)
end

-- !Name "Disable volume event"
-- !Section "Game flow"
-- !Description "Disables an event in a specified volume event set."
-- !Arguments "NewLine, 65, VolumeEventSets, Target event set"
-- !Arguments "VolumeEvents, 35, Event to disable"

LevelFuncs.Engine.Node.DisableEvent = function(setName, eventType)
	if (LevelFuncs.Engine.Node.StringIsEmpty(setName)) then
		print("There is no specified event set in level!")
		return
	end
	print("Disable " .. setName .. " type: " .. eventType)

	TEN.Logic.DisableEvent(setName, eventType)
end

-- !Name "Run global event"
-- !Section "Game flow"
-- !Description "Runs a global event from another event set."
-- !Arguments "NewLine, 65, GlobalEventSets, Target event set"
-- !Arguments "GlobalEvents, 35, Event to run"
-- !Arguments "NewLine, Moveables, Activator for the event (when necessary)"

LevelFuncs.Engine.Node.RunGlobalEventSet = function(setName, eventType, activator)
	if (LevelFuncs.Engine.Node.StringIsEmpty(setName)) then
		print("There is no specified event set in level!")
		return
	end

	TEN.Logic.HandleEvent(setName, eventType + Logic.EventType.LOOP, TEN.Objects.GetMoveableByName(activator))
end

-- !Name "Enable global event"
-- !Section "Game flow"
-- !Description "Enables an event in a specified global event set."
-- !Arguments "NewLine, 65, GlobalEventSets, Target event set"
-- !Arguments "GlobalEvents, 35, Event to enable"

LevelFuncs.Engine.Node.EnableGlobalEvent = function(setName, eventType)
	if (LevelFuncs.Engine.Node.StringIsEmpty(setName)) then
		print("There is no specified event set in level!")
		return
	end
	print("Enable " .. setName .. " type: " .. eventType + Logic.EventType.LOOP)
	TEN.Logic.EnableEvent(setName, eventType + Logic.EventType.LOOP)
end

-- !Name "Disable global event"
-- !Section "Game flow"
-- !Description "Disables an event in a specified global event set."
-- !Arguments "NewLine, 65, GlobalEventSets, Target event set"
-- !Arguments "GlobalEvents, 35, Event to disable"

LevelFuncs.Engine.Node.DisableGlobalEvent = function(setName, eventType)
	if (LevelFuncs.Engine.Node.StringIsEmpty(setName)) then
		print("There is no specified event set in level!")
		return
	end
	print("Disable " .. setName .. " type: " .. eventType + Logic.EventType.LOOP)

	TEN.Logic.DisableEvent(setName, eventType + Logic.EventType.LOOP)
end

-- !Name "Run script function"
-- !Section "Game flow"
-- !Description "Runs specified Lua function from level script file."
-- !Description "If needed, arguments should be provided in string form, divided by commas."
-- !Arguments "NewLine, LuaScript, Target Lua script function" "NewLine, String, Arguments"

LevelFuncs.Engine.Node.RunLuaScript = function(funcName, args)
	if (LevelFuncs.Engine.Node.StringIsEmpty(funcName)) then
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
	if (LevelFuncs.Engine.Node.StringIsEmpty(funcName)) then
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
-- !Arguments "Numerical, 20, [ 0 | 99 | 0 ], Save slots"
LevelFuncs.Engine.Node.SaveGame = function(slot)
	TEN.Flow.SaveGame(slot)
end

-- !Name "Load the game"
-- !Conditional "False"
-- !Description "Load the selected save slot"
-- !Section "Game flow"
-- !Arguments "Numerical, 20, [ 0 | 99 | 0 ], Save slots"
LevelFuncs.Engine.Node.LoadGame = function(slot)
	TEN.Flow.LoadGame(slot)
end

-- !Name "Delete savegame"
-- !Conditional "False"
-- !Description "Delete a specific save slot"
-- !Section "Game flow"
-- !Arguments "Numerical, 20, [ 0 | 99 | 0 ], Save slots"
LevelFuncs.Engine.Node.DeleteSaveGame = function(slot)
	TEN.Flow.DeleteSaveGame(slot)
end

-- !Name "If savegame exists..."
-- !Conditional "True"
-- !Description "Check if SaveGame exists in a specific slot"
-- !Section "Game flow"
-- !Arguments "Numerical, 20, [ 0 | 99 | 0 ], Save slots"
LevelFuncs.Engine.Node.DoesSaveGameExist = function(slot)
	return TEN.Flow.DoesSaveGameExist(slot)
end

-- !Name "If game status is..."
-- !Conditional "True"
-- !Description "Check if the game is in specific status.\nNormal game state is controlled in the 'On Loop' event.\nOther states are controlled in the 'On Level End' event."
-- !Section "Game flow"
-- !Arguments "NewLine, Enumeration, [ Normal | New game | Load game | Exit game | Exit to title | Player death | Level complete ], Reason"
LevelFuncs.Engine.Node.GetEndLevelReason = function(reason)
	return LevelFuncs.Engine.Node.GetGameStatus(reason) == Flow.GetGameStatus()
end

-- !Name "If freeze mode is..."
-- !Conditional "True"
-- !Description "Check if the game is in specific freeze mode."
-- !Section "Game flow"
-- !Arguments "Enumeration, 25, [ None | Full | Spectator | Player ], Freeze mode"
LevelFuncs.Engine.Node.TestFreezeMode = function(mode)
	return LevelFuncs.Engine.Node.GetFreezeMode(mode) == Flow.GetFreezeMode()
end

-- !Name "Set freeze mode"
-- !Section "Game flow"
-- !Description "Set current freeze mode. Any values except 'None' will freeze the game in different ways."
-- !Arguments "Enumeration, 25, [ None | Full | Spectator | Player ], Freeze mode"
LevelFuncs.Engine.Node.SetFreezeMode = function(mode)
	TEN.Flow.SetFreezeMode(LevelFuncs.Engine.Node.GetFreezeMode(mode))
end