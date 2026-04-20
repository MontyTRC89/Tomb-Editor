using System.Globalization;
using System.Reflection;
using TombLib.LevelData;
using TombLib.LevelData.Compilers.TombEngine;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;

namespace TombLib.Test;

[TestClass]
public class DiagonalWallCollisionShapeTests
{
    private static readonly Type RoomSectorShapeType = typeof(LevelCompilerTombEngine)
        .GetNestedType("RoomSectorShape", BindingFlags.NonPublic)!;

    [DataTestMethod]
    [DataRow(DiagonalSplit.XnZn, "HeightXpZp", "HeightXnZp", "HeightXpZn")]
    [DataRow(DiagonalSplit.XnZp, "HeightXpZn", "HeightXnZn", "HeightXpZp")]
    [DataRow(DiagonalSplit.XpZn, "HeightXnZp", "HeightXnZn", "HeightXpZp")]
    [DataRow(DiagonalSplit.XpZp, "HeightXnZn", "HeightXnZp", "HeightXpZn")]
    public void RoomSectorShape_FlattensDiagonalWallFloorCollision(DiagonalSplit diagonalSplit, string flatHeightField, string firstFlattenedField, string secondFlattenedField)
    {
        var sector = CreateDiagonalWallSector(diagonalSplit, isFloor: true);
        var shape = CreateRoomSectorShape(sector, floor: true);
        int flatHeight = GetField<int>(shape, flatHeightField);

        Assert.AreEqual(flatHeight, GetField<int>(shape, firstFlattenedField));
        Assert.AreEqual(flatHeight, GetField<int>(shape, secondFlattenedField));
        Assert.AreEqual(0, GetField<int>(shape, "DiagonalStep"));
    }

    [DataTestMethod]
    [DataRow(DiagonalSplit.XnZn, "HeightXpZp", "HeightXnZp", "HeightXpZn")]
    [DataRow(DiagonalSplit.XnZp, "HeightXpZn", "HeightXnZn", "HeightXpZp")]
    [DataRow(DiagonalSplit.XpZn, "HeightXnZp", "HeightXnZn", "HeightXpZp")]
    [DataRow(DiagonalSplit.XpZp, "HeightXnZn", "HeightXnZp", "HeightXpZn")]
    public void RoomSectorShape_FlattensDiagonalWallCeilingCollision(DiagonalSplit diagonalSplit, string flatHeightField, string firstFlattenedField, string secondFlattenedField)
    {
        var sector = CreateDiagonalWallSector(diagonalSplit, isFloor: false);
        var shape = CreateRoomSectorShape(sector, floor: false);
        int flatHeight = GetField<int>(shape, flatHeightField);

        Assert.AreEqual(flatHeight, GetField<int>(shape, firstFlattenedField));
        Assert.AreEqual(flatHeight, GetField<int>(shape, secondFlattenedField));
        Assert.AreEqual(0, GetField<int>(shape, "DiagonalStep"));
    }

    [TestMethod]
    public void RoomSectorShape_LeavesNonWallDiagonalSplitUntouched()
    {
        var sector = new Sector(0, 0)
        {
            Type = SectorType.Floor,
            Floor = new SectorSurface
            {
                DiagonalSplit = DiagonalSplit.XpZn,
                XnZn = 28,
                XnZp = 4,
                XpZn = 16,
                XpZp = 40
            }
        };

        var shape = CreateRoomSectorShape(sector, floor: true);

        Assert.AreNotEqual(0, GetField<int>(shape, "DiagonalStep"));
    }

    private static Sector CreateDiagonalWallSector(DiagonalSplit diagonalSplit, bool isFloor)
    {
        var sector = new Sector(0, 0)
        {
            Type = SectorType.Wall
        };

        var surface = new SectorSurface
        {
            DiagonalSplit = diagonalSplit,
            XnZn = 28,
            XnZp = 4,
            XpZn = 16,
            XpZp = 40
        };

        if (isFloor)
            sector.Floor = surface;
        else
            sector.Ceiling = surface;

        return sector;
    }

    private static object CreateRoomSectorShape(Sector sector, bool floor)
        => Activator.CreateInstance(RoomSectorShapeType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: new object[] { sector, floor, Room.RoomConnectionType.NoPortal, sector.IsAnyWall },
            culture: CultureInfo.InvariantCulture)!;

    private static T GetField<T>(object instance, string fieldName)
        => (T)RoomSectorShapeType
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(instance)!;
}
