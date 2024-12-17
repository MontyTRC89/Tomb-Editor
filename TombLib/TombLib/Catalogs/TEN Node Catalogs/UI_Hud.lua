--UI/Hud Nodes by Adngel,DaviDMRR and TrainWreck ; Code references by Lwmte and TEN Nodes.
local CustomBar = require("Engine.CustomBar")

-- !Name "Create basic custom bar"
-- !Section "UI/Hud"
-- !Conditional "False"
-- !Description "Creates a bar with max value 1000."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Number, [ 0 | 65535 | 2 ], {0}, 25, Start value of bar"
-- !Arguments "Number, [ 0 | 65535 | 2 ], {1000}, 25, Max value of bar"
-- !Arguments "Newline, Number, 33, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 33, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Color, 34, {TEN.Color(255,255,255)}, Color of bar sprite"

LevelFuncs.Engine.Node.BasicBars = function(barName, startvalue, maxValue, posX, posY, colorbar)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'Create basic custom bar' node. No bar name provided", LogLevel.ERROR)
    else
		local alignMode = TEN.View.AlignMode.CENTER_LEFT
		local scaleMode = TEN.View.ScaleMode.FIT
		local blendMode = TEN.Effects.BlendID.ALPHATEST
		CustomBar.Create(barName, startvalue, maxValue, 
		TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC, 0, Color(255,255,255), TEN.Vec2(posX, posY), 0, TEN.Vec2(19.05, 19.1), alignMode, scaleMode, blendMode, 
		TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC, 1, colorbar, TEN.Vec2(posX+0.15, posY), 0, TEN.Vec2(18.7, 18.48), alignMode, scaleMode, blendMode,
								barName, TEN.Vec2(posX, posY), {}, 1, colorbar, true, 50, false, 0.25)
	end
end

-- !Name "Create advanced custom bar"
-- !Section "UI/Hud"
-- !Conditional "False"
-- !Description "Creates a custom bar."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Number, [ 0 | 65535 | 2 ], {0}, 25, Start value of bar"
-- !Arguments "Number, [ 0 | 65535 | 2 ], {1000}, 25, Max value of bar"
-- !Arguments "Newline, SpriteSlots, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC}, 61, Background sprite sequence object ID"
-- !Arguments "Number, [ 0 | 9999 | 0 ], {0}, 19, Sprite ID for background in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, {TEN.Color(255,255,255)}, 20, Color of background sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, [ -1000 | 1000 | 2 ], 20, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, [ 0 | 360 | 2 ], 20, Rotation\nRange [0 to 360]"
-- !Arguments "Number, [ 0 | 1000 | 2 ], {20}, 20, Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, [ 0 | 1000 | 2 ], {20}, 20, Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "Newline, SpriteSlots, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC}, 61, Bar sprite sequence object ID"
-- !Arguments "Number, [ 0 | 9999 | 0 ], {1}, 19, Sprite ID for bar in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, {TEN.Color(255,255,255)}, 20, Color of bar sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, [ -1000 | 1000 | 2 ], 20, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, [ 0 | 360 | 2 ], 20, Rotation\nRange [0 to 360]"
-- !Arguments "Number, [ 0 | 1000 | 2 ], {20}, 20, Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, [ 0 | 1000 | 2 ], {20}, 20, Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {3}, 35, Align mode"
-- !Arguments "Enumeration, [ Fit | Fill | Stretch ], 22, Scale mode"
-- !Arguments "Enumeration, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, 28, Blend mode"

LevelFuncs.Engine.Node.DisplayBars = function(barName, startvalue, maxValue, objectIDbg, spriteIDbg, colorbg, posXBG, posYBG, rotBG, scaleXBG, scaleYBG, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'Create advanced custom bar' node. No bar name provided", LogLevel.ERROR)
    else
		local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    	local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    	local blendM = LevelFuncs.Engine.Node.GetBlendMode(blendMode)
		CustomBar.Create(barName, startvalue, maxValue,
					objectIDbg, spriteIDbg, colorbg, TEN.Vec2(posXBG, posYBG), rotBG, TEN.Vec2(scaleXBG, scaleYBG), alignM, scaleM, blendM, 
					objectIDbar, spriteIDbar, colorbar, TEN.Vec2(posX, posY), rot, TEN.Vec2(scaleX, scaleY), alignMode, scaleMode, blendM,
					barName, TEN.Vec2(posX, posY), {}, 1, colorbar, true, 50, false, 0.25)
	end
end
-- !Name "Change custom bar value over time"
-- !Section "UI/Hud"
-- !Description "Change bar value over time."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Numerical, [ -1000 | 1000 | 2 | 1 | 1 ], {1}, 25, Value"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 25, Time (in seconds)"

LevelFuncs.Engine.Node.ChangeBarValueOverTimespan = function(barName, value, time)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'Change custom bar value over time' node. No bar name provided", LogLevel.ERROR)
    else
		local dataName	= barName .. "_bar_data"
		if LevelVars.Engine.CustomBars.bars[dataName] then
			local bar = CustomBar.Get(barName)
			bar:ChangeBarValueOverTimespan(value, time)
		end
	end
end

-- !Name "Set custom bar value over time"
-- !Section "UI/Hud"
-- !Description "Set bar value over time."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Numerical, [ -1000 | 1000 | 2 | 1 | 1 ], {1}, 25, Value"
-- !Arguments "Numerical, [ 0.1 | 65535 | 2 | 0.1 | 1 ], {1}, 25, Time (in seconds)"

LevelFuncs.Engine.Node.SetBarValueOverTimespan = function(barName, value, time)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'Set custom bar value over time' node. No bar name provided", LogLevel.ERROR)
    else
		local dataName	= barName .. "_bar_data"
		if LevelVars.Engine.CustomBars.bars[dataName] then
			local bar = CustomBar.Get(barName)
			bar:SetBarValue(value, time)
		end
	end
end

-- !Name "Delete custom bar"
-- !Section "UI/Hud"
-- !Description "Delete a custom bar."
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Bar name"

LevelFuncs.Engine.Node.DeleteCustomBar = function(barName)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'Delete custom bar' node. No bar name provided", LogLevel.ERROR)
    else
		local dataName	= barName .. "_bar_data"
		if LevelVars.Engine.CustomBars.bars[dataName] then
			CustomBar.Delete(barName)
		end
	end
end

-- !Name "Show/Hide custom bar"
-- !Section "UI/Hud"
-- !Description "Hides or Shows the custom bar."
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Bar name"
-- !Arguments "Enumeration, 30, [ Not visible | Visible ], Visibility"

LevelFuncs.Engine.Node.BarVisibility = function(barName, value)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'Show/Hide custom bar' node. No bar name provided", LogLevel.ERROR)
    else
		local dataName	= barName .. "_bar_data"
		if LevelVars.Engine.CustomBars.bars[dataName] then
			local bar = CustomBar.Get(barName)
			
			if value == 0 then
				bar:SetVisibility(false)
			elseif value == 1 then
				bar:SetVisibility(true)
			end
		end
	end
end

-- !Name "If custom bar is visible..."
-- !Section "UI/Hud"
-- !Description "Checks if specified custom bar is visible."
-- !Conditional "True"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Bar name"

LevelFuncs.Engine.Node.TestBarVisibility = function(barName)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'If custom bar is visible...' node. No bar name provided", LogLevel.ERROR)
    else
		local dataName	= barName .. "_bar_data"
		if LevelVars.Engine.CustomBars.bars[dataName] then
			local bar = CustomBar.Get(barName)
			if bar:IsVisible() then
				return true
			else
				return false
			end
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
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'If custom bar value is...' node. No bar name provided", LogLevel.ERROR)
    else
		local dataName	= barName .. "_bar_data"
		if LevelVars.Engine.CustomBars.bars[dataName] then
			local bar = CustomBar.Get(barName)
				
			return LevelFuncs.Engine.Node.CompareValue(bar:GetValue(), value, operator)
		end
	end
end

-- !Name "Draw health bar for all enemies"
-- !Section "UI/Hud"
-- !Conditional "False"
-- !Description "Draw health bar for all enemies."
-- !Arguments "Newline, Number, 25, {78.6}, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 25,{12}, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Color, 25, {TEN.Color(255,0,0)}, Color of bar"

-- Construct custom bar for all enemies
LevelFuncs.Engine.Node.ConstructEnemiesHPBar = function(posX, posY, colorbar)

	CustomBar.SetEnemiesHpGenericBar(TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC, 0, TEN.Color(255,255,255), TEN.Vec2(posX, posY), 0, TEN.Vec2(19.05, 19.1), TEN.View.AlignMode.CENTER_LEFT, TEN.View.ScaleMode.FIT, TEN.Effects.BlendID.ALPHABLEND, 
	TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC, 1, colorbar, TEN.Vec2(posX+.15, posY), 0, TEN.Vec2(18.7, 18.48), TEN.View.AlignMode.CENTER_LEFT, TEN.View.ScaleMode.FIT, TEN.Effects.BlendID.ALPHABLEND,
								TEN.Vec2(0, 0), {},
								1, Color(0,0,0), true, 50, true, 0.25)
end

-- !Name "Create bars for Lara stats."
-- !Section "UI/Hud"
-- !Conditional "False"
-- !Description "Draw bar for Lara stats."
-- !Arguments "NewLine, Enumeration, 100, [ Health | Air | Sprint ], {0}, Bar Type"
-- !Arguments "Newline, SpriteSlots, 60, Background sprite sequence object ID, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC}"
-- !Arguments "Number, 20, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999],{0}"
-- !Arguments "Color, 20, {TEN.Color(255,255,255)}, Color of background sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "Newline, SpriteSlots, 60, Bar sprite sequence object ID, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC}"
-- !Arguments "Number, 20, [ 0 | 9999 | 0 ], Sprite ID for bar in sprite sequence\nRange[0 to 9999],{1}"
-- !Arguments "Color, {TEN.Color(255,0,0)}, 20, Color of bar sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {3}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"
-- !Arguments "Newline, Boolean, 50, Show stat bar always."
-- !Arguments "Boolean, 50, Blink."
LevelFuncs.Engine.Node.ConstructLaraBar = function(bartype, objectIDbg, spriteIDbg, colorbg, posXBG, posYBG, rotBG, scaleXBG, scaleYBG, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode, showBar, blink)

	local operand = bartype+1
	local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
	local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    local blendM = LevelFuncs.Engine.Node.GetBlendMode(blendMode)

	CustomBar.CreateLaraBar(objectIDbg, spriteIDbg, colorbg, TEN.Vec2(posXBG, posYBG), rotBG, TEN.Vec2(scaleXBG, scaleYBG), alignM, scaleM, blendM, 
					objectIDbar, spriteIDbar, colorbar, TEN.Vec2(posX, posY), rot, TEN.Vec2(scaleX, scaleY), alignM, scaleM, blendM,
					50, operand, showBar, blink, 0.25)

end

-- !Name "Start/Stop enemy health bars."
-- !Section "UI/Hud"
-- !Description "Starts or Stops the enemy health bars."
-- !Arguments "Enumeration, 30, [ Stop | Start ], Status"
-- !Arguments "NewLine, Boolean , Hide existing bars"

LevelFuncs.Engine.Node.EnemyHealthBarStatus = function(value, hideBars)
	if value == 0 then
		CustomBar.ShowEnemiesHpGenericBar(false)
	elseif value == 1 then
		CustomBar.ShowEnemiesHpGenericBar(true)
	end
	if hideBars then
		CustomBar.DeleteExistingHpGenericBars()
	end
end


--Code for Ammo Counter
LevelVars.AmmoCounter = {}

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
-- !Arguments "Color, 20, Color of Ammo Counter"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {3}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"

LevelFuncs.Engine.Node.ShowAmmoCounter = function(displayType, color, alignment, posX, posY, scale, effect, unlimited, swap, sprite, objectIDammo, spriteColor, spritePosX, spritePosY, rot, spriteScaleX, spriteScaleY, alignMode, scaleMode, blendMode)
	LevelVars.AmmoCounter.DisplayType	= displayType
	LevelVars.AmmoCounter.Color			= color
	LevelVars.AmmoCounter.Alignment		= alignment
	LevelVars.AmmoCounter.PosX			= posX
	LevelVars.AmmoCounter.PosY			= posY
	LevelVars.AmmoCounter.Scale			= scale
	LevelVars.AmmoCounter.Effect		= effect
	LevelVars.AmmoCounter.Unlimited		= unlimited
	LevelVars.AmmoCounter.Swap			= swap
	LevelVars.AmmoCounter.sprite		= sprite
	LevelVars.AmmoCounter.spriteObj		= objectIDammo
	LevelVars.AmmoCounter.spriteColor	= spriteColor
	LevelVars.AmmoCounter.spritePosX	= spritePosX
	LevelVars.AmmoCounter.spritePosY	= spritePosY
	LevelVars.AmmoCounter.SpriteScaleX	= spriteScaleX
	LevelVars.AmmoCounter.SpriteScaleY	= spriteScaleY
	LevelVars.AmmoCounter.Rot			= rot
	LevelVars.AmmoCounter.AlignMode		= alignMode
	LevelVars.AmmoCounter.ScaleMode		= scaleMode
	LevelVars.AmmoCounter.BlendMode		= blendMode
	LevelVars.AmmoCounter.AlphaBlendSpeed       = 100
	LevelVars.AmmoCounter.CurrentAlpha		    = 0
	LevelVars.AmmoCounter.TargetAlpha		    = 255
	LevelVars.AmmoCounter.AmmoName = {
		'pistol_ammo', 'revolver_ammo', 'uzi_ammo', 'shotgun_normal_ammo', 'shotgun_wideshot_ammo', 'hk_ammo',
		'crossbow_normal_ammo', 'crossbow_poison_ammo', 'crossbow_explosive_ammo', 'grenade_launcher_normal_ammo', 
		'grenade_launcher_super_ammo', 'grenade_launcher_flash_ammo', 'harpoon_gun_ammo', 'rocket_launcher_ammo'
	}

	AddCallback(TEN.Logic.CallbackPoint.POSTCONTROLPHASE,LevelFuncs.Engine.Node.__ShowAmmoCounter)
	PrintLog('Ammo counter initialized correctly', LogLevel.INFO)
end

-- !Name "Remove ammo counter"
-- !Conditional "False"
-- !Description "Remove the number of available ammo"
-- !Section "UI/Hud"
LevelFuncs.Engine.Node.RemoveAmmoCounter = function()
	RemoveCallback(TEN.Logic.CallbackPoint.POSTCONTROLPHASE, LevelFuncs.Engine.Node.__ShowAmmoCounter)
end

-- !Ignore
LevelFuncs.Engine.Node.__ShowAmmoCounter = function()

	local alphaDelta = LevelVars.AmmoCounter.AlphaBlendSpeed

	if LevelVars.AmmoCounter.CurrentAlpha ~= LevelVars.AmmoCounter.TargetAlpha then
            
		if LevelVars.AmmoCounter.CurrentAlpha < LevelVars.AmmoCounter.TargetAlpha then
			LevelVars.AmmoCounter.CurrentAlpha = math.floor(math.min(LevelVars.AmmoCounter.CurrentAlpha + alphaDelta, LevelVars.AmmoCounter.TargetAlpha))
		else
			LevelVars.AmmoCounter.CurrentAlpha = math.floor(math.max(LevelVars.AmmoCounter.CurrentAlpha - alphaDelta, LevelVars.AmmoCounter.TargetAlpha))
		end
		
	end

	local normalAlpha = (LevelVars.AmmoCounter.CurrentAlpha/255)

	if ((Lara:GetHandStatus() == 2 or Lara:GetHandStatus() == 4) and (Lara:GetWeaponType() ~= 7 and Lara:GetWeaponType() ~= 8)) then
		LevelVars.AmmoCounter.TargetAlpha=255
	else
		LevelVars.AmmoCounter.TargetAlpha = 0
	end
	
	if LevelVars.AmmoCounter.CurrentAlpha > 0 and Lara:GetHP() > 0 then
		local number = (Lara:GetAmmoCount() >= 0) and Lara:GetAmmoCount() or (LevelVars.AmmoCounter.Unlimited and (GetString("unlimited"):gsub(" ", "")):gsub("%%s", "") or "")
		local ammoName = (LevelVars.AmmoCounter.DisplayType == 0) and ((not LevelVars.AmmoCounter.Unlimited and Lara:GetAmmoCount() < 0) and "" or GetString(LevelVars.AmmoCounter.AmmoName[Lara:GetAmmoType()])) or ""
		local ammoText = (ammoName == "") and tostring(number) or ((LevelVars.AmmoCounter.Swap) and ammoName .. " " .. number or number .. " " .. ammoName)

		local myText = LevelFuncs.Engine.Node.GenerateString(ammoText, LevelVars.AmmoCounter.PosX, LevelVars.AmmoCounter.PosY, LevelVars.AmmoCounter.Scale, LevelVars.AmmoCounter.Alignment, LevelVars.AmmoCounter.Effect, LevelVars.AmmoCounter.Color, normalAlpha)
		ShowString(myText, 1/30)
			
		if LevelVars.AmmoCounter.sprite then
			local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(LevelVars.AmmoCounter.AlignMode)
			local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(LevelVars.AmmoCounter.ScaleMode)
			local blendID = LevelFuncs.Engine.Node.GetBlendMode(LevelVars.AmmoCounter.BlendMode)
			local spriteColor = Color(LevelVars.AmmoCounter.spriteColor.r,LevelVars.AmmoCounter.spriteColor.g,LevelVars.AmmoCounter.spriteColor.b,LevelVars.AmmoCounter.CurrentAlpha)
			local ammoSprite = DisplaySprite(LevelVars.AmmoCounter.spriteObj, Lara:GetAmmoType(), Vec2(LevelVars.AmmoCounter.spritePosX,LevelVars.AmmoCounter.spritePosY),0, Vec2(LevelVars.AmmoCounter.SpriteScaleX,LevelVars.AmmoCounter.SpriteScaleY),spriteColor)
			ammoSprite:Draw(0,alignM, scaleM, blendID)
		end
	end
end
