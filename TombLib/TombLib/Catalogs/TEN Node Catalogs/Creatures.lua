-- !Name "If creature was hit with weapon..."
-- !Section "Creature state"
-- !Description "Checks if creature was shot with any weapon."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.TestCreatureHitStatus = function(creatureName)
	return TEN.Objects.GetMoveableByName(creatureName):GetHitStatus()
end

-- !Name "Set intelligent creature location"
-- !Section "Creature AI"
-- !Description "Sets the location of intelligent enemies to a specified location.\nOnly to be used with GUIDE, Sophia-Leigh or Von Croy.\nPlace AI_X1 objects with an OCB to create a location."
-- !Arguments "Newline, Moveables, 60, [ guide | sophia_leigh | von_croy ], Creature to set location for."
-- !Arguments "Numerical, 20, [ 0 | 1000 ], Location to set."
-- !Arguments "Boolean, 20, {false}, Debug to console."
LevelFuncs.Engine.Node.SetCreatureLocation = function(objectId, location, consoleDebug)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(objectId, "setting creature location")
    if not mov then return end

    mov:SetLocationAI(location)

    if consoleDebug == true then
        TEN.Util.PrintLog("Location of [ " .. objectId .. " ] set to location " .. location, TEN.Util.LogLevel.INFO)
    end
end

-- !Name "Set creature mood"
-- !Section "Creature AI"
-- !Description "Set creature mood"
-- !Arguments "Newline, Moveables, 80, Moveable to set mood for."
-- !Arguments "Enumeration, 20, [ Attack | Auto | Bored | Escape | Stalk ], Mood to set for creature."
LevelFuncs.Engine.Node.SetCreatureMood = function(moveable, index)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "setting mood")
    if not mov then return end

    local mood = LevelFuncs.Engine.Node.GetCreatureMood(index)
    Objects.Creature(mov):SetMood(mood)
end

-- !Name "If creature mood is..."
-- !Section "Creature AI"
-- !Description "Checks if creature mood is a specified mood."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, 80, Moveable to check mood for."
-- !Arguments "Enumeration, 20, [ Attack | Auto | Bored | Escape | Stalk ], Mood to check for."
LevelFuncs.Engine.Node.TestCreatureMood = function(moveable, index)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "checking mood", true)
    if not mov then return false end

    local mood = LevelFuncs.Engine.Node.GetCreatureMood(index)
    return Objects.Creature(mov):GetMood() == mood
end

-- !Name "Set creature target"
-- !Section "Creature AI"
-- !Description "Set creature target"
-- !Arguments "Newline, Moveables, 50, Moveable to set target for."
-- !Arguments "Moveables, 50, Moveable to set as target."
-- !Arguments "Newline, Boolean, 50, {true}, Retaliate target"
LevelFuncs.Engine.Node.SetCreatureTarget = function(moveable, target, retaliate)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "setting target")
    if not mov then return end

    local targetMov = LevelFuncs.Engine.Node.RequireActiveMoveable(target, "setting target")
    if not targetMov then return end

    local movAI = Objects.Creature(mov)
    movAI:SetTarget(targetMov)

    if retaliate then
        local success, targetMovAI = pcall(Objects.Creature, targetMov)
        if success then
            targetMovAI:SetTarget(mov)
        else
            TEN.Util.PrintLog("moveable [ " .. target .. " ] is not a creature. Retaliation skipped.", TEN.Util.LogLevel.ERROR)
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
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "checking target", true)
    if not mov then return false end

    local targetMov = TEN.Objects.GetMoveableByName(target)
    return Objects.Creature(mov):GetTarget() == targetMov
end

-- !Name "Set creature as friendly"
-- !Section "Creature AI"
-- !Description "Sets creature as friendly to the player."
-- !Arguments "Newline, Moveables, 70, Moveable to set as friendly."
-- !Arguments "Boolean, 30, {true}, Undo friendly if attacked."
LevelFuncs.Engine.Node.SetCreatureFriendly = function(moveable, undoIfAttacked)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "setting friendly state")
    if not mov then return end

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
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "checking friendly state", true)
    if not mov then return false end

    return Objects.Creature(mov):IsFriendly()
end

-- !Name "Set creature poison status"
-- !Section "Creature AI"
-- !Description "Sets whether the creature is poisoned or not."
-- !Arguments "Newline, Moveables, 70, Moveable to set poison status for."
-- !Arguments "Boolean, 30, {true}, Poison"
LevelFuncs.Engine.Node.SetCreaturePoisoned = function(moveable, poisoned)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "setting poison status")
    if not mov then return end

    Objects.Creature(mov):SetPoisoned(poisoned)
end

-- !Name "If creature is poisoned..."
-- !Section "Creature AI"
-- !Description "Checks if the creature is poisoned."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, Moveable to check poison status for."
LevelFuncs.Engine.Node.TestCreaturePoisoned = function(moveable)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "checking poison status", true)
    if not mov then return false end

    return Objects.Creature(mov):GetPoisoned()
end

-- !Name "Set hurt by player status"
-- !Section "Creature AI"
-- !Description "Sets whether the creature has been hurt by the player or not."
-- !Arguments "Newline, Moveables, 70, Moveable to set hurt by player status for."
-- !Arguments "Boolean, 30, {true}, Hurt by player"
LevelFuncs.Engine.Node.SetHurtByPlayer = function(moveable, hurtByPlayer)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "setting hurt-by-player status")
    if not mov then return end

    Objects.Creature(mov):SetHurtByPlayer(hurtByPlayer)
end

-- !Name "If creature is hurt by player..."
-- !Section "Creature AI"
-- !Description "Checks if the creature has been hurt by the player."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, Moveable to check hurt by player status for."
LevelFuncs.Engine.Node.TestHurtByPlayer = function(moveable)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "checking hurt-by-player status", true)
    if not mov then return false end

    return Objects.Creature(mov):GetHurtByPlayer()
end

-- !Name "If creature is jumping..."
-- !Section "Creature AI"
-- !Description "Checks if the creature is currently jumping."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, Moveable to check jumping status for."
LevelFuncs.Engine.Node.TestCreatureJumping = function(moveable)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "checking jumping status", true)
    if not mov then return false end

    return Objects.Creature(mov):GetJumping()
end

-- !Name "If creature is monkey-swinging..."
-- !Section "Creature AI"
-- !Description "Checks if the creature is currently monkey-swinging."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, Moveable to check monkey-swinging status for."
LevelFuncs.Engine.Node.TestCreatureMonkeySwinging = function(moveable)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "checking monkey-swing status", true)
    if not mov then return false end

    return Objects.Creature(mov):GetMonkeying()
end

-- !Name "If creature has reached their goal..."
-- !Section "Creature AI"
-- !Description "Checks if the creature has reached their goal."
-- !Conditional "True"
-- !Arguments "Newline, Moveables, Moveable to check goal status for."
LevelFuncs.Engine.Node.TestCreatureReachedGoal = function(moveable)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "checking goal status", true)
    if not mov then return false end

    return Objects.Creature(mov):GetAtGoal()
end

-- !Name "Set creature goal status"
-- !Section "Creature AI"
-- !Description "Sets whether the creature has reached their goal or not."
-- !Arguments "Newline, Moveables, Moveable to set goal status for."
-- !Arguments "Boolean, 30, {true}, At goal"
LevelFuncs.Engine.Node.SetCreatureReachedGoal = function(moveable, goal)
    local mov = LevelFuncs.Engine.Node.RequireActiveMoveable(moveable, "setting goal status")
    if not mov then return end

    Objects.Creature(mov):SetAtGoal(goal)
end