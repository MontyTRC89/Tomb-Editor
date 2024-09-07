-- !Name "Add dynamic light to moveable"
-- !Section "Dynamic lights"
-- !Description "Add a dynamic light to a moveable's mesh."
-- !Arguments "NewLine, Moveables, 70, Select moveable to attach light to." "Numerical, [ 0 | 100 | 0 ] , 30, Select mesh number of moveable. \nThis can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 25, Color of light." "Numerical, [ 0 | 100 | 0 ], 25, Light radius (in sectors)."
-- !Arguments "Vector3, 50, Offset (relative)"

LevelFuncs.Engine.Node.MoveableLight = function(moveable, meshnumber, lightcolor, range, effectOffset)

	local entityPos = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
	effectOffset = effectOffset or Vec3(0, 0, 0)  -- Set a default value if nil
	offset = (entityPos) + (effectOffset)
	TEN.Effects.EmitLight(offset, lightcolor, range)
end

-- !Name "Add dynamic light to static"
-- !Section "Dynamic lights"
-- !Description "Add a dynamic light to a static object."
-- !Arguments "NewLine, Statics, Select static to attach light to."
-- !Arguments "NewLine, Color, 25, Color of light." "Numerical, 25, [ 0 | 100 | 0 ], Light radius (in sectors)."
-- !Arguments "Vector3, 50, Offset (relative)"

LevelFuncs.Engine.Node.StaticLight = function(static, lightcolor, range, effectOffset)

	local entityPos = TEN.Objects.GetStaticByName(static):GetPosition()
	local offset = entityPos + effectOffset
	TEN.Effects.EmitLight(offset, lightcolor, range)
end

