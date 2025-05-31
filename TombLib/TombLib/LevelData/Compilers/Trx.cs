using System;
using System.IO;
using TombLib.IO;
using TombLib.LevelData.Compilers.Util;

namespace TombLib.LevelData.Compilers;

public partial class LevelCompilerClassicTR
{
    private const int _legacyRoomLimit = 255;

    private void WriteLevelTrx()
    {
        switch (_level.Settings.GameVersion)
        {
            case TRVersion.Game.TR1:
                WriteLevelTr1();
                break;
            case TRVersion.Game.TR2:
                WriteLevelTr2();
                break;
            default:
                throw new NotImplementedException("The selected game engine is not supported yet");
        }

        ReportProgress(98, "Writing TRX data");

        var injData = new TrxInjectionData();
        GenerateTrxSectorEdits(injData);

        using var writer = new BinaryWriterEx(new FileStream(_dest, FileMode.Append));
        TrxInjector.Serialize(injData, writer);
    }

    private void GenerateTrxSectorEdits(TrxInjectionData data)
    {
        foreach (var (teRoom, trRoom) in _tempRooms)
        {
            short roomIndex = (short)_roomRemapping[teRoom];
            for (ushort x = 1; x < teRoom.NumXSectors - 1; x++)
            {
                for (ushort z = 1; z < teRoom.NumZSectors - 1; z++)
                {
                    var teSector = teRoom.Sectors[x, z];
                    short roomBelow = -1;
                    short roomAbove = -1;
                    if (teSector.FloorPortal != null && teSector.FloorPortal.Opacity != PortalOpacity.SolidFaces)
                    {
                        roomBelow = (short)_roomRemapping[teSector.FloorPortal.AdjoiningRoom];
                    }
                    if (teSector.CeilingPortal != null && teSector.CeilingPortal.Opacity != PortalOpacity.SolidFaces)
                    {
                        roomAbove = (short)_roomRemapping[teSector.CeilingPortal.AdjoiningRoom];
                    }

                    if (roomBelow < _legacyRoomLimit && roomAbove < _legacyRoomLimit)
                    {
                        continue;
                    }

                    data.SectorEdits.Add(new()
                    {
                        RoomIndex = roomIndex,
                        X = x,
                        Z = z,
                        BaseSector = trRoom.Sectors[x * teRoom.NumZSectors + z],
                        RoomAboveExt = roomAbove,
                        RoomBelowExt = roomBelow,
                    });
                }
            }
        }
    }
}
