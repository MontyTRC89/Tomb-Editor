using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;

namespace TombEditor.Geometry.IO
{
    // TODO Add documentation for binary offsets of the chunks
    internal static class Prj2Chunks
    {
        public static readonly byte[] MagicNumber = new byte[] { 0x50, 0x52, 0x4A, 0x32 };

        public static readonly ChunkId Settings = ChunkId.FromString("TeSettings");
        /**/public static readonly ChunkId WadFilePath = ChunkId.FromString("TeWadFilePath"); // UTF-8 string
        /**/public static readonly ChunkId FontTextureFilePath = ChunkId.FromString("TeFontTextureFilePath"); // UTF-8 string
        /**/public static readonly ChunkId SkyTextureFilePath = ChunkId.FromString("TeSkyTextureFilePath"); // UTF-8 string
        /**/public static readonly ChunkId OldWadSoundPaths = ChunkId.FromString("TeOldWadSoundPaths");
        /******/public static readonly ChunkId OldWadSoundPath = ChunkId.FromString("TeOldWadSoundPath");
        /**********/public static readonly ChunkId OldWadSoundPathPath = ChunkId.FromString("TePath"); // UTF-8 string
        /**/public static readonly ChunkId GameDirectory = ChunkId.FromString("TeGameDirectory"); // UTF-8 string
        /**/public static readonly ChunkId GameLevelFilePath = ChunkId.FromString("TeGameLevelFilePath"); // UTF-8 string
        /**/public static readonly ChunkId GameExecutableFilePath = ChunkId.FromString("TeGameExecutableFilePath"); // UTF-8 string
        /**/public static readonly ChunkId GameEnableQuickStartFeature = ChunkId.FromString("TeGameEnableQuickStartFeature"); // UTF-8 string
        /**/public static readonly ChunkId GameVersion = ChunkId.FromString("TeGameVersion");
        /**/public static readonly ChunkId Textures = ChunkId.FromString("TeTextures");
        /******/public static readonly ChunkId InvisibleTexture = ChunkId.FromString("TeInvisibleTexture");
        /******/public static readonly ChunkId LevelTexture = ChunkId.FromString("TeLvlTexture");
        /**********/public static readonly ChunkId LevelTextureIndex = ChunkId.FromString("TeI");
        /**********/public static readonly ChunkId LevelTexturePath = ChunkId.FromString("TePath");
        /**********/public static readonly ChunkId LevelTextureConvert512PixelsToDoubleRows = ChunkId.FromString("Te512C");
        /**********/public static readonly ChunkId LevelTextureReplaceMagentaWithTransparency = ChunkId.FromString("TeMagentaR");
        /**********/public static readonly ChunkId LevelTextureSounds = ChunkId.FromString("TeTextureSounds");
        /**/public static readonly ChunkId ImportedGeometries = ChunkId.FromString("TeImportedGeometries");
        /******/public static readonly ChunkId ImportedGeometry = ChunkId.FromString("TeImportedGeometry");
        /********/public static readonly ChunkId ImportedGeometryIndex = ChunkId.FromString("TeI");
        /********/public static readonly ChunkId ImportedGeometryName = ChunkId.FromString("TeName");
        /********/public static readonly ChunkId ImportedGeometryPath = ChunkId.FromString("TePath");
        /********/public static readonly ChunkId ImportedGeometryScale = ChunkId.FromString("TeScale");
        /********/public static readonly ChunkId ImportedGeometryPosAxisFlags = ChunkId.FromString("TePosAxisFlags");
        /********/public static readonly ChunkId ImportedGeometryTexAxisFlags = ChunkId.FromString("TeTexAxisFlags");
        /**/public static readonly ChunkId AnimatedTextureSets = ChunkId.FromString("TeAnimatedTextureSets");
        /******/public static readonly ChunkId AnimatedTextureSet = ChunkId.FromString("TeAnimatedTextureSet");
        /**********/public static readonly ChunkId AnimatedTextureSetExtraInfo = ChunkId.FromString("TeAnimatedTextureSetExtra");
        /**************/public static readonly ChunkId AnimatedTextureFrames = ChunkId.FromString("TeFrames");
        /******************/public static readonly ChunkId AnimatedTextureFrame = ChunkId.FromString("TeFrame");
        public static readonly ChunkId Rooms = ChunkId.FromString("TeRooms");
        /**/public static readonly ChunkId Room = ChunkId.FromString("TeRoom"); // Contains X, Y sectors, Name, Position directly
        /******/public static readonly ChunkId RoomIndex = ChunkId.FromString("TeI");
        /******/public static readonly ChunkId RoomName = ChunkId.FromString("TeName");
        /******/public static readonly ChunkId RoomPosition = ChunkId.FromString("TePos");
        /******/public static readonly ChunkId RoomSectors = ChunkId.FromString("TeSecs");
        /**********/public static readonly ChunkId Sector = ChunkId.FromString("TeS");
        /**************/public static readonly ChunkId SectorProperties = new ChunkId(new byte[] { 0 }); // These chunks occur very often, this minimizes their size impact
        /**************/public static readonly ChunkId SectorFloor = new ChunkId(new byte[] { 1 });
        /**************/public static readonly ChunkId SectorCeiling = new ChunkId(new byte[] { 2 });
        /**************/public static readonly ChunkId TextureLevelTexture = new ChunkId(new byte[] { 16 });
        /**************/public static readonly ChunkId TextureInvisible = new ChunkId(new byte[] { 17 });
        /******/public static readonly ChunkId RoomAmbientLight = ChunkId.FromString("TeAmbient");
        /******/public static readonly ChunkId RoomAlternate = ChunkId.FromString("TeAlternate");
        /**********/public static readonly ChunkId AlternateRoom = ChunkId.FromString("TeRoom");
        /**********/public static readonly ChunkId AlternateGroup = ChunkId.FromString("TeGroup");
        /******/public static readonly ChunkId RoomFlagCold = ChunkId.FromString("TeCold");
        /******/public static readonly ChunkId RoomFlagDamage = ChunkId.FromString("TeDmg");
        /******/public static readonly ChunkId RoomFlagHorizon = ChunkId.FromString("TeHorizon");
        /******/public static readonly ChunkId RoomFlagOutside = ChunkId.FromString("TeOutside");
        /******/public static readonly ChunkId RoomFlagNoLensflare = ChunkId.FromString("TeNoLens");
        /******/public static readonly ChunkId RoomFlagRain = ChunkId.FromString("TeRain");
        /******/public static readonly ChunkId RoomFlagSnow = ChunkId.FromString("TeSnow");
        /******/public static readonly ChunkId RoomFlagQuickSand = ChunkId.FromString("TeQuickSand");
        /******/public static readonly ChunkId RoomFlagExcludeFromPathFinding = ChunkId.FromString("TeNoPath");
        /******/public static readonly ChunkId RoomWaterLevel = ChunkId.FromString("TeWater");
        /******/public static readonly ChunkId RoomMistLevel = ChunkId.FromString("TeMist");
        /******/public static readonly ChunkId RoomReflectionLevel = ChunkId.FromString("TeReflect");
        /******/public static readonly ChunkId RoomReverberation = ChunkId.FromString("TeReverb");
        /******/public static readonly ChunkId RoomLocked = ChunkId.FromString("TeLocked");
        /******/public static readonly ChunkId Objects = ChunkId.FromString("TeObjects");
        /**********/public static readonly ChunkId ObjectMovable = ChunkId.FromString("TeMov");
        /**********/public static readonly ChunkId ObjectStatic = ChunkId.FromString("TeSta");
        /**********/public static readonly ChunkId ObjectCamera = ChunkId.FromString("TeCam");
        /**********/public static readonly ChunkId ObjectFlyBy = ChunkId.FromString("TeFly");
        /**********/public static readonly ChunkId ObjectSink = ChunkId.FromString("TeSin");
        /**********/public static readonly ChunkId ObjectSoundSource = ChunkId.FromString("TeSou");
        /**********/public static readonly ChunkId ObjectImportedGeometry = ChunkId.FromString("TeImp");
        /**********/public static readonly ChunkId ObjectLight = ChunkId.FromString("TeLig");
        /**********/public static readonly ChunkId ObjectPortal = ChunkId.FromString("TePor");
        /**********/public static readonly ChunkId ObjectTrigger = ChunkId.FromString("TeTri");
        /************/public static readonly ChunkId ObjectTriggerExtra = ChunkId.FromString("TeTriEx");
    }
}
