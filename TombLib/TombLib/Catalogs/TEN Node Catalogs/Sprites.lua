-- !Name "Draw sprite"
-- !Section "Sprites"
-- !Conditional "False"
-- !Description "Draw a display sprite on the frame that this node is active."
-- !Arguments "Newline, Enumeration, 62, [ SKY_GRAPHICS | DEFAULT_SPRITES | MISC_SPRITES | CUSTOM_SPRITES | FIRE_SPRITES | SMOKE_SPRITES | SPARK_SPRITE | DRIP_SPRITE | EXPLOSION_SPRITES | MOTORBOAT_FOAM_SPRITES | RUBBER_BOAT_WAVE_SPRITES | SKIDOO_SNOW_TRAIL_SPRITES | KAYAK_PADDLE_TRAIL_SPRITE | KAYAK_WAKE_SPRTIES | BINOCULAR_GRAPHIC | LASER_SIGHT_GRAPHIC | CAUSTICS_TEXTURES | BAR_BORDER_GRAPHIC | HEALTH_BAR_TEXTURE | AIR_BAR_TEXTURE | DASH_BAR_TEXTURE | SFX_BAR_TEXTURE |	CROSSHAIR ], Sprite sequence object ID"
--!Arguments "Number, 19.3, [ 0 | 9999 | 0 ], Sprite ID in sprite sequence"
--!Arguments "Number, 20, [ 0 | 9999 | 0 ], Draw priority"
--!Arguments "Newline, Number, 20, [ 0 | 100 | 1 ], Position X (%)"
--!Arguments "Number, 20, [ 0 | 100 | 1 ], Position Y (%)"
--!Arguments "Number, 20, [ -360 | 360 | 1 ], Rotation"
--!Arguments "Number, 20, [ 0 | 100 | 1 ], Scale X (%)"
--!Arguments "Number, 20, [ 0 | 100 | 1 ], Scale Y (%)"
--!Arguments "NewLine, Color, 14.6, Color of sprite"
--!Arguments "Enumeration, 34.81, [ CENTER | CENTER_TOP | CENTER_BOTTOM | CENTER_LEFT | CENTER_RIGHT | TOP_LEFT | TOP_RIGHT | BOTTOM_LEFT | BOTTOM_RIGHT ], Align mode"
--!Arguments "Enumeration, 21.5, [ FIT | FILL | STRETCH ], Scale mode"
--!Arguments "Enumeration, 28.6, [ OPAQUE | ALPHATEST | ADDITIVE | SUBTRACTIVE | EXCLUDE | SCREEN | LIGHTEN | ALPHABLEND ], Blend mode"
LevelFuncs.Engine.Node.DisplaySprite = function(objectID, spriteID, priority, posX, posY, rot, scaleX, scaleY, color,
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
