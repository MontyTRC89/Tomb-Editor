using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.Util
{
    public struct NgAnimatedTextureInfo
    {
        public AnimatedTextureAnimationType AnimationType;
        public sbyte Fps;
        public sbyte UvRotate;
        public short Delay;
        public List<ushort> ObjectTextureIndices;

        public bool IsUvRotate
        {
            get
            {
                return AnimationType == AnimatedTextureAnimationType.FullRotate ||
                       AnimationType == AnimatedTextureAnimationType.HalfRotate ||
                       AnimationType == AnimatedTextureAnimationType.RiverRotate;
            }
        }
    }

    public class ObjectTextureManagerWithAnimations : ObjectTextureManager
    {
        public const int AnimationLookupGranularityX = 64;
        public const int AnimationLookupGranularityY = 64;
        public static readonly Vector2 AnimationLookupFactor = new Vector2(1.0f / AnimationLookupGranularityX, 1.0f / AnimationLookupGranularityY);

        private struct AnimationLookupEntry
        {
            public AnimatedTextureSet _set;
            public AnimatedTextureFrame _frame;
            public int _frameIndex;
            public uint _textureSpaceIdentifier;
            public Vector2 _texCoord0;
            public Vector2 _texCoord1;
            public Vector2 _texCoord2;
            public Vector2 _texCoord3;
        };
        private readonly Dictionary<Texture, List<AnimationLookupEntry>[,]> _animationSpaceLookup = new Dictionary<Texture, List<AnimationLookupEntry>[,]>();
        private readonly Dictionary<AnimatedTextureFrame, uint> _animationTextureSpaceIdentifierLookup = new Dictionary<AnimatedTextureFrame, uint>();

        private struct AnimationVersion
        {
            public TextureArea _textureArea;
            public AnimatedTextureSet _set;
            public uint _textureSpaceIdentifier;
            public int _frameIndex;
            public bool _isTriangle;
            public bool _isUsedInRoomMesh;
        }
        // Animation expansion is delayed to allow to allow them to use really big object texture indices.
        private readonly Dictionary<Result, AnimationVersion> _delayAddedAnimationVersions = new Dictionary<Result, AnimationVersion>();

        private List<NgAnimatedTextureInfo> _compiledAnimatedTextures = new List<NgAnimatedTextureInfo>();

        private const float _marginFactor = 1.0f / 512.0f;

        public IReadOnlyList<NgAnimatedTextureInfo> CompiledAnimatedTextures { get { return _compiledAnimatedTextures; } }

        public ObjectTextureManagerWithAnimations(IEnumerable<AnimatedTextureSet> animatedTextureSets)
        {
            // Build animated texture set lookup
            foreach (AnimatedTextureSet set in animatedTextureSets)
            {
                if (set.AnimationIsTrivial)
                    continue;

                for (int i = 0; i < set.Frames.Count; ++i)
                {
                    AnimatedTextureFrame frame = set.Frames[i];

                    Vector2 mid = (frame.TexCoord0 + frame.TexCoord1 + frame.TexCoord2 + frame.TexCoord3) * 0.25f;

                    var entry = new AnimationLookupEntry
                    {
                        _set = set,
                        _frame = frame,
                        _frameIndex = i,
                        _textureSpaceIdentifier = GetNewTextureSpaceIdentifier(),
                        _texCoord0 = frame.TexCoord0 + Vector2.Normalize(frame.TexCoord0 - mid) * _marginFactor,
                        _texCoord1 = frame.TexCoord1 + Vector2.Normalize(frame.TexCoord1 - mid) * _marginFactor,
                        _texCoord2 = frame.TexCoord2 + Vector2.Normalize(frame.TexCoord2 - mid) * _marginFactor,
                        _texCoord3 = frame.TexCoord3 + Vector2.Normalize(frame.TexCoord3 - mid) * _marginFactor
                    };
                    _animationTextureSpaceIdentifierLookup.Add(frame, entry._textureSpaceIdentifier);

                    List<AnimationLookupEntry>[,] lookup;
                    if (!_animationSpaceLookup.TryGetValue(frame.Texture, out lookup))
                        _animationSpaceLookup.Add(frame.Texture, lookup = new List<AnimationLookupEntry>[
                            (frame.Texture.Image.Width + (AnimationLookupGranularityX - 1)) / AnimationLookupGranularityX,
                            (frame.Texture.Image.Height + (AnimationLookupGranularityY - 1)) / AnimationLookupGranularityY]);

                    ConservativeRasterizer.RasterizeQuadUniquely(
                        frame.TexCoord0 * AnimationLookupFactor,
                        frame.TexCoord1 * AnimationLookupFactor,
                        frame.TexCoord2 * AnimationLookupFactor,
                        frame.TexCoord3 * AnimationLookupFactor,
                        (startX, startY, endX, endY) =>
                        {
                            for (int y = startY; y < endY; ++y)
                                for (int x = startX; x < endX; ++x)
                                {
                                    List<AnimationLookupEntry> relevantList = lookup[x, y];
                                    if (relevantList == null)
                                        lookup[x, y] = relevantList = new List<AnimationLookupEntry>();
                                    relevantList.Add(entry);
                                }
                        });
                }
            }
        }

        public Result AddTexturePossiblyAnimated(TextureArea texture, bool isTriangle, bool isUsedInRoomMesh, bool supportsUpTo65536 = false, bool canRotate = true)
        {
            // Check if there are any animations on the given texture
            List<AnimationLookupEntry>[,] lookup;
            if (!_animationSpaceLookup.TryGetValue(texture.Texture, out lookup))
                return AddTexture(texture, isTriangle, isUsedInRoomMesh, 0, supportsUpTo65536, canRotate);

            // Check if the texture tile where the texture starts is animated
            Vector2 lookupPos = texture.TexCoord0 * AnimationLookupFactor;
            if (!((lookupPos.X >= 0) && (lookupPos.Y >= 0) && (lookupPos.X < lookup.GetLength(0)) && (lookupPos.Y < lookup.GetLength(1))))
                return AddTexture(texture, isTriangle, isUsedInRoomMesh, 0, supportsUpTo65536, canRotate);
            List<AnimationLookupEntry> relevantList = lookup[(int)lookupPos.X, (int)lookupPos.Y];
            if (relevantList == null || relevantList.Count == 0)
                return AddTexture(texture, isTriangle, isUsedInRoomMesh, 0, supportsUpTo65536, canRotate);

            // Figure out which animation (if any) is enclosing the given area
            foreach (AnimationLookupEntry entry in relevantList)
            {
                if (!PolygonFunctions.DoesQuadContainPoint(entry._texCoord0, entry._texCoord1, entry._texCoord2, entry._texCoord3, texture.TexCoord0))
                    continue;
                if (!PolygonFunctions.DoesQuadContainPoint(entry._texCoord0, entry._texCoord1, entry._texCoord2, entry._texCoord3, texture.TexCoord1))
                    continue;
                if (!PolygonFunctions.DoesQuadContainPoint(entry._texCoord0, entry._texCoord1, entry._texCoord2, entry._texCoord3, texture.TexCoord2))
                    continue;
                if (!PolygonFunctions.DoesQuadContainPoint(entry._texCoord0, entry._texCoord1, entry._texCoord2, entry._texCoord3, texture.TexCoord3))
                    continue;
                if (entry._set.AnimationIsTrivial)
                    continue;

                // Found enclosing animation
                Result result = AddTexture(texture, isTriangle, isUsedInRoomMesh, 0, supportsUpTo65536, canRotate, entry._textureSpaceIdentifier);
                if ((result.Flags & ResultFlags.IsNew) != ResultFlags.None)
                {
                    Result storedResult = result;
                    storedResult.Flags = ResultFlags.None;
                    if (!_delayAddedAnimationVersions.ContainsKey(storedResult))
                        _delayAddedAnimationVersions.Add(storedResult, new AnimationVersion
                        {
                            _textureArea = texture,
                            _textureSpaceIdentifier = entry._textureSpaceIdentifier,
                            _set = entry._set,
                            _frameIndex = entry._frameIndex,
                            _isTriangle = isTriangle,
                            _isUsedInRoomMesh = isUsedInRoomMesh
                        });
                }
                return result;
            }
            return AddTexture(texture, isTriangle, isUsedInRoomMesh, 0, supportsUpTo65536, canRotate);
        }

        protected override void OnPackingTextures(IProgressReporter progressReporter)
        {
            // Complete prevent animated textures
            while (_delayAddedAnimationVersions.Count > 0)
            {
                KeyValuePair<Result, AnimationVersion> animation = _delayAddedAnimationVersions.First();
                _delayAddedAnimationVersions.Remove(animation.Key);
                AnimatedTextureSet set = animation.Value._set;
                AnimatedTextureFrame expansionBaseFrame = set.Frames[animation.Value._frameIndex];

                // The texture space coordinates need to be transformed into animation relative coordinates
                Vector2 texCoord0, texCoord1, texCoord2, texCoord3;
                {
                    // Definition of the problem:
                    //   Texture coordinates of animation frame: A0(a0x, a0y), A1(a1x, a1y), A2(a2x, a2y), A3(a3x, a3y)
                    //   A Texture coordinate of the applied texture: P(px, py)
                    //   A sub texture coordinates relative to frame: S(sx, sy)
                    //
                    // Relationship (Basically linear interpolation of relative frames inside the animation frame):
                    //   px = a0x*((1-sx)*(1-sy)) + a1x*((1-sx)*sy) + a2x*(sx*sy) + a3x*(sx*(1-sy))
                    //   py = a0y*((1-sx)*(1-sy)) + a1y*((1-sx)*sy) + a2y*(sx*sy) + a3y*(sx*(1-sy))

                    // https://stackoverflow.com/questions/808441/inverse-bilinear-interpolation
                    // http://www.iquilezles.org/www/articles/ibilinear/ibilinear.htm
                    // https://www.shadertoy.com/view/lsBSDm
                    Vector2 a0 = expansionBaseFrame.TexCoord0;
                    Vector2 a1 = expansionBaseFrame.TexCoord1;
                    Vector2 a2 = expansionBaseFrame.TexCoord2;
                    Vector2 a3 = expansionBaseFrame.TexCoord3;

                    // Detect of UVs in clockwise order to avoid NaN later on
                    float animationArea =
                        (((a0.X - a1.X) * (a0.Y + a1.Y) + (a1.X - a2.X) * (a1.Y + a2.Y)) +
                        ((a2.X - a3.X) * (a2.Y + a3.Y) + (a3.X - a0.X) * (a3.Y + a0.Y)));
                    bool animationUVsAreClockwise = animationArea > 0.0f;
                    if (animationUVsAreClockwise)
                        Swap.Do(ref a1, ref a3);

                    Vector4 pX = new Vector4(
                        animation.Value._textureArea.TexCoord0.X,
                        animation.Value._textureArea.TexCoord1.X,
                        animation.Value._textureArea.TexCoord2.X,
                        animation.Value._textureArea.TexCoord3.X);
                    Vector4 pY = new Vector4(
                        animation.Value._textureArea.TexCoord0.Y,
                        animation.Value._textureArea.TexCoord1.Y,
                        animation.Value._textureArea.TexCoord2.Y,
                        animation.Value._textureArea.TexCoord3.Y);

                    Vector2 e = a1 - a0;
                    Vector2 f = a3 - a0;
                    Vector2 g = (a0 - a1) + (a2 - a3);

                    float k2 = g.X * f.Y - g.Y * f.X; // 2D cross product
                    float eCrossF = e.X * f.Y - e.Y * f.X; // 2D cross product

                    Vector4 hX = pX - new Vector4(a0.X);
                    Vector4 hY = pY - new Vector4(a0.Y);
                    Vector4 k1 = new Vector4(eCrossF) + (hX * g.Y - hY * g.X); // 2D cross product
                    Vector4 k0 = hX * e.Y - hY * e.X; // 2D cross product

                    // If edges are parallel, this is a linear equation
                    if (Math.Abs(k2) < 0.00001)
                    { // Linear case (needs specialication to avoid division by 0!)
                        Vector4 sx = -k0 / k1;
                        Vector4 sy = (hX - f.X * sx) / (new Vector4(e.X) + g.X * sx);
                        // TODO INVESTIGATE If sy is NaN it seems, we should swap the assignment of a1 and a3 back and also swap x and y of the texture coordinates
                        texCoord0 = new Vector2(sx.X, sy.X);
                        texCoord1 = new Vector2(sx.Y, sy.Y);
                        texCoord2 = new Vector2(sx.Z, sy.Z);
                        texCoord3 = new Vector2(sx.W, sy.W);
                    }
                    else
                    { // Quadratic case
                        Vector4 w = k1 * k1 - 4.0f * k0 * k2;
                        w = Vector4.SquareRoot(w);

                        Vector4 v1 = (-k1 - w) / (k2 + k2);
                        Vector4 v2 = (-k1 + w) / (k2 + k2);
                        Vector4 u1 = (hX - f.X * v1) / (new Vector4(e.X) + g.X * v1);
                        Vector4 u2 = (hX - f.X * v2) / (new Vector4(e.X) + g.X * v2);

                        Vector4 maxOvershoot = Vector4.Min(Vector4.Min(v1, new Vector4(1.0f) - v1), Vector4.Min(u1, new Vector4(1.0f) - u1));
                        texCoord0 = (maxOvershoot.X >= 0) ? new Vector2(v1.X, u1.X) : new Vector2(v2.X, u2.X);
                        texCoord1 = (maxOvershoot.Y >= 0) ? new Vector2(v1.Y, u1.Y) : new Vector2(v2.Y, u2.Y);
                        texCoord2 = (maxOvershoot.Z >= 0) ? new Vector2(v1.Z, u1.Z) : new Vector2(v2.Z, u2.Z);
                        texCoord3 = (maxOvershoot.W >= 0) ? new Vector2(v1.W, u1.W) : new Vector2(v2.W, u2.W);
                    }


                    // Correct texture coordinates
                    if (animationUVsAreClockwise)
                    {
                        Swap.Do(ref texCoord0.X, ref texCoord0.Y);
                        Swap.Do(ref texCoord1.X, ref texCoord1.Y);
                        Swap.Do(ref texCoord2.X, ref texCoord2.Y);
                        Swap.Do(ref texCoord3.X, ref texCoord3.Y);
                    }
                }

                // Create compiled animated texture
                // TODO: remove test values when UI will be ready
                NgAnimatedTextureInfo compiledAnimatedTexture = new NgAnimatedTextureInfo();
                compiledAnimatedTexture.ObjectTextureIndices = new List<ushort>();
                compiledAnimatedTexture.AnimationType = set.AnimationType;
                if (set.AnimationType == AnimatedTextureAnimationType.Frames)
                {
                    if (set.Fps < 0)
                    {
                        compiledAnimatedTexture.Fps = 0;
                        compiledAnimatedTexture.Delay = (byte)-set.Fps;
                    }
                    else
                    {
                        compiledAnimatedTexture.Fps = set.Fps;
                        compiledAnimatedTexture.Delay = 0;
                    }
                }
                else if (set.AnimationType == AnimatedTextureAnimationType.PFrames)
                {
                    compiledAnimatedTexture.Fps = 0;
                    compiledAnimatedTexture.Delay = 0;
                }
                else
                {
                    compiledAnimatedTexture.Fps = set.Fps;
                    compiledAnimatedTexture.UvRotate = set.UvRotate;
                }

                // Expand animation
                for (int i = 0; i < set.Frames.Count; ++i)
                {
                    AnimatedTextureFrame frame = set.Frames[i];
                    TextureArea currentArea = animation.Value._textureArea;

                    ushort objectTextureIndex;
                    if (animation.Value._frameIndex == i)
                    {
                        ApplyTexCoordRotation(ref currentArea, animation.Key.FirstVertexIndexToEmit, animation.Value._isTriangle);
                        objectTextureIndex = animation.Key.ObjectTextureIndex;
                    }
                    else
                    {
                        // Determine the texture coordinates in frame 'i' by bilinear interpolation
                        currentArea.Texture = frame.Texture;
                        currentArea.TexCoord0 =
                            frame.TexCoord0 * ((1 - texCoord0.X) * (1 - texCoord0.Y)) +
                            frame.TexCoord1 * ((1 - texCoord0.X) * texCoord0.Y) +
                            frame.TexCoord2 * (texCoord0.X * texCoord0.Y) +
                            frame.TexCoord3 * (texCoord0.X * (1 - texCoord0.Y));
                        currentArea.TexCoord1 =
                            frame.TexCoord0 * ((1 - texCoord1.X) * (1 - texCoord1.Y)) +
                            frame.TexCoord1 * ((1 - texCoord1.X) * texCoord1.Y) +
                            frame.TexCoord2 * (texCoord1.X * texCoord1.Y) +
                            frame.TexCoord3 * (texCoord1.X * (1 - texCoord1.Y));
                        currentArea.TexCoord2 =
                            frame.TexCoord0 * ((1 - texCoord2.X) * (1 - texCoord2.Y)) +
                            frame.TexCoord1 * ((1 - texCoord2.X) * texCoord2.Y) +
                            frame.TexCoord2 * (texCoord2.X * texCoord2.Y) +
                            frame.TexCoord3 * (texCoord2.X * (1 - texCoord2.Y));
                        currentArea.TexCoord3 =
                            frame.TexCoord0 * ((1 - texCoord3.X) * (1 - texCoord3.Y)) +
                            frame.TexCoord1 * ((1 - texCoord3.X) * texCoord3.Y) +
                            frame.TexCoord2 * (texCoord3.X * texCoord3.Y) +
                            frame.TexCoord3 * (texCoord3.X * (1 - texCoord3.Y));

                        ApplyTexCoordRotation(ref currentArea, animation.Key.FirstVertexIndexToEmit, animation.Value._isTriangle);

                        // Get a suitable object texture
                        var result = AddTexture(currentArea, animation.Value._isTriangle, animation.Value._isUsedInRoomMesh, 0, true, false, _animationTextureSpaceIdentifierLookup[frame]);
                        result.FirstVertexIndexToEmit = animation.Key.FirstVertexIndexToEmit;
                        result.Flags = ResultFlags.None;
                        bool wasRemoved = _delayAddedAnimationVersions.Remove(result); // Remove queued version of this texture if necessary
                        if ((!wasRemoved) && ((result.Flags & ResultFlags.IsNew) == ResultFlags.None)) // Was this texture already used for another animation? If yes create a new one.
                            result = AddTexture(currentArea, animation.Value._isTriangle, animation.Value._isUsedInRoomMesh, 0, true, false, GetNewTextureSpaceIdentifier());

                        objectTextureIndex = result.ObjectTextureIndex;
                    }

                    compiledAnimatedTexture.ObjectTextureIndices.Add(objectTextureIndex);

                    // Add repeats of the frame
                    for (int j = 1; j < frame.Repeat; ++j)
                    {
                        var result = AddTexture(currentArea, animation.Value._isTriangle, animation.Value._isUsedInRoomMesh, 0, true, false, GetNewTextureSpaceIdentifier());
                        compiledAnimatedTexture.ObjectTextureIndices.Add(result.ObjectTextureIndex);
                    }
                }

                _compiledAnimatedTextures.Add(compiledAnimatedTexture);
            }

            // Sort sets for keeping UVRotate ranges first (stable sort)
            _compiledAnimatedTextures = _compiledAnimatedTextures.OrderBy(x => x.UvRotate == 0).ToList();

            // Continue
            base.OnPackingTextures(progressReporter);
        }

        private static void ApplyTexCoordRotation(ref TextureArea currentArea, int firstVertexIndexToEmit, bool isTriangle)
        {
            if (isTriangle)
            {
                for (int j = 0; j < firstVertexIndexToEmit; ++j)
                {
                    Vector2 temp = currentArea.TexCoord0;
                    currentArea.TexCoord0 = currentArea.TexCoord1;
                    currentArea.TexCoord1 = currentArea.TexCoord2;
                    currentArea.TexCoord2 = temp;
                }
                currentArea.TexCoord3 = currentArea.TexCoord2;
            }
            else
                for (int j = 0; j < firstVertexIndexToEmit; ++j)
                {
                    Vector2 temp = currentArea.TexCoord0;
                    currentArea.TexCoord0 = currentArea.TexCoord1;
                    currentArea.TexCoord1 = currentArea.TexCoord2;
                    currentArea.TexCoord2 = currentArea.TexCoord3;
                    currentArea.TexCoord3 = temp;
                }
        }

        public int NgUvRotateCount
        {
            get
            {
                var num = 0;
                foreach (var set in _compiledAnimatedTextures)
                    if (set.AnimationType == AnimatedTextureAnimationType.FullRotate ||
                        set.AnimationType == AnimatedTextureAnimationType.HalfRotate ||
                        set.AnimationType == AnimatedTextureAnimationType.RiverRotate)
                        num++;
                return (int)num;
            }
        }

        public void WriteAnimatedTexturesForTr4(BinaryWriterEx stream)
        {
            int numAnimatedTexture = 1;
            foreach (var compiledAnimatedTexture in _compiledAnimatedTextures)
                numAnimatedTexture += compiledAnimatedTexture.ObjectTextureIndices.Count + 1;
            stream.Write((uint)numAnimatedTexture);

            stream.Write((ushort)_compiledAnimatedTextures.Count);
            foreach (var compiledAnimatedTexture in _compiledAnimatedTextures)
            {
                stream.Write((ushort)(compiledAnimatedTexture.ObjectTextureIndices.Count - 1));
                foreach (var objectTextureIndex in compiledAnimatedTexture.ObjectTextureIndices)
                    stream.Write((ushort)objectTextureIndex);
            }
        }
    }
}
