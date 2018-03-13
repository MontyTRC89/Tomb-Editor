using NLog;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TombLib.IO;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.Util
{
    public class ObjectTextureManager
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [Flags]
        public enum ResultFlags : byte
        {
            None = 0,
            IsNew = 1,
            DoubleSided = 2
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Result : IEquatable<Result>
        {
            public ushort ObjectTextureIndex;
            public byte FirstVertexIndexToEmit;
            public ResultFlags Flags;

            public tr_face3 CreateFace3(ushort index0, ushort index1, ushort index2, ushort lightingEffect)
            {
                ushort objectTextureIndex = (ushort)(ObjectTextureIndex | ((Flags & ResultFlags.DoubleSided) != ResultFlags.None ? (ushort)0x8000 : (ushort)0));
                switch (FirstVertexIndexToEmit)
                {
                    case 0:
                        return new tr_face3 { Vertices = new ushort[3] { index0, index1, index2 }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
                    case 1:
                        return new tr_face3 { Vertices = new ushort[3] { index1, index2, index0 }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
                    case 2:
                        return new tr_face3 { Vertices = new ushort[3] { index2, index0, index1 }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
                    default:
                        throw new ArgumentOutOfRangeException("firstIndexToEmit");
                }
            }

            public tr_face4 CreateFace4(ushort index0, ushort index1, ushort index2, ushort index3, ushort lightingEffect)
            {
                ushort objectTextureIndex = (ushort)(ObjectTextureIndex | ((Flags & ResultFlags.DoubleSided) != ResultFlags.None ? (ushort)0x8000 : (ushort)0));
                switch (FirstVertexIndexToEmit)
                {
                    case 0:
                        return new tr_face4 { Vertices = new ushort[4] { index0, index1, index2, index3 }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
                    case 1:
                        return new tr_face4 { Vertices = new ushort[4] { index1, index2, index3, index0 }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
                    case 2:
                        return new tr_face4 { Vertices = new ushort[4] { index2, index3, index0, index1 }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
                    case 3:
                        return new tr_face4 { Vertices = new ushort[4] { index3, index0, index1, index2 }, Texture = objectTextureIndex, LightingEffect = lightingEffect };
                    default:
                        throw new ArgumentOutOfRangeException("firstIndexToEmit");
                }
            }

            // Custom implementation of these because default implementation is *insanely* slow.
            // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
            public static unsafe bool operator ==(Result first, Result second)
            {
                return *((uint*)&first) == *((uint*)&second);
            }

            public static bool operator !=(Result first, Result second) => !(first == second);
            public bool Equals(Result other) => this == other;
            public override bool Equals(object other) => (other is Result) && this == (Result)other;
            public unsafe override int GetHashCode()
            {
                Result this2 = this;
                return unchecked((-368200913) * *((int*)&this2)); // Random prime
            }
        }

        private static readonly Vector2[] _quad0 = new Vector2[4] { new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f) };
        private static readonly Vector2[] _quad1 = new Vector2[4] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f) };
        private static readonly Vector2[] _zero = new Vector2[4] { new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f) };
        private static readonly Vector2[] _triangle0 = new Vector2[3] { new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f) };
        private static readonly Vector2[] _triangle1 = new Vector2[3] { new Vector2(-0.5f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f) };
        private static readonly Vector2[] _triangle2 = new Vector2[3] { new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, 0.5f) };
        private static readonly Vector2[] _triangle3 = new Vector2[3] { new Vector2(0.5f, -0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, -0.5f) };
        private static readonly Vector2[] _triangle4 = new Vector2[3] { new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, -0.5f) };
        private static readonly Vector2[] _triangle5 = new Vector2[3] { new Vector2(0.5f, 0.5f), new Vector2(0.5f, -0.5f), new Vector2(-0.5f, 0.5f) };
        private static readonly Vector2[] _triangle6 = new Vector2[3] { new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0.5f, 0.5f) };
        private static readonly Vector2[] _triangle7 = new Vector2[3] { new Vector2(-0.5f, -0.5f), new Vector2(-0.5f, 0.5f), new Vector2(0.5f, -0.5f) };

        public static Vector2[] GetTexCoordModificationFromNewFlags(ushort NewFlags, bool isTriangular)
        {
            if (isTriangular)
            {
                switch (NewFlags & 0x7)
                {
                    case 0x0:
                        return _triangle0;
                    case 0x1:
                        return _triangle1;
                    case 0x2:
                        return _triangle2;
                    case 0x3:
                        return _triangle3;
                    case 0x4:
                        return _triangle4;
                    case 0x5:
                        return _triangle5;
                    case 0x6:
                        return _triangle6;
                    case 0x7:
                        return _triangle7;
                }
                throw new NotImplementedException();
            }
            else
            {
                switch (NewFlags & 0x7)
                {
                    case 0x0:
                        return _quad0;
                    case 0x1:
                        return _quad1;
                    default:
                        return _zero; //Remaining cases do not do anything...
                }
            }
        }

        private static Vector2 HeuristcallyFixTexCoordUpperBound(Vector2 textureCoord)
        {
            // Add (2.01 / 256.0) to all vertices coordinates that are exactly at (127.0 / 256.0)
            Vector2 fractCoord = textureCoord - new Vector2((float)Math.Floor(textureCoord.X), (float)Math.Floor(textureCoord.Y));
            if (fractCoord == new Vector2(127.0f / 256.0f))
                return textureCoord + new Vector2(2.01f / 256.0f);
            return textureCoord;
        }

        private static ushort GetNewFlag(TextureArea texture, bool isTriangular, bool isUsedInRoomMesh, bool canRotate, out byte firstTexCoordToEmit)
        {
            firstTexCoordToEmit = 0;
            ushort result = isUsedInRoomMesh ? (ushort)0x8000 : (ushort)0;
            if (isTriangular)
            {
                texture.TexCoord0 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord0);
                texture.TexCoord1 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord1);
                texture.TexCoord2 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord2);

                bool isClockwise = !(texture.TriangleArea > 0.0); //'Not' because Y-axis is flipped in texture space
                Vector2 midPoint = (texture.TexCoord0 + texture.TexCoord1 + texture.TexCoord2) * (1.0f / 3.0f);

                // Determine closest edge to the mid
                float distance0 = (texture.TexCoord0 - midPoint).LengthSquared();
                float distance1 = (texture.TexCoord1 - midPoint).LengthSquared();
                float distance2 = (texture.TexCoord2 - midPoint).LengthSquared();
                byte closeEdgeIndex = 0;
                if (distance1 < Math.Min(distance0, distance2))
                    closeEdgeIndex = 1;
                if (distance2 < Math.Min(distance0, distance1))
                    closeEdgeIndex = 2;

                // Determine case
                Vector2 toClosestEdge = texture.GetTexCoord(closeEdgeIndex) - midPoint;
                if (toClosestEdge.X < 0)
                    if (toClosestEdge.Y < 0)
                    { // Negative X, Negative Y
                        // +---+
                        // |  /
                        // | /
                        // |/
                        // +
                        if (isClockwise)
                            result |= 0; //static constexpr Diverse::Vec<2, float> Triangle0[3] = { { 0.5f, 0.5f }, { -0.5f, 0.5f }, { 0.5f, -0.5f } };
                        else
                            result |= 5; //static constexpr Diverse::Vec<2, float> Triangle5[3] = { { 0.5f, 0.5f }, { 0.5f, -0.5f }, { -0.5f, 0.5f } };
                    }
                    else
                    { // Negative X, Postive Y
                        // +
                        // |\
                        // | \
                        // |  \
                        // +---+
                        if (isClockwise)
                            result |= 3; //static constexpr Diverse::Vec<2, float> Triangle3[3] = { { 0.5f, -0.5f }, { 0.5f, 0.5f }, { -0.5f, -0.5f } };
                        else
                            result |= 6; //static constexpr Diverse::Vec<2, float> Triangle6[3] = { { 0.5f, -0.5f }, { -0.5f, -0.5f }, { 0.5f, 0.5f } };
                    }
                else
                    if (toClosestEdge.Y < 0)
                    { // Postive X, Negative Y
                      // +---+
                      //  \  |
                      //   \ |
                      //    \|
                      //     +
                        if (isClockwise)
                            result |= 1; //static constexpr Diverse::Vec<2, float> Triangle1[3] = { { -0.5f, 0.5f }, { -0.5f, -0.5f }, { 0.5f, 0.5f } };
                        else
                            result |= 4; //static constexpr Diverse::Vec<2, float> Triangle4[3] = { { -0.5f, 0.5f }, { 0.5f, 0.5f }, { -0.5f, -0.5f } };
                    }
                    else
                    { // Postive X, Postive Y
                      //     +
                      //    /|
                      //   / |
                      //  /  |
                      // +---+
                        if (isClockwise)
                            result |= 2; //static constexpr Diverse::Vec<2, float> Triangle2[3] = { { -0.5f, -0.5f }, { 0.5f, -0.5f }, { -0.5f, 0.5f } };
                        else
                            result |= 7; //static constexpr Diverse::Vec<2, float> Triangle7[3] = { { -0.5f, -0.5f }, { -0.5f, 0.5f }, { 0.5f, -0.5f } };
                    }

                // Rotate closest edge to the first place
                if (canRotate)
                    firstTexCoordToEmit = closeEdgeIndex;
            }
            else
            {
                texture.TexCoord0 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord0);
                texture.TexCoord1 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord1);
                texture.TexCoord2 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord2);
                texture.TexCoord3 = HeuristcallyFixTexCoordUpperBound(texture.TexCoord3);

                bool isClockwise = !(texture.QuadArea > 0.0); // 'Not' because Y-axis is flipped in texture space

                // Determine upper left edge
                byte upperLeftIndex = 0;
                float upperLeftScore = texture.TexCoord0.Y + texture.TexCoord0.X;
                for (byte i = 1; i < 4; ++i)
                {
                    float iScore = texture.TexCoord1.Y + texture.GetTexCoord(i).X;
                    if (iScore < upperLeftScore)
                    {
                        upperLeftIndex = i;
                        upperLeftScore = iScore;
                    }
                }

                // Unify orientation depending on orientation
                if (isClockwise)
                { // Unify to mode 0
                    if (canRotate)
                        firstTexCoordToEmit = upperLeftIndex;
                }
                else
                { // Unify to mode 1
                    result |= 1;
                    if (canRotate)
                        firstTexCoordToEmit = (byte)((upperLeftIndex + 3) % 4);
                }
            }
            return result;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private unsafe struct SavedObjectTexture : IEquatable<SavedObjectTexture>
        {
            public ushort TextureID;
            public ushort IsTriangularAndPadding;
            public ushort BlendMode;
            //Bit 0 - 2     mapping correction. It seems that these bits change the way the texture is applied...
            //Bit 11 - 12   the bump mapping type. Can be 0x01 or 0x10. It's 0x00 if not bump mapped. Only textures for room or animated textures can be bump mapped, not meshes
            //Bit 15        if set, the texture is for a tri/quad from a room or animated texture. If not set, the texture is for a mesh
            public ushort NewFlags;
            public ushort TexCoord0X; // C# sucks!!!!!
            public ushort TexCoord0Y;
            public ushort TexCoord1X;
            public ushort TexCoord1Y;
            public ushort TexCoord2X;
            public ushort TexCoord2Y;
            public ushort TexCoord3X;
            public ushort TexCoord3Y;
            public uint TextureSpaceIdentifier;
            private uint Unused;

            public SavedObjectTexture(ushort textureID, TextureArea texture, uint textureSpaceIdentifier, TextureAllocator.TextureView view, bool isTriangular, bool isUsedInRoomMesh, bool canRotate, out byte firstTexCoordToEmit)
            {
                TextureID = textureID;
                IsTriangularAndPadding = isTriangular ? (ushort)1 : (ushort)0;
                BlendMode = (ushort)(texture.BlendMode);
                NewFlags = GetNewFlag(texture, isTriangular, isUsedInRoomMesh, canRotate, out firstTexCoordToEmit);
                TextureSpaceIdentifier = textureSpaceIdentifier;
                Unused = 0;

                // C# sucks!!!!!
                TexCoord0X = TexCoord0Y = 0;
                TexCoord1X = TexCoord1Y = 0;
                TexCoord2X = TexCoord2Y = 0;
                TexCoord3X = TexCoord3Y = 0;

                //Save UV coordinates...
                int edgeCount = isTriangular ? 3 : 4;
                Vector2 lowerBound = new Vector2(127.0f / 256.0f);
                Vector2 upperBound = new Vector2(view.Area.Width, view.Area.Height) - new Vector2(129.0f / 256.0f);
                Vector2 lowerBountUint16 = new Vector2(0.0f);
                Vector2 upperBoundUint16 = new Vector2(view.Area.Width, view.Area.Height) * 256.0f - new Vector2(1.0f);
                Vector2[] texCoordModification = GetTexCoordModificationFromNewFlags(NewFlags, isTriangular);
                for (int i = 0; i < edgeCount; ++i)
                {
                    Vector2 value = texture.GetTexCoord((i + (canRotate ? firstTexCoordToEmit : 0)) % edgeCount);
                    value = Vector2.Max(lowerBound, Vector2.Min(upperBound, value));
                    value -= texCoordModification[i];
                    value *= 256.0f;
                    value = new Vector2((float)Math.Round(value.X), (float)Math.Round(value.Y));
                    value = Vector2.Max(lowerBountUint16, Vector2.Min(upperBoundUint16, value));

                    // C# sucks!!!!!
                    ushort x = (ushort)value.X;
                    ushort y = (ushort)value.Y;
                    switch (i)
                    {
                        case 0:
                            TexCoord0X = x;
                            TexCoord0Y = y;
                            break;
                        case 1:
                            TexCoord1X = x;
                            TexCoord1Y = y;
                            break;
                        case 2:
                            TexCoord2X = x;
                            TexCoord2Y = y;
                            break;
                        case 3:
                            TexCoord3X = x;
                            TexCoord3Y = y;
                            break;
                    }
                }
            }

            public void GetRealTexCoords(out Vector2 texCoord0, out Vector2 texCoord1, out Vector2 texCoord2, out Vector2 texCoord3)
            {
                bool isTriangle = IsTriangularAndPadding != 0;
                Vector2[] texCoordModification = GetTexCoordModificationFromNewFlags(NewFlags, isTriangle);
                texCoord0 = new Vector2(TexCoord0X, TexCoord0Y) * (1.0f / 256.0f) + texCoordModification[0];
                texCoord1 = new Vector2(TexCoord1X, TexCoord1Y) * (1.0f / 256.0f) + texCoordModification[1];
                texCoord2 = new Vector2(TexCoord2X, TexCoord2Y) * (1.0f / 256.0f) + texCoordModification[2];
                texCoord3 = (isTriangle) ? new Vector2() : (new Vector2(TexCoord3X, TexCoord3Y) * (1.0f / 256.0f) + texCoordModification[3]);
            }

            public void GetRealTexRect(out Vector2 minTexCoord, out Vector2 maxTexCoord)
            {
                bool isTriangle = IsTriangularAndPadding != 0;
                Vector2 texCoord0, texCoord1, texCoord2, texCoord3;
                GetRealTexCoords(out texCoord0, out texCoord1, out texCoord2, out texCoord3);
                minTexCoord = Vector2.Min(Vector2.Min(texCoord0, texCoord1), isTriangle ? texCoord2 : Vector2.Min(texCoord2, texCoord3));
                maxTexCoord = Vector2.Max(Vector2.Max(texCoord0, texCoord1), isTriangle ? texCoord2 : Vector2.Max(texCoord2, texCoord3));
            }

            // Custom implementation of these because default implementation is *insanely* slow.
            // Its not just a quite a bit slow, it really is *insanely* *crazy* slow so we need those functions :/
            public static unsafe bool operator ==(SavedObjectTexture first, SavedObjectTexture second)
            {
                ulong* firstPtr = (ulong*)&first;
                ulong* secondPtr = (ulong*)&second;
                return (firstPtr[0] == secondPtr[0]) && (firstPtr[1] == secondPtr[1]) && (firstPtr[2] == secondPtr[2]);
            }

            public static bool operator !=(SavedObjectTexture first, SavedObjectTexture second) => !(first == second);
            public bool Equals(SavedObjectTexture other) => this == other;
            public override bool Equals(object other) => (other is SavedObjectTexture) && this == (SavedObjectTexture)other;
            public override int GetHashCode() => base.GetHashCode();
        }
        private List<SavedObjectTexture> _objectTextures = new List<SavedObjectTexture>();
        private Dictionary<SavedObjectTexture, ushort> _objectTexturesLookup = new Dictionary<SavedObjectTexture, ushort>();
        private uint _textureSpaceIdentifier;
        private int _supportsUpTo65536TextureCount;

        private TextureAllocator _textureAllocator = new TextureAllocator();

        private ushort AddObjectTextureWithoutLookup(SavedObjectTexture newEntry, bool supportsUpTo65536)
        {
            int newID = _objectTextures.Count;
            if (supportsUpTo65536)
            {
                if (newID > 0xffff)
                    throw new ApplicationException("More than 0xffff object textures are not possible for animated textures.");
                _supportsUpTo65536TextureCount += 1;
            }
            else
            {
                if (newID > 0x7fff)
                    throw new ApplicationException("More than 0x7fff object textures that are used for meshes in rooms/movables/statics are not possible.");
            }
            _objectTextures.Add(newEntry);
            return (ushort)(newID);
        }

        private ushort AddOrGetObjectTexture(SavedObjectTexture newEntry, bool supportsUpTo65536, out bool isNew)
        {
            ushort id;
            if (_objectTexturesLookup.TryGetValue(newEntry, out id))
            {
                isNew = false;
                return id;
            }
            id = AddObjectTextureWithoutLookup(newEntry, supportsUpTo65536);
            _objectTexturesLookup.Add(newEntry, id);
            isNew = true;
            return id;
        }

        public Result AddTexture(TextureArea texture, bool isTriangle, bool isUsedInRoomMesh, int packPriorityClass = 0, bool supportsUpTo65536 = false, bool canRotate = true, uint textureSpaceIdentifier = 0)
        {
            // Add object textures
            int textureID = _textureAllocator.GetOrAllocateTextureID(ref texture, isTriangle, packPriorityClass);
            bool isNew;
            byte firstTexCoordToEmit;
            ushort objTexIndex = AddOrGetObjectTexture(new SavedObjectTexture((ushort)textureID, texture, textureSpaceIdentifier,
                _textureAllocator.GetTextureFromID(textureID), isTriangle, isUsedInRoomMesh, canRotate, out firstTexCoordToEmit), supportsUpTo65536, out isNew);
            return new Result { ObjectTextureIndex = objTexIndex, FirstVertexIndexToEmit = firstTexCoordToEmit,
                Flags = (texture.DoubleSided ? ResultFlags.DoubleSided : ResultFlags.None) | (isNew ? ResultFlags.IsNew : ResultFlags.None) };
        }

        protected virtual void OnPackingTextures(IProgressReporter progressReporter)
        {}

        private volatile int _alphaBlendingTextureCount;
        public List<ImageC> PackTextures(IProgressReporter progressReporter)
        {
            //Add not yet required object textures to texture animations...
            OnPackingTextures(progressReporter);

            // Enable alpha blending for faces whose textures are not completely opaque.
            Parallel.For(0, _objectTextures.Count, objectTextureIndex =>
            {
                SavedObjectTexture objectTexture = _objectTextures[objectTextureIndex];
                if (objectTexture.BlendMode != (ushort)BlendMode.Normal) // Only consider alpha blending when blend mode is 0.
                    return;

                bool isTriangle = objectTexture.IsTriangularAndPadding != 0;
                TextureAllocator.TextureView textureView = _textureAllocator.GetTextureFromID(objectTexture.TextureID);

                // To simplify the test just use the rectangular region around. (We could do a polygonal thing but I am not sure its worth it)
                Vector2 minTexCoord, maxTexCoord;
                objectTexture.GetRealTexRect(out minTexCoord, out maxTexCoord);
                int startX = textureView.Area.X0 + (int)Math.Floor(minTexCoord.X);
                int startY = textureView.Area.Y0 + (int)Math.Floor(minTexCoord.Y);
                int endX = textureView.Area.X0 + (int)Math.Ceiling(maxTexCoord.X);
                int endY = textureView.Area.Y0 + (int)Math.Ceiling(maxTexCoord.Y);

                // Check for alpha
                bool hasAlpha = textureView.Texture.Image.HasAlpha(startX, startY, endX - startX, endY - startY);

                // Enable alpha if necessary
                if (hasAlpha)
                {
                    objectTexture.BlendMode = 1;
                    _objectTextures[objectTextureIndex] = objectTexture;
                    Interlocked.Increment(ref _alphaBlendingTextureCount); // Thread safe increment because Parallel.For
                }
            });
            progressReporter.ReportInfo("Alpha blending information built. ( Alpha blending enabled for " + _alphaBlendingTextureCount + " of " + _objectTextures.Count + " object textures)");
            progressReporter.ReportInfo("Count of general purpose object textures: " + (_objectTextures.Count - _supportsUpTo65536TextureCount) + " (The maximum is 32768)\n");
            progressReporter.ReportInfo("Count of extended object textures: " + _objectTextures.Count + " (The maximum is 65536)\n");

            //progressReporter.ReportInfo("Count of animated textures: " + AnimationsCache.Count);

            // Debug object textures (if used)
            DebugObjectTextures();

            // Pack textures...
            List<ImageC> result = _textureAllocator.PackTextures();
            progressReporter.ReportInfo("Packed all level and wad textures into " + result.Count + " pages.");
            return result;
        }

        public uint GetNewTextureSpaceIdentifier() => ++_textureSpaceIdentifier;

        public int ObjectTextureCount => _objectTextures.Count;

        private void DebugObjectTextures()
        {
            /*float zoomSpeed = (float)Math.Sqrt(2);
            float moveSpeed = 160.0f;

            float posX = 128;
            float posY = 128;
            float scale = 12;
            int currentlyDisplayedObjectTexture = 0;
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(form, true);
            form.Paint += delegate (object sender, System.Windows.Forms.PaintEventArgs e)
                {
                    SavedObjectTexture objectTexture = _objectTextures[currentlyDisplayedObjectTexture];
                    bool isTriangle = objectTexture.IsTriangularAndPadding != 0;

                    // Camera transform
                    e.Graphics.ResetTransform();
                    e.Graphics.TranslateTransform(-posX, -posY, System.Drawing.Drawing2D.MatrixOrder.Append);
                    e.Graphics.ScaleTransform(scale, scale, System.Drawing.Drawing2D.MatrixOrder.Append);
                    e.Graphics.TranslateTransform(form.Width * 0.5f, form.Height * 0.5f, System.Drawing.Drawing2D.MatrixOrder.Append);

                    // Draw texture
                    TextureAllocator.TextureView texture = _textureAllocator.GetTextureFromID(objectTexture.TextureID);

                    // Directly drawing a bitmap makes the pixels on the border cut off.
                    // This is very confusing.
                    //e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    //texture.Texture.Image.GetTempSystemDrawingBitmap((tempBitmap) =>
                    //    {
                    //        e.Graphics.DrawImage(tempBitmap,
                    //            new System.Drawing.RectangleF(0, 0, texture.Width, texture.Height),
                    //            new System.Drawing.RectangleF(texture.PosX, texture.PosY, texture.Width, texture.Height),
                    //            System.Drawing.GraphicsUnit.Pixel);
                    //    });

                    for (int y = 0; y < texture.Height; ++y)
                        for (int x = 0; x < texture.Width; ++x)
                        {
                            ColorC color = texture.Texture.Image.GetPixel(x, y);
                            using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B)))
                                e.Graphics.FillRectangle(brush, new System.Drawing.RectangleInt2(x, y, 1, 1));
                        }

                    // Draw virtual coordinates
                    List<System.Drawing.PointF> virtualCoords = new List<System.Drawing.PointF>();
                    virtualCoords.Add(new System.Drawing.PointF(objectTexture.TexCoord0X / 256.0f, objectTexture.TexCoord0Y / 256.0f));
                    virtualCoords.Add(new System.Drawing.PointF(objectTexture.TexCoord1X / 256.0f, objectTexture.TexCoord1Y / 256.0f));
                    virtualCoords.Add(new System.Drawing.PointF(objectTexture.TexCoord2X / 256.0f, objectTexture.TexCoord2Y / 256.0f));
                    if (!isTriangle)
                        virtualCoords.Add(new System.Drawing.PointF(objectTexture.TexCoord3X / 256.0f, objectTexture.TexCoord3Y / 256.0f));
                    using (var pen = new System.Drawing.Pen(System.Drawing.Color.LightBlue, 0.1f))
                        e.Graphics.DrawPolygon(pen, virtualCoords.ToArray());

                    // Draw real tex coordinates
                    Vector2 texCoord0, texCoord1, texCoord2, texCoord3;
                    objectTexture.GetRealTexCoords(out texCoord0, out texCoord1, out texCoord2, out texCoord3);
                    List<System.Drawing.PointF> realCoords = new List<System.Drawing.PointF>();
                    realCoords.Add(new System.Drawing.PointF(texCoord0.X, texCoord0.Y));
                    realCoords.Add(new System.Drawing.PointF(texCoord1.X, texCoord1.Y));
                    realCoords.Add(new System.Drawing.PointF(texCoord2.X, texCoord2.Y));
                    if (!isTriangle)
                        realCoords.Add(new System.Drawing.PointF(texCoord3.X, texCoord3.Y));
                    using (var pen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 0.1f))
                        e.Graphics.DrawPolygon(pen, realCoords.ToArray());

                    // Draw information
                    e.Graphics.ResetTransform();
                    string informationString = "Object texture index: " + currentlyDisplayedObjectTexture + "\n";
                    foreach (System.Reflection.FieldInfo fieldInfo in typeof(SavedObjectTexture).GetFields())
                    {
                        var value = fieldInfo.GetValue(objectTexture);
                        var formatableValue = value as IFormattable;
                        string valueStr = formatableValue != null ? "0x" + formatableValue.ToString("X", null) : value.ToString();
                        informationString += fieldInfo.Name + ": " + valueStr + "\n";
                    }
                    var stringSize = e.Graphics.MeasureString(informationString, form.Font);
                    var stringArea = new System.Drawing.RectangleF(10, 10, stringSize.Width, stringSize.Height);
                    using (var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(200, 255, 255, 255)))
                        e.Graphics.FillRectangle(brush, stringArea);
                    e.Graphics.DrawString(informationString, form.Font, System.Drawing.Brushes.Black, stringArea);
                };
            form.KeyDown += delegate (object sender, System.Windows.Forms.KeyEventArgs e)
                {
                    switch (e.KeyCode)
                    {
                        case System.Windows.Forms.Keys.Home:
                            currentlyDisplayedObjectTexture = 0;
                            goto CenterView;
                        case System.Windows.Forms.Keys.End:
                            currentlyDisplayedObjectTexture = _objectTextures.Count - 1;
                            goto CenterView;
                        case System.Windows.Forms.Keys.Enter:
                            currentlyDisplayedObjectTexture = Math.Min(currentlyDisplayedObjectTexture + 1, _objectTextures.Count - 1);
                            goto CenterView;
                        case System.Windows.Forms.Keys.Back:
                            currentlyDisplayedObjectTexture = Math.Max(currentlyDisplayedObjectTexture - 1, 0);
                            goto CenterView;

                        case System.Windows.Forms.Keys.PageDown:
                            scale /= (float)Math.Sqrt(2);
                            form.Invalidate();
                            break;
                        case System.Windows.Forms.Keys.PageUp:
                            scale *= (float)Math.Sqrt(2);
                            form.Invalidate();
                            break;
                        case System.Windows.Forms.Keys.Left:
                            posX -= moveSpeed / scale;
                            form.Invalidate();
                            break;
                        case System.Windows.Forms.Keys.Right:
                            posX += moveSpeed / scale;
                            form.Invalidate();
                            break;
                        case System.Windows.Forms.Keys.Up:
                            posY -= moveSpeed / scale;
                            form.Invalidate();
                            break;
                        case System.Windows.Forms.Keys.Down:
                            posY += moveSpeed / scale;
                            form.Invalidate();
                            break;

                        case System.Windows.Forms.Keys.Escape:
                            form.Close();
                            break;
                    }
                    return;

                    // Center view
                    CenterView:
                    Vector2 minTexCoord, maxTexCoord;
                    _objectTextures[currentlyDisplayedObjectTexture].GetRealTexRect(out minTexCoord, out maxTexCoord);
                    posX = (minTexCoord.X + maxTexCoord.X) * 0.5f;
                    posY = (minTexCoord.Y + maxTexCoord.Y) * 0.5f;

                    form.Invalidate();
                };
            form.ShowDialog();*/
        }

        public void WriteObjectTextures(BinaryWriterEx stream, Level level)
        {
            // Write object textures
            stream.Write((uint)_objectTextures.Count);
            for (int i = 0; i < _objectTextures.Count; ++i)
            {
                SavedObjectTexture objectTexture = _objectTextures[i];
                TextureAllocator.Result UsedTexturePackInfo = _textureAllocator.GetPackInfo(objectTexture.TextureID);
                ushort Tile = UsedTexturePackInfo.OutputTextureID;
                if (level.Settings.GameVersion != GameVersion.TR2)
                    Tile |= (objectTexture.IsTriangularAndPadding != 0) ? (ushort)0x8000 : (ushort)0;

                stream.Write((ushort)objectTexture.BlendMode);
                stream.Write((ushort)Tile);

                if (level.Settings.GameVersion == GameVersion.TR4 || level.Settings.GameVersion == GameVersion.TRNG ||
                                    level.Settings.GameVersion == GameVersion.TR5)
                    stream.Write((ushort)objectTexture.NewFlags);

                UsedTexturePackInfo.TransformTexCoord(ref objectTexture.TexCoord0X, ref objectTexture.TexCoord0Y);
                UsedTexturePackInfo.TransformTexCoord(ref objectTexture.TexCoord1X, ref objectTexture.TexCoord1Y);
                UsedTexturePackInfo.TransformTexCoord(ref objectTexture.TexCoord2X, ref objectTexture.TexCoord2Y);
                UsedTexturePackInfo.TransformTexCoord(ref objectTexture.TexCoord3X, ref objectTexture.TexCoord3Y);

                stream.Write((ushort)objectTexture.TexCoord0X);
                stream.Write((ushort)objectTexture.TexCoord0Y);
                stream.Write((ushort)objectTexture.TexCoord1X);
                stream.Write((ushort)objectTexture.TexCoord1Y);
                stream.Write((ushort)objectTexture.TexCoord2X);
                stream.Write((ushort)objectTexture.TexCoord2Y);
                stream.Write((ushort)objectTexture.TexCoord3X);
                stream.Write((ushort)objectTexture.TexCoord3Y);

                if (level.Settings.GameVersion == GameVersion.TR4 || level.Settings.GameVersion == GameVersion.TRNG || level.Settings.GameVersion == GameVersion.TR5)
                {
                    stream.Write((uint)0);
                    stream.Write((uint)0);
                    stream.Write((uint)0);
                    stream.Write((uint)0);
                }

                if (level.Settings.GameVersion == GameVersion.TR5)
                    stream.Write((ushort)0);
            }
        }
    }
}
