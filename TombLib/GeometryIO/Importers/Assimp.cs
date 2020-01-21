using Assimp;
using Assimp.Configs;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.GeometryIO.Importers
{
    public class AssimpImporter : BaseGeometryImporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public AssimpImporter(IOGeometrySettings settings, GetTextureDelegate getTextureCallback)
            : base(settings, getTextureCallback)
        {

        }

        private List<string> CollectMeshNodeNames(Node node, List<string> list = null)
        {
            if (list == null)
                list = new List<string>();

            if (node.HasMeshes && !list.Any(entry => entry.Equals(node.Name)))
                list.Add(node.Name);

            if (node.HasChildren && node.ChildCount > 0)
                foreach (var child in node.Children)
                    CollectMeshNodeNames(child, list);

            return list;
        }

        public override IOModel ImportFromFile(string filename)
        {
            string path = Path.GetDirectoryName(filename);

            // Use Assimp.NET for importing model
            AssimpContext context = new AssimpContext();
            context.SetConfig(new NormalSmoothingAngleConfig(90.0f));
            Scene scene = context.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality);

            var newModel = new IOModel();
            var textures = new Dictionary<int, Texture>();

            if (_settings.ImportGeometry)
            {
                var tmpList = new List<IOMesh>();

                // Create the list of materials to load
                for (int i = 0; i < scene.Materials.Count; i++)
                {
                    var mat = scene.Materials[i];
                    var material = new IOMaterial(mat.HasName ? mat.Name : "Material_" + i);

                    var diffusePath = mat.HasTextureDiffuse ? mat.TextureDiffuse.FilePath : null;
                    if (string.IsNullOrWhiteSpace(diffusePath))
                        continue;

                    // Don't add materials with missing textures
                    var texture = GetTexture(path, diffusePath);
                    if (texture == null)
                    {
                        logger.Warn("Texture for material " + mat.Name + " is missing. Meshes referencing this material won't be imported.");
                        continue;
                    }
                    else
                        textures.Add(i, texture);

                    // Create the new material
                    material.Texture = textures[i];
                    material.AdditiveBlending = mat.HasBlendMode && mat.BlendMode == global::Assimp.BlendMode.Additive;
                    material.DoubleSided = mat.HasTwoSided && mat.IsTwoSided;
                    material.Shininess = mat.HasShininess ? (int)mat.Shininess : 0;
                    newModel.Materials.Add(material);
                }

                var lastBaseVertex = 0;

                // Loop for each mesh loaded in scene
                foreach (var mesh in scene.Meshes)
                {
                    // Discard nullmeshes
                    if (!mesh.HasFaces || !mesh.HasVertices || mesh.VertexCount < 3 ||
                         mesh.TextureCoordinateChannelCount == 0 || !mesh.HasTextureCoords(0))
                    {
                        logger.Warn("Mesh \"" + (mesh.Name ?? "") + "\" has no faces, no texture coordinates or wrong vertex count.");
                        continue;
                    }

                    // Import only textured meshes with valid materials
                    Texture faceTexture;
                    if (!textures.TryGetValue(mesh.MaterialIndex, out faceTexture))
                    {
                        logger.Warn("Mesh \"" + (mesh.Name ?? "") + "\" does have material index " + mesh.MaterialIndex + " which is unsupported or can't be found.");
                        continue;
                    }

                    // Make sure we have appropriate material in list. If not, skip mesh and warn user.
                    var material = newModel.Materials.FirstOrDefault(mat => mat.Name.Equals(scene.Materials[mesh.MaterialIndex].Name));
                    if (material == null)
                    {
                        logger.Warn("Can't find material with specified index (" + mesh.MaterialIndex + "). Probably you're missing textures or using non-diffuse materials only for this mesh.");
                        continue;
                    }

                    // Assimp's mesh is our IOSubmesh so we import meshes with just one submesh
                    var newMesh = new IOMesh(mesh.Name);
                    var newSubmesh = new IOSubmesh(material);
                    newMesh.Submeshes.Add(material, newSubmesh);

                    bool hasColors  = mesh.VertexColorChannelCount > 0 && mesh.HasVertexColors(0);
                    bool hasNormals = mesh.HasNormals;

                    // Additional integrity checks
                    if ((mesh.VertexCount != mesh.TextureCoordinateChannels[0].Count) ||
                        (hasColors  && mesh.VertexCount != mesh.VertexColorChannels[0].Count) ||
                        (hasNormals && mesh.VertexCount != mesh.Normals.Count))
                    {
                        logger.Warn("Mesh \"" + (mesh.Name ?? "") + "\" data structure is inconsistent.");
                        continue;
                    }

                    // Source data
                    var positions = mesh.Vertices;
                    var normals = mesh.Normals;
                    var texCoords = mesh.TextureCoordinateChannels[0];
                    var colors = mesh.VertexColorChannels[0];

                    for (int i = 0; i < mesh.VertexCount; i++)
                    {
                        // Create position
                        var position = new Vector3(positions[i].X, positions[i].Y, positions[i].Z);
                        position = ApplyAxesTransforms(position);
                        newMesh.Positions.Add(position);

                        // Create normal
                        if (hasNormals)
                        {
                            var normal = new Vector3(normals[i].X, normals[i].Y, normals[i].Z);
                            normal = ApplyAxesTransforms(normal);
                            newMesh.Normals.Add(normal);
                        }
                        else
                            newMesh.CalculateNormals();

                        // Create UV
                        var currentUV = new Vector2(texCoords[i].X, texCoords[i].Y);
                        if (faceTexture != null)
                            currentUV = ApplyUVTransform(currentUV, faceTexture.Image.Width, faceTexture.Image.Height);
                        newMesh.UV.Add(currentUV);

                        // Create colors
                        if (hasColors)
                        {
                            var color = new Vector4(colors[i].R, colors[i].G, colors[i].B, colors[i].A);
                            newMesh.Colors.Add(color);
                        }
                    }

                    // Add polygons
                    foreach (var face in mesh.Faces)
                    {
                        if (face.IndexCount == 3)
                        {
                            var poly = new IOPolygon(IOPolygonShape.Triangle);

                            poly.Indices.Add(lastBaseVertex + face.Indices[0]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[1]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[2]);

                            if (_settings.InvertFaces)
                                poly.Indices.Reverse();

                            newSubmesh.Polygons.Add(poly);
                        }
                        else if (face.IndexCount == 4)
                        {
                            var poly = new IOPolygon(IOPolygonShape.Quad);

                            poly.Indices.Add(lastBaseVertex + face.Indices[0]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[1]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[2]);
                            poly.Indices.Add(lastBaseVertex + face.Indices[3]);

                            if (_settings.InvertFaces)
                                poly.Indices.Reverse();

                            newSubmesh.Polygons.Add(poly);
                        }
                    }

                    tmpList.Add(newMesh);
                }

                // Sort meshes by name, if specified
                if (_settings.SortByName)
                    tmpList = tmpList.OrderBy(m => m.Name, new CustomComparer<string>(NaturalComparer.Do)).ToList();

                foreach (var mesh in tmpList)
                    newModel.Meshes.Add(mesh);
            }

            if (_settings.ImportAnimations && 
                scene.HasAnimations && scene.AnimationCount > 0)
            {
                // Find all mesh nodes to count against animation nodes
                var meshNameList = CollectMeshNodeNames(scene.RootNode);

                // Sort animations by name, if specified
                if (_settings.SortByName)
                    meshNameList = meshNameList.OrderBy(s => s, new CustomComparer<string>(NaturalComparer.Do)).ToList();

                // Loop through all animations and add appropriate ones.
                // Integrity check: there should be meshes and mesh count should be equal to unique mesh name count.
                if (scene.MeshCount <= 0 || scene.MeshCount != meshNameList.Count)
                    logger.Warn("Actual number of meshes doesn't correspond to mesh list. Animations won't be imported.");
                else
                {
                    for (int i = 0; i < scene.AnimationCount; i++)
                    {
                        var anim = scene.Animations[i];

                        // Integrity check: support only time-based node animations
                        if (!anim.HasNodeAnimations || anim.DurationInTicks <= 0)
                        {
                            logger.Warn("Anim " + i + " isn't a valid type of animation for TR formats.");
                            continue;
                        }

                        // Guess possible maximum frame and time
                        var frameCount = 0;
                        double maximumTime = 0;
                        foreach (var node in anim.NodeAnimationChannels)
                        {
                            if (node.HasPositionKeys)
                            {
                                var maxNodeTime = node.PositionKeys.Max(key => key.Time);
                                maximumTime = maximumTime >= maxNodeTime ? maximumTime : maxNodeTime;
                                frameCount = frameCount >= node.PositionKeyCount ? frameCount : node.PositionKeyCount;
                            }
                            if (node.HasRotationKeys)
                            {
                                var maxNodeTime = node.RotationKeys.Max(key => key.Time);
                                maximumTime = maximumTime >= maxNodeTime ? maximumTime : maxNodeTime;
                                frameCount = frameCount >= node.RotationKeyCount ? frameCount : node.RotationKeyCount;
                            }
                        }

                        // Calculate time multiplier
                        var timeMult = (double)(frameCount - 1) / anim.DurationInTicks;

                        // Integrity check: maximum frame time shouldn't excess duration
                        if (timeMult * maximumTime >= frameCount)
                        {
                            logger.Warn("Anim " + i + " has frames outside of time limits and won't be imported.");
                            continue;
                        }

                        IOAnimation ioAnim = new IOAnimation(string.IsNullOrEmpty(anim.Name) ? "Imported animation " + i : anim.Name,
                                                             scene.MeshCount);

                        // Precreate frames and set them to identity
                        for (int j = 0; j < frameCount; j++)
                            ioAnim.Frames.Add(new IOFrame());

                        // Precreate rotations and set them to identity
                        // I am using generic foreach here instead of linq foreach because for some reason it
                        // returns wrong amount of angles during enumeration with Enumerable.Repeat.
                        foreach (var frame in ioAnim.Frames)
                        {
                            var angleList = Enumerable.Repeat(Vector3.Zero, scene.MeshCount);
                            frame.Angles.AddRange(angleList);
                        }

                        // Search through all nodes and put data into corresponding frames.
                        // It's not clear what should we do in case if multiple nodes refer to same mesh, but sometimes
                        // it happens, e. g. in case of fbx format. In this case, we'll just add to existing values for now.

                        foreach (var chan in anim.NodeAnimationChannels)
                        {
                            // Look if this channel belongs to any mesh in list. 
                            // If so, attribute it to appropriate frame.
                            var chanIndex = meshNameList.IndexOf(item => chan.NodeName.Contains(item));

                            // Integrity check: no appropriate mesh found
                            if (chanIndex < 0)
                            {
                                logger.Warn("Anim " + i + " channel " + chan.NodeName + " has no corresponding mesh in meshtree and will be ignored");
                                continue;
                            }

                            // Apply translation only if found channel belongs to root mesh.
                            if (chanIndex == 0 && chan.HasPositionKeys && chan.PositionKeyCount > 0)
                                foreach (var key in chan.PositionKeys)
                                {
                                    // Integrity check: frame shouldn't fall out of keyframe array bounds.
                                    var frameIndex = (int)Math.Round(key.Time * timeMult, MidpointRounding.AwayFromZero);
                                    if (frameIndex >= frameCount)
                                    {
                                        logger.Warn("Anim " + i + " channel " + chan.NodeName + " has a key outside of time limits and will be ignored.");
                                        continue;
                                    }

                                    float rX = key.Value.X;
                                    float rY = key.Value.Y;
                                    float rZ = key.Value.Z;

                                    if (_settings.SwapAnimTranslationXY) { var temp = rX; rX = rY; rY = temp; }
                                    if (_settings.SwapAnimTranslationXZ) { var temp = rX; rX = rZ; rZ = temp; }
                                    if (_settings.SwapAnimTranslationYZ) { var temp = rY; rY = rZ; rZ = temp; }

                                    ioAnim.Frames[frameIndex].Offset += new Vector3(rX, rY, rZ);
                                }

                            if (chan.HasRotationKeys && chan.RotationKeyCount > 0)
                                foreach (var key in chan.RotationKeys)
                                {
                                    // Integrity check: frame shouldn't fall out of keyframe array bounds.
                                    var frameIndex = (int)Math.Round(key.Time * timeMult, MidpointRounding.AwayFromZero);
                                    if (frameIndex >= frameCount)
                                    {
                                        logger.Warn("Anim " + i + " channel " + chan.NodeName + " has a key outside of time limits and will be ignored.");
                                        continue;
                                    }

                                    // Convert quaternions back to rotations.
                                    // This is similar to TRViewer's conversion routine.

                                    var quatI = System.Numerics.Quaternion.Identity;
                                    var quat = new System.Numerics.Quaternion(key.Value.X,
                                                                              key.Value.Z,
                                                                              key.Value.Y,
                                                                             -key.Value.W);
                                    quatI *= quat;

                                    var eulers = MathC.QuaternionToEuler(quatI);
                                    var rotation = new Vector3(eulers.X * 180.0f / (float)Math.PI,
                                                               eulers.Y * 180.0f / (float)Math.PI,
                                                               eulers.Z * 180.0f / (float)Math.PI);

                                    ioAnim.Frames[frameIndex].Angles[chanIndex] += MathC.NormalizeAngle(rotation);
                                }
                        }

                        newModel.Animations.Add(ioAnim);
                    }
                }
            }

            return newModel;
        }
    }
}
