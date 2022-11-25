--!Name "Emit Particles From A MOVEABLE"
--!Section "Special Effects"
--!Conditional "False"
--!Description "If you want the effect from a single mesh then please use the `Emit Particles From A Mesh..` node.\nThis effect is drawn every frame."
--!Arguments "NewLine,Moveables,Choose the moveable where the particles will spawn from."
--!Arguments "NewLine,Vector3,100,[-32000|32000],Velocity X Y Z"
--!Arguments "NewLine,Numerical,33,[0|100],Choose sprite for emitter.\nSprites are based on the DEFAULT_SPRITES slot in Wadtool\n0 = Flame Emitter\n1 = Underwater blood\n2 = Waterfall\n3 = Mist\n4 = Splash Ring 1\n5 = Splash Ring 2\n6 = Splash Ring 3\n7 = Splash Ring 4\n8 = Water Splash\n9 = Water Ring\n11 = Specular\n13 = Underwater Bubble\n14 = Underwater Dust\n15 = Blood\n28 = Lightning\n29 = Lensflare Ring\n30 = Lensflare Ring 2 \n31 = Lensflare Sundisc\n32 = Lensflare Bright Spark"
--!Arguments "Numerical,33,[-32768| 32767],Gravity"
--!Arguments "Numerical,33,[-32000|32000],Rotation"
--!Arguments "NewLine,Color,50,Start Color","Color,50,End Color"
--!Arguments "NewLine,Enumeration,25,[OPAQUE|ALPHATEST|ADDITIVE|NOZTEST|SUBTRACTIVE|WIREFRAME|EXCLUDE|SCREEN|LIGHTEN|ALPHABLEND], Choose blend type for particles"
--!Arguments "Numerical,25,[-32000|32000],Start Size"
--!Arguments "Numerical,25,[-32000|32000],End Size"
--!Arguments "Numerical,25,[-32000|32000],Lifetime (in seconds)"
--!Arguments "NewLine,Boolean,50,Add Damage?","Boolean,50,Add Poison?"

LevelFuncs.Engine.Node.MoveableParticleEmitter=function(
    Moveable,
    particleVelocity,
    spriteID,
    ParticleGravity,
    ParticleRotation,
    StartColor,
    EndColor,
    BlendMode,
    ParticleStartSize,
    ParticleEndSize,
    ParticleLife,
    DamageBool,
    PoisonBool
)

local Entity = GetMoveableByName(Moveable)
local EntityPos = Entity:GetPosition()

local BlendID 
if BlendMode == 0 then
    BlendID = TEN.Effects.BlendID.OPAQUE end
if BlendMode == 1 then
    BlendID = TEN.Effects.BlendID.ALPHATEST end
if BlendMode == 2 then
    BlendID = TEN.Effects.BlendID.ADDITIVE end
if BlendMode == 3 then
    BlendID = TEN.Effects.BlendID.NOZTEST end
if BlendMode == 4 then
    BlendID = TEN.Effects.BlendID.SUBTRACTIVE end
if BlendMode == 5 then
    BlendID = TEN.Effects.BlendID.WIREFRAME end
if BlendMode == 6 then
    BlendID = TEN.Effects.BlendID.EXCLUDE end
if BlendMode == 7 then
    BlendID = TEN.Effects.BlendID.SCREEN end
if BlendMode == 8 then
    BlendID = TEN.Effects.BlendID.LIGHTEN end
if BlendMode == 9 then
    BlendID = TEN.Effects.BlendID.ALPHABLEND end

    TEN.Effects.EmitParticle(
    EntityPos,
    particleVelocity,
    spriteID,
    ParticleGravity,
    ParticleRotation,
    StartColor,
    EndColor,
    BlendID,
    ParticleStartSize,
    ParticleEndSize,
    ParticleLife,
    DamageBool,
    PoisonBool
    )

end

--!Name "Emit Particles From A Mesh"
--!Section "Special Effects"
--!Conditional "False"
--!Description "Add particles to a specified mesh.\nThis effect is drawn every frame."
--!Arguments "NewLine,Moveables,70,Choose the moveable where the particles will spawn from." "Numerical,30,[0|100],Mesh Number.\nThis can be found in Wadtool in the Animation Editor"
--!Arguments "NewLine,Vector3,100,[-32000|32000],Velocity X Y Z"
--!Arguments "NewLine,Numerical,33,[0|100],Choose sprite for emitter.\nSprites are based on the DEFAULT_SPRITES slot in Wadtool\n0 = Flame Emitter\n1 = Underwater blood\n2 = Waterfall\n3 = Mist\n4 = Splash Ring 1\n5 = Splash Ring 2\n6 = Splash Ring 3\n7 = Splash Ring 4\n8 = Water Splash\n9 = Water Ring\n11 = Specular\n13 = Underwater Bubble\n14 = Underwater Dust\n15 = Blood\n28 = Lightning\n29 = Lensflare Ring\n30 = Lensflare Ring 2 \n31 = Lensflare Sundisc\n32 = Lensflare Bright Spark"
--!Arguments "Numerical,33,[-32768| 32767],Gravity"
--!Arguments "Numerical,33,[-32000|32000],Rotation"
--!Arguments "NewLine,Color,50,Start Color","Color,50,End Color"
--!Arguments "NewLine,Enumeration,25,[OPAQUE|ALPHATEST|ADDITIVE|NOZTEST|SUBTRACTIVE|WIREFRAME|EXCLUDE|SCREEN|LIGHTEN|ALPHABLEND], Choose blend type for particles"
--!Arguments "Numerical,25,[-32000|32000],Start Size"
--!Arguments "Numerical,25,[-32000|32000],End Size"
--!Arguments "Numerical,25,[-32000|32000],Lifetime (in seconds)"
--!Arguments "NewLine,Boolean,50,Add Damage?","Boolean,50,Add Poison?"

LevelFuncs.Engine.Node.MeshParticleEmitter=function(
    Moveable,
    MeshNum,
    particleVelocity,
    spriteID,
    ParticleGravity,
    ParticleRotation,
    StartColor,
    EndColor,
    BlendMode,
    ParticleStartSize,
    ParticleEndSize,
    ParticleLife,
    DamageBool,
    PoisonBool
)
local Entity = GetMoveableByName(Moveable)
local JointPos = Moveable:GetJointPosition(MeshNum)

local BlendID 
if BlendMode == 0 then
    BlendID = TEN.Effects.BlendID.OPAQUE end
if BlendMode == 1 then
    BlendID = TEN.Effects.BlendID.ALPHATEST end
if BlendMode == 2 then
    BlendID = TEN.Effects.BlendID.ADDITIVE end
if BlendMode == 3 then
    BlendID = TEN.Effects.BlendID.NOZTEST end
if BlendMode == 4 then
    BlendID = TEN.Effects.BlendID.SUBTRACTIVE end
if BlendMode == 5 then
    BlendID = TEN.Effects.BlendID.WIREFRAME end
if BlendMode == 6 then
    BlendID = TEN.Effects.BlendID.EXCLUDE end
if BlendMode == 7 then
    BlendID = TEN.Effects.BlendID.SCREEN end
if BlendMode == 8 then
    BlendID = TEN.Effects.BlendID.LIGHTEN end
if BlendMode == 9 then
    BlendID = TEN.Effects.BlendID.ALPHABLEND end

    TEN.Effects.EmitParticle(
    JointPos,
    particleVelocity,
    spriteID,
    ParticleGravity,
    ParticleRotation,
    StartColor,
    EndColor,
    BlendID,
    ParticleStartSize,
    ParticleEndSize,
    ParticleLife,
    DamageBool,
    PoisonBool
    )
end

--!Name "Emit Particles From A STATIC"
--!Section "Special Effects"
--!Conditional "False"
--!Description "Emit Particles from a moveable. \n If you want the effect from a single mesh then please use the `Emit Particles from a mesh` node."
--!Arguments "NewLine,Statics,Choose the static where the particles will spawn from."
--!Arguments "NewLine,Vector3,100,[-32000|32000],Velocity X Y Z"
--!Arguments "NewLine,Numerical,33,[0|100],Choose sprite for emitter.\nSprites are based on the DEFAULT_SPRITES slot in Wadtool\n0 = Flame Emitter\n1 = Underwater blood\n2 = Waterfall\n3 = Mist\n4 = Splash Ring 1\n5 = Splash Ring 2\n6 = Splash Ring 3\n7 = Splash Ring 4\n8 = Water Splash\n9 = Water Ring\n11 = Specular\n13 = Underwater Bubble\n14 = Underwater Dust\n15 = Blood\n28 = Lightning\n29 = Lensflare Ring\n30 = Lensflare Ring 2 \n31 = Lensflare Sundisc\n32 = Lensflare Bright Spark"
--!Arguments "Numerical,33,[-32768| 32767],Gravity"
--!Arguments "Numerical,33,[-32000|32000],Rotation"
--!Arguments "NewLine,Color,50,Start Color","Color,50,End Color"
--!Arguments "NewLine,Enumeration,25,[OPAQUE|ALPHATEST|ADDITIVE|NOZTEST|SUBTRACTIVE|WIREFRAME|EXCLUDE|SCREEN|LIGHTEN|ALPHABLEND], Choose blend type for particles"
--!Arguments "Numerical,25,[-32000|32000],Start Size"
--!Arguments "Numerical,25,[-32000|32000],End Size"
--!Arguments "Numerical,25,[-32000|32000],Lifetime (in seconds)"
--!Arguments "NewLine,Boolean,50,Add Damage?","Boolean,50,Add Poison?"

LevelFuncs.Engine.Node.StaticParticleEmitter=function(
    static,
    particleVelocity,
    spriteID,
    ParticleGravity,
    ParticleRotation,
    StartColor,
    EndColor,
    BlendMode,
    ParticleStartSize,
    ParticleEndSize,
    ParticleLife,
    DamageBool,
    PoisonBool
)

local static = GetStaticByName(static)
local staticPos = static:GetPosition()
local BlendID 

if BlendMode == 0 then
    BlendID = TEN.Effects.BlendID.OPAQUE end
if BlendMode == 1 then
    BlendID = TEN.Effects.BlendID.ALPHATEST end
 if BlendMode == 2 then
    BlendID = TEN.Effects.BlendID.ADDITIVE end
if BlendMode == 3 then
    BlendID = TEN.Effects.BlendID.NOZTEST end
if BlendMode == 4 then
    BlendID = TEN.Effects.BlendID.SUBTRACTIVE end
if BlendMode == 5 then
    BlendID = TEN.Effects.BlendID.WIREFRAME end
if BlendMode == 6 then
    BlendID = TEN.Effects.BlendID.EXCLUDE end
if BlendMode == 7 then
    BlendID = TEN.Effects.BlendID.SCREEN end
if BlendMode == 8 then
    BlendID = TEN.Effects.BlendID.LIGHTEN end
if BlendMode == 9 then
    BlendID = TEN.Effects.BlendID.ALPHABLEND end

    TEN.Effects.EmitParticle(
    staticPos,
    particleVelocity,
    spriteID,
    ParticleGravity,
    ParticleRotation,
    StartColor,
    EndColor,
    BlendID,
    ParticleStartSize,
    ParticleEndSize,
    ParticleLife,
    DamageBool,
    PoisonBool
    )

end

--!Name "Emit Lightning Arc"
--!Section "Special Effects"
--!Conditional "False"
--!Description "Emit A Lightning Arc Between Two Points in 3D Space"
--!Arguments "Newline,Moveables,50,Source Position","Moveables,50,Destination Position"
--!Arguments "NewLine,Color,100,Color of Lightning Effect"
--!Arguments "Newline,Number,25,[0|4.2|1],Lifetime in seconds." ,"Number,25,[1|255|0],Amplitude (strength) of effect","Number,25,[1|127|0],Beam Width","Number,25,[1|127|0],Detail of effect\n 1 is 1 segment between the two points for example."
--!Arguments "Newline,Boolean,50,Toggle Smooth Effect","Boolean,50,Toggle End Drift"
--!Arguments "NewLine,Boolean,30, Add Source Light" , "Boolean,40,Add Destination Light" , "Boolean,30,Toggle SFX"
LevelFuncs.Engine.Node.LightningArc=function(
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
        soundBool
)

local startingPoint = GetMoveableByName(source):GetPosition()
local endingpoint = GetMoveableByName(dest):GetPosition()
TEN.Effects.EmitLightningArc(
    startingPoint,
    endingpoint,
    color,
    lifetime,
    amplitude,
    beamWidth,
    detail,
    smooth,
    endDrift

)
local sfxcheck
    if soundBool==true then TEN.Misc.PlaySound(197,startingPoint)
    end

local sourceLightCheck 
    if sourcelightBool == true 
        then
            TEN.Effects.EmitLight(startingPoint,color,math.random(10,20))
        end

local destLightCheck 
    if destlightBool == true 
        then
            TEN.Effects.EmitLight(endingpoint,color,math.random(10,20))
        end
end

--!Name "Emit Shockwave"
--!Section "Special Effects"
--!Description "Emit a Shockwave effect from a moveable's origin point "
--!Arguments "NewLine, Moveables, Choose origin point of shockwave"
--!Arguments "NewLine,Number,50,[0|10400|0],Inner Radius" "Number,50,[0|10400|0],Outer Radius."
--!Arguments "NewLine,Color,100,Color of shockwave effect"
--!Arguments "NewLine,Numerical,33,[0|8.5|1],Lifetime of effect (in seconds)" , "Number,33,[0|500|0],Speed" , "Number,33,[-360|360|0],Angle about the X axis"
--!Arguments "NewLine,Boolean,30,Toogle Lara Damage" , "Boolean,33,Toggle Rumble", "Boolean,33,Toggle SFX"

LevelFuncs.Engine.Node.Shockwave = function(
pos,
innerRadius,
outerRadius,
color,
lifetime,
speed,
angle,
hurtsLaraBool,
RumbleBool,
soundBool)

local origin= GetMoveableByName(pos):GetPosition()
TEN.Effects.EmitShockwave(
origin,
innerRadius,
outerRadius,
color,
lifetime,
speed,
angle,
hurtsLaraBool)

local Rumblecheck
    if RumbleBool==true
		then 
			TEN.Misc.MakeEarthquake(20)
			TEN.Misc.Vibrate(20,0.3)
    end

local sfxcheck
    if soundBool==true 
		then
			TEN.Misc.PlaySound(197,startingPoint)
    end

end

-- !Name "Add Dynamic Light to Moveable"
-- !Section "Special Effects"
-- !Conditional "False"
-- !Description "Add A Dynamic Light To A Moveable's mesh. (Version 1.0)"
-- !Arguments "NewLine,Moveables,70,Select Moveable To Attach Light To" ,"Numerical,30, Select Mesh Number of Moveable \n This can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine,Color,70","Numerical,30,Select Range of Light \n(Range is in blocks from origin"
-- !Arguments "NewLine,Boolean,Add Sound Effect,30" , "SoundEffects,70, Choose sound to play"
-- !Arguments "NewLine,Boolean,Add a Random Effect to the light? "

LevelFuncs.Engine.Node.DynamicLightMesh = function(Moveable,MeshNumber,LightColor,Range,RandomCheck,SoundEffectCheck,SoundEffect)

        local Moveable = GetMoveableByName(Moveable)
    local MoveableMeshPos =Moveable:GetJointPosition(MeshNumber)
        if RandomCheck == false then 
    TEN.Effects.EmitLight(MoveableMeshPos,LightColor,Range)
    else
        TEN.Effects.EmitLight(MoveableMeshPos, LightColor, math.random((Range)/2,Range))
    end
    if SoundEffectCheck == true then
    TEN.Misc.PlaySound(SoundEffect,MoveableMeshPos)
    end
end

-- !Name "Add Dynamic Light to Static"
-- !Section "Special Effects"
-- !Conditional "False"
-- !Description "Add A Dynamic Light To A Static Object. (Version 1.0)"
-- !Arguments "NewLine,Statics,Select Static To Attach Light To" ,
-- !Arguments "NewLine,Color,70","Numerical,30,[0],Select Range of Light \n(Range is in blocks from origin"
-- !Arguments "NewLine,Boolean,Add Sound Effect,30" , "SoundEffects,70,Choose sound to play"
-- !Arguments "NewLine,Boolean,Add a Random Effect to the light? "
LevelFuncs.Engine.Node.DynamicLightStaticMesh = function(Static, LightColor, Range, RandomCheck,SoundEffectCheck, SoundEffect)

    local Static = GetStaticByName(Static)
    local StaticPos = Static:GetPosition()
    if RandomCheck == false then
        TEN.Effects.EmitLight(StaticPos, LightColor, Range)
    else
        TEN.Effects.EmitLight(StaticPos, LightColor, math.random(Range))
    end
    if SoundEffectCheck == true then
        TEN.Misc.PlaySound(SoundEffect, StaticPos)
    end
end