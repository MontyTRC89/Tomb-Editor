-- !Name "If key is hit..."
-- !Section "Controls"
-- !Description "Checks if specific game key has just been hit (will be true only for 1 frame, until the key is released and hit again)."
-- !Conditional "True"
-- !Arguments "Enumeration, 35, [ Forward | Back | Left | Right | Crouch | Sprint | Walk | Jump | Action | Draw | Flare | Look | Roll | Inventory | Pause | Step Left | Step Right ]"

LevelFuncs.Engine.Node.KeyIsHit = function(keyCode)
	return TEN.Misc.KeyIsHit(keyCode)
end

-- !Name "If key is held..."
-- !Section "Controls"
-- !Description "Checks if specific game key is being held (will be true as long as the player keeps their finger on that key)."
-- !Conditional "True"
-- !Arguments "Enumeration, 35, [ Forward | Back | Left | Right | Crouch | Sprint | Walk | Jump | Action | Draw | Flare | Look | Roll | Inventory | Pause | Step Left | Step Right ]"

LevelFuncs.Engine.Node.KeyIsHeld = function(keyCode)
	return TEN.Misc.KeyIsHeld(keyCode)
end