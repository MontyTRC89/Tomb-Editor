using System;
using System.Collections.Generic;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum ReloadableResourceType
    {
        Texture,
        Wad,
        SoundCatalog,
        ImportedGeometry
    }

    public interface IReloadableResource
    {
        ReloadableResourceType ResourceType { get; }
        Exception LoadException { get; set; }
        IEnumerable<FileFormat> FileExtensions { get; }
        List<IReloadableResource> GetResourceList(LevelSettings settings);

        string GetPath();
        void SetPath(LevelSettings settings, string path);
    }
}
