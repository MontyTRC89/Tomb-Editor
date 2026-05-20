using System;
using System.IO;
using System.Numerics;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Test;

[TestClass]
public class ImportedGeometryTests
{
    [TestMethod]
    public void UpdateBuffers_UpdatesBoundingBoxesAndVersion()
    {
        var model = new ImportedGeometry.Model(1.0f);
        var material = new Material("TestMaterial");
        model.Materials.Add(material);

        var mesh = new ImportedGeometryMesh("TestMesh");
        mesh.Vertices.Add(CreateVertex(new Vector3(0.0f, 0.0f, 0.0f)));
        mesh.Vertices.Add(CreateVertex(new Vector3(1024.0f, 0.0f, 0.0f)));
        mesh.Vertices.Add(CreateVertex(new Vector3(0.0f, 1024.0f, 0.0f)));

        var submesh = new Submesh(material);
        submesh.Indices.AddRange(new[] { 0, 1, 2 });
        mesh.Submeshes.Add(material, submesh);
        model.Meshes.Add(mesh);

        model.UpdateBuffers();
        var firstVersion = model.Version;

        Assert.AreEqual(new Vector3(0.0f, 0.0f, 0.0f), mesh.BoundingBox.Minimum);
        Assert.AreEqual(new Vector3(1024.0f, 1024.0f, 0.0f), mesh.BoundingBox.Maximum);
        Assert.AreEqual(mesh.BoundingBox.Minimum, model.BoundingBox.Minimum);
        Assert.AreEqual(mesh.BoundingBox.Maximum, model.BoundingBox.Maximum);
        CollectionAssert.AreEqual(new[] { 0, 1, 2 }, mesh.Indices);

        mesh.Vertices[0] = CreateVertex(new Vector3(-1024.0f, 0.0f, 0.0f));
        model.UpdateBuffers();

        Assert.IsTrue(model.Version > firstVersion);
        Assert.AreEqual(new Vector3(-1024.0f, 0.0f, 0.0f), model.BoundingBox.Minimum);
        Assert.AreEqual(new Vector3(1024.0f, 1024.0f, 0.0f), model.BoundingBox.Maximum);
    }

    [TestMethod]
    public void Assign_UpdatesTextureVersion()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
        ImageC.CreateNew(2, 2).SaveToFile(fileName);

        try
        {
            var original = new ImportedGeometryTexture(fileName);
            var reloaded = new ImportedGeometryTexture(fileName);
            var originalVersion = original.Version;

            original.Assign(reloaded);

            Assert.IsTrue(original.Version > originalVersion);
            Assert.AreEqual(reloaded.AbsolutePath, original.AbsolutePath);
            Assert.AreEqual(reloaded.Image.Width, original.Image.Width);
            Assert.AreEqual(reloaded.Image.Height, original.Image.Height);
        }
        finally
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
        }
    }

    private static ImportedGeometryVertex CreateVertex(Vector3 position)
    {
        return new ImportedGeometryVertex
        {
            Position = position,
            UV = Vector2.Zero,
            Color = Vector3.One,
            Normal = Vector3.UnitY
        };
    }
}
