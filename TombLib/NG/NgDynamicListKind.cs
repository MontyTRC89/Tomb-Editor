using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.NG
{
    public enum NgDynamicListKind
    {
        Rooms255, // ROOMS_255
        SoundEffectsA, // SOUND_EFFECT_A
        SoundEffectsB, // SOUND_EFFECT_B
        Sfx1024, // SFX_1024
        NgStringsList255, // NG_STRING_LIST_255
        NgStringsAll, // NG_STRING_LIST_ALL
        PsxStringsList, // PSX_STRING_LIST
        PcStringsList, // PC_STRING_LIST
        StringsList255, // STRING_LIST_255
        MoveablesInLevel, // MOVEABLES
        SinksInLevel, // SINK_LIST
        StaticsInLevel, // STATIC_LIST
        FlybyCamerasInLevel, // FLYBY_LIST
        CamerasInLevel, // CAMERA_EFFECTS
        WadSlots, // WAD-SLOTS
        StaticsSlots, // STATIC_SLOTS
    }
}
