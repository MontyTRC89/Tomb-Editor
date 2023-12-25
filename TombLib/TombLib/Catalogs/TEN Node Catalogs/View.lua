-- !Name "Set display postprocessing mode"
-- !Section "View"
-- !Description "Set the postprocessing mode for all graphics, excluding GUI"
-- !Arguments "NewLine, Enumeration, 50, [ None | Monochrome | Negative | Exclusion ], Sets the color mode"
-- !Arguments "Color, 25, Set the tint color that overlays over the chosen color mode"
-- !Arguments "Numerical, 25, [ 0 | 1 | 1 | 0.1 | 0.5 ], Color overlay strength"

LevelFuncs.Engine.Node.SetPostProcessDisplay = function(postProcessModeEnum, tintColor, power) 

    local postProcessMode = LevelFuncs.Engine.Node.SetPostProcessMode(postProcessModeEnum)
    TEN.View.SetPostProcessMode(postProcessMode)
    TEN.View.SetPostProcessStrength(power)
    TEN.View.SetPostProcessTint(tintColor)
end