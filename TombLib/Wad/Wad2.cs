using System.Collections.Generic;

namespace TombLib.Wad
{
    public class Wad2
    {
        public Wad2()
        {
            for (int i = 0; i < 370; i++)
            {
                SoundMap.Add(-1);
            }
        }

        public List<WadTexture> Textures { get; } = new List<WadTexture>();
        public List<WadMesh> Meshes { get; } = new List<WadMesh>();
        public List<uint> MeshPointers { get; } = new List<uint>();
        public List<WadLink> MeshTrees { get; } = new List<WadLink>();
        public List<WadAnimation> Animations { get; } = new List<WadAnimation>();
        public List<WadStateChange> StateChanges { get; } = new List<WadStateChange>();
        public List<WadAnimDispatch> AnimDispatches { get; } = new List<WadAnimDispatch>();
        public List<ushort> AnimCommands { get; } = new List<ushort>();
        public List<WadKeyFrame> KeyFrames { get; } = new List<WadKeyFrame>();
        public List<WadMoveable> Moveables { get; } = new List<WadMoveable>();
        public List<WadStatic> StaticMeshes { get; } = new List<WadStatic>();
        public List<short> SoundMap { get; } = new List<short>();
        public List<WadSoundInfo> SoundInfo { get; } = new List<WadSoundInfo>();
    }
}
