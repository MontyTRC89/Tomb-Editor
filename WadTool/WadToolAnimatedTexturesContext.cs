using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
	public class WadToolAnimatedTexturesContext : FormAnimatedTextures.IAnimatedTexturesContext
	{
		private readonly WadToolClass _tool;

		private TextureArea _selectedTexture;

		public WadToolAnimatedTexturesContext(WadToolClass tool)
		{
			_tool = tool;
			_tool.EditorEventRaised += EditorEventRaised;
		}

		private void EditorEventRaised(IEditorEvent obj)
		{
			// @FIXME: Proper event handling for these events was removed, because there were
			// serious issues with DarkDataGridView on level reloading or unloading textures.
			if (obj is WadToolClass.DestinationWadChangedEvent)
				OnContextInvalidated?.Invoke();

			// Update animated texture set combo box
			if (obj is WadToolClass.AnimatedTexturesChangedEvent)
			{
				OnAnimatedTexturesChanged?.Invoke();
			}
		}

		public TextureArea SelectedTexture
		{
			get => _selectedTexture;
			set => _selectedTexture = value;
		}

		public List<Texture> AvailableTextures => _tool.DestinationWad.MeshTexturesUnique
			.Where(x => x is WadTexture)
			.Cast<Texture>()
			.ToList();

		public Action OnAnimatedTexturesChanged { get; set; }
	
		public Action OnContextInvalidated { get; set; }
	
		public List<AnimatedTextureSet> AnimatedTextureSets => _tool.DestinationWad.AnimatedTextureSets;
	
		public TRVersion.Game Version { get => _tool.DestinationWad.GameVersion; }
	}
}
