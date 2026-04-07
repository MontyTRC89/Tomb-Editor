--Constants
local SOUNDS =
{
    FIRE_LOOP = 150,
    SMALL_SWITCH = 269,
    SHATTER = 347 
}

local PLAYER_ANIMS = 
{
    STAND_IDLE = 103,
    POUR_WATERSKIN_HIGH = 402,
    TORCH_LIGHT_3 = 429
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

end

--Burning Floor
local FIRE_OFFSETS =
{   --displacement, rotation, size
    {1219.784, 94.488, 2},
    {924.822, -37.256, 2},
    {1067.333, -203.896, 2},
    {143.108, -206.638, 2},
    {826.482, -85.558, 2},
    {576.056, -127.676, 1},
    {526.847, -48.063, 1},
    {1253.348, -60.981, 1},
    {593.997, -225.522, 1},
    {1263.924, -24.288, 1},
    {572.503, 25.722, 1},
    {811.951, 84.365, 1},
    {1252.326, -252.225, 0.5},
    {1250.485, -226.342, 0.5},
    {922.085, -188.477, 0.5},
    {635.232, -16.846, 0.5}
}

LevelVars.Engine.BurningFloor = {}
LevelVars.Engine.BurningFloor.Active = false
LevelVars.Engine.BurningFloor.BurnDuration = 1024
LevelVars.Engine.BurningFloor.Counter = LevelVars.Engine.BurningFloor.BurnDuration
LevelVars.Engine.BurningFloor.ItemName = nil
LevelVars.Engine.BurningFloor.PlayerInVolume = false
LevelVars.Engine.BurningFloor.FlipMap = 0

-- !Name "Burning Floor"
-- !Section "Objects"
-- !Description "Create a Burning Floor Object."
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, Moveables, 60"
LevelFuncs.Engine.Node.BurningFloor = function(moveableName, activator)

    local object = TEN.Objects.GetMoveableByName(activator)
    local burningFloor = TEN.Objects.GetMoveableByName(moveableName)

    LevelVars.Engine.BurningFloor.PlayerInVolume = false

    if object:GetObjectID() == TEN.Objects.ObjID.BURNING_TORCH_ITEM and object:GetItemFlags(3) == 1 and (math.abs(object:GetPosition().y - burningFloor:GetPosition().y) < 64) and burningFloor:GetStatus() ~= Objects.MoveableStatus.DEACTIVATED then
        LevelVars.Engine.BurningFloor.ItemName = moveableName
        LevelVars.Engine.BurningFloor.Active = true
        TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PRELOOP, LevelFuncs.Engine.Node.RunBurningFloor)
        object:Destroy()
    end

    if object:GetObjectID() == TEN.Objects.ObjID.LARA and (math.abs(object:GetPosition().y - burningFloor:GetPosition().y) < 32) then
        LevelVars.Engine.BurningFloor.PlayerInVolume = true
    end

end

LevelFuncs.Engine.Node.RunBurningFloor = function()

    if not LevelVars.Engine.BurningFloor.Active then
        return
    end

    local burningFloor = TEN.Objects.GetMoveableByName(LevelVars.Engine.BurningFloor.ItemName)

    LevelVars.Engine.BurningFloor.Counter = math.max(0, LevelVars.Engine.BurningFloor.Counter - 1)
    local elapsed = BURN_DURATION - LevelVars.Engine.BurningFloor.Counter
    local normalizedTime = elapsed / BURN_DURATION
    local colorLife = 1 - normalizedTime
    local c = math.floor(colorLife * 128)
    burningFloor:SetColor(Color(c, c, c))

    local fireLife = 0
    if normalizedTime >= START_DELAY then
        local t = (normalizedTime - START_DELAY) / (1 - START_DELAY)
        fireLife = math.sin(t * math.pi)
    end
    
    local position = burningFloor:GetPosition()
    local yaw = burningFloor:GetRotation().y

    for _, entry in ipairs(FIRE_OFFSETS) do
        local distance = entry[1]
        local angleDeg = entry[2]
        local size = entry[3]

        local rot = Rotation(0, yaw + angleDeg, 0)
        local firePos = position:Translate(rot, distance)

        local fireStrength = fireLife * (0.8 + size * 0.6)
        if fireStrength > 0.02 then
            TEN.Effects.EmitFire(firePos, fireStrength)
        end
    end

    TEN.Sound.PlaySound(SOUNDS.FIRE_LOOP)

    if (math.abs(Lara:GetPosition().y - burningFloor:GetPosition().y) < 64) and normalizedTime >= START_DELAY and LevelVars.Engine.BurningFloor.PlayerInVolume then
        Lara:SetEffect(Effects.EffectID.FIRE)
        local ocb = burningFloor:GetOCB()
        TEN.Sound.PlaySound(SOUNDS.SHATTER)
        burningFloor:Shatter()
        TEN.Flow.FlipMap(ocb)
        Lara:SetHP(20)
        LevelVars.Engine.BurningFloor.Active = false
        return
    end

    if LevelVars.Engine.BurningFloor.Counter == 0 then
        local ocb = burningFloor:GetOCB()
        TEN.Sound.PlaySound(SOUNDS.SHATTER)
        burningFloor:Shatter()
        TEN.Flow.FlipMap(ocb)
        LevelVars.Engine.BurningFloor.Active = false
        LevelVars.Engine.BurningFloor.Counter = LevelVars.Engine.BurningFloor.BurnDuration
        LevelVars.Engine.BurningFloor.ItemName = nil
    end

end

--Elemental Puzzle
local ELEMENTAL_TYPE = 
{
    WATER = 0,
    FIRE = 1,
    EARTH = 2,
    SCALES = 3
}

local ELEMENTAL_MESHSET = 
{
    [ELEMENTAL_TYPE.WATER] = {off= 6, on=5},
    [ELEMENTAL_TYPE.FIRE] = {off= 6, on=1},
    [ELEMENTAL_TYPE.EARTH] = {off= 6, on=3},
}

local MESH_SET = 
{
    [ELEMENTAL_TYPE.WATER] = TEN.Objects.ObjID.LARA_WATER_MESH,
    [ELEMENTAL_TYPE.FIRE] = TEN.Objects.ObjID.LARA_PETROL_MESH,
    [ELEMENTAL_TYPE.EARTH] = TEN.Objects.ObjID.LARA_DIRT_MESH
}

local ITEM_SET = 
{
    [ELEMENTAL_TYPE.WATER] = GetWaterSkinObjectID(),
    [ELEMENTAL_TYPE.FIRE] = TEN.Objects.ObjID.PICKUP_ITEM2,
    [ELEMENTAL_TYPE.EARTH] = TEN.Objects.ObjID.PICKUP_ITEM1
}

LevelVars.Engine.ElementalPuzzle = {}
LevelVars.Engine.ElementalPuzzle.Active = false
LevelVars.Engine.ElementalPuzzle.Type = ELEMENTAL_TYPE.WATER
LevelVars.Engine.ElementalPuzzle.FireList = {}

-- !Name "Elemental Puzzle"
-- !Section "Objects"
-- !Description "Create an Elemental Puzzle Object."
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, Moveables, Trigger Triggerer object to activate"
LevelFuncs.Engine.Node.ElementalPuzzle = function(moveableName, trigger)

    local elementalPuzzle = TEN.Objects.GetMoveableByName(moveableName)
    local type = elementalPuzzle:GetOCB()
    LevelVars.Engine.ElementalPuzzle.Type = type
    local item = ITEM_SET[type]

    if elementalPuzzle:GetItemFlags(type) == 2 then
        elementalPuzzle:HideInteractionHighlight()
    end

    local positionTest = TestPosition(elementalPuzzle, Vec3(-256, -512, 0), Vec3(256, 0, 512), 30)

    if positionTest and elementalPuzzle:GetItemFlags(type) ~= 2 and elementalPuzzle:GetItemFlags(type) ~= 1 and TEN.Inventory.GetItemCount(item) > 0 then
        TEN.Inventory.SetFocusedItem(item)
        LevelVars.Engine.ElementalPuzzle.Active = true
    end

    if positionTest and elementalPuzzle:GetItemFlags(type) == 1 and Lara:IsTorchLit() then
        Lara:SetAnim(PLAYER_ANIMS.TORCH_LIGHT_3)
    end

    if Lara:GetAnim() == PLAYER_ANIMS.POUR_WATERSKIN_HIGH or Lara:GetAnim() == PLAYER_ANIMS.TORCH_LIGHT_3 then
        KeyClear(ActionID.ACTION)
    end
    
    if Lara:GetAnim() == PLAYER_ANIMS.POUR_WATERSKIN_HIGH and Lara:GetFrame() == 16 then

        if type ~= ELEMENTAL_TYPE.WATER then
            Lara:SwapMesh(13,MESH_SET[type], 13)
        end

    end

    if Lara:GetAnim() == PLAYER_ANIMS.POUR_WATERSKIN_HIGH and Lara:GetFrame() == 47  and elementalPuzzle:GetItemFlags(type) ~= 2 then

        elementalPuzzle:SetMeshVisible(ELEMENTAL_MESHSET[type].on, true)
        elementalPuzzle:SetMeshVisible(ELEMENTAL_MESHSET[type].off, false)
        
        if type == ELEMENTAL_TYPE.FIRE then
            elementalPuzzle:SetItemFlags(1, type)
        else
            elementalPuzzle:SetItemFlags(2, type)
            local triggerer = GetMoveableByName(trigger)
            triggerer:Enable()
        end
    end

    if Lara:GetAnim() == PLAYER_ANIMS.TORCH_LIGHT_3 and Lara:GetFrame() == 25  and elementalPuzzle:GetItemFlags(type) == 1 then
        local position = elementalPuzzle:GetPosition()
        local triggerer = GetMoveableByName(trigger)
        triggerer:Enable()
        table.insert(LevelVars.Engine.ElementalPuzzle.FireList, Vec3(position.x, position.y - 570, position.z))
        elementalPuzzle:SetItemFlags(2, type)
    end
end



LevelFuncs.Engine.Node.InterceptWaterSkin = function()

    if LevelVars.Engine.ElementalPuzzle.Active then
        local type = LevelVars.Engine.ElementalPuzzle.Type
        local id = TEN.Inventory.GetUsedItem()
        TEN.Inventory.ClearUsedItem()

        if type == ELEMENTAL_TYPE.WATER then
            local isSmall = (id >= TEN.Objects.ObjID.WATERSKIN1_EMPTY and id <= TEN.Objects.ObjID.WATERSKIN1_3)
            local isLarge = (id >= TEN.Objects.ObjID.WATERSKIN2_EMPTY and id <= TEN.Objects.ObjID.WATERSKIN2_5)

            if isSmall then
                Lara:SetWaterSkinStatus(1, false)
                Lara:SetAnim(PLAYER_ANIMS.POUR_WATERSKIN_HIGH)
            elseif isLarge then
                Lara:SetWaterSkinStatus(1, true)
                Lara:SetAnim(PLAYER_ANIMS.POUR_WATERSKIN_HIGH)
            end

        elseif type == ELEMENTAL_TYPE.FIRE  or type == ELEMENTAL_TYPE.EARTH then  

            if id == ITEM_SET[type] then
                Lara:SetAnim(PLAYER_ANIMS.POUR_WATERSKIN_HIGH)
            end

        end

        LevelVars.Engine.ElementalPuzzle.Active = false
    end

    if LevelVars.Engine.ScalesPuzzle.Active  then
        local id = TEN.Inventory.GetUsedItem()
        TEN.Inventory.ClearUsedItem()
        local isSmall = (id >= TEN.Objects.ObjID.WATERSKIN1_EMPTY and id <= TEN.Objects.ObjID.WATERSKIN1_3)
        local isLarge = (id >= TEN.Objects.ObjID.WATERSKIN2_EMPTY and id <= TEN.Objects.ObjID.WATERSKIN2_5)

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

LevelFuncs.Engine.Node.ElementalPuzzleFire = function()

    local fireList = LevelVars.Engine.ElementalPuzzle.FireList
    if not fireList then return end

    for _, flame in ipairs(fireList) do
        TEN.Effects.EmitFire(flame, 1)
        TEN.Sound.PlaySound(SOUNDS.FIRE_LOOP, flame)
    end

end

TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PRE_USE_ITEM, LevelFuncs.Engine.Node.InterceptWaterSkin)
TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PRE_LOOP, LevelFuncs.Engine.Node.ElementalPuzzleFire)

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

-- !Name "Scales"
-- !Section "Objects"
-- !Description "Create a Scale Puzzle Object."
-- !Arguments "NewLine, Moveables"
-- !Arguments "NewLine, 65, VolumeEventSets, Target event set at correct value"
-- !Arguments "VolumeEvents, 35, Event to run"
-- !Arguments "NewLine, 65, VolumeEventSets, Target event set at incorrect value"
-- !Arguments "VolumeEvents, 35, Event to run"
LevelFuncs.Engine.Node.ScalesPuzzle = function(moveableName, volumeEventSuccess, eventTypeSuccess, volumeEventFail, eventTypeFail)

    local scalePuzzle = TEN.Objects.GetMoveableByName(moveableName)
    local requiredVolume = math.max(1, math.min(scalePuzzle:GetOCB(), 5))
    local itemPresent = Lara:GetWaterSkinStatus(false) > 0 or Lara:GetWaterSkinStatus(true) > 0

    if scalePuzzle:GetItemFlags(SCALE_FLAGS.STATUS) == 1 then
        scalePuzzle:HideInteractionHighlight()
    else 
        scalePuzzle:ShowInteractionHighlight()
    end

    local positionTest = TestPosition(scalePuzzle, Vec3(768, -512, 0), Vec3(1280, 0, 512), 30)

    if positionTest and scalePuzzle:GetItemFlags(SCALE_FLAGS.STATUS) ~= 1 and itemPresent then
        TEN.Inventory.SetFocusedItem(GetWaterSkinObjectID())
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
-- !Description "Reset a Scale Puzzle Object."
-- !Arguments "NewLine, Moveables"
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

-- !Name "Sequence Switches"
-- !Section "Objects"
-- !Description "Create an Elemental Puzzle Object."
-- !Arguments "NewLine, String"
-- !Arguments "NewLine, String, Trigger Triggerer object to activate"
LevelFuncs.Engine.Node.SequenceSwitches = function(switchPrefix, doorPrefix)

    if not LevelVars.Engine.SequenceSwitches[switchPrefix] then
        LevelVars.Engine.SequenceSwitches[switchPrefix] = {
            sequence = {},
            switchStates = { false, false, false },
            activeDoor = nil,
            switchPositions = {},
            switchRotations = {},
            switchRoom = nil,
            switchID = {},
            switchOCB = {},
            switchStatus = nil,
            needsRecreate = false
        }

        -- Cache switch positions/rotations/room on first run
        local state = LevelVars.Engine.SequenceSwitches[switchPrefix]
        for i = 1, 3 do
            local switch = TEN.Objects.GetMoveableByName(switchPrefix .. "_" .. i)
            state.switchPositions[i] = switch:GetPosition()
            state.switchRotations[i] = switch:GetRotation()
            state.switchOCB[i] = switch:GetOCB()
            state.switchID[i] = switch:GetObjectID()
            if i == 1 then
                state.switchStatus = switch:GetStatus()
                state.switchRoom = switch:GetRoomNumber()
            end
        end
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
                sw:SetAnim(0)
                sw:SetStatus(state.switchStatus)
            end
        end
        state.needsRecreate = false
    end

end

