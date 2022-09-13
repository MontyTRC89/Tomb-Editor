-- !Name "If random event occurs..."
-- !Section "Game flow"
-- !Conditional "True"
-- !Description "Happens whether a random event occurs on the range from 1 to 100 percent."
-- !Description "Every time this condition is called, chance is recalculated."
-- !Arguments "Numerical, 15, [ 1 | 100 ], Percentage of event occurence"

LevelFuncs.Engine.Node.TestPercentageChance = function(percentageRange)
    return (math.random() * 100 <= percentageRange)
end

-- !Name "End level"
-- !Section "Game flow"
-- !Description "Ends current level and loads next level according to number. If number is 0, loads next level."
-- !Description "If number is more than level count, loads title."
-- !Arguments "Numerical, 15, [ 0 | 99 ], Next level number"

LevelFuncs.Engine.Node.EndLevel = function(number)
    Flow.EndLevel(number)
end

-- !Name "Add secret"
-- !Section "Game flow"
-- !Description "Adds one secret to game secret count and plays secret soundtrack."
-- !Arguments "Numerical, 15, [ 0 | 7 ], Level secret index"

LevelFuncs.Engine.Node.AddSecret = function(number)
    TEN.Flow.AddSecret(number)
end

-- !Name "Set secret count"
-- !Section "Game flow"
-- !Description "Overwrites current game secret count with provided one."
-- !Arguments "Numerical, 15, [0 | 99 ], New secret count"

LevelFuncs.Engine.Node.SetSecretCount = function(number)
    TEN.Flow.SetSecretCount(number)
end

-- !Name "If game secret count is..."
-- !Section "Game flow"
-- !Description "Checks current game secret count."
-- !Conditional "True"
-- !Arguments "CompareOperator, 25, Compare operation" "Numerical, 15, [0 | 99 ], Secret count"

LevelFuncs.Engine.Node.GetSecretCount = function(operator, number)
    return LevelFuncs.Engine.Node.CompareValue(TEN.Flow.GetSecretCount(), number, operator)
end