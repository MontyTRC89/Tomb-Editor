using System.Reflection;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.TRX;
using TombLib.Scripting.Text;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Tests;

[TestClass]
public class ScriptingDependencyArchitectureTests
{
	private static readonly string[] ForbiddenNeutralCoreReferences =
	[
		"AvalonEdit",
		"DarkUI",
		"DarkUI.WPF",
		"PresentationFramework",
		"WindowsBase",
		"System.Windows.Forms",
		"TombLib",
		"TombLib.Forms",
		"TombLib.WPF"
	];

	[TestMethod]
	public void NeutralCore_DoesNotReferenceUiOrHostAssemblies()
	{
		string[] offending = GetReferencedAssemblyNames(typeof(TextRange).Assembly)
			.Where(static name => ForbiddenNeutralCoreReferences.Contains(name))
			.ToArray();

		Assert.AreEqual(0, offending.Length, "TombLib.Scripting must not reference UI or host assemblies: " + string.Join(", ", offending));
	}

	[TestMethod]
	public void NeutralCore_DoesNotReferenceSiblingScriptingProjects()
	{
		string[] siblingReferences = GetReferencedAssemblyNames(typeof(TextRange).Assembly)
			.Where(static name => name.StartsWith("TombLib.Scripting", StringComparison.Ordinal)
				&& !string.Equals(name, "TombLib.Scripting.LanguageServer", StringComparison.Ordinal))
			.ToArray();

		Assert.AreEqual(0, siblingReferences.Length, "TombLib.Scripting must not reference sibling scripting projects: " + string.Join(", ", siblingReferences));
	}

	[TestMethod]
	public void EditorIntegrationProjects_ReferenceTheNeutralCore()
	{
		Assert.IsTrue(ReferencesAssembly(typeof(TextEditorBase).Assembly, "TombLib.Scripting"), "TombLib.Scripting.UI must reference TombLib.Scripting.");
		Assert.IsTrue(ReferencesAssembly(typeof(ClassicScriptEditor).Assembly, "TombLib.Scripting"), "TombLib.Scripting.ClassicScript must reference TombLib.Scripting.");
		Assert.IsTrue(ReferencesAssembly(typeof(GameFlowEditor).Assembly, "TombLib.Scripting"), "TombLib.Scripting.GameFlowScript must reference TombLib.Scripting.");
		Assert.IsTrue(ReferencesAssembly(typeof(TRXEditor).Assembly, "TombLib.Scripting"), "TombLib.Scripting.TRX must reference TombLib.Scripting.");
		Assert.IsTrue(ReferencesAssembly(typeof(LuaEditor).Assembly, "TombLib.Scripting"), "TombLib.Scripting.Lua must reference TombLib.Scripting.");
	}

	private static string[] GetReferencedAssemblyNames(Assembly assembly)
	{
		return assembly.GetReferencedAssemblies()
			.Select(static reference => reference.Name)
			.OfType<string>()
			.ToArray();
	}

	private static bool ReferencesAssembly(Assembly assembly, string assemblyName)
		=> GetReferencedAssemblyNames(assembly).Contains(assemblyName, StringComparer.Ordinal);
}
