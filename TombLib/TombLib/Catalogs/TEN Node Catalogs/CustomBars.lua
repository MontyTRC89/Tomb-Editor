--Custom Bar Nodes by TrainWreck and DaviDMRR and Adngel; Code references by Lwmte and TEN Nodes.

LevelVars.CustomBars = {}
-- Construct custom bar
LevelFuncs.Engine.Node.ConstructCustomBar = function(barName, startvalue, objectIDbg, spriteIDbg, colorbg, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)

	local dataName	= barName .. "_bar_data"

	if LevelVars.CustomBars[dataName] then
		return
	end

	LevelVars.CustomBars[dataName]				= {}
	LevelVars.CustomBars[dataName].Name			= dataName
	LevelVars.CustomBars[dataName].FixedInterval		= 1/30
	LevelVars.CustomBars[dataName].Progress			= startvalue / 1000 -- Set initial progress from start value (clamped to 0-1000)
	LevelVars.CustomBars[dataName].Interval			= 1 / (30)	-- assuming 30 updates per second
	LevelVars.CustomBars[dataName].ObjectIDbg		= objectIDbg
	LevelVars.CustomBars[dataName].SpriteIDbg		= spriteIDbg
	LevelVars.CustomBars[dataName].ColorBG			= colorbg
	LevelVars.CustomBars[dataName].ObjectIDbar		= objectIDbar
	LevelVars.CustomBars[dataName].SpriteIDbar		= spriteIDbar
	LevelVars.CustomBars[dataName].ColorBar			= colorbar
	LevelVars.CustomBars[dataName].PosX			= posX
	LevelVars.CustomBars[dataName].PosY			= posY
	LevelVars.CustomBars[dataName].ScaleX			= scaleX
	LevelVars.CustomBars[dataName].ScaleY			= scaleY
	LevelVars.CustomBars[dataName].Rot			= rot
	LevelVars.CustomBars[dataName].Visible			= true
	LevelVars.CustomBars[dataName].CurrentAlpha		= 1
	LevelVars.CustomBars[dataName].TargetAlpha		= 255
	LevelVars.CustomBars[dataName].AlignMode		= alignMode
	LevelVars.CustomBars[dataName].ScaleMode		= scaleMode
	LevelVars.CustomBars[dataName].BlendMode		= blendMode
	LevelVars.CustomBars[dataName].OldValue			= startvalue  -- stores the current bar value
	LevelVars.CustomBars[dataName].TargetValue		= startvalue  -- target value to reach
	
end



-- Transform object parameter using previously saved timed transform data
LevelFuncs.Engine.UpdateCustomBars = function()

	for _, CustomBar in pairs (LevelVars.CustomBars) do

		if CustomBar ~= nil then
			-- Smoothly transition to target value
			local currentValue = CustomBar.OldValue or 0
			local targetValue = CustomBar.TargetValue or 0
			local delta = CustomBar.FixedInterval

			if currentValue ~= targetValue then
				-- Update current value by delta (increment or decrement)
				if currentValue < targetValue then
					currentValue = math.min(currentValue + delta, targetValue)
				else
					currentValue = math.max(currentValue + delta, targetValue)
				end

				-- Update the bar's progress (0-1 scale)
				CustomBar.OldValue = currentValue
				CustomBar.Progress = currentValue / 1000
			end
			-- Smoothly transition alpha
			if CustomBar.CurrentAlpha ~= CustomBar.TargetAlpha then
				local alphaDelta = 50 -- Adjust speed of alpha transition
				if CustomBar.CurrentAlpha < CustomBar.TargetAlpha then
					CustomBar.CurrentAlpha = math.floor(math.min(CustomBar.CurrentAlpha + alphaDelta, CustomBar.TargetAlpha))
				else
					CustomBar.CurrentAlpha = math.floor(math.max(CustomBar.CurrentAlpha - alphaDelta, CustomBar.TargetAlpha))
				end
			end
			-- Set up parameters for drawing
			local pos = TEN.Vec2(CustomBar.PosX, CustomBar.PosY)
			local scale = TEN.Vec2(CustomBar.ScaleX, CustomBar.ScaleY)
			local rot = CustomBar.Rot
			local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(CustomBar.AlignMode)
			local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(CustomBar.ScaleMode)
			local blendID = LevelFuncs.Engine.Node.GetBlendMode(CustomBar.BlendMode)
			
			-- Adjust color with alpha blending
			local bgColor =Color(CustomBar.ColorBG.r,CustomBar.ColorBG.g,CustomBar.ColorBG.b,CustomBar.CurrentAlpha)
			local barColor = Color(CustomBar.ColorBar.r,CustomBar.ColorBar.g,CustomBar.ColorBar.b,CustomBar.CurrentAlpha)
			
			if CustomBar.CurrentAlpha == 0 then
				LevelVars.CustomBars[dataName].Visible = false
			end
			
			if CustomBar.Visible and CustomBar.CurrentAlpha > 0 then
				-- Draw background sprite
				local bgSprite = TEN.DisplaySprite(CustomBar.ObjectIDbg, CustomBar.SpriteIDbg, pos, rot, scale, bgColor)
				bgSprite:Draw(0, alignM, scaleM, blendID)

				-- Draw foreground sprite (the bar itself) proportional to Progress
				local barScale = TEN.Vec2(CustomBar.ScaleX * CustomBar.Progress, CustomBar.ScaleY)
				local barSprite = TEN.DisplaySprite(CustomBar.ObjectIDbar, CustomBar.SpriteIDbar, pos, rot, barScale, barColor)
				barSprite:Draw(1, alignM, scaleM, blendID)
				
				--- Debugging code
				local barValue = math.floor(CustomBar.OldValue)
				myTextString = "Bar Value: " .. barValue
				myText = DisplayString(myTextString, CustomBar.PosX, CustomBar.PosY-10, CustomBar.ColorBar)
				ShowString(myText,1/30)
			end
		end
	end
end

-- !Name "Draw custom bar"
-- !Section "Custom bars"
-- !Conditional "False"
-- !Description "Draw a custom bar."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Number, 20, {0}, [ 0 | 1000 | 2 ], Start value of bar"
-- !Arguments "Newline, SpriteSlots, 61, Background sprite sequence object ID,{TEN.Objects.ObjID.BAR_BORDER_GRAPHIC}"
-- !Arguments "Number, 19, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, 20, Color of background sprite"
-- !Arguments "Newline, SpriteSlots, 61, Bar sprite sequence object ID, {TEN.Objects.ObjID.SFX_BAR_TEXTURE}"
-- !Arguments "Number, 19, [ 0 | 9999 | 0 ], Sprite ID for bar in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, 20, Color of bar sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {3}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"

LevelFuncs.Engine.Node.DisplayBars = function(barName, startvalue, objectIDbg, spriteIDbg, colorbg, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)
-- Wrap another node function call into do/end to prevent wrong parsing
	do LevelFuncs.Engine.Node.ConstructCustomBar(barName, startvalue, objectIDbg, spriteIDbg, colorbg, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)
	end
end

-- !Name "Change custom bar value over time"
-- !Section "Custom bars"
-- !Description "Change bar value over time."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Numerical, [ -1000 | 1000 | 2 | 1 | 1 ], {1}, 25, Value"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 25, Time (in seconds)"

LevelFuncs.Engine.Node.ChangeBarValueOverTimespan = function(barName, value, time)

	local dataName = barName .. "_bar_data"

	-- Check if bar data and timer exist
	if LevelVars.CustomBars[dataName] then

		-- Get the current target value or old value if no target value exists
		local currentValue = LevelVars.CustomBars[dataName].OldValue
		local currentTarget = LevelVars.CustomBars[dataName].TargetValue or currentValue

		-- Calculate new target value by adding the relative 'value' and clamp between 0 and 1000
		local newTargetValue = math.max(0, math.min(1000, currentTarget + value))

		-- Set the new target value
		LevelVars.CustomBars[dataName].TargetValue = newTargetValue

		-- Calculate total frames based on time and FPS (30 FPS)
		local totalFrames = time * 30

		-- Calculate the fixed interval for the entire transition
		LevelVars.CustomBars[dataName].FixedInterval = (newTargetValue - currentValue) / totalFrames
	end
end

-- !Name "Delete custom bar"
-- !Section "Custom bars"
-- !Description "Delete a custom bar."
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Bar name"

LevelFuncs.Engine.Node.DeleteCustomBar = function(barName)
	local dataName = barName .. "_bar_data"

	if LevelVars.CustomBars[dataName] then
		LevelVars.CustomBars[dataName] = nil
	end
end

-- !Name "Show/Hide custom bar"
-- !Section "Custom bars"
-- !Description "Hides or Shows the custom bar."
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Bar name"
-- !Arguments "Enumeration, 30, [ Not visible | Visible ], Visibility"

LevelFuncs.Engine.Node.BarVisibility = function(barName, value)
	local dataName = barName .. "_bar_data"

	if LevelVars.CustomBars[dataName] then
		if value == 0 then
			LevelVars.CustomBars[dataName].TargetAlpha = 0
		elseif value == 1 then
			LevelVars.CustomBars[dataName].TargetAlpha = 255
			LevelVars.CustomBars[dataName].Visible = true
		end
	end
end

-- !Name "If custom bar is visible..."
-- !Section "Custom bars"
-- !Description "Checks if specified custom bar is visible."
-- !Conditional "True"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Bar name"

LevelFuncs.Engine.Node.TestBarVisibility = function(barName)
	
	local dataName = barName .. "_bar_data"
	if LevelVars.CustomBars[dataName] then
		if LevelVars.CustomBars[dataName].Visible then
			return true
		else
			return false
		end
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
	
	if (LevelVars.CustomBars[dataName] == nil) then
		return LevelFuncs.Engine.Node.CompareValue(0, value, operator)
	else
		return LevelFuncs.Engine.Node.CompareValue(LevelVars.CustomBars[dataName].OldValue, value, operator)
	end
	
end

TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PRELOOP, LevelFuncs.Engine.UpdateCustomBars)
