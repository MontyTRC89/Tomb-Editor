LevelFuncs.Engine.Node = {}

-- Helper function for value comparisons. Any function which uses
-- CompareOperator arguments should use this helper function for comparison.
LevelFuncs.Engine.Node.CompareValue = function(operand, reference, operator)
	local result = false

	-- Fix Lua-specific treatment of bools as non-numerical values
	if (operand == false) then operand = 0 end;
	if (operand == true) then operand = 1 end;
	if (reference == false) then reference = 0 end;
	if (reference == true) then reference = 1 end;

	if (operator == 0 and operand == reference) then result = true end
	if (operator == 1 and operand ~= reference) then result = true end
	if (operator == 2 and operand < reference) then result = true end
	if (operator == 3 and operand <= reference) then result = true end
	if (operator == 4 and operand > reference) then result = true end
	if (operator == 5 and operand >= reference) then result = true end
	return result
end

-- Helper function for value modification.
LevelFuncs.Engine.Node.ModifyValue = function(operand, reference, operator)
	local result = reference
	if (operator == 0) then result = reference + operand end
	if (operator == 1) then result = reference - operand end
	if (operator == 2) then result = reference * operand end
	if (operator == 3) then result = reference / operand end
	if (operator == 4) then result = operand end
	return result
end

-- Helper function for easy generation of a display string with all parameters set.
LevelFuncs.Engine.Node.GenerateString = function(textOrKey, x, y, scale, alignment, effects, color, alpha)
	local options = {}
	if (effects == 1 or effects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
	if (effects == 2 or effects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.BLINK) end
	if (alignment == 1) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
	if (alignment == 2) then table.insert(options, TEN.Strings.DisplayStringOption.RIGHT) end
	if (alpha ~= nil) then color.a = alpha * 255 end
	local rX, rY = TEN.Util.PercentToScreen(x, y)
	return TEN.Strings.DisplayString(textOrKey, TEN.Vec2(rX, rY), scale, color, TEN.Flow.IsStringPresent(textOrKey), options)
end

-- Helper function for easy generation text option for display string.
LevelFuncs.Engine.Node.GeneratesTextOption = function (alignment, effects)
	local options = {}
	if (effects == 1 or effects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
	if (effects == 2 or effects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.BLINK) end
	if (alignment == 1) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
	if (alignment == 2) then table.insert(options, TEN.Strings.DisplayStringOption.RIGHT) end
	return options
end

-- Helper function to split string using specified delimiter.
LevelFuncs.Engine.Node.SplitString = function(inputStr, delimiter)
	if inputStr == nil then
		inputStr = "%s"
	end

	local t = {}
	for str in string.gmatch(inputStr, "([^" .. delimiter .. "]+)") do
		table.insert(t, str)
	end

	return t
end

LevelFuncs.Engine.Node.StringIsEmpty = function(str)
	return (str == nil or str == '')
end

-- Wrap angle value around 360
LevelFuncs.Engine.Node.WrapRotation = function(source, value)
	if (value == 0) then
		return source
	end

	local rot = source + value
	if (rot > 360) then
		rot = rot - 360
	elseif (rot < 0) then
		rot = 360 + rot
	end
	return rot
end

LevelFuncs.Engine.Node.Smoothstep = function(source)
	source = math.max(0, math.min(1, source))
	return ((source ^ 3) * (source * (source * 6 - 15) + 10))
end

LevelFuncs.Engine.Node.Lerp = function(val1, val2, factor)
	return val1 * (1 - factor) + val2 * factor
end

-- Convert UI enum to room flag ID enum
LevelFuncs.Engine.Node.GetRoomFlag = function(value)
	local roomFlagID =
	{
		[0] = Objects.RoomFlagID.WATER,
		[1] = Objects.RoomFlagID.QUICKSAND,
		[2] = Objects.RoomFlagID.SKYBOX,
		[3] = Objects.RoomFlagID.WIND,
		[4] = Objects.RoomFlagID.COLD,
		[5] = Objects.RoomFlagID.DAMAGE,
		[6] = Objects.RoomFlagID.NOLENSFLARE,
	}
	return roomFlagID[value]
end

LevelFuncs.Engine.Node.GetSoundTrackType = function(value)
	local SoundTrackType =
	{
		[0] = Sound.SoundTrackType.ONESHOT,
		[1] = Sound.SoundTrackType.LOOPED,
		[2] = Sound.SoundTrackType.VOICE,
	}
	return SoundTrackType[value]
end

LevelFuncs.Engine.Node.GetBlendMode = function(index)
	local blendID =
	{
		[0] = TEN.Effects.BlendID.OPAQUE,
		[1] = TEN.Effects.BlendID.ALPHATEST,
		[2] = TEN.Effects.BlendID.ADDITIVE,
		[3] = TEN.Effects.BlendID.NOZTEST,
		[4] = TEN.Effects.BlendID.SUBTRACTIVE,
		[5] = TEN.Effects.BlendID.WIREFRAME,
		[6] = TEN.Effects.BlendID.EXCLUDE,
		[7] = TEN.Effects.BlendID.SCREEN,
		[8] = TEN.Effects.BlendID.LIGHTEN,
		[9] = TEN.Effects.BlendID.ALPHABLEND
	}
	return blendID[index]
end

LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode = function(index)
	local displaySpriteAlignMode =
	{
		[0] = TEN.View.AlignMode.CENTER,
		[1] = TEN.View.AlignMode.CENTER_TOP,
		[2] = TEN.View.AlignMode.CENTER_BOTTOM,
		[3] = TEN.View.AlignMode.CENTER_LEFT,
		[4] = TEN.View.AlignMode.CENTER_RIGHT,
		[5] = TEN.View.AlignMode.TOP_LEFT,
		[6] = TEN.View.AlignMode.TOP_RIGHT,
		[7] = TEN.View.AlignMode.BOTTOM_LEFT,
		[8] = TEN.View.AlignMode.BOTTOM_RIGHT
	}
	return displaySpriteAlignMode[index]
end

LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode = function(index)
	local displaySpriteScaleMode =
	{
		[0] = TEN.View.ScaleMode.FIT,
		[1] = TEN.View.ScaleMode.FILL,
		[2] = TEN.View.ScaleMode.STRETCH
	}
	return displaySpriteScaleMode[index]
end

LevelFuncs.Engine.Node.GetGameStatus = function(index)
	local gameStatus =
	{
		[0] = Flow.GameStatus.NORMAL,
		[1] = Flow.GameStatus.NEW_GAME,
		[2] = Flow.GameStatus.LOAD_GAME,
		[3] = Flow.GameStatus.EXIT_GAME,
		[4] = Flow.GameStatus.EXIT_TO_TITLE,
		[5] = Flow.GameStatus.LARA_DEAD,
		[6] = Flow.GameStatus.LEVEL_COMPLETE
	}
	return gameStatus[index]
end

LevelFuncs.Engine.Node.GetFreezeMode = function(index)
	local freezeMode =
	{
		[0] = Flow.FreezeMode.NONE,
		[1] = Flow.FreezeMode.FULL,
		[2] = Flow.FreezeMode.SPECTATOR,
		[3] = Flow.FreezeMode.PLAYER
	}
	return freezeMode[index]
end

LevelFuncs.Engine.Node.SetPostProcessMode = function(index)
	local postProcessMode =
	{
		[0] = TEN.View.PostProcessMode.NONE,
		[1] = TEN.View.PostProcessMode.MONOCHROME,
		[2] = TEN.View.PostProcessMode.NEGATIVE,
		[3] = TEN.View.PostProcessMode.EXCLUSION,
	}
	return postProcessMode[index]
end

LevelFuncs.Engine.Node.SetInteractionHighlightType = function(index)
	local interactionIconType =
	{
		[0] = TEN.Objects.InteractionType.PICKUP,
		[1] = TEN.Objects.InteractionType.TALK,
		[2] = TEN.Objects.InteractionType.USE,
	}	
	return interactionIconType[index]
end

local ladderStates = 
{
	10, -- hang state
	55, -- climbing up
	56, -- idle on ladder
	57, -- ladder up
	58, -- ladder left
	59, -- ladder down
	60, -- ladder right
	61, -- climbing down
	107, -- shimmy inner left
	105, -- crouch turn left
	106, -- crouch turn right
	107, -- shimmy outer left
	108, -- shimmy outer right
	109, -- shimmy inner left
	110, -- shimmy inner right
	138, -- ladder to crouch
}

local crawlingStates = 
{
	71, -- crouch idle
	72, -- crouch roll
	80, -- crawling idle
	81, -- crawl forward
	84, -- crawling turn left
	85, -- crawling turn right
	86, -- crawling backwards
	71, -- crawling idle
	72, -- crouch roll
	105, -- crouch turn left
	106, -- crouch turn right
	160, -- crawl step up
	161, -- crawl step down
	167, -- 1 step crouch vault
	168, -- 2 step crouch vault
	169, -- 3 step crouch vault
	171, -- crouch turn 180
	172, -- crawl turn 180
}

local horizontalBarStates =
{
	128, -- horizontal bar swing
	129, -- horizontal bar leap
}

local monkeySwingStates =
{
	75, -- monkey swing idle
	76, -- monkey swing forward
	77, -- monkey swing shimmy left
	78, -- monkey swing shimmy right
	79, -- monkey swing turn 180
	82, -- monkey turn left
	83, -- monkey turn right
}

local poleVaultStates =
{
	99, -- pole idle
	100, -- pole up
	101, -- pole down
	102, -- pole turn clockwise
	103, -- pole turn counterclockwise
}

local ropeSwingStates =
{
	90, -- rope turn clockwise
	91, -- rope turn counterclockwise
	111, -- rope idle
	112, -- rope up
	113, -- rope down
	114, -- rope swing
	115, -- rope unknown
}

local swimStates =
{
	13, -- swimming idle
	17, -- swim forward
	18, -- swim intertia
	66  -- underwater roll
}

local tightropeStates =
{
	119, -- Tightrope idle 
	120, -- Tightrope turn 180
	121, -- Tightrope walk
	122, -- Tightrope unbalance left
	123, -- Tightrope unbalance right
	124, -- Tightrope enter
	125, -- Tightrope dismount
}

local function IsStateInList(state, states)
	for _, expectedState in ipairs(states) do
		if state == expectedState then
			return true
		end
	end

	return false
end

local LaraTraversalMode = 
{
	CLIMB = 0,
	CRAWL = 1,
	HORIZONTAL_BAR = 2,
	MONKEY_SWING = 3,
	POLE_VAULT = 4,
	SWIMMING = 5,
	TIGHTROPE = 6,
}

local traversalModeTests = 
{
	[LaraTraversalMode.CLIMB] = function(state)
		return IsStateInList(state, ladderStates)
	end,

	[LaraTraversalMode.CRAWL] = function(state)
		return IsStateInList(state, crawlingStates)
	end,

	[LaraTraversalMode.HORIZONTAL_BAR] = function(state)
		return IsStateInList(state, horizontalBarStates)
	end,

	[LaraTraversalMode.MONKEY_SWING] = function(state)
		return IsStateInList(state, monkeySwingStates)
	end,

	[LaraTraversalMode.POLE_VAULT] = function(state)
		return IsStateInList(state, poleVaultStates)
	end,

	[LaraTraversalMode.ROPE_SWING] = function(state)
		return IsStateInList(state, ropeSwingStates)
	end,
	
	[LaraTraversalMode.SWIMMING] = function(state)
		if TEN.Objects.Lara:GetWaterStatus() == true then
			return IsStateInList(state, swimStates)
		else
			return false
		end
	end,
	
	[LaraTraversalMode.TIGHTROPE] = function(state)
		return IsStateInList(state, tightropeStates)
	end,
}

LevelFuncs.Engine.Node.TestLaraTraversalMode = function(mode, state)
	local traversalTest = traversalModeTests[mode]
	
	if (traversalTest == nil) then
		return false
	end

	return traversalTest(state)
end