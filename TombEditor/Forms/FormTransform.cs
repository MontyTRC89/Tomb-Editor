using System;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Wad.Catalog;

namespace TombEditor.Forms
{
    public partial class FormTransform : DarkForm
    {
        private Editor _editor;
        private readonly PositionBasedObjectInstance _instance;

        public FormTransform(PositionBasedObjectInstance instance)
        {
            InitializeComponent();
            _editor = Editor.Instance;

            _instance = instance;
            UpdateUI();
        }

        private void UpdateUI()
        {
            nudRotY.Enabled = (_instance is IRotateableY);

            if (_editor.SelectedObject is MoveableInstance moveable)
            {
                nudRotX.Enabled =
                nudRotZ.Enabled = TrCatalog.IsFreelyRotateable(_editor.Level.Settings.GameVersion, moveable.WadObjectId.TypeId);
            }
            else
            {
                nudRotX.Enabled = (_instance is IRotateableYX);
                nudRotZ.Enabled = (_instance is IRotateableYXRoll);
            }

            nudScaleX.Enabled = (_instance is IScaleable);

            nudTransX.Value = (decimal)_instance.Position.X + _instance.Room.Position.X * (int)Level.SectorSizeUnit;
            nudTransY.Value = (decimal)_instance.Position.Y + _instance.Room.Position.Y;
            nudTransZ.Value = (decimal)_instance.Position.Z + _instance.Room.Position.Z * (int)Level.SectorSizeUnit;

            if (_instance is IRotateableY rotateableY)
                nudRotY.Value = (decimal)rotateableY.RotationY;

            if (_instance is IRotateableYX rotateableYX)
                nudRotX.Value = (decimal)rotateableYX.RotationX;

            if (_instance is IRotateableYXRoll rotateableYXRoll)
                nudRotZ.Value = (decimal)rotateableYXRoll.Roll;

            if (_instance is IScaleable scaleable)
            {
                // TODO: Replace this code with separate value assignments for Y/Z scale
                nudScaleX.Value = (decimal)scaleable.Scale;
                nudScaleY.Value = (decimal)scaleable.Scale;
                nudScaleZ.Value = (decimal)scaleable.Scale;
            }
        }

        private void nudScaleX_Validated(object sender, EventArgs e)
        {
            // TODO: Replace this code with separate methods for Y/Z scale
            nudScaleY.Value = nudScaleX.Value;
            nudScaleZ.Value = nudScaleX.Value;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _editor.UndoManager.PushObjectTransformed(_instance);

            _instance.Position = new Vector3((float)nudTransX.Value - _instance.Room.Position.X * (int)Level.SectorSizeUnit,
                                             (float)nudTransY.Value - _instance.Room.Position.Y,
                                             (float)nudTransZ.Value - _instance.Room.Position.Z * (int)Level.SectorSizeUnit);

            if (_instance is IRotateableY rotateableY)
                rotateableY.RotationY = (float)nudRotY.Value;

            if (_instance is IRotateableYX rotateableYX)
                rotateableYX.RotationX = (float)nudRotX.Value;

            if (_instance is IRotateableYXRoll rotateableYXRoll)
                rotateableYXRoll.Roll = (float)nudRotZ.Value;

            if (_instance is IScaleable scaleable)
                scaleable.Scale = (float)nudScaleX.Value;


            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
