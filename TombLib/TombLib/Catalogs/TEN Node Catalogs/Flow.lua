-- !Name "If random event occurs..."
-- !Section "Game flow"
-- !Conditional "True"
-- !Description "Happens whether a random event occurs on the range from 1 to 100 percent."
-- !Description "Every time this condition is called, chance is recalculated."
-- !Arguments "Numerical, 15, [ 1 | 100 | 0 ], Percentage of event occurence"

LevelFuncs.TestPercentageChance = function(percentageRange)
    return (math.random() * 100 <= percentageRange)
end

-- !Name "End level"
-- !Section "Game flow"
-- !Description "Ends current level and loads next level according to number. If number is 0, loads next level."
-- !Description "If number is more than level count, loads title."
-- !Arguments "Numerical, 15, [ 0 | 99 | 0 ], Level number"

LevelFuncs.EndLevel = function(number)
    Flow.EndLevel(number)
end

-- !Name "Add secret"
-- !Section "Game flow"
-- !Description "Adds one secret to game secret count and plays secret soundtrack."
-- !Arguments "Numerical, 15, [ 0 | 7 | 0 ], Level secret index"

LevelFuncs.AddSecret = function(number)
    TEN.Flow.AddSecret(number)
end

-- !Name "Set secret count"
-- !Section "Game flow"
-- !Description "Overwrites current game secret count with provided one."
-- !Arguments "Numerical, 15, [0 | 99 | 0], New secret count"

LevelFuncs.SetSecretCount = function(number)
    TEN.Flow.SetSecretCount(number)
end

-- !Name "If game secret count is..."
-- !Section "Game flow"
-- !Description "Checks current game secret count."
-- !Conditional "True"
-- !Arguments "CompareOperand, 25, Compare operation" "Numerical, 15, [0 | 99 | 0 ], Secret count"

LevelFuncs.GetSecretCount = function(operand, number)
    return LevelFuncs.CompareValue(TEN.Flow.GetSecretCount(), number, operand)
end