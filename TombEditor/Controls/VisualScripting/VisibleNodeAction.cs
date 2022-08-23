using System.Drawing;
using TombLib.LevelData.VisualScripting;

namespace TombEditor.Controls.VisualScripting
{
    public partial class VisibleNodeAction : VisibleNodeBase
    {
        public VisibleNodeAction(TriggerNode node) : base(node)
        {
            InitializeComponent();

            _grips.Add(new Rectangle(Width / 2 - _gripWidth / 2, 0, _gripWidth, _gripHeight));
            _grips.Add(new Rectangle(Width / 2 - _gripWidth / 2, Height - _gripHeight, _gripWidth, _gripHeight));
        }
    }
}
