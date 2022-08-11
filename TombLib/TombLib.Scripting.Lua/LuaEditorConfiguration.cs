﻿using System.IO;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;
using TombLib.Utils;

namespace TombLib.Scripting.Lua
{
	public sealed class LuaEditorConfiguration : TextEditorConfigBase
	{
		public override string DefaultPath { get; }

		#region Color scheme

		private string _selectedColorSchemeName = ConfigurationDefaults.SelectedColorSchemeName;
		public string SelectedColorSchemeName
		{
			get => _selectedColorSchemeName;
			set
			{
				_selectedColorSchemeName = value;

				string schemeFilePath =
					Path.Combine(DefaultPaths.LuaColorConfigsDirectory, value + ConfigurationDefaults.ColorSchemeFileExtension);

				if (!File.Exists(schemeFilePath))
					ColorScheme = new ColorScheme();
				else
					ColorScheme = XmlUtils.ReadXmlFile<ColorScheme>(schemeFilePath);
			}
		}

		public ColorScheme ColorScheme;

		#endregion Color scheme

		#region Construction

		public LuaEditorConfiguration()
			=> DefaultPath = Path.Combine(DefaultPaths.TextEditorConfigsDirectory, ConfigurationDefaults.ConfigurationFileName);

		#endregion Construction
	}
}
