using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF;

public readonly struct ValueResult<TValue, TError>
{
	public readonly TValue? _value;
	public readonly TError? _error;

    public ValueResult(TValue value)
    {
		IsError = false;
		_value = value;
		_error = default;
    }

	public ValueResult(TError error)
	{
		IsError = true;
		_error = error;
		_value = default;
	}

	public bool IsError { get; }
	public bool IsSuccess => !IsError;

	public static implicit operator ValueResult<TValue, TError>(TValue value) => new(value);
	public static implicit operator ValueResult<TValue, TError>(TError error) => new(error);

	public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<TError, TResult> onError)
		=> IsSuccess ? onSuccess(_value!) : onError(_error!);
}

public readonly struct EditorResult
{
	public bool HasMessage { get; }
	public EditorMessage Message { get; }

	public EditorResult() => HasMessage = false;

	public EditorResult(EditorMessage message)
	{
		HasMessage = true;
		Message = message;
	}
}

public readonly struct EditorMessage(string message, PopupType popupType)
{
	public string Message { get; } = message;
	public PopupType PopupType { get; } = popupType;
}

public enum PopupType
{
	Info,
	Warning,
	Error
}

public enum BlockSurface
{
	Floor,
	Ceiling
}

public interface ILevel
{

}

public interface IBlock
{

}

public interface IRoomGeometry
{

}

public class RoomProperties : ICloneable
{
	public Vector3 AmbientLight { get; set; } = new Vector3(0.25f, 0.25f, 0.25f); // Normalized float. (1.0 meaning normal brightness, 2.0 is the maximal brightness supported by tomb4.exe)
	public RoomType Type { get; set; } = RoomType.Normal;
	public byte TypeStrength { get; set; } = 0; // Internal legacy value for TRNG rain/snow strengths
	public RoomLightEffect LightEffect { get; set; } = RoomLightEffect.Default;
	public byte LightEffectStrength { get; set; } = 1;
	public RoomLightInterpolationMode LightInterpolationMode { get; set; } = RoomLightInterpolationMode.Default;

	public bool DamagesPlayer { get; set; }
	public bool IsCold { get; set; }
	public bool IsHorizonVisible { get; set; }
	public bool IsWindy { get; set; }
	public bool IsExcludedFromPathFinding { get; set; }
	public bool IsLensFlareDisabled { get; set; }
	public byte Reverberation { get; set; }
	public bool IsLocked { get; set; }
	public bool IsHidden { get; set; }

	public List<string> Tags { get; set; } = [];

	object ICloneable.Clone() => Clone();

	public RoomProperties Clone()
	{
		var result = (RoomProperties)MemberwiseClone();
		result.Tags = new List<string>(Tags);
		return result;
	}
}

public interface IRoom
{
	ILevel Level { get; }

	string Name { get; set; }
	RoomProperties Properties { get; set; }
	VectorInt3 Position { get; set; }
	IBlock[,] Blocks { get; }

	IRoom AlternateBaseRoom { get; set; }
	IRoom AlternateRoom { get; set; }
	short AlternateGroup { get; set; }

	public IRoomGeometry RoomGeometry { get; set; }

	public IReadOnlyList<IRoom> AndAdjoiningRooms { get; }

	public bool ExistsInLevel { get; }
	public bool Alternated { get; }
	public bool IsAlternate { get; }
	public IRoom AlternateOpposite { get; }
	public VectorInt2 SectorSize { get; }
	public RectangleInt2 WorldArea { get; }
	public BoundingBox WorldBoundingBox { get; }

	public IEnumerable<IRoom> Versions { get; }

	public VectorInt2 SectorPosition { get; set; }
	public RectangleInt2 LocalArea { get; }

	public IEnumerable<PortalInstance> Portals { get; }

	public IEnumerable<ObjectInstance> Objects { get; }
	public IReadOnlyList<PositionBasedObjectInstance> PositionBasedObjects { get; }

	public IEnumerable<TriggerInstance> Triggers { get; }
	public IEnumerable<GhostBlockInstance> GhostBlocks { get; }
	public IEnumerable<SectorBasedObjectInstance> SectorObjects { get; }

	/// <summary>
	/// Turns the selected sectors to the specified surface (e.g. Floor or Ceiling).
	/// </summary>
	EditorResult SetSurfaces(RectangleInt2 area, BlockSurface surface);

	/// <summary>
	/// Turns the selected sectors into walls.
	/// </summary>
	EditorResult SetWalls(RectangleInt2 area);

	/// <summary>
	/// Turns the selected sectors into diagonal walls.
	/// </summary>
	EditorResult SetDiagonalWalls(RectangleInt2 area);

	/// <summary>
	/// Turns the selected sectors into diagonal steps for the given surface.
	/// </summary>
	EditorResult SetDiagonalSteps(RectangleInt2 area, BlockSurface surface);

	/// <summary>
	/// Toggles the selected sectors' flags (e.g. Death Tile, Monkey Bars, Ladders etc.).
	/// </summary>
	EditorResult ToggleFlags(RectangleInt2 area, BlockFlags flags);

	/// <summary>
	/// Toggles a flag for selected sectors, which forces them to act solid, no matter whether there is a portal or not.
	/// </summary>
	EditorResult ToggleForceSolidFloor(RectangleInt2 area);

	/// <summary>
	/// Adds Ghost Blocks to the selected sectors.
	/// </summary>
	EditorResult AddGhostBlocks(RectangleInt2 area);

	/// <summary>
	/// Adds a portal to the selected sectors.
	/// </summary>
	EditorResult AddPortal(RectangleInt2 area);

	/// <summary>
	/// Resets the geometry of the selected sectors.
	/// </summary>
	EditorResult ResetGeometry(RectangleInt2 area);

	/// <summary>
	/// Flattens the selected sectors.
	/// </summary>
	EditorResult Flatten(RectangleInt2 area, BlockSurface surface);

	/// <summary>
	/// Flattens the selected sectors to the average height of each individual sector.
	/// </summary>
	EditorResult Average(RectangleInt2 area, BlockVertical vertical, int increments);

	/// <summary>
	/// Generates a sharp noise on the selected area.
	/// </summary>
	EditorResult SharpRandom(RectangleInt2 area, BlockVertical vertical, float strengthDirection);

	/// <summary>
	/// Generates a smooth noise on the selected area.
	/// </summary>
	EditorResult SmoothRandom(RectangleInt2 area, BlockVertical vertical, float strengthDirection);

	/// <summary>
	/// Grids the selected sectors into equal parts (or as equal as possible).
	/// </summary>
	EditorResult GridWalls(RectangleInt2 area, int divisionCount, bool squares);
}
