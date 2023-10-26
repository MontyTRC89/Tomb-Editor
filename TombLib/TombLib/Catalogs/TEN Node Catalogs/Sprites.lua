-- !Name "Draw sprite"
-- !Section "Sprites"
-- !Conditional "False"
-- !Description "Draw a display sprite on the frame that this node is active."
-- !Arguments "Newline, Enumeration, 60, [ DEFAULT_SPRITES | MISC_SPRITES | CUSTOM_SPRITES | FIRE_SPRITES | SMOKE_SPRITES | SPARK_SPRITE | DRIP_SPRITE | EXPLOSION_SPRITES ], Sprite sequence object ID"
--!Arguments "Number, 40, [ 0 | 1000 | 0 ], Sprite ID in sprite sequence"
--!Arguments "Newline, Number, 20, [ 0 | 100 | 1 ], Position X (%)"
--!Arguments "Number, 20, [ 0 | 100 | 1 ], Position Y (%)"
--!Arguments "Number, 20, [ -360 | 360 | 1 ], Rotation"
--!Arguments "Number, 20, [ 0 | 100 | 1 ], Scale X (%)"
--!Arguments "Number, 20, [ 0 | 100 | 1 ], Scale Y (%)"
--!Arguments "NewLine, Color, 50, Color of sprite"
--!Arguments "Number, 50, [ 0 | 500 | 0 ], Draw priority"
--!Arguments "NewLine, Enumeration, 35, [ CENTER | CENTER_TOP | CENTER_BOTTOM | CENTER_LEFT | CENTER_RIGHT | TOP_LEFT | TOP_RIGHT | BOTTOM_LEFT | BOTTOM_RIGHT ], Align mode"
--!Arguments "Enumeration, 32.5, [ FIT | FILL | STRETCH ], Scale mode"
--!Arguments "Enumeration, 32.5, [ OPAQUE | ALPHATEST | ADDITIVE | SUBTRACTIVE | EXCLUDE | SCREEN | LIGHTEN | ALPHABLEND ], Blend mode"
LevelFuncs.Engine.Node.DisplaySprite = function(objectID, spriteID, posX, posY, rot, scaleX, scaleY, color, priority,
                                                alignMode, scaleMode, blendMode)
    local object = LevelFuncs.Engine.Node.GetSpriteSlot(objectID)
    local pos = TEN.Vec2(posX, posY)
    local scale = TEN.Vec2(scaleX, scaleY)
    local alignM = LevelFuncs.Engine.Node.GetDisplaySpriteAlignMode(alignMode)
    local scaleM = LevelFuncs.Engine.Node.GetDisplaySpriteScaleMode(scaleMode)
    local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)
    local sprite = TEN.DisplaySprite(object, spriteID, pos, rot, scale, color)
    sprite:Draw(priority, alignM, scaleM, blendID)
end
