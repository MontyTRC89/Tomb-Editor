using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.Wad
{
    public static class WadOperations
    {
        public static Dictionary<int, WadTexture> ConvertTr4TexturesToWadTexture(TR4Wad oldWad)
        {
            var textures = new Dictionary<int, WadTexture>();

            int i = 0;
            foreach (var oldTexture in oldWad.Textures)
            {
                var texture = new WadTexture();

                short startX = (short)(oldTexture.X);
                short startY = (short)(oldTexture.Page * 256 + oldTexture.Y);

                // Create the texture ImageC
                var textureData = ImageC.CreateNew(oldTexture.Width + 1, oldTexture.Height + 1);

                for (int y = 0; y < textureData.Height; y++)
                {
                    for (int x = 0; x < textureData.Width; x++)
                    {
                        byte r = oldWad.TexturePages[startY + y, startX * 3 + 3 * x + 0];
                        byte g = oldWad.TexturePages[startY + y, startX * 3 + 3 * x + 1];
                        byte b = oldWad.TexturePages[startY + y, startX * 3 + 3 * x + 2];
                        byte a = 255;

                        var color = new ColorC(r, g, b, a);
                        textureData.SetPixel(x, y, color);
                    }
                }

                // Replace magenta color with alpha transparent black
                textureData.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

                texture.Image = textureData;

                // Update the hash of the texture
                texture.UpdateHash();

                textures.Add(i, texture);

                i++;
            }

            return textures;
        }

        private static int GetTr4TextureIdFromPolygon(wad_polygon polygon)
        {
            int textureId = polygon.Texture & 0xfff;
            if (textureId > 2047)
                textureId = -(textureId - 4096);
            return textureId;
        }

        public static WadMesh ConvertTr4MeshToWadMesh(Wad2 wad, TR4Wad oldWad, wad_mesh oldMesh,
                                                      Dictionary<int, WadTexture> convertedTextures)
        {
            WadMesh mesh = new WadMesh();

            int xMin = Int32.MaxValue;
            int yMin = Int32.MaxValue;
            int zMin = Int32.MaxValue;
            int xMax = Int32.MinValue;
            int yMax = Int32.MinValue;
            int zMax = Int32.MinValue;

            // Create the bounding sphere
            mesh.BoundingSphere = new BoundingSphere(new Vector3(oldMesh.SphereX, oldMesh.SphereY, oldMesh.SphereZ), 
                                                     oldMesh.Radius);

            // Add positions
            foreach (var oldVertex in oldMesh.Vertices)
            {
                mesh.VerticesPositions.Add(new Vector3(oldVertex.X, -oldVertex.Y, oldVertex.Z));

                if (oldVertex.X < xMin)
                    xMin = oldVertex.X;
                if (-oldVertex.Y < yMin)
                    yMin = -oldVertex.Y;
                if (oldVertex.Z < zMin)
                    zMin = oldVertex.Z;

                if (oldVertex.X > xMax)
                    xMax = oldVertex.X;
                if (-oldVertex.Y > yMax)
                    yMax = -oldVertex.Y;
                if (oldVertex.Z > zMax)
                    zMax = oldVertex.Z;
            }

            Vector3 minVertex = new Vector3(xMin, yMin, zMin);
            Vector3 maxVertex = new Vector3(xMax, yMax, zMax);

            // Add normals
            foreach (var oldNormal in oldMesh.Normals)
            {
                mesh.VerticesNormals.Add(new Vector3(oldNormal.X, -oldNormal.Y, oldNormal.Z));
            }

            // Add shades
            foreach (var oldShade in oldMesh.Shades)
            {
                mesh.VerticesShades.Add(oldShade);
            }

            // Add polygons
            foreach (var oldPoly in oldMesh.Polygons)
            {
                WadPolygon poly = new WadPolygon(oldPoly.Shape == 8 ? WadPolygonShape.Triangle : WadPolygonShape.Rectangle);

                // Polygon indices
                poly.Indices.Add(oldPoly.V1);
                poly.Indices.Add(oldPoly.V2);
                poly.Indices.Add(oldPoly.V3);
                if (poly.Shape == WadPolygonShape.Rectangle) poly.Indices.Add(oldPoly.V4);

                // Polygon special effects
                poly.ShineStrength = (byte)((oldPoly.Attributes & 0x7c) >> 2);
                poly.Transparent = (oldPoly.Attributes & 0x01) == 0x01;

                // Add the texture
                int textureId = GetTr4TextureIdFromPolygon(oldPoly);
                WadTexture newTexture = convertedTextures[textureId];

                if (wad.Textures.ContainsKey(newTexture.Hash))
                {
                    poly.Texture = wad.Textures[newTexture.Hash];
                }
                else
                {
                    wad.Textures.Add(newTexture.Hash, newTexture);
                    poly.Texture = newTexture;
                }

                // Calculate UV coordinates for this polygon
                List<Vector2> uv = CalculateTr4UVCoordinates(oldWad, oldPoly);

                // Add the UV coordinates
                poly.UV.Add(uv[0]);
                poly.UV.Add(uv[1]);
                poly.UV.Add(uv[2]);
                if (poly.Shape == WadPolygonShape.Rectangle) poly.UV.Add(uv[3]);

                mesh.Polys.Add(poly);
            }

            mesh.BoundingBox = new BoundingBox(minVertex, maxVertex);

            // Calculate hash
            mesh.UpdateHash();

            // Now add to the dictionary only if it doesn't contain a mesh with this hash
            if (wad.Meshes.ContainsKey(mesh.Hash))
            {
                return wad.Meshes[mesh.Hash];
            }
            else
            {
                wad.Meshes.Add(mesh.Hash, mesh);
                return mesh;
            }
        }

        public static Wad2 ConvertTr4Wad(TR4Wad oldWad)
        {
            Wad2 wad = new Wad2();

            // First convert all textures
            Dictionary<int, WadTexture> textures = ConvertTr4TexturesToWadTexture(oldWad);
            for (int i = 0; i < textures.Count; i++)
            {
                if (!wad.Textures.ContainsKey(textures.ElementAt(i).Value.Hash))
                    wad.Textures.Add(textures.ElementAt(i).Value.Hash, textures.ElementAt(i).Value);
            }

            // Then convert moveables and static meshes
            // Meshes will be converted inside each model
            for (int i = 0; i < oldWad.Moveables.Count; i++)
            {
                ConvertTr4MoveableToWadMoveable(wad, oldWad, i, textures);
            }

            for (int i = 0; i < oldWad.StaticMeshes.Count; i++)
            {
                ConvertTr4StaticMeshToWadStatic(wad, oldWad, i, textures);
            }

            // Convert sounds
            ConvertTr4Sounds(wad, oldWad);

            return wad;
        }

        private static void ConvertTr4Sounds(Wad2 wad, TR4Wad oldWad)
        {
            for (int i = 0; i < 370; i++)
            {
                // Check if sound was used
                if (oldWad.SoundMap[i] == -1) continue;

                var oldInfo = oldWad.SoundInfo[oldWad.SoundMap[i]];
                var newInfo = new WadSoundInfo();
                
                // Fill the new sound info
                newInfo.Volume = oldInfo.Volume;
                newInfo.Range = oldInfo.Range;
                newInfo.Chance = oldInfo.Chance;
                newInfo.Pitch = oldInfo.Pitch;
                newInfo.RandomizePitch = ((oldInfo.Characteristics & 0x2000) != 0);
                newInfo.RandomizeGain = ((oldInfo.Characteristics & 0x4000) != 0);
                newInfo.FlagN = ((oldInfo.Characteristics & 0x1000) != 0);
                newInfo.Loop = (byte)(oldInfo.Characteristics & 0x03);

                int numSamplesInGroup = (oldInfo.Characteristics & 0x00fc) >> 2;

                // Read all samples linked to this sound info (for example footstep has 4 samples)
                for (int j = oldInfo.Sample; j < oldInfo.Sample + numSamplesInGroup; j++)
                {
                    // TODO: use the configured path in editor
                    string fileName = "Sounds\\Samples\\" + oldWad.Sounds[j];

                    // If wave sound exists, then load it in memory
                    if (File.Exists(fileName))
                    {
                        using (var reader = new BinaryReader(File.OpenRead(fileName)))
                        {
                            var sound = new WadSound(reader.ReadBytes((int)reader.BaseStream.Length));
                            newInfo.WaveSounds.Add(sound);
                        }
                    }
                }

                wad.SoundInfo.Add((ushort)i, newInfo);
            }
        }

        public static WadMoveable ConvertTr4MoveableToWadMoveable(Wad2 wad, TR4Wad oldWad, int moveableIndex,
                                                                  Dictionary<int, WadTexture> textures)
        {
            WadMoveable moveable = new WadMoveable();
            wad_moveable m = oldWad.Moveables[moveableIndex];

            moveable.ObjectID = m.ObjectID;

            // First I build a list of meshes for this moveable
            List<wad_mesh> meshes = new List<wad_mesh>();
            for (int j = 0; j < m.NumPointers; j++)
                meshes.Add(oldWad.Meshes[(int)oldWad.RealPointers[(int)(m.PointerIndex + j)]]);

            // Then I convert them to WadMesh
            foreach (var oldMesh in meshes)
            {
                WadMesh newMesh = ConvertTr4MeshToWadMesh(wad, oldWad, oldMesh, textures);
                moveable.Meshes.Add(newMesh);
            }

            int currentLink = (int)m.LinksIndex;

            moveable.Offset = Vector3.Zero;

            // Build the skeleton
            for (int j = 0; j < meshes.Count - 1; j++)
            {
                WadLink link = new WadLink((WadLinkOpcode)oldWad.Links[currentLink], 
                                           new Vector3(oldWad.Links[currentLink + 1],
                                                       -oldWad.Links[currentLink + 2],
                                                       oldWad.Links[currentLink + 3]));

                currentLink += 4;

                moveable.Links.Add(link);
            }

            // Convert animations
            int numAnimations = 0;
            int nextMoveable = oldWad.GetNextMoveableWithAnimations(moveableIndex);

            if (nextMoveable == -1)
                numAnimations = oldWad.Animations.Count - m.AnimationIndex;
            else
                numAnimations = oldWad.Moveables[nextMoveable].AnimationIndex - m.AnimationIndex;

            for (int j = 0; j < numAnimations; j++)
            {
                if (m.AnimationIndex == -1)
                    break;

                WadAnimation animation = new WadAnimation();
                wad_animation anim = oldWad.Animations[j + m.AnimationIndex];
                animation.Acceleration = anim.Accel;
                animation.Speed = anim.Speed;
                animation.LateralSpeed = anim.SpeedLateral;
                animation.LateralAcceleration = anim.AccelLateral;
                animation.FrameDuration = anim.FrameDuration;
                animation.FrameStart = anim.FrameStart;
                animation.FrameEnd = anim.FrameEnd;
                animation.NextAnimation = (ushort)(anim.NextAnimation - m.AnimationIndex);
                animation.NextFrame = anim.NextFrame;
                animation.StateId = anim.StateId;
                animation.RealNumberOfFrames = (ushort)(anim.FrameEnd - anim.FrameStart + 1);

                for (int k = 0; k < anim.NumStateChanges; k++)
                {
                    WadStateChange sc = new WadStateChange();
                    wad_state_change wadSc = oldWad.Changes[(int)anim.ChangesIndex + k];
                    sc.StateId = (ushort)wadSc.StateId;
                    sc.Dispatches = new List<WadAnimDispatch>();

                    for (int n = 0; n < wadSc.NumDispatches; n++)
                    {
                        WadAnimDispatch ad = new WadAnimDispatch();
                        wad_anim_dispatch wadAd = oldWad.Dispatches[(int)wadSc.DispatchesIndex + n];

                        ad.InFrame = (ushort)(wadAd.Low - anim.FrameStart);
                        ad.OutFrame = (ushort)(wadAd.High - anim.FrameStart);
                        ad.NextAnimation = (ushort)((wadAd.NextAnimation - m.AnimationIndex) % numAnimations);
                        ad.NextFrame = (ushort)wadAd.NextFrame; 

                        sc.Dispatches.Add(ad);
                    }

                    animation.StateChanges.Add(sc);
                }

                animation.AnimCommands = new List<WadAnimCommand>();
                if (anim.NumCommands < oldWad.Commands.Count)
                {
                    int lastCommand = anim.CommandOffset;

                    for (int k = 0; k < anim.NumCommands; k++)
                    {
                        short commandType = oldWad.Commands[lastCommand + 0];

                        // Ignore invalid anim commands (see for example karnak.wad)
                        if (commandType < 1 || commandType > 6) continue;

                        WadAnimCommand command = new WadAnimCommand((WadAnimCommandType)commandType);

                        switch (commandType)
                        {
                            case 1:
                                command.Parameter1 = (ushort)oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (ushort)oldWad.Commands[lastCommand + 2];
                                command.Parameter3 = (ushort)oldWad.Commands[lastCommand + 3];

                                lastCommand += 4;
                                break;

                            case 2:
                                command.Parameter1 = (ushort)oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (ushort)oldWad.Commands[lastCommand + 2];

                                lastCommand += 3;
                                break;

                            case 3:
                                lastCommand += 1;
                                break;

                            case 4:
                                lastCommand += 1;
                                break;

                            case 5:
                                command.Parameter1 = (ushort)oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (ushort)oldWad.Commands[lastCommand + 2];
                                lastCommand += 3;
                                break;

                            case 6:
                                command.Parameter1 = (ushort)oldWad.Commands[lastCommand + 1];
                                command.Parameter2 = (ushort)oldWad.Commands[lastCommand + 2];
                                lastCommand += 3;
                                break;
                        }

                        animation.AnimCommands.Add(command);
                    }
                }

                int frames = (int)anim.KeyFrameOffset / 2;
                uint numFrames;

                if (j + m.AnimationIndex == oldWad.Animations.Count - 1)
                {
                    numFrames = ((uint)(2 * oldWad.KeyFrames.Count) - anim.KeyFrameOffset) / (uint)(2 * anim.KeyFrameSize);
                }
                else
                {
                    if (anim.KeyFrameSize == 0)
                    {
                        numFrames = 0;
                    }
                    else
                    {
                        numFrames = (oldWad.Animations[m.AnimationIndex + j + 1].KeyFrameOffset - anim.KeyFrameOffset) / (uint)(2 * anim.KeyFrameSize);
                    }
                }

                for (int f = 0; f < numFrames; f++)
                {
                    WadKeyFrame frame = new WadKeyFrame();
                    int startOfFrame = frames;

                    frame.BoundingBox = new BoundingBox(new Vector3(oldWad.KeyFrames[frames],
                                                                    -oldWad.KeyFrames[frames + 2],
                                                                    oldWad.KeyFrames[frames + 4]),
                                                        new Vector3(oldWad.KeyFrames[frames + 1],
                                                                    -oldWad.KeyFrames[frames + 3],
                                                                    oldWad.KeyFrames[frames + 5]));

                    frames += 6;

                    frame.Offset = new Vector3(oldWad.KeyFrames[frames],
                                               (short)(-oldWad.KeyFrames[frames + 1]),
                                               oldWad.KeyFrames[frames + 2]);

                    frames += 3;

                    frame.Angles = new WadKeyFrameRotation[m.NumPointers];

                    for (int n = 0; n < frame.Angles.Length; n++)
                    {
                        short rot = oldWad.KeyFrames[frames];
                        WadKeyFrameRotation kfAngle = new WadKeyFrameRotation();

                        switch (rot & 0xc000)
                        {
                            case 0:
                                int rotation = rot;
                                int rotation2 = oldWad.KeyFrames[frames + 1];

                                frames += 2;

                                int rotX = (int)((rotation & 0x3ff0) >> 4);
                                int rotY = (int)(((rotation2 & 0xfc00) >> 10) + ((rotation & 0xf) << 6) & 0x3ff);
                                int rotZ = (int)((rotation2) & 0x3ff);

                                kfAngle.Axis = WadKeyFrameRotationAxis.ThreeAxes;
                                kfAngle.X = rotX;
                                kfAngle.Y = rotY;
                                kfAngle.Z = rotZ;

                                break;

                            case 0x4000:
                                frames += 1;
                                int rotationX = rot & 0x3fff;

                                kfAngle.Axis = WadKeyFrameRotationAxis.AxisX;
                                kfAngle.X = rotationX;

                                break;

                            case 0x8000:
                                frames += 1;
                                int rotationY = rot & 0x3fff;

                                kfAngle.Axis = WadKeyFrameRotationAxis.AxisY;
                                kfAngle.Y = rotationY;

                                break;

                            case 0xc000:
                                int rotationZ = rot & 0x3fff;
                                frames += 1;

                                kfAngle.Axis = WadKeyFrameRotationAxis.AxisZ;
                                kfAngle.Z = rotationZ;

                                break;
                        }

                        frame.Angles[n] = kfAngle;
                    }

                    if ((frames - startOfFrame) < anim.KeyFrameSize)
                        frames += ((int)anim.KeyFrameSize - (frames - startOfFrame));

                    animation.KeyFrames.Add(frame);
                }

                // TODO: check if this hack work well
                // In original WADs animations with no keyframes had some random FrameEnd values
                if (animation.KeyFrames.Count == 0)
                {
                    animation.FrameEnd = anim.FrameStart;
                }

                animation.FrameBase = animation.FrameStart;

                moveable.Animations.Add(animation);
            }

            for (int i = 0; i < moveable.Animations.Count; i++)
            {
                var animation = moveable.Animations[i];

                if (animation.KeyFrames.Count == 0) animation.RealNumberOfFrames = 0;

                // HACK: this fixes some invalid NextAnimations values
                animation.NextAnimation %= (ushort)moveable.Animations.Count;

                ushort baseFrame = animation.FrameStart;

                // Frames become relative to current animation
                animation.FrameEnd -= baseFrame;
                animation.FrameStart -= baseFrame;

                moveable.Animations[i] = animation;
            }

            for (int i = 0; i < moveable.Animations.Count; i++)
            {
                var animation = moveable.Animations[i];

                // HACK: this fixes some invalid NextFrame values
                if (moveable.Animations[animation.NextAnimation].FrameBase != 0)
                    animation.NextFrame %= moveable.Animations[animation.NextAnimation].FrameBase;

                foreach (var stateChange in animation.StateChanges)
                {
                    foreach (var animDispatch in stateChange.Dispatches)
                    {
                        if (moveable.Animations[animDispatch.NextAnimation].FrameBase != 0)
                        {
                            ushort newFrame = (ushort)(animDispatch.NextFrame % moveable.Animations[animDispatch.NextAnimation].FrameBase);

                            // In some cases dispatches have invalid NextFrame.
                            // From tests it seems that's ok to delete the dispatch or put the NextFrame equal to zero.
                            if (newFrame > moveable.Animations[animDispatch.NextAnimation].RealNumberOfFrames) newFrame = 0;

                            animDispatch.NextFrame = newFrame;
                        }
                    }
                }

                moveable.Animations[i] = animation;
            }

            wad.Moveables.Add(m.ObjectID, moveable);

            return moveable;
        }

        public static WadStatic ConvertTr4StaticMeshToWadStatic(Wad2 wad, TR4Wad oldWad, int staticIndex,
                                                                Dictionary<int, WadTexture> textures)
        {
            var staticMesh = new WadStatic();
            var oldStaticMesh = oldWad.StaticMeshes[staticIndex];

            // First setup collisional and visibility bounding boxes
            staticMesh.CollisionBox = new BoundingBox(new Vector3(oldStaticMesh.CollisionX1,
                                                                  -oldStaticMesh.CollisionY1,
                                                                  oldStaticMesh.CollisionZ1),
                                                      new Vector3(oldStaticMesh.CollisionX2,
                                                                  -oldStaticMesh.CollisionY2,
                                                                  oldStaticMesh.CollisionZ2));

            staticMesh.VisibilityBox = new BoundingBox(new Vector3(oldStaticMesh.VisibilityX1,
                                                                   -oldStaticMesh.VisibilityY1,
                                                                   oldStaticMesh.VisibilityZ1),
                                                       new Vector3(oldStaticMesh.VisibilityX2,
                                                                   -oldStaticMesh.VisibilityY2,
                                                                   oldStaticMesh.VisibilityZ2));

            // Then import the mesh. If it was already added, the mesh will not be added to the dictionary.
            staticMesh.Mesh = ConvertTr4MeshToWadMesh(wad,
                                                      oldWad,
                                                      oldWad.GetMeshFromPointer(oldStaticMesh.PointersIndex),
                                                      textures);

            staticMesh.ObjectID = oldStaticMesh.ObjectId;

            wad.Statics.Add(staticMesh.ObjectID, staticMesh);

            return staticMesh;
        }

        private static List<Vector2> CalculateTr4UVCoordinates(TR4Wad oldWad, wad_polygon poly)
        {
            List<Vector2> uv = new List<Vector2>();

            // recupero le informazioni necessarie
            int shape = (poly.Texture & 0x7000) >> 12;
            int flipped = (poly.Texture & 0x8000) >> 15;
            int textureId = poly.Texture & 0xfff;
            if (textureId > 2047)
                textureId = -(textureId - 4096);

            wad_object_texture texture = oldWad.Textures[textureId];

            Vector2 nw = new Vector2(0.5f, 0.5f);
            Vector2 ne = new Vector2(texture.Width - 0.5f, 0.5f);
            Vector2 se = new Vector2(texture.Width - 0.5f, texture.Height - 0.5f);
            Vector2 sw = new Vector2(0.5f, texture.Height - 0.5f);

            if (poly.Shape == 9)
            {
                if (flipped == 1)
                {
                    uv.Add(ne);
                    uv.Add(nw);
                    uv.Add(sw);
                    uv.Add(se);
                }
                else
                {
                    uv.Add(nw);
                    uv.Add(ne);
                    uv.Add(se);
                    uv.Add(sw);
                }
            }
            else
            {
                switch (shape)
                {
                    case 0:
                        if (flipped == 1)
                        {
                            uv.Add(ne);
                            uv.Add(nw);
                            uv.Add(se);
                        }
                        else
                        {
                            uv.Add(nw);
                            uv.Add(ne);
                            uv.Add(sw);
                        }
                        break;

                    case 2:
                        if (flipped == 1)
                        {
                            uv.Add(nw);
                            uv.Add(sw);
                            uv.Add(ne);
                        }
                        else
                        {
                            uv.Add(ne);
                            uv.Add(se);
                            uv.Add(nw);
                        }
                        break;

                    case 4:
                        if (flipped == 1)
                        {
                            uv.Add(sw);
                            uv.Add(se);
                            uv.Add(nw);
                        }
                        else
                        {
                            uv.Add(se);
                            uv.Add(sw);
                            uv.Add(ne);
                        }
                        break;

                    case 6:
                        if (flipped == 1)
                        {
                            uv.Add(se);
                            uv.Add(ne);
                            uv.Add(sw);
                        }
                        else
                        {
                            uv.Add(sw);
                            uv.Add(nw);
                            uv.Add(se);
                        }
                        break;
                }
            }

            return uv;
        }

        private static void CollectTexturesAndMeshesForCancellation(Wad2 wad,
                                                                    WadObject obj,
                                                                    out List<WadTexture> textures,
                                                                    out List<WadMesh> meshes)
        {
            textures = new List<WadTexture>();
            meshes = new List<WadMesh>();

            var meshesToCheck = new List<WadMesh>();
            var texturesToCheck = new List<WadTexture>();

            // Collect all meshes
            if (obj.GetType() == typeof(WadMoveable))
            {
                WadMoveable moveable = (WadMoveable)obj;

                foreach (var mesh in moveable.Meshes)
                {
                    if (!meshesToCheck.Contains(mesh)) meshesToCheck.Add(mesh);
                }
            }
            else
            {
                WadStatic staticMesh = (WadStatic)obj;
                meshesToCheck.Add(staticMesh.Mesh);
            }

            // Now check if some of selected meshes are used elsewhere
            foreach (var mesh in meshesToCheck)
            {
                bool found = false;

                for (int i = 0; i < wad.Moveables.Count; i++)
                {
                    var moveable = wad.Moveables.ElementAt(i).Value;
                    if (obj.GetType() == typeof(WadMoveable) && moveable.ObjectID == obj.ObjectID) continue;

                    foreach (var moveableMesh in moveable.Meshes)
                    {
                        if (moveableMesh == mesh)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found) break;
                }

                if (!found) meshes.Add(mesh);

                found = false;

                for (int i = 0; i < wad.Statics.Count; i++)
                {
                    var staticMesh = wad.Statics.ElementAt(i).Value;
                    if (obj.GetType() == typeof(WadStatic) && staticMesh.ObjectID == obj.ObjectID) continue;

                    if (staticMesh.Mesh == mesh)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found) meshes.Add(mesh);
            }

            // At this point, I have only the meshes to remove and from them I collect all textures
            foreach (var mesh in meshes)
            {
                foreach (var poly in mesh.Polys)
                {
                    if (!texturesToCheck.Contains(poly.Texture) && poly.Texture != null) texturesToCheck.Add(poly.Texture); 
                }
            }

            // Like for meshes, search inside other objects
            foreach (var texture in texturesToCheck)
            {
                bool found = false;

                for (int i = 0; i < wad.Moveables.Count; i++)
                {
                    var moveable = wad.Moveables.ElementAt(i).Value;
                    if (obj.GetType() == typeof(WadMoveable) && moveable.ObjectID == obj.ObjectID) continue;

                    foreach (var moveableMesh in moveable.Meshes)
                    {
                        foreach (var poly in moveableMesh.Polys)
                        {
                            if (poly.Texture == texture)
                            {
                                found = true;
                                break;
                            }
                        }

                        if (found) break;
                    }

                    if (found) break;
                }

                if (!found) textures.Add(texture);

                found = false;

                for (int i = 0; i < wad.Statics.Count; i++)
                {
                    var staticMesh = wad.Statics.ElementAt(i).Value;
                    if (obj.GetType() == typeof(WadStatic) && staticMesh.ObjectID == obj.ObjectID) continue;

                    foreach (var poly in staticMesh.Mesh.Polys)
                    {
                        if (poly.Texture == texture)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found) break;
                }

                if (!found) textures.Add(texture);
            }
        }

        public static void DeleteMoveableFromWad2(Wad2 wad, WadMoveable moveable)
        {
            throw new NotImplementedException();
        }

        public static void AddMoveableToWad2(Wad2 sourceWad, Wad2 destWad, WadMoveable moveable)
        {
            throw new NotImplementedException();
        }
    }
}
