-- !Name "Set display colour mode"
-- !Section "View"
-- !Description "Set the colour mode for all graphics"
-- !Arguments "NewLine, Enumeration, 50, [ None | Monochrome | Negative | Exclusion ], Sets the base color operation"
-- !Arguments "Color, 25, Set colour of tint that overlays over the base color operation"
-- !Arguments "Numerical, 25, [ 0 | 1 | 1 | 0.1 | 0.5 ],Effect strength"

LevelFuncs.Engine.Node.SetPostProcessDisplay = function(postProcessEffectEnum, tintColor,power) 

    local postProcessEffect = LevelFuncs.Engine.Node.SetPostProcessMode(postProcessEffectEnum)
    TEN.View.SetPostProcessMode(postProcessEffect)
    TEN.View.SetPostProcessStrength(power)
    TEN.View.SetPostProcessTint(tintColor)
end