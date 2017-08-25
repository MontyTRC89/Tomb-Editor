using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public abstract class RectPacker
    {
        public struct Point
        {
            public int X;
            public int Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public int Width { get; protected set; }
        public int Height { get; protected set; }

        protected RectPacker(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public abstract Point? TryAdd(int width, int height);
    }

    public class RectPackerSimpleStack : RectPacker
    {
        private int _currentX = 0;
        private int _currentY = 0;
        private int _stackHeight = 0;

        public RectPackerSimpleStack(int width, int height)
            : base(width, height)
        { }

        public override Point? TryAdd(int width, int height)
        {
            if ((_currentY + height) > Height)
                return null;

            if ((_currentX + width) > Width)
            { // Does not fit in that row, but maybe in a new row
                if (width > Width)
                    return null;
                if ((_currentY + _stackHeight + height) > Height)
                    return null;
                _currentX = 0;
                _currentY = _currentY + _stackHeight;
                _stackHeight = 0;
            }

            // Pack and adjust coordinates
            Point result = new Point(_currentX, _currentY);
            _stackHeight = Math.Max(_stackHeight, height);
            _currentX += width;
            return result;
        }
    }
}
