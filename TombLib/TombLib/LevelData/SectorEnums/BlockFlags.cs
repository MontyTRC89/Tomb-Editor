using System;

namespace TombLib.LevelData.SectorEnums;

[Flags]
public enum BlockFlags : short
{
	None = 0,
	Monkey = 1,
	Box = 2,
	DeathFire = 4,
	DeathLava = 8,
	DeathElectricity = 16,
	Beetle = 32,
	TriggerTriggerer = 64,
	NotWalkableFloor = 128,
	ClimbPositiveZ = 256,
	ClimbNegativeZ = 512,
	ClimbPositiveX = 1024,
	ClimbNegativeX = 2048,
	ClimbAny = ClimbPositiveZ | ClimbNegativeZ | ClimbPositiveX | ClimbNegativeX
}
