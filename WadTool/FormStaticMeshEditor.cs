using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormStaticMeshEditor : DarkUI.Forms.DarkForm
    {
        public WadStatic StaticMesh { get; set; }

        private WadToolClass _tool;

        public FormStaticMeshEditor()
        {
            InitializeComponent();
            _tool = WadToolClass.Instance;
            panelRendering.InitializePanel(_tool.Device);
        }

        private void FormStaticMeshEditor_Load(object sender, EventArgs e)
        {
            panelRendering.StaticMesh = StaticMesh;
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            // Check if we need to transform mesh
            var transform = panelRendering.GizmoTransform;
            if (transform != Matrix.Identity)
            {
                for (int i = 0; i < StaticMesh.Mesh.VerticesPositions.Count; i++)
                {
                    var position = Vector3.Transform(StaticMesh.Mesh.VerticesPositions[i], transform);
                    StaticMesh.Mesh.VerticesPositions[i] = new Vector3(position.X, position.Y, position.Z);
                }

                for (int i = 0; i < StaticMesh.Mesh.VerticesNormals.Count; i++)
                {
                    var normal = Vector3.Transform(StaticMesh.Mesh.VerticesNormals[i], transform);
                    StaticMesh.Mesh.VerticesNormals[i] = new Vector3(normal.X, normal.Y, normal.Z);
                }

                // Dispose old model
                _tool.DestinationWad.DirectXStatics[StaticMesh.ObjectID].Dispose();
                _tool.DestinationWad.DirectXStatics.Remove(StaticMesh.ObjectID);
                _tool.DestinationWad.DirectXStatics.Add(StaticMesh.ObjectID, StaticModel.FromWad2(_tool.Device, 
                                                                                                  _tool.DestinationWad, 
                                                                                                  StaticMesh, 
                                                                                                  _tool.DestinationWad.PackedTextures));
                StaticMesh.Mesh.UpdateHash();

                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
