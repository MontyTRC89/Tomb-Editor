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
        public const uint Version = 3;
        public const uint VersionCompressed = Version ^ 0x80000000;

        public static readonly ChunkID Settings = ChunkID.FromString("TeSettings");
        /**/public static readonly ChunkID WadFilePath = ChunkID.FromString("TeWadFilePath");
        /**/public static readonly ChunkID FontTextureFilePath = ChunkID.FromString("TeFontTextureFilePath");
        /**/public static readonly ChunkID SkyTextureFilePath = ChunkID.FromString("TeSkyTextureFilePath");
        /**/public static readonly ChunkID SoundPaths = ChunkID.FromString("TeSoundPaths");
        /******/public static readonly ChunkID SoundPath = ChunkID.FromString("TeSoundPath"); //String
        /**/public static readonly ChunkID GameDirectory = ChunkID.FromString("TeGameDirectory");
        /**/public static readonly ChunkID GameLevelFilePath = ChunkID.FromString("TeGameLevelFilePath");
        /**/public static readonly ChunkID GameExecutableFilePath = ChunkID.FromString("TeGameExecutableFilePath");
        /**/public static readonly ChunkID GameExecutableSuppressAskingForOptions = ChunkID.FromString("TeGameExecutableSuppressAskingForOptions");
        /**/public static readonly ChunkID IgnoreMissingSounds = ChunkID.FromString("TeIgnoreMissingSounds");
        /**/public static readonly ChunkID Textures = ChunkID.FromString("TeTextures");
        /******/public static readonly ChunkID Texture = ChunkID.FromString("TeTexture");
        /**********/public static readonly ChunkID TexturePath = ChunkID.FromString("TePath");
        /**********/public static readonly ChunkID TextureConvert512PixelsToDoubleRows = ChunkID.FromString("Te512C");
        /**********/public static readonly ChunkID TextureReplaceMagentaWithTransparency = ChunkID.FromString("TeMagentaR");
        public static readonly ChunkID Rooms = ChunkID.FromString("TeRooms");
        /**/public static readonly ChunkID Room = ChunkID.FromString("TeRoom"); // Contains X, Y sectors, Name, Position directly
        /******/public static readonly ChunkID Sectors = ChunkID.FromString("TeSectors");
        /**********/public static readonly ChunkID Sector = ChunkID.FromString("TeS");
        /**************/public static readonly ChunkID SectorGeometry = ChunkID.FromString("TeG"); //EDFaces, QAFaces, WSFaces, RFFaces, [*]SplitDirectionToggled, 
        /**************/public static readonly ChunkID SectorProperties = ChunkID.FromString("TeP"); //Flags, Opacitiy, ...
        /******/public static readonly ChunkID RoomProperties = ChunkID.FromString("TeRoomP"); // Ambient, Flags, ...
        /******/public static readonly ChunkID Objects = ChunkID.FromString("TeObjects");
        /**********/public static readonly ChunkID Camera = ChunkID.FromString("TeCam");
        /**************/public static readonly ChunkID CameraData = ChunkID.FromString("TeDat");
        /**************/public static readonly ChunkID Pos = ChunkID.FromString("TePos");
        /**********/public static readonly ChunkID FlybyCamera = ChunkID.FromString("TeFlyBy");
        /**************///public static readonly ChunkID Pos = ChunkID.FromString("TePos"); // Used here too
        /**************/public static readonly ChunkID FlyCameraData = ChunkID.FromString("TeDat");
        /**********/public static readonly ChunkID Light = ChunkID.FromString("TeLight");
        /**************///public static readonly ChunkID Pos = ChunkID.FromString("TePos"); // Used here too
        /**************/public static readonly ChunkID LightData = ChunkID.FromString("TeDat");
        // ...
    }
}
