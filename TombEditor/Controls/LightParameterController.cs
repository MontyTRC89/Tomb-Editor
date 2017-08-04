using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor.Controls
{
    public partial class LightParameterController : UserControl
    {
        public LightParameter LightParameter { get; set; }

        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == value)
                    return;
                _value = value;
                SetParameters();
                UpdateLightValue();
            }
        }
        
        private Color _backgroundColor = Color.FromArgb(255, 60, 63, 65);
        private Color _borderColor = Color.FromArgb(255, 171, 173, 179);

        private SolidBrush _backgroundBrush;
        private Pen _borderPen;

        private float _value;
        private float _minValue;
        private float _maxValue;

        private float _step;
        private float _fastStep;

        private int _decimals;

        private Editor _editor;

        public LightParameterController()
        {
            InitializeComponent();
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            _backgroundBrush = new SolidBrush(_backgroundColor);
            _borderPen = new Pen(_borderColor);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {}

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            g.FillRectangle(_backgroundBrush, 0, 0, 60, 22);
            g.DrawRectangle(_borderPen, 0, 0, 60, 21);

        }

        private void SetParameters()
        {
            if (!_editor.SelectedObject.HasValue || (_editor.SelectedObject.Value.Type != ObjectInstanceType.Light))
                return;

            Light light = _editor.SelectedRoom.Lights[_editor.SelectedObject.Value.Id];

            switch (LightParameter)
            {
                case LightParameter.Intensity:
                    if (light.Type == LightType.Shadow)
                    {
                        _minValue = -1.0f;
                        _maxValue = 0.0f;
                    }
                    else if (light.Type == LightType.Effect)
                    {
                        _minValue = -4.0f;
                        _maxValue = 4.0f;
                    }
                    else
                    {
                        _minValue = 0.0f;
                        _maxValue = 1.0f;
                    }

                    _step = 0.03f;
                    _fastStep = 0.12f;

                    _decimals = 2;

                    break;

                case LightParameter.In:
                    _minValue = 0.0f;
                    _maxValue = (light.Type == LightType.Spot ? 89.0f : 40.0f);

                    if (light.Type == LightType.Light || light.Type == LightType.Shadow || light.Type == LightType.FogBulb)
                    {
                        _step = 0.03f;
                        _fastStep = 1.0f;

                        _decimals = 2;
                    }
                    else
                    {
                        _step = 1.0f;
                        _fastStep = 10.0f;

                        _decimals = 0;
                    }

                    break;

                case LightParameter.Out:
                    _minValue = 0.0f;
                    _maxValue = (light.Type == LightType.Spot ? 89.0f : 40.0f);

                    if (light.Type == LightType.Light || light.Type == LightType.Shadow || light.Type == LightType.FogBulb)
                    {
                        _step = 0.03f;
                        _fastStep = 1.0f;

                        _decimals = 2;
                    }
                    else
                    {
                        _step = 1.0f;
                        _fastStep = 10.0f;

                        _decimals = 0;
                    }

                    break;

                case LightParameter.Len:
                    _minValue = 0.0f;
                    _maxValue = 40.0f;

                    _step = 0.03f;
                    _fastStep = 1.0f;

                    _decimals = 2;

                    break;

                case LightParameter.CutOff:
                    _minValue = 0.0f;
                    _maxValue = 40.0f;

                    _step = 0.03f;
                    _fastStep = 1.0f;

                    _decimals = 2;

                    break;

                case LightParameter.DirectionX:
                    _minValue = 0.0f;
                    _maxValue = 359.0f;

                    _step = 1.0f;
                    _fastStep = 10.0f;

                    _decimals = 0;

                    break;

                case LightParameter.DirectionY:
                    _minValue = 0.0f;
                    _maxValue = 359.0f;

                    _step = 1.0f;
                    _fastStep = 10.0f;

                    _decimals = 0;

                    break;
            }
        }

        private void butDown_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_editor.SelectedObject.HasValue || (_editor.SelectedObject.Value.Type != ObjectInstanceType.Light))
                return;

            // Hack for setting up light parameter ranges
            SetParameters();

            // Get the current light
            Light light = _editor.SelectedRoom.Lights[_editor.SelectedObject.Value.Id];

            float newValue = _value;

            // Change parameter value
            if (e.Button == MouseButtons.Left)
                newValue -= _step;
            if (e.Button == MouseButtons.Right)
                newValue -= _fastStep;

            // Check for ranges
            if ((light.Type == LightType.Spot || light.Type == LightType.Sun) &&
                (LightParameter == LightParameter.DirectionX || LightParameter == LightParameter.DirectionY))
            {
                if (newValue < 0.0f)
                    newValue = 360.0f + newValue;
            }
            else
            {
                newValue = Math.Max(newValue, _minValue);
            }

            _value = newValue;

            // Update light parameters value on screen
            UpdateLightValue();

            // Save the new value into the light
            ChangeLightParameter();
        }

        private void butUp_MouseDown(object sender, MouseEventArgs e)
        {
            if (!_editor.SelectedObject.HasValue || (_editor.SelectedObject.Value.Type != ObjectInstanceType.Light))
                return;

            // Hack for setting up light parameter ranges
            SetParameters();

            // Get the current light
            Light light = _editor.SelectedRoom.Lights[_editor.SelectedObject.Value.Id];

            float newValue = _value;

            // Change parameter value
            if (e.Button == MouseButtons.Left)
                newValue += _step;
            if (e.Button == MouseButtons.Right)
                newValue += _fastStep;

            // Check for ranges
            if ((light.Type == LightType.Spot || light.Type == LightType.Sun) &&
                (LightParameter == LightParameter.DirectionX || LightParameter == LightParameter.DirectionY))
            {
                if (newValue >= 360.0f)
                    newValue = newValue - 360.0f;
            }
            else
            {
                newValue = Math.Min(newValue, _maxValue);
            }

            _value = newValue;

            // Update light parameters value on screen
            UpdateLightValue();

            // Save the new value into the light
            ChangeLightParameter();
        }

        private void UpdateLightValue()
        {
            if (_decimals == 2)
                labelContent.Text = String.Format("{0:0.00}", _value);
            else
                labelContent.Text = ((int)_value).ToString();
        }

        private void ChangeLightParameter()
        {
            if (_editor.SelectedObject.HasValue && (_editor.SelectedObject.Value.Type == ObjectInstanceType.Light))
            {
                Light light = _editor.SelectedRoom.Lights[_editor.SelectedObject.Value.Id];

                switch (LightParameter)
                {
                    case LightParameter.Intensity:
                        light.Intensity = _value;
                        break;

                    case LightParameter.In:
                        light.In = _value;
                        break;

                    case LightParameter.Out:
                        light.Out = _value;
                        break;

                    case LightParameter.Len:
                        light.Len = _value;
                        break;

                    case LightParameter.CutOff:
                        light.Cutoff = _value;
                        break;

                    case LightParameter.DirectionX:
                        light.DirectionX = _value;
                        break;

                    case LightParameter.DirectionY:
                        light.DirectionY = _value;
                        break;
                }
                
                _editor.SelectedRoom.CalculateLightingForThisRoom();
                _editor.SelectedRoom.UpdateBuffers();
                _editor.ObjectChange(light);
            }
        }
    }
}
