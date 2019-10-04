using DarkUI.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class SettingsLogo : UserControl
	{
		private IDE _ide;

		#region Initialization

		public SettingsLogo()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			UpdatePreview();
		}

		#endregion Initialization

		#region Events

		private void button_Change_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = "All Supported Files|*.bmp;*.png|Bitmap Files|*.bmp|PNG Files|*.png";

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					try
					{
						using (Image image = Image.FromFile(dialog.FileName))
						{
							if (image.Width != 512 || image.Height != 256)
								throw new ArgumentException("Wrong image size. The size of the logo has to be 512x256 px.");

							string pakFilePath = Path.Combine(_ide.Project.EnginePath, @"data\uklogo.pak");
							byte[] rawImageData = null;

							if (Path.GetExtension(dialog.FileName).ToLower() == ".bmp")
								rawImageData = ImageHandling.GetRawDataFromBitmap((Bitmap)image);
							else if (Path.GetExtension(dialog.FileName).ToLower() == ".png")
								rawImageData = ImageHandling.GetRawDataFromImage(image);

							PakFile.SavePakFile(pakFilePath, rawImageData);
							UpdatePreview();
						}
					}
					catch (Exception ex)
					{
						DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void button_UseBlank_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to apply a blank image?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				try
				{
					Bitmap bitmap = new Bitmap(512, 256);

					using (Graphics graphics = Graphics.FromImage(bitmap))
					{
						Rectangle imageSize = new Rectangle(0, 0, 512, 256);
						graphics.FillRectangle(Brushes.Black, imageSize);
					}

					string pakFilePath = Path.Combine(_ide.Project.EnginePath, @"data\uklogo.pak");
					byte[] rawImageData = ImageHandling.GetRawDataFromBitmap(bitmap);

					PakFile.SavePakFile(pakFilePath, rawImageData);
					UpdatePreview();
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void button_Reset_Click(object sender, EventArgs e)
		{
			DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to restore the default image?", "Are you sure?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				try
				{
					string sourcePakPath = string.Empty;

					if (_ide.Project.GameVersion == TRVersion.Game.TR4 || _ide.Project.GameVersion == TRVersion.Game.TRNG)
						sourcePakPath = Path.Combine(SharedMethods.GetProgramDirectory(), @"Templates\TOMB4\Defaults", "uklogo.pak");
					else if (_ide.Project.GameVersion == TRVersion.Game.TR5Main)
						sourcePakPath = Path.Combine(SharedMethods.GetProgramDirectory(), @"Templates\TOMB5\Defaults", "uklogo.pak");

					string destPakPath = Path.Combine(_ide.Project.EnginePath, @"data\uklogo.pak");

					File.Copy(sourcePakPath, destPakPath, true);
					UpdatePreview();
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		#endregion Events

		#region Methods

		private void UpdatePreview()
		{
			try
			{
				string pakFilePath = Path.Combine(_ide.Project.EnginePath, @"data\uklogo.pak");
				byte[] pakData = PakFile.GetDecompressedData(pakFilePath);

				panel_Preview.BackgroundImage = ImageHandling.GetImageFromRawData(pakData, 512, 256);

				label_Blank.Visible = IsBlankImage(pakData);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private bool IsBlankImage(byte[] imageData)
		{
			foreach (byte byteInfo in imageData)
			{
				if (byteInfo != 0)
					return false;
			}

			return true;
		}

		#endregion Methods
	}
}
