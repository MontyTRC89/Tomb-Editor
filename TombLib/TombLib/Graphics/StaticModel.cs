using System.Numerics;

namespace TombLib.Graphics
{
    // CPU-side static model — a flat list of meshes with no animation/skinning.
    public class StaticModel : Model<ObjectMesh, ObjectVertex>
    {
        public StaticModel()
            : base(ModelType.Static)
        { }

        public override void UpdateBuffers(Vector3? position = null)
        {
            foreach (var mesh in Meshes)
            {
                mesh.UpdateBoundingBox();
                mesh.UpdateBuffers(position);
            }
        }
    }
}
