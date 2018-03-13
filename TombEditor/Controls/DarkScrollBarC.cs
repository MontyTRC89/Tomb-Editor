using DarkUI.Controls;
using System;
using System.Reflection;

namespace TombEditor.Controls
{
    // This avoids various trouble with the official DarkUI control and also allows floating point values
    // TODO Maybe fork DarkUI to fix this in the source.
    public class DarkScrollBarC : DarkScrollBar
    {
        private readonly static FieldInfo _value = typeof(DarkScrollBar).GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly static FieldInfo _viewSize = typeof(DarkScrollBar).GetField("_viewSize", BindingFlags.NonPublic | BindingFlags.Instance);

        private const int _precision = 20000;
        private double _newMinimum = 0;
        private double _newMaximum = 1;
        private bool _currentlySet = false;

        public new double Minimum => _newMinimum;
        public new double Maximum => _newMaximum;
        public new double Value => base.Value * ((_newMaximum - _newMinimum) / _precision) + _newMinimum;
        public new double ViewSize => base.ViewSize * ((_newMaximum - _newMinimum) / _precision) + _newMinimum;
        public double ValueCentered => Value + ViewSize * 0.5f;

        public new event EventHandler<ScrollValueEventArgs> ValueChanged;

        public DarkScrollBarC()
        {
            base.Minimum = 0;
            base.Maximum = _precision;

            base.ValueChanged += (sender, e) => 
            {
                if (!_currentlySet)
                    ValueChanged.Invoke(sender, e);
            };
        }

        public void SetView(double minimum, double maximum, double viewSize, double value, bool enable)
        {
            try
            {
                _currentlySet = true;

                _newMinimum = minimum;
                _newMaximum = maximum;

                double scale = _precision / (_newMaximum - _newMinimum);
                int viewSizeInt = (int)Math.Round(viewSize * scale);
                int valueInt = (int)Math.Round((value - _newMinimum) * scale);

                viewSizeInt = Math.Min(viewSizeInt, _precision - 1);
                _viewSize.SetValue(this, viewSizeInt);
                _value.SetValue(this, Math.Max(0, Math.Min(_precision - viewSizeInt, valueInt)));

                // Update control state
                if (Enabled = enable && (viewSize < _precision))
                    UpdateScrollBar();
            }
            finally
            {
                _currentlySet = false;
            }
        }

        public void SetViewCentered(double minimum, double maximum, double viewSize, double value, bool enable)
        {
            SetView(minimum, maximum, viewSize, value - viewSize * 0.5f, enable);
        }
    }
}
