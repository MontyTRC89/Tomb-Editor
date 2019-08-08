using DarkUI.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster
{
	public partial class SettingsLogo : UserControl
	{
		private IDE _ide;

		public SettingsLogo()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			UpdatePreview();
		}

		private void UpdatePreview()
		{
			try
			{
				string pakFilePath = Path.Combine(_ide.Project.ProjectPath, "data", "uklogo.pak");
				byte[] pakData = PakFile.GetDecompressedData(pakFilePath);

				Image image = ImageHandling.GetImageFromRawData(pakData, 512, 256);
				panel_Preview.BackgroundImage = image;

				label_Blank.Visible = IsBlankImage(pakData);
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void button_Change_Click(object sender, EventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog
			{
				Filter = "All Supported Files|*.bmp;*.png|Bitmap Files|*.bmp|PNG Files|*.png"
			};

			if (dialog.ShowDialog(this) == DialogResult.OK)
			{
				try
				{
					using (Image image = Image.FromFile(dialog.FileName))
					{
						if (image.Width == 512 && image.Height == 256)
						{
							string pakFilePath = Path.Combine(_ide.Project.ProjectPath, "data", "uklogo.pak");

							if (Path.GetExtension(dialog.FileName).ToLower().Contains("bmp"))
							{
								byte[] rawImageData = ImageHandling.GetRawDataFromBitmap((Bitmap)image);
								PakFile.SavePakFile(pakFilePath, rawImageData);
							}
							else if (Path.GetExtension(dialog.FileName).ToLower().Contains("png"))
							{
								byte[] rawImageData = ImageHandling.GetRawDataFromImage(image);
								PakFile.SavePakFile(pakFilePath, rawImageData);
							}

							UpdatePreview();
						}
						else
							throw new ArgumentException("Wrong image size. The size of the logo has to be 512x256 px.");
					}
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

					string pakFilePath = Path.Combine(_ide.Project.ProjectPath, "data", "uklogo.pak");
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
					string sourcePathPath = Path.Combine("Templates", "uklogo.pak");
					string destPakPath = Path.Combine(_ide.Project.ProjectPath, "data", "uklogo.pak");

					File.Copy(sourcePathPath, destPakPath, true);
					UpdatePreview();
				}
				catch (Exception ex)
				{
					DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
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
	}
}
