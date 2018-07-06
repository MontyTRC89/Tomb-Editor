using System;
using System.Windows.Forms;
using TombLib.Rendering;

namespace TombEditor
{
    public class SectorColoringManager : IDisposable
    {
        public class ChangeSectorColoringInfoEvent : IEditorEvent { }
        public SectorColoringInfo ColoringInfo = new SectorColoringInfo();

        private readonly Editor _editor;

        private const float _transitionSpeed = 0.335f;
        private readonly Timer _transitionAnimator = new Timer() { Interval = 50 };

        public SectorColoringManager(Editor editor)
        {
            _editor = editor;
            _transitionAnimator.Tick += UpdateTransitionAnimation;
        }

        public void Dispose()
        {
            _transitionAnimator.Stop();
            _transitionAnimator.Tick -= UpdateTransitionAnimation;
            _transitionAnimator.Dispose();
        }

        public void SetPriority(SectorColoringType type)
        {
            if (ColoringInfo.SetPriority(type, false))
                _transitionAnimator.Start();
        }

        private void UpdateTransitionAnimation(object sender, EventArgs e)
        {
            ColoringInfo.TransistionValue += _transitionSpeed;
            if (ColoringInfo.TransistionValue >= 1.0f)
            {
                ColoringInfo.TransistionValue = 1.0f;
                _transitionAnimator.Stop();
            }

            _editor.RaiseEvent(new ChangeSectorColoringInfoEvent());
        }
    }
}
