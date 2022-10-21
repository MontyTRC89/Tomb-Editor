-- !Name "Toggle flipmap"
-- !Section "Room state"
-- !Description "Sets specified flipmap number, if it's not set, or unsets it if it is."
-- !Arguments "Numerical, 15, [ 0 | 16 | 0 ]"

LevelFuncs.Engine.Node.ToggleFlipMap = function(flipmapNumber)
	TEN.Misc.FlipMap(flipmapNumber)
end