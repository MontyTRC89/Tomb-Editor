-- !Name "Add dynamic light to moveable"
-- !Section "Dynamic lights"
-- !Description "Add a dynamic light to a moveable's mesh."
-- !Arguments "NewLine, Moveables, 70, Select moveable to attach light to." "Numerical, [ 0 | 100 | 0 ] , 30, Select mesh number of moveable. \nThis can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 70, Color of light." "Numerical, [ 0 | 100 | 0 ], 30, Light radius (in sectors)."

LevelFuncs.Engine.Node.MoveableLight = function(moveable, meshnumber, lightcolor, range)

	local entity = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
	TEN.Effects.EmitLight(entity, lightcolor, range)
end

-- !Name "Add dynamic light to static"
-- !Section "Dynamic lights"
-- !Description "Add a dynamic light to a static object."
-- !Arguments "NewLine, Statics, Select static to attach light to."
-- !Arguments "NewLine, Color, 70, Color of light." "Numerical, 30, [ 0 | 100 | 0 ], Light radius (in sectors)."

LevelFuncs.Engine.Node.StaticLight = function(static,lightcolor,range)

	local entity = TEN.Objects.GetStaticByName(static):GetPosition()
	TEN.Effects.EmitLight(entity, lightcolor, range)
end

