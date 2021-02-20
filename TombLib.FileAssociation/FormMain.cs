using DarkUI.Forms;
using System;

namespace TombLib.FileAssociation
{
	public partial class FormMain : DarkForm
	{
		private bool _wasPRJ2Checked = false;
		private bool _wasWAD2Checked = false;
		private bool _wasTRPROJChecked = false;

		public FormMain()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			if (Association.IsPRJ2Associated())
				checkBox_prj2.Checked = true;

			if (Association.IsWAD2Associated())
				checkBox_wad2.Checked = true;

			if (Association.IsTRPROJAssociated())
				checkBox_trproj.Checked = true;

			_wasPRJ2Checked = checkBox_prj2.Checked;
			_wasWAD2Checked = checkBox_wad2.Checked;
			_wasTRPROJChecked = checkBox_trproj.Checked;

			button_Apply.Enabled = false;

			base.OnLoad(e);
		}

		private void button_Apply_Click(object sender, EventArgs e)
		{
			if (checkBox_prj2.Checked)
				Association.AssociatePRJ2();
			else
				Association.RemoveAssociation(".prj2");

			if (checkBox_wad2.Checked)
				Association.AssociateWAD2();
			else
				Association.RemoveAssociation(".wad2");

			if (checkBox_trproj.Checked)
				Association.AssociateTRPROJ();
			else
				Association.RemoveAssociation(".trproj");

			_wasPRJ2Checked = checkBox_prj2.Checked;
			_wasWAD2Checked = checkBox_wad2.Checked;
			_wasTRPROJChecked = checkBox_trproj.Checked;

			button_Apply.Enabled = false;
		}

		private void button_Close_Click(object sender, EventArgs e)
			=> Close();

		private void checkBox_CheckedChanged(object sender, EventArgs e)
			=> button_Apply.Enabled = checkBox_prj2.Checked != _wasPRJ2Checked
				|| checkBox_wad2.Checked != _wasWAD2Checked
				|| checkBox_trproj.Checked != _wasTRPROJChecked;
	}
}
