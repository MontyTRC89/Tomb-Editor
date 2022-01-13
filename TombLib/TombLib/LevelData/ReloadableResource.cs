using System;
using System.Collections.Generic;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public interface IReloadableResource
    {
        string ResourceName { get; }
        Exception LoadException { get; set; }
        IEnumerable<FileFormat> FileExtensions { get; }
        List<IReloadableResource> GetResourceList(LevelSettings settings);

        string GetPath();
        void SetPath(LevelSettings settings, string path);
    }
}
