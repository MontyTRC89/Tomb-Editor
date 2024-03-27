﻿-- !Name "Set hit points for all moveables"
-- !Section "Batch actions"
-- !Description "Set hit points for all moveables of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use"  "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, Numerical, [ 0 | 32767 | 0 ], Hit points to set"

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
-- !Arguments "NewLine, Numerical, [ -32768 | 32767 | 0 ], OCB value to set"

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
-- !Arguments "NewLine, Numerical, [ 0 | 1000 ], Animation ID to set" 

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
-- !Section "Batch actions"
-- !Description "Meshswap all moveables of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, WadSlots, 65, Object ID to use"  "String, 35, Word to search in a moveable name"
-- !Arguments "NewLine, WadSlots, Meshswap object ID to use"

LevelFuncs.Engine.Node.MeshswapForAllMoveables = function(objectId, namePart, slotID)
	local moveables = GetMoveablesBySlot(objectId)
    local noNameSearch = (namePart == nil or namePart == "")

    for i, moveable in pairs(moveables) do
        if noNameSearch or (string.find(moveable:GetName(), namePart)) then
            for mesh = 0, (moveable:GetMeshCount() - 1) do
                moveable:SwapMesh(mesh, slotID)
            end
        end
    end
end

-- !Name "Set hit points for all static meshes"
-- !Section "Batch actions"
-- !Description "Set hit points for all static meshes of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, Numerical, 34, [ 0 | 1000 | 0 ], Static mesh ID to use"  "String, 33, Word to search in a moveable name"
-- !Arguments "Numerical, 33, [ 0 | 32767 | 0 ], Hit points to set"

LevelFuncs.Engine.Node.SetHitPointsForAllStatics = function(slotId, namePart, hitPoints)
	local statics =  GetStaticsBySlot(slotId)
    local noNameSearch = (namePart == nil or namePart == "")

    for i, stat in pairs(statics) do
        if noNameSearch or (string.find(stat:GetName(), namePart)) then
            stat:SetHP(hitPoints)
        end
    end
end

-- !Name "Set collision type for all static meshes"
-- !Section "Batch actions"
-- !Description "Set collision type (soft or hard) for all static meshes of a given type and optionally with a particular word in their name."
-- !Arguments "NewLine, Numerical, 34, [ 0 | 1000 | 0 ], Static mesh ID to use"  "String, 33, Word to search in a moveable name"
-- !Arguments "Boolean, 33, {True}, Hard collision"

LevelFuncs.Engine.Node.SetCollisionModeForAllStatics = function(slotId, namePart, softCollision)
	local statics = GetStaticsBySlot(slotId)
    local noNameSearch = (namePart == nil or namePart == "")

    for i, stat in pairs(statics) do
        if noNameSearch or (string.find(stat:GetName(), namePart)) then
            stat:SetSolid(softCollision)
        end
    end
end