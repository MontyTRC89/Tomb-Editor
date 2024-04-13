local Timer = require("Engine.Timer")
LevelVars.TimerRemainingTime = {}

-- !Name "Create Basic timer"
-- !Conditional "False"
-- !Description "Creates a simple countdown.\n\nNot to be used inside the `On Volume Inside` or `On Loop` events."
-- !Section "Timer"
-- !Arguments "NewLine, String, 57, [ NoMultiline ], Timer name"
-- !Arguments "Numerical, 30, [ 0 | 1000 | 1 | 0.1 | 1 ], The duration of the timer in seconds"
-- !Arguments "Boolean , 13, Loop"
-- !Arguments "NewLine, Boolean , 33, Show Minutes" "Boolean , 33, Show Seconds" "Boolean , 33, Show Deciseconds"
LevelFuncs.Engine.Node.CreateTimer = function(name, time, loop, minutes, seconds, deciseconds)
    if name ~= '' then
        LevelVars[name] = Timer.Create(name, time, loop,
            { minutes = minutes, seconds = seconds, deciseconds = deciseconds }, nil)
        LevelVars.TimerRemainingTime[name] = Timer.Get(name):GetRemainingTime()
        TEN.Util.PrintLog('Timer "' .. name .. '" created successfully!', LogLevel.INFO)
    else
        TEN.Util.PrintLog('Error in the "Create Timer" node. No timer name provided', LogLevel.ERROR)
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
LevelFuncs.Engine.Node.CreateTimerWithFunction = function(name, time, loop, minutes, seconds, deciseconds, luaFunction)
    if name ~= '' then
        LevelVars[name] = Timer.Create(name, time, loop,
            { minutes = minutes, seconds = seconds, deciseconds = deciseconds }, luaFunction)
        LevelVars.TimerRemainingTime[name] = Timer.Get(name):GetRemainingTime()
        TEN.Util.PrintLog('Timer with Function "' .. name .. '" created successfully!', LogLevel.INFO)
    else
        TEN.Util.PrintLog('Error in the "Create Timer with Function" node. No timer name provided', LogLevel.ERROR)
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
LevelFuncs.Engine.Node.CreateTimerWithEventSet = function(name, time, loop, minutes, seconds, deciseconds, setName,
                                                          eventType, activator)
    if name ~= '' then
        LevelVars[name] = Timer.Create(name, time, loop,
            { minutes = minutes, seconds = seconds, deciseconds = deciseconds }, LevelFuncs.Engine.Node.RunEventSet,
            setName, eventType, activator)
        LevelVars.TimerRemainingTime[name] = Timer.Get(name):GetRemainingTime()
        TEN.Util.PrintLog('Timer with volume event set "' .. name .. '" created successfully', LogLevel.INFO)
    else
        TEN.Util.PrintLog('Error in the "Create Timer with Function" node. No timer name provided', LogLevel.ERROR)
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
LevelFuncs.Engine.Node.CreateTimerWithGEventSet = function(name, time, loop, minutes, seconds, deciseconds, setName,
                                                           eventType, activator)
    if name ~= '' then
        LevelVars[name] = Timer.Create(name, time, loop,
            { minutes = minutes, seconds = seconds, deciseconds = deciseconds }, LevelFuncs.Engine.Node.RunEventSet,
            setName, eventType, activator)
        LevelVars.TimerRemainingTime[name] = Timer.Get(name):GetRemainingTime()
        TEN.Util.PrintLog('Timer with global event set "' .. name .. '" created successfully', LogLevel.INFO)
    else
        TEN.Util.PrintLog('Error in the "Create Timer with Function" node. No timer name provided', LogLevel.ERROR)
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
                Timer.Get(name):SetRemainingTime(LevelVars.TimerRemainingTime[name])
            end
            Timer.Get(name):Start()
            TEN.Util.PrintLog('Timer "' .. name .. '" has started', LogLevel.INFO)
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Start Timer" node. No timer name provided', LogLevel.ERROR)
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
            TEN.Util.PrintLog('Timer "' .. name .. '" has been stopped', LogLevel.INFO)
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Stop Timer" node. No timer name provided', LogLevel.ERROR)
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
            TEN.Util.PrintLog('The "' .. name .. '" Timer has been paused', LogLevel.INFO)
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Set Paused Timer" node. No timer name provided', LogLevel.ERROR)
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
            TEN.Util.PrintLog('Timer "' .. name .. '" remaining time: ' .. remainingTime .. " seconds",
                LogLevel.INFO)
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Set Remaining Time" node. No timer name provided', LogLevel.ERROR)
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
            TEN.Util.PrintLog('Timer "' .. name .. '" total time: ' .. totalTime .. " seconds", LogLevel.INFO)
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Set Total Time" node. No timer name provided', LogLevel.ERROR)
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
            TEN.Util.PrintLog('Timer "' .. name .. '" Loop: ' .. tostring(state), LogLevel.INFO)
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Set Timer Loop" node. No timer name provided', LogLevel.ERROR)
    end
end

-- !Name "Get remaining time (in seconds) in console."
-- !Conditional "False"
-- !Description "Prints on console the remaining time value (in seconds) of a specific timer.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.GetRemainingTime = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            print("remaining time: " .. string.format("%.1f", Timer.Get(name):GetRemainingTime()))
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Get Remaining Time" node. No timer name provided', LogLevel.ERROR)
    end
end

-- !Name "Get total time (in seconds) in console."
-- !Conditional "False"
-- !Description "Prints on console the total time value (in seconds) of a specific timer.\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, [ NoMultiline ], Timer name"
LevelFuncs.Engine.Node.GetTotalTime = function(name)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            print("Total time: " .. Timer.Get(name):GetTotalTime())
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Get Total Time" node. No timer name provided', LogLevel.ERROR)
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
            local mss = Timer.Get(name):IsActive() and
                'Timer "' .. name .. '" is active' or
                'Timer "' .. name .. '" is not active'
            TEN.Util.PrintLog(mss, LogLevel.INFO)
            return Timer.Get(name):IsActive()
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Timer is Active" node. No timer name provided', LogLevel.ERROR)
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
            return Timer.Get(name):IsPaused()
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "Timer is Paused" node. No timer name provided', LogLevel.ERROR)
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
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "If Timer has expired" node. No timer name provided', LogLevel.ERROR)
    end
end

-- !Name "If remaining time is..."
-- !Conditional "True"
-- !Description "Checks if the remaining time is equal to, greater than, less than..\n\nTo be used inside the `On Volume Inside` or `On Loop` events only."
-- !Section "Timer"
-- !Arguments "NewLine, String, 50, [ NoMultiline ], Timer name"
-- !Arguments "CompareOperator, 30"
-- !Arguments "Numerical, 20, [ 0 | 1000 | 1 | 0.1 | 1 ], Remaining time (in seconds)"
LevelFuncs.Engine.Node.IfRemainingTimeIs = function(name, operator, time)
    if name ~= '' then
        if Timer.Get(name) ~= nil then
            local temp = tonumber(string.format("%.1f", Timer.Get(name):GetRemainingTime()))
            return LevelFuncs.Engine.Node.CompareValue(temp, time, operator)
        else
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "If remaining Time is" node. No timer name provided', LogLevel.ERROR)
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
            TEN.Util.PrintLog('Timer "' .. name .. '" does not exist', LogLevel.ERROR)
        end
    else
        TEN.Util.PrintLog('Error in the "If Total Time is" node. No timer name provided', LogLevel.ERROR)
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
        TEN.Util.PrintLog('Error in the "If timer exists" node. No timer name provided', LogLevel.ERROR)
    end
end
