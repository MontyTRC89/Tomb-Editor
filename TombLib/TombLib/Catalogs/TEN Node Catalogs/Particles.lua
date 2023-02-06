--!Name "Particle Generator (Moveables)"
--!Section "Particles"
--!Conditional "False"
--!Description "Emit particles from a moveable."
--!Arguments "NewLine, Moveables, 70, The moveable particles will spawn from." "Numerical, 30, [0 | 100], Mesh Number."
--!Arguments "NewLine, Vector3, 100, [ -32000 | 32000 ], Velocity X Y Z"
--!Arguments "NewLine, Numerical, 33, [ 0 | 31 | 0 ], Choose sprite for emitter.\nSprites are based on the DEFAULT_SPRITES slot in Wadtool\n0 = Flame Emitter\n1 = Underwater blood\n2 = Waterfall\n3 = Mist\n4 = Splash Ring 1\n5 = Splash Ring 2\n6 = Splash Ring 3\n7 = Splash Ring 4\n8 = Water Splash\n9 = Water Ring\n11 = Specular\n13 = Underwater Bubble\n14 = Underwater Dust\n15 = Blood\n28 = Lightning\n29 = Lensflare Ring\n30 = Lensflare Ring 2 \n31 = Lensflare Sundisc\n32 = Lensflare Bright Spark"  "Numerical, 33, [ -32768 | 32767 | 0 ], Gravity" "Numerical, 33, [ -32000 | 32000 | 1 ], Rotation" 
--!Arguments "NewLine, Color, 50, Start Color", "Color, 50, End Color"
--!Arguments "NewLine, Enumeration, 100, [Opaque | Alpha test | Add | No Z test | Subtract | Wireframe | Exclude | Screen | Lighten | Alpha blend], Blend type for particles"
--!Arguments "NewLine, Numerical, 33, [ -32000 | 32000 | 0 ], Start size." "Numerical, 33, [ -32000 | 32000 | 0 ], End size." "Numerical, 33, [ 0 | 32000 | 0 ], Lifetime (in seconds)"
--!Arguments "NewLine, Boolean, 50, Damage." , "Boolean, 50, Poison."

	LevelFuncs.Engine.Node.MoveableParticles=function(entity, meshnum, velocity, spriteID, gravity, rotation, startColor, endColor, blendID, startSize, endSize, life, damage, poison)

		local origin = TEN.Objects.GetMoveableByName(entity):GetJointPosition(meshnum)
		local blendmode = LevelFuncs.Engine.Node.GetBlendMode(blendID)

		TEN.Effects.EmitParticle(origin, velocity, spriteID, gravity, rotation, startColor, endColor, blendmode, startSize, endSize, life, damage, poison)
		
	end
	
--!Name "Particle Generator (Statics)"
--!Section "Particles"
--!Conditional "False"
--!Description "Emit particles from a static object."
--!Arguments "NewLine, Statics, 50, The static particles will spawn from." "Vector3, 50, [ -32000 | 32000 ], Velocity X Y Z"
--!Arguments "NewLine, Numerical, 33, [ 0 | 31 | 0 ], Choose sprite for emitter.\nSprites are based on the DEFAULT_SPRITES slot in Wadtool\n0 = Flame Emitter\n1 = Underwater blood\n2 = Waterfall\n3 = Mist\n4 = Splash Ring 1\n5 = Splash Ring 2\n6 = Splash Ring 3\n7 = Splash Ring 4\n8 = Water Splash\n9 = Water Ring\n11 = Specular\n13 = Underwater Bubble\n14 = Underwater Dust\n15 = Blood\n28 = Lightning\n29 = Lensflare Ring\n30 = Lensflare Ring 2 \n31 = Lensflare Sundisc\n32 = Lensflare Bright Spark"  "Numerical, 33, [ -32768 | 32767 | 0 ], Gravity" "Numerical, 33, [ -32000 | 32000 | 1 ], Rotation" 
--!Arguments "NewLine, Color, 50, Start Color", "Color, 50, End Color"
--!Arguments "NewLine, Enumeration, 100, [Opaque | Alpha test | Add | No Z test | Subtract | Wireframe | Exclude | Screen | Lighten | Alpha blend], Blend type for particles"
--!Arguments "NewLine, Numerical, 33, [ -32000 | 32000 | 0 ], Start size." "Numerical, 33, [ -32000 | 32000 | 0 ], End size." "Numerical, 33, [ 0 | 32000 | 0 ], Lifetime (in seconds)"
--!Arguments "NewLine, Boolean, 50, Damage." , "Boolean, 50, Poison."

	LevelFuncs.Engine.Node.StaticParticles=function(entity, velocity, spriteID, gravity, rotation, startColor, endColor, blendID, startSize, endSize, life, damage, poison)

		local origin = TEN.Objects.GetStaticByName(entity):GetPosition()
		local blendmode = LevelFuncs.Engine.Node.GetBlendMode(blendID)

		TEN.Effects.EmitParticle(origin, velocity, spriteID, gravity, rotation, startColor, endColor, blendmode, startSize, endSize, life, damage, poison)
		
	end

--!Name "Emit lightning arc"
--!Section "Particles"
--!Conditional "False"
--!Description "Emit a lightning arc between two points in 3D space"
--!Arguments "Newline, Moveables, 50, Source Position", "Moveables, 50, Destination Position"
--!Arguments "NewLine, Color, 100, Color of Lightning Effect"
--!Arguments "Newline, Number, 25, [ 0 | 4.2 | 1 ], Lifetime in seconds." , "Number, 25, [ 1 | 255 | 0 ], Amplitude (strength) of effect", "Number, 25, [ 1 | 127 | 0 ], Beam Width", "Number, 25, [ 1 | 127 | 0 ], Detail of effect\n 1 is 1 segment between the two points for example."
--!Arguments "Newline, Boolean, 50, Toggle smooth effect.", "Boolean, 50, Toggle end drift."
--!Arguments "NewLine, Boolean, 50, Add source light" , "Boolean, 50, Add destination light"

LevelFuncs.Engine.Node.LightningArc = function(source, dest, color, lifetime, amplitude, beamWidth, detail, smooth, endDrift, sourcelight, destlight)
	
	local randomiserX = math.random(-64,64)
	local randomiserZ = math.random(-256,256)
	
    local entity = TEN.Objects.GetMoveableByName(source)
	
    local startingpoint = entity:GetPosition()
		startingpoint.x = startingpoint.x - randomiserX
		startingpoint.y = startingpoint.y - 64
		startingpoint.z = startingpoint.z - randomiserZ 	
		
	local endingpoint = TEN.Objects.GetMoveableByName(dest):GetPosition()
		endingpoint.x = endingpoint.x - randomiserX
		endingpoint.y = endingpoint.y - 64
		endingpoint.z = endingpoint.z - randomiserZ 
		
	local beamrandom = math.random((beamWidth-10),(beamWidth+10))
	local amprandom =  math.random((amplitude-10),(amplitude+10))
	
	local sourceLightCheck 
		if sourcelight == true
		then
			TEN.Effects.EmitLight(startingpoint, color, math.random(1,10))
		end

	local destLightCheck 
		if destlight == true
		then 
			TEN.Effects.EmitLight(endingpoint, color, math.random(1,10))
		end
	TEN.Effects.EmitLightningArc(startingpoint, endingpoint, color, lifetime, amprandom, beamrandom, detail, smooth, endDrift)
	

end

--!Name "Emit shockwave"
--!Section "Particles"
--!Description "Emit a shockwave effect."
--!Arguments "NewLine, Moveables, 70, Origin point of shockwave" "Numerical, 30, [0 | 100], Mesh Number (optional)."
--!Arguments "NewLine, Number, 50, [ 0 | 10400 | 0 ], Inner Radius" "Number, 50, [0|10400|0], Outer Radius."
--!Arguments "NewLine, Color, 100, Color of shockwave effect"
--!Arguments "NewLine, Numerical, 33, [ 0 | 8.5 | 1 ], Lifetime of effect (in seconds)" , "Number, 33, [ 0 | 500 | 0 ], Speed" , "Number, 33, [ -360 | 360 |0 ], X axis rotation"
--!Arguments "NewLine, Boolean, 50, Damage" "Boolean, 50, Randomise spawn point"

	LevelFuncs.Engine.Node.Shockwave = function(pos, meshnum, innerRadius, outerRadius, color, lifetime, speed, angle, damage, randomise)
			
		local randomiser = math.random(0,14)
		local radiusInVar = (innerRadius + randomiser)
		local radiusOutVar = ((outerRadius + randomiser) ^ 2)
		print (radiusInVar, radiusOutVar)
				
			local randomMesh
			if randomise == true 
			then
				local origin = TEN.Objects.GetMoveableByName(pos):GetJointPosition(randomMesh)
				TEN.Effects.EmitShockwave(origin, radiusInVar, radiusOutVar, color, lifetime, speed, angle, damage)
					else
						local origin = TEN.Objects.GetMoveableByName(pos):GetJointPosition(meshnum)
						TEN.Effects.EmitShockwave(origin, radiusInnerVar, radiusOutVar, color, lifetime, speed, angle, damage)
				
			end
	end