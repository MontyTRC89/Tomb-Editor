namespace TombLib.Utils
{
    public class TreePacker : RectPacker
    {
        private class TreePackerNode
        {
            public TreePackerNode Left { get; set; }
            public TreePackerNode Right { get; set; }
            public RectangleInt2 Rectangle { get; set; }
            public bool Filled { get; set; } = false;

            private bool RectangleFitsIn(VectorInt2 size)
            {
                return Rectangle.Width >= size.X && Rectangle.Height >= size.Y;
            }

            private bool RectangleSameSizeOf(VectorInt2 size)
            {
                return Rectangle.Width == size.X && Rectangle.Height == size.Y;
            }

            public TreePackerNode InsertNode(VectorInt2 size)
            {
                if (Left != null)
                {
                    var node = Left.InsertNode(size);
                    if (node == null)
                        return Right.InsertNode(size);
                    else
                        return node;
                }

                if (Filled)
                    return null;

                if (!RectangleFitsIn(size))
                    return null;

                if (RectangleSameSizeOf(size))
                {
                    Filled = true;
                    return this;
                }

                Left = new TreePackerNode();
                Right = new TreePackerNode();

                var widthDifference = Rectangle.Width - size.X;
                var heightDifference = Rectangle.Height - size.Y;
                if (widthDifference > heightDifference)
                {
                    Left.Rectangle = new RectangleInt2(Rectangle.X0, Rectangle.Y0, Rectangle.X0 + size.X, Rectangle.Y1);
                    Right.Rectangle = new RectangleInt2(Rectangle.X0 + size.X, Rectangle.Y0, Rectangle.X1, Rectangle.Y1);
                }
                else
                {
                    Left.Rectangle = new RectangleInt2(Rectangle.X0, Rectangle.Y0, Rectangle.X1, Rectangle.Y0 + size.Y);
                    Right.Rectangle = new RectangleInt2(Rectangle.X0, Rectangle.Y0 + size.Y, Rectangle.X1, Rectangle.Y1);
                }

                return Left.InsertNode(size);
            }
        }

        private TreePackerNode _startNode;

        public TreePacker(VectorInt2 size)
            : base(size)
        {
            _startNode = new TreePackerNode();
            _startNode.Rectangle = new RectangleInt2(new VectorInt2(), size);
        }

        public override VectorInt2? TryAdd(VectorInt2 size)
        {
            var node = _startNode.InsertNode(size);
            if (node == null)
                return null;
            return node.Rectangle.Start;
        }
    }
}
