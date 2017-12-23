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
            var materials = new List<string>();
            var positions = new List<Vector3>();
            var textures = new Dictionary<int, Texture>();

            // TODO: to remove after presets refactoring
            _settings = new IOGeometrySettings
            {
                PremultiplyUV = true,
                Scale = 1024.0f,
                FlipX = false,
                FlipZ = true,
                FlipUV_V = false
            };

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
                    if (line == "" || line == "}")
                        continue;

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
                        if (numMaterials == 0)
                            return null;
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
                                    if (texturePath != "")
                                        textures.Add(i, GetTexture(Path.GetDirectoryName(filename), texturePath));
                                    break;
                                }
                            }
                        }
                        continue;
                    }
                    else if (chunk == "Object")
                    {
                        var mesh = new IOMesh();
                        positions = new List<Vector3>();

                        var lastVertex = 0;
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
                                    positions.Add(ApplyAxesTransforms(new Vector3(MathUtilEx.ParseFloatCultureInvariant(tokensPosition[0]),
                                                                                  MathUtilEx.ParseFloatCultureInvariant(tokensPosition[1]),
                                                                                  MathUtilEx.ParseFloatCultureInvariant(tokensPosition[2]))));
                                }
                                line = reader.ReadLine().Trim();
                            }
                            else if (tokens[0] == "face")
                            {
                                var numFaces = int.Parse(tokens[1]);
                                for (var i = 0; i < numFaces; i++)
                                {
                                    line = reader.ReadLine().Trim();

                                    var numVerticesInFace = int.Parse(line.Substring(0, line.IndexOf(' ')));
                                    var poly = new IOPolygon(numVerticesInFace == 3 ? IOPolygonShape.Triangle : IOPolygonShape.Quad);

                                    // We MUST have vertices
                                    var stringVertices = GetSubBlock(line, "V");
                                    if (stringVertices == "")
                                        return null;
                                    var tokensVertices = stringVertices.Split(' ');
                                    for (var k = 0; k < numVerticesInFace; k++)
                                    {
                                        var index = int.Parse(tokensVertices[k]);
                                        mesh.Positions.Add(positions[index]);
                                        poly.Indices.Add(lastVertex);
                                        lastVertex++;
                                    }

                                    // UV
                                    var stringUV = GetSubBlock(line, "UV");
                                    if (stringUV != "")
                                    {
                                        var tokensUV = stringUV.Split(' ');
                                        for (var k = 0; k < numVerticesInFace; k++)
                                        {
                                            var uv = ApplyUVTransform(new Vector2(MathUtilEx.ParseFloatCultureInvariant(tokensUV[2 * k]),
                                                                                  MathUtilEx.ParseFloatCultureInvariant(tokensUV[2 * k + 1])),
                                                                      textures[0].Image.Width,
                                                                      textures[0].Image.Height);
                                            mesh.UV.Add(uv);
                                        }
                                    }

                                    // Colors
                                    var stringColor = GetSubBlock(line, "COL");
                                    if (stringColor != "")
                                    {
                                        var tokensColor = stringColor.Split(' ');
                                        for (var k = 0; k < numVerticesInFace; k++)
                                        {
                                            var color = ApplyColorTransform(GetColor(int.Parse(tokensVertices[k])));
                                            mesh.Colors.Add(color);
                                        }
                                    }

                                    mesh.Polygons.Add(poly);
                                }
                                line = reader.ReadLine().Trim();
                            }
                            else if (tokens[0] == "}")
                                break;
                        }

                        mesh.Texture = textures[0];
                        model.Meshes.Add(mesh);
                    }
                }
            }

            return model;
        }

        private string GetSubBlock(string line, string pattern)
        {
            if (!line.Contains(" " + pattern + "("))
                return "";
            var s = line.Substring(line.IndexOf(" " + pattern + "(") + 2 + pattern.Length);
            s = s.Substring(0, s.IndexOf(")"));
            return s;
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
                if (depth == 0)
                    return;
                var line = reader.ReadLine();
                if (line.Contains("{"))
                { depth++; continue; }
                if (line.Contains("}"))
                { depth--; continue; }
            }
        }
    }
}
