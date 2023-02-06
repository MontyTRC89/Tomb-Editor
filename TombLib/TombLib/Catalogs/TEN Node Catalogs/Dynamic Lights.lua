-- !Name "Add dynamic light to moveable."
-- !Section "Dynamic Light"
-- !Conditional "False"
-- !Description "Add A Dynamic Light To A Moveable's mesh. (Version 1.0)"
-- !Arguments "NewLine, Moveables, 70, Select Moveable To Attach Light To" , "Numerical, [0|100|0] , 30, Select Mesh Number of Moveable \n This can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 70", "Numerical, [0|100|0] , 30, Select range of Light \n(range is in blocks from origin"

LevelFuncs.Engine.Node.DynamicLightMesh = function(	moveable, meshnumber, lightcolor )
		local entity = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
		TEN.Effects.EmitLight(entity, lightcolor, range)		
	
end

-- !Name "Add dynamic light to static."
-- !Section "Dynamic Light"
-- !Conditional "False"
-- !Description "Add A Dynamic Light To A Static Object. (Version 1.0)"
-- !Arguments "NewLine, Statics, Select Static To Attach Light To" , 
-- !Arguments "NewLine, Color, 70", "Numerical, 30, [0|100|0], Select range of Light \n(range is in blocks from origin"

LevelFuncs.Engine.Node.DynamicLightStaticMesh = function(static,lightcolor,range)
	local entity = TEN.Objects.GetStaticByName(static):GetPosition()
	TEN.Effects.EmitLight(entity, lightcolor, range)
end

