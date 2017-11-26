using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Configuration
{
    public interface IBaseConfiguration
    {
        // Gizmo settings
        float Gizmo_Size { get; set; }
        float Gizmo_TranslationConeSize { get; set; }
        float Gizmo_CenterCubeSize { get; set; }
        float Gizmo_ScaleCubeSize { get; set; }
        float Gizmo_LineThickness { get; set; }

        // Graphics settings
        int TextureAtlasSize { get; set; }
    }
}
