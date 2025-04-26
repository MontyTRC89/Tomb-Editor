using CommunityToolkit.Mvvm.ComponentModel;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Models;

public partial class LevelModel : ObservableObject
{
	public ILevelProject Base { get; }
	public bool IsExternal { get; }

	[ObservableProperty] private string _name = string.Empty;
	[ObservableProperty] private string _partialPrj2FilePath = string.Empty;

	public LevelModel(ILevelProject @base, string levelsDirectoryPath)
	{
		Base = @base;
		IsExternal = @base.IsExternal(levelsDirectoryPath);

		Name = @base.Name;
		PartialPrj2FilePath = @base.TargetPrj2FileName ?? @base.GetMostRecentlyModifiedPrj2FileName();
	}

	public void Update()
	{
		Name = Base.Name;
		PartialPrj2FilePath = Base.TargetPrj2FileName ?? Base.GetMostRecentlyModifiedPrj2FileName();
	}
}
