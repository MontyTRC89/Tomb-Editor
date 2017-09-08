using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.Wad
{
    public partial class Wad2
    {
        private static byte[] _magicWord = new byte[] { 0x57, 0x41, 0x44, 0x32 };
        private static byte[] _soundsMagicWord = new byte[] { 0x53, 0x4F, 0x55, 0x4E, 0x44, 0x53 };
        private static byte[] _spritesMagicWord = new byte[] { 0x53, 0x50, 0x52, 0x49, 0x54, 0x45, 0x53 };

        public static Wad2 LoadFromStream(Stream stream)
        {
            Wad2 wad = new Wad2();
            WadChunkType chunkType;

            using (var reader = new BinaryReaderEx(stream))
            {
                byte[] version = reader.ReadBytes(_magicWord.Length);

                // Read textures
                uint numTextures = reader.ReadUInt32();
                for (int i = 0; i < numTextures; i++)
                {
                    var texture = new WadTexture();

                    var image = ImageC.CreateNew(reader.ReadInt32(), reader.ReadInt32());
                    var buffer = reader.ReadBytes(4 * image.Width * image.Height);
                    image.SetData(buffer);

                    texture.Image = image;

                    // Check for other chunks
                    chunkType = (WadChunkType)reader.ReadUInt16();
                    if (chunkType != WadChunkType.NoExtraChunk)
                    {
                        /* TODO: logic for reading in the future other chunks
                           Example:

                           if (chunkType == WadChunkType.AdditionalTextureAttributes)
                           {
                                // Read new fields
                           }
                           else
                           {
                                // Unknown chunk (probably Wad2 newer than TombLib)
                                long chunkSize = reader.ReadUInt64();
                                reader.Seek(reader.BaseStream.Position + chunkSize, SeekOrigin.Begin);
                           }
                        */
                    }

                    texture.UpdateHash();

                    wad.Textures.Add(texture.Hash, texture);
                }

                // Read meshes
                uint numMeshes = reader.ReadUInt32();
                for (int i = 0; i < numMeshes; i++)
                {
                    var mesh = new WadMesh();

                    int xMin = Int32.MaxValue;
                    int yMin = Int32.MaxValue;
                    int zMin = Int32.MaxValue;
                    int xMax = Int32.MinValue;
                    int yMax = Int32.MinValue;
                    int zMax = Int32.MinValue;

                    mesh.BoundingSphere = new BoundingSphere(reader.ReadVector3(), reader.ReadSingle());

                    uint numVertices = reader.ReadUInt32();
                    for (int j = 0; j < numVertices; j++)
                    {
                        var position = reader.ReadVector3();

                        if (position.X < xMin)
                            xMin = (int)position.X;
                        if (position.Y < yMin)
                            yMin = (int)position.Y;
                        if (position.Z < zMin)
                            zMin = (int)position.Z;

                        if (position.X > xMax)
                            xMax = (int)position.X;
                        if (position.Y > yMax)
                            yMax = (int)position.Y;
                        if (position.Z > zMax)
                            zMax = (int)position.Z;

                        mesh.VerticesPositions.Add(position);
                    }

                    WadMeshLightingType normalsOrShades = (WadMeshLightingType)reader.ReadUInt16();
                    if (normalsOrShades == WadMeshLightingType.Normals)
                    {
                        for (int j = 0; j < numVertices; j++)
                        {
                            mesh.VerticesNormals.Add(reader.ReadVector3());
                        }
                    }
                    else
                    {
                        for (int j = 0; j < numVertices; j++)
                        {
                            mesh.VerticesShades.Add(reader.ReadInt16());
                        }
                    }

                    uint numPolygons = reader.ReadUInt32();
                    for (int j = 0; j < numPolygons; j++)
                    {
                        var poly = new WadPolygon((WadPolygonShape)reader.ReadUInt16());

                        poly.Indices.Add(reader.ReadInt32());
                        poly.Indices.Add(reader.ReadInt32());
                        poly.Indices.Add(reader.ReadInt32());
                        if (poly.Shape == WadPolygonShape.Rectangle) poly.Indices.Add(reader.ReadInt32());

                        poly.UV.Add(reader.ReadVector2());
                        poly.UV.Add(reader.ReadVector2());
                        poly.UV.Add(reader.ReadVector2());
                        if (poly.Shape == WadPolygonShape.Rectangle) poly.UV.Add(reader.ReadVector2());

                        uint textureIndex = reader.ReadUInt32();
                        poly.Texture = wad.Textures.ElementAt((int)textureIndex).Value;

                        poly.ShineStrength = reader.ReadByte();
                        poly.Transparent = reader.ReadBoolean();
                        poly.Attributes = reader.ReadByte();

                        // Check for other chunks
                        chunkType = (WadChunkType)reader.ReadUInt16();
                        if (chunkType != WadChunkType.NoExtraChunk)
                        {
                            // TODO: logic for reading in the future other chunks
                        }

                        mesh.Polys.Add(poly);
                    }

                    // Check for other chunks
                    chunkType = (WadChunkType)reader.ReadUInt16();
                    if (chunkType != WadChunkType.NoExtraChunk)
                    {
                        // TODO: logic for reading in the future other chunks
                    }

                    mesh.BoundingBox = new BoundingBox(new Vector3(xMin, yMin, zMin), new Vector3(xMax, yMax, zMax));

                    mesh.UpdateHash();
                    wad.Meshes.Add(mesh.Hash, mesh);
                }

                // Read moveables
                uint numMoveables = reader.ReadUInt32();
                for (int i = 0; i < numMoveables; i++)
                {
                    var moveable = new WadMoveable();

                    moveable.ObjectID = reader.ReadUInt32();

                    uint numMeshesInThisMoveable = reader.ReadUInt32();
                    for (int j = 0; j < numMeshesInThisMoveable; j++)
                    {
                        moveable.Meshes.Add(wad.Meshes.ElementAt((int)reader.ReadUInt32()).Value);
                    }

                    uint numLinks = reader.ReadUInt32();
                    for (int j = 0; j < numLinks; j++)
                    {
                        var link = new WadLink((WadLinkOpcode)reader.ReadUInt16(),
                                               reader.ReadVector3());

                        // Check for other chunks
                        chunkType = (WadChunkType)reader.ReadUInt16();
                        if (chunkType != WadChunkType.NoExtraChunk)
                        {
                            // TODO: logic for reading in the future other chunks
                        }

                        moveable.Links.Add(link);
                    }

                    moveable.Offset = reader.ReadVector3();

                    uint numAnimations = reader.ReadUInt32();
                    for (int j = 0; j < numAnimations; j++)
                    {
                        var animation = new WadAnimation();

                        animation.FrameDuration = reader.ReadByte();
                        animation.StateId = reader.ReadUInt16();
                        animation.Speed = reader.ReadInt32();
                        animation.Acceleration = reader.ReadInt32();
                        animation.LateralSpeed = reader.ReadInt32();
                        animation.LateralAcceleration = reader.ReadInt32();
                        animation.NextAnimation = reader.ReadUInt16();
                        animation.NextFrame = reader.ReadUInt16();
                        animation.FrameStart = reader.ReadUInt16();
                        animation.FrameEnd = reader.ReadUInt16();
                        animation.RealNumberOfFrames = reader.ReadUInt16();

                        uint numKeyframes = reader.ReadUInt32();
                        for (int k = 0; k < numKeyframes; k++)
                        {
                            var keyFrame = new WadKeyFrame();

                            keyFrame.BoundingBox = reader.ReadBoundingBox();
                            keyFrame.Offset = reader.ReadVector3();

                            for (int l = 0; l < numMeshesInThisMoveable; l++)
                            {
                                var angle = new WadKeyFrameRotation();

                                angle.Axis = (WadKeyFrameRotationAxis)reader.ReadUInt16();
                                angle.X = reader.ReadInt32();
                                angle.Y = reader.ReadInt32();
                                angle.Z = reader.ReadInt32();

                                keyFrame.Angles.Add(angle);
                            }

                            // Check for other chunks
                            chunkType = (WadChunkType)reader.ReadUInt16();
                            if (chunkType != WadChunkType.NoExtraChunk)
                            {
                                // TODO: logic for reading in the future other chunks
                            }

                            animation.KeyFrames.Add(keyFrame);
                        }

                        uint numStateChanges = reader.ReadUInt32();
                        for (int k = 0; k < numStateChanges; k++)
                        {
                            var stateChange = new WadStateChange();

                            stateChange.StateId = reader.ReadUInt16();
                            stateChange.NumDispatches = reader.ReadUInt32();

                            for (int l = 0; l < stateChange.NumDispatches; l++)
                            {
                                var dispatch = new WadAnimDispatch();

                                dispatch.InFrame = reader.ReadUInt16();
                                dispatch.OutFrame = reader.ReadUInt16();
                                dispatch.NextAnimation = reader.ReadUInt16();
                                dispatch.NextFrame = reader.ReadUInt16();

                                // Check for other chunks
                                chunkType = (WadChunkType)reader.ReadUInt16();
                                if (chunkType != WadChunkType.NoExtraChunk)
                                {
                                    // TODO: logic for reading in the future other chunks
                                }

                                stateChange.Dispatches.Add(dispatch);
                            }

                            // Check for other chunks
                            chunkType = (WadChunkType)reader.ReadUInt16();
                            if (chunkType != WadChunkType.NoExtraChunk)
                            {
                                // TODO: logic for reading in the future other chunks
                            }

                            animation.StateChanges.Add(stateChange);
                        }

                        uint numAnimCommands = reader.ReadUInt32();
                        for (int k = 0; k < numAnimCommands; k++)
                        {
                            var animCommand = new WadAnimCommand((WadAnimCommandType)reader.ReadUInt16());

                            animCommand.Parameter1 = reader.ReadUInt16();
                            animCommand.Parameter2 = reader.ReadUInt16();
                            animCommand.Parameter3 = reader.ReadUInt16();

                            // Check for other chunks
                            chunkType = (WadChunkType)reader.ReadUInt16();
                            if (chunkType != WadChunkType.NoExtraChunk)
                            {
                                // TODO: logic for reading in the future other chunks
                            }

                            animation.AnimCommands.Add(animCommand);
                        }

                        // Check for other chunks
                        chunkType = (WadChunkType)reader.ReadUInt16();
                        if (chunkType != WadChunkType.NoExtraChunk)
                        {
                            // TODO: logic for reading in the future other chunks
                        }

                        moveable.Animations.Add(animation);
                    }

                    // Check for other chunks
                    chunkType = (WadChunkType)reader.ReadUInt16();
                    if (chunkType != WadChunkType.NoExtraChunk)
                    {
                        // TODO: logic for reading in the future other chunks
                    }

                    wad.Moveables.Add(moveable.ObjectID, moveable);
                }

                // Read static meshes
                uint numStaticMeshes = reader.ReadUInt32();
                for (int i = 0; i < numStaticMeshes; i++)
                {
                    var staticMesh = new WadStatic();

                    staticMesh.ObjectID = reader.ReadUInt32();
                    staticMesh.VisibilityBox = reader.ReadBoundingBox();
                    staticMesh.CollisionBox = reader.ReadBoundingBox();
                    staticMesh.Flags = reader.ReadInt16();
                    staticMesh.Mesh = wad.Meshes.ElementAt((int)reader.ReadUInt32()).Value;

                    // Check for other chunks
                    chunkType = (WadChunkType)reader.ReadUInt16();
                    if (chunkType != WadChunkType.NoExtraChunk)
                    {
                        // TODO: logic for reading in the future other chunks
                    }

                    wad.Statics.Add(staticMesh.ObjectID, staticMesh);
                }

                // Read sounds
                byte[] soundMagicWord = reader.ReadBytes(_soundsMagicWord.Length);

                uint numSounds = reader.ReadUInt32();
                for (int i = 0; i < numSounds; i++)
                {
                    var sound = new WadSoundInfo();

                    ushort soundId = reader.ReadUInt16();

                    sound.Volume = reader.ReadByte();
                    sound.Range = reader.ReadByte();
                    sound.Pitch = reader.ReadByte();
                    sound.Loop = reader.ReadByte();
                    sound.FlagN = reader.ReadBoolean();
                    sound.RandomizeGain = reader.ReadBoolean();
                    sound.RandomizePitch = reader.ReadBoolean();

                    uint numWaves = reader.ReadUInt32();
                    for (int j = 0; j < numWaves; j++)
                    {
                        uint waveSize = reader.ReadUInt32();
                        var wave = new WadSound(reader.ReadBytes((int)waveSize));
                        sound.WaveSounds.Add(wave);
                    }

                    // Check for other chunks
                    chunkType = (WadChunkType)reader.ReadUInt16();
                    if (chunkType != WadChunkType.NoExtraChunk)
                    {
                        // TODO: logic for reading in the future other chunks
                    }

                    wad.SoundInfo.Add(soundId, sound);
                }

                // Read sprites
                byte[] spritesMagicWord = reader.ReadBytes(_spritesMagicWord.Length);

                uint numSprites = reader.ReadUInt32();
                for (int i = 0; i < numSprites; i++)
                {
                    var texture = new WadTexture();

                    var image = ImageC.CreateNew(reader.ReadInt32(), reader.ReadInt32());
                    var buffer = reader.ReadBytes(4 * image.Width * image.Height);
                    image.SetData(buffer);

                    texture.Image = image;

                    // Check for other chunks
                    chunkType = (WadChunkType)reader.ReadUInt16();
                    if (chunkType != WadChunkType.NoExtraChunk)
                    {
                        // TODO: logic for reading in the future other chunks
                    }

                    texture.UpdateHash();

                    wad.SpriteTextures.Add(texture.Hash, texture);
                }

                uint numSequences = reader.ReadUInt32();
                for (int i = 0; i < numSequences; i++)
                {
                    var sequence = new WadSpriteSequence();

                    sequence.ObjectID = reader.ReadUInt32();

                    uint numSpritesInSequence = reader.ReadUInt32();
                    for (int j = 0; j < numSpritesInSequence; j++)
                    {
                        sequence.Sprites.Add(wad.SpriteTextures.ElementAt((int)reader.ReadUInt32()).Value);
                    }

                    // Check for other chunks
                    chunkType = (WadChunkType)reader.ReadUInt16();
                    if (chunkType != WadChunkType.NoExtraChunk)
                    {
                        // TODO: logic for reading in the future other chunks
                    }

                    wad.SpriteSequences.Add(sequence);
                }

                // Check for other chunks
                chunkType = (WadChunkType)reader.ReadUInt16();
                if (chunkType != WadChunkType.NoExtraChunk)
                {
                    // TODO: logic for reading in the future other chunks
                }
            }

            return wad;
        }

        public static bool SaveToStream(Wad2 wad, Stream stream)
        {
            ushort chunkMagicWord;
            uint chunkSize;

            var texturesList = new List<WadTexture>();
            for (int i = 0; i < wad.Textures.Count; i++)
            {
                var texture = wad.Textures.ElementAt(i).Value;
                texturesList.Add(texture); 
            }

            var meshesList = new List<WadMesh>();
            for (int i = 0; i < wad.Meshes.Count; i++)
            {
                var mesh = wad.Meshes.ElementAt(i).Value;
                meshesList.Add(mesh);
            }

            using (var writer = new BinaryWriterEx(stream))
            {
                // Write magic word
                writer.Write(_magicWord);

                // Store number of textures
                uint numTextures = (uint)wad.Textures.Count;
                writer.Write(numTextures);

                // Write textures
                for (int i = 0; i < numTextures; i++)
                {
                    var texture = wad.Textures.ElementAt(i).Value;

                    writer.Write(texture.Width);
                    writer.Write(texture.Height);
                    writer.Write(texture.Image.ToByteArray());

                    // No more data, in future we can expand the structure using chunks
                    chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                    writer.Write(chunkMagicWord);
                }

                // Store number of meshes
                uint numMeshes = (uint)wad.Meshes.Count;
                writer.Write(numMeshes);

                // Write meshes
                for (int i = 0; i < numMeshes; i++)
                {
                    var mesh = wad.Meshes.ElementAt(i).Value;

                    writer.Write(mesh.BoundingSphere.Center);
                    writer.Write(mesh.BoundingSphere.Radius);

                    uint numVertices = (uint)mesh.VerticesPositions.Count;
                    writer.Write(numVertices);
                    foreach (var position in mesh.VerticesPositions)
                    {
                        writer.Write(position);
                    }

                    // Has normals or shades?
                    var hasNormalsOrShades = (mesh.VerticesNormals.Count != 0 ? WadMeshLightingType.Normals :
                                                                                WadMeshLightingType.PrecalculatedGrayShades);
                    writer.Write((ushort)hasNormalsOrShades);

                    if (hasNormalsOrShades == WadMeshLightingType.Normals)
                    {
                        foreach (var normal in mesh.VerticesNormals)
                        {
                            writer.Write(normal);
                        }
                    }
                    else
                    {
                        foreach (var shade in mesh.VerticesShades)
                        {
                            writer.Write(shade);
                        }
                    }

                    // Store number of polygons
                    uint numPolygons = (uint)mesh.Polys.Count;
                    writer.Write(numPolygons);
                    foreach (var poly in mesh.Polys)
                    {
                        writer.Write((ushort)poly.Shape);

                        // Write indices
                        writer.Write(poly.Indices[0]);
                        writer.Write(poly.Indices[1]);
                        writer.Write(poly.Indices[2]);
                        if (poly.Shape == WadPolygonShape.Rectangle) writer.Write(poly.Indices[3]);

                        // Write UVs
                        writer.Write(poly.UV[0]);
                        writer.Write(poly.UV[1]);
                        writer.Write(poly.UV[2]);
                        if (poly.Shape == WadPolygonShape.Rectangle) writer.Write(poly.UV[3]);

                        // Store index of texture
                        uint textureIndex = (uint)texturesList.IndexOf(poly.Texture);
                        writer.Write(textureIndex);

                        // Attributes
                        writer.Write(poly.ShineStrength);
                        writer.Write(poly.Transparent);
                        writer.Write(poly.Attributes);

                        // No more data, in future we can expand the structure using chunks
                        chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                        writer.Write(chunkMagicWord);
                    }

                    // No more data, in future we can expand the structure using chunks
                    chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                    writer.Write(chunkMagicWord);
                }

                // Store number of moveables
                uint numMoveables = (uint)wad.Moveables.Count;
                writer.Write(numMoveables);

                for (int i = 0; i < numMoveables; i++)
                {
                    var moveable = wad.Moveables.ElementAt(i).Value;

                    writer.Write(moveable.ObjectID);

                    // Store meshes
                    uint numMeshesInThisMoveable = (uint)moveable.Meshes.Count;
                    writer.Write(numMeshesInThisMoveable);

                    foreach (var mesh in moveable.Meshes)
                    {
                        uint meshIndex = (uint)meshesList.IndexOf(mesh);
                        writer.Write(meshIndex);
                    }

                    uint numLinks = (uint)moveable.Links.Count;
                    writer.Write(numLinks);

                    // Store links
                    foreach (var link in moveable.Links)
                    {
                        writer.Write((ushort)link.Opcode);
                        writer.Write(link.Offset);

                        // No more data, in future we can expand the structure using chunks
                        chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                        writer.Write(chunkMagicWord);
                    }

                    // Store offset
                    writer.Write(moveable.Offset);

                    // Store animations
                    uint numAnimations = (uint)moveable.Animations.Count;
                    writer.Write(numAnimations);

                    foreach (var animation in moveable.Animations)
                    {
                        writer.Write(animation.FrameDuration);
                        writer.Write(animation.StateId);
                        writer.Write(animation.Speed);
                        writer.Write(animation.Acceleration);
                        writer.Write(animation.LateralSpeed);
                        writer.Write(animation.LateralAcceleration);
                        writer.Write(animation.NextAnimation);
                        writer.Write(animation.NextFrame);
                        writer.Write(animation.FrameStart);
                        writer.Write(animation.FrameEnd);
                        writer.Write(animation.RealNumberOfFrames);

                        // Write keyframes
                        uint numKeyframes = (uint)animation.KeyFrames.Count;
                        writer.Write(numKeyframes);

                        foreach (var keyframe in animation.KeyFrames)
                        {
                            writer.Write(keyframe.BoundingBox);
                            writer.Write(keyframe.Offset);

                            foreach (var angle in keyframe.Angles)
                            {
                                writer.Write((ushort)angle.Axis);
                                writer.Write(angle.X);
                                writer.Write(angle.Y);
                                writer.Write(angle.Z);
                            }

                            // No more data, in future we can expand the structure using chunks
                            chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                            writer.Write(chunkMagicWord);
                        }

                        // Write state changes
                        uint numStateChanges = (uint)animation.StateChanges.Count;
                        writer.Write(numStateChanges);

                        foreach (var stateChange in animation.StateChanges)
                        {
                            writer.Write(stateChange.StateId);

                            uint numDispatches = (uint)stateChange.Dispatches.Count;
                            writer.Write(numDispatches);

                            // Write dispatches
                            foreach (var dispatch in stateChange.Dispatches)
                            {
                                writer.Write(dispatch.InFrame);
                                writer.Write(dispatch.OutFrame);
                                writer.Write(dispatch.NextAnimation);
                                writer.Write(dispatch.NextFrame);

                                // No more data, in future we can expand the structure using chunks
                                chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                                writer.Write(chunkMagicWord);
                            }

                            // No more data, in future we can expand the structure using chunks
                            chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                            writer.Write(chunkMagicWord);
                        }

                        // Write anim commands
                        uint numAnimCommands = (uint)animation.AnimCommands.Count;
                        writer.Write(numAnimCommands);

                        foreach (var animCommands in animation.AnimCommands)
                        {
                            writer.Write((ushort)animCommands.Type);
                            writer.Write(animCommands.Parameter1);
                            writer.Write(animCommands.Parameter2);
                            writer.Write(animCommands.Parameter3);

                            // No more data, in future we can expand the structure using chunks
                            chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                            writer.Write(chunkMagicWord);
                        }

                        // No more data, in future we can expand the structure using chunks
                        chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                        writer.Write(chunkMagicWord);
                    }

                    // No more data, in future we can expand the structure using chunks
                    chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                    writer.Write(chunkMagicWord);
                }

                // Store number of static meshes
                uint numStaticMeshes = (uint)wad.Statics.Count;
                writer.Write(numStaticMeshes);

                for (int i = 0; i < numStaticMeshes; i++)
                {
                    var staticMesh = wad.Statics.ElementAt(i).Value;

                    writer.Write(staticMesh.ObjectID);
                    writer.Write(staticMesh.VisibilityBox);
                    writer.Write(staticMesh.CollisionBox);
                    writer.Write(staticMesh.Flags);

                    uint meshIndex = (uint)meshesList.IndexOf(staticMesh.Mesh);
                    writer.Write(meshIndex);

                    // No more data, in future we can expand the structure using chunks
                    chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                    writer.Write(chunkMagicWord);
                }

                writer.Write(_soundsMagicWord);

                // Write sounds
                uint numSounds = (uint)wad.SoundInfo.Count;
                writer.Write(numSounds);

                for (int i = 0; i < wad.SoundInfo.Count; i++)
                {
                    var sound = wad.SoundInfo.ElementAt(i).Value;
                    ushort soundId = (ushort)wad.SoundInfo.ElementAt(i).Key;

                    writer.Write(soundId);
                    writer.Write(sound.Volume);
                    writer.Write(sound.Range);
                    writer.Write(sound.Pitch);
                    writer.Write(sound.Loop);
                    writer.Write(sound.FlagN);
                    writer.Write(sound.RandomizeGain);
                    writer.Write(sound.RandomizePitch);

                    uint numWaves = (uint)sound.WaveSounds.Count;
                    writer.Write(numWaves);

                    foreach (var wave in sound.WaveSounds)
                    {
                        uint waveSize = (uint)wave.WaveData.Length;
                        writer.Write(waveSize);
                        writer.Write(wave.WaveData);
                    }

                    // No more data, in future we can expand the structure using chunks
                    chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                    writer.Write(chunkMagicWord);
                }

                writer.Write(_spritesMagicWord);

                // Write sprites
                var spritesList = new List<WadTexture>();

                uint numSpritesTextures = (uint)wad.SpriteTextures.Count;
                writer.Write(numSpritesTextures);

                for (int i = 0; i < wad.SpriteTextures.Count; i++)
                {
                    var texture = wad.SpriteTextures.ElementAt(i).Value;

                    writer.Write(texture.Width);
                    writer.Write(texture.Height);
                    writer.Write(texture.Image.ToByteArray());

                    // No more data, in future we can expand the structure using chunks
                    chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                    writer.Write(chunkMagicWord);

                    spritesList.Add(texture);
                }

                uint numSpritesSequences = (uint)wad.SpriteSequences.Count;
                writer.Write(numSpritesSequences);

                foreach (var sequence in wad.SpriteSequences)
                {
                    writer.Write(sequence.ObjectID);

                    uint numSprites = (uint)sequence.Sprites.Count;
                    writer.Write(numSprites);

                    foreach (var sprite in sequence.Sprites)
                    {
                        uint spriteIndex = (uint)spritesList.IndexOf(sprite);
                        writer.Write(spriteIndex);
                    }

                    // No more data, in future we can expand the structure using chunks
                    chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                    writer.Write(chunkMagicWord);
                }

                // No more data, in future we can expand the structure using chunks
                chunkMagicWord = (ushort)WadChunkType.NoExtraChunk;
                writer.Write(chunkMagicWord);
            }

            return true;
        }
    }
}
