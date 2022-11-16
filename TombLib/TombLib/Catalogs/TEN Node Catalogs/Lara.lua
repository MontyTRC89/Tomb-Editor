-- !Name "If Lara air value is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks current Lara air value."
-- !Arguments "CompareOperator, 30, Kind of check"
-- !Arguments "Numerical, 20, Air value, [ 0 | 1800 ]"

LevelFuncs.Engine.Node.TestLaraAir = function(operator, value)
    return LevelFuncs.Engine.Node.CompareValue(TEN.Objects.Lara:GetAir(), value, operator)
end

-- !Name "Modify Lara air value"
-- !Section "Lara state"
-- !Description "Sets Lara air value to specified."
-- !Arguments "Enumeration, [ Change | Set ], 20, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -1000 | 1000 | 0 | 1 | 5 ], 15, Air value to define"

LevelFuncs.Engine.Node.ModifyLaraAir = function(operation, value)

	if (operation == 0) then
		TEN.Objects.Lara:SetAir(TEN.Objects.Lara:GetAir() + value)
	else
		TEN.Objects.Lara:SetAir(value)
	end
end

-- !Name "If Lara poison value is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks current Lara poison value."
-- !Arguments "CompareOperator, 30, Kind of check"
-- !Arguments "Numerical, 20, Poison value, [ 0 | 1800 ]"

LevelFuncs.Engine.Node.TestLaraPoison = function(operator, value)
    return LevelFuncs.Engine.Node.CompareValue(TEN.Objects.Lara:GetPoison(), value, operator)
end

-- !Name "Modify Lara poison value"
-- !Section "Lara state"
-- !Description "Sets Lara poison value to specified."
-- !Arguments "Enumeration, [ Change | Set ], 20, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -1000 | 1000 | 0 | 1 | 5 ], 15, Poison value to define"

LevelFuncs.Engine.Node.ModifyLaraPoison = function(operation, value)

	if (operation == 0) then
		TEN.Objects.Lara:SetPoison(TEN.Objects.Lara:GetPoison() + value)
	else
		TEN.Objects.Lara:SetPoison(value)
	end
end

-- !Name "If Lara is on fire..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks if Lara is currently on fire."

LevelFuncs.Engine.Node.TestLaraOnFire = function(operator, value)
    return TEN.Objects.Lara:GetOnFire()
end

-- !Name "Set Lara on fire"
-- !Section "Lara state"
-- !Description "Sets or unsets Lara on fire."
-- !Arguments "Boolean, 20, Fire is on"

LevelFuncs.Engine.Node.SetLaraOnFire = function(value)
    return TEN.Objects.Lara:SetOnFire(value)
end