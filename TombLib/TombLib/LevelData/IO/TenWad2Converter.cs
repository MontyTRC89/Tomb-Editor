using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Utils;
using TombLib.Wad;

namespace TombLib.LevelData.IO
{
    /// <summary>
    /// Rebuilds Wad2 objects (<see cref="WadMesh"/>, <see cref="WadStatic"/>, ...) from the intermediate
    /// model parsed out of a compiled <c>.ten</c> level. This is the inverse of
    /// <c>LevelCompilerTombEngine.ConvertWadMesh</c>: vertex Y is re-negated back into editor space,
    /// per-polygon UVs (normalized to the atlas page) are scaled into pixel coordinates against the
    /// decoded atlas image, and shine/lighting are un-scaled to their Wad2 ranges.
    ///
    /// SLICE 6: meshes + static meshes. Moveables (skeleton from the mesh-tree blob + animations) are
    /// layered on in a later slice.
    /// </summary>
    public sealed class TenWad2Converter
    {
        private readonly TenObjectData _objects;
        private readonly TenMediaData _media;

        // One WadTexture per decoded atlas page, shared across every polygon that maps into it. Keyed by
        // the owning atlas list (reference identity) plus the page index, so moveable/static pages that
        // happen to share an index don't collide.
        private readonly Dictionary<(object Atlas, int Page), WadTexture> _atlasTextures = new();

        public TenWad2Converter(TenObjectData objects, TenMediaData media)
        {
            _objects = objects ?? throw new ArgumentNullException(nameof(objects));
            _media = media ?? throw new ArgumentNullException(nameof(media));
        }

        /// <summary>
        /// Converts every static mesh in the parsed level into a <see cref="WadStatic"/>. The owning Wad2
        /// is passed so the conversion can stamp the engine version onto catalog lookups.
        /// </summary>
        public void ConvertStatics(Wad2 wad)
        {
            foreach (var src in _objects.Statics)
            {
                var converted = ConvertStatic(src);
                wad.Statics.Add(converted.Id, converted);
            }
        }

        public WadStatic ConvertStatic(TenStatic src)
        {
            var dest = new WadStatic(new WadStaticId(checked((uint)src.ObjectID)));

            // The compiler stored the boxes with Y negated (Y1 = -Min.Y, Y2 = -Max.Y); re-negate to recover
            // editor space, exactly as the TR-level importer does.
            dest.VisibilityBox = MakeBox(src.VisibilityBoxMin, src.VisibilityBoxMax);
            dest.CollisionBox = MakeBox(src.CollisionBoxMin, src.CollisionBoxMax);

            dest.Flags = unchecked((short)src.Flags);
            dest.Shatter = src.ShatterType != 0; // ShatterType.None == 0
            dest.ShatterSoundID = src.ShatterSound;

            dest.Mesh = ConvertMesh(_objects.Meshes[src.Mesh], _media.StaticsAtlas, "Static_" + src.ObjectID);
            return dest;
        }

        private static BoundingBox MakeBox(Vector3 min, Vector3 max) =>
            new BoundingBox(new Vector3(min.X, -min.Y, min.Z), new Vector3(max.X, -max.Y, max.Z));

        // ---- Moveables --------------------------------------------------------------------------

        public void ConvertMoveables(Wad2 wad)
        {
            foreach (var src in _objects.Moveables)
            {
                var converted = ConvertMoveable(src);
                wad.Moveables.Add(converted.Id, converted);
            }
        }

        public WadMoveable ConvertMoveable(TenMoveable src)
        {
            var dest = new WadMoveable(new WadMoveableId(checked((uint)src.ObjectID)));
            string name = "Moveable_" + src.ObjectID;

            // Meshes are stored sequentially; the compiler set StartingMesh to the running mesh count just
            // before emitting this moveable's meshes, so it is a direct index into the shared mesh list.
            var meshes = new List<WadMesh>(src.NumMeshes);
            for (int i = 0; i < src.NumMeshes; i++)
                meshes.Add(ConvertMesh(_objects.Meshes[src.StartingMesh + i], _media.MoveablesAtlas, name + "_Mesh_" + i));

            // Root bone always exists and carries no link; remaining bones are decoded from the mesh-tree
            // blob (4 ints each: opcode + X/Y/Z, with Y negated by the compiler).
            var root = new WadBone
            {
                Name = "bone_0_root",
                Parent = null,
                Translation = Vector3.Zero,
                Mesh = meshes.Count > 0 ? meshes[0] : null
            };
            dest.Bones.Add(root);

            for (int b = 1; b < src.NumMeshes; b++)
            {
                int baseOffset = src.MeshTree + (b - 1) * 4;
                var bone = new WadBone
                {
                    Name = "bone_" + b,
                    Parent = null,
                    Mesh = meshes[b],
                    OpCode = (WadLinkOpcode)_objects.MeshTrees[baseOffset + 0],
                    Translation = new Vector3(
                        _objects.MeshTrees[baseOffset + 1],
                        -_objects.MeshTrees[baseOffset + 2],
                        _objects.MeshTrees[baseOffset + 3])
                };
                dest.Bones.Add(bone);
            }

            if (src.Skin >= 0 && src.Skin < _objects.Meshes.Count)
                dest.Skin = ConvertMesh(_objects.Meshes[src.Skin], _media.MoveablesAtlas, name + "_Skin");

            foreach (var anim in src.Animations)
                dest.Animations.Add(ConvertAnimation(anim));

            return dest;
        }

        private WadAnimation ConvertAnimation(TenAnimation src)
        {
            var anim = new WadAnimation
            {
                // Legacy levels retain the authored frame rate (with original keyframes); current levels bake
                // one interpolated frame per engine frame, which the reader reports as a rate of 1.
                FrameRate = (byte)Math.Clamp(src.FrameRate, 1, byte.MaxValue),
                StateId = (ushort)src.StateID,
                EndFrame = (ushort)src.FrameEnd,
                NextAnimation = (ushort)src.NextAnimation,
                NextFrame = (ushort)src.NextFrame,
                BlendFrameCount = (ushort)src.BlendFrameCount,
                Name = "Animation",
                // VelocityStart/End were packed as (lateral, 0, forward) by the compiler.
                StartLateralVelocity = src.VelocityStart.X,
                StartVelocity = src.VelocityStart.Z,
                EndLateralVelocity = src.VelocityEnd.X,
                EndVelocity = src.VelocityEnd.Z,
                RootMotion = new WadAnimRootMotionSettings { Flags = (WadAnimRootMotionFlags)src.RootMotionFlags }
            };

            foreach (var frame in src.InterpolatedFrames)
                anim.KeyFrames.Add(ConvertKeyFrame(frame));

            foreach (var cmd in src.Commands)
                anim.AnimCommands.Add(ConvertAnimCommand(cmd));

            // The .ten stream flattens state changes to one record per dispatch (each tagged with its state
            // id); regroup consecutive records that share a state id back into a single WadStateChange.
            foreach (var sc in src.StateChanges)
            {
                var dispatch = new WadAnimDispatch
                {
                    InFrame = (ushort)sc.FrameLow,
                    OutFrame = (ushort)sc.FrameHigh,
                    NextAnimation = (ushort)sc.NextAnimation,
                    NextLowFrame = (ushort)sc.NextLowFrame,
                    NextHighFrame = (ushort)sc.NextHighFrame,
                    BlendFrames = (ushort)sc.BlendFrames
                };

                var last = anim.StateChanges.Count > 0 ? anim.StateChanges[anim.StateChanges.Count - 1] : null;
                if (last != null && last.StateId == (ushort)sc.StateID)
                    last.Dispatches.Add(dispatch);
                else
                {
                    var stateChange = new WadStateChange { StateId = (ushort)sc.StateID };
                    stateChange.Dispatches.Add(dispatch);
                    anim.StateChanges.Add(stateChange);
                }
            }

            return anim;
        }

        private static WadKeyFrame ConvertKeyFrame(TenKeyFrame src)
        {
            // Recover engine-space box corners from center/extents, then re-negate Y into editor space.
            var min = src.BoundingBoxCenter - src.BoundingBoxExtents;
            var max = src.BoundingBoxCenter + src.BoundingBoxExtents;

            var frame = new WadKeyFrame
            {
                BoundingBox = new BoundingBox(
                    new Vector3(min.X, -min.Y, min.Z),
                    new Vector3(max.X, -max.Y, max.Z)),
                Offset = new Vector3(src.RootOffset.X, -src.RootOffset.Y, src.RootOffset.Z)
            };

            foreach (var q in src.BoneOrientations)
                frame.Angles.Add(ReverseRotation(q));

            return frame;
        }

        private static WadAnimCommand ConvertAnimCommand(TenAnimCommand src)
        {
            var cmd = new WadAnimCommand { Type = (WadAnimCommandType)src.Type };
            switch (cmd.Type)
            {
                case WadAnimCommandType.SetPosition: // Vector = (X, Y, Z)
                    cmd.Parameter1 = (short)src.Vector.X;
                    cmd.Parameter2 = (short)src.Vector.Y;
                    cmd.Parameter3 = (short)src.Vector.Z;
                    break;
                case WadAnimCommandType.SetJumpDistance: // compiler packed (0, H, V)
                    cmd.Parameter1 = (short)src.Vector.Y;
                    cmd.Parameter2 = (short)src.Vector.Z;
                    break;
                case WadAnimCommandType.PlaySound: // Ints = [SoundID, Frame, Environment]
                    cmd.Parameter1 = (short)src.Ints[1];
                    cmd.Parameter2 = (short)src.Ints[0];
                    cmd.Parameter3 = (short)src.Ints[2];
                    break;
                case WadAnimCommandType.FlipEffect: // Ints = [FlipEffectID, Frame]
                    cmd.Parameter1 = (short)src.Ints[1];
                    cmd.Parameter2 = (short)src.Ints[0];
                    break;
                case WadAnimCommandType.DisableInterpolation: // Ints = [Frame]
                    cmd.Parameter1 = (short)src.Ints[0];
                    break;
            }
            return cmd;
        }

        // Inverse of WadKeyFrameRotation.Quaternion, which is CreateFromYawPitchRoll(Y, -X, -Z). QuaternionToEuler
        // inverts CreateFromYawPitchRoll(Y, X, Z) (returns pitch=X, yaw=Y, roll=Z in radians), so the pitch and
        // roll signs are flipped back here. Euler is ambiguous, but the reconstructed angles reproduce the
        // original quaternion (verified by round-trip test).
        private static WadKeyFrameRotation ReverseRotation(Quaternion q)
        {
            var e = MathC.QuaternionToEuler(q);
            const float radToDeg = 180.0f / (float)Math.PI;
            return new WadKeyFrameRotation { Rotations = new Vector3(-e.X, e.Y, -e.Z) * radToDeg };
        }

        /// <summary>
        /// Rebuilds a single <see cref="WadMesh"/> from a parsed <see cref="TenMesh"/>. <paramref name="atlas"/>
        /// is the destination-specific atlas page list (moveables vs statics) the mesh's polygons index into.
        /// </summary>
        public WadMesh ConvertMesh(TenMesh src, List<ImageC> atlas, string name)
        {
            var mesh = new WadMesh
            {
                Name = name,
                Hidden = src.Hidden,
                LightingType = (WadMeshLightingType)src.LightingType
            };

            for (int i = 0; i < src.Vertices.Count; i++)
            {
                var v = src.Vertices[i];
                mesh.VertexPositions.Add(new Vector3(v.Position.X, -v.Position.Y, v.Position.Z));
                mesh.VertexColors.Add(v.Color);
                mesh.VertexAttributes.Add(new VertexAttributes
                {
                    // Compiler wrote Glow/Move as value / 64; round-trip back to the integer Wad2 range.
                    Glow = (int)Math.Round(v.Glow * 64.0f),
                    Move = (int)Math.Round(v.Move * 64.0f)
                });
            }

            // Per-vertex normals are not stored per-vertex in the .ten stream; they are duplicated on each
            // polygon corner. Gather them back onto the shared vertex list (the compiler emitted identical
            // normals for every corner referencing the same vertex).
            var normals = new Vector3[src.Vertices.Count];
            var hasNormal = new bool[src.Vertices.Count];

            foreach (var poly in src.Polygons)
            {
                int n = poly.VertexCount;
                for (int i = 0; i < n; i++)
                {
                    int idx = poly.Indices[i];
                    if (idx >= 0 && idx < normals.Length && !hasNormal[idx])
                    {
                        var pn = poly.Normals[i];
                        normals[idx] = new Vector3(pn.X, -pn.Y, pn.Z);
                        hasNormal[idx] = true;
                    }
                }

                mesh.Polys.Add(ConvertPolygon(poly, atlas));
            }

            bool allNormals = src.Vertices.Count > 0;
            for (int i = 0; i < hasNormal.Length; i++)
                if (!hasNormal[i]) { allNormals = false; break; }

            if (allNormals)
                mesh.VertexNormals.AddRange(normals);

            mesh.BoundingSphere = new BoundingSphere(
                new Vector3(src.SphereCenter.X, -src.SphereCenter.Y, src.SphereCenter.Z),
                Math.Abs(src.SphereRadius));
            mesh.BoundingBox = mesh.CalculateBoundingBox();

            if (!mesh.HasNormals)
                mesh.CalculateNormals();

            return mesh;
        }

        private WadPolygon ConvertPolygon(TenPolygon src, List<ImageC> atlas)
        {
            var poly = new WadPolygon
            {
                Shape = src.IsTriangle ? WadPolygonShape.Triangle : WadPolygonShape.Quad,
                Index0 = src.Indices[0],
                Index1 = src.Indices[1],
                Index2 = src.Indices[2],
                Index3 = src.IsTriangle ? 0 : src.Indices[3],
                ShineStrength = (byte)Math.Clamp((int)Math.Round(src.ShineStrength * 63.0f), 0, 63),
                Texture = MakeTextureArea(src, atlas)
            };
            return poly;
        }

        private TextureArea MakeTextureArea(TenPolygon src, List<ImageC> atlas)
        {
            if (atlas == null || src.TextureAtlas < 0 || src.TextureAtlas >= atlas.Count)
                return WadMesh.EmptyTextureArea;

            var texture = GetAtlasTexture(atlas, src.TextureAtlas);
            var size = new Vector2(texture.Image.Width, texture.Image.Height);

            // .ten UVs are normalized against the atlas page; Wad2 TextureArea coords are in pixels.
            var area = new TextureArea
            {
                Texture = texture,
                BlendMode = (BlendMode)src.BlendMode,
                DoubleSided = false,
                TexCoord0 = src.TexCoords[0] * size,
                TexCoord1 = src.TexCoords[1] * size,
                TexCoord2 = src.TexCoords[2] * size,
                TexCoord3 = src.IsTriangle ? src.TexCoords[2] * size : src.TexCoords[3] * size
            };
            return area;
        }

        private WadTexture GetAtlasTexture(List<ImageC> atlas, int page)
        {
            var key = (atlas, page);
            if (!_atlasTextures.TryGetValue(key, out var texture))
            {
                var image = atlas[page];
                image.FileName = string.Empty;
                texture = new WadTexture(image);
                _atlasTextures[key] = texture;
            }
            return texture;
        }
    }
}
