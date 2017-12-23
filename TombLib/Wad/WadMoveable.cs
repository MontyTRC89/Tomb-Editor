using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public class WadMoveable : WadObject
    {
        public List<WadMesh> Meshes {  get { return _meshes; } }
        public List<WadLink> Links {  get { return _links; } }
        public Vector3 Offset { get { return _offset; } set { _offset = value; } }
        public List<WadAnimation> Animations { get { return _animations; } }

        private List<WadMesh> _meshes;
        private List<WadLink> _links;
        private Vector3 _offset;
        private List<WadAnimation> _animations;

        public WadMoveable(Wad2 wad)
            : base (wad)
        {
            _meshes = new List<WadMesh>();
            _links = new List<WadLink>();
            _animations = new List<WadAnimation>();
        }

        public override string ToString()
        {
            return "(" + ObjectID + ") " + TrCatalog.GetMoveableName(Wad.Version, ObjectID);
        }

        public List<WadSoundInfo> GetSounds(Wad2 wad)
        {
            var sounds = new List<WadSoundInfo>();

            foreach (var animation in Animations)
            {
                foreach (var command in animation.AnimCommands)
                {
                    ushort soundId = (ushort)(command.Parameter2 & 0x3fff);
                    if (wad.SoundInfo.ContainsKey(soundId))
                    {
                        if (!sounds.Contains(wad.SoundInfo[soundId]))
                            sounds.Add(wad.SoundInfo[soundId]);
                    }
                }
            }

            return sounds;
        }
    }
}
