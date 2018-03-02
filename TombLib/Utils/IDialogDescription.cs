using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;
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
        public Level Level { get; set; }
    }

    public class DialogDescriptonMissingSounds : IDialogDescription
    {
        public string WadBasePath { get; set; }
        public string WadBaseFileName { get; set; }
        public List<SamplePathInfo> Samples { get; set; }
        public List<string> SoundPaths { get; set; }
        public Func<bool> FindTr4Samples { get; set; }
    }
}
