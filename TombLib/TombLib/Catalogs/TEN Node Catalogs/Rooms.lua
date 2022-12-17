-- !Name "If room is enabled..."
-- !Section "Rooms"
-- !Description "Checks if room is enabled (disabled means it's in flipped state)."
-- !Conditional "True"
-- !Arguments "NewLine, Rooms"

LevelFuncs.Engine.Node.TestRoomEnabled = function(roomName)
	return TEN.Objects.GetRoomByName(roomName):GetActive()
end

-- !Name "If room flag is set..."
-- !Section "Rooms"
-- !Description "Checks if specified room flag is set."
-- !Conditional "True"
-- !Arguments "NewLine, Rooms, 70"
-- !Arguments "Enumeration, [ Water | Quicksand | Skybox | Wind | Cold | Damage | No lensflare ], 30, Flag type"

LevelFuncs.Engine.Node.TestRoomFlag = function(roomName, flag)

	local flagIndex = LevelFuncs.Engine.Node.GetRoomFlag(flag)
	return TEN.Objects.GetRoomByName(roomName):GetFlag(flagIndex)
end

-- !Name "If room reverb type is..."
-- !Section "Rooms"
-- !Description "Checks if room reverb type is equal to specified."
-- !Conditional "True"
-- !Arguments "NewLine, Rooms, 75"
-- !Arguments "Enumeration, [ None | Small | Medium | Large | Pipe ], 25, Reverb type"

LevelFuncs.Engine.Node.TestRoomReverbType = function(roomName, reverbType)

	return (TEN.Objects.GetRoomByName(roomName):GetReverbType() == reverbType)
end

-- !Name "If room tag is present..."
-- !Section "Rooms"
-- !Description "Checks if room has specified tag."
-- !Conditional "True"
-- !Arguments "NewLine, Rooms, 70"
-- !Arguments "String, Tag to search"

LevelFuncs.Engine.Node.TestRoomTag = function(roomName, tag)

	return TEN.Objects.GetRoomByName(roomName):IsTagPresent(tag)
end

-- !Name "Set room flag"
-- !Section "Rooms"
-- !Description "Sets or unsets selected room flag."
-- !Arguments "NewLine, Rooms, 70"
-- !Arguments "Enumeration, [ Water | Quicksand | Skybox | Wind | Cold | Damage | No lensflare ], 25, Flag type"
-- !Arguments "Boolean, , 5"

LevelFuncs.Engine.Node.SetRoomFlag = function(roomName, flag, value)
	
	local flagIndex = LevelFuncs.Engine.Node.GetRoomFlag(flag)
	TEN.Objects.GetRoomByName(roomName):SetFlag(flagIndex, value)
end

-- !Name "Set room reverb type"
-- !Section "Rooms"
-- !Description "Sets selected room reverb type to specified."
-- !Arguments "NewLine, Rooms, 75"
-- !Arguments "Enumeration, [ None | Small | Medium | Large | Pipe ], 25, Reverb type"

LevelFuncs.Engine.Node.SetRoomReverbType = function(roomName, reverbType)
	return TEN.Objects.GetRoomByName(roomName):SetReverbType(reverbType)
end

-- !Name "Toggle flipmap"
-- !Section "Rooms"
-- !Description "Sets specified flipmap number, if it's not set, or unsets it if it is."
-- !Arguments "Numerical, 15, [ 0 | 16 | 0 ]"

LevelFuncs.Engine.Node.ToggleFlipMap = function(flipmapNumber)
	TEN.Misc.FlipMap(flipmapNumber)
end