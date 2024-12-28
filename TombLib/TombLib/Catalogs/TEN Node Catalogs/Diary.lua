local CustomDiary = require("Engine.CustomDiary")

-- !Name "Create a diary from the template file"
-- !Section "Diary"
-- !Description "Create diary from template file."
-- !Arguments "NewLine, String, Name of the file to import diary entries., {DiarySetup}"

LevelFuncs.Engine.Node.DiaryImport = function(fileName)
	CustomDiary.ImportDiary(fileName)
end

-- !Name "Start / Stop the diary feature"
-- !Section "Diary"
-- !Description "Starts or stops the diary feature. To be used with start in a new level to enable the diary."
-- !Arguments "Enumeration, 30, [ Start | Stop ], Status"

LevelFuncs.Engine.Node.DiaryStatus = function(value)

	if GameVars.Engine.Diaries then
		if value == 0 then
			CustomDiary.Status(true)
		elseif value == 1 then
			CustomDiary.Status(false)
		end
	end
end

-- !Name "Add a text entry to the specified page of the diary"
-- !Section "Diary"
-- !Description "Add a new entry with to a diary."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], Diary Object to add an entry., {TEN.Objects.ObjID.DIARY_ITEM}" "Numerical, 20, Diary Page, [ 1 | 65535 ], {1}"
-- !Arguments "NewLine, String, 70, [ Multiline ], Diary entry" "Numerical, 15, X position, [ 0 | 100 ]" "Numerical, 15, Y position, [ 0 | 100 ]"
-- !Arguments "NewLine, Enumeration, 20, [ Left | Center | Right ], Horizontal alignment"
-- !Arguments "Enumeration, 40, [ Flat | Shadow | Blinking | Shadow + Blinking ], Effects"
-- !Arguments "Numerical, 20, {1}, [ 0 | 9 | 2 | 0.1 ], Scale" "Color, 20, {TEN.Color(255,255,255)}, Text color"

LevelFuncs.Engine.Node.DiaryAddTextEntry = function(object, pageIndex, text, textX, textY, textAlignment, textEffects, textScale, textColor)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then

		local pos = TEN.Vec2(textX, textY)
		local options = {}
		if (textEffects == 1 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.SHADOW) end
		if (textEffects == 2 or textEffects == 3) then table.insert(options, TEN.Strings.DisplayStringOption.BLINK) end
		if (textAlignment == 1) then table.insert(options, TEN.Strings.DisplayStringOption.CENTER) end
		if (textAlignment == 2) then table.insert(options, TEN.Strings.DisplayStringOption.RIGHT) end

		local diary = CustomDiary.Get(object)
		diary:addTextEntry(pageIndex, text, pos, options, textScale, textColor)
	end
	
end

-- !Name "Add an image entry to the specified page of the diary"
-- !Section "Diary"
-- !Description "Add a new entry with to a diary."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], Diary Object to add an entry., {TEN.Objects.ObjID.DIARY_ITEM}" "Numerical, 20, Diary Page, [ 1 | 65535 ], {1}"
-- !Arguments "Newline, SpriteSlots, 60, Background sprite sequence object ID, {TEN.Objects.ObjID.CUSTOM_BAR_GRAPHIC}"
-- !Arguments "Number, 20, [ 0 | 9999 | 0 ], Sprite ID for background in sprite sequence\nRange[0 to 9999],{0}"
-- !Arguments "Color, 20, Color of background sprite"
-- !Arguments "Newline, Number, 20, [ -1000 | 1000 | 2 ], Position X (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ -1000 | 1000 | 2 ], Position Y (%)\nRange [-1000 to 1000]\nVisible range [0 to 100]"
-- !Arguments "Number, 20, [ 0 | 360 | 2 ], Rotation\nRange [0 to 360]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale X (%)\nRange [0 to 1000]"
-- !Arguments "Number, 20, {100}, [ 0 | 1000 | 2 ], Scale Y (%)\nRange [0 to 1000]"
-- !Arguments "NewLine, Enumeration, 35, [ Center | Center Top | Center Bottom | Center Left | Center Right | Top Left | Top Right | Bottom Left | Bottom Right ], {3}, Align mode"
-- !Arguments "Enumeration, 22, [ Fit | Fill | Stretch ], Scale mode"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha Test | Additive | No Z Test | Subtractive | Wireframe | Exclude | Screen | Lighten | Alphablend ], {9}, Blend mode"

LevelFuncs.Engine.Node.DiaryAddImageEntry = function(object, pageIndex, objectIDbg, spriteIDbg, colorbg, posX, posY, rot, scaleX, scaleY, alignMode, scaleMode, blendMode)
	
	local dataName = object .. "_diarydata"
	local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)
	local pos = TEN.Vec2(posX, posY)
	local scale = TEN.Vec2(scaleX, scaleY)

	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:addImageEntry(pageIndex, objectIDbg, spriteIDbg, colorbg, pos, rot, scale, alignM, scaleM, blendID)
	end

end

-- !Name "Unlock diary pages till..."
-- !Section "Diary"
-- !Description "Unlock the diary pages til specified page number."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], Diary Object to unlock pages., {TEN.Objects.ObjID.DIARY_ITEM}" 
-- !Arguments "Numerical, 20, Page Number to unlock till, [ 1 | 65535 ], {1}"
-- !Arguments "NewLine, Boolean, Notification"
LevelFuncs.Engine.Node.DiaryUnlockPages = function(object, index, notification)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:unlockPages(index, notification)
	end
end

-- !Name "Clear diary page"
-- !Section "Diary"
-- !Description "This function clears the specified diary page."
-- !Arguments "NewLine, 80, WadSlots, [ _ITEM ], Diary Object to clear the page for, {TEN.Objects.ObjID.DIARY_ITEM}" 
-- !Arguments "Numerical, 20, Page Number to clear, [ 1 | 65535 ], {1}"

LevelFuncs.Engine.Node.DiaryClearPage = function(object, index)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:clearPage(index)
	end

end

-- !Name "Show Diary"
-- !Section "Diary"
-- !Description "This function displays the diary. To be used to display the diary via volume or triggers."
-- !Arguments "NewLine, WadSlots, [ _ITEM ], Diary to display., {TEN.Objects.ObjID.DIARY_ITEM}" 

LevelFuncs.Engine.Node.ShowDiary = function(object)
	
	local dataName = object .. "_diarydata"
	if GameVars.Engine.Diaries[dataName] then
		local diary = CustomDiary.Get(object)
		diary:showDiary()
	end
end

-- !Name "Clear a diary"
-- !Section "Diary"
-- !Description "Clear a diary. Please do this once a diary has served its purpose. \n It helps reduce the savegame size."
-- !Arguments "NewLine, WadSlots, [ _ITEM ], Diary to clear., {TEN.Objects.ObjID.DIARY_ITEM}"

LevelFuncs.Engine.Node.ClearDiary = function(object)

	CustomDiary.Delete(object)

end