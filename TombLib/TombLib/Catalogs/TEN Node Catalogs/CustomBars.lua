--Custom Bar Nodes by TrainWreck; Code references by Lwmte and TEN Nodes.

local Timer = require("Engine.Timer")

-- Construct custom bar
LevelFuncs.Engine.Node.ConstructCustomBar = function(barName, startvalue, objectIDbg, spriteIDbg, colorbg, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)

	local dataName	= barName .. "_bar_data"

	if LevelVars[dataName] and LevelVars[dataName].Timer then
		if Timer.Get(dataName):IsActive() then
			return
		end
		Timer.Delete(LevelVars[dataName].Name)
		LevelVars[dataName] = nil
	end

	LevelVars[dataName] = {}
	LevelVars[dataName].Name			= dataName
	LevelVars[dataName].FixedInterval 	= 1/30
	LevelVars[dataName].Progress		= startvalue / 1000 -- Set initial progress from start value (clamped to 0-1000)
	LevelVars[dataName].Interval		= 1 / (30)	-- assuming 30 updates per second
	LevelVars[dataName].ObjectIDbg		= objectIDbg
	LevelVars[dataName].SpriteIDbg		= spriteIDbg
	LevelVars[dataName].ColorBG			= colorbg
	LevelVars[dataName].ObjectIDbar 	= objectIDbar
	LevelVars[dataName].SpriteIDbar 	= spriteIDbar
	LevelVars[dataName].ColorBar		= colorbar
	LevelVars[dataName].PosX			= posX
	LevelVars[dataName].PosY			= posY
	LevelVars[dataName].ScaleX			= scaleX
	LevelVars[dataName].ScaleY			= scaleY
	LevelVars[dataName].Rot				= rot
	LevelVars[dataName].AlignMode		= alignMode
	LevelVars[dataName].ScaleMode		= scaleMode
	LevelVars[dataName].BlendMode		= blendMode
	LevelVars[dataName].OldValue		= startvalue  -- stores the current bar value
	LevelVars[dataName].TargetValue 	= startvalue  -- target value to reach
	LevelVars[dataName].Timer			= Timer.Create(dataName, 1 / 30, true, false, LevelFuncs.Engine.Node.DisplayBar, dataName)
	
	LevelVars[dataName].Timer:Start()
	
end



-- Transform object parameter using previously saved timed transform data
LevelFuncs.Engine.Node.DisplayBar = function(dataName)

-- Smoothly transition to target value
	local currentValue = LevelVars[dataName].OldValue or 0
	local targetValue = LevelVars[dataName].TargetValue or 0
	local delta = LevelVars[dataName].FixedInterval

	if currentValue ~= targetValue then
		-- Update current value by delta (increment or decrement)
		if currentValue < targetValue then
			currentValue = math.min(currentValue + delta, targetValue)
		else
			currentValue = math.max(currentValue + delta, targetValue)
		end

		-- Update the bar's progress (0-1 scale)
		LevelVars[dataName].OldValue = currentValue
		LevelVars[dataName].Progress = currentValue / 1000
	end

	-- Set up parameters for drawing
	local pos = TEN.Vec2(LevelVars[dataName].PosX, LevelVars[dataName].PosY)
	local scale = TEN.Vec2(LevelVars[dataName].ScaleX, LevelVars[dataName].ScaleY)
	local rot = LevelVars[dataName].Rot
	local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(LevelVars[dataName].AlignMode)
	local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(LevelVars[dataName].ScaleMode)
	local blendID = LevelFuncs.Engine.Node.GetBlendMode(LevelVars[dataName].BlendMode)

	-- Draw background sprite
	local bgSprite = TEN.DisplaySprite(LevelVars[dataName].ObjectIDbg, LevelVars[dataName].SpriteIDbg, pos, rot, scale, LevelVars[dataName].ColorBG)
	bgSprite:Draw(0, alignM, scaleM, blendID)

	-- Draw foreground sprite (the bar itself) proportional to Progress
	local barScale = TEN.Vec2(LevelVars[dataName].ScaleX * LevelVars[dataName].Progress, LevelVars[dataName].ScaleY)
	local barSprite = TEN.DisplaySprite(LevelVars[dataName].ObjectIDbar, LevelVars[dataName].SpriteIDbar, pos, rot, barScale, LevelVars[dataName].ColorBar)
	barSprite:Draw(1, alignM, scaleM, blendID)
	
	local rotations = LevelVars[dataName].OldValue
	myTextString = "Bar Value: " .. rotations
	myText = DisplayString(myTextString, 100, 200, Color.new(64,250,60))
	ShowString(myText,0.1)
	
end

-- !Name "Draw custom bar"
-- !Section "Custom bars"
-- !Conditional "False"
-- !Description "Draw a custom bar."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Start value of bar"
-- !Arguments "Newline, SpriteSlots, 61, Background sprite sequence object ID"
-- !Arguments "Number, 19, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, 20, Color of background sprite"
-- !Arguments "Newline, SpriteSlots, 61, Bar sprite sequence object ID"
-- !Arguments "Number, 19, [ 0 | 9999 | 0 ], Sprite ID for bar in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, 20, Color of bar sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], Blend mode"

LevelFuncs.Engine.Node.DisplayBars = function(barName, startvalue, objectIDbg, spriteIDbg, colorbg, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)
-- Wrap another node function call into do/end to prevent wrong parsing
	do LevelFuncs.Engine.Node.ConstructCustomBar(barName, startvalue, objectIDbg, spriteIDbg, colorbg, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)
	end
end

-- !Name "Change bar value"
-- !Section "Custom bars"
-- !Description "Change bar value over time."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Numerical, [ -1000 | 1000 | 2 | 1 | 1 ], {1}, 20, Value"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 20, Time (in seconds)"

LevelFuncs.Engine.Node.ChangeBarValueOverTimespan = function(barName, value, time)

	local dataName = barName .. "_bar_data"

	-- Check if bar data and timer exist
	if LevelVars[dataName] and LevelVars[dataName].Timer then

		-- Get the current target value or old value if no target value exists
		local currentValue = LevelVars[dataName].OldValue
		local currentTarget = LevelVars[dataName].TargetValue or currentValue

		-- Calculate new target value by adding the relative 'value' and clamp between 0 and 1000
		local newTargetValue = math.max(0, math.min(1000, currentTarget + value))

		-- Set the new target value
		LevelVars[dataName].TargetValue = newTargetValue

		-- Calculate total frames based on time and FPS (30 FPS)
		local totalFrames = time * 30

		-- Calculate the fixed interval for the entire transition
		LevelVars[dataName].FixedInterval = (newTargetValue - currentValue) / totalFrames
	end
end

-- !Name "Delete custom bar"
-- !Section "Custom bars"
-- !Description "Delete a custom bar."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"

LevelFuncs.Engine.Node.DeleteCustomBar = function(barName)
	local dataName = barName .. "_bar_data"

	if LevelVars[dataName] and LevelVars[dataName].Timer and Timer.Get(dataName):IsActive() then
		Timer.Delete(LevelVars[dataName].Name)
		LevelVars[dataName] = nil
	end
end

-- !Name "If custom bar value is..."
-- !Section "Custom bars"
-- !Description "Checks if specified bar value complies to specified compare function."
-- !Conditional "True"
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "CompareOperator, 25" "Numerical, 25, [ 0 | 1000 | 2 ], Bar value"

LevelFuncs.Engine.Node.TestBarValue = function(barName, operator, value)
	
	local dataName = barName .. "_bar_data"
	
	if (LevelVars[dataName] == nil) then
		return LevelFuncs.Engine.Node.CompareValue(0, value, operator)
	else
		return LevelFuncs.Engine.Node.CompareValue(LevelVars[dataName].OldValue, value, operator)
	end
	
end