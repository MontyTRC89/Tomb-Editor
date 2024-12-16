local Timer = require("Engine.Timer")
LevelVars.nodeTimers = {}

-- !Name "Create Basic timer"
-- !Conditional "False"
-- !Description "Creates a simple countdown.\n\nNot to be used inside the `On Volume Inside` or `On Loop` events."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 | 1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, Loop"
-- !Arguments "NewLine, Boolean , 33, Show Minutes" "Boolean , 33, Show Seconds" "Boolean , 33, Show Deciseconds"
-- !Arguments "NewLine, Boolean , 100, Show debug messages in console"
LevelFuncs.Engine.Node.CreateTimer = function(name, time, loop, minutes, seconds, deciseconds, debug)
    if name ~= '' then
        local nodeTimerFormat = { minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        LevelVars.nodeTimers[name] = {}
        LevelVars.nodeTimers[name].timer = Timer.Create(name, time, loop, nodeTimerFormat, nil)
        LevelVars.nodeTimers[name].remainingTime = Timer.Get(name):GetRemainingTime()
        LevelVars.nodeTimers[name].remainingTimeFormatted = tostring(math.floor(Timer.Get(name):GetRemainingTime() * 10) / 10)
        LevelVars.nodeTimers[name].test = false
        LevelVars.nodeTimers[name].debug = debug
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer '" .. name .. "' created successfully!", LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create Timer' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Create timer with function"
-- !Conditional "False"
-- !Description "Creates a countdown which will execute a `LevelFuncs` lua function upon ending.\n\nNot to be used inside the `On Volume Inside` or `On Loop` events."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 | 1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, Loop"
-- !Arguments "NewLine, Boolean , 33, Show Minutes" "Boolean , 33, Show Seconds" "Boolean , 33, Show Deciseconds"
-- !Arguments "NewLine, LuaScript, The function to call when the time is up"
-- !Arguments "NewLine, String, Arguments"
-- !Arguments "NewLine, Boolean , 100, Show debug messages in console"
LevelFuncs.Engine.Node.CreateTimerWithFunction = function(name, time, loop, minutes, seconds, deciseconds, luaFunction, args, debug)
    if name ~= '' then
        local nodeTimerFormat = { minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        local argsTable = args ~= '' and table.unpack(LevelFuncs.Engine.Node.SplitString(args, ",")) or nil
        LevelVars.nodeTimers[name] = {}
        LevelVars.nodeTimers[name].timer = Timer.Create(name, time, loop, nodeTimerFormat, luaFunction, argsTable)
        LevelVars.nodeTimers[name].remainingTime = Timer.Get(name):GetRemainingTime()
        LevelVars.nodeTimers[name].remainingTimeFormatted = tostring(math.floor(Timer.Get(name):GetRemainingTime() * 10) / 10)
        LevelVars.nodeTimers[name].test = false
        LevelVars.nodeTimers[name].debug = debug
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer with Function '" .. name .. "' created successfully!", LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create Timer with Function' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Create timer with volume event set"
-- !Conditional "False"
-- !Description "Creates a countdown that triggers a volume event set upon ending.\n\nNot to be used inside the `On Volume Inside` or `On Loop` events."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 | 1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, Loop"
-- !Arguments "NewLine, Boolean , 33, Show Minutes" "Boolean , 33, Show Seconds" "Boolean , 33, Show Deciseconds"
-- !Arguments "NewLine, 66, VolumeEventSets, The event set to be called when the time is up"
-- !Arguments "VolumeEvents, 34, Event to run"
-- !Arguments "NewLine, Moveables, Activator for the event (when necessary)"
-- !Arguments "NewLine, Boolean , 100, Show debug messages in console"
LevelFuncs.Engine.Node.CreateTimerWithEventSet = function(name, time, loop, minutes, seconds, deciseconds, setName, eventType, activator, debug)
    if name ~= '' then
        local nodeTimerFormat = { minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        LevelVars.nodeTimers[name] = {}
        LevelVars.nodeTimers[name].timer = Timer.Create(name, time, loop, nodeTimerFormat, LevelFuncs.Engine.Node.RunEventSet, setName, eventType, activator)
        LevelVars.nodeTimers[name].remainingTime = Timer.Get(name):GetRemainingTime()
        LevelVars.nodeTimers[name].remainingTimeFormatted = tostring(math.floor(Timer.Get(name):GetRemainingTime() * 10) / 10)
        LevelVars.nodeTimers[name].test = false
        LevelVars.nodeTimers[name].debug = debug
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer with volume event set '" .. name .. "' created successfully", LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create Timer with Function' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Create timer with global event set"
-- !Conditional "False"
-- !Description "Creates a countdown that triggers a global event set upon ending.\n\nNot to be used inside the `On Volume Inside` or `On Loop` events."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 | 1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, Loop"
-- !Arguments "NewLine, Boolean , 33, Show Minutes" "Boolean , 33, Show Seconds" "Boolean , 33, Show Deciseconds"
-- !Arguments "NewLine, 70, GlobalEventSets, The event set to be called when the time is up"
-- !Arguments "GlobalEvents, 30, Event to run"
-- !Arguments "NewLine, Moveables, Activator for the event (when necessary)"
-- !Arguments "NewLine, Boolean , 100, Show debug messages in console"
LevelFuncs.Engine.Node.CreateTimerWithGEventSet = function(name, time, loop, minutes, seconds, deciseconds, setName, eventType, activator, debug)
    if name ~= '' then
        local nodeTimerFormat = { minutes = minutes, seconds = seconds, deciseconds = deciseconds }
        LevelVars.nodeTimers[name] = {}
        LevelVars.nodeTimers[name].timer = Timer.Create(name, time, loop, nodeTimerFormat, LevelFuncs.Engine.Node.RunGlobalEventSet, setName, eventType, activator)
        LevelVars.nodeTimers[name].remainingTime = Timer.Get(name):GetRemainingTime()
        LevelVars.nodeTimers[name].remainingTimeFormatted = tostring(math.floor(Timer.Get(name):GetRemainingTime() * 10) / 10)
        LevelVars.nodeTimers[name].test = false
        LevelVars.nodeTimers[name].debug = debug
        if LevelVars.nodeTimers[name].debug then
            TEN.Util.PrintLog("Timer with global event set '" .. name .. "' created successfully", LogLevel.INFO)
        end
    else
        TEN.Util.PrintLog("Error in the 'Create Timer with Function' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Start timer"
-- !Conditional "False"
-- !Description "Begins or resumes a timer.\n\nNot to be used inside the On Volume Inside or On Loop events."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
-- !Arguments "NewLine, Boolean , Reset timer when expired"
LevelFuncs.Engine.Node.StartTimer = function(name, reset)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            if reset and not Timer.Get(name):IsPaused() then
                Timer.Get(name):SetRemainingTime(LevelVars.nodeTimers[name].remainingTime)
            end
            Timer.Get(name):Start()
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' has started", LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Start Timer' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Stop timer"
-- !Conditional "False"
-- !Description "Stops a timer.\n\nNot to be used inside the `On Volume Inside` or `On Loop` events."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.StopTimer = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):Stop()
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' has been stopped", LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Stop Timer' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Pause timer"
-- !Conditional "False"
-- !Description "Pauses a timer.\n\nNot to be used inside the On Volume Inside or On Loop events."
-- !Section "Timer"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.SetPausedTimer = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):SetPaused(true)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' has been paused", LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set Paused Timer' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Set remaining time"
-- !Conditional "False"
-- !Description "Changes the remaining time value (in seconds) of a specific timer.\n\nNot to be used inside the `On Volume Inside` or `On Loop` events."
-- !Section "Timer"
-- !Arguments "NewLine, String, 67, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 33, [ 0 | 1000 | 1 | 0.1 | 1 ], the new time remaining for the timer"
LevelFuncs.Engine.Node.SetRemainingTime = function(name, remainingTime)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):SetRemainingTime(remainingTime)
            LevelVars.nodeTimers[name].remainingTimeFormatted = tostring(math.floor(Timer.Get(name):GetRemainingTime() * 10) / 10)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' remaining time:" .. remainingTime .. " seconds", LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set Remaining Time' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Set total time"
-- !Conditional "False"
-- !Description "Changes the total duration value (in seconds) of a specific timer.\n\nNot to be used inside the `On Volume Inside` or `On Loop` events."
-- !Section "Timer"
-- !Arguments "NewLine, String, 67, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 33, [ 0 | 1000 | 1 | 0.1 | 1 ], timer's new total time"
LevelFuncs.Engine.Node.SetTotalTime = function(name, totalTime)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            Timer.Get(name):SetTotalTime(totalTime)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' total time: " .. totalTime .. " seconds", LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set Total Time' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Set timer loop"
-- !Conditional "False"
-- !Description "Sets an existing timer as looped or one shot.\n\nNot to be used inside the `On Volume Inside` or `On Loop` events."
-- !Section "Timer"
-- !Arguments "NewLine, String, 67, [ NoMultiline ], Timer name"
-- !Arguments "Enumeration, 33, [ One shot | Looped ], Set timer as"
LevelFuncs.Engine.Node.SetLooping = function(name, looping)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            local state = (looping == 1) and true or false
            Timer.Get(name):SetLooping(state)
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. "' loop: " .. tostring(state), LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Set Timer Loop' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Get remaining time (in seconds) in console"
-- !Conditional "False"
-- !Description "Prints on console the remaining time value (in seconds) of a specific timer.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.GetRemainingTime = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            if LevelVars.nodeTimers[name].test then
                TEN.Util.PrintLog("Timer '" .. name .. "' remaining time: " .. LevelVars.nodeTimers[name].remainingTimeFormatted, LogLevel.INFO)
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Get Remaining Time' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "Get total time (in seconds) in console"
-- !Conditional "False"
-- !Description "Prints on console the total time value (in seconds) of a specific timer.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.GetTotalTime = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            TEN.Util.PrintLog("Timer '" .. name .. "' total time: " .. Timer.Get(name):GetTotalTime(), LogLevel.INFO)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Get Total Time' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "If the timer active..."
-- !Conditional "True"
-- !Description "Checks if a specific timer is active.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IsTimerActive = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            local status = Timer.Get(name):IsActive()
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. (status and "' is active" or "' is not active"), LogLevel.INFO)
            end
            return status
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Timer is Active' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "If the timer is paused..."
-- !Conditional "True"
-- !Description "Checks if a specific timer is paused.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IsTimerPaused = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            local status = Timer.Get(name):IsPaused()
            if LevelVars.nodeTimers[name].debug then
                TEN.Util.PrintLog("Timer '" .. name .. (status and "' is paused" or "' is not paused"), LogLevel.INFO)
            end
            return status
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'Timer is Paused' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "If timer has expired..."
-- !Conditional "True"
-- !Description "Checks if a specific timer is expired.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IfTimerExpired = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            local temp = tonumber(string.format("%.1f", Timer.Get(name):GetRemainingTime()))
            return (temp == 0.0) and true or false
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If Timer has expired' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "If remaining time is..."
-- !Conditional "True"
-- !Description "Checks if the remaining time is equal to, greater than, less than..\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Timer name"
-- !Arguments "CompareOperator, 30"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 1 | 0.1 | 1 ], Remaining time (in seconds)"
LevelFuncs.Engine.Node.IfRemainingTimeIs = function(name, operator, value)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            if Timer.Get(name):IsActive() and LevelVars.nodeTimers[name].test then
                local remainingTime = LevelVars.nodeTimers[name].remainingTimeFormatted
                local result = LevelFuncs.Engine.Node.CompareValue(remainingTime, tostring(value + 0.0) , operator)
                if LevelVars.nodeTimers[name].debug then
                    TEN.Util.PrintLog("If the remaining time is:"..  tostring(value + 0.0) .. ". Remaining time:" .. remainingTime .. ". Result: " .. tostring(result), LogLevel.INFO)
                end
                return result
            end
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If remaining Time is' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "If total time is..."
-- !Conditional "True"
-- !Description "Checks if the Total Time is equal to, greater than, less than..\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Timer name"
-- !Arguments "CompareOperator, 30"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 1 | 0.1 | 1 ], Total Time (in seconds)"
LevelFuncs.Engine.Node.IfTotalTimeIs = function(name, operator, time)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            return LevelFuncs.Engine.Node.CompareValue(Timer.Get(name):GetTotalTime(), time, operator)
        else
            TEN.Util.PrintLog("Timer '" .. name .. "' does not exist", LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog("Error in the 'If Total Time is' node. No timer name provided", LogLevel.ERROR)
    end
end

-- !Name "If timer exists..."
-- !Conditional "True"
-- !Description "Checks if a specific timer exists."
-- !Section "Timer"
-- !Arguments "NewLine, String, 100, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.IfTimerExists = function(name)
    if name ~= '' then
        return (Timer.Get(name) ~= nil) and true or false
    else
        TEN.Util.PrintLog("Error in the 'If timer exists' node. No timer name provided", LogLevel.ERROR)
    end
end

LevelFuncs.Engine.Timer.FormatRemainingTime = function(dt)
    for name, timer in pairs(LevelVars.nodeTimers) do
        if Timer.Get(name):IsActive() then
            if timer.remainingTimeFormatted ~= tostring(math.floor(Timer.Get(name):GetRemainingTime() * 10) / 10) then
                timer.remainingTimeFormatted = tostring(math.floor(Timer.Get(name):GetRemainingTime() * 10) / 10)
                timer.test = true
            else
                timer.test = false
            end
        end
    end
end

TEN.Logic.AddCallback(TEN.Logic.CallbackPoint.PRELOOP, LevelFuncs.Engine.Timer.FormatRemainingTime)
