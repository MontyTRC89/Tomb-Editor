local Timer = require("Engine.Timer")

-- !Name "Create timer with function"
-- !Conditional "False"
-- !Description "After a specified number of seconds, the specified thing happens"
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, Loop"
-- !Arguments "NewLine, Boolean , 33, Show Minutes" "Boolean , 33, Show Seconds" "Boolean , 33, Show Deciseconds"
-- !Arguments "NewLine, LuaScript, The function to call when the time is up"
LevelFuncs.Engine.Node.CreateTimerWithFunction = function(name, time, loop, minutes, seconds, deciseconds, luaFunction)
    if name ~= '' then
        LevelVars[name] = Timer.Create(name, time, loop,
            { minutes = minutes, seconds = seconds, deciseconds = deciseconds }, luaFunction)
        PrintLog('Timer with Function "' .. name .. '" successfully created', LogLevel.INFO)
    else
        PrintLog('Error in the "Create Timer with Function" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Create timer"
-- !Conditional "False"
-- !Description "Basic timer"
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, Loop"
-- !Arguments "NewLine, Boolean , 33, Show Minutes" "Boolean , 33, Show Seconds" "Boolean , 33, Show Deciseconds"
LevelFuncs.Engine.Node.CreateTimer = function(name, time, loop, minutes, seconds, deciseconds)
    if name ~= '' then
        LevelVars[name] = Timer.Create(name, time, loop,
            { minutes = minutes, seconds = seconds, deciseconds = deciseconds }, nil)
        PrintLog('Timer "' .. name .. '" successfully created', LogLevel.INFO)
    else
        PrintLog('Error in the "Create Timer" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Start timer"
-- !Conditional "False"
-- !Description "Begin or unpause a timer"
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.StartTimer = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):Start()
            PrintLog('Timer "' .. name .. '" started', LogLevel.INFO)
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Start Timer" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Stop timer"
-- !Conditional "False"
-- !Description "Stop the timer"
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.StopTimer = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):Stop()
            PrintLog('Timer "' .. name .. '" is stopped', LogLevel.INFO)
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Stop Timer" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Is the timer active?"
-- !Conditional "True"
-- !Description "Get whether or not the timer is active"
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IsTimerActive = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            local mss = Timer.Get(name):IsActive() and
                'Timer "' .. name .. '"is active' or
                'Timer "' .. name .. '"is not active'
            PrintLog(mss, LogLevel.INFO)
            return Timer.Get(name):IsActive()
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Timer is Active" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Set paused timer"
-- !Conditional "False"
-- !Description "Pause or unpause the timer"
-- !Section "Timer"
-- !Arguments "NewLine, String, 67, [ NoMultiline ], Timer name"
-- !Arguments "Boolean , 33, Pause"
LevelFuncs.Engine.Node.SetPausedTimer = function(name, isOnPause)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):SetPaused(isOnPause)
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Set Paused Timer" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "if the timer is paused..."
-- !Conditional "True"
-- !Description "Get whether or not the timer is paused"
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IsTimerPaused = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            return Timer.Get(name):IsPaused()
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Timer is Paused" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Get remaining time (in seconds) in console"
-- !Conditional "False"
-- !Description "Gets the remaining time (in seconds) of a timer"
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.GetRemainingTime = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            print(string.format("%.1f", Timer.Get(name):GetRemainingTime()))
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Get Remaining Time" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Set remaining time"
-- !Conditional "False"
-- !Description "Set the remaining time (in seconds) of a timer"
-- !Section "Timer"
-- !Arguments "NewLine, String,67, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 33, [ 0 | 1000 | 2 ], the new time remaining for the timer"
LevelFuncs.Engine.Node.SetRemainingTime = function(name, remainingTime)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):SetRemainingTime(remainingTime)
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Set Remaining Time" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Get total time (in seconds) in console"
-- !Conditional "False"
-- !Description "This is the amount of time the timer will start with, as well as when starting a new loop"
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.GetTotalTime = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            print(Timer.Get(name):GetTotalTime())
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Get Total Time" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Set total time"
-- !Conditional "False"
-- !Description "Set the total time (in seconds) for a timer"
-- !Section "Timer"
-- !Arguments "NewLine, String,67, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 33, [ 0 | 1000 | 2 ], timer's new total time"
LevelFuncs.Engine.Node.SetTotalTime = function(name, totalTime)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):SetTotalTime(totalTime)
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Set Total Time" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "Set timer loop"
-- !Conditional "False"
-- !Description "Set whether or not the timer loops"
-- !Section "Timer"
-- !Arguments "NewLine, String, 67, [ NoMultiline ], Timer name"
-- !Arguments "Boolean , 33, Is it on a loop?"
LevelFuncs.Engine.Node.SetLooping = function(name, looping)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):SetLooping(looping)
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "Set Timer Loop" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "If timer has expired..."
-- !Conditional "True"
-- !Description "Check if the timer is expired"
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IfTimerExpired = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            local temp = tonumber(string.format("%.1f", Timer.Get(name):GetRemainingTime()))
            return (temp == 0.0) and true or false
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "If Timer has expired" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "If remaining Time is..."
-- !Conditional "True"
-- !Description "Check if the remaining time is equal to, greater to, less to..."
-- !Section "Timer"
-- !Arguments "NewLine, String,50, [ NoMultiline ], Timer name" "CompareOperator, 30"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 1 ], remaining time (in seconds)"
LevelFuncs.Engine.Node.IfRemainingTimeIs = function(name, operator, time)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            local temp = tonumber(string.format("%.1f", Timer.Get(name):GetRemainingTime()))
            return LevelFuncs.Engine.Node.CompareValue(temp, time, operator)
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "If remaining Time is" node. The name of Timer is empty', LogLevel.ERROR)
    end
end

-- !Name "If total time is..."
-- !Conditional "True"
-- !Description "Check if the Total Time is equal to ..."
-- !Section "Timer"
-- !Arguments "NewLine, String,50, [ NoMultiline ], Timer name" "CompareOperator, 30"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 1 ], Total Time (in seconds)"
LevelFuncs.Engine.Node.IfTotalTimeIs = function(name, operator, time)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            return LevelFuncs.Engine.Node.CompareValue(Timer.Get(name):GetTotalTime(), time, operator)
        else
            PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        PrintLog('Error in the "If Total Time is" node. The name of Timer is empty', LogLevel.ERROR)
    end
end
