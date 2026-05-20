local ladderStates = 
{
	10, -- hang state
    19, -- grabbing (pulling up)
	55, -- climbing up
	56, -- idle on ladder
	57, -- ladder up
	58, -- ladder left
	59, -- ladder down
	60, -- ladder right
	61, -- climbing down
    88, -- climb off laddder
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
	35, -- dive
	40, -- use switch
	42, -- use key
	43, -- use puzzle
	44, -- underwater death
	66, -- underwater roll
	67, -- pickup flare
	89, -- misc control (opening door, trapdoor, kick)
	93, -- trapdoor floor open
	104, -- using pulley
	189, -- remove puzzle
	198, -- ungrab pulley
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
    ROPE_SWING = 5,
	SWIM = 6,
	TIGHTROPE = 7,	
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
	
	[LaraTraversalMode.SWIM] = function(state)
        return IsStateInList(state, swimStates)
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

-- !Name "If Lara traversal state is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks Lara's current traversal state."
-- !Arguments "Enumeration, [ Climb | Crawl | Horizontal Bar | Monkey Swing | Pole Vault | Rope Swing | Swim | Tightrope ], 30, Traversal state to test."

LevelFuncs.Engine.Node.TestLaraTraversalState = function(mode)
	return LevelFuncs.Engine.Node.TestLaraTraversalMode(mode, TEN.Objects.Lara:GetState())
end