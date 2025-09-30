using DarkUI.Forms;
using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombLib.Forms
{
	public partial class FormMaterialEditor : DarkForm
	{
		public string MaterialFileName { get; set; }
		public bool MaterialChanged => _saveXml;

		private MaterialData _materialData;
		private string _texturePath;

		private readonly Color _correctColor;
		private readonly Color _wrongColor;

		private bool _saveXml = false;
		private bool _loaded = false;

		public FormMaterialEditor(string texturePath, ConfigurationBase configuration)
		{
			InitializeComponent();

			_correctColor = tbNormalMapPath.BackColor;
			_wrongColor = _correctColor.MixWith(Color.DarkRed, 0.55);

			_texturePath = texturePath;
			_materialData = MaterialData.TrySidecarLoadOrLoadExisting(texturePath);

			// Populate material type combobox.
			foreach (MaterialType matType in Enum.GetValues(typeof(MaterialType)))
				comboMaterialType.Items.Add(matType.ToString().SplitCamelcase());

			// Set window property handlers.
			ConfigurationBase.ConfigureWindow(this, configuration);
		}

		private void FormMaterialEditor_Load(object sender, EventArgs e)
		{
			tbColorMapPath.Text = _materialData.ColorMap;
			tbNormalMapPath.Text = _materialData.NormalMap;
			tbSpecularMapPath.Text = _materialData.SpecularMap;
			tbAmbientOcclusionMapPath.Text = _materialData.AmbientOcclusionMap;
			tbEmissiveMapPath.Text = _materialData.EmissiveMap;
			tbRoughnessMapPath.Text = _materialData.RoughnessMap;

            if (!string.IsNullOrEmpty(_materialData.NormalMap))
				tbNormalMapPath.BackColor = (_materialData.IsNormalMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.AmbientOcclusionMap))
				tbAmbientOcclusionMapPath.BackColor = (_materialData.IsAmbientOcclusionMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.SpecularMap))
				tbSpecularMapPath.BackColor = (_materialData.IsSpecularMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.EmissiveMap))
				tbEmissiveMapPath.BackColor = (_materialData.IsEmissiveMapFound ? _correctColor : _wrongColor);

			if (!string.IsNullOrEmpty(_materialData.RoughnessMap))
				tbRoughnessMapPath.BackColor = (_materialData.IsRoughnessMapFound ? _correctColor : _wrongColor);

            LoadTexturePreview(_materialData.ColorMap, picPreviewColorMap);
			LoadTexturePreview(_materialData.NormalMap, picPreviewNormalMap);
			LoadTexturePreview(_materialData.SpecularMap, picPreviewSpecularMap);
			LoadTexturePreview(_materialData.AmbientOcclusionMap, picPreviewAmbientOcclusionMap);
			LoadTexturePreview(_materialData.EmissiveMap, picPreviewEmissiveMap);
			LoadTexturePreview(_materialData.RoughnessMap, picPreviewRoughnessMap);

            lblXmlMaterialFile.Text = string.IsNullOrEmpty(_materialData.XmlMaterialFileName) ? string.Empty :
				"Material settings file: " + _materialData.XmlMaterialFileName;

			comboMaterialType.SelectedIndex = (int)_materialData.Type;
			LoadMaterialProperties();
			UpdateUI();

			_loaded = true;
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

		private void LoadMaterialProperties()
		{
			var materialType = (MaterialType)comboMaterialType.SelectedIndex;

			switch (materialType)
			{
				case MaterialType.Default:
					tabcontainerParameters.SelectedIndex = 0;
					nmNormalMapStrength.Value = (decimal)_materialData.Parameters0.X;
					nmSpecularIntensity.Value = (decimal)(_materialData.Parameters0.Y);
					break;
			}
		}

		private void SaveMaterialProperties()
		{
			if (!_saveXml)
				return;

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
			materialData.RoughnessMap = tbRoughnessMapPath.Text;

            switch (materialData.Type)
			{
				case MaterialType.Default:
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
				DarkMessageBox.Show(this, $"An error occurred while saving XML material file to '{externalMaterialDataPath}'",
					"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
		}

		private void UpdateUI()
		{
			butClearNormalMap.Enabled = !string.IsNullOrEmpty(tbNormalMapPath.Text);
			butClearAmbientOcclusionMap.Enabled = !string.IsNullOrEmpty(tbAmbientOcclusionMapPath.Text);
			butClearEmissiveMap.Enabled = !string.IsNullOrEmpty(tbEmissiveMapPath.Text);
			butClearSpecularMap.Enabled = !string.IsNullOrEmpty(tbSpecularMapPath.Text);
			butClearRoughnessMap.Enabled = !string.IsNullOrEmpty(tbRoughnessMapPath.Text);
        }

		private void BrowseTexture(TextBox textBox, PictureBox previewBox)
		{
			var texturePath = LevelFileDialog.BrowseFile(this, "Browse texture", ImageC.FileExtensions, false);

			if (!string.IsNullOrEmpty(texturePath))
			{
				textBox.Text = texturePath;
				textBox.BackColor = _correctColor;
				LoadTexturePreview(texturePath, previewBox);
				UpdateUI();
				_saveXml = true;
			}
		}

		private void ClearTexture(TextBox textBox, PictureBox previewBox, string mapName)
		{
			var message = "Do you really want to clear the " + mapName + " map ?";

			if (DarkMessageBox.Show(this, message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				textBox.Text = string.Empty;
				textBox.BackColor = this.BackColor;
				LoadTexturePreview(string.Empty, previewBox);
				UpdateUI();
				_saveXml = true;
			}
		}

		private void butCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void butOK_Click(object sender, EventArgs e)
		{
			SaveMaterialProperties();
			DialogResult = DialogResult.OK;
			Close();
		}

		private void comboMaterialType_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadMaterialProperties();
			if (_loaded)
				_saveXml = true;
		}

		private void butBrowseNormalMap_Click(object sender, EventArgs e) => BrowseTexture(tbNormalMapPath, picPreviewNormalMap);
		private void butBrowseAmbientOcclusionMap_Click(object sender, EventArgs e) => BrowseTexture(tbAmbientOcclusionMapPath, picPreviewAmbientOcclusionMap);
		private void butBrowseEmissiveMap_Click(object sender, EventArgs e) => BrowseTexture(tbEmissiveMapPath, picPreviewEmissiveMap);
		private void butBrowseSpecularMap_Click(object sender, EventArgs e) => BrowseTexture(tbSpecularMapPath, picPreviewSpecularMap);
        private void butBrowseRoughnessMap_Click(object sender, EventArgs e) => BrowseTexture(tbRoughnessMapPath, picPreviewRoughnessMap);

        private void butClearNormalMap_Click(object sender, EventArgs e) => ClearTexture(tbNormalMapPath, picPreviewNormalMap, "normal");
		private void butClearAmbientOcclusionMap_Click(object sender, EventArgs e) => ClearTexture(tbAmbientOcclusionMapPath, picPreviewAmbientOcclusionMap, "ambient occlusion");
		private void butClearEmissiveMap_Click(object sender, EventArgs e) => ClearTexture(tbEmissiveMapPath, picPreviewEmissiveMap, "emissive");
		private void butClearSpecularMap_Click(object sender, EventArgs e) => ClearTexture(tbSpecularMapPath, picPreviewSpecularMap, "specular");
        private void butClearRoughnessMap_Click(object sender, EventArgs e) => ClearTexture(tbRoughnessMapPath, picPreviewRoughnessMap, "roughness");


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
