using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadPolygon
    {
        public WadPolygonShape Shape { get { return _shape; } }
        public List<int> Indices { get { return _indices; } }
        public List<Vector2> UV { get { return _uv; } }
        public WadTexture Texture { get { return _texture; } set { _texture = value; } }
        public byte ShineStrength { get { return _shineStrength; } set { _shineStrength = value; } }
        public bool Transparent { get { return _transparent; } set { _transparent = value; } }
        public byte Attributes { get { return _attributes; } set { _attributes = value; } }

        private WadPolygonShape _shape;
        private List<int> _indices;
        private List<Vector2> _uv;
        private WadTexture _texture;
        private byte _shineStrength;
        private bool _transparent;
        private byte _attributes;

        public WadPolygon(WadPolygonShape shape)
        {
            _shape = shape;
            _indices = new List<int>();
            _uv = new List<Vector2>();
        }
    }
}
