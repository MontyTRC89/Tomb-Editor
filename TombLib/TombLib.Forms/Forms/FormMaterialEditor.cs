using DarkUI.Forms;
using Microsoft.Build.Tasks;
using NLog;
using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using static TombLib.LevelData.Compilers.TombEngineTexInfoManager;

namespace TombLib.Forms
{
	public partial class FormMaterialEditor : DarkForm
	{
		public string MaterialFileName { get; set; }

		private MaterialData _materialData;
		private string _texturePath;

		private readonly Color _correctColor;
		private readonly Color _wrongColor;

		private bool _saveXml = false;

		public FormMaterialEditor(string texturePath)
		{
			InitializeComponent();

			_correctColor = tbNormalMapPath.BackColor;
			_wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);

			_texturePath = texturePath;
			_materialData = MaterialData.TrySidecarLoadOrLoadExisting(texturePath);
		}

		private void FormMaterialEditor_Load(object sender, EventArgs e)
		{
			tbColorMapPath.Text = _materialData.ColorMap;
			tbNormalMapPath.Text = _materialData.NormalMap;
			tbSpecularMapPath.Text = _materialData.SpecularMap;
			tbAmbientOcclusionMapPath.Text = _materialData.AmbientOcclusionMap;
			tbEmissiveMapPath.Text = _materialData.EmissiveMap;

			if (!string.IsNullOrEmpty(_materialData.NormalMap))
				tbNormalMapPath.BackColor = (_materialData.IsNormalMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.AmbientOcclusionMap))
				tbAmbientOcclusionMapPath.BackColor = (_materialData.IsAmbientOcclusionMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.SpecularMap))
				tbSpecularMapPath.BackColor = (_materialData.IsSpecularMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.EmissiveMap))
				tbEmissiveMapPath.BackColor = (_materialData.IsEmissiveMapFound ? _correctColor : _wrongColor);

			LoadTexturePreview(_materialData.ColorMap, picPreviewColorMap);
			LoadTexturePreview(_materialData.NormalMap, picPreviewNormalMap);
			LoadTexturePreview(_materialData.SpecularMap, picPreviewSpecularMap);
			LoadTexturePreview(_materialData.AmbientOcclusionMap, picPreviewAmbientOcclusionMap);
			LoadTexturePreview(_materialData.EmissiveMap, picPreviewEmissiveMap);

			lblXmlMaterialFile.Text = _materialData.XmlMaterialFileName;

			comboMaterialType.SelectedIndex = (int)_materialData.Type;
			UpdateMaterialProperties();
		}

		private void LoadTexturePreview(string path, PictureBox pictureBox)
		{
			if (string.IsNullOrEmpty(path))
			{
				pictureBox.Image?.Dispose();
				pictureBox.Image = null;
				pictureBox.BackgroundImage = TombLib.Properties.Resources.misc_TransparentBackground;
			}
			else
			{
				try
				{
					if (!string.IsNullOrEmpty(path))
					{

						pictureBox.Image?.Dispose();
						pictureBox.Image = ImageC.FromFile(path).ToBitmap();
						pictureBox.BackgroundImage = TombLib.Properties.Resources.misc_TransparentBackground;
						pictureBox.Tag = null;
						pictureBox.BackColor = _correctColor;
					}
				}
				catch (Exception exc)
				{
					pictureBox.Image = null;
					pictureBox.BackgroundImage = null;
					pictureBox.Tag = exc;
					pictureBox.BackColor = _wrongColor;
				}
			}
		}

		private void UpdateMaterialProperties()
		{
			var materialType = (MaterialType)comboMaterialType.SelectedIndex;

			switch (materialType)
			{
				case MaterialType.Opaque:
					tabcontainerParameters.SelectedIndex = 0;
					nmNormalMapStrength.Value = (decimal)_materialData.Parameters0.X;
					nmSpecularIntensity.Value = (decimal)(_materialData.Parameters0.Y);
					break;
			}
		}

		private void butCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void butOK_Click(object sender, EventArgs e)
		{
			if (_saveXml)
			{
				string externalMaterialDataPath = Path.Combine(
						 Path.GetDirectoryName(_texturePath),
						 Path.GetFileNameWithoutExtension(_texturePath) + ".xml");

				var materialData = new MaterialData();

				materialData.Type = (MaterialType)comboMaterialType.SelectedIndex;

				materialData.ColorMap = _texturePath;
				materialData.NormalMap = tbNormalMapPath.Text;
				materialData.SpecularMap = tbSpecularMapPath.Text;
				materialData.EmissiveMap = tbEmissiveMapPath.Text;
				materialData.AmbientOcclusionMap = tbAmbientOcclusionMapPath.Text;

				switch (materialData.Type)
				{
					case MaterialType.Opaque:
						materialData.Parameters0 = new Vector4(
								(float)nmNormalMapStrength.Value,
								(float)nmSpecularIntensity.Value,
								0.0f,
								0.0f);
						break;
				}

				try
				{
					File.Delete(externalMaterialDataPath);
					MaterialData.SaveToXml(externalMaterialDataPath, materialData);
					MaterialFileName = externalMaterialDataPath;
				}
				catch (Exception)
				{
					DarkMessageBox.Show(this, $"An error occurred while saving XML material file to '{externalMaterialDataPath}'", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void comboMaterialType_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateMaterialProperties();
		}

		private void butBrowseNormalMap_Click(object sender, EventArgs e)
		{
			var texturePath = LevelFileDialog.BrowseFile(this, "Browse texture", ImageC.FileExtensions, false);
			if (!string.IsNullOrEmpty(texturePath))
			{
				tbNormalMapPath.Text = texturePath;
				tbNormalMapPath.BackColor = _correctColor;
				LoadTexturePreview(texturePath, picPreviewNormalMap);
				_saveXml = true;
			}
		}

		private void butBrowseAmbientOcclusionMap_Click(object sender, EventArgs e)
		{
			var texturePath = LevelFileDialog.BrowseFile(this, "Browse texture", ImageC.FileExtensions, false);
			if (!string.IsNullOrEmpty(texturePath))
			{
				tbAmbientOcclusionMapPath.Text = texturePath;
				tbAmbientOcclusionMapPath.BackColor = _correctColor;
				LoadTexturePreview(texturePath, picPreviewAmbientOcclusionMap);
				_saveXml = true;
			}
		}

		private void butBrowseEmissiveMap_Click(object sender, EventArgs e)
		{
			var texturePath = LevelFileDialog.BrowseFile(this, "Browse texture", ImageC.FileExtensions, false);
			if (!string.IsNullOrEmpty(texturePath))
			{
				tbEmissiveMapPath.Text = texturePath;
				tbEmissiveMapPath.BackColor = _correctColor;
				LoadTexturePreview(texturePath, picPreviewEmissiveMap);
				_saveXml = true;
			}
		}

		private void butBrowseSpecularMap_Click(object sender, EventArgs e)
		{
			var texturePath = LevelFileDialog.BrowseFile(this, "Browse texture", ImageC.FileExtensions, false);
			if (!string.IsNullOrEmpty(texturePath))
			{
				tbSpecularMapPath.Text = texturePath;
				tbSpecularMapPath.BackColor = _correctColor;
				LoadTexturePreview(texturePath, picPreviewSpecularMap);
				_saveXml = true;
			}
		}

		private void butClearNormalMap_Click(object sender, EventArgs e)
		{
			if (DarkMessageBox.Show(this, "Do you really want to clear the normal map?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				tbNormalMapPath.Text = "";
				tbSpecularMapPath.BackColor = this.BackColor;
				LoadTexturePreview("", picPreviewNormalMap);
				_saveXml = true;
			}
		}

		private void butClearAmbientOcclusionMap_Click(object sender, EventArgs e)
		{
			if (DarkMessageBox.Show(this, "Do you really want to clear the ambient occlusion map?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				tbAmbientOcclusionMapPath.Text = "";
				tbSpecularMapPath.BackColor = this.BackColor;
				LoadTexturePreview("", picPreviewAmbientOcclusionMap);
				_saveXml = true;
			}
		}

		private void butClearEmissiveMap_Click(object sender, EventArgs e)
		{
			if (DarkMessageBox.Show(this, "Do you really want to clear the emissive map?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				tbEmissiveMapPath.Text = "";
				tbSpecularMapPath.BackColor = this.BackColor;
				LoadTexturePreview("", picPreviewEmissiveMap);
				_saveXml = true;
			}
		}

		private void butClearSpecularMap_Click(object sender, EventArgs e)
		{
			if (DarkMessageBox.Show(this, "Do you really want to clear normal map?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				tbSpecularMapPath.Text = "";
				tbSpecularMapPath.BackColor = this.BackColor;
				LoadTexturePreview("", picPreviewSpecularMap);
				_saveXml = true;
			}
		}

		private void nmNormalMapStrength_ValueChanged(object sender, EventArgs e)
		{
			_saveXml = true;
		}

		private void nmSpecularIntensity_ValueChanged(object sender, EventArgs e)
		{
			_saveXml = true;
		}
	}
}
