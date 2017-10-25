using System;
using System.Collections.Generic;
using System.Drawing;
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
            public Rectangle Rectangle { get; set; }
            public bool Filled { get; set; } = false;

            private bool RectangleFitsIn(Rectangle rect)
            {
                return Rectangle.Width >= rect.Width && Rectangle.Height >= rect.Height;
            }

            private bool RectangleSameSizeOf(Rectangle rect)
            {
                return Rectangle.Width == rect.Width && Rectangle.Height == rect.Height;
            }

            public TreePackerNode InsertNode(Rectangle rect)
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

                var widthDifference = Rectangle.Width - rect.Width;
                var heightDifference = Rectangle.Height - rect.Height;

                if (widthDifference > heightDifference)
                {
                    Left.Rectangle = new Rectangle(Rectangle.X, Rectangle.Y, rect.Width, Rectangle.Height);
                    Right.Rectangle = new Rectangle(Rectangle.X + rect.Width, Rectangle.Y,
                                                    Rectangle.Width - rect.Width, Rectangle.Height);
                }
                else
                {
                    Left.Rectangle = new Rectangle(Rectangle.X, Rectangle.Y, Rectangle.Width, rect.Height);
                    Right.Rectangle = new Rectangle(Rectangle.X, Rectangle.Y + rect.Height,
                                                    Rectangle.Width, Rectangle.Height - rect.Height);
                }

                return Left.InsertNode(rect);
            }
        }

        private TreePackerNode _startNode;

        public TreePacker(int width, int height)
            : base(width, height)
        {
            _startNode = new TreePackerNode();
            _startNode.Rectangle = new Rectangle(0, 0, width, height);
        }

        public override Point? TryAdd(int width, int height)
        {
            var rect = new Rectangle(0, 0, width, height);
            var node = _startNode.InsertNode(rect);
            if (node == null) return null;
            var result = new Point(node.Rectangle.X, node.Rectangle.Y);
            return result;
        }
    }
}
