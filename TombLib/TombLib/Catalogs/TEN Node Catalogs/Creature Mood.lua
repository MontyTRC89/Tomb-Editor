-- !Name "Set intelligent creature location"
-- !Section "Creature AI"
-- !Description "Sets the location of intelligent enemies to a specified location.\nOnly to be used with GUIDE, Sophia-Leigh or Von Croy.\nPlace AI_X1 objects with an OCB to create a location."
-- !Arguments "Newline, Moveables, 60, [ guide | sophia_leigh | von_croy ], Creature to set location for."
-- !Arguments "Numerical, 20, [ 0 | 1000 ], Location to set."
-- !Arguments "Boolean, 20, {false}, Debug to console."
LevelFuncs.Engine.Node.SetCreatureLocation = function(objectId, location, debug)
    local moveables = TEN.Objects.GetMoveableByName(objectId)
    if moveables:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. objectId .. " ] is not active. No location set.",TEN.Util.LogLevel.ERROR)
        return
    end

    moveables:SetLocationAI(location)

    if debug == true then
        TEN.Util.PrintLog("Location of [ " .. objectId .. " ] set to location " .. location,TEN.Util.LogLevel.INFO)
    end
end

-- !Name "Set creature mood"
-- !Section "Creature AI"
-- !Description "Set creature mood"
-- !Arguments "Newline, Moveables, 80, Moveable to set mood for."
-- !Arguments "Enumeration, 20, [ Attack | Auto | Bored | Escape | Stalk ], Mood to set for creature."
LevelFuncs.Engine.Node.SetCreatureMood = function(moveable, index)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. No mood set.",TEN.Util.LogLevel.ERROR) 
        return
    end

    local mood = LevelFuncs.Engine.Node.GetCreatureMood(index)
    local movAI = Objects.Creature(mov)

    movAI:SetMood(mood)
end

-- !Name "If creature mood is..."
-- !Section "Creature AI"
-- !Description "Checks if creature mood is a specified mood."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, 80, Moveable to check mood for."
-- !Arguments "Enumeration, 20, [ Attack | Auto | Bored | Escape | Stalk ], Mood to check for."
LevelFuncs.Engine.Node.TestCreatureMood = function(moveable, index)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot check mood.",TEN.Util.LogLevel.ERROR)
        return false
    end

    local mood = LevelFuncs.Engine.Node.GetCreatureMood(index)
    local movAI = Objects.Creature(mov)

    return movAI:GetMood() == mood
end

-- !Name "Set creature target"
-- !Section "Creature AI"
-- !Description "Set creature target"
-- !Arguments "Newline, Moveables, 50, Moveable to set target for."
-- !Arguments "Moveables, 50, Moveable to set as target."
-- !Arguments "Newline, Boolean, 50, {true}, Retaliate target"
LevelFuncs.Engine.Node.SetCreatureTarget = function(moveable, target, retaliate)

    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. No target set.",TEN.Util.LogLevel.ERROR)
        return
    end

    local movAI = Objects.Creature(mov)
    local targetMov = TEN.Objects.GetMoveableByName(target)

    movAI:SetTarget(targetMov)

    if retaliate and targetMov:GetStatus() == 1 then
        local success, targetMovAI = pcall(Objects.Creature, targetMov)
        if success then
            targetMovAI:SetTarget(mov)
        else
            TEN.Util.PrintLog("moveable [ " .. target .. " ] is not a creature. Retaliation skipped.",TEN.Util.LogLevel.ERROR)
        end
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

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. No target set.",TEN.Util.LogLevel.ERROR)
        return false
    end

    local movAI = Objects.Creature(mov)
    local targetMov = TEN.Objects.GetMoveableByName(target)

    return movAI:GetTarget() == targetMov
end

-- !Name "Set creature as friendly"
-- !Section "Creature AI"
-- !Description "Sets creature as friendly to the player."
-- !Arguments "Newline, Moveables, 70, Moveable to set as friendly."
-- !Arguments "Boolean, 30, {true}, Undo friendly if attacked."
LevelFuncs.Engine.Node.SetCreatureFriendly = function(moveable, undoIfAttacked) 
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot set as friendly.",TEN.Util.LogLevel.ERROR)   
        return
    end

    local movAI = Objects.Creature(mov)

    movAI:SetFriendly(true)

    if undoIfAttacked and movAI:GetHurtByPlayer() == true then
        movAI:SetFriendly(false)
    end
end

-- !Name "If creature is friendly..."
-- !Section "Creature AI"
-- !Description "Checks if creature is friendly to the player."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, 50, Moveable to check."
LevelFuncs.Engine.Node.TestCreatureFriendly = function(moveable)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot check if friendly.",TEN.Util.LogLevel.ERROR)
        return false
    end

    local movAI = Objects.Creature(mov)

    return movAI:IsFriendly()
end

-- !Name "Set creature poison status"
-- !Section "Creature AI"
-- !Description "Sets whether the creature is poisoned or not."
-- !Arguments "Newline, Moveables, 70, Moveable to set poison status for."
-- !Arguments "Boolean, 30, {true}, Poison"
LevelFuncs.Engine.Node.SetCreaturePoisoned = function(moveable, poisoned)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot set poison status.",TEN.Util.LogLevel.ERROR)   
        return
    end

    local movAI = Objects.Creature(mov)

    movAI:SetPoisoned(poisoned)
end

-- !Name "If creature is poisoned..."
-- !Section "Creature AI"
-- !Description "Checks if the creature is poisoned."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, Moveable to check poison status for."
LevelFuncs.Engine.Node.TestCreaturePoisoned = function(moveable)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot check poison status.",TEN.Util.LogLevel.ERROR)   
        return false
    end

    local movAI = Objects.Creature(mov)

    return movAI:GetPoisoned()
end

-- !Name "Set hurt by player status"
-- !Section "Creature AI"
-- !Description "Sets whether the creature has been hurt by the player or not."
-- !Arguments "Newline, Moveables, 70, Moveable to set hurt by player status for."
-- !Arguments "Boolean, 30, {true}, Hurt by player"
LevelFuncs.Engine.Node.SetHurtByPlayer = function(moveable, hurtByPlayer)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot set hurt by player status.",TEN.Util.LogLevel.ERROR)   
        return
    end

    local movAI = Objects.Creature(mov)

    movAI:SetHurtByPlayer(hurtByPlayer)
end

-- !Name "If creature is hurt by player..."
-- !Section "Creature AI"
-- !Description "Checks if the creature has been hurt by the player."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, Moveable to check hurt by player status for."
LevelFuncs.Engine.Node.TestHurtByPlayer = function(moveable)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot check hurt by player status.",TEN.Util.LogLevel.ERROR)   
        return false
    end

    local movAI = Objects.Creature(mov)

    return movAI:GetHurtByPlayer()
end

-- !Name "If creature is jumping..."
-- !Section "Creature AI"
-- !Description "Checks if the creature is currently jumping. Only works for creatures that can jump."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, [ baddy | enemy_jeep | mafia | raptor | scientist | skeleton | swat_ | von_croy ], Moveable to check jumping status for."
LevelFuncs.Engine.Node.TestCreatureJumping = function(moveable)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot check jumping status.",TEN.Util.LogLevel.ERROR)   
        return false
    end

    local movAI = Objects.Creature(mov)

    return movAI:GetJumping()
end

-- !Name "If creature is monkey-swinging..."
-- !Section "Creature AI"
-- !Description "Checks if the creature is currently monkey-swinging. Only works for creatures that can monkey-swing."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, [ baddy | von_croy ], Moveable to check monkey-swinging status for."
LevelFuncs.Engine.Node.TestCreatureMonkeySwinging = function(moveable)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot check monkey-swing status.",TEN.Util.LogLevel.ERROR)   
        return false
    end

    local movAI = Objects.Creature(mov)

    return movAI:GetMonkeying()
end

-- !Name "If creature has reached their goal..."
-- !Section "Creature AI"
-- !Description "Checks if the creature has reached their goal.\nThis setting may be used to find out whether a creature that is using AI nullmesh objects has reached its currently specified AI nullmesh."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, Moveable to check goal status for."
LevelFuncs.Engine.Node.TestCreatureReachedGoal = function(moveable)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot check goal status.",TEN.Util.LogLevel.ERROR)   
        return false
    end

    local movAI = Objects.Creature(mov)

    return movAI:GetAtGoal()
end

-- !Name "Set creature goal status"
-- !Section "Creature AI"
-- !Description "Sets whether the creature has reached their goal or not.\nThis setting may be used to break out the creature from reaching the next specified AI object nullmesh."
-- !Arguments "Newline, Moveables, Moveable to set goal status for."
-- !Arguments "Boolean, 30, {true}, At goal"
LevelFuncs.Engine.Node.SetCreatureReachedGoal = function(moveable, goal)
    local mov = TEN.Objects.GetMoveableByName(moveable)

    if mov:GetStatus() ~= 1 then
        TEN.Util.PrintLog("moveable [ " .. moveable .. " ] is not active. Cannot set goal status.",TEN.Util.LogLevel.ERROR)   
        return
    end

    local movAI = Objects.Creature(mov)

    movAI:SetAtGoal(goal)
end