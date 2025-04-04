-- !Name "Particle generator (moveables)"
-- !Section "Particles"
-- !Description "Emit particles from a moveable"
-- !Arguments "NewLine, Moveables, 70, The moveable particles will spawn from." "Numerical, 15, [ 0 | 100], Mesh number"
-- !Arguments "Numerical, 15, [ 0 | 100 | 0 ], Sprite number. Refers to a DEFAULT_SPRITES sequence in a wad."
-- !Arguments "NewLine, Vector3, 60, [ -32000 | 32000 ], Velocity X Y Z"
-- !Arguments "Numerical, 20, [ -32768 | 32767 | 0 ], Gravity" "Numerical, 20, [ -32000 | 32000 | 1 ], Rotation"
-- !Arguments "NewLine, Color, 33, Start color", "Color, 34, End color"
-- !Arguments "Enumeration, 33, [ Opaque | Alpha test | Add | Subtract | Exclude | Screen | Lighten | Alpha blend ], Blending method for particles. \nSee Lua API Documentation for further information."
-- !Arguments "NewLine, Numerical, 20, [ -32000 | 32000 | 0 ], Start size" "Numerical, 20, [ -32000 | 32000 | 0 ], End size" "Numerical, 20, [ 0 | 32000 | 1 | .1 ], Lifetime (in seconds)"
-- !Arguments "Boolean, 20, Poison" "Boolean, 20, Damage"

LevelFuncs.Engine.Node.ParticleEmitter = function(entity, meshnum, spriteID, velocity, gravity, rotation, startColor, endColor, blendID, startSize, endSize, life, poison, damage)
	local origin = TEN.Objects.GetMoveableByName(entity):GetJointPosition(meshnum)
	local blendmode = LevelFuncs.Engine.Node.GetBlendMode(blendID)

	TEN.Effects.EmitParticle(origin, velocity, spriteID, gravity, rotation, startColor, endColor, blendmode, startSize, endSize, life, damage, poison)
end

-- !Name "Particle generator (statics)"
-- !Section "Particles"
-- !Description "Emit particles from a moveable"
-- !Arguments "NewLine, Statics, 85, The moveable particles will spawn from."
-- !Arguments "Numerical, 15, [ 0 | 100 | 0 ], Sprite number. Refers to a DEFAULT_SPRITES sequence in a wad."
-- !Arguments "NewLine, Vector3, 60, [ -32000 | 32000 ], Velocity X Y Z"
-- !Arguments "Numerical, 20, [ -32768 | 32767 | 0 ], Gravity" "Numerical, 20, [ -32000 | 32000 | 1 ], Rotation"
-- !Arguments "NewLine, Color, 6, Start color", "Color, 6, End color"
-- !Arguments "Enumeration, 28, [ Opaque | Alpha test | Add | Subtract | Exclude | Screen | Lighten | Alpha blend ], Blending method for particles. \nSee Lua API Documentation for further information."
-- !Arguments "Numerical, 18.2, [ -32000 | 32000 | 0 ], Start size" "Numerical, 18.2, [ -32000 | 32000 | 0 ], End size" "Numerical, 24, [ 0 | 32000 | 1 | 0.1 ], Lifetime (in seconds)"
-- !Arguments "NewLine, Boolean, 15, Poison" "Boolean, 17, Damage"
-- !Arguments "Boolean, 65, Show particle only if static mesh is visible"

LevelFuncs.Engine.Node.ParticleEmitterStatics = function(entity, spriteID, velocity, gravity, rotation, startColor, endColor, blendID, startSize, endSize, life, poison, damage,visibility)
	local origin = TEN.Objects.GetStaticByName(entity):GetPosition()
	local blendmode = LevelFuncs.Engine.Node.GetBlendMode(blendID)

	local shouldEmit = not visibility or LevelFuncs.Engine.Node.TestStaticActivity(entity)
	if shouldEmit then
		TEN.Effects.EmitParticle(origin, velocity, spriteID, gravity, rotation, startColor, endColor, blendmode,
			startSize, endSize, life, damage, poison)
	end
end

-- !Name "Particle generator (volume)"
-- !Section "Particles"
-- !Description "Emit particles from the centre of a volume"
-- !Arguments "NewLine, Volumes, 70, The volume particles will spawn from." "Numerical, 30, [ 0 | 100 | 0 ], Sprite number. Refers to a DEFAULT_SPRITES sequence in a wad."
-- !Arguments "NewLine, Vector3, 60, [ -32000 | 32000 ], Velocity X Y Z"
-- !Arguments "Numerical, 20, [ -32768 | 32767 | 0 ], Gravity" "Numerical, 20, [ -32000 | 32000 | 1 ], Rotation"
-- !Arguments "NewLine, Color, 33, Start color", "Color, 34, End color"
-- !Arguments "Enumeration, 33, [ Opaque | Alpha test | Add | Subtract | Exclude | Screen | Lighten | Alpha blend ], Blending method for particles. \nSee Lua API Documentation for further information."
-- !Arguments "NewLine, Numerical, 20, [ -32000 | 32000 | 0 ], Start size" "Numerical, 20, [ -32000 | 32000 | 0 ], End size" "Numerical, 20, [ 0 | 32000 | 1 | .1 ], Lifetime (in seconds)"
-- !Arguments "Boolean, 20, Poison" "Boolean, 20, Damage"

LevelFuncs.Engine.Node.ParticleEmitterVolume = function(volume, spriteID, velocity, gravity, rotation, startColor, endColor, blendID, startSize, endSize, life, poison, damage)
	local origin = TEN.Objects.GetVolumeByName(volume):GetPosition()
	local blendmode = LevelFuncs.Engine.Node.GetBlendMode(blendID)

	TEN.Effects.EmitParticle(origin, velocity, spriteID, gravity, rotation, startColor, endColor, blendmode, startSize, endSize, life, damage, poison)
end

-- !Name "Emit lightning arc"
-- !Section "Particles"
-- !Description "Emit a lightning arc between two points in 3D space"
-- !Arguments "NewLine, Moveables, 100, Source position" 
-- !Arguments "NewLine, Moveables, 100, Destination position"
-- !Arguments "NewLine, Color, 20, Color of lightning Effect"
-- !Arguments "Numerical, 20, [ 0 | 4.2 | 1 ], Lifetime in seconds" "Numerical, 20, [ 1 | 255 | 0 ], Effect strength"
-- !Arguments "Numerical, 20, [ 1 | 127 | 0 ], Beam width" "Numerical, 20, [ 1 | 127 | 0 ], Detail level"
-- !Arguments "NewLine, Boolean, 22, Smooth" "Boolean, 22, End drift" "Boolean, 26, Source light" "Boolean, 30, Destination light"

LevelFuncs.Engine.Node.LightningArc = function(source, dest, color, lifetime, amplitude, beamWidth, detail, smooth, endDrift, sourcelight, destlight)
	local randomiserX = (math.random(-64, 64))
	local randomiserZ = (math.random(-256, 256))

	local entity = TEN.Objects.GetMoveableByName(source)

	local startingpoint = entity:GetPosition()

	startingpoint.x = (startingpoint.x - randomiserX)
	startingpoint.y = (startingpoint.y - 64)
	startingpoint.z = (startingpoint.z - randomiserZ)

	local endingpoint = TEN.Objects.GetMoveableByName(dest):GetPosition()

	endingpoint.x = (endingpoint.x - randomiserX)
	endingpoint.y = (endingpoint.y - 64)
	endingpoint.z = (endingpoint.z - randomiserZ)

	local beamRandom = math.random((beamWidth - 10), (beamWidth + 10))
	local ampRandom = math.random((amplitude - 10), (amplitude + 10))

	if (sourcelight == true) then
		TEN.Effects.EmitLight(startingpoint, color, math.random(1, 10))
	end

	if (destlight == true) then
		TEN.Effects.EmitLight(endingpoint, color, math.random(1, 10))
	end

	TEN.Effects.EmitLightningArc(startingpoint, endingpoint, color, lifetime, ampRandom, beamRandom, detail, smooth, endDrift)
end

-- !Name "Emit shockwave"
-- !Section "Particles"
-- !Description "Emit a shockwave effect."
-- !Arguments "NewLine, Moveables, 70, Shockwave position." "Numerical, 30, [ 0 | 100 ], Mesh number (optional)"
-- !Arguments "NewLine, Numerical, 33, [ 1 | 10400 | 0 ], Inner radius" "Numerical, 33, [ 1 | 10400 | 0 ], Outer radius"
-- !Arguments "Color, 34, Color of shockwave"
-- !Arguments "NewLine, Numerical, 33, [ 0 | 8.5 | 1 ], Lifetime of effect (in seconds)" "Numerical, 33, [ 0 | 500 | 0 ], Speed" "Numerical, 33, [ -360 | 360 | 0 ], X axis rotation"
-- !Arguments "NewLine, Boolean, 50, Damage" "Boolean, 50, Randomize spawn point"

LevelFuncs.Engine.Node.Shockwave = function(pos, meshnum, innerRadius, outerRadius, color, lifetime, speed, angle, damage,
											randomSpawn)
	local randomiser = math.random(1, 14)
	local radiusInVar = innerRadius + math.random(1, 3)
	local radiusOutVar = outerRadius + math.random(1, 3)
	local randomMesh = math.random(0, 14)

	if (randomSpawn == true) then
		local origin = TEN.Objects.GetMoveableByName(pos):GetJointPosition(randomMesh)
		TEN.Effects.EmitShockwave(origin, radiusInVar, radiusOutVar, color, lifetime, speed, angle, damage)
	else
		local origin = TEN.Objects.GetMoveableByName(pos):GetJointPosition(meshnum)
		TEN.Effects.EmitShockwave(origin, radiusInVar, radiusOutVar, color, lifetime, speed, angle, damage)
	end
end
