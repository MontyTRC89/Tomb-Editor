-- !Name "Set hit points for all moveables"
-- !Section "Batch actions"
-- !Description "Set hit points for all moveables of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use"  "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Numerical, Hit points to set"

LevelFuncs.Engine.Node.SetHitPointsForAllMoveables = function(objectId, namePart, hitPoints)
	local moveables = GetMoveablesBySlot(objectId)
    local noNameSearch = (namePart == nil or namePart == "")

    for i, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetHP(hitPoints)
        end
    end
end

-- !Name "Set OCB for all moveables"
-- !Section "Batch actions"
-- !Description "Set OCB for all moveables of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use"  "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Numerical, OCB value to set"

LevelFuncs.Engine.Node.SetOCBForAllMoveables = function(objectId, namePart, ocb)
	local moveables = GetMoveablesBySlot(objectId)
    local noNameSearch = (namePart == nil or namePart == "")

    for i, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetOCB(ocb)
        end
    end
end

-- !Name "Set color for all moveables"
-- !Section "Batch actions"
-- !Description "Set color for all moveables of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use"  "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Color, {TEN.Color(128,128,128)}, Color to set"

LevelFuncs.Engine.Node.SetColorForAllMoveables = function(objectId, namePart, color)
	local moveables = GetMoveablesBySlot(objectId)
    local noNameSearch = (namePart == nil or namePart == "")

    for i, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetColor(color)
        end
    end
end

-- !Name "Set animation for all moveables"
-- !Section "Batch actions"
-- !Description "Set animation for all moveables of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use"  "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Numerical, 20, [ 0 | 1000 ], Animation ID to set" 

LevelFuncs.Engine.Node.SetAnimationForAllMoveables = function(objectId, namePart, animID)
	local moveables = GetMoveablesBySlot(objectId)
    local noNameSearch = (namePart == nil or namePart == "")

    for i, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetAnim(animID)
        end
    end
end

-- !Name "Set effect for all moveables"
-- !Section "Batch actions"
-- !Description "Set effect for all moveables of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use"  "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Enumeration, [ None | Fire | Sparks | Smoke | Electric ignite | Red ignite ], Effect type to set"

LevelFuncs.Engine.Node.SetEffectForAllMoveables = function(objectId, namePart, effectID)
	local moveables = GetMoveablesBySlot(objectId)
    local noNameSearch = (namePart == nil or namePart == "")

    for i, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            moveable:SetEffect(effectID, timeout)
        end
    end
end

-- !Name "Meshswap all moveables"
-- !Section "Moveable properties"
-- !Description "Meshswap all moveables of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use"  "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, WadSlots, Meshswap object ID to use"

LevelFuncs.Engine.Node.MeshswapForAllMoveables = function(objectId, namePart, slotID)
	local moveables = GetMoveablesBySlot(objectId)
    local noNameSearch = (namePart == nil or namePart == "")

    for i, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            for mesh = 0, (moveable:GetMeshNumber() - 1) do
                moveable:SwapMesh(mesh, slotID)
            end
        end
    end
end

