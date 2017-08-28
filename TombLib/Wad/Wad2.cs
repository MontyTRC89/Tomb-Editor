using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Wad
{
    public class Wad2
    {
        // Textures
        private List<WadTexture> _textures;

        // Meshes
        private List<WadMesh> _meshes;
        private List<uint> _meshPointers;
        private List<WadLink> _meshTrees;

        // Animations
        private List<WadAnimation> _animations;
        private List<WadStateChange> _stateChanges;
        private List<WadAnimDispatch> _animDispatches;
        private List<ushort> _animCommands;
        private List<WadKeyFrame> _keyFrames;

        // Objects
        private List<WadMoveable> _moveables;
        private List<WadStatic> _staticMeshes;

        // Sounds
        private List<short> _soundMap;
        private List<WadSoundInfo> _soundInfos;

        public Wad2()
        {
            _textures = new List<WadTexture>();
            _meshes = new List<WadMesh>();
            _meshPointers = new List<uint>();
            _meshTrees = new List<WadLink>();
            _animations = new List<WadAnimation>();
            _stateChanges = new List<WadStateChange>();
            _animDispatches = new List<WadAnimDispatch>();
            _animCommands = new List<ushort>();
            _keyFrames = new List<WadKeyFrame>();
            _moveables = new List<WadMoveable>();
            _staticMeshes = new List<WadStatic>();
            _soundMap = new List<short>();
            _soundInfos = new List<WadSoundInfo>();

            for (int i = 0; i < 370; i++)
            {
                _soundMap.Add(-1);
            }
        }

        public List<WadTexture> Textures { get { return _textures; } }
        public List<WadMesh> Meshes { get { return _meshes; } }
        public List<uint> MeshPointers { get { return _meshPointers; } }
        public List<WadLink> MeshTrees { get { return _meshTrees; } }
        public List<WadAnimation> Animations { get { return _animations; } }
        public List<WadStateChange> StateChanges { get { return _stateChanges; } }
        public List<WadAnimDispatch> AnimDispatches { get { return _animDispatches; } }
        public List<ushort> AnimCommands { get { return _animCommands; } }
        public List<WadKeyFrame> KeyFrames { get { return _keyFrames; } }
        public List<WadMoveable> Moveables { get { return _moveables; } }
        public List<WadStatic> StaticMeshes { get { return _staticMeshes; } }
        public List<short> SoundMap { get { return _soundMap; } }
        public List<WadSoundInfo> SoundInfo { get { return _soundInfos; } }
    }
}
