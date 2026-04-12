--Constants
local SOUNDS =
{
    SMALL_SWITCH = 269, 
}

local PLAYER_ANIMS = 
{
    STAND_IDLE = 103,
    POUR_WATERSKIN_HIGH = 402
}


--for flipped items
local function TestPosition(item, positionOffset1, positionOffset2, rotOffset)

    local targetRot  = item:GetRotation()
    local targetPos  = item:GetPosition()
    local laraPos    = Lara:GetPosition()
    local laraRot    = Lara:GetRotation()

    -- Lara must face the opposite direction of the item (180° offset), within ±rotOffset
    local requiredLaraY = (targetRot.y + 180) % 360
    local angleDiff     = math.abs(((laraRot.y - requiredLaraY) + 180) % 360 - 180)

    if angleDiff > rotOffset then
        return false
    end

    -- Rotate the bounding box corners into world space using the item's rotation
    local worldCorner1 = targetPos:Translate(targetRot, positionOffset1)
    local worldCorner2 = targetPos:Translate(targetRot, positionOffset2)

    -- Build an axis-aligned bounding box from the two rotated corners
    local minX = math.min(worldCorner1.x, worldCorner2.x)
    local maxX = math.max(worldCorner1.x, worldCorner2.x)
    local minY = math.min(worldCorner1.y, worldCorner2.y)
    local maxY = math.max(worldCorner1.y, worldCorner2.y)
    local minZ = math.min(worldCorner1.z, worldCorner2.z)
    local maxZ = math.max(worldCorner1.z, worldCorner2.z)

    if laraPos.x < minX or laraPos.x > maxX then return false end
    if laraPos.y < minY or laraPos.y > maxY then return false end
    if laraPos.z < minZ or laraPos.z > maxZ then return false end

    return KeyIsHit(ActionID.ACTION)

end

local function GetWaterSkinObjectID()

    local smallQty = Lara:GetWaterSkinStatus(false)
    local largeQty = Lara:GetWaterSkinStatus(true)

    if smallQty > 1 then
        return TEN.Objects.ObjID.WATERSKIN1_EMPTY + smallQty - 1
    elseif largeQty > 1 then
        return TEN.Objects.ObjID.WATERSKIN2_EMPTY + largeQty - 1
    end

    return nil

end

LevelFuncs.Engine.Node.InterceptInventoryItem = function()

    if LevelVars.Engine.ScalesPuzzle.Active  then
        local id = TEN.Inventory.GetUsedItem()
        TEN.Inventory.ClearUsedItem()
        local isSmall = (id > TEN.Objects.ObjID.WATERSKIN1_EMPTY and id <= TEN.Objects.ObjID.WATERSKIN1_3)
        local isLarge = (id > TEN.Objects.ObjID.WATERSKIN2_EMPTY and id <= TEN.Objects.ObjID.WATERSKIN2_5)

        if isSmall or isLarge then           
            Lara:SetAnim(PLAYER_ANIMS.POUR_WATERSKIN_HIGH)
        end

        if isSmall then
            Lara:SetWaterSkinStatus(1, false)
            LevelVars.Engine.ScalesPuzzle.PouredVolume = id - TEN.Objects.ObjID.WATERSKIN1_EMPTY
        elseif isLarge then
            Lara:SetWaterSkinStatus(1, true)
            LevelVars.Engine.ScalesPuzzle.PouredVolume = id - TEN.Objects.ObjID.WATERSKIN2_EMPTY
        end

        LevelVars.Engine.ScalesPuzzle.Active = false
    end
    
end

TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PRE_USE_ITEM, LevelFuncs.Engine.Node.InterceptInventoryItem)

--Scales
local SCALE_STATES = 
{
    REST = 0,
    BALANCE = 1,
    HEAVY = 2,
    LIGHT = 3,
    RESET_HEAVY = 4,
    RESET_LIGHT = 5
}

local SCALE_FLAGS = 
{
    STATUS = 0,
    ANIMATION = 1
}

LevelVars.Engine.ScalesPuzzle = {}
LevelVars.Engine.ScalesPuzzle.Active = false
LevelVars.Engine.ScalesPuzzle.PouredVolume = 0

-- !Name "Create a Scales Puzzle"
-- !Section "Objects"
-- !Description "Create a Scale Puzzle with volume events. Set OCB of the moveable to the required volume (1-5)"
-- !Arguments "NewLine, Moveables, Scales Object"
-- !Arguments "NewLine, 65, VolumeEventSets, Target event set to run at correct value"
-- !Arguments "VolumeEvents, 35, Event to run"
-- !Arguments "NewLine, 65, VolumeEventSets, Target event set to run at incorrect value"
-- !Arguments "VolumeEvents, 35, Event to run"
LevelFuncs.Engine.Node.ScalesPuzzle = function(moveableName, volumeEventSuccess, eventTypeSuccess, volumeEventFail, eventTypeFail)

    local scalePuzzle = TEN.Objects.GetMoveableByName(moveableName)
    local requiredVolume = math.max(1, math.min(scalePuzzle:GetOCB(), 5))
    local itemPresent = GetWaterSkinObjectID()

    if scalePuzzle:GetItemFlags(SCALE_FLAGS.STATUS) == 1 then
        scalePuzzle:HideInteractionHighlight()
    else 
        scalePuzzle:ShowInteractionHighlight()
    end

    local positionTest = TestPosition(scalePuzzle, Vec3(768, -512, 0), Vec3(1280, 0, 512), 30)

    if positionTest and scalePuzzle:GetItemFlags(SCALE_FLAGS.STATUS) ~= 1 and itemPresent then
        TEN.Inventory.SetFocusedItem(itemPresent)
        LevelVars.Engine.ScalesPuzzle.Active = true
    end

    if Lara:GetAnim() == PLAYER_ANIMS.POUR_WATERSKIN_HIGH then
        KeyClear(ActionID.ACTION)
    end

    if Lara:GetAnim() == PLAYER_ANIMS.POUR_WATERSKIN_HIGH and Lara:GetFrame() == 93  and scalePuzzle:GetItemFlags(SCALE_FLAGS.STATUS) ~= 1 then
        scalePuzzle:SetItemFlags(1, SCALE_FLAGS.STATUS)
        if LevelVars.Engine.ScalesPuzzle.PouredVolume == requiredVolume then
            scalePuzzle:SetAnim(SCALE_STATES.BALANCE)
            TEN.Logic.HandleEvent(volumeEventSuccess, eventTypeSuccess, Lara)
        elseif LevelVars.Engine.ScalesPuzzle.PouredVolume < requiredVolume then
            scalePuzzle:SetAnim(SCALE_STATES.LIGHT)
            scalePuzzle:SetItemFlags(SCALE_STATES.LIGHT, SCALE_FLAGS.ANIMATION)
            TEN.Logic.HandleEvent(volumeEventFail, eventTypeFail, Lara)
        else
            scalePuzzle:SetAnim(SCALE_STATES.HEAVY)
            scalePuzzle:SetItemFlags(SCALE_STATES.HEAVY, SCALE_FLAGS.ANIMATION)
            TEN.Logic.HandleEvent(volumeEventFail, eventTypeFail, Lara)
        end
    end
end

-- !Name "Reset Scales"
-- !Section "Objects"
-- !Description "Reset a Scale Puzzle Object. To be used after an incorrect attempt."
-- !Arguments "NewLine, Moveables, Scales Object"
LevelFuncs.Engine.Node.ResetScalesPuzzle = function(moveableName)

    local scalePuzzle = TEN.Objects.GetMoveableByName(moveableName)
    local animation = scalePuzzle:GetItemFlags(SCALE_FLAGS.ANIMATION)
    scalePuzzle:SetItemFlags(0, SCALE_FLAGS.STATUS)
    scalePuzzle:SetItemFlags(0, SCALE_FLAGS.ANIMATION)
    if animation == SCALE_STATES.HEAVY then
        scalePuzzle:SetAnim(SCALE_STATES.RESET_HEAVY)
    else
        scalePuzzle:SetAnim(SCALE_STATES.RESET_LIGHT)
    end
    
end

LevelVars.Engine.SequenceSwitches = {}

-- !Name "Create a Sequence Switches Puzzle"
-- !Section "Objects"
-- !Description "Create a Sequence Switch and Door puzzle."
-- !Arguments "NewLine, String, Prefix for switch names (Switch for Switch_1 Switch_2 Switch_3)"
-- !Arguments "NewLine, String, Prefix for door names (Door for Door_1 Door_2 Door_3 Door_4 Door_5 Door_6)"
LevelFuncs.Engine.Node.SequenceSwitches = function(switchPrefix, doorPrefix)

    if not LevelVars.Engine.SequenceSwitches[switchPrefix] then
        LevelVars.Engine.SequenceSwitches[switchPrefix] = {
            sequence = {},
            switchStates = { false, false, false },
            activeDoor = nil,
            switchAnim = nil,
            switchStatus = nil,
            needsRecreate = false
        }

        -- Cache switch data on first run
        local state = LevelVars.Engine.SequenceSwitches[switchPrefix]
        local switch = TEN.Objects.GetMoveableByName(switchPrefix .. "_" .. 1)
        state.switchStatus = switch:GetStatus()
        state.switchAnim = switch:GetAnim()
    end


    local state = LevelVars.Engine.SequenceSwitches[switchPrefix]

    local combinationToDoor = {
        ["123"] = 1,
        ["132"] = 2,
        ["213"] = 3,
        ["231"] = 4,
        ["312"] = 5,
        ["321"] = 6
    }

    local switches = {
        TEN.Objects.GetMoveableByName(switchPrefix .. "_1"),
        TEN.Objects.GetMoveableByName(switchPrefix .. "_2"),
        TEN.Objects.GetMoveableByName(switchPrefix .. "_3")
    }

    for i, switch in ipairs(switches) do
        if switch:GetAnim() == 2 and state.switchStates[i] == false then
            state.switchStates[i] = true
            table.insert(state.sequence, i)
        end
    end
    
    if #state.sequence == 3 and Lara:GetAnim() == PLAYER_ANIMS.STAND_IDLE then
        
        local input = table.concat(state.sequence)
        local doorIndex = combinationToDoor[input]
        TEN.Sound.PlaySound(SOUNDS.SMALL_SWITCH)

        if state.activeDoor then
            TEN.Objects.GetMoveableByName(doorPrefix .. "_" .. state.activeDoor):Disable()
            state.activeDoor = nil
        end

        if doorIndex then
            TEN.Objects.GetMoveableByName(doorPrefix .. "_" .. doorIndex):Enable()
            state.activeDoor = doorIndex
        end

        state.sequence = {}
        state.switchStates = { false, false, false }
        state.needsRecreate = true
    end

    if state.needsRecreate then
        for i = 1, 3 do
            local sw = TEN.Objects.GetMoveableByName(switchPrefix .. "_" .. i)
            if sw then
                sw:SetAnim(state.switchAnim)
                sw:SetStatus(state.switchStatus)
            end
        end
        state.needsRecreate = false
    end

end

