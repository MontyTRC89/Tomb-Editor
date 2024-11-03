--UI/Hud Nodes by Adngel,DaviDMRR and TrainWreck ; Code references by Lwmte and TEN Nodes.

LevelVars.CustomBars = {}
-- Construct custom bar
LevelFuncs.Engine.Node.ConstructCustomBar = function(barName, startvalue, objectIDbg, spriteIDbg, colorbg, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)

	local dataName	= barName .. "_bar_data"

	if LevelVars.CustomBars[dataName] then
		return
	end

	LevelVars.CustomBars[dataName]			= {}
	LevelVars.CustomBars[dataName].Name		= dataName
	LevelVars.CustomBars[dataName].FixedInterval	= 1/30
	LevelVars.CustomBars[dataName].Progress		= startvalue / 1000 -- Set initial progress from start value (clamped to 0-1000)
	LevelVars.CustomBars[dataName].Interval		= 1 / (30)	-- assuming 30 updates per second
	LevelVars.CustomBars[dataName].ObjectIDbg	= objectIDbg
	LevelVars.CustomBars[dataName].SpriteIDbg	= spriteIDbg
	LevelVars.CustomBars[dataName].ColorBG		= colorbg
	LevelVars.CustomBars[dataName].ObjectIDbar	= objectIDbar
	LevelVars.CustomBars[dataName].SpriteIDbar	= spriteIDbar
	LevelVars.CustomBars[dataName].ColorBar		= colorbar
	LevelVars.CustomBars[dataName].PosX		= posX
	LevelVars.CustomBars[dataName].PosY		= posY
	LevelVars.CustomBars[dataName].ScaleX		= scaleX
	LevelVars.CustomBars[dataName].ScaleY		= scaleY
	LevelVars.CustomBars[dataName].Rot		= rot
	LevelVars.CustomBars[dataName].Visible		= true
	LevelVars.CustomBars[dataName].CurrentAlpha	= 1
	LevelVars.CustomBars[dataName].TargetAlpha	= 255
	LevelVars.CustomBars[dataName].AlignMode	= alignMode
	LevelVars.CustomBars[dataName].ScaleMode	= scaleMode
	LevelVars.CustomBars[dataName].BlendMode	= blendMode
	LevelVars.CustomBars[dataName].OldValue		= startvalue  -- stores the current bar value
	LevelVars.CustomBars[dataName].TargetValue	= startvalue  -- target value to reach
	
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
			
			-- when Alpha reaches 0 set visibility to false
			if CustomBar.CurrentAlpha == 0 then
				CustomBar.Visible = false
			end
			
			--draw bar if alpha is greater than 1 and visibility is true
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
				local myTextString = "Bar Value: " .. barValue
				local myText = DisplayString(myTextString, CustomBar.PosX, CustomBar.PosY-10, CustomBar.ColorBar)
				ShowString(myText,1/30)
			end
		end
	end
end

-- !Name "Draw custom bar"
-- !Section "UI/Hud"
-- !Conditional "False"
-- !Description "Draw a custom bar."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Number, 20, {0}, [ 0 | 1000 | 2 ], Start value of bar"
-- !Arguments "Newline, SpriteSlots, 61, Background sprite sequence object ID,, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC}"
-- !Arguments "Number, 19, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999],{0}"
-- !Arguments "Color, 20, Color of background sprite"
-- !Arguments "Newline, SpriteSlots, 61, Bar sprite sequence object ID,, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC}"
-- !Arguments "Number, 19, [ 0 | 9999 | 0 ], Sprite ID for bar in sprite sequence\nRange[0 to 9999],{3}"
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
-- !Section "UI/Hud"
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
-- !Section "UI/Hud"
-- !Description "Delete a custom bar."
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Bar name"

LevelFuncs.Engine.Node.DeleteCustomBar = function(barName)
	local dataName = barName .. "_bar_data"

	if LevelVars.CustomBars[dataName] then
		LevelVars.CustomBars[dataName] = nil
	end
end

-- !Name "Show/Hide custom bar"
-- !Section "UI/Hud"
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
-- !Section "UI/Hud"
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
-- !Section "UI/Hud"
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

LevelVars.CustomEnemyBars = {}

-- !Name "Draw enemy health bar"
-- !Section "UI/Hud"
-- !Conditional "False"
-- !Description "Draw a health bar for enemy."
-- !Arguments "NewLine, Moveables, Enemy to show health bar for"
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Enemy name" "Numerical, 15, X position, [ 0 | 100 ]" "Numerical, 15, Y position, [ 0 | 100 ]"
-- !Arguments "NewLine, Enumeration, 20, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "Enumeration, 40, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "Numerical, 20, {1}, [ 0 | 9 | 2 | 0.1 ], Scale" "Color, 20, {TEN.Color(255,255,255)}, Text color"
-- !Arguments "Newline, SpriteSlots, 60, Background sprite sequence object ID, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC}"
-- !Arguments "Number, 20, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999],{0}"
-- !Arguments "Color, 20, Color of background sprite"
-- !Arguments "Newline, SpriteSlots, 60, Bar sprite sequence object ID, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC}"
-- !Arguments "Number, 20, [ 0 | 9999 | 0 ], Sprite ID for bar in sprite sequence\nRange[0 to 9999],{3}"
-- !Arguments "Color, 20, Color of bar sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {3}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"


-- Construct custom bar
LevelFuncs.Engine.Node.ConstructEnemyBar = function(object, text, textX, textY, textAlignment, textEffects, textScale, textColor, objectIDbg, spriteIDbg, colorbg, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)
	local dataName = object .. "_enemybar_data"

	if LevelVars.CustomEnemyBars[dataName] then
		return
	end
	
	local enemy = GetMoveableByName(object)
	local enemyHP = enemy:GetHP()
	
	LevelVars.CustomEnemyBars[dataName]			= {}
	LevelVars.CustomEnemyBars[dataName].Name		= dataName
	LevelVars.CustomEnemyBars[dataName].Object		= object
	LevelVars.CustomEnemyBars[dataName].FixedInterval	= 1/3
	LevelVars.CustomEnemyBars[dataName].ObjectIDbg		= objectIDbg
	LevelVars.CustomEnemyBars[dataName].SpriteIDbg		= spriteIDbg
	LevelVars.CustomEnemyBars[dataName].ColorBG		= colorbg
	LevelVars.CustomEnemyBars[dataName].ObjectIDbar		= objectIDbar
	LevelVars.CustomEnemyBars[dataName].SpriteIDbar		= spriteIDbar
	LevelVars.CustomEnemyBars[dataName].ColorBar		= colorbar
	LevelVars.CustomEnemyBars[dataName].PosX		= posX
	LevelVars.CustomEnemyBars[dataName].PosY		= posY
	LevelVars.CustomEnemyBars[dataName].ScaleX		= scaleX
	LevelVars.CustomEnemyBars[dataName].ScaleY		= scaleY
	LevelVars.CustomEnemyBars[dataName].Rot			= rot
	LevelVars.CustomEnemyBars[dataName].Visible		= true
	LevelVars.CustomEnemyBars[dataName].CurrentAlpha	= 1
	LevelVars.CustomEnemyBars[dataName].TargetAlpha		= 255
	LevelVars.CustomEnemyBars[dataName].AlignMode		= alignMode
	LevelVars.CustomEnemyBars[dataName].ScaleMode		= scaleMode
	LevelVars.CustomEnemyBars[dataName].BlendMode		= blendMode
	LevelVars.CustomEnemyBars[dataName].OldValue		= enemyHP  -- start value as the current HP
	LevelVars.CustomEnemyBars[dataName].TargetValue		= enemyHP -- target value as the current HP
	LevelVars.CustomEnemyBars[dataName].TotalHP		= enemyHP -- total health (Changed from Slot HP to allow user to set any health using node when activating the enemy)
	LevelVars.CustomEnemyBars[dataName].Text		= text
	LevelVars.CustomEnemyBars[dataName].TextX		= textX
	LevelVars.CustomEnemyBars[dataName].TextY		= textY
	LevelVars.CustomEnemyBars[dataName].TextAlignment	= textAlignment
	LevelVars.CustomEnemyBars[dataName].TextEffects		= textEffects
	LevelVars.CustomEnemyBars[dataName].TextScale		= textScale
	LevelVars.CustomEnemyBars[dataName].TextColor		= textColor
end

-- Update enemy bars
LevelFuncs.Engine.UpdateEnemyBars = function()

	for _, CustomEBar in pairs(LevelVars.CustomEnemyBars) do

		if CustomEBar ~= nil then
			-- Smoothly transition to target value
			local enemy = GetMoveableByName(CustomEBar.Object)
			local currentHP = enemy:GetHP()
			local totalHP = CustomEBar.TotalHP
			local delta = CustomEBar.FixedInterval

			-- Update the bar's progress (0-1 scale) based on current health and total health
			CustomEBar.Progress = math.max(0, math.min(currentHP / totalHP, 1))
			
			-- Smoothly transition alpha
			if CustomEBar.CurrentAlpha ~= CustomEBar.TargetAlpha then
				local alphaDelta = 50 -- Adjust speed of alpha transition
				if CustomEBar.CurrentAlpha < CustomEBar.TargetAlpha then
					CustomEBar.CurrentAlpha = math.floor(math.min(CustomEBar.CurrentAlpha + alphaDelta, CustomEBar.TargetAlpha))
				else
					CustomEBar.CurrentAlpha = math.floor(math.max(CustomEBar.CurrentAlpha - alphaDelta, CustomEBar.TargetAlpha))
				end
			end

			-- Set up parameters for drawing
			local pos = TEN.Vec2(CustomEBar.PosX, CustomEBar.PosY)
			local scale = TEN.Vec2(CustomEBar.ScaleX, CustomEBar.ScaleY)
			local rot = CustomEBar.Rot
			local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(CustomEBar.AlignMode)
			local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(CustomEBar.ScaleMode)
			local blendID = LevelFuncs.Engine.Node.GetBlendMode(CustomEBar.BlendMode)
			
			-- Adjust color with alpha blending
			local bgColor = Color(CustomEBar.ColorBG.r, CustomEBar.ColorBG.g, CustomEBar.ColorBG.b, CustomEBar.CurrentAlpha)
			local barColor = Color(CustomEBar.ColorBar.r, CustomEBar.ColorBar.g, CustomEBar.ColorBar.b, CustomEBar.CurrentAlpha)
			
			-- When HP reaches 0 then set target alpha = 0
			if currentHP <= 0 then
				CustomEBar.TargetAlpha = 0
			end
			
			-- When Alpha reaches 0 set visibility to false
			if CustomEBar.CurrentAlpha == 0 then
				CustomEBar.Visible = false
				LevelVars.CustomEnemyBars[CustomEBar.Name] = nil
			end

			-- Draw bar if alpha is greater than 1 and visibility is true
			if CustomEBar.Visible and CustomEBar.CurrentAlpha > 0 then
				-- Draw background sprite
				local bgSprite = TEN.DisplaySprite(CustomEBar.ObjectIDbg, CustomEBar.SpriteIDbg, pos, rot, scale, bgColor)
				bgSprite:Draw(0, alignM, scaleM, blendID)

				-- Draw foreground sprite (the bar itself) proportional to Progress
				local barScale = TEN.Vec2(CustomEBar.ScaleX * CustomEBar.Progress, CustomEBar.ScaleY)
				local barSprite = TEN.DisplaySprite(CustomEBar.ObjectIDbar, CustomEBar.SpriteIDbar, pos, rot, barScale, barColor)
				barSprite:Draw(1, alignM, scaleM, blendID)

				-- Draw text (enemy name and health)
				local enemyText = tostring(CustomEBar.Text) --debug text	 .. " (" .. currentHP .. " / " .. totalHP .. ")"
				local myText = LevelFuncs.Engine.Node.GenerateString(enemyText, CustomEBar.TextX, CustomEBar.TextY, CustomEBar.TextScale, CustomEBar.TextAlignment, CustomEBar.TextEffects, CustomEBar.TextColor)
				ShowString(myText, 1/30)
			end
		end
	end
end

TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PRELOOP, LevelFuncs.Engine.UpdateCustomBars)
TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PRELOOP, LevelFuncs.Engine.UpdateEnemyBars)

--Code for Ammo Counter
LevelVars.AC = {}
LevelVars.AC.AmmoName = {
	'pistol_ammo', 'revolver_ammo', 'uzi_ammo', 'shotgun_normal_ammo', 'shotgun_wideshot_ammo', 'hk_ammo',
	'crossbow_normal_ammo', 'crossbow_poison_ammo', 'crossbow_explosive_ammo', 'grenade_launcher_normal_ammo', 
	'grenade_launcher_super_ammo', 'grenade_launcher_flash_ammo', 'harpoon_gun_ammo', 'rocket_launcher_ammo'
}

-- !Name "Show ammo counter"
-- !Conditional "False"
-- !Description "Displays the number of available ammo of the weapon in hand"
-- !Section "UI/Hud"
-- !Arguments "NewLine, Enumeration , 65,[ Counter + ammo name | Use only counter], Display type"
-- !Arguments "Color, 17, Counter text color"
-- !Arguments "Enumeration , 19,[ Left	| Center | Right ], Text alignment"
-- !Arguments "NewLine, Numerical, 17, [ 0 | 100 | 1 | 0.1 ], Counter position x"
-- !Arguments "Numerical, 17, [ 0 | 100 | 1 | 0.1 ], Counter position y"
-- !Arguments "Numerical, 17, {1}, [ 0 | 9 | 2 | 0.1 ], Scale"
-- !Arguments "Enumeration, 49, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "NewLine, Boolean , 33, Show unlimited ammo"
-- !Arguments "Boolean , 33, Swap ammo name & counter position"
-- !Arguments "Boolean , 33, Show sprite"
-- !Arguments "Newline, SpriteSlots, 80, Bar sprite sequence object ID, {TEN.Objects.ObjID.CUSTOM_AMMO_GRAPHIC}"
-- !Arguments "Color, 20, Color of bar sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {3}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"

LevelFuncs.Engine.Node.ShowAmmoCounter = function(displayType, color, alignment, posX, posY, scale, effect, unlimited, swap, sprite, objectIDammo, colorbar, spritePosX, spritePosY, rot, spriteScaleX, spriteScaleY, alignMode, scaleMode, blendMode)
	LevelVars.AC.DisplayType	= displayType
	LevelVars.AC.Color		= color
	LevelVars.AC.Alignment		= alignment
	LevelVars.AC.PosX		= posX
	LevelVars.AC.PosY		= posY
	LevelVars.AC.Scale		= scale
	LevelVars.AC.Effect		= effect
	LevelVars.AC.Unlimited		= unlimited
	LevelVars.AC.Swap		= swap
	LevelVars.AC.sprite		= sprite
	LevelVars.AC.spriteObj		= objectIDammo
	LevelVars.AC.spriteColor	= spriteColor
	LevelVars.AC.spritePosX		= spritePosX
	LevelVars.AC.spritePosY		= spritePosY
	LevelVars.AC.SpriteScaleX	= spriteScaleX
	LevelVars.AC.SpriteScaleY	= spriteScaleY
	LevelVars.AC.Rot		= rot
	LevelVars.AC.AlignMode		= alignMode
	LevelVars.AC.ScaleMode		= scaleMode
	LevelVars.AC.BlendMode		= blendMode
	
	AddCallback(TEN.Logic.CallbackPoint.POSTCONTROLPHASE, LevelFuncs.__ShowAmmoCounter)
	PrintLog('Ammo counter initialized correctly', LogLevel.INFO)
end

-- !Name "Remove ammo counter"
-- !Conditional "False"
-- !Description "Remove the number of available ammo"
-- !Section "UI/Hud"
LevelFuncs.Engine.Node.RemoveAmmoCounter = function()
	RemoveCallback(TEN.Logic.CallbackPoint.POSTCONTROLPHASE, LevelFuncs.__ShowAmmoCounter)
end

LevelFuncs.__ShowAmmoCounter = function()

	local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(LevelVars.AC.AlignMode)
	local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(LevelVars.AC.ScaleMode)
	local blendID = LevelFuncs.Engine.Node.GetBlendMode(LevelVars.AC.BlendMode)
			
	if ((Lara:GetHandStatus() == 2 or Lara:GetHandStatus() == 4) and (Lara:GetWeaponType() ~= 7 and Lara:GetWeaponType() ~= 8)) then
		local number = (Lara:GetAmmoCount() >= 0) and Lara:GetAmmoCount() or (LevelVars.AC.Unlimited and (GetString("unlimited"):gsub(" ", "")):gsub("%%s", "") or "")
		local ammoName = (LevelVars.AC.DisplayType == 0) and ((not LevelVars.AC.Unlimited and Lara:GetAmmoCount() < 0) and "" or GetString(LevelVars.AC.AmmoName[Lara:GetAmmoType()])) or ""
		local ammoText = (ammoName == "") and tostring(number) or ((LevelVars.AC.Swap) and ammoName .. " " .. number or number .. " " .. ammoName)
		if Lara:GetHP() > 0 and ammoText ~= "" then
			local myText = LevelFuncs.Engine.Node.GenerateString(ammoText, LevelVars.AC.PosX, LevelVars.AC.PosY, LevelVars.AC.Scale, LevelVars.AC.Alignment, LevelVars.AC.Effect, LevelVars.AC.Color)
			ShowString(myText, 1/30)
			
			if LevelVars.AC.sprite then
				local ammoSprite = DisplaySprite(LevelVars.AC.spriteObj, Lara:GetAmmoType(), Vec2(LevelVars.AC.spritePosX,LevelVars.AC.spritePosY),0, Vec2(LevelVars.AC.SpriteScaleX,LevelVars.AC.SpriteScaleY))
				ammoSprite:Draw(0,alignM, scaleM, blendID)
			end
		end
	end
end
