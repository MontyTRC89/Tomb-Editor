-- !Name "Add dynamic light to moveable"
-- !Section "Dynamic lights"
-- !Description "Add a dynamic light to a moveable's mesh."
-- !Arguments "NewLine, Moveables, 70, Select moveable to attach light to." "Numerical, [ 0 | 100 | 0 ] , 30, Select mesh number of moveable. \nThis can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 70, Color of light." "Numerical, [ 0 | 100 | 0 ], 30, Light radius (in sectors)."
-- !Arguments "NewLine, 100, Vector3, Offset (relative)"

LevelFuncs.Engine.Node.MoveableLight = function(moveable, meshnumber, lightcolor, range, effectOffset)

	local entityPos = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
	local offset = entityPos + effectOffset
	TEN.Effects.EmitLight(offset, lightcolor, range)
end

-- !Name "Add dynamic light to static"
-- !Section "Dynamic lights"
-- !Description "Add a dynamic light to a static object."
-- !Arguments "NewLine, Statics, Select static to attach light to."
-- !Arguments "NewLine, Color, 70, Color of light." "Numerical, 30, [ 0 | 100 | 0 ], Light radius (in sectors)."
-- !Arguments "NewLine, 100, Vector3, Offset (relative)"

LevelFuncs.Engine.Node.StaticLight = function(static, lightcolor, range, effectOffset)

	local entityPos = TEN.Objects.GetStaticByName(static):GetPosition()
	local offset = entityPos + effectOffset
	TEN.Effects.EmitLight(offset, lightcolor, range)
end

