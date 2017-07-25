using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace TombLib.Graphics
{
    public abstract class Camera
    {
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public void GeneratePerspectiveProjectionMatrix(float FieldOfView, float width, float height)
        {
            float aspectRatio = width / height;
            this.Projection = Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(50), aspectRatio, 10.0f, 100000.0f);
        }

        public void GenerateOrthoProjectionMatrix(float width, float height)
        {
            this.Projection = Matrix.OrthoLH(width * 10, height * 10, 10.0f, 100000.0f);
        }

        public virtual void Update()
        { }
    }
}
