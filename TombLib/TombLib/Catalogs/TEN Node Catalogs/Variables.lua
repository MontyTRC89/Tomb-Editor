-- !Name "If level variable is..."
-- !Section "Variables"
-- !Description "Checks if specified level variable complies to specified compare function."
-- !Conditional "True"
-- !Arguments "NewLine, String, 50" "CompareOperator, 25" "Numerical, 25"

LevelFuncs.Engine.Node.TestLevelVariable = function(varName, operator, value)
    if (LevelVars[varName] == nil) then
        return false
    else
        return LevelFuncs.Engine.Node.CompareValue(LevelVars[varName], value, operator)
    end
end

-- !Name "Modify or create level variable"
-- !Section "Variables"
-- !Description "Modify variable, according to specified operator and operand."
-- !Description "If variable with specified name does not exist,\nit is initialized as 0 before performing modify operation."
-- !Arguments "NewLine, String, 50" "Enumeration, 25, [ + | - | * | / ]" "Numerical, 25, [ -65536 | 65535 | 2 ]"

LevelFuncs.Engine.Node.ModifyLevelVariable = function(varName, operator, operand)
    if (LevelVars[varName] == nil) then
        print("Variable " .. varName .. " did not exist and was initialized as 0.")
        LevelVars[varName] = 0
    end

    LevelVars[varName] = LevelFuncs.Engine.Node.ModifyValue(operand, LevelVars[varName], operator)
end

-- !Name "Delete level variable"
-- !Section "Variables"
-- !Description "Delete level variable, if it exists."
-- !Arguments "NewLine, String, 100"

LevelFuncs.Engine.Node.DeleteLevelVariable = function(varName, operator, operand)
    if (LevelVars[varName] ~= nil) then
        LevelVars[varName] = nil
    else
        print("Variable " .. varName .. " did not exist and was not deleted.")
    end
end