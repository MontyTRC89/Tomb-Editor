-- !Name "Add dynamic light to moveable."
-- !Section "Dynamic lights"
-- !Conditional "False"
-- !Description "Add a dynamic light to a moveable's mesh. (Version 1.0)"
-- !Arguments "NewLine, Moveables, 70, Select moveable to attach light to" , "Numerical, [0|100|0] , 30, Select mesh number of moveable \n This can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 70", "Numerical, [0|100|0] , 30, Light radius \n(range is in blocks from origin"

LevelFuncs.Engine.Node.DynamicLightMesh = function(	moveable, meshnumber, lightcolor )

		local entity = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
		TEN.Effects.EmitLight(entity, lightcolor, range)		
	
end

-- !Name "Add dynamic light to static."
-- !Section "Dynamic lights"
-- !Conditional "False"
-- !Description "Add a dynamic light to a static object. (Version 1.0)"
-- !Arguments "NewLine, Statics, Select static to attach light to" , 
-- !Arguments "NewLine, Color, 70", "Numerical, 30, [0|100|0], Light radius \nrange is in blocks from origin"

LevelFuncs.Engine.Node.DynamicLightStaticMesh = function(static,lightcolor,range)
		local entity = TEN.Objects.GetStaticByName(static):GetPosition()
		TEN.Effects.EmitLight(entity, lightcolor, range)
end

