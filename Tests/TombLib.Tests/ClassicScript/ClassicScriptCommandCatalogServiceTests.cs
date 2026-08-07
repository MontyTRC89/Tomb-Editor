using TombLib.Scripting.ClassicScript.Commands;

namespace TombLib.Tests;

[TestClass]
public class ClassicScriptCommandCatalogServiceTests
{
	private readonly ClassicScriptCommandCatalogService _service = new();

	[TestMethod]
	public void IsOldCommand_ClassifiesOldCommands()
	{
		Assert.IsTrue(_service.IsOldCommand("Legend"));
		Assert.IsTrue(_service.IsOldCommand("AnimatingMIP"));
		Assert.IsTrue(_service.IsOldCommand("unknown")); // "Unknown" is a real TRNG command.
		Assert.IsTrue(_service.IsOldCommand("Cut"));     // Legacy array-only names are kept.
		Assert.IsTrue(_service.IsOldCommand("File"));
		Assert.IsTrue(_service.IsOldCommand("Layer2"));
	}

	[TestMethod]
	public void IsOldCommand_ClassifiesNewCommandsAsNotOld()
	{
		Assert.IsFalse(_service.IsOldCommand("AddEffect"));
		Assert.IsFalse(_service.IsOldCommand("Animation"));
		Assert.IsFalse(_service.IsOldCommand("FMV")); // Listed in both lists; new wins.
	}

	[TestMethod]
	public void IsNewCommand_ClassifiesKnownNewAndFallbackCommands()
	{
		Assert.IsTrue(_service.IsNewCommand("AddEffect"));
		Assert.IsTrue(_service.IsNewCommand("FMV"));
		Assert.IsTrue(_service.IsNewCommand("#DEFINE"));
		Assert.IsTrue(_service.IsNewCommand("#FIRST_ID"));
		Assert.IsTrue(_service.IsNewCommand("#INCLUDE"));
		Assert.IsTrue(_service.IsNewCommand("definitely_not_a_real_command")); // Absent from both lists -> new.
		Assert.IsFalse(_service.IsNewCommand("Legend"));
		Assert.IsFalse(_service.IsNewCommand("Unknown"));
	}

	[TestMethod]
	public void CommandLists_ContainTheFourEntriesAbsentFromLegacyNewCommandArray()
	{
		Assert.IsTrue(_service.NewCommands.Contains("#DEFINE", StringComparer.OrdinalIgnoreCase));
		Assert.IsTrue(_service.NewCommands.Contains("#FIRST_ID", StringComparer.OrdinalIgnoreCase));
		Assert.IsTrue(_service.NewCommands.Contains("#INCLUDE", StringComparer.OrdinalIgnoreCase));
		Assert.IsTrue(_service.NewCommands.Contains("FMV", StringComparer.OrdinalIgnoreCase));

		Assert.AreEqual(58, _service.NewCommands.Count);
		Assert.AreEqual(41, _service.OldCommands.Count);
	}

	[TestMethod]
	public void Sections_ContainAllTenSections()
	{
		string[] expected =
		[
			"Language", "Level", "Options", "PCExtensions", "PSXExtensions", "Title",
			"Strings", "PSXStrings", "PCStrings", "ExtraNG"
		];

		CollectionAssert.AreEqual(expected, _service.Sections.ToArray());
	}

	[TestMethod]
	public void Lookups_AreCaseInsensitive()
	{
		Assert.IsTrue(_service.IsOldCommand("legend"));
		Assert.IsTrue(_service.IsOldCommand("LEGEND"));
		Assert.IsFalse(_service.IsOldCommand("addeffect"));
		Assert.IsTrue(_service.NewCommands.Contains("addeffect", StringComparer.OrdinalIgnoreCase));
	}
}
