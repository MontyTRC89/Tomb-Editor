-- !Name "Add point light to moveable"
-- !Section "Dynamic lights"
-- !Description "Add a point light to a moveable."
-- !Description "Updated for TombEngine Version 1.6 and above."
-- !Arguments "NewLine, Moveables, 80, Select moveable to attach light to."
-- !Arguments "Numerical, 20, [ 0 | 100 | 0 ] , Select mesh number of moveable. \nThis can be found in the Animation Editor within Wadtool."
-- !Arguments "NewLine, Color, 20, { TEN.Color(128,128,128) }, Light color." 
-- !Arguments "Numerical, 20, [ 0 | 100 | 0 ], { 20 }, (Optional) Light radius (in clicks of 256 world units)."
-- !Arguments "Vector3 , 60, [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset (relative)"
-- !Arguments "NewLine, Boolean, 50, Casts Dynamic Shadow"
-- !Arguments "String, 50, [ NoMultiline ], (Optional) A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required." 

LevelFuncs.Engine.Node.MoveableLight = function(moveable, meshnumber, lightcolor, range, effectOffset, shadow, name)

	local entityPos = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
	effectOffset = effectOffset or Vec3(0, 0, 0)
	local offset = (entityPos) + (effectOffset)
	TEN.Effects.EmitLight(offset,lightcolor, range, shadow, name)
end

-- !Name "Add point light to static"
-- !Section "Dynamic lights"
-- !Description "Add a point light to a static object."
-- !Description "Updated for TombEngine Version 1.6 and above."
-- !Arguments "NewLine, Statics, 80, Select static to attach light to."
-- !Arguments "Color, 20, { TEN.Color(128,128,128) }, Light color." 
-- !Arguments "NewLine, Numerical, 40, [ 0 | 100 | 0 ], { 20 }, (Optional) Light radius (in clicks of 256 world units)."
-- !Arguments "Vector3 , 60, [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset (relative)"
-- !Arguments "NewLine, Boolean, 50, Casts Dynamic Shadow"
-- !Arguments "String, 50, [ NoMultiline ], (Optional) A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required." 

LevelFuncs.Engine.Node.StaticLight = function(static, lightcolor, range, effectOffset, shadow, name)

	local entityPos = TEN.Objects.GetStaticByName(static):GetPosition()
	
	local offset = (entityPos) + (effectOffset)
	TEN.Effects.EmitLight(offset, lightcolor, range, shadow, name)
end

-- !Name "Add spotlight to a moveable"
-- !Section "Dynamic lights"
-- !Description "Adds a spotlight to a moveable."
-- !Arguments "NewLine, Moveables, 50, Select moveable to attach light to."
-- !Arguments "Numerical, 25, [ 0 | 100 | 0 ] , Select mesh number of moveable. \nThis can be found in the Animation Editor within Wadtool."
-- !Arguments "String, 25, [ NoMultiline ], (Optional) A unique name for the light.\nTo enable interpolation in high framerate mode the light must have a unique name.\nIf the source moveable does not move significantly this field is not required." 
-- !Arguments "NewLine, Color, 20, { TEN.Color(128,128,128) }, Light color." 
-- !Arguments "Numerical, 20, [ 0 | 100 | 0 ], { 5 }, (Optional) Light falloff (in clicks of 256 world units)."
-- !Arguments "Numerical, 20, [ 0 | 100 | 0 ], { 20 }, (Optional) Light distance (in clicks of 256 world units)."
-- !Arguments "Numerical, 20, [ 0 | 100 | 0 ], { 10 }, (Optional) Light radius (in clicks of 256 world units)."
-- !Arguments "Boolean, 50, Shadow"
-- !Arguments "NewLine, Vector3 , [ -1000000 | 1000000 |  | 1 | 32 ], { TEN.Vec3(.1,.1,.1) }, Offset"

LevelFuncs.Engine.Node.MoveableSpotLight = function(moveable, meshnumber, name, color, radius, falloff, distance, shadow, direction, effectOffset)

	local entityPos = TEN.Objects.GetMoveableByName(moveable):GetJointPosition(meshnumber)
	local effectOffset = effectOffset or Vec3(0, 0, 0)
	local offset = entityPos + effectOffset
	local direction = TEN.Objects.GetMoveableByName(moveable):GetJointRotation(meshnumber):Direction()

	EmitSpotLight(offset, direction, color, radius, falloff, distance, shadow, name)

end