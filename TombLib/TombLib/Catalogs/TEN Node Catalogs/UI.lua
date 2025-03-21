local CustomBar = require("Engine.CustomBar")

-- !Name "Create basic custom bar"
-- !Section "User interface"
-- !Conditional "False"
-- !Description "Creates a basic custom bar."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Numerical, [ 0 | 65535 | 2 ], {0}, 25, Start value of bar"
-- !Arguments "Numerical, [ 0 | 65535 | 2 ], {1000}, 25, Max value of bar"
-- !Arguments "Newline, Number, 33, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 33, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Color, 34, {TEN.Color(255,255,255)}, Color of bar sprite"

LevelFuncs.Engine.Node.BasicBars = function(barName, startvalue, maxValue, posX, posY, colorBar)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'Create basic custom bar' node. No bar name provided", LogLevel.ERROR)
    else
		local bar = {}
		bar.barName			= barName
		bar.startValue		= startvalue
		bar.maxValue		= maxValue
		bar.objectIdBg		= TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS
		bar.spriteIdBg		= 0
		bar.colorBg			= Color(255,255,255)
		bar.posBg			= TEN.Vec2(posX, posY)
		bar.rotBg			= 0
		bar.scaleBg			= TEN.Vec2(19.05, 19.1)
		bar.alignModeBg		= TEN.View.AlignMode.CENTER_LEFT
		bar.scaleModeBg		= TEN.View.ScaleMode.FIT
		bar.blendModeBg		= TEN.Effects.BlendID.ALPHABLEND
		bar.objectIdBar		= TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS
		bar.spriteIdBar		= 1
		bar.colorBar		= colorBar
		bar.posBar			= TEN.Vec2(posX+0.15, posY)
		bar.rot				= 0
		bar.scaleBar		= TEN.Vec2(18.7, 18.48)
		bar.alignMode		= TEN.View.AlignMode.CENTER_LEFT
		bar.scaleMode		= TEN.View.ScaleMode.FIT
		bar.blendMode		= TEN.Effects.BlendID.ALPHABLEND
		bar.text			= barName
		bar.textPos			= TEN.Vec2(posX, posY)
		bar.textOptions		= {}
		bar.textScale		= 1
		bar.textColor		= colorBar
		bar.hideText		= true
		bar.alphaBlendSpeed	= 50
		bar.blink			= false
		bar.blinkLimit		= 0.25

		CustomBar.Create(bar)

	end
end

-- !Name "Create advanced custom bar"
-- !Section "User interface"
-- !Conditional "False"
-- !Description "Creates a custom bar with extended configuration options."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Numerical, [ 0 | 65535 | 2 ], {0}, 25, Start value of bar"
-- !Arguments "Numerical, [ 0 | 65535 | 2 ], {1000}, 25, Max value of bar"
-- !Arguments "Newline, SpriteSlots, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS}, 61, Background sprite sequence object ID"
-- !Arguments "Numerical, [ 0 | 9999 | 0 ], {0}, 19, Sprite ID for background in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, {TEN.Color(255,255,255)}, 20, Color of background sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, [ -1000 | 1000 | 2 ], 20, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, [ 0 | 360 | 2 ], 20, Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, [ 0 | 1000 | 2 ], {20}, 20, Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, [ 0 | 1000 | 2 ], {20}, 20, Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "Newline, SpriteSlots, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS}, 61, Bar sprite sequence object ID"
-- !Arguments "Numerical, [ 0 | 9999 | 0 ], {1}, 19, Sprite ID for bar in sprite sequence\nRange[0 to 9999]"
-- !Arguments "Color, {TEN.Color(255,0,0)}, 20, Color of bar sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, [ -1000 | 1000 | 2 ], 20, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, [ 0 | 360 | 2 ], 20, Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, [ 0 | 1000 | 2 ], {20}, 20, Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, [ 0 | 1000 | 2 ], {20}, 20, Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {3}, 35, Align mode"
-- !Arguments "Enumeration, [ Fit | Fill | Stretch ], 22, Scale mode"
-- !Arguments "Enumeration, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, 28, Blend mode"

LevelFuncs.Engine.Node.DisplayBars = function(barName, startValue, maxValue, objectIdBg, spriteIdBg, colorBg, posXBg, posYBg, rotBg, scaleXBg, scaleYBg, objectIdBar, spriteIdBar, colorBar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'Create advanced custom bar' node. No bar name provided", LogLevel.ERROR)
    else
		local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    	local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    	local blendM = LevelFuncs.Engine.Node.GetBlendMode(blendMode)

		local bar = {}
		bar.barName			= barName
		bar.startValue		= startValue
		bar.maxValue		= maxValue
		bar.objectIdBg		= objectIdBg
		bar.spriteIdBg		= spriteIdBg
		bar.colorBg			= colorBg
		bar.posBg			= TEN.Vec2(posXBg, posYBg)
		bar.rotBg			= rotBg
		bar.scaleBg			= TEN.Vec2(scaleXBg, scaleYBg)
		bar.alignModeBg		= alignM
		bar.scaleModeBg		= scaleM
		bar.blendModeBg		= blendM
		bar.objectIdBar		= objectIdBar
		bar.spriteIdBar		= spriteIdBar
		bar.colorBar		= colorBar
		bar.posBar			= TEN.Vec2(posX, posY)
		bar.rot				= rot
		bar.scaleBar		= TEN.Vec2(scaleX, scaleY)
		bar.alignMode		= alignM
		bar.scaleMode		= scaleM
		bar.blendMode		= blendM
		bar.text			= barName
		bar.textPos			= TEN.Vec2(posX, posY)
		bar.textOptions		= {}
		bar.textScale		= 1
		bar.textColor		= colorBar
		bar.hideText		= true
		bar.alphaBlendSpeed	= 50
		bar.blink			= false
		bar.blinkLimit		= 0.25

		CustomBar.Create(bar)
	end
end
-- !Name "Change custom bar value over time"
-- !Section "User interface"
-- !Description "Change bar value over time."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Numerical, [ -65535 | 65535 | 2 | 1 | 1 ], {1}, 25, Value"
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
-- !Section "User interface"
-- !Description "Set bar value over time."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Bar name"
-- !Arguments "Numerical, [ -65535 | 65535 | 2 | 1 | 1 ], {1}, 25, Value"
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
-- !Section "User interface"
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

-- !Name "Show or hide the custom bar"
-- !Section "User interface"
-- !Description "Hides or Shows the custom bar."
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Bar name"
-- !Arguments "Enumeration, 30, [ Not visible | Visible ], Visibility"

LevelFuncs.Engine.Node.BarVisibility = function(barName, value)
	if barName == "" then
        TEN.Util.PrintLog("Error in the 'Show or hide custom bar' node. No bar name provided", LogLevel.ERROR)
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
-- !Section "User interface"
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
-- !Section "User interface"
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
-- !Section "User interface"
-- !Conditional "False"
-- !Description "Draw health bar for all enemies."
-- !Arguments "Newline, Number, 25, {78.6}, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 25,{12}, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Color, 25, {TEN.Color(255,0,0)}, Color of bar"

-- Construct custom bar for all enemies
LevelFuncs.Engine.Node.ConstructEnemiesHPBar = function(posX, posY, colorbar)

	local enemyBars = {}

	enemyBars.objectIdBg		= TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS
	enemyBars.spriteIdBg 		= 0
	enemyBars.colorBg			= TEN.Color(255,255,255)
	enemyBars.posBg				= TEN.Vec2(posX, posY)
	enemyBars.scaleBg			= TEN.Vec2(19.05, 19.1)
	enemyBars.rotBg				= 0
	enemyBars.alignModeBg		= TEN.View.AlignMode.CENTER_LEFT
	enemyBars.scaleModeBg		= TEN.View.ScaleMode.FIT
	enemyBars.blendModeBg		= TEN.Effects.BlendID.ALPHABLEND
	enemyBars.objectIdBar		= TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS
	enemyBars.spriteIdBar		= 1
	enemyBars.colorBar			= colorbar
	enemyBars.posBar			= TEN.Vec2(posX+.15, posY)
	enemyBars.scaleBar			= TEN.Vec2(18.7, 18.48)
	enemyBars.rot				= 0
	enemyBars.alignMode			= TEN.View.AlignMode.CENTER_LEFT
	enemyBars.scaleMode			= TEN.View.ScaleMode.FIT
	enemyBars.blendMode			= TEN.Effects.BlendID.ALPHABLEND
	enemyBars.textPos			= TEN.Vec2(0, 0)
	enemyBars.textOptions		= {}
	enemyBars.textScale			= 1
	enemyBars.textColor			= colorbar
	enemyBars.hideText			= true
	enemyBars.alphaBlendSpeed	= 50
	enemyBars.blink				= true
	enemyBars.blinkLimit		= 0.25

	CustomBar.SetEnemiesHpGenericBar(enemyBars)
end

-- !Name "Draw health bar for specific enemy"
-- !Section "User interface"
-- !Conditional "False"
-- !Description "Draw health bar for an enemy."
-- !Arguments "NewLine, Moveables, Enemy to show health bar for"
-- !Arguments "NewLine, String, 70, [ NoMultiline ], Enemy name" "Numerical, 15, X position, [ 0 | 100 ]" "Numerical, 15, Y position, [ 0 | 100 ]"
-- !Arguments "NewLine, Enumeration, 20, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "Enumeration, 40, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "Numerical, 20, {1}, [ 0 | 9 | 2 | 0.1 ], Scale" "Color, 20, {TEN.Color(255,255,255)}, Text color"
-- !Arguments "NewLine, Number, 25, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999],{0}"
-- !Arguments "Color, 25, {TEN.Color(255,255,255)}, Color of background sprite"
-- !Arguments "Numerical, 25, [ 0 | 9999 | 0 ], Sprite ID for bar in sprite sequence\nRange[0 to 9999],{1}"
-- !Arguments "Color, 25, {TEN.Color(255,0,0)}, Color of bar sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "Newline, Boolean, 80, Show health bar when enemy is not targeted"
-- !Arguments "Boolean, 20, Hide text"

LevelFuncs.Engine.Node.ConstructEnemyBar = function(object, text, textX, textY, textAlignment, textEffects, textScale, textColor, spriteIdBg, colorBg, spriteIdBar, colorBar,posXBg, posYBg, rotBg, scaleXBg, scaleYBg, posX, posY, rot, scaleX, scaleY, showBar, hideText)
	
	local dataName = object .. "_bar_data"

	if LevelVars.Engine.CustomBars.bars[dataName] then
		return
		print("Bar already exists for the specified enemy.")
	end

	local options = {}
	if (textEffects == 1 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
	if (textEffects == 2 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.BLINK) end
	if (textAlignment == 1) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
	if (textAlignment == 2) then table.insert(options, TEN.Strings.DisplayStringOption.RIGHT) end


	local enemyBar = {
		barName			= object,
		objectIdBg		= TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS,
		spriteIdBg		= spriteIdBg,
		colorBg			= colorBg,
		posBg			= TEN.Vec2(posXBg, posYBg),
		rotBg			= rotBg,
		scaleBg			= TEN.Vec2(scaleXBg, scaleYBg),
		alignModeBg		= TEN.View.AlignMode.CENTER_LEFT,
		scaleModeBg		= TEN.View.ScaleMode.FIT,
		blendModeBg		= TEN.Effects.BlendID.ALPHABLEND,
		objectIdBar		= TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS,
		spriteIdBar		= spriteIdBar,
		colorBar		= colorBar,
		posBar			= TEN.Vec2(posX, posY),
		rot				= rot,
		scaleBar		= TEN.Vec2(scaleX, scaleY),
		alignMode		= TEN.View.AlignMode.CENTER_LEFT,
		scaleMode		= TEN.View.ScaleMode.FIT,
		blendMode		= TEN.Effects.BlendID.ALPHABLEND,
		text			= text,
		textPos			= TEN.Vec2(textX, textY),
		textOptions		= options,
		textScale		= textScale,
		textColor		= textColor,
		hideText		= hideText,
		alphaBlendSpeed	= 50,
		blink			= true,
		blinkLimit		= 0.25,
		showBar 		= showBar,
		object			= object
	}

	CustomBar.CreateEnemyHpBar(enemyBar)

end

-- !Name "Create bars for player stats"
-- !Section "User interface"
-- !Conditional "False"
-- !Description "Draw bar for player stats."
-- !Arguments "NewLine, Enumeration, 20, [ Health | Air | Sprint ], {0}, Bar Type"
-- !Arguments "Boolean, 40, Show stat bar always"
-- !Arguments "Boolean, 40, Blink"
-- !Arguments "Newline, SpriteSlots, 60, Background sprite sequence object ID, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS}"
-- !Arguments "Numerical, 20, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999],{0}"
-- !Arguments "Color, 20, {TEN.Color(255,255,255)}, Color of background sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "Newline, SpriteSlots, 60, Bar sprite sequence object ID, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHICS}"
-- !Arguments "Numerical, 20, [ 0 | 9999 | 0 ], Sprite ID for bar in sprite sequence\nRange[0 to 9999],{1}"
-- !Arguments "Color, {TEN.Color(255,0,0)}, 20, Color of bar sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {3}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"
LevelFuncs.Engine.Node.ConstructLaraBar = function(bartype, showBar, blink, objectIDbg, spriteIDbg, colorbg, posXBG, posYBG, rotBG, scaleXBG, scaleYBG, objectIDbar, spriteIDbar, colorbar, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)

	local operand = bartype+1
	local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
	local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    local blendM = LevelFuncs.Engine.Node.GetBlendMode(blendMode)

	local playerBar = {}

	playerBar.getActionType		= operand
	playerBar.objectIdBg		= objectIDbg
	playerBar.spriteIdBg		= spriteIDbg
	playerBar.colorBg			= colorbg
	playerBar.posBg				= TEN.Vec2(posXBG, posYBG)
	playerBar.rotBg				= rotBG
	playerBar.scaleBg			= TEN.Vec2(scaleXBG, scaleYBG)
	playerBar.alignModeBg		= alignM
	playerBar.scaleModeBg		= scaleM
	playerBar.blendModeBg		= blendM
	playerBar.objectIdBar		= objectIDbar
	playerBar.spriteIdBar		= spriteIDbar
	playerBar.colorBar			= colorbar
	playerBar.posBar			= TEN.Vec2(posX, posY)
	playerBar.rot				= rot
	playerBar.scaleBar			= TEN.Vec2(scaleX, scaleY)
	playerBar.alignMode			= alignM
	playerBar.scaleMode			= scaleM
	playerBar.blendMode			= blendM
	playerBar.alphaBlendSpeed	= 50
	playerBar.showBar			= showBar
	playerBar.blink				= blink
	playerBar.blinkLimit		= 0.25

	CustomBar.CreatePlayerBar(playerBar)

end

-- !Name "Start or stop enemy health bars"
-- !Section "User interface"
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
-- !Description "Displays the ammo counter for the weapon in hand."
-- !Section "User interface"
-- !Arguments "NewLine, Enumeration , 60, [ Counter + ammo name | Use only counter ], {1}, Display type"
-- !Arguments "Color, 20, {TEN.Color(255,255,255)}, Text color"
-- !Arguments "Enumeration , 20,[ Left	| Center | Right ], Text alignment"
-- !Arguments "NewLine, Numerical, 20, [ 0 | 100 | 1 | 0.1 ], {95}, Text position x"
-- !Arguments "Numerical, 20, [ 0 | 100 | 1 | 0.1 ], {5}, Text position y"
-- !Arguments "Numerical, 20, {1}, [ 0 | 9 | 2 | 0.1 ], Text scale"
-- !Arguments "Enumeration, 40, [ Flat | Shadow | Blinking | Shadow + Blinking ], {1}, Text effects"
-- !Arguments "Newline, SpriteSlots, 80, {TEN.Objects.ObjID.CUSTOM_AMMO_GRAPHICS}, Ammo counter sprite sequence object ID"
-- !Arguments "Color, 20, {TEN.Color(255,255,255)}, Color of ammo counter sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], {94}, Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ -1000 | 1000 | 2 ], {5}, Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Numerical, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], {5}, Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Numerical, 20, {100}, [ 0 | 1000 | 2 ], {5}, Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 34, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {6}, Align mode"
-- !Arguments "Enumeration, 33, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 33, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"
-- !Arguments "NewLine, Boolean , 40, Show unlimited ammo"
-- !Arguments "Boolean , 60, Swap ammo name & counter position"
-- !Arguments "Newline, Boolean , 100, {true}, Show ammo sprites"

LevelFuncs.Engine.Node.ShowAmmoCounter = function(displayType, color, alignment, posX, posY, scale, effect, objectIDammo, spriteColor, spritePosX, spritePosY, rot, spriteScaleX, spriteScaleY, alignMode, scaleMode, blendMode, unlimited, swap, sprite)
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
-- !Description "Removes the ammo counter."
-- !Section "User interface"
LevelFuncs.Engine.Node.RemoveAmmoCounter = function()
	RemoveCallback(TEN.Logic.CallbackPoint.POSTCONTROLPHASE, LevelFuncs.Engine.Node.__ShowAmmoCounter)
end

-- !Ignore
LevelFuncs.Engine.Node.__ShowAmmoCounter = function()
	
	local cameraType = View.GetCameraType()
	if (cameraType == CameraType.FLYBY or cameraType == CameraType.BINOCULARS or cameraType == CameraType.LASERSIGHT) then
		return
	end
	
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
	
	if LevelVars.AmmoCounter.CurrentAlpha > 0 and Lara:GetHP() > 0 and Lara:GetHandStatus() ~= 0 then
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
