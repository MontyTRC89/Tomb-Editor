using System;
using System.Windows.Forms;
using TombLib;

namespace TombEditor.Controls
{
    public class MovementTimer : IDisposable
    {
        private readonly Timer _moveTimer = new Timer() { Interval = 16 };
        private readonly float _moveAcceleration;
        private EventHandler _moveEvent;

        private float _autoMoveStep = 0.0f;
        private float _autoMoveCurrent = 0.0f;

        public float MoveMultiplier { get; private set; }
        public Keys MoveKey { get; private set; } = Keys.None;
        public bool Animating { get; private set; } = false;

        public MovementTimer(EventHandler moveEvent, float moveAcceleration = 0.02f)
        {
            _moveTimer.Tick += UpdateTimer;

            _moveEvent = moveEvent;
            _moveAcceleration = moveAcceleration;
            _moveTimer.Tick += _moveEvent;
        }

        public void Dispose()
        {
            _moveTimer.Tick -= _moveEvent;
            _moveEvent = null;
        }

        public bool Engage(Keys moveDirection)
        {
            if (Animating) return false; // Reject if auto-move is in progress

            if (Hotkey.ReservedCameraKeys.Contains(moveDirection))
            {
                if (_moveTimer.Enabled == false)
                    _moveTimer.Enabled = true;
                if (MoveKey != moveDirection)
                {
                    MoveKey = moveDirection;
                    MoveMultiplier = 0.0f;
                }
                return true;
            }
            else
                return false;
        }

        public void Animate(float timeInSeconds)
        {
            Animating = true;
            _autoMoveCurrent = 0.0f;
            _autoMoveStep = (1.0f / (float)_moveTimer.Interval) / timeInSeconds;
            _moveTimer.Enabled = true;
        }

        public void Stop(bool force = false)
        {
            if (Animating & !force) return; // Reject if auto-move is in progress
            
            _moveTimer.Enabled = false;
            Animating = false;
            MoveMultiplier = 0.0f;
            MoveKey = Keys.None;
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            if(Animating)
            {
                _autoMoveCurrent += _autoMoveStep;

                // Allow timer to overflow for 2 subsequent frames, so we
                // get finalizing 1.0 value at all times

                if(_autoMoveCurrent >= 1.0f + (_autoMoveStep * 2))
                    Stop(true);
                else
                {
                    var newMultiplier = MathC.SmoothStep(0.0, 1.0, _autoMoveCurrent);
                    MoveMultiplier = (float)MathC.Clamp(newMultiplier, 0.0, 1.0);
                }
            }
            else
            {
                if (MoveMultiplier < 1.0f)
                    MoveMultiplier += _moveAcceleration;
                else
                    MoveMultiplier = 1.0f;
            }
        }
    }
}
