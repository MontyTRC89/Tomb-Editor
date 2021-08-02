using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Utils
{
    public class VertexNormalAverageHelper
    {
        private List<Vector3> _positions;
        private List<Vector3> _normals;
        private List<int> _counts;

        public VertexNormalAverageHelper(List<Vector3> positions)
        {
            _positions = new List<Vector3>(positions);
            _normals = new List<Vector3>();
            _counts = new List<int>();

            for (int i = 0; i < _positions.Count; i++)
            {
                _normals.Add(Vector3.Zero);
                _counts.Add(0);
            }
        }

        public void AddPolygon(bool weighted, int index1, int index2, int index3, int index4 = -1)
        {
            if (index1 >= _positions.Count ||
                index1 >= _positions.Count ||
                index3 >= _positions.Count ||
                index4 >= _positions.Count)
                return;

            var p0 = _positions[index1];
            var p1 = _positions[index2];
            var p2 = _positions[index3];

            var v1 = p1 - p0;
            var v2 = p2 - p0;
            var n = Vector3.Cross(v1, v2);

            if (weighted)
                n = Vector3.Normalize(n);

            // Get the angle between the two other points for each point;
            // The starting point will be the 'base' and the two adjacent points will be normalized against it

            var a1 = (p1 - p0).Angle(p2 - p0);
            var a2 = (p2 - p1).Angle(p0 - p1);
            var a3 = (p0 - p2).Angle(p1 - p2);

            _normals[index1] += weighted ? Vector3.Multiply(a1, n) : n;
            _counts[index1]++;

            _normals[index2] += weighted ? Vector3.Multiply(a2, n) : n;
            _counts[index2]++;

            _normals[index3] += weighted ? Vector3.Multiply(a3, n) : n;
            _counts[index3]++;

            if (index4 >= 0)
            {
                _normals[index4] += weighted ? Vector3.Multiply(a3, n) : n;
                _counts[index4]++;
            }
        }

        public List<Vector3> CalculateNormals()
        {
            var result = new List<Vector3>();

            for (int i = 0; i < _normals.Count; i++)
            {
                var normal = _normals[i] / Math.Max(1, _counts[i]);
                normal = Vector3.Normalize(normal);
                result.Add(normal);
            }

            return result;
        }
    }
}
