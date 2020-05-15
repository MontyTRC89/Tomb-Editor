using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Wad;
using TombLib.Graphics;
using System.Drawing.Drawing2D;
using DarkUI.Config;
using DarkUI.Extensions;

namespace TombLib.Controls
{
    public partial class AnimationTrackBar : UserControl
    {
        private static readonly Pen _frameBorderPen = new Pen(Colors.LightestBackground.Multiply(0.7f), 1);
        private static readonly Pen _keyFrameBorderPen = new Pen(Colors.LightestBackground, 2);
        private static readonly Pen _selectionPen = new Pen(Colors.HighlightBase.MultiplyAlpha(0.8f), 1);
        private static readonly Brush _selectionBrush = new SolidBrush(Colors.HighlightBase.MultiplyAlpha(0.4f));
        private static readonly Brush _highlightBrush = new SolidBrush(Colors.GreyHighlight.MultiplyAlpha(0.7f));
        private static readonly Brush _cursorBrush = new SolidBrush(Colors.LightestBackground.MultiplyAlpha(0.6f));
        private static readonly Brush _stateChangeBrush = new SolidBrush(Color.FromArgb(30, 220, 160, 180));
        private static readonly Brush _animCommandSoundBrush = new SolidBrush(Color.FromArgb(220, 100, 170, 255));
        private static readonly Brush _animCommandFlipeffectBrush = new SolidBrush(Color.FromArgb(220, 230, 110, 110));

        private static readonly Brush _lblKeyframeBrush = new SolidBrush(Colors.LightText);
        private static readonly Brush _lblFrameBrush = new SolidBrush(Colors.LightText.MultiplyAlpha(0.3f));

        private static readonly int _cursorWidth = 6;
        private static readonly int _animCommandMarkerRadius = 14;
        private static readonly int _stateChangeMarkerThicknessDivider = 2;

        private static readonly int _highlightTimerInterval = 30;
        private static readonly float _highlightTime = 1000;
        private static readonly float _highlightStep = (float)_highlightTimerInterval / _highlightTime;

        private Timer _highlightTimer = new Timer() { Interval = 30 };
        private float _highlightCounter = 0;
        private int _highlightStart;
        private int _highlightEnd;

        private void highlightTimer_Tick(object sender, EventArgs e)
        {
            _highlightCounter -= _highlightStep;

            if (_highlightCounter <= 0.0f)
            {
                _highlightCounter = 0.0f;
                _highlightTimer.Stop();
            }

            Invalidate();
        }

        private int realFrameCount => Animation.WadAnimation.FrameRate * (Animation.DirectXAnimation.KeyFrames.Count - 1) + 1;
        private int marginWidth => picSlider.ClientSize.Width - picSlider.Padding.Horizontal - 1;
        private float frameStep => realFrameCount <= 1 ? marginWidth : (float)marginWidth / (float)(realFrameCount - 1);

        private int XtoMinMax(int x, int max, bool interpolate = true) => (int)Math.Round((double)(Minimum + (max - Minimum) * x) / (picSlider.ClientSize.Width - picSlider.Padding.Horizontal), interpolate ? MidpointRounding.ToEven : MidpointRounding.AwayFromZero);
        private int XtoRealFrameNumber(int x) => Math.Max(XtoMinMax(x, realFrameCount - 1, false), 0);
        private int XtoValue(int x) => XtoMinMax(x, Maximum, false);
        private int ValueToX(int value) => Maximum - Minimum == 0 ? 0 : (int)Math.Round((double)((picSlider.ClientSize.Width - picSlider.Padding.Horizontal) * (value - Minimum)) / (Maximum - Minimum), MidpointRounding.ToEven);

        private int _minimum;
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (_minimum == value)
                {
                    picSlider.Invalidate(); // Invalidate anyway in case other values have changed
                    return;
                }

                if (value >= _maximum || value < 0) return;

                _minimum = value;
                if (_minimum > Value) Value = _minimum;

                MinMaxChanged?.Invoke(this, new EventArgs());
                picSlider.Invalidate();
            }
        }

        private int _maximum;
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum == value)
                {
                    picSlider.Invalidate(); // Invalidate anyway in case other values have changed
                    return;
                }

                if (value < _minimum || value < 0) return;

                _maximum = value;
                if (_maximum < Value) Value = _maximum;

                MinMaxChanged?.Invoke(this, new EventArgs());
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
                if (value == _selectionStart || value < _minimum || value > _maximum)
                {
                    picSlider.Invalidate(); // Invalidate anyway in case other values have changed
                    return;
                }

                _selectionStart = value;
                if (_selectionEnd == -1) _selectionEnd = value;
                else if (_selectionStart > _selectionEnd)  _selectionEnd = _selectionStart;

                SelectionChanged?.Invoke(this, new EventArgs());
                picSlider.Invalidate();
            }
        }
        public int SelectionEnd
        {
            get { return _selectionEnd; }
            set
            {
                if (value == _selectionEnd || value < _minimum || value > _maximum)
                {
                    picSlider.Invalidate(); // Invalidate anyway in case other values have changed
                    return;
                }

                _selectionEnd = value;
                if (_selectionStart == -1) _selectionStart = value;
                else if (_selectionStart > _selectionEnd) _selectionStart = _selectionEnd;

                SelectionChanged?.Invoke(this, new EventArgs());
                picSlider.Invalidate();
            }
        }
        public int SelectionStartFrameIndex => Animation.WadAnimation.FrameRate * SelectionStart;
        public int SelectionEndFrameIndex => Animation.WadAnimation.FrameRate * SelectionEnd;

        public VectorInt2 Selection => SelectionIsEmpty ? new VectorInt2(-1, -1) : new VectorInt2(Math.Min(SelectionStart, SelectionEnd), Math.Max(SelectionStart, SelectionEnd));
        public bool SelectionIsEmpty => SelectionEnd == -1 && SelectionStart == -1;
        public int SelectionSize => SelectionIsEmpty ? 0 : Selection.Y - Selection.X + 1;

        public void ResetSelection()
        {
            _selectionStart = _selectionEnd = -1;
            SelectionChanged?.Invoke(this, new EventArgs());
            Invalidate();
        }

        public void SelectAll()
        {
            _selectionStart = _minimum;
            _selectionEnd = _maximum;
            SelectionChanged?.Invoke(this, new EventArgs());
            Invalidate();
        }

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                if (value == _value)
                {
                    picSlider.Invalidate(); // Invalidate anyway in case other values have changed
                    return;
                }
                if (value < _minimum) value = _minimum;
                if (value > _maximum) value = _maximum;

                _value = value;

                ValueChanged?.Invoke(this, new EventArgs());
                picSlider.Invalidate();
            }
        }
        public int FrameIndex => Animation.WadAnimation.FrameRate * Value;

        public void ValueLoopInc() { if (Value < Maximum) Value++; else Value = 0; }
        public void ValueLoopDec() { if (Value > Minimum) Value--; else Value = Maximum; }
                
        public override Color BackColor
        {
            get { return picSlider.BackColor; }
            set { picSlider.BackColor = value; picSlider.Invalidate(); }
        }

        public event EventHandler<WadAnimCommand> AnimCommandDoubleClick; // Happens when user double-clicks on frame with frame-based animcommands
        public event EventHandler ValueChanged;
        public event EventHandler SelectionChanged;
        public event EventHandler MinMaxChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimationNode Animation { get; set; }

        private bool mouseDown = false;

        public AnimationTrackBar()
        {
            InitializeComponent();
            picSlider.MouseWheel += picSlider_MouseWheel;
            picSlider.MouseEnter += picSlider_MouseEnter;
            _highlightTimer.Tick += highlightTimer_Tick;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Colors.GreyBackground;
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            picSlider.Invalidate();
            base.OnInvalidated(e);
        }

        public void Highlight(int start, int end)
        {
            if (start > end || start < _minimum || end > _maximum) return;
            _highlightStart = start;
            _highlightEnd = end;
            _highlightCounter = 1.0f;
            _highlightTimer.Start();
        }
        public void Highlight() => Highlight(Minimum, Maximum);

        public void SelectCurrentValue()
        {
            _selectionStart = Value;
            _selectionEnd = Value;
            SelectionChanged?.Invoke(this, new EventArgs());
        }



        private void picSlider_SizeChanged(object sender, EventArgs e) => picSlider.Invalidate();

        private void picSlider_MouseEnter(object sender, EventArgs e)
        {
            if (Form.ActiveForm == FindForm())
                picSlider.Focus();
        }

        private void picSlider_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;

            // Fix selection dimensions after possible reverse dragging
            if (_selectionStart > _selectionEnd)
            {
                int v = _selectionStart;
                _selectionStart = _selectionEnd;
                _selectionEnd = v;
            }
        }

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

            // Invoke parent event
            OnMouseDown(e);
        }

        private void picSlider_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int targetFrame = XtoRealFrameNumber(e.X);
            if (targetFrame < 0) targetFrame = 0;

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
                if (SelectionIsEmpty)
                    SelectionStart = XtoValue(e.X);

                // Manually update selection end to allow "reverse dragging"
                int potentialNewSelection = XtoValue(e.X);
                if (!SelectionIsEmpty && potentialNewSelection != _selectionEnd && potentialNewSelection >= _minimum && potentialNewSelection <= _maximum)
                {
                    _selectionEnd = XtoValue(e.X);
                    SelectionChanged?.Invoke(this, new EventArgs());
                }

                Invalidate();
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
            if (Animation == null) errorMessage = "No animation! Create or select animation to start editing.";
            else if (Animation.DirectXAnimation.KeyFrames.Count == 0) errorMessage = "No frames! Add some frames to start editing.";
            
            if(!string.IsNullOrEmpty(errorMessage))
            {
                using (var b = new SolidBrush(Colors.DisabledText))
                    e.Graphics.DrawString(errorMessage, Font, b, ClientRectangle,
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

            int halfCursorWidth = (int)Math.Round(_cursorWidth / 2.0f);

            // Shift the cursor at start/stop positions to prevent clipping
            int addShift = -halfCursorWidth;
            if (Value == 0) addShift += halfCursorWidth;
            else if (Value == Maximum) addShift += -halfCursorWidth;

            // Draw selection and highlight in 2 passes
            for (int passes = 0; passes < 2; passes++)
            {
                int x = passes == 0 ? Selection.X : _highlightStart;
                int y = passes == 0 ? Selection.Y : _highlightEnd;
                int realX = ValueToX(x);
                int realY = ValueToX(y);
                int size = realY - realX;

                if ((passes == 0 && !SelectionIsEmpty) || (passes == 1 && _highlightTimer.Enabled))
                {
                    int width = size == 0 ? _cursorWidth : size + _cursorWidth;
                    Rectangle rect = new Rectangle(realX + picSlider.Padding.Left - halfCursorWidth, picSlider.Padding.Top, width, picSlider.ClientSize.Height - picSlider.Padding.Bottom); ;

                    if (size == 0)
                    {
                        if (y != Minimum && y == Maximum) rect.X -= halfCursorWidth;
                        else if (x == Minimum) rect.X += halfCursorWidth;
                    }
                    else
                    {
                        if (x == Minimum)
                        {
                            rect.X += halfCursorWidth;
                            rect.Width -= halfCursorWidth;
                        }
                        if (y == Maximum)
                        {
                            rect.Width -= halfCursorWidth;
                        }
                    }

                    if(passes == 0)
                    {
                        e.Graphics.FillRectangle(_selectionBrush, rect);
                        e.Graphics.DrawRectangle(_selectionPen, rect);
                    }
                    else
                    {
                        using (SolidBrush currBrush = (SolidBrush)_highlightBrush.Clone())
                        {
                            currBrush.Color = Color.FromArgb((int)((float)currBrush.Color.A * _highlightCounter), currBrush.Color);
                            e.Graphics.FillRectangle(currBrush, rect);
                        }
                    }
                }
            }

            // Measure maximum label size
            SizeF maxLabelSize = TextRenderer.MeasureText(realFrameCount.ToString(), Font,
                                                          new Size(picSlider.Width - picSlider.Padding.Horizontal, picSlider.Height - picSlider.Padding.Vertical),
                                                          TextFormatFlags.WordBreak);

            // Precache some variables for speeding up renderer with ultra-long animations (5000+ frames)
            var step = frameStep;
            var padding = picSlider.Padding;
            var drawStepWidth = _keyFrameBorderPen.Width * 4;

            // Draw frame-specific animcommands, numericals and dividers
            for (int passes = 0; passes < 2; passes++)
                for (int i = 0; i < realFrameCount; ++i)
                {
                    int  currX = (int)Math.Round(step * i, MidpointRounding.AwayFromZero) + padding.Left;
                    bool isKeyFrame = (i % (Animation.WadAnimation.FrameRate == 0 ? 1 : Animation.WadAnimation.FrameRate) == 0);
                    bool first = i == 0;
                    bool last  = i >= realFrameCount - 1;

                    if (passes == 0)
                    {
                        int count = 0;

                        // Draw animcommands
                        foreach (var ac in Animation.WadAnimation.AnimCommands)
                        {
                            Rectangle currRect = new Rectangle(currX - _animCommandMarkerRadius / 2, padding.Top - _animCommandMarkerRadius / 2 + (_animCommandMarkerRadius / 3 * count), _animCommandMarkerRadius, _animCommandMarkerRadius);
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

                        // Draw frame lines.
                        // Determine if labels are overlapping and decide on drawing
                        bool drawCurrentLine = true;

                        if (step < drawStepWidth)
                        {
                            int period = (int)Math.Round(drawStepWidth / step, MidpointRounding.AwayFromZero);
                            if (i % period != 0) drawCurrentLine = false;
                        }

                        if (drawCurrentLine)
                        {
                            e.Graphics.SmoothingMode = SmoothingMode.Default;

                            var lineHeight = picSlider.Height / (isKeyFrame ? 2 : 3);
                            if (isKeyFrame)
                                e.Graphics.DrawLine(_keyFrameBorderPen, currX, padding.Top, currX, lineHeight);  // Draw keyframe
                            else
                                e.Graphics.DrawLine(_frameBorderPen, currX, padding.Top, currX, lineHeight);  // Draw ordinary frame

                            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                        }

                    }

                    // Draw cursor on 2nd pass's first occurence (only for real animations, not for single-frame ones)
                    if (i == 0 && passes == 1 && realFrameCount > 1)
                        e.Graphics.FillRectangle(_cursorBrush, new RectangleF(ValueToX(Value) + addShift + padding.Left, padding.Top, _cursorWidth, picSlider.ClientSize.Height - padding.Bottom - 2));

                    // Draw labels
                    bool drawCurrentLabel = true;
                    if ((passes == 0 && !isKeyFrame) || (passes != 0 && isKeyFrame))
                    {
                        // Determine if labels are overlapping and decide on drawing
                        if (step < maxLabelSize.Width * 1.25)
                        {
                            int period = (int)Math.Round(maxLabelSize.Width * 1.25 / step, MidpointRounding.AwayFromZero);
                            if (i % period != 0) drawCurrentLabel = false;
                        }

                        if (drawCurrentLabel)
                        {
                            // Align first and last numerical entries so they are not concealed by control border
                            StringAlignment align = StringAlignment.Center;
                            int shift = 0;
                            if (first)
                            {
                                shift -= padding.Left;
                                align = StringAlignment.Near;
                            }
                            else if (last)
                            {
                                shift += padding.Left;
                                align = StringAlignment.Far;
                            }

                            // Finally draw it after all these tests
                            e.Graphics.DrawString(i.ToString(), Font, (isKeyFrame ? _lblKeyframeBrush : _lblFrameBrush), currX + shift, picSlider.Height,
                                                    new StringFormat { Alignment = align, LineAlignment = StringAlignment.Far });
                        }
                    }
            }

            // Draw horizontal guide (only for real anims, for single-frame anims we wouldn't wanna show that
            if(realFrameCount > 1)
            {
                e.Graphics.SmoothingMode = SmoothingMode.Default;
                e.Graphics.DrawLine(_keyFrameBorderPen, padding.Left, padding.Top + 1, picSlider.ClientSize.Width - padding.Left, padding.Top + 1);
            }
        }
    }
}
