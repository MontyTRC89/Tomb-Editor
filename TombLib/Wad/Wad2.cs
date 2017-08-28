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
        private List<WadTextureSample> _textureInfos;

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
    }
}
