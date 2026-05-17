-- !Name "Set intelligent creature location"
-- !Section "Creature Mood"
-- !Description "Sets the location of intelligent enemies to a specified location.\nOnly to be used with GUIDE, Sophia-Leigh or Von Croy.\nPlace AI_X1 objects with an OCB to create a location."
-- !Arguments "Newline, WadSlots, 70, [ GUIDE | SOPHIA_LEIGH | VON_CROY ], Creature to set location for."
-- !Arguments "Numerical, 30, [ 0 | 1000 ], Location to set.
LevelFuncs.Engine.Node.SetCreatureLocation = function(objectId, location)
    local moveables = TEN.Objects.GetMoveablesBySlot(objectId)

    for _, moveable in pairs(moveables) do
        if moveable:GetStatus() == Objects.MoveableStatus.ACTIVE then
            moveable:SetLocationAI(location)
            print("Location set for " .. moveable:GetName() .. " to location: " .. location .. ".")
        end

        if moveable:GetStatus() == Objects.MoveableStatus.INACTIVE then
            print("Warning: " .. moveable:GetName() .. " is inactive. Location not set.")
        end
    end
end

-- !Name "Set creature mood"
-- !Section "Creature Mood"
-- !Description "Set creature mood"
-- !Arguments "Newline, Moveables, 50, Moveable to set mood for."
-- !Arguments "Enumeration, 50, [ Attack | Auto | Bored | Escape | Stalk ], Mood to set for creature."
LevelFuncs.Engine.Node.SetCreatureMood = function(moveable, index)

    local mood = LevelFuncs.Engine.Node.GetCreatureMood(index)
    local mov = TEN.Objects.GetMoveableByName(moveable)
    local movAI = Objects.Creature(mov)
    movAI:SetMood(mood)
end