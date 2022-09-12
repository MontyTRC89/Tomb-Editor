-- !Name "If creature was hit with weapon..."
-- !Section "Creature state"
-- !Description "Checks if creature was shot with any weapon."
-- !Conditional "True"
-- !Arguments "NewLine, Moveables"

LevelFuncs.Engine.Node.TestCreatureHitStatus = function(creatureName)
	return TEN.Objects.GetMoveableByName(creatureName):GetHitStatus()
end