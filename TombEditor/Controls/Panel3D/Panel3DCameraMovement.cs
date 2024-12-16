using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.LevelData;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (_movementTimer.Animating)
            {
                if (_movementTimer.Mode == AnimationMode.Snap)
                {
                    var lerpedRot = Vector2.Lerp(_lastCameraRot, _nextCameraRot, _movementTimer.MoveMultiplier);
                    Camera.Target = Vector3.Lerp(_lastCameraPos, _nextCameraPos, _movementTimer.MoveMultiplier);
                    Camera.RotationX = lerpedRot.X;
                    Camera.RotationY = lerpedRot.Y;
                    Camera.Distance = (float)MathC.Lerp(_lastCameraDist, _nextCameraDist, _movementTimer.MoveMultiplier);
                }
                Invalidate();
            }
            else
            {
                switch (_movementTimer.MoveKey)
                {
                    case Keys.Up:
                        Camera.Rotate(0, -_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;

                    case Keys.Down:
                        Camera.Rotate(0, _editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;

                    case Keys.Left:
                        Camera.Rotate(_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier, 0);
                        Invalidate();
                        break;

                    case Keys.Right:
                        Camera.Rotate(-_editor.Configuration.Rendering3D_NavigationSpeedKeyRotate * _movementTimer.MoveMultiplier, 0);
                        Invalidate();
                        break;

                    case Keys.PageUp:
                        Camera.Zoom(-_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;

                    case Keys.PageDown:
                        Camera.Zoom(_editor.Configuration.Rendering3D_NavigationSpeedKeyZoom * _movementTimer.MoveMultiplier);
                        Invalidate();
                        break;
                }
            }
        }

        private void FlyModeTimer_Tick(object sender, EventArgs e)
        {
            if (_lastWindow != GetForegroundWindow() || filter.IsKeyPressed(Keys.Escape))
            {
                ToggleFlyMode(false);
                _lastWindow = GetForegroundWindow();
                return;
            }

            Capture = true;

            Invalidate();
            var step = (float)_watch.Elapsed.TotalSeconds - _flyModeTimer.Interval / 1000.0f;

            step *= 500;
            step *= _editor.Configuration.Rendering3D_FlyModeMoveSpeed;

            /* Camera position handling */
            var newCameraPos = new Vector3();
            var cameraMoveSpeed = _editor.Configuration.Rendering3D_FlyModeMoveSpeed * 5 + step;

            if (ModifierKeys.HasFlag(Keys.Shift))
                cameraMoveSpeed *= 2;
            else if (ModifierKeys.HasFlag(Keys.Control))
                cameraMoveSpeed /= 2;

            if (filter.IsKeyPressed(Keys.W))
                newCameraPos.Z -= cameraMoveSpeed;

            if (filter.IsKeyPressed(Keys.A))
                newCameraPos.X += cameraMoveSpeed;

            if (filter.IsKeyPressed(Keys.S))
                newCameraPos.Z += cameraMoveSpeed;

            if (filter.IsKeyPressed(Keys.D))
                newCameraPos.X -= cameraMoveSpeed;

            Camera.MoveCameraPlane(newCameraPos);

            var room = GetCurrentRoom();

            if (room != null)
                _editor.SelectedRoom = room;

            /* Camera rotation handling */
            var cursorPos = PointToClient(Cursor.Position);

            float relativeDeltaX = (cursorPos.X - _lastMousePosition.X) / (float)Height;
            float relativeDeltaY = (cursorPos.Y - _lastMousePosition.Y) / (float)Height;

            if (cursorPos.X <= 0)
                Cursor.Position = new Point(Cursor.Position.X + Width - 2, Cursor.Position.Y);
            else if (cursorPos.X >= Width - 1)
                Cursor.Position = new Point(Cursor.Position.X - Width + 2, Cursor.Position.Y);

            if (cursorPos.Y <= 0)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + Height - 2);
            else if (cursorPos.Y >= Height - 1)
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - Height + 2);

            if (cursorPos.X - _lastMousePosition.X >= (float)Width / 2 || cursorPos.X - _lastMousePosition.X <= -(float)Width / 2)
                relativeDeltaX = 0;

            if (cursorPos.Y - _lastMousePosition.Y >= (float)Height / 2 || cursorPos.Y - _lastMousePosition.Y <= -(float)Height / 2)
                relativeDeltaY = 0;

            Camera.Rotate(
                relativeDeltaX * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate,
                -relativeDeltaY * _editor.Configuration.Rendering3D_NavigationSpeedMouseRotate);

            _gizmo.MouseMoved(_viewProjection, GetRay(cursorPos.X, cursorPos.Y));

            _lastMousePosition = cursorPos;
        }

        public void ToggleFlyMode(bool state)
        {
            if (state == true)
            {
                _lastWindow = GetForegroundWindow();

                _oldCamera = Camera;
                Camera = new FreeCamera(_oldCamera.GetPosition(), _oldCamera.RotationX, _oldCamera.RotationY - (float)Math.PI,
                    _oldCamera.MinRotationX, _oldCamera.MaxRotationX, _oldCamera.FieldOfView);

                Cursor.Hide();

                _flyModeTimer.Start();
            }
            else
            {
                Capture = false;

                var p = Camera.GetPosition();
                var d = Camera.GetDirection();
                var t = Camera.GetTarget();

                t = p + d * Level.SectorSizeUnit;

                _oldCamera.RotationX = Camera.RotationX;
                _oldCamera.RotationY = Camera.RotationY - (float)Math.PI;

                Camera = _oldCamera;
                Camera.Distance = Level.SectorSizeUnit;
                Camera.Position = p;
                Camera.Target = t;

                Cursor.Position = PointToScreen(new Point(Width / 2, Height / 2)); // Center cursor
                Cursor.Show();

                _flyModeTimer.Stop();
            }

            _editor.FlyMode = state;
        }
    }
}
