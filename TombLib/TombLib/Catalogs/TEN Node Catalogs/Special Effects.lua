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
    blendMode, 
    startSize, 
    endSize, 
    life, 
    damage, 
    poison
)

    local pos = GetMoveableByName(moveable):GetPosition()
    local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)

    TEN.Effects.EmitParticle(
        pos, 
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

end

--!Name "Emit particles from a mesh"
--!Section "Particles"
--!Conditional "False"
--!Description "Emit particles from a specified mesh.\nThis effect is drawn every frame."
--!Arguments "NewLine, Moveables, 70, Choose the moveable where the particles will spawn from." "Numerical, 30, [0 | 100], Mesh Number.\nThis can be found in Wadtool in the Animation Editor"
--!Arguments "NewLine, Vector3, 100, [-32000 | 32000], Velocity X Y Z\n Please note that the Y value is inversed. Positive values are down and negative is up"
--!Arguments "NewLine, Numerical, 100, [0 | 100], Choose sprite for emitter.\nSprites are based on the DEFAULT_SPRITES slot in Wadtool\n0 = Flame Emitter\n1 = Underwater blood\n2 = Waterfall\n3 = Mist\n4 = Splash Ring 1\n5 = Splash Ring 2\n6 = Splash Ring 3\n7 = Splash Ring 4\n8 = Water Splash\n9 = Water Ring\n11 = Specular\n13 = Underwater Bubble\n14 = Underwater Dust\n15 = Blood\n28 = Lightning\n29 = Lensflare Ring\n30 = Lensflare Ring 2 \n31 = Lensflare Sundisc\n32 = Lensflare Bright Spark"
--!Arguments "NewLine, 100, [-32768 |  32767], Gravity X Y\n Please note that the Y value is inversed. Positive values are down and negative is up"
--!Arguments "NewLine, 100, [-32000 | 32000], Rotation X Y\n Please note that the Y value is inversed. Positive values are down and negative is up"
--!Arguments "NewLine, Color, 50, Start Color", "Color, 50, End Color"
--!Arguments "NewLine, Enumeration, 100, [Opaque | Alpha test | Add | No Z test | Subtract | Wireframe | Exclude | Screen | Lighten | Alpha blend], Blend type for particles"
--!Arguments "NewLine, 100, [-32000 | 32000], Start size."
--!Arguments "NewLine, 100, [-32000 | 32000], End size."
--!Arguments "NewLine, 100, [-32000 | 32000], Lifetime (in seconds)"
--!Arguments "NewLine, Boolean, 50, Add damage?" , "Boolean, 50, Add poison?"

LevelFuncs.Engine.Node.MeshParticleEmitter=function(
	pos, 
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

	local pos = GetMoveableByName(moveable):GetJointPosition(meshnum)
	local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)


    TEN.Effects.EmitParticle(
		pos, 
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
	
end

--!Name "Emit particle from a static object"
--!Section "Particles"
--!Description "Emits particle from static object's position.\nThis effect is drawn every frame."
--!Arguments "NewLine, Statics, The static where the particles will spawn from"
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
    static, 
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

    local pos = GetStaticByName(static):GetPosition()
    local blendID = LevelFuncs.Engine.Node.GetBlendMode(blendMode)

    TEN.Effects.EmitParticle(
        pos, 
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
	
end

--!Name "Emit lightning arc"
--!Section "Particles"
--!Conditional "False"
--!Description "Emit a lightning arc between two points in 3D space"
--!Arguments "Newline, Moveables, 50, Source Position", "Moveables, 50, Destination Position"
--!Arguments "NewLine, Color, 100, Color of Lightning Effect"
--!Arguments "Newline, Number, 25, [0 | 4.2 | 1], Lifetime in seconds." , "Number, 25, [1 | 255 | 0], Amplitude (strength) of effect", "Number, 25, [1 | 127 | 0], Beam Width", "Number, 25, [1 | 127 | 0], Detail of effect\n 1 is 1 segment between the two points for example."
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
	sourcelightBool, 
	destlightBool, 
	sound
	)

	local startingPoint = GetMoveableByName(source):GetPosition()
	local endingPoint = GetMoveableByName(dest):GetPosition()
	
	TEN.Effects.EmitLightningArc(
		startingPoint, 
		endingPoint, 
		color, 
		lifetime, 
		amplitude, 
		beamWidth, 
		detail, 
		smooth, 
		endDrift

		)
	
		local sfxcheck
			if sound == true
			then
				TEN.Misc.PlaySound(197, startingPoint)
			end

		local sourceLightCheck 
			if sourcelightBool == true
			then
				TEN.Effects.EmitLight(startingPoint, color, math.random(10, 20))
			end

		local destLightCheck 
			if destlightBool == true
			then 
				TEN.Effects.EmitLight(endingPoint, color, math.random(10, 20))
			end
end

--!Name "Emit shockwave"
--!Section "Particles"
--!Description "Emit a shockwave effect from a moveable's origin point."
--!Arguments "NewLine, Moveables, Choose origin point of shockwave"
--!Arguments "NewLine, Number, 50, [0|10400|0], Inner Radius" "Number, 50, [0|10400|0], Outer Radius."
--!Arguments "NewLine, Color, 100, Color of shockwave effect"
--!Arguments "NewLine, Numerical, 33, [0|8.5|1], Lifetime of effect (in seconds)" , "Number, 33, [0|500|0], Speed" , "Number, 33, [-360|360|0], Angle around the X axis"
--!Arguments "NewLine, Boolean, 30, Toogle Lara Damage" , "Boolean, 33, Toggle Rumble", "Boolean, 33, Toggle SFX"

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
			TEN.Misc.MakeEarthquake(20)
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

-- !Name "Add Dynamic Light to Moveable"
-- !Section "Special Effects"
-- !Conditional "False"
-- !Description "Add A Dynamic Light To A Moveable's mesh. (Version 1.0)"
-- !Arguments "NewLine, Moveables, 70, Select Moveable To Attach Light To" , "Numerical, 30, Select Mesh Number of Moveable \n This can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 70", "Numerical, 30, Select Range of Light \n(Range is in blocks from origin"
-- !Arguments "NewLine, Boolean, Add Sound Effect, 30" , "SoundEffects, 70, Choose sound to play"
-- !Arguments "NewLine, Boolean, Add a random effect to the light? "

LevelFuncs.Engine.Node.DynamicLightMesh = function(
	moveable,
	meshnumber,
	lightcolor,
	range,
	randomcheck,
	soundeffectcheck,
	soundeffect
	)

		local moveable = GetMoveableByName(moveable)
		local moveablemeshpos = moveable:GetJointPosition(meshnumber)
		
		if randomcheck == false
		then 
			TEN.Effects.EmitLight(moveablemeshpos, lightcolor, Range)
		else 
			TEN.Effects.EmitLight(moveablemeshpos, lightcolor, math.random((Range)/2, Range))
		end

		if SoundEffectCheck == true
		then
			TEN.Misc.PlaySound(soundeffect, moveablemeshpos)
		end
	
end

-- !Name "Add Dynamic Light to Static"
-- !Section "Special Effects"
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

