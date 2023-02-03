--!Name "Emit particle from a moveable"
--!Section "Particles"
--!Description "Emits particle from moveable position. To emit particle from a particular mesh, use `Emit particle from a mesh` node.\nThis effect is active for a single frame only."
--!Arguments "NewLine, Moveables, The moveable where the particles will spawn from"
--!Arguments "NewLine, Vector3, 100, [-32000 | 32000], Velocity X Y Z\n Please note that the Y value is inversed. Positive values are down and negative is up"
--!Arguments "NewLine, Numerical, 100, [0 | 100], Choose sprite for emitter.\nSprites are based on the DEFAULT_SPRITES slot in Wadtool\n0 = Flame Emitter\n1 = Underwater blood\n2 = Waterfall\n3 = Mist\n4 = Splash Ring 1\n5 = Splash Ring 2\n6 = Splash Ring 3\n7 = Splash Ring 4\n8 = Water Splash\n9 = Water Ring\n11 = Specular\n13 = Underwater Bubble\n14 = Underwater Dust\n15 = Blood\n28 = Lightning\n29 = Lensflare Ring\n30 = Lensflare Ring 2 \n31 = Lensflare Sundisc\n32 = Lensflare Bright Spark"
--!Arguments "NewLine, Numerical, 100, [-32768 | 32767], Gravity X Y\n Please note that the Y value is inversed. Positive values are down and negative is up"
--!Arguments "NewLine, Numerical, 100, [-32000 | 32000], Rotation X Y\n Please note that the Y value is inversed. Positive values are down and negative is up"
--!Arguments "NewLine, Color, 50, Start color", "Color, 50, End color"
--!Arguments "NewLine, Enumeration, 100, [Opaque | Alpha test | Add | No Z test | Subtract | Wireframe | Exclude | Screen | Lighten | Alpha blend], Blend type for particles"
--!Arguments "NewLine, Numerical, 100, [-32000 | 32000], Start size"
--!Arguments "NewLine, Numerical, 100, [-32000 | 32000], End size"
--!Arguments "NewLine, Numerical, 100, [-32000 | 32000], Lifetime (in seconds)"
--!Arguments "NewLine, Boolean, 50, Add damage?", "Boolean, 50, Add poison?"

LevelFuncs.Engine.Node.MoveableParticleEmitter = function(
    moveable, 
    velocity, 
    spriteID, 
    gravity, 
    rotation, 
    startColor, 
    endColor, 
    blendID, 
    startSize, 
    endSize, 
    life, 
    damage, 
    poison
)

		local pos = GetMoveableByName(moveable):GetPosition()
		local blendMode = LevelFuncs.Engine.Node.GetBlendMode(blendID)

		TEN.Effects.EmitParticle(
			pos, 
			velocity, 
			spriteID, 
			gravity, 
			rotation, 
			startColor, 
			endColor, 
			blendMode, 
			startSize, 
			endSize, 
			life, 
			damage, 
			poison
		)

end

--!Name "Emit particles from a mesh"
--!Section "Particles"
--!Conditional "False"
--!Description "Emit particles from a specified mesh.\nThis effect is drawn every frame."
--!Arguments "NewLine, Moveables, 70, Choose the moveable where the particles will spawn from." "Numerical, 30, [0 | 100], Mesh Number.\nThis can be found in Wadtool in the Animation Editor"
--!Arguments "NewLine, Vector3, 100, [-32000 | 32000], Velocity X Y Z\n Please note that the Y value is inversed. Positive values are down and negative is up"
--!Arguments "NewLine, Numerical, 100, [0 | 31], Choose sprite for emitter.\nSprites are based on the DEFAULT_SPRITES slot in Wadtool\n0 = Flame Emitter\n1 = Underwater blood\n2 = Waterfall\n3 = Mist\n4 = Splash Ring 1\n5 = Splash Ring 2\n6 = Splash Ring 3\n7 = Splash Ring 4\n8 = Water Splash\n9 = Water Ring\n11 = Specular\n13 = Underwater Bubble\n14 = Underwater Dust\n15 = Blood\n28 = Lightning\n29 = Lensflare Ring\n30 = Lensflare Ring 2 \n31 = Lensflare Sundisc\n32 = Lensflare Bright Spark"
--!Arguments "NewLine, 100, [-32768 |  32767], Gravity X Y\n Please note that the Y value is inversed. Positive values are down and negative is up"
--!Arguments "NewLine, 100, [-32000 | 32000], Rotation X Y\n Please note that the Y value is inversed. Positive values are down and negative is up"
--!Arguments "NewLine, Color, 50, Start Color", "Color, 50, End Color"
--!Arguments "NewLine, Enumeration, 100, [Opaque | Alpha test | Add | No Z test | Subtract | Wireframe | Exclude | Screen | Lighten | Alpha blend], Blend type for particles"
--!Arguments "NewLine, 100, [-32000 | 32000], Start size."
--!Arguments "NewLine, 100, [-32000 | 32000], End size."
--!Arguments "NewLine, 100, [-32000 | 32000], Lifetime (in seconds)"
--!Arguments "NewLine, Boolean, 50, Add damage?" , "Boolean, 50, Add poison?"

	LevelFuncs.Engine.Node.MeshParticleEmitter=function(
		activator, 
		meshnum, 
		velocity, 
		spriteID, 
		gravity, 
		rotation, 
		startColor, 
		endColor, 
		blendID, 
		startSize, 
		endSize, 
		life, 
		damage, 
		poison
		)

		local pos = GetMoveableByName(activator):GetJointPosition(meshnum)
		local blendmode = LevelFuncs.Engine.Node.GetBlendMode(blendID)


		TEN.Effects.EmitParticle(
			pos, 
			velocity, 
			spriteID, 
			gravity, 
			rotation, 
			startColor, 
			endColor, 
			blendmode, 
			startSize, 
			endSize, 
			life, 
			damage, 
			poison
		)
		
	end

--!Name "Emit lightning arc"
--!Section "Particles"
--!Conditional "False"
--!Description "Emit a lightning arc between two points in 3D space"
--!Arguments "Newline, Moveables, 50, Source Position", "Moveables, 50, Destination Position"
--!Arguments "NewLine, Color, 100, Color of Lightning Effect"
--!Arguments "Newline, Number, 25, [ 0 | 4.2 | 1 ], Lifetime in seconds." , "Number, 25, [ 1 | 255 | 0 ], Amplitude (strength) of effect", "Number, 25, [ 1 | 127 | 0 ], Beam Width", "Number, 25, [ 1 | 127 | 0 ], Detail of effect\n 1 is 1 segment between the two points for example."
--!Arguments "Newline, Boolean, 50, Toggle smooth effect.", "Boolean, 50, Toggle end drift."
--!Arguments "NewLine, Boolean, 30, Add source light" , "Boolean, 40, Add destination light" , "Boolean, 30, Toggle SFX"

LevelFuncs.Engine.Node.LightningArc = function(
	source, 
	dest, 
	color, 
	lifetime, 
	amplitude, 
	beamWidth, 
	detail, 
	smooth, 
	endDrift, 
	sourcelight, 
	destlight, 
	sound
	)
	
	local randomiserX = math.random(-64,64)
	local randomiserZ = math.random(-256,256)
	
    local entity = GetMoveableByName(source)
	
    local startingpoint = entity:GetPosition()
		startingpoint.x = startingpoint.x - randomiserX
		startingpoint.y = startingpoint.y - 64
		startingpoint.z = startingpoint.z - randomiserZ 		

		
    local endingpoint = GetMoveableByName(dest):GetPosition()
		endingpoint.x = endingpoint.x - randomiserX
		endingpoint.y = endingpoint.y - 64
		endingpoint.z = endingpoint.z - randomiserZ 
	
	local endingpoint = GetMoveableByName(dest):GetPosition()
	
	local beamrandom = math.random((beamWidth-10),(beamWidth+10))
	local amprandom =  math.random((amplitude-10),(amplitude+10))
	
	print(beamrandom, amprandom)

	TEN.Effects.EmitLightningArc(
		startingpoint,
		endingpoint, 
		color, 
		lifetime, 
		amprandom, 
		beamrandom,
		detail, 
		smooth, 
		endDrift

		)
	
		local sfxcheck
			if sound == true
			then
				TEN.Misc.PlaySound(197, startingpoint)
			end

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
end

--!Name "Emit shockwave"
--!Section "Particles"
--!Description "Emit a shockwave effect from a moveable's origin point."
--!Arguments "NewLine, Moveables, Choose origin point of shockwave"
--!Arguments "NewLine, Number, 50, [ 0 | 10400 | 0 ], Inner Radius" "Number, 50, [0|10400|0], Outer Radius."
--!Arguments "NewLine, Color, 100, Color of shockwave effect"
--!Arguments "NewLine, Numerical, 33, [ 0 | 8.5 | 1 ], Lifetime of effect (in seconds)" , "Number, 33, [ 0 | 500 | 0 ], Speed" , "Number, 33, [ -360 | 360 |0 ], Angle around the X axis"
--!Arguments "NewLine, Boolean, 50, Toogle Lara Damage" , "Boolean, 50, Toggle Rumble"

	LevelFuncs.Engine.Node.Shockwave = function(
		pos, 
		innerRadius, 
		outerRadius, 
		color, 
		lifetime, 
		speed, 
		angle, 
		hurtPlayer, 
		rumble
		)

			local origin = GetMoveableByName(pos):GetPosition() 
				print(pos.y)
			
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
				TEN.Misc.PlaySound(197, startingpoint)
			end

	end