using System;
using System.Collections.Generic;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.Wad.Tr4Wad;

namespace TombLib.Utils
{
    public interface IDialogHandler
    {
        void RaiseDialog(IDialogDescription description);
    }

    public interface IDialogDescription
    {}

    public class DialogDescriptonTextureUnloadable : IDialogDescription
    {
        public LevelSettings Settings { get; set; }
        public LevelTexture Texture { get; set; }
    }

    public class DialogDescriptonWadUnloadable : IDialogDescription
    {
        public LevelSettings Settings { get; set; }
        public ReferencedWad Wad { get; set; }
    }

    public class DialogDescriptonSoundsCatalogUnloadable : IDialogDescription
    {
        public LevelSettings Settings { get; set; }
        public ReferencedSoundsCatalog Sounds { get; set; }
    }
}
