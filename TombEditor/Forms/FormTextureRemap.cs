using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormTextureRemap : DarkForm
    {
        private Editor _editor;
        private Vector2 _sourceStart;
        private Vector2 _sourceEnd;
        private Vector2 _destinationStart;

        public FormTextureRemap(Editor editor)
        {
            _editor = editor;
            InitializeComponent();
            sourceTextureMap.FormParent = destinationTextureMap.FormParent = this;

            LevelTexture firstTexture = editor.Level.Settings.Textures.FirstOrDefault();
            comboSourceTexture.Items.AddRange(editor.Level.Settings.Textures.ToArray());
            comboSourceTexture.SelectedItem = firstTexture;
            comboDestinationTexture.Items.AddRange(editor.Level.Settings.Textures.ToArray());
            comboDestinationTexture.SelectedItem = firstTexture;
            if (firstTexture != null)
                _sourceEnd = firstTexture.Image.Size;
        }

        private bool SourceContains(Vector2 texCoord)
        {
            return _sourceStart.X <= texCoord.X && _sourceStart.Y <= texCoord.Y &&
                _sourceEnd.X >= texCoord.X && _sourceEnd.Y >= texCoord.Y;
        }

        private void cbUntextureCompletely_CheckedChanged(object sender, EventArgs e)
        {
            destinationPanel.Enabled = !cbUntextureCompletely.Checked;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            // Gather some data
            Level level = _editor.Level;
            LevelTexture sourceTexture = comboSourceTexture.SelectedItem as LevelTexture;
            LevelTexture destinationTexture = comboDestinationTexture.SelectedItem as LevelTexture;
            bool untextureCompletely = cbUntextureCompletely.Checked;
            IEnumerable<Room> relevantRooms;
            if (cbRestrictToSelectedRooms.Checked)
                relevantRooms = _editor.SelectedRooms;
            else
                relevantRooms = _editor.Level.Rooms.Where(room => room != null);

            if (sourceTexture == null || destinationTexture == null)
            {
                DarkMessageBox.Show(this, "Source or destination texture may not be unset.", "Problem", MessageBoxIcon.Asterisk);
                return;
            }

            // Room textures
            int roomTextureCount = 0;
            foreach (Room room in relevantRooms)
                foreach (Block sector in room.Blocks)
                    for (BlockFace face = 0; face < BlockFace.Count; ++face)
                    {
                        TextureArea currentTextureArea = sector.GetFaceTexture(face);
                        if (currentTextureArea.Texture == sourceTexture &&
                            SourceContains(currentTextureArea.TexCoord0) &&
                            SourceContains(currentTextureArea.TexCoord1) &&
                            SourceContains(currentTextureArea.TexCoord2) &&
                            SourceContains(currentTextureArea.TexCoord3))
                        {
                            currentTextureArea.TexCoord0 += _destinationStart - _sourceStart;
                            currentTextureArea.TexCoord1 += _destinationStart - _sourceStart;
                            currentTextureArea.TexCoord2 += _destinationStart - _sourceStart;
                            currentTextureArea.TexCoord3 += _destinationStart - _sourceStart;
                            currentTextureArea.Texture = destinationTexture;
                            if (untextureCompletely)
                                currentTextureArea.Texture = null;
                            sector.SetFaceTexture(face, currentTextureArea);
                            ++roomTextureCount;
                        }
                    }

            // Animated textures
            int animatedTextureCount = 0;
            if (cbRestrictToSelectedRooms.Checked)
                foreach (AnimatedTextureSet set in level.Settings.AnimatedTextureSets)
                {
                    var framesToRemove = new List<AnimatedTextureFrame>();
                    foreach (AnimatedTextureFrame frame in set.Frames)
                    {
                        if (frame.Texture == sourceTexture &&
                            SourceContains(frame.TexCoord0) &&
                            SourceContains(frame.TexCoord1) &&
                            SourceContains(frame.TexCoord2) &&
                            SourceContains(frame.TexCoord3))
                        {
                            frame.TexCoord0 += _destinationStart - _sourceStart;
                            frame.TexCoord1 += _destinationStart - _sourceStart;
                            frame.TexCoord2 += _destinationStart - _sourceStart;
                            frame.TexCoord3 += _destinationStart - _sourceStart;
                            frame.Texture = destinationTexture;
                            ++animatedTextureCount;
                            if (untextureCompletely)
                                framesToRemove.Add(frame);
                        }
                    }
                    set.Frames.RemoveAll(frame => framesToRemove.Contains(frame));
                }

            // Send out updates
            Parallel.ForEach(relevantRooms, room => room.RoomGeometry = new RoomGeometry(room));
            foreach (Room room in relevantRooms)
                _editor.RoomTextureChange(room);

            // Inform user
            if (DarkMessageBox.Show(this, "Remapped textures successfully (" + roomTextureCount + " in rooms, " + animatedTextureCount + " in texture animations.).\n" +
                "Do you want close the texture remapping dialog now?", "Succcess", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboSourceTexture_DropDown(object sender, EventArgs e)
        {
            // Make the combo box as wide as possible
            Point screenPointLeft = comboSourceTexture.PointToScreen(new Point(0, 0));
            Rectangle screenPointRight = Screen.GetBounds(comboSourceTexture.PointToScreen(new Point(0, comboSourceTexture.Width)));
            comboSourceTexture.DropDownWidth = screenPointRight.Right - screenPointLeft.X - 15; // Margin
        }

        private void comboDestinationTexture_DropDown(object sender, EventArgs e)
        {
            // Make the combo box as wide as possible
            Point screenPointLeft = comboDestinationTexture.PointToScreen(new Point(0, 0));
            Rectangle screenPointRight = Screen.GetBounds(comboDestinationTexture.PointToScreen(new Point(0, comboDestinationTexture.Width)));
            comboDestinationTexture.DropDownWidth = screenPointRight.Right - screenPointLeft.X - 15; // Margin
        }

        private void comboSourceTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (sourceTextureMap.VisibleTexture != comboSourceTexture.SelectedItem)
                sourceTextureMap.ResetVisibleTexture(comboSourceTexture.SelectedItem as LevelTexture);
        }

        private void comboDestinationTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (destinationTextureMap.VisibleTexture != comboDestinationTexture.SelectedItem)
                destinationTextureMap.ResetVisibleTexture(comboDestinationTexture.SelectedItem as LevelTexture);
        }

        private void scalingFactor_ValueChanged(object sender, EventArgs e)
        {
            destinationTextureMap.Invalidate();
        }

        public class PanelTextureMapForRemap : Controls.PanelTextureMap
        {
            public FormTextureRemap FormParent { get; set; }
            public bool IsDestination { get; set; }

            private void DrawArea(Vector2 start, Vector2 end, Graphics g)
            {
                PointF drawStart = ToVisualCoord(start);
                PointF drawEnd = ToVisualCoord(end);
                RectangleF drawArea = RectangleF.FromLTRB(drawStart.X, drawStart.Y, drawEnd.X, drawEnd.Y);

                using (var brush = new SolidBrush(Color.FromArgb(30, 220, 210, 20)))
                    g.FillRectangle(brush, drawArea);
                using (var pen = new Pen(Color.FromArgb(255, 220, 210, 20), 3))
                    g.DrawRectangle(pen, drawArea);
            }

            protected override void OnPaintSelection(PaintEventArgs e)
            {
                if (IsDestination)
                    DrawArea(FormParent._destinationStart, FormParent._destinationStart + (FormParent._sourceEnd - FormParent._sourceStart) * (float)FormParent.scalingFactor.Value, e.Graphics);
                else
                    DrawArea(FormParent._sourceStart, FormParent._sourceEnd, e.Graphics);
            }

            private Vector2 Quantize2(Vector2 texCoord, bool end)
            {
                var selectionPrecision = GetSelectionPrecision(true);
                if (selectionPrecision.Precision == 0.0f)
                    return texCoord;
                texCoord /= selectionPrecision.Precision;
                if (end)
                    texCoord = new Vector2((float)Math.Ceiling(texCoord.X), (float)Math.Ceiling(texCoord.Y));
                else
                    texCoord = new Vector2((float)Math.Floor(texCoord.X), (float)Math.Floor(texCoord.Y));
                texCoord *= selectionPrecision.Precision;
                return texCoord;
            }

            private void ProcessMouse(MouseEventArgs e)
            {
                if (!IsDestination && e.Button == MouseButtons.Left)
                {
                    FormParent._sourceStart = Quantize2(FromVisualCoord(e.Location, false), false);
                    Invalidate();
                    FormParent.destinationTextureMap.Invalidate();
                }
                else if (!IsDestination && e.Button == MouseButtons.Middle)
                {
                    FormParent._sourceEnd = Quantize2(FromVisualCoord(e.Location, false), true);
                    Invalidate();
                    FormParent.destinationTextureMap.Invalidate();
                }
                else if (IsDestination && e.Button == MouseButtons.Left)
                {
                    FormParent._destinationStart = Quantize2(FromVisualCoord(e.Location, false), false);
                    Invalidate();
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                ProcessMouse(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                ProcessMouse(e);
            }
        }
    }
}
