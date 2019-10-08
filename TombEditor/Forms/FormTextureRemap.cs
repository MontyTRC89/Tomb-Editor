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
                sourceTextureMap.End = firstTexture.Image.Size;
        }

        private bool SourceContains(Vector2 texCoord)
        {
            return sourceTextureMap.Start.X <= texCoord.X && sourceTextureMap.Start.Y <= texCoord.Y &&
                sourceTextureMap.End.X >= texCoord.X && sourceTextureMap.End.Y >= texCoord.Y;
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
                DarkMessageBox.Show(this, "Source or destination texture may not be unset.", "Problem", MessageBoxIcon.Exclamation);
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
                            currentTextureArea.TexCoord0 += destinationTextureMap.Start - sourceTextureMap.Start;
                            currentTextureArea.TexCoord1 += destinationTextureMap.Start - sourceTextureMap.Start;
                            currentTextureArea.TexCoord2 += destinationTextureMap.Start - sourceTextureMap.Start;
                            currentTextureArea.TexCoord3 += destinationTextureMap.Start - sourceTextureMap.Start;
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
                            frame.TexCoord0 += destinationTextureMap.Start - sourceTextureMap.Start;
                            frame.TexCoord1 += destinationTextureMap.Start - sourceTextureMap.Start;
                            frame.TexCoord2 += destinationTextureMap.Start - sourceTextureMap.Start;
                            frame.TexCoord3 += destinationTextureMap.Start - sourceTextureMap.Start;
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
                "Replace another texture?", "Succcess", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
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

            public Vector2 Start { get; set; }
            public Vector2 End
            {
                get { return _end; }
                set
                {
                    _end = value;

                    if (!IsDestination)
                    {
                        bool clamped = false;

                        var size = End - Start;
                        var destStart = FormParent.destinationTextureMap.Start;
                        var destEnd = destStart + size;

                        if (destEnd.X > VisibleTexture.Image.Size.X)
                        {
                            clamped = true;
                            destStart.X -= destEnd.X - VisibleTexture.Image.Size.X;
                        }

                        if (destEnd.Y > VisibleTexture.Image.Size.Y)
                        {
                            clamped = true;
                            destStart.Y -= destEnd.Y - VisibleTexture.Image.Size.Y;
                        }

                        if (clamped)
                            FormParent.destinationTextureMap.Start = destStart;
                    }
                }
            }
            private Vector2 _end;

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
                    DrawArea(FormParent.destinationTextureMap.Start, FormParent.destinationTextureMap.Start + (FormParent.sourceTextureMap.End - FormParent.sourceTextureMap.Start), e.Graphics);
                else
                    DrawArea(FormParent.sourceTextureMap.Start, FormParent.sourceTextureMap.End, e.Graphics);
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
                    FormParent.sourceTextureMap.End = Quantize2(FromVisualCoord(e.Location, true), true);
                    Invalidate();

                    FormParent.destinationTextureMap.Invalidate();
                }
                else if (IsDestination && e.Button == MouseButtons.Left)
                {
                    var newStart = Quantize2(FromVisualCoord(e.Location, false), false);
                    var size = FormParent.sourceTextureMap.End - FormParent.sourceTextureMap.Start;

                    var finalStart = FormParent.destinationTextureMap.Start;

                    if (newStart.X >= 0 && newStart.X + size.X <= VisibleTexture.Image.Size.X)
                        finalStart.X = newStart.X;

                    if (newStart.Y >= 0 && newStart.Y + size.Y <= VisibleTexture.Image.Size.Y)
                        finalStart.Y = newStart.Y;

                    FormParent.destinationTextureMap.Start = finalStart;

                    Invalidate();
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (!IsDestination)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        var newStart = Quantize2(FromVisualCoord(e.Location, false), false);
                        if (newStart.X >= 0 && newStart.Y >= 0 && 
                            newStart.X < VisibleTexture.Image.Size.X && newStart.Y < VisibleTexture.Image.Size.Y)
                        {
                            FormParent.sourceTextureMap.Start = newStart;
                            Invalidate();
                            FormParent.destinationTextureMap.Invalidate();
                        }
                    }
                }

                base.OnMouseDown(e);
                ProcessMouse(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                ProcessMouse(e);
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                ProcessMouse(e);
            }
        }
    }
}
