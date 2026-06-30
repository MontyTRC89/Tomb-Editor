local PhotoMode = require("Engine.PhotoMode.PhotoMode")
-- !Name "Unlock outfit in Photo Mode"
-- !Section "User interface"
-- !Description "Unlocks outfit in photo mode for specified name."
-- !Arguments "NewLine, String, 100, Outfit Name set in PhotoMode Outfits.lua"
LevelFuncs.Engine.Node.PhotoModeUnlockOutfit = function(outfitName)
    PhotoMode.UnlockOutfit(outfitName)
end

-- !Name "Clear Outfits in Photo Mode"
-- !Section "User interface"
-- !Description "Clears all unlocked outfits in Photo Mode"
LevelFuncs.Engine.Node.PhotoModeClearOutfits = function()
    PhotoMode.ClearOutfits()
end