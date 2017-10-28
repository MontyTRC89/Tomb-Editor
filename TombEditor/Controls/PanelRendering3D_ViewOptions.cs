using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor.Controls
{
    public partial class PanelRendering3D_ViewOptions : FloatingToolbox
    {
        private Editor _editor;
        private PanelRendering3D _parentPanel;

        public PanelRendering3D_ViewOptions(PanelRendering3D parentPanel, Point location)
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            Location = location;
            _parentPanel = parentPanel;
            RefreshViewOptions();
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ConfigurationChangedEvent)
                Location = ((Editor.ConfigurationChangedEvent)obj).Current.Rendering3D_ViewOptionsPosition;
        }

        private void RefreshViewOptions()
        {
            butDrawPortals.Checked = _parentPanel.DrawPortals;
            butDrawHorizon.Checked = _parentPanel.DrawHorizon;
            butDrawRoomNames.Checked = _parentPanel.DrawRoomNames;
            butDrawIllegalSlopes.Checked = _parentPanel.DrawIllegalSlopes;
            butDrawMoveables.Checked = _parentPanel.ShowMoveables;
            butDrawStatics.Checked = _parentPanel.ShowStatics;
            butDrawImportedGeometry.Checked = _parentPanel.ShowImportedGeometry;
            butDrawLightMeshes.Checked = _parentPanel.ShowLightMeshes;
            butDrawOther.Checked = _parentPanel.ShowOtherObjects;

            _parentPanel.Invalidate();
        }

        private void butDrawPortals_Click(object sender, EventArgs e)
        {
            _parentPanel.DrawPortals = !_parentPanel.DrawPortals;
            RefreshViewOptions();
        }

        private void butDrawHorizon_Click(object sender, EventArgs e)
        {
            _parentPanel.DrawHorizon = !_parentPanel.DrawHorizon;
            RefreshViewOptions();
        }

        private void butDrawRoomNames_Click(object sender, EventArgs e)
        {
            _parentPanel.DrawRoomNames = !_parentPanel.DrawRoomNames;
            RefreshViewOptions();
        }

        private void butDrawIllegalSlopes_Click(object sender, EventArgs e)
        {
            _parentPanel.DrawIllegalSlopes = !_parentPanel.DrawIllegalSlopes;
            RefreshViewOptions();
        }

        private void butDrawMoveables_Click(object sender, EventArgs e)
        {
            _parentPanel.ShowMoveables = !_parentPanel.ShowMoveables;
            RefreshViewOptions();
        }

        private void butDrawStatics_Click(object sender, EventArgs e)
        {
            _parentPanel.ShowStatics = !_parentPanel.ShowStatics;
            RefreshViewOptions();
        }

        private void butDrawImportedGeometry_Click(object sender, EventArgs e)
        {
            _parentPanel.ShowImportedGeometry = !_parentPanel.ShowImportedGeometry;
            RefreshViewOptions();
        }

        private void butDrawLightMeshes_Click(object sender, EventArgs e)
        {
            _parentPanel.ShowLightMeshes = !_parentPanel.ShowLightMeshes;
            RefreshViewOptions();
        }

        private void butDrawOther_Click(object sender, EventArgs e)
        {
            _parentPanel.ShowOtherObjects = !_parentPanel.ShowOtherObjects;
            RefreshViewOptions();
        }

        private void toolStripLabel1_MouseDown(object sender, MouseEventArgs e)
        {
            DragStart(e.Location);
        }

        private void PanelRendering3D_ViewOptions_MouseUp(object sender, MouseEventArgs e)
        {
            DragStop();
        }
    }
}
