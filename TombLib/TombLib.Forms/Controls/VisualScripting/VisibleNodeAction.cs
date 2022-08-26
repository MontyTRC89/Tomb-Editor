using System;
using System.Drawing;
using TombLib.LevelData.VisualScripting;

namespace TombLib.Controls.VisualScripting
{
    public partial class VisibleNodeAction : VisibleNodeBase
    {
        public VisibleNodeAction(TriggerNode node) : base(node)
        {
            InitializeComponent();
        }

        protected override void SpawnGrips()
        {
            base.SpawnGrips();
            _grips.Add(new Rectangle(Width / 2 - _gripWidth / 2, Height - _gripHeight, _gripWidth, _gripHeight));
        }

        private void cbAction_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: Dynamic population of ArgumentEditor!

            Editor?.Invalidate(); // Leave here for proper grip redraw
        }
    }
}
