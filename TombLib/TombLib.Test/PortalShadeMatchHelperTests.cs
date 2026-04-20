using System.Numerics;
using System.Reflection;
using TombLib.LevelData.Compilers;
using TombLib;

namespace TombLib.Test;

[TestClass]
public class PortalShadeMatchHelperTests
{
    [TestMethod]
    public void IsCandidate_RejectsInteriorPortalVertex()
    {
        var legacyPortalVertices = new[]
        {
            new tr_vertex(0, 0, 0),
            new tr_vertex(4, 2, 0),
            new tr_vertex(4, 2, 4),
            new tr_vertex(0, 0, 4)
        };

        var tombEnginePortalVertices = new[]
        {
            new VectorInt3(0, 0, 0),
            new VectorInt3(4, 2, 0),
            new VectorInt3(4, 2, 4),
            new VectorInt3(0, 0, 4)
        };

        Assert.IsFalse(InvokeLegacyCandidate(legacyPortalVertices, new tr_vertex(2, 1, 2)));
        Assert.IsFalse(InvokeTombEngineCandidate(tombEnginePortalVertices, new Vector3(2.0f, 1.0f, 2.0f)));
    }

    [TestMethod]
    public void IsCandidate_AcceptsPortalEdgeVertex()
    {
        var legacyPortalVertices = new[]
        {
            new tr_vertex(0, 0, 0),
            new tr_vertex(4, 2, 0),
            new tr_vertex(4, 2, 4),
            new tr_vertex(0, 0, 4)
        };

        var tombEnginePortalVertices = new[]
        {
            new VectorInt3(0, 0, 0),
            new VectorInt3(4, 2, 0),
            new VectorInt3(4, 2, 4),
            new VectorInt3(0, 0, 4)
        };

        Assert.IsTrue(InvokeLegacyCandidate(legacyPortalVertices, new tr_vertex(2, 1, 0)));
        Assert.IsTrue(InvokeTombEngineCandidate(tombEnginePortalVertices, new Vector3(2.0f, 1.0f, 0.0f)));
    }

    private static bool InvokeLegacyCandidate(tr_vertex[] portalVertices, tr_vertex vertexPosition)
    {
        var helperType = typeof(ShadeMatchSignature).Assembly.GetType("TombLib.LevelData.Compilers.PortalShadeMatchHelper", true)!;
        var method = helperType.GetMethod("IsCandidate", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(tr_vertex[]), typeof(tr_vertex) }, null)!;
        return (bool)method.Invoke(null, new object[] { portalVertices, vertexPosition })!;
    }

    private static bool InvokeTombEngineCandidate(VectorInt3[] portalVertices, Vector3 vertexPosition)
    {
        var helperType = typeof(ShadeMatchSignature).Assembly.GetType("TombLib.LevelData.Compilers.PortalShadeMatchHelper", true)!;
        var method = helperType.GetMethod("IsCandidate", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(VectorInt3[]), typeof(Vector3) }, null)!;
        return (bool)method.Invoke(null, new object[] { portalVertices, vertexPosition })!;
    }
}
