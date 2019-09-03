using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Wad;
using TombLib.Graphics;
using System.Drawing.Drawing2D;

namespace TombLib.Controls
{
    public partial class AnimationTrackBar : UserControl
    {
        private static readonly Pen _frameBorderPen = new Pen(Color.FromArgb(140, 140, 140), 1);
        private static readonly Pen _keyFrameBorderPen = new Pen(Color.FromArgb(170, 160, 160), 2);
        private static readonly Pen _selectionPen = new Pen(Color.FromArgb(190, 140, 140, 250), 1);
        private static readonly Brush _selectionBrush = new SolidBrush(Color.FromArgb(80, 170, 170, 250));
        private static readonly Brush _cursorBrush = new SolidBrush(Color.FromArgb(180, 240, 140, 50));
        private static readonly Brush _stateChangeBrush = new SolidBrush(Color.FromArgb(30, 220, 160, 180));
        private static readonly Brush _animCommandSoundBrush = new SolidBrush(Color.FromArgb(220, 80, 80, 250));
        private static readonly Brush _animCommandFlipeffectBrush = new SolidBrush(Color.FromArgb(220, 230, 80, 20));

        private static readonly int _cursorWidth = 6;
        private static readonly int _animCommandMarkerRadius = 14;
        private static readonly int _stateChangeMarkerThicknessDivider = 2;

        private int realFrameCount => Animation.DirectXAnimation.Framerate * (Animation.DirectXAnimation.KeyFrames.Count - 1) + 1;
        private int marginWidth => picSlider.ClientSize.Width - picSlider.Padding.Horizontal - 1;
        private float frameStep => realFrameCount <= 1 ? marginWidth : (float)marginWidth / (float)(realFrameCount - 1);

        private int XtoMinMax(int x, int max, bool interpolate = true) => (int)Math.Round((double)(Minimum + (max - Minimum) * x) / (double)(picSlider.ClientSize.Width - picSlider.Padding.Horizontal), interpolate ? MidpointRounding.ToEven : MidpointRounding.AwayFromZero);
        private int XtoRealFrameNumber(int x) => XtoMinMax(x, realFrameCount - 1, false);
        private int XtoValue(int x) => XtoMinMax(x, Maximum, false);
        private int ValueToX(int value) => Maximum - Minimum == 0 ? 0 : (int)Math.Round((double)((picSlider.ClientSize.Width - picSlider.Padding.Horizontal) * (value - Minimum)) / (double)(Maximum - Minimum), MidpointRounding.ToEven);

        private int _minimum;
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (_minimum == value) return;
                if (value >= _maximum || value < 0) return;

                _minimum = value;
                picSlider.Invalidate();
            }
        }

        private int _maximum;
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum == value) return;
                if (value < _minimum || value < 0) return;

                _maximum = value;
                picSlider.Invalidate();
            }
        }

        private int _selectionStart;
        private int _selectionEnd;
        public int SelectionStart
        {
            get { return _selectionStart; }
            set
            {
                if (value == _selectionStart || value < _minimum || value > _maximum) return;
                _selectionStart = value;
                picSlider.Invalidate();
            }
        }
        public int SelectionEnd
        {
            get { return _selectionEnd; }
            set
            {
                if (value == _selectionEnd || value < _minimum || value > _maximum) return;
                _selectionEnd = value;
                picSlider.Invalidate();
            }
        }

        public VectorInt2 Selection => SelectionIsEmpty ? new VectorInt2(Value, 1) : new VectorInt2(Math.Min(SelectionStart, SelectionEnd), Math.Max(SelectionStart, SelectionEnd));
        public bool SelectionIsEmpty => SelectionEnd == SelectionStart;
        public int SelectionSize => SelectionIsEmpty ? 1 : Math.Abs(SelectionEnd - SelectionStart);
        public void ResetSelection() => SelectionEnd = SelectionStart = 0;

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                if (value == _value) return;
                if (value < _minimum) value = _minimum;
                if (value > _maximum) value = _maximum;

                _value = value;

                ValueChanged?.Invoke(this, new EventArgs());
                picSlider.Invalidate();
            }
        }

        public void ValueLoopInc() { if (Value < Maximum) Value++; else Value = 0; }
        public void ValueLoopDec() { if (Value > Minimum) Value--; else Value = Maximum; }
                
        public override Color BackColor
        {
            get { return picSlider.BackColor; }
            set { picSlider.BackColor = value; picSlider.Invalidate(); }
        }

        public event EventHandler<WadAnimCommand> AnimCommandDoubleClick; // Happens when user double-clicks on frame with frame-based animcommands
        public event EventHandler ValueChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimationNode Animation { get; set; }

        private bool mouseDown = false;

        public AnimationTrackBar()
        {
            InitializeComponent();
            picSlider.MouseWheel += picSlider_MouseWheel;
            picSlider.MouseEnter += picSlider_MouseEnter;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            picSlider.Invalidate();
            base.OnInvalidated(e);
        }

        private void picSlider_SizeChanged(object sender, EventArgs e) => picSlider.Invalidate();
        private void picSlider_MouseUp(object sender, MouseEventArgs e) => mouseDown = false;
        private void picSlider_MouseEnter(object sender, EventArgs e) => picSlider.Focus();

        private void picSlider_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;

            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && ModifierKeys == Keys.Shift))
            {
                SelectionStart = XtoValue(e.X);
                SelectionEnd = SelectionStart;
            }
            else if (e.Button == MouseButtons.Left)
                Value = XtoValue(e.X);
        }

        private void picSlider_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int targetFrame = XtoRealFrameNumber(e.X);

            // Try to find animcommand under cursor
            foreach (WadAnimCommand ac in Animation.WadAnimation.AnimCommands)
                if (ac.FrameBased && ac.Parameter1 == targetFrame)
                {
                    AnimCommandDoubleClick?.Invoke(this, ac);
                    return;
                }

            // No animcommand found, try to create new one
            WadAnimCommand newCommand = new WadAnimCommand() { Type = WadAnimCommandType.PlaySound, Parameter1 = (short)targetFrame };
            AnimCommandDoubleClick?.Invoke(this, newCommand);
        }

        private void picSlider_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0) ValueLoopInc();
            else if (e.Delta > 0) ValueLoopDec();
        }

        private void picSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown) return;

            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && ModifierKeys == Keys.Shift))
            {
                SelectionEnd = XtoValue(e.X);
            }
            else if (e.Button == MouseButtons.Left)
            {
                // Warp cursor
                if (e.X <= 0)
                    Cursor.Position = new Point(Cursor.Position.X + Width - 2, Cursor.Position.Y);
                else if (e.X >= Width - 1)
                    Cursor.Position = new Point(Cursor.Position.X - Width + 2, Cursor.Position.Y);

                Value = XtoValue(e.X);
            }
        }

        private void picSlider_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            // Any messages in case of any errors
            string errorMessage = null;
            if (Animation == null || Animation.DirectXAnimation == null || Animation.WadAnimation == null) errorMessage = "No animation! Select animation to start editing.";
            else if (Animation.DirectXAnimation.KeyFrames.Count == 0) errorMessage = "No frames! Add some frames to start editing.";

            if(!string.IsNullOrEmpty(errorMessage))
            {
                e.Graphics.DrawString(errorMessage, Font, Brushes.DarkGray, ClientRectangle,
                                      new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            
            // Draw state change ranges
            foreach (var sch in Animation.WadAnimation.StateChanges)
                foreach (var disp in sch.Dispatches)
                {
                    int realOutFrame = disp.OutFrame >= realFrameCount ? realFrameCount - 1 : disp.OutFrame;
                    e.Graphics.FillRectangle(_stateChangeBrush, new RectangleF(picSlider.Padding.Left + (disp.InFrame * frameStep), 
                        picSlider.Padding.Top, 
                        (realOutFrame - disp.InFrame) * frameStep,
                        picSlider.ClientSize.Height / _stateChangeMarkerThicknessDivider - picSlider.Padding.Bottom - 2));
                }

            // Shift the cursor at start/stop positions to prevent clipping
            int addShift = -_cursorWidth / 2;
            if (Value == 0) addShift += _cursorWidth / 2;
            else if (Value == Maximum) addShift += -_cursorWidth / 2;

            // Draw selection
            if (!SelectionIsEmpty)
            {
                var rect = new Rectangle(ValueToX(Selection.X) + picSlider.Padding.Left, picSlider.Padding.Top, ValueToX(Selection.Y) - ValueToX(Selection.X), picSlider.ClientSize.Height - picSlider.Padding.Bottom);
                e.Graphics.FillRectangle(_selectionBrush, rect);
                e.Graphics.DrawRectangle(_selectionPen, rect);
            }

            // Draw cursors
            e.Graphics.FillRectangle(_cursorBrush, new RectangleF(ValueToX(Value) + addShift + picSlider.Padding.Left, picSlider.Padding.Top, _cursorWidth, picSlider.ClientSize.Height - picSlider.Padding.Bottom - 2));

            // Draw frame-specific animcommands, numericals and dividers
            for (int passes = 0; passes < 2; passes++)
                for (int i = 0; i < realFrameCount; ++i)
                {
                    int  currX = (int)Math.Round(frameStep * i, MidpointRounding.ToEven) + picSlider.Padding.Left;
                    bool isKeyFrame = (i % (Animation.DirectXAnimation.Framerate == 0 ? 1 : Animation.DirectXAnimation.Framerate) == 0);
                    bool first = i == 0;
                    bool last  = i >= realFrameCount - 1;

                    if (passes == 0)
                    {
                        int count = 0;

                        // Draw animcommands
                        foreach (var ac in Animation.WadAnimation.AnimCommands)
                        {
                            Rectangle currRect = new Rectangle(currX - _animCommandMarkerRadius / 2, picSlider.Padding.Top - _animCommandMarkerRadius / 2 + (_animCommandMarkerRadius / 3 * count), _animCommandMarkerRadius, _animCommandMarkerRadius);
                            float startAngle = !first ? (!last ? 0   : 90 ) : 0;
                            float endAngle   = !first ? (!last ? 180 : 90 ) : 90;

                            if (ac.FrameBased && ac.Parameter1 == i)
                                using (SolidBrush currBrush = (SolidBrush)(ac.Type == WadAnimCommandType.PlaySound ? _animCommandSoundBrush : _animCommandFlipeffectBrush).Clone())
                                {
                                    currBrush.Color = Color.FromArgb((int)((float)currBrush.Color.A / (1.0f + ((float)count / 3.0f))), currBrush.Color);
                                    e.Graphics.FillPie(currBrush, currRect, startAngle, endAngle);
                                    count++;
                                }
                        }

                        // Draw dividers
                        var lineHeight = picSlider.Height / (isKeyFrame ? 2 : 3);
                        e.Graphics.DrawLine(_frameBorderPen, currX, picSlider.Padding.Top, currX, lineHeight);  // Draw ordinary frame
                        if (isKeyFrame) e.Graphics.DrawLine(_keyFrameBorderPen, currX, picSlider.Padding.Top, currX, lineHeight - 1);  // Draw keyframe
                    }

                    // Measure maximum label size
                    SizeF maxLabelSize = TextRenderer.MeasureText(realFrameCount.ToString(), Font,
                                                                  new Size(picSlider.Width - picSlider.Padding.Horizontal, picSlider.Height - picSlider.Padding.Vertical),
                                                                  TextFormatFlags.WordBreak);

                    // Draw labels
                    bool drawCurrentLabel = true;

                    if ((passes == 0 && !isKeyFrame) || (passes != 0 && isKeyFrame))
                    {
                        // Determine if labels are overlapping and decide on drawing
                        if (frameStep < maxLabelSize.Width * 1.25)
                        {
                            int period = (int)Math.Round(maxLabelSize.Width * 1.25 / frameStep, MidpointRounding.AwayFromZero);
                            if (i % period != 0) drawCurrentLabel = false;
                        }

                        if (drawCurrentLabel)
                        {
                            // Align first and last numerical entries so they are not concealed by control border
                            StringAlignment align = StringAlignment.Center;
                            int shift = 0;
                            if (first)
                            {
                                shift -= picSlider.Padding.Left;
                                align = StringAlignment.Near;
                            }
                            else if (last)
                            {
                                shift += picSlider.Padding.Left;
                                align = StringAlignment.Far;
                            }

                            // Finally draw it after all these tests
                            e.Graphics.DrawString(i.ToString(), Font, (isKeyFrame ? Brushes.White : Brushes.DimGray), currX + shift, picSlider.Height,
                                                  new StringFormat { Alignment = align, LineAlignment = StringAlignment.Far });
                        }
                    }
            }

            // Draw horizontal guide
            e.Graphics.DrawLine(_keyFrameBorderPen, picSlider.Padding.Left, picSlider.Padding.Top, picSlider.ClientSize.Width - picSlider.Padding.Left, picSlider.Padding.Top);
        }
    }
}
