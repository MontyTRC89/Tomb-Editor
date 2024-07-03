-- !Name "If level variable is..."
-- !Section "Variables"
-- !Description "Checks if specified level variable complies to specified compare function."
-- !Conditional "True"
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Variable name"
-- !Arguments "CompareOperator, 25" "Numerical, 25, [ -65536 | 65535 | 2 ], Variable value"

LevelFuncs.Engine.Node.TestLevelVariable = function(varName, operator, value)
    if (LevelVars[varName] == nil) then
        return LevelFuncs.Engine.Node.CompareValue(0, value, operator)
    else
        return LevelFuncs.Engine.Node.CompareValue(LevelVars[varName], value, operator)
    end
end

-- !Name "Modify or create level variable"
-- !Section "Variables"
-- !Description "Modify level variable, according to specified operator and operand."
-- !Description "If level variable with specified name does not exist,\nit is initialized as 0 before performing modify operation."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Variable name"
-- !Arguments "Enumeration, 25, [ + | - | * | / | = ], Mathematical operation to perform"
-- !Arguments "Numerical, [ -65536 | 65535 | 2 ], Variable value"

LevelFuncs.Engine.Node.ModifyLevelVariable = function(varName, operator, operand)
    if (LevelVars[varName] == nil) then
        if (operator == 4) then
            LevelVars[varName] = operand
        else
            LevelVars[varName] = 0
        end
        print("Level variable " .. varName .. " did not exist and was initialized as " .. LevelVars[varName] .. ".")
    end

    LevelVars[varName] = LevelFuncs.Engine.Node.ModifyValue(operand, LevelVars[varName], operator)
end

-- !Name "Delete level variable"
-- !Section "Variables"
-- !Description "Delete level variable, if it exists."
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Variable name"

LevelFuncs.Engine.Node.DeleteLevelVariable = function(varName, operator, operand)
    if (LevelVars[varName] ~= nil) then
        LevelVars[varName] = nil
    else
        print("Level variable " .. varName .. " did not exist and was not deleted.")
    end
end

-- !Name "If game variable is..."
-- !Section "Variables"
-- !Description "Checks if specified game variable complies to specified compare function."
-- !Conditional "True"
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Variable name"
-- !Arguments "CompareOperator, 25" "Numerical, 25, [ -65536 | 65535 | 2 ], Variable value"

LevelFuncs.Engine.Node.TestGameVariable = function(varName, operator, value)
    if (GameVars[varName] == nil) then
        return LevelFuncs.Engine.Node.CompareValue(0, value, operator)
    else
        return LevelFuncs.Engine.Node.CompareValue(GameVars[varName], value, operator)
    end
end

-- !Name "Modify or create game variable"
-- !Section "Variables"
-- !Description "Modify game variable, according to specified operator and operand."
-- !Description "If game variable with specified name does not exist,\nit is initialized as 0 before performing modify operation."
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Variable name"
-- !Arguments "Enumeration, 25, [ + | - | * | / | = ], Mathematical operation to perform"
-- !Arguments "Numerical, [ -65536 | 65535 | 2 ], Variable value"

LevelFuncs.Engine.Node.ModifyGameVariable = function(varName, operator, operand)
    if (GameVars[varName] == nil) then
        if (operator == 4) then
            GameVars[varName] = operand
        else
            GameVars[varName] = 0
        end
        print("Game variable " .. varName .. " did not exist and was initialized as " .. GameVars[varName] .. ".")
    end

    GameVars[varName] = LevelFuncs.Engine.Node.ModifyValue(operand, GameVars[varName], operator)
end

-- !Name "Delete game variable"
-- !Section "Variables"
-- !Description "Delete game variable, if it exists."
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Variable name"

LevelFuncs.Engine.Node.DeleteGameVariable = function(varName, operator, operand)
    if (GameVars[varName] ~= nil) then
        GameVars[varName] = nil
    else
        print("Game variable " .. varName .. " did not exist and was not deleted.")
    end
end
