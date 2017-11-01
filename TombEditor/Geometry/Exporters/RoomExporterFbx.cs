using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry.Exporters
{
    public class RoomExporterFbx : BaseRoomExporter
    {
        /* private void Test(Room room)
         {
             var document = new FbxDocument();
             document.Version = FbxVersion.v7_1;

             var nodeFBXHeaderExtension = new FbxNode();
             nodeFBXHeaderExtension.Name = "FBXHeaderExtension";

             var nodeFBXHeaderVersion = new FbxNode();
             nodeFBXHeaderVersion.Name = "FBXHeaderVersion";
             nodeFBXHeaderVersion.Value = 1003;

             var nodeFBXVersion = new FbxNode();
             nodeFBXVersion.Name = "FBXVersion";
             nodeFBXVersion.Value = 7100;

             var nodeCreationTimeStamp = new FbxNode();
             nodeCreationTimeStamp.Name = "CreationTimeStamp";

             var nodeCreationTimeStampVersion = new FbxNode();
             nodeCreationTimeStampVersion.Name = "CreationTimeStamp";
             nodeCreationTimeStampVersion.Value = 1000;

             var nodeCreationTimeStampYear = new FbxNode();
             nodeCreationTimeStampYear.Name = "Year";
             nodeCreationTimeStampYear.Value = DateTime.Now.Year;

             var nodeCreationTimeStampMonth = new FbxNode();
             nodeCreationTimeStampMonth.Name = "Month";
             nodeCreationTimeStampMonth.Value = DateTime.Now.Month;

             var nodeCreationTimeStampDay = new FbxNode();
             nodeCreationTimeStampDay.Name = "Day";
             nodeCreationTimeStampDay.Value = DateTime.Now.Day;

             var nodeCreationTimeStampHour = new FbxNode();
             nodeCreationTimeStampHour.Name = "Hour";
             nodeCreationTimeStampHour.Value = DateTime.Now.Hour;

             var nodeCreationTimeStampMinute = new FbxNode();
             nodeCreationTimeStampMinute.Name = "Minute";
             nodeCreationTimeStampMinute.Value = DateTime.Now.Minute;

             var nodeCreationTimeStampSecond = new FbxNode();
             nodeCreationTimeStampSecond.Name = "Second";
             nodeCreationTimeStampSecond.Value = DateTime.Now.Second;

             var nodeCreationTimeStampMillisecond = new FbxNode();
             nodeCreationTimeStampMillisecond.Name = "Millisecond";
             nodeCreationTimeStampMillisecond.Value = DateTime.Now.Millisecond;

             nodeCreationTimeStamp.Nodes.Add(nodeCreationTimeStampVersion);
             nodeCreationTimeStamp.Nodes.Add(nodeCreationTimeStampYear);
             nodeCreationTimeStamp.Nodes.Add(nodeCreationTimeStampMonth);
             nodeCreationTimeStamp.Nodes.Add(nodeCreationTimeStampDay);
             nodeCreationTimeStamp.Nodes.Add(nodeCreationTimeStampHour);
             nodeCreationTimeStamp.Nodes.Add(nodeCreationTimeStampMinute);
             nodeCreationTimeStamp.Nodes.Add(nodeCreationTimeStampSecond);
             nodeCreationTimeStamp.Nodes.Add(nodeCreationTimeStampMillisecond);

             var nodeCreator = new FbxNode();
             nodeCreator.Name = "Creator";
             nodeCreator.Value = "Tomb Editor";

             nodeFBXHeaderExtension.Nodes.Add(nodeFBXHeaderVersion);
             nodeFBXHeaderExtension.Nodes.Add(nodeFBXVersion);
             nodeFBXHeaderExtension.Nodes.Add(nodeCreationTimeStamp);
             nodeFBXHeaderExtension.Nodes.Add(nodeCreator);

             var nodeFileId = new FbxNode();
             nodeFileId.Value = new byte[] { 40, 179, 42, 235, 182, 36, 204, 194, 191, 200, 176, 42, 169, 43, 252, 241 };

             var nodeCreationTime = new FbxNode();
             nodeCreationTime.Name = "CreationTime";
             nodeCreationTime.Value = DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + " " +
                                      DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond;

             var nodeCreator2 = new FbxNode();
             nodeCreator2.Name = "Creator";
             nodeCreator2.Value = "Tomb Editor";

             document.Nodes.Add(nodeFBXHeaderExtension);
             document.Nodes.Add(nodeFileId);
             document.Nodes.Add(nodeCreationTime);
             document.Nodes.Add(nodeCreator2);

             var nodeGlobalSettings = new FbxNode();
             nodeGlobalSettings.Name = "GlobalSettings";

             var nodeGlobalSettingsVersion = new FbxNode();
             nodeGlobalSettingsVersion.Name = "Version";
             nodeGlobalSettingsVersion.Value = 1000;

             var nodeGlobalSettings70 = new FbxNode();
             nodeGlobalSettings70.Name = "Properties70";

             nodeGlobalSettings.Nodes.Add(nodeGlobalSettingsVersion);
             nodeGlobalSettings.Nodes.Add(nodeGlobalSettings70);

             document.Nodes.Add(nodeGlobalSettings);

             var nodeDocuments = new FbxNode();
             nodeDocuments.Name = "Documents";

             var nodeDocumentsCount = new FbxNode();
             nodeDocumentsCount.Name = "Count";
             nodeDocuments.Value = 1;

             var nodeDocument = new FbxNode();
             nodeDocument.Name = "Document";
             nodeDocument.Properties.Add(321289858);
             nodeDocument.Properties.Add("Scene");
             nodeDocument.Properties.Add("Scene");

             var nodeDocument70 = new FbxNode();
             nodeDocument70.Name = "Properties70";

             var nodeDocumentRoot = new FbxNode();
             nodeDocumentRoot.Name = "RootNode";
             nodeDocumentRoot.Value = 0;

             nodeDocument.Nodes.Add(nodeDocument70);
             nodeDocument.Nodes.Add(nodeDocumentRoot);

             nodeDocuments.Nodes.Add(nodeDocumentsCount);
             nodeDocuments.Nodes.Add(nodeDocument);

             document.Nodes.Add(nodeDocument);

             var nodeObjects = new FbxNode();
             nodeObjects.Name = "Objects";

             var nodeGeometry = new FbxNode();
             nodeGeometry.Properties.Add(663859776);
             nodeGeometry.Properties.Add("Geometry::Room0");
             nodeGeometry.Properties.Add("Mesh");

             var document = FbxIO.ReadBinary("untitled.fbx");

             var nodeVertices = new FbxNode();
             nodeVertices.Name = "Vertices";

             var arrayVertices = new List<float>();
             var vertices = room.GetRoomVertices();
             foreach (var vertex in vertices)
             {
                 arrayVertices.Add(vertex.Position.X);
                 arrayVertices.Add(vertex.Position.Y);
                 arrayVertices.Add(vertex.Position.Z);
             }

             nodeVertices.Value = arrayVertices.ToArray();

             var nodeIndices = new FbxNode();
             nodeIndices.Name = "PolygonVertexIndex";

             var arrayIndices = new List<int>();
             for (var z = 0; z < room.NumZSectors; z++)
             {
                 for (var x = 0; x < room.NumXSectors; x++)
                 {
                     for (var f = 0; f < 29; f++)
                     {
                         if (room.IsFaceDefined(x, z, (BlockFace)f) && !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsInvisble &&
                             !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsUnavailable)
                         {
                             var vertexRange = room.GetFaceVertexRange(x, z, (BlockFace)f);
                             var v1 = vertexRange.Start + 0;
                             var v2 = vertexRange.Start + 1;
                             var v3 = (vertexRange.Start + 2) ^ -1;
                             var v4 = vertexRange.Start + 3;
                             var v5 = vertexRange.Start + 4;
                             var v6 = (vertexRange.Start + 5) ^ -1;

                             if (vertexRange.Count == 3)
                             {
                                 arrayIndices.Add(v1);
                                 arrayIndices.Add(v2);
                                 arrayIndices.Add(v3);
                             }
                             else
                             {
                                 arrayIndices.Add(v1);
                                 arrayIndices.Add(v2);
                                 arrayIndices.Add(v3);
                                 arrayIndices.Add(v4);
                                 arrayIndices.Add(v5);
                                 arrayIndices.Add(v6);
                             }
                         }
                     }
                 }
             }

             nodeIndices.Value = arrayIndices.ToArray();

             //nodeGeometry.Nodes.Add(nodeVertices);
             //nodeGeometry.Nodes.Add(nodeIndices);

             var nodeModel = new FbxNode();
             nodeModel.Name = "Model";
             nodeModel.Properties.Add(109449652);
             nodeModel.Properties.Add("Model::Room0");
             nodeModel.Properties.Add("Mesh");

             var nodeModelVersion = new FbxNode();
             nodeModelVersion.Name = "Version";
             nodeModelVersion.Value = 232;

             var nodeModel70 = new FbxNode();
             nodeModel70.Name = "Properties70";

             nodeModel.Nodes.Add(nodeModelVersion);
             nodeModel.Nodes.Add(nodeModel70);

             //nodeObjects.Nodes.Add(nodeGeometry);
             //nodeObjects.Nodes.Add(nodeModel);

             //document.Nodes.Add(nodeObjects);

             var nodeConnections = new FbxNode();
             nodeConnections.Name = "Connections";

             var nodeConnection1 = new FbxNode();
             nodeConnection1.Name = "C";
             nodeConnection1.Properties.Add("OO");
             nodeConnection1.Properties.Add(109449652);
             nodeConnection1.Properties.Add(0);

             var nodeConnection2 = new FbxNode();
             nodeConnection2.Name = "C";
             nodeConnection2.Properties.Add("OO");
             nodeConnection2.Properties.Add(663859776);
             nodeConnection2.Properties.Add(109449652);

             nodeConnections.Nodes.Add(nodeConnection1);
             nodeConnections.Nodes.Add(nodeConnection2);

             document.Nodes.Add(nodeConnections);

             FbxIO.WriteBinary(document, "testio.fbx");
         }


         public override bool ExportToFile(Room room, string filename)
         {
             Test(room);
             return true;

             var path = Path.GetDirectoryName(filename);

             if (File.Exists(filename)) File.Delete(filename);

             using (var writer = new BinaryWriter(File.OpenWrite(filename)))
             {
                 WriteRootNode(writer);
                 WriteNodeRaw(writer, "FileId", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                                             0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                 WriteNodeString(writer, "Creator", "Tomb Editor BETA");
                 WriteGlobalSettings(writer);
                 WriteObjects(writer, room);
             }

             return true;
         }

         private void WriteRootNode(BinaryWriter writer)
         {
             var position = writer.BaseStream.Position;
             writer.Write((int)0);
             writer.Write((int)0);
             writer.Write((int)0);
             writer.Write((byte)"FBXHeaderExtension".Length);
             writer.Write(ASCIIEncoding.ASCII.GetBytes("FBXHeaderExtension"));

             WriteNodeInt(writer, "FBXHeaderVersion", 1003);
             WriteNodeInt(writer, "FBXVersion", 6100);
             WriteCreationTimeStamp(writer);

             WriteEndingFiller(writer);

             // Store chunk length
             var nextPosition = writer.BaseStream.Position;
             writer.Seek((int)position, SeekOrigin.Begin);
             writer.Write((int)nextPosition);
             writer.Seek((int)nextPosition, SeekOrigin.Begin);
         }

         private void WriteCreationTimeStamp(BinaryWriter writer)
         {
             var position = writer.BaseStream.Position;
             writer.Write((int)0);
             writer.Write((int)0);
             writer.Write((int)0);
             writer.Write((byte)"CreationTimeStamp".Length);
             writer.Write(ASCIIEncoding.ASCII.GetBytes("CreationTimeStamp"));

             WriteNodeInt(writer, "Version", 100);
             WriteNodeInt(writer, "Year", DateTime.Now.Year);
             WriteNodeInt(writer, "Month", DateTime.Now.Month);
             WriteNodeInt(writer, "Day", DateTime.Now.Day);
             WriteNodeInt(writer, "Hour", DateTime.Now.Hour);
             WriteNodeInt(writer, "Minute", DateTime.Now.Minute);
             WriteNodeInt(writer, "Second", DateTime.Now.Second);
             WriteNodeInt(writer, "Millisecond", DateTime.Now.Millisecond);
             WriteNodeString(writer, "Creator", "Tomb Editor BETA");
             WriteEndingFiller(writer);

             // Store chunk length
             var nextPosition = writer.BaseStream.Position;
             writer.Seek((int)position, SeekOrigin.Begin);
             writer.Write((int)nextPosition);
             writer.Seek((int)nextPosition, SeekOrigin.Begin);
         }

         private void WriteGlobalSettings(BinaryWriter writer)
         {
             var position = writer.BaseStream.Position;
             writer.Write((int)0);
             writer.Write((int)0);
             writer.Write((int)0);
             writer.Write((byte)"GlobalSettings".Length);
             writer.Write(ASCIIEncoding.ASCII.GetBytes("GlobalSettings"));

             WriteNodeInt(writer, "Version", 1000);

             WriteEndingFiller(writer);

             // Store chunk length
             var nextPosition = writer.BaseStream.Position;
             writer.Seek((int)position, SeekOrigin.Begin);
             writer.Write((int)nextPosition);
             writer.Seek((int)nextPosition, SeekOrigin.Begin);
         }

         private void WriteObjects(BinaryWriter writer, Room room)
         {
             var position = writer.BaseStream.Position;
             writer.Write((int)0);
             writer.Write((int)0);
             writer.Write((int)0);
             writer.Write((byte)"Objects".Length);
             writer.Write(ASCIIEncoding.ASCII.GetBytes("Objects"));

             WriteGeometry(writer, room);

             WriteEndingFiller(writer);

             // Store chunk length
             var nextPosition = writer.BaseStream.Position;
             writer.Seek((int)position, SeekOrigin.Begin);
             writer.Write((int)nextPosition);
             writer.Seek((int)nextPosition, SeekOrigin.Begin);
         }

         private void WriteGeometry(BinaryWriter writer, Room room)
         {
             var position = writer.BaseStream.Position;
             writer.Write((int)0);
             writer.Write((int)3);
             writer.Write((int)38);
             writer.Write((byte)"Geometry".Length);
             writer.Write(ASCIIEncoding.ASCII.GetBytes("Geometry"));
             writer.Write((byte)0x4C);
             writer.Write((int)0x04);
             writer.Write((long)0);
             writer.Write((byte)0x53);
             writer.Write((int)0x0F);
             writer.Write(ASCIIEncoding.ASCII.GetBytes("Room0"));
             writer.Write((byte)0x00);
             writer.Write((byte)0x01);
             writer.Write(ASCIIEncoding.ASCII.GetBytes("Geometry"));
             writer.Write((byte)0x53);
             writer.Write((int)0x04);
             writer.Write(ASCIIEncoding.ASCII.GetBytes("Mesh"));

             WriteEndingFiller(writer);

             // Store chunk length
             var nextPosition = writer.BaseStream.Position;
             writer.Seek((int)position, SeekOrigin.Begin);
             writer.Write((int)nextPosition);
             writer.Seek((int)nextPosition, SeekOrigin.Begin);
         }

         private void WriteEndingFiller(BinaryWriter writer)
         {
             for (var i = 0; i < 13; i++)
                 writer.Write(0x00);
         }

         private void WriteNodeInt(BinaryWriter writer, string node, int value)
         {
             var position = writer.BaseStream.Position;
             writer.Write((int)0);
             writer.Write((int)0x01);
             writer.Write((int)0x05);
             writer.Write((byte)node.Length);
             writer.Write(ASCIIEncoding.ASCII.GetBytes(node));
             writer.Write((byte)0x49);
             writer.Write(value);

             // Store chunk length
             var nextPosition = writer.BaseStream.Position;
             writer.Seek((int)position, SeekOrigin.Begin);
             writer.Write((int)nextPosition);
             writer.Seek((int)nextPosition, SeekOrigin.Begin);
         }

         private void WriteNodeLong(BinaryWriter writer, string node, long value)
         {
             var position = writer.BaseStream.Position;
             writer.Write((int)0);
             writer.Write((int)0x01);
             writer.Write((int)0x05);
             writer.Write((byte)node.Length);
             writer.Write(ASCIIEncoding.ASCII.GetBytes(node));
             writer.Write((byte)0x49);
             writer.Write(value);

             // Store chunk length
             var nextPosition = writer.BaseStream.Position;
             writer.Seek((int)position, SeekOrigin.Begin);
             writer.Write((int)nextPosition);
             writer.Seek((int)nextPosition, SeekOrigin.Begin);
         }

         private void WriteNodeRaw(BinaryWriter writer, string node, byte[] value)
         {
             var position = writer.BaseStream.Position;
             writer.Write((int)0);
             writer.Write((int)0x01);
             writer.Write((int)(5 + value.Length));
             writer.Write((byte)node.Length);
             writer.Write(ASCIIEncoding.ASCII.GetBytes(node));
             writer.Write((byte)0x52);
             writer.Write(value);

             // Store chunk length
             var nextPosition = writer.BaseStream.Position;
             writer.Seek((int)position, SeekOrigin.Begin);
             writer.Write((int)nextPosition);
             writer.Seek((int)nextPosition, SeekOrigin.Begin);
         }

         private void WriteNodeString(BinaryWriter writer, string node, string value)
         {
             var position = writer.BaseStream.Position;
             writer.Write((int)0);
             writer.Write((int)0x01);
             writer.Write((int)(5 + value.Length));
             writer.Write((byte)node.Length);
             writer.Write(ASCIIEncoding.ASCII.GetBytes(node));
             writer.Write((byte)0x53);
             writer.Write(value);

             // Store chunk length
             var nextPosition = writer.BaseStream.Position;
             writer.Seek((int)position, SeekOrigin.Begin);
             writer.Write((int)nextPosition);
             writer.Seek((int)nextPosition, SeekOrigin.Begin);
         }

         // ASCII version
         /*
         public override bool ExportToFile(Room room, string filename)
         {
             var path = Path.GetDirectoryName(filename);

             if (File.Exists(filename)) File.Delete(filename);

             using (var writer = new StreamWriter(File.OpenWrite(filename)))
             {
                 // Write FBX header
                 writer.WriteLine("; FBX 6.1.0 project file");
                 writer.WriteLine("; Created by Tomb Editor");
                 writer.WriteLine("; ----------------------------------------------------");
                 writer.WriteLine();

                 writer.WriteLine("FBXHeaderExtension:  {");
                 writer.WriteLine("FBXHeaderVersion: 1003");
                 writer.WriteLine("FBXVersion: 6100");
                 writer.WriteLine("CreationTimeStamp: {");
                 writer.WriteLine("Version: 1000");
                 writer.WriteLine("Year: " + DateTime.Now.Year);
                 writer.WriteLine("Month: " + DateTime.Now.Month);
                 writer.WriteLine("Day: " + DateTime.Now.Day);
                 writer.WriteLine("Hour: " + DateTime.Now.Hour);
                 writer.WriteLine("Minute: " + DateTime.Now.Minute);
                 writer.WriteLine("Second: " + DateTime.Now.Second);
                 writer.WriteLine("Millisecond: " + DateTime.Now.Millisecond);
                 writer.WriteLine("}");
                 writer.WriteLine("Creator: \"Tomb Editor\"");
                 writer.WriteLine("}");
                 writer.WriteLine("CreationTime: \"" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + 
                                  " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + 
                                  ":" + DateTime.Now.Millisecond + "\"");
                 writer.WriteLine("Creator: \"Tomb Editor\"");
                 writer.WriteLine();

                 // Write room
                 writer.WriteLine("; Object properties");
                 writer.WriteLine(";------------------------------------------------------------------");
                 writer.WriteLine("Objects:  {");
                 writer.WriteLine("Model: \"Model::Room\", \"Mesh\" {");
                 writer.WriteLine("Version: 232");
                 writer.WriteLine("Shading: Y");

                 // Save vertices
                 writer.Write("Vertices: ");
                 var scale = 1024.0f;
                 var vertices = room.GetRoomVertices();
                 foreach (var vertex in vertices)
                     writer.Write((vertex.Position.X / scale).ToString(CultureInfo.InvariantCulture) + ", " +
                                  (vertex.Position.Y / scale).ToString(CultureInfo.InvariantCulture) + ", " +
                                  (-vertex.Position.Z / scale).ToString(CultureInfo.InvariantCulture) + ", ");
                 writer.WriteLine("0, 0, 0"); // Dummy vertex for now

                 // Save indices
                 writer.Write("PolygonVertexIndex: ");

                 var stringIndices = "PolygonVertexIndex: ";
                 for (var z = 0; z < room.NumZSectors; z++)
                 {
                     for (var x = 0; x < room.NumXSectors; x++)
                     {
                         for (var f = 0; f < 29; f++)
                         {
                             if (room.IsFaceDefined(x, z, (BlockFace)f) && !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsInvisble &&
                                 !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsUnavailable)
                             {
                                 var vertexRange = room.GetFaceVertexRange(x, z, (BlockFace)f);
                                 var v1 = vertexRange.Start + 0;
                                 var v2 = vertexRange.Start + 1;
                                 var v3 = (vertexRange.Start + 2) ^ -1;
                                 var v4 = vertexRange.Start + 3;
                                 var v5 = vertexRange.Start + 4;
                                 var v6 = (vertexRange.Start + 5) ^ -1;

                                 if (vertexRange.Count == 3)
                                 {
                                     stringIndices += v1 + "," + v2 + "," + v3 + ",";
                                 }
                                 else
                                 {
                                     stringIndices += v1 + "," + v2 + "," + v3 + ",";
                                     stringIndices += v4 + "," + v5 + "," + v6 + ",";
                                 }
                             }
                         }
                     }
                 }

                 stringIndices = stringIndices.TrimEnd(',');
                 writer.WriteLine(stringIndices);

                 writer.WriteLine("}");
                 writer.WriteLine("}");
             }

             return true;
         }*/

        public override bool ExportToFile(Room room, string filename)
        {
            throw new NotImplementedException();
        }
    }
}
