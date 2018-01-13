using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
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
                            var material = new IOMaterial(tokensMaterial[0]);

                            for (var j = 0; j < tokensMaterial.Length; j++)
                            {
                                var texturePath = "";
                                if (tokensMaterial[j].StartsWith("tex"))
                                {
                                    texturePath = tokensMaterial[j].Substring(5, tokensMaterial[j].Length - 7);
                                    if (texturePath != "")
                                        textures.Add(i, GetTexture(Path.GetDirectoryName(filename), texturePath));
                                    material.Texture = textures[i];
                                    break;
                                }
                            }

                            model.Materials.Add(material);
                        }
                        continue;
                    }
                    else if (chunk == "Object")
                    {
                        var name = line.Split(' ')[1];
                        var mesh = new IOMesh(name);
                        var isRoom = false;
                        positions = new List<Vector3>();

                        if (name.Contains("TeRoom_"))
                        {
                            model.HasMultipleRooms = true;
                            isRoom = true;
                        }

                        var lastVertex = 0;
                        var translation = Vector3.Zero;

                        while (!reader.EndOfStream)
                        {
                            line = reader.ReadLine().Trim();
                            var tokens = line.Split(' ');
                            
                            if (tokens[0] == "translation" && model.HasMultipleRooms)
                            {
                                translation = ApplyAxesTransforms(new Vector3(ParseFloatCultureInvariant(tokens[1]),
                                                                              ParseFloatCultureInvariant(tokens[2]),
                                                                              ParseFloatCultureInvariant(tokens[3])));
                            }
                            else if (tokens[0] == "vertex")
                            {
                                var numVertices = int.Parse(tokens[1]);
                                for (var i = 0; i < numVertices; i++)
                                {
                                    var tokensPosition = reader.ReadLine().Trim().Split(' ');
                                    positions.Add(ApplyAxesTransforms(new Vector3(ParseFloatCultureInvariant(tokensPosition[0]),
                                                                                  ParseFloatCultureInvariant(tokensPosition[1]),
                                                                                  ParseFloatCultureInvariant(tokensPosition[2]))
                                                                      )- translation);
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
                                            var uv = ApplyUVTransform(new Vector2(ParseFloatCultureInvariant(tokensUV[2 * k]),
                                                                                  ParseFloatCultureInvariant(tokensUV[2 * k + 1])),
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

                                    // Material index
                                    var stringMaterialIndex = GetSubBlock(line, "M");
                                    var materialIndex = 0;
                                    if (stringMaterialIndex != "")
                                        materialIndex = int.Parse(stringMaterialIndex);

                                    // Add polygon to the submesh (and add submesh if not existing yet)
                                    var material = model.Materials[materialIndex];
                                    if (!mesh.Submeshes.ContainsKey(material))
                                        mesh.Submeshes.Add(material, new IOSubmesh(material));

                                    mesh.Submeshes[material].Polygons.Add(poly);
                                }
                                line = reader.ReadLine().Trim();
                            }
                            else if (tokens[0] == "}")
                                break;
                        }

                        model.Meshes.Add(mesh);
                    }
                }
            }

            return model;
        }

        private static float ParseFloatCultureInvariant(string num)
        {
            if (string.IsNullOrEmpty(num))
            { throw new ArgumentNullException("input"); }

            float res;
            if (float.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out res))
            {
                return res;
            }

            return 0.0f;
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
