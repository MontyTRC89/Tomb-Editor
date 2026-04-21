LevelVars.Engine.SequenceSwitches = {}

-- !Name "Create a Sequence Switches Puzzle"
-- !Section "Objects"
-- !Description "Create a Sequence Switch and Door puzzle."
-- !Arguments "NewLine, String, Prefix for switch names (Switch for Switch_1 Switch_2 Switch_3)"
-- !Arguments "NewLine, String, Prefix for door names (Door for Door_1 Door_2 Door_3 Door_4 Door_5 Door_6)"
LevelFuncs.Engine.Node.SequenceSwitches = function(switchPrefix, doorPrefix)

    local SMALL_SWITCH_SOUND = 269 
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
        TEN.Sound.PlaySound(SMALL_SWITCH_SOUND)

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

