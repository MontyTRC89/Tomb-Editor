using System;
using System.Windows.Forms;

namespace TombEditor.Controls
{
    public class MovementTimer : IDisposable
    {
        private readonly Timer _moveTimer = new Timer() { Interval = 16 };
        private readonly float _moveAcceleration;
        private EventHandler _moveEvent;

        public float MoveMultiplier { get; private set; }
        public Keys MoveDirection { get; private set; } = Keys.None;

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
            if (moveDirection == Keys.Up ||
                moveDirection == Keys.Down ||
                moveDirection == Keys.Left ||
                moveDirection == Keys.Right ||
                moveDirection == Keys.PageDown ||
                moveDirection == Keys.PageUp)
            {
                if (_moveTimer.Enabled == false)
                    _moveTimer.Enabled = true;
                if (MoveDirection != moveDirection)
                {
                    MoveDirection = moveDirection;
                    MoveMultiplier = 0.0f;
                }
                return true;
            }
            else
                return false;
        }

        public void Stop()
        {
            _moveTimer.Enabled = false;
            MoveMultiplier = 0.0f;
            MoveDirection = Keys.None;
        }

        private void UpdateTimer(object sender, EventArgs e)
        {
            if (MoveMultiplier < 1.0f)
                MoveMultiplier += _moveAcceleration;
            else
                MoveMultiplier = 1.0f;
        }
    }
}
