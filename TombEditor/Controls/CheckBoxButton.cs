using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TombEditor.Controls
{
    public class CheckBoxButton : Button
    {
        private bool _state = false;

        public bool Checked
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;

                if (_state)
                    BackColor = System.Drawing.Color.LightGreen;
                else
                    BackColor = Control.DefaultBackColor;
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (Checked)
            {
                Checked = false;
                //BackColor = Parent.BackColor;
            }
            else
            {
                Checked = true;
                // = System.Drawing.Color.LightGreen;
            }

            base.OnClick(e);
        }
    }
}
