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
-- !Arguments "Newline, Moveables, 50, Moveable to set mood for."
-- !Arguments "Enumeration, 50, [ Attack | Auto | Bored | Escape | Stalk ], Mood to set for creature."
LevelFuncs.Engine.Node.SetCreatureMood = function(moveable, index)

    local mood = LevelFuncs.Engine.Node.GetCreatureMood(index)
    local mov = TEN.Objects.GetMoveableByName(moveable)
    local movAI = Objects.Creature(mov)
    movAI:SetMood(mood)
end