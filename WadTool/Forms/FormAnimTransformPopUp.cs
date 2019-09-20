using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using WadTool.Controls;

namespace WadTool
{
    public partial class FormAnimTransformPopUp : PopUpWindow
    {
        private readonly PanelRenderingAnimationEditor _control;
        private readonly AnimationEditor _editor;
        
        private List<Vector3> _backupRot;
        private List<Vector3> _backupPos;
        private Vector3 _initialRot;
        private Vector3 _initialPos;
        private int _meshIndex;

        private bool _allowUpdate = true;
        private bool _madeChanges = false;

        public FormAnimTransformPopUp(AnimationEditor editor, PanelRenderingAnimationEditor control, Point location) : base(location)
        {
            InitializeComponent();

            _editor    = editor;
            _control   = control;
            _backupPos = new List<Vector3>();
            _backupRot = new List<Vector3>();

            // Create backup rotations/translations for all affected frames
            _meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);
            _editor.ActiveFrames.ForEach(f => { _backupRot.Add(f.Rotations[_meshIndex]); _backupPos.Add(f.Translations[_meshIndex]); });

            // Convert and keep current frame data to display it in controls
            var rotX = MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[_meshIndex].X);
            var rotY = MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[_meshIndex].Y);
            var rotZ = MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[_meshIndex].Z);

            var posX = _editor.CurrentKeyFrame.Translations[_meshIndex].X;
            var posY = _editor.CurrentKeyFrame.Translations[_meshIndex].Y;
            var posZ = _editor.CurrentKeyFrame.Translations[_meshIndex].Z;

            // These two are merely used for resetting to default via big cross button :3
            _initialPos = new Vector3(posX, posY, posZ);
            _initialRot = new Vector3(rotX, rotY, rotZ);

            if (_meshIndex != 0) Size = MinimumSize;
            Location = Cursor.Position;
        }

        public void UpdateTransform()
        {
            if (!_allowUpdate) return;

            ClampRotations();

            // Push undo on first occurence of editing
            if (!_madeChanges)
            {
                _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
                _madeChanges = true;
            }

            // Internally use absolute rotation values and relative translation values
            var newPos = new Vector3((float)nudTransX.Value, (float)nudTransY.Value, (float)nudTransZ.Value);
            var newRot = _initialRot + new Vector3((float)nudRotX.Value, (float)nudRotY.Value, (float)nudRotZ.Value);
            
            var meshIndex = _control.Model.Meshes.IndexOf(_control.SelectedMesh);

            int index = 0;
            foreach (var keyframe in _editor.ActiveFrames)
            {
                var translationVector = keyframe.Translations[meshIndex];
                translationVector = _backupPos[index] + newPos;
                keyframe.Translations[meshIndex] = translationVector;
                keyframe.TranslationsMatrices[meshIndex] = Matrix4x4.CreateTranslation(translationVector);
                
                var rotationVector = new Vector3(MathC.DegToRad(newRot.X), MathC.DegToRad(newRot.Y), MathC.DegToRad(newRot.Z));
                keyframe.Quaternions[meshIndex] = Quaternion.CreateFromYawPitchRoll(rotationVector.Y, 
                                                                                    rotationVector.X, 
                                                                                    rotationVector.Z);
                keyframe.Rotations[meshIndex] = MathC.QuaternionToEuler(keyframe.Quaternions[meshIndex]);

                index++;
            }

            _control.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
            _control.Invalidate();
        }

        private void ClampRotations() // Don't go outside 0-360 range
        {
            _allowUpdate = false; 

            if (nudRotX.Value < 0 || nudRotX.Value >= 360) { nudRotX.Value = nudRotX.Value % 360; if (nudRotX.Value < 0) nudRotX.Value += 360; }
            if (nudRotY.Value < 0 || nudRotY.Value >= 360) { nudRotY.Value = nudRotY.Value % 360; if (nudRotY.Value < 0) nudRotY.Value += 360; }
            if (nudRotZ.Value < 0 || nudRotZ.Value >= 360) { nudRotZ.Value = nudRotZ.Value % 360; if (nudRotZ.Value < 0) nudRotZ.Value += 360; }

            _allowUpdate = true;
        }

        private void nudRotX_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudRotY_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudRotZ_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudRotX_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudRotY_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudRotZ_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudTransX_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudTransY_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudTransZ_ValueChanged(object sender, EventArgs e) => UpdateTransform();
        private void nudTransX_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudTransY_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();
        private void nudTransZ_KeyUp(object sender, KeyEventArgs e) => UpdateTransform();

        private void FormPopUpSearch_Deactivate(object sender, EventArgs e) => Close();
        private void darkSectionPanel1_MouseDown(object sender, MouseEventArgs e) => Drag(e);
        private void darkSectionPanel2_MouseDown(object sender, MouseEventArgs e) => Drag(e);

        private void butCancel_Click(object sender, EventArgs e)
        {
            _allowUpdate = false;
            nudRotX.Value = 0; nudRotY.Value = 0; nudRotZ.Value = 0;
            nudTransX.Value = 0; nudTransY.Value = 0; nudTransZ.Value = 0;
            _allowUpdate = true;

            UpdateTransform();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData.HasFlag(Keys.Escape))
                Close();

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
