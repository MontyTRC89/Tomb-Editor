-- !Name "Set intelligent creature location"
-- !Section "Creature AI"
-- !Description "Sets the location of intelligent enemies to a specified location.\nOnly to be used with GUIDE, Sophia-Leigh or Von Croy.\nPlace AI_X1 objects with an OCB to create a location."
-- !Arguments "Newline, Moveables, 60, [ guide | sophia_leigh | von_croy ], Creature to set location for."
-- !Arguments "Numerical, 20, [ 0 | 1000 ], Location to set.
-- !Arguments "Boolean, 20, {false}, Debug to console."
LevelFuncs.Engine.Node.SetCreatureLocation = function(objectId, location,debug)
    local moveables = TEN.Objects.GetMoveableByName(objectId)
    if moveables:GetStatus() ==  1 then
        moveables:SetLocationAI(location)
    end

    if moveables:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. objectId .. " ] is not active. No location set.",TEN.Util.LogLevel.ERROR)
        
    end
    
    if moveables:GetStatus() ==  1 and debug == true then
        TEN.Util.PrintLog("Location of [ " .. objectId .. " ] set to location " .. location,TEN.Util.LogLevel.INFO)
    end
end

-- !Name "Set creature mood"
-- !Section "Creature AI"
-- !Description "Set creature mood"
-- !Arguments "Newline, Moveables, 80, Moveable to set mood for."
-- !Arguments "Enumeration, 20, [ Attack | Auto | Bored | Escape | Stalk ], Mood to set for creature."
LevelFuncs.Engine.Node.SetCreatureMood = function(moveable, index)

    local mood = LevelFuncs.Engine.Node.GetCreatureMood(index)
    local mov = TEN.Objects.GetMoveableByName(moveable)
    local movAI = Objects.Creature(mov)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. No mood set.",TEN.Util.LogLevel.ERROR) 
        
    end

    if mov:GetStatus() ==  1 then
        movAI:SetMood(mood)
    end
end

-- !Name "If creature mood is..."
-- !Section "Creature AI"
-- !Description "Checks if creature mood is a specified mood."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, 80, Moveable to check mood for."
-- !Arguments "Enumeration, 20, [ Attack | Auto | Bored | Escape | Stalk ], Mood to check for."
LevelFuncs.Engine.Node.TestCreatureMood = function(moveable, index)

    local mood = LevelFuncs.Engine.Node.GetCreatureMood(index)
    local mov = TEN.Objects.GetMoveableByName(moveable)
    local movAI = Objects.Creature(mov)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. No mood set.",TEN.Util.LogLevel.ERROR)
        
    end

    if mov:GetStatus() ==  1 then
         return movAI:GetMood() == mood    
    end
end

-- !Name "Set creature target"
-- !Section "Creature AI"
-- !Description "Set creature target"
-- !Arguments "Newline, Moveables, 50, Moveable to set target for."
-- !Arguments "Moveables, 50, Moveable to set as target."
-- !Arguments "Newline, Boolean, 50, {true}, Retaliate target"
LevelFuncs.Engine.Node.SetCreatureTarget = function(moveable, target, retaliate)

    local mov = TEN.Objects.GetMoveableByName(moveable)
    local movAI = Objects.Creature(mov)
    local targetMov = TEN.Objects.GetMoveableByName(target)
    local targetMovAI = Objects.Creature(targetMov)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. No target set.",TEN.Util.LogLevel.ERROR)
        
    end

    if mov:GetStatus() == 1 then
        movAI:SetTarget(targetMov)
    end

    if retaliate and targetMov:GetStatus() == 1 then
        targetMovAI:SetTarget(mov)
    end
end

-- !Name "If creature target is..."
-- !Section "Creature AI"
-- !Description "Checks if creature target is a specified moveable."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, 50, Moveable to check target for."
-- !Arguments "Moveables, 50, Moveable to check as target."
LevelFuncs.Engine.Node.TestCreatureTarget = function(moveable, target)

    local mov = TEN.Objects.GetMoveableByName(moveable)
    local movAI = Objects.Creature(mov)
    local targetMov = TEN.Objects.GetMoveableByName(target)

    if mov:GetStatus() == 1 then
    return movAI:GetTarget() == targetMov
    end

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. No target set.",TEN.Util.LogLevel.ERROR)
    end
end

-- !Name "Set creature as friendly"
-- !Section "Creature AI"
-- !Description "Sets creature as friendly to the player."
-- !Arguments "Newline, Moveables, 70, Moveable to set as friendly."
-- !Arguments "Boolean, 30, {true}, Undo friendly if attacked."
LevelFuncs.Engine.Node.SetCreatureFriendly = function(moveable, undoIfAttacked) 
    local mov = TEN.Objects.GetMoveableByName(moveable)
    local movAI = Objects.Creature(mov)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot set as friendly.",TEN.Util.LogLevel.ERROR)   
    end

    if mov:GetStatus() == 1 then
        movAI:SetFriendly(true)

        if undoIfAttacked and movAI:GetHurtByPlayer() == true then
            movAI:SetFriendly(false)
        end

        if movAI:GetHurtByPlayer() == true then
            movAI:SetFriendly(false)
        end
    end
end

-- !Name "If creature is friendly..."
-- !Section "Creature AI"
-- !Description "Checks if creature is friendly to the player."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, 50, Moveable to check."
LevelFuncs.Engine.Node.TestCreatureFriendly = function(moveable) 
    local mov = TEN.Objects.GetMoveableByName(moveable)
    local movAI = Objects.Creature(mov)

    if mov:GetStatus() == 1 then
         return movAI:IsFriendly()
    end

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot check if friendly.",TEN.Util.LogLevel.ERROR)
    end
end