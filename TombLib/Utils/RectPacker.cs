using System;

namespace TombLib.Utils
{
    public abstract class RectPacker
    {
        public VectorInt2 Size { get; protected set; }

        protected RectPacker(VectorInt2 size)
        {
            Size = size;
        }
        public abstract VectorInt2? TryAdd(VectorInt2 size);
    }

    public class RectPackerSimpleStack : RectPacker
    {
        private int _currentX;
        private int _currentY;
        private int _stackHeight;

        public RectPackerSimpleStack(VectorInt2 size)
            : base(size)
        { }

        public override VectorInt2? TryAdd(VectorInt2 size)
        {
            if (_currentY + size.Y > Size.Y)
                return null;

            if (_currentX + size.X > Size.X)
            { // Does not fit in that row, but maybe in a new row
                if (size.X > Size.X)
                    return null;
                if (_currentY + _stackHeight + size.Y > Size.Y)
                    return null;
                _currentX = 0;
                _currentY = _currentY + _stackHeight;
                _stackHeight = 0;
            }

            // Pack and adjust coordinates
            VectorInt2 result = new VectorInt2(_currentX, _currentY);
            _stackHeight = Math.Max(_stackHeight, size.Y);
            _currentX += size.X;
            return result;
        }
    }
}
