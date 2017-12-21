using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public class TreePacker : RectPacker
    {
        private class TreePackerNode
        {
            public TreePackerNode Left { get; set; }
            public TreePackerNode Right { get; set; }
            public RectangleInt2 RectangleInt2 { get; set; }
            public bool Filled { get; set; } = false;

            private bool RectangleFitsIn(RectangleInt2 rect)
            {
                return RectangleInt2.Width >= rect.Width && RectangleInt2.Height >= rect.Height;
            }

            private bool RectangleSameSizeOf(RectangleInt2 rect)
            {
                return RectangleInt2.Width == rect.Width && RectangleInt2.Height == rect.Height;
            }

            public TreePackerNode InsertNode(RectangleInt2 rect)
            {
                if (Left != null)
                {
                    var node = Left.InsertNode(rect);
                    if (node == null)
                        return Right.InsertNode(rect);
                    else
                        return node;
                }

                if (Filled)
                    return null;

                if (!RectangleFitsIn(rect))
                    return null;

                if (RectangleSameSizeOf(rect))
                {
                    Filled = true;
                    return this;
                }

                Left = new TreePackerNode();
                Right = new TreePackerNode();

                var widthDifference = RectangleInt2.Width - rect.Width;
                var heightDifference = RectangleInt2.Height - rect.Height;

                if (widthDifference > heightDifference)
                {
                    Left.RectangleInt2 = RectangleInt2.FromLTRB(RectangleInt2.X0,  RectangleInt2.Y0, RectangleInt2.X0 + rect.Width, RectangleInt2.Y0 + RectangleInt2.Height);
                    Right.RectangleInt2 = RectangleInt2.FromLTRB(RectangleInt2.X0 + rect.Width, RectangleInt2.Y0, RectangleInt2.Width - rect.Width, RectangleInt2.Height);
                }
                else
                {
                    Left.RectangleInt2 = RectangleInt2.FromLTRB(RectangleInt2.X0, RectangleInt2.Y0, RectangleInt2.Width, rect.Height);
                    Right.RectangleInt2 = RectangleInt2.FromLTRB(RectangleInt2.X0, RectangleInt2.Y0 + rect.Height, RectangleInt2.Width, RectangleInt2.Height - rect.Height);
                }

                return Left.InsertNode(rect);
            }
        }

        private TreePackerNode _startNode;
        private int _maxHeight = 0;

        public override int MaxHeight { get { return _maxHeight; } }

        public TreePacker(int width, int height)
            : base(width, height)
        {
            _startNode = new TreePackerNode();
            _startNode.RectangleInt2 = new RectangleInt2(0, 0, width, height);
            _maxHeight = 0;
        }

        public override Point? TryAdd(int width, int height)
        {
            var rect = new RectangleInt2(0, 0, width, height);
            var node = _startNode.InsertNode(rect);
            if (node == null) return null;
            var result = new Point(node.RectangleInt2.X0, node.RectangleInt2.Y0);
            var newHeight = node.RectangleInt2.Y0 + node.RectangleInt2.Height;
            if (newHeight > _maxHeight) _maxHeight = newHeight;
            return result;
        }
    }
}
