LevelFuncs.Engine.Node.Shockwave = function(
	pos, 
	innerRadius, 
	outerRadius, 
	color, 
	lifetime, 
	speed, 
	angle, 
	hurtPlayer, 
	rumble, 
	sound
	)

		local origin = GetMoveableByName(pos):GetPosition()
		
		local function earthquake()
			TEN.Effects.MakeEarthquake(20)
			TEN.Misc.Vibrate(20, 0.3)
		end

		TEN.Effects.EmitShockwave(
		origin, 
		innerRadius, 
		outerRadius, 
		color, 
		lifetime, 
		speed, 
		angle, 
		hurtPlayer
		)

		local Rumblecheck
		if rumble == true
		then 
			earthquake()
		end

		local sfxcheck
		if sound == true
		then
			TEN.Misc.PlaySound(197, startingPoint)
		end

end

-- !Name "Add dynamic light to moveable's mesh."
-- !Section "Dynamic Light"
-- !Conditional "False"
-- !Description "Add A Dynamic Light To A Moveable's mesh. (Version 1.0)"
-- !Arguments "NewLine, Moveables, 70, Select Moveable To Attach Light To" , "Numerical, [0|100|0] , 30, Select Mesh Number of Moveable \n This can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 70", "Numerical, [0|100|0] , 30, Select Range of Light \n(Range is in blocks from origin"
-- !Arguments "NewLine, Boolean, Add a random effect to the light? "

LevelFuncs.Engine.Node.DynamicLightMesh = function(
	moveable,
	meshnumber,
	lightcolor,
	range,
	randomcheck
	
	)

		local moveable = GetMoveableByName(moveable)
		local moveablemeshpos = moveable:GetJointPosition(meshnumber)
		
		if randomcheck == false
		then 
			TEN.Effects.EmitLight(moveablemeshpos, lightcolor, Range)
		else 
			TEN.Effects.EmitLight(moveablemeshpos, lightcolor, math.random((Range)/2, Range))
		end

		
	
end

-- !Name "Add Dynamic Light to Static"
-- !Section "Dynamic Light"
-- !Conditional "False"
-- !Description "Add A Dynamic Light To A Static Object. (Version 1.0)"
-- !Arguments "NewLine, Statics, Select Static To Attach Light To" , 
-- !Arguments "NewLine, Color, 70", "Numerical, 30, [0], Select Range of Light \n(Range is in blocks from origin"
-- !Arguments "NewLine, Boolean, Add Sound Effect, 30" , "SoundEffects, 70, Choose sound to play"
-- !Arguments "NewLine, Boolean, Add a Random Effect to the light? "

LevelFuncs.Engine.Node.DynamicLightStaticMesh = function(
	static,
	lightcolor,
	range,
	randomcheck,
	soundeffectcheck,
	soundeffect
	)

		local static = GetStaticByName(static)
		local staticpos = static:GetPosition()
	
		if randomcheck == false
		then
			TEN.Effects.EmitLight(staticpos, lightcolor,range)
		else
			TEN.Effects.EmitLight(staticpos, LightColor, math.random(range))
		end
		
		if soundeffectcheck == true
		then
			TEN.Misc.PlaySound(soundeffect, staticpos)
			
		end
end
