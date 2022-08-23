using System.Drawing;
using TombLib.LevelData.VisualScripting;

namespace TombEditor.Controls.VisualScripting
{
    public partial class VisibleNodeCondition : VisibleNodeBase
    {
        private const int _condGripWidth = _gripWidth / 2;
        public VisibleNodeCondition(TriggerNode node) : base(node)
        {
            InitializeComponent();

            _grips.Add(new Rectangle(Width / 2 - _gripWidth / 2, 0, _gripWidth, _gripHeight));
            _grips.Add(new Rectangle(Width / 3 - _condGripWidth / 2, Height - _gripHeight, _condGripWidth, _gripHeight));
            _grips.Add(new Rectangle(Width - Width / 3 - _condGripWidth / 2, Height - _gripHeight, _condGripWidth, _gripHeight));
        }
    }
}
