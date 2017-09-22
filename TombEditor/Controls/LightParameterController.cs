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
    public enum LightParameter
    {
        Intensity,
        In,
        Out,
        Len,
        CutOff,
        DirectionX,
        DirectionY
    }

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
        
        private Color _backgroundColor = Color.FromArgb(69, 73, 74);
        private Color _borderColor = Color.Gainsboro;

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

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if(e.Delta < 0)
                butDown_MouseDown(null, e);
            else
                butUp_MouseDown(null, e);
        }

        private void SetParameters()
        {
            Light light = _editor.SelectedObject as Light;
            if (light == null)
                return;

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
                    _minValue = -90.0f;
                    _maxValue = 90.0f;

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
            Light light = _editor.SelectedObject as Light;
            if (light == null)
                return;

            // Hack for setting up light parameter ranges
            SetParameters();
            
            float newValue = _value;

            // Change parameter value
            if (ModifierKeys.HasFlag(Keys.Shift))
                newValue -= _fastStep;
            else
                newValue -= _step;

            // Check for ranges
            if ((light.Type == LightType.Spot || light.Type == LightType.Sun) && (LightParameter == LightParameter.DirectionY))
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
            Light light = _editor.SelectedObject as Light;
            if (light == null)
                return;

            // Hack for setting up light parameter ranges
            SetParameters();
            
            float newValue = _value;

            // Change parameter value
            if (ModifierKeys.HasFlag(Keys.Shift))
                newValue += _fastStep;
            else 
                newValue += _step;

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
            Light light = _editor.SelectedObject as Light;
            if (light == null)
                return;
            
            switch (LightParameter)
            {
                case LightParameter.Intensity:
                    if (light.Intensity == _value)
                        return;
                    light.Intensity = _value;
                    break;

                case LightParameter.In:
                    if (light.In == _value)
                        return;
                    light.In = _value;
                    break;

                case LightParameter.Out:
                    if (light.Out == _value)
                        return;
                    light.Out = _value;
                    break;

                case LightParameter.Len:
                    if (light.Len == _value)
                        return;
                    light.Len = _value;
                    break;

                case LightParameter.CutOff:
                    if (light.Cutoff == _value)
                        return;
                    light.Cutoff = _value;
                    break;

                case LightParameter.DirectionX:
                    if (light.RotationX == _value)
                        return;
                    light.RotationX = _value;
                    break;

                case LightParameter.DirectionY:
                    if (light.RotationY == _value)
                        return;
                    light.RotationY = _value;
                    break;
            }
                
            _editor.SelectedRoom.UpdateCompletely();
            _editor.ObjectChange(light);
        }
    }
}
