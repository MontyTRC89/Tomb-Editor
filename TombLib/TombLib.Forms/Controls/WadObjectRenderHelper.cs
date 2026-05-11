using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombLib.Controls
{
    // Migrated to the unified rendering path (RenderingDrawingMesh / RenderingDrawingImportedGeometry).
    // Per-mesh GPU resources are cached in static dictionaries keyed by the legacy
    // ObjectMesh / ImportedGeometryMesh references (rebuilt automatically when the
    // legacy mesh is invalidated by WadRenderer).
    public static class WadObjectRenderHelper
    {
        // Static caches — entries live until process exit (the legacy meshes themselves
        // are owned by WadRenderer instances and don't currently expose disposal hooks
        // we could subscribe to). Memory cost: one extra GPU buffer per legacy mesh.
        private static readonly Dictionary<ObjectMesh, RenderingDrawingMesh> _meshCache = new Dictionary<ObjectMesh, RenderingDrawingMesh>();
        private static readonly Dictionary<TombLib.LevelData.ImportedGeometryMesh, RenderingDrawingImportedGeometry> _importedCache = new Dictionary<TombLib.LevelData.ImportedGeometryMesh, RenderingDrawingImportedGeometry>();

        /// <summary>
        /// Applies optional skin substitute for moveables that need it.
        /// If the object is a WadMoveable and has a skin defined in TrCatalog,
        /// replaces dummy meshes with the skin's meshes.
        /// Returns the original object unchanged for non-moveables or when no skin is found.
        /// </summary>
        public static IWadObject GetRenderObject(IWadObject wadObject, LevelSettings settings)
        {
            if (wadObject is WadMoveable moveable)
            {
                var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(settings.GameVersion, moveable.Id.TypeId));
                var skin = settings.WadTryGetMoveable(skinId);

                if (skin != null && skin != moveable)
                    return moveable.ReplaceDummyMeshes(skin);
            }
            return wadObject;
        }

        /// <summary>
        /// Computes a bounding sphere for the given wad object, suitable for camera framing.
        /// </summary>
        public static BoundingSphere ComputeBoundingSphere(IWadObject wadObject, WadRenderer wadRenderer)
        {
            var bs = new BoundingSphere(new Vector3(0.0f, 256.0f, 0.0f), 640.0f);

            if (wadObject is WadMoveable moveable)
            {
                if (moveable.Meshes.Count == 0 || (moveable.Meshes.Count == 1 && moveable.Meshes[0] == null))
                    return bs;

                var model = wadRenderer.GetMoveable(moveable);
                if (model.Animations.Count > 0 && model.Animations[0].KeyFrames.Count > 0)
                {
                    model.UpdateAnimation(0, 0);
                    var bb = model.Animations[0].KeyFrames[0].CalculateBoundingBox(model, model);
                    bs = BoundingSphere.FromBoundingBox(bb);
                }
            }
            else if (wadObject is WadStatic staticObj)
            {
                if (staticObj.Mesh != null)
                    bs = staticObj.Mesh.CalculateBoundingSphere();
            }
            else if (wadObject is ImportedGeometry impGeo)
            {
                if (impGeo.DirectXModel != null && impGeo.DirectXModel.Meshes != null)
                {
                    var bb = new BoundingBox();
                    foreach (var mesh in impGeo.DirectXModel.Meshes)
                        bb = bb.Union(mesh.BoundingBox);
                    bs = BoundingSphere.FromBoundingBox(bb);
                }
            }

            return bs;
        }

        /// <summary>
        /// Creates a camera positioned to frame the given WAD object.
        /// Returns null if the object type is unsupported or has no renderable content.
        /// </summary>
        public static ArcBallCamera CreateCameraForObject(IWadObject wadObject, WadRenderer wadRenderer, float fieldOfView)
        {
            if (wadObject is WadMoveable moveable)
            {
                if (moveable.Meshes.Count == 0 || (moveable.Meshes.Count == 1 && moveable.Meshes[0] == null))
                    return null;
            }
            else if (wadObject is WadStatic staticObj)
            {
                if (staticObj.Mesh == null || staticObj.Mesh.VertexPositions.Count == 0)
                    return null;
            }
            else if (!(wadObject is WadStatic) && !(wadObject is ImportedGeometry))
            {
                return null;
            }

            var bs = ComputeBoundingSphere(wadObject, wadRenderer);
            var center = bs.Center;
            var radius = bs.Radius * 1.15f;

            return new ArcBallCamera(center, MathC.DegToRad(35), MathC.DegToRad(35),
                -(float)Math.PI / 2, (float)Math.PI / 2, radius * 3, 50, 1000000, fieldOfView * (float)(Math.PI / 180));
        }

        // Caller passes a stateBuffer with TransformMatrix already set to viewProjection.
        public static void RenderObject(IWadObject wadObject, WadRenderer wadRenderer,
            RenderingDevice device, RenderingSwapChain swapChain, RenderingStateBuffer stateBuffer,
            Vector3 cameraPosition, bool drawTransparency)
        {
            if (wadObject is WadMoveable moveable)
                RenderMoveable(moveable, wadRenderer, device, swapChain, stateBuffer, cameraPosition, drawTransparency);
            else if (wadObject is WadStatic staticObj)
                RenderStatic(staticObj, wadRenderer, device, swapChain, stateBuffer, cameraPosition, drawTransparency);
            else if (wadObject is ImportedGeometry impGeo)
                RenderImportedGeometry(impGeo, device, swapChain, stateBuffer, cameraPosition, drawTransparency);
        }

        public static void RenderMoveable(WadMoveable moveable, WadRenderer wadRenderer,
            RenderingDevice device, RenderingSwapChain swapChain, RenderingStateBuffer stateBuffer,
            Vector3 cameraPosition, bool drawTransparency)
        {
            if (moveable.Meshes.Count == 0 || (moveable.Meshes.Count == 1 && moveable.Meshes[0] == null))
                return;

            var model = wadRenderer.GetMoveable(moveable);
            model.UpdateAnimation(0, 0);

            // Per-bone world matrix for non-skinned per-mesh draws.
            var matrices = new List<Matrix4x4>();
            if (model.Animations.Count != 0)
                for (var b = 0; b < model.Meshes.Count; b++)
                    matrices.Add(model.AnimationTransforms[b]);
            else
                foreach (var bone in model.Bones)
                    matrices.Add(bone.GlobalTransform);

            // GPU-skinned skin pass (replaces the legacy AnimatedModel.RenderSkin).
            if (model.Skin != null)
            {
                var skinDraw = GetOrCreateMesh(device, model.Skin);
                int boneCount = model.AnimationTransforms.Count;
                var bones = new Matrix4x4[boneCount];
                for (int b = 0; b < boneCount; ++b)
                {
                    if (Matrix4x4.Invert(model.BindPoseTransforms[b], out var invBindPose))
                        bones[b] = invBindPose * model.AnimationTransforms[b];
                    else
                        bones[b] = Matrix4x4.Identity;
                }
                skinDraw.Render(new RenderingDrawingMesh.RenderArgs
                {
                    RenderTarget = swapChain,
                    StateBuffer = stateBuffer,
                    Atlas = wadRenderer.Texture,
                    World = Matrix4x4.Identity,
                    Tint = Vector4.One,
                    StaticLighting = false,
                    ColoredVertices = false,
                    AlphaTest = drawTransparency,
                    Skinned = true,
                    BoneMatrices = bones,
                });
            }

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                if (mesh.Vertices.Count == 0)
                    continue;
                if (model.Skin != null && mesh.Hidden)
                    continue;

                var drawMesh = GetOrCreateMesh(device, mesh);
                drawMesh.Render(new RenderingDrawingMesh.RenderArgs
                {
                    RenderTarget = swapChain,
                    StateBuffer = stateBuffer,
                    Atlas = wadRenderer.Texture,
                    World = matrices[i],
                    Tint = Vector4.One,
                    StaticLighting = false,
                    ColoredVertices = false,
                    AlphaTest = drawTransparency,
                });
            }
        }

        public static void RenderStatic(WadStatic staticObj, WadRenderer wadRenderer,
            RenderingDevice device, RenderingSwapChain swapChain, RenderingStateBuffer stateBuffer,
            Vector3 cameraPosition, bool drawTransparency)
        {
            var model = wadRenderer.GetStatic(staticObj);

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                if (mesh.Vertices.Count == 0)
                    continue;

                var drawMesh = GetOrCreateMesh(device, mesh);
                drawMesh.Render(new RenderingDrawingMesh.RenderArgs
                {
                    RenderTarget = swapChain,
                    StateBuffer = stateBuffer,
                    Atlas = wadRenderer.Texture,
                    World = Matrix4x4.Identity,
                    Tint = Vector4.One,
                    StaticLighting = false,
                    ColoredVertices = false,
                    AlphaTest = drawTransparency,
                });
            }
        }

        public static void RenderImportedGeometry(ImportedGeometry geo,
            RenderingDevice device, RenderingSwapChain swapChain, RenderingStateBuffer stateBuffer,
            Vector3 cameraPosition, bool drawTransparency)
        {
            var model = geo.DirectXModel;
            if (model == null || model.Meshes == null || model.Meshes.Count == 0)
                return;

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var legacyMesh = model.Meshes[i];
                if (legacyMesh.Vertices.Count == 0)
                    continue;
                var drawMesh = GetOrCreateImported(device, legacyMesh);
                drawMesh.Render(new RenderingDrawingImportedGeometry.RenderArgs
                {
                    RenderTarget = swapChain,
                    StateBuffer = stateBuffer,
                    World = Matrix4x4.Identity,
                    Tint = Vector4.One,
                    UseVertexColors = true,
                    AlphaTest = drawTransparency,
                });
            }
        }

        private static RenderingDrawingMesh GetOrCreateMesh(RenderingDevice device, ObjectMesh legacyMesh)
        {
            if (_meshCache.TryGetValue(legacyMesh, out var cached))
                return cached;

            var verts = new MeshVertex[legacyMesh.Vertices.Count];
            for (int i = 0; i < legacyMesh.Vertices.Count; ++i)
            {
                var s = legacyMesh.Vertices[i];
                verts[i] = new MeshVertex
                {
                    Position = s.Position,
                    UVW = s.UVW,
                    Normal = s.Normal,
                    Color = s.Color,
                    BoneIndex = s.Indices,
                    BoneWeight = s.Weights,
                };
            }
            var subList = new List<RenderingDrawingMesh.Submesh>(legacyMesh.Submeshes.Count);
            foreach (var kv in legacyMesh.Submeshes)
            {
                if (kv.Value.NumIndices == 0) continue;
                subList.Add(new RenderingDrawingMesh.Submesh
                {
                    IndexStart = kv.Value.BaseIndex,
                    IndexCount = kv.Value.NumIndices,
                    DoubleSided = kv.Key.DoubleSided,
                    AdditiveBlending = kv.Key.AdditiveBlending,
                });
            }
            var mesh = device.CreateDrawingMesh(new RenderingDrawingMesh.Description
            {
                Vertices = verts,
                Indices = legacyMesh.Indices,
                Submeshes = subList,
            });
            _meshCache[legacyMesh] = mesh;
            return mesh;
        }

        private static RenderingDrawingImportedGeometry GetOrCreateImported(RenderingDevice device, TombLib.LevelData.ImportedGeometryMesh legacyMesh)
        {
            if (_importedCache.TryGetValue(legacyMesh, out var cached))
                return cached;

            var verts = new RenderingDrawingImportedGeometry.Vertex[legacyMesh.Vertices.Count];
            for (int i = 0; i < legacyMesh.Vertices.Count; ++i)
            {
                var s = legacyMesh.Vertices[i];
                verts[i] = new RenderingDrawingImportedGeometry.Vertex
                {
                    Position = s.Position,
                    UV = s.UV,
                    Color = s.Color,
                    Normal = s.Normal,
                };
            }
            var subList = new List<RenderingDrawingImportedGeometry.Submesh>(legacyMesh.Submeshes.Count);
            foreach (var kv in legacyMesh.Submeshes)
            {
                if (kv.Value.NumIndices == 0) continue;
                var matTexture = kv.Value.Material.Texture;
                object texObj = null;
                Vector2 texSize = Vector2.Zero;
                if (matTexture is TombLib.LevelData.ImportedGeometryTexture igt)
                {
                    texObj = igt.DirectXTexture;
                    texSize = new Vector2(matTexture.Image.Width, matTexture.Image.Height);
                }
                subList.Add(new RenderingDrawingImportedGeometry.Submesh
                {
                    IndexStart = kv.Value.BaseIndex,
                    IndexCount = kv.Value.NumIndices,
                    DoubleSided = kv.Key.DoubleSided,
                    AdditiveBlending = kv.Key.AdditiveBlending,
                    Texture = texObj,
                    TextureSize = texSize,
                });
            }
            var mesh = device.CreateDrawingImportedGeometry(new RenderingDrawingImportedGeometry.Description
            {
                Vertices = verts,
                Indices = legacyMesh.Indices,
                Submeshes = subList,
            });
            _importedCache[legacyMesh] = mesh;
            return mesh;
        }
    }
}
