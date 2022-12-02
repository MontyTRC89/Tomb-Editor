-- !Name "If air value is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks current air value."
-- !Arguments "CompareOperator, 30, Kind of check"
-- !Arguments "Numerical, 20, Air value, [ 0 | 1800 ]"

LevelFuncs.Engine.Node.TestLaraAir = function(operator, value)
    return LevelFuncs.Engine.Node.CompareValue(TEN.Objects.Lara:GetAir(), value, operator)
end

-- !Name "Modify air value"
-- !Section "Lara state"
-- !Description "Sets air value to specified."
-- !Arguments "Enumeration, [ Change | Set ], 20, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -1000 | 1000 | 0 | 1 | 5 ], 15, Air value to define"

LevelFuncs.Engine.Node.ModifyLaraAir = function(operation, value)

	if (operation == 0) then
		TEN.Objects.Lara:SetAir(TEN.Objects.Lara:GetAir() + value)
	else
		TEN.Objects.Lara:SetAir(value)
	end
end

-- !Name "If poison value is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks current poison value."
-- !Arguments "CompareOperator, 30, Kind of check"
-- !Arguments "Numerical, 20, Poison value, [ 0 | 1800 ]"

LevelFuncs.Engine.Node.TestLaraPoison = function(operator, value)
    return LevelFuncs.Engine.Node.CompareValue(TEN.Objects.Lara:GetPoison(), value, operator)
end

-- !Name "Modify poison value"
-- !Section "Lara state"
-- !Description "Sets poison value to specified."
-- !Arguments "Enumeration, [ Change | Set ], 20, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -1000 | 1000 | 0 | 1 | 5 ], 15, Poison value to define"

LevelFuncs.Engine.Node.ModifyLaraPoison = function(operation, value)

	if (operation == 0) then
		TEN.Objects.Lara:SetPoison(TEN.Objects.Lara:GetPoison() + value)
	else
		TEN.Objects.Lara:SetPoison(value)
	end
end

-- !Name "If sprint energy value is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks current sprint energy value."
-- !Arguments "CompareOperator, 30, Kind of check"
-- !Arguments "Numerical, 20, Sprint energy value, [ 0 | 120 ]"

LevelFuncs.Engine.Node.TestLaraSprint = function(operator, value)
    return LevelFuncs.Engine.Node.CompareValue(TEN.Objects.Lara:GetSprintEnergy(), value, operator)
end

-- !Name "Modify sprint energy value"
-- !Section "Lara state"
-- !Description "Sets sprint energy value to specified."
-- !Arguments "Enumeration, [ Change | Set ], 20, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -120 | 120 | 0 | 1 | 5 ], 15, Sprint energy value to define"

LevelFuncs.Engine.Node.ModifyLaraSprint = function(operation, value)

	if (operation == 0) then
		TEN.Objects.Lara:SetSprintEnergy(TEN.Objects.Lara:GetSprintEnergy() + value)
	else
		TEN.Objects.Lara:SetSprintEnergy(value)
	end
end

-- !Name "If wetness value is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks current wetness value."
-- !Arguments "CompareOperator, 30, Kind of check"
-- !Arguments "Numerical, 20, Wetness value, [ 0 | 255 ]"

LevelFuncs.Engine.Node.TestLaraWet = function(operator, value)
    return LevelFuncs.Engine.Node.CompareValue(TEN.Objects.Lara:GetWet(), value, operator)
end

-- !Name "Modify wetness value"
-- !Section "Lara state"
-- !Description "Sets wetness value to specified."
-- !Arguments "Enumeration, [ Change | Set ], 20, Change adds/subtracts given value while Set forces it."
-- !Arguments "Numerical, [ -255 | 255 | 0 | 1 | 5 ], 15, Wetness value to define"

LevelFuncs.Engine.Node.ModifyLaraWet = function(operation, value)

	if (operation == 0) then
		TEN.Objects.Lara:SetWet(TEN.Objects.Lara:GetWet() + value)
	else
		TEN.Objects.Lara:SetWet(value)
	end
end

-- !Name "Undraw weapon"
-- !Section "Lara state"
-- !Description "Undraws any currently selected weapon."

LevelFuncs.Engine.Node.UndrawWeapon = function()
    return TEN.Objects.Lara:UndrawWeapon()
end

-- !Name "Throw away torch"
-- !Section "Lara state"
-- !Description "Throws torch away, if it is currently present in hand."

LevelFuncs.Engine.Node.ThrowAwayTorch = function()
    return TEN.Objects.Lara:ThrowAwayTorch()
end

-- !Name "If current hand status is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks current hand status."
-- !Arguments "Enumeration, [ Free | Busy | Draw | Undraw | Armed | Special ], 25, Current hand state value."

LevelFuncs.Engine.Node.TestLaraHandStatus = function(value)
    return (TEN.Objects.Lara:GetHandStatus() == value)
end

-- !Name "If current weapon is..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks current weapon."
-- !Arguments "Enumeration, [ None | Pistols | Revolver | Uzis | Shotgun | HK | Crossbow | Flare | Torch | Grenade Gun | Harpoon Gun | Rocket Launcher | Snowmobile guns ], 50, Current weapon type."

LevelFuncs.Engine.Node.TestLaraWeaponType = function(value)
    return (TEN.Objects.Lara:GetWeaponType() == value)
end

-- !Name "Set current weapon"
-- !Section "Lara state"
-- !Description "Sets weapon to selected one."
-- !Arguments "Enumeration, [ None | Pistols | Revolver | Uzis | Shotgun | HK | Crossbow | Flare | Torch | Grenade Gun | Harpoon Gun | Rocket Launcher | Snowmobile guns ], 35, Current weapon type."
-- !Arguments "Boolean, Draw, 15"

LevelFuncs.Engine.Node.SetLaraWeaponType = function(value, draw)
    TEN.Objects.Lara:SetWeaponType(value, draw)
end

-- !Name "If Lara is on a vehicle..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks if Lara is currently mounting a vehicle."

LevelFuncs.Engine.Node.TestLaraVehicle = function()
    return TEN.Objects.Lara:GetVehicle() ~= nil
end

-- !Name "If Lara is targeting enemy..."
-- !Section "Lara state"
-- !Conditional "True"
-- !Description "Checks if Lara is currently targeting an enemy."

LevelFuncs.Engine.Node.TestLaraTargeting = function()
    return TEN.Objects.Lara:GetTarget() ~= nil
end