-- !Name "If minimum fog distance is..."
-- !Section "Environment"
-- !Description "Checks current minimum (near) fog distance value in sectors."
-- !Conditional "True"
-- !Arguments "CompareOperator, 25, Compare operation" "Numerical, 15, [ 0 | 256 ], Minimum fog distance to check"

LevelFuncs.Engine.Node.TestFogMinDistance = function(operator, number)
	return LevelFuncs.Engine.Node.CompareValue(TEN.Flow.GetCurrentLevel().fog.minDistance, number, operator)
end

-- !Name "If maximum fog distance is..."
-- !Section "Environment"
-- !Description "Checks current maximum (far) fog distance value in sectors."
-- !Conditional "True"
-- !Arguments "CompareOperator, 25, Compare operation" "Numerical, 15, [ 0 | 256 ], Maximum fog distance to check"

LevelFuncs.Engine.Node.TestFogMaxDistance = function(operator, number)
	return LevelFuncs.Engine.Node.CompareValue(TEN.Flow.GetCurrentLevel().fog.maxDistance, number, operator)
end

-- !Name "If fog color is..."
-- !Section "Environment"
-- !Description "Checks current fog color."
-- !Conditional "True"
-- !Arguments "Color, 20, Fog color"

LevelFuncs.Engine.Node.TestFogColor = function(color)
	local fog = TEN.Flow.GetCurrentLevel().fog
	return (color.r == fog.color.r and color.g == fog.color.g and color.b == fog.color.b)
end

-- !Name "Set fog minimum distance"
-- !Section "Environment"
-- !Description "Sets fog minimum (near) distance to specified value in sectors."
-- !Arguments "Numerical, 15, [ 0 | 256 ], Minimum fog distance"

LevelFuncs.Engine.Node.SetFogMinDistance = function(distance)
	TEN.Flow.GetCurrentLevel().fog.minDistance = distance
end

-- !Name "Set fog maximum distance"
-- !Section "Environment"
-- !Description "Sets fog maximum (far) distance to specified value in sectors."
-- !Arguments "Numerical, 15, [ 0 | 256 ], Maximum fog distance"

LevelFuncs.Engine.Node.SetFogMaxDistance = function(distance)
	TEN.Flow.GetCurrentLevel().fog.maxDistance = distance
end

-- !Name "Set fog color"
-- !Section "Environment"
-- !Description "Sets fog color to specified."
-- !Arguments "Color, 20, Fog color"

LevelFuncs.Engine.Node.SetFogColor = function(color)
	TEN.Flow.GetCurrentLevel().fog.color = color
end