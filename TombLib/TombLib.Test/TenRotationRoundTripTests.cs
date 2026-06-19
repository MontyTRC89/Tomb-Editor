using System;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.Test;

/// <summary>
/// Guards the quaternion → Euler reversal used by <c>TenWad2Converter</c> when rebuilding animation
/// keyframes from a compiled .ten level. The compiler stores bone orientations as quaternions produced by
/// <see cref="WadKeyFrameRotation.Quaternion"/> (CreateFromYawPitchRoll(Y, -X, -Z)); the converter must
/// recover Euler angles that reproduce that same quaternion. Euler is ambiguous, so we compare quaternions.
/// </summary>
[TestClass]
public class TenRotationRoundTripTests
{
    // Mirror of TenWad2Converter.ReverseRotation (private). If that formula changes, this must change too.
    private static WadKeyFrameRotation ReverseRotation(Quaternion q)
    {
        var e = MathC.QuaternionToEuler(q);
        const float radToDeg = 180.0f / (float)Math.PI;
        return new WadKeyFrameRotation { Rotations = new Vector3(-e.X, e.Y, -e.Z) * radToDeg };
    }

    private static bool QuaternionsMatch(Quaternion a, Quaternion b, float tolerance)
    {
        // q and -q represent the same rotation, so accept either sign alignment.
        float dot = Math.Abs(a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W);
        return dot >= 1.0f - tolerance;
    }

    [TestMethod]
    public void ReverseRotation_ReproducesOriginalQuaternion_AcrossAngleGrid()
    {
        // Step well off the gimbal-lock poles (pitch = ±90) which are intentionally degenerate.
        for (int x = -160; x <= 160; x += 40)
            for (int y = -160; y <= 160; y += 40)
                for (int z = -160; z <= 160; z += 40)
                {
                    var original = new WadKeyFrameRotation { Rotations = new Vector3(x, y, z) };
                    var quaternion = original.Quaternion;

                    var recovered = ReverseRotation(quaternion);

                    Assert.IsTrue(QuaternionsMatch(quaternion, recovered.Quaternion, 1e-4f),
                        $"Mismatch at ({x}, {y}, {z}): recovered {recovered.Rotations} did not reproduce the quaternion.");
                }
    }
}
