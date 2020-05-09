using DarkUI.Forms;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TombLib.Scripting.TextEditors.Forms
{
	internal enum ColorSchemeType
	{
		ClassicScript,
		Lua
	}

	internal partial class FormSaveSchemeAs : DarkForm
	{
		public string SchemeFilePath { get; set; }

		private ColorSchemeType _schemeType;

		#region Construction

		public FormSaveSchemeAs(ColorSchemeType type)
		{
			InitializeComponent();

			_schemeType = type;
		}

		#endregion Construction

		#region Events

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			textBox_Name.Text = "New Scheme";
			textBox_Name.SelectAll();
		}

		private void button_Save_Click(object sender, EventArgs e)
		{
			try
			{
				string newName = RemoveIllegalPathSymbols(textBox_Name.Text).Trim();

				if (string.IsNullOrWhiteSpace(newName))
					throw new ArgumentException("Invalid name.");

				string schemeFilePath = null;

				switch (_schemeType)
				{
					case ColorSchemeType.ClassicScript:
					{
						string schemeFolderPath = DefaultPaths.GetClassicScriptColorConfigsPath();

						foreach (string file in Directory.GetFiles(schemeFolderPath, "*.cssch", SearchOption.TopDirectoryOnly))
							if (Path.GetFileNameWithoutExtension(file).Equals(newName, StringComparison.OrdinalIgnoreCase))
								throw new ArgumentException("A scheme with the same name already exists.");

						schemeFilePath = Path.Combine(schemeFolderPath, newName + ".cssch");
						break;
					}
					case ColorSchemeType.Lua:
					{
						string schemeFolderPath = DefaultPaths.GetLuaColorConfigsPath();

						foreach (string file in Directory.GetFiles(schemeFolderPath, "*.luasch", SearchOption.TopDirectoryOnly))
							if (Path.GetFileNameWithoutExtension(file).Equals(newName, StringComparison.OrdinalIgnoreCase))
								throw new ArgumentException("A scheme with the same name already exists.");

						schemeFilePath = Path.Combine(schemeFolderPath, newName + ".luasch");
						break;
					}
				}

				// // // //
				SchemeFilePath = schemeFilePath;
				// // // //
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				DialogResult = DialogResult.None;
			}
		}

		#endregion Events

		public static string RemoveIllegalPathSymbols(string fileName)
		{
			return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
		}
	}
}
