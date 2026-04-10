#nullable enable

using System.Numerics;
using TombLib;

namespace TombEditor.Controls.FlybyTimeline.Preview;

/// <summary>
/// Stores the camera state for a single flyby frame. Used both as the internal cache
/// element in <see cref="Sequence.FlybySequenceCache"/> and as the public preview output.
/// </summary>
public struct FlybyFrameState
{
    public Vector3 Position { get; set; }
    public float RotationY { get; set; }
    public float RotationX { get; set; }
    public float Roll { get; set; }
    public float Fov { get; set; }

    /// <summary>
    /// Builds a frame from degree-based flyby camera angles, applying the engine sign convention.
    /// </summary>
    public static FlybyFrameState FromDegrees(Vector3 worldPosition, float rotationY, float rotationX, float roll, float fov) => new()
    {
        Position = worldPosition,
        RotationY = MathC.DegToRad(rotationY),
        RotationX = -MathC.DegToRad(rotationX),
        Roll = MathC.DegToRad(roll),
        Fov = MathC.DegToRad(fov)
    };
}
