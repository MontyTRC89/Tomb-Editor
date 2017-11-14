using NLog;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.GeometryIO.Importers
{
    public class MetasequoiaRoomImporter : BaseGeometryImporter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public MetasequoiaRoomImporter(IOGeometrySettings settings, GetTextureDelegate getTextureCallback) 
            : base(settings, getTextureCallback)
        {

        }

        public override IOModel ImportFromFile(string filename)
        {
            var model = new IOModel();
            var mesh = new IOMesh();
            var materials = new List<string>();
            var positions = new List<Vector3>();
            var textures = new Dictionary<int, Texture>();
            var lastVertex = 0;

            using (var reader = new StreamReader(File.OpenRead(filename)))
            {
                var line = reader.ReadLine();
                if (line.Trim() != "Metasequoia Document")
                {
                    logger.Error("Not a valid Metasequoia file");
                    return null;
                }

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine().Trim();
                    if (line == "" || line == "}") continue;

                    // Parse chunks
                    var chunk = line.Split(' ')[0];
                    if (chunk == "Format")
                    {
                        continue;
                    }
                    else if (chunk == "Thumbnail")
                    {
                        IgnoreChunk(reader);
                        continue;
                    }
                    else if (chunk == "Scene")
                    {
                        IgnoreChunk(reader);
                        continue;
                    }
                    else if (chunk == "Material")
                    {
                        var numMaterials = int.Parse(line.Split(' ')[1]);
                        if (numMaterials == 0) return null;
                        for (var i = 0; i < numMaterials; i++)
                        {
                            var materialString = reader.ReadLine().Trim();
                            var tokensMaterial = materialString.Split(' ');
                            for (var j = 0; j < tokensMaterial.Length; j++)
                            {
                                var texturePath = "";
                                if (tokensMaterial[j].StartsWith("tex"))
                                {
                                    texturePath = tokensMaterial[j].Substring(5, tokensMaterial[j].Length - 7);
                                    break;
                                }
                                if (texturePath != "")
                                    textures.Add(j, GetTexture(Path.GetDirectoryName(filename), texturePath));
                            }
                        }
                        continue;
                    }
                    else if (chunk == "Object")
                    {
                        while (!reader.EndOfStream)
                        {
                            line = reader.ReadLine().Trim();
                            var tokens = line.Split(' ');

                            if (tokens[0] == "vertex")
                            {
                                var numVertices = int.Parse(tokens[1]);
                                for (var i = 0; i < numVertices; i++)
                                {
                                    var tokensPosition = reader.ReadLine().Trim().Split(' ');
                                    positions.Add(ApplyAxesTransforms(new Vector3(float.Parse(tokensPosition[0]),
                                                                      float.Parse(tokensPosition[1]),
                                                                      float.Parse(tokensPosition[2]))));
                                }
                            }
                            else if (tokens[0] == "face")
                            {
                                var numFaces = int.Parse(tokens[1]);
                                for (var i = 0; i < numFaces; i++)
                                {
                                    var tokensFace = reader.ReadLine().Trim().Split(' ');
                                    var numVerticesInFace = int.Parse(tokensFace[0]);
                                    var poly = new IOPolygon(numVerticesInFace == 3 ? IOPolygonShape.Triangle : IOPolygonShape.Quad);

                                    for (var j = 1; j < tokensFace.Length; j++)
                                    {
                                        if (tokensFace[j].StartsWith("V("))
                                        {
                                            lastVertex = 0;
                                            var tokensVertices = tokensFace[j].Replace("V(", "").Replace(")", "").Split(' ');
                                            for (var k = 0; k < numVerticesInFace; k++)
                                            {
                                                var index = int.Parse(tokensVertices[k]);
                                                mesh.Positions.Add(positions[index]);
                                                poly.Indices.Add(lastVertex);
                                                lastVertex++;
                                            }
                                        }
                                        else if (tokensFace[j].StartsWith("M("))
                                        {
                                            var material = int.Parse(tokensFace[j].Replace("M(", "").Replace(")", ""));
                                        }
                                        if (tokensFace[j].StartsWith("UV("))
                                        {
                                            var tokensVertices = tokensFace[j].Replace("UV(", "").Replace(")", "").Split(' ');
                                            for (var k = 0; k < numVerticesInFace; k++)
                                            {
                                                var uv = ApplyUVTransform(new Vector2(float.Parse(tokensVertices[2 * k]),
                                                                                      float.Parse(tokensVertices[2 * k])),
                                                                          textures[0].Image.Width,
                                                                          textures[0].Image.Height);
                                                mesh.UV.Add(uv);
                                            }
                                        }
                                        if (tokensFace[j].StartsWith("COL("))
                                        {
                                            var tokensVertices = tokensFace[j].Replace("COL(", "").Replace(")", "").Split(' ');
                                            for (var k = 0; k < numVerticesInFace; k++)
                                            {
                                                var color = ApplyColorTransform(GetColor(int.Parse(tokensVertices[k])));
                                                mesh.Colors.Add(color);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                model.Meshes.Add(mesh);
            }        

            return model;
        }

        private Vector4 GetColor(int color)
        {
            var r = (float)(color & 0xFF);
            var g = (float)((color >> 8) & 0xFF);
            var b = (float)((color >> 16) & 0xFF);

            r /= 128.0f;
            g /= 128.0f;
            b /= 128.0f;

            return new Vector4(r, g, b, 1.0f);
        }

        private void IgnoreChunk(StreamReader reader)
        {
            var depth = 1;
            while (!reader.EndOfStream)
            {
                if (depth == 0) return;
                var line = reader.ReadLine();
                if (line.Contains("{")) { depth++; continue; }
                if (line.Contains("}")) { depth--; continue; }
            }
        }
    }
}
