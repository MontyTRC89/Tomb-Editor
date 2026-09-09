using System.Reflection;
using System.Runtime.Versioning;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Tests;

[TestClass]
public class DependencyLayeringTests
{
	private static readonly string[] HostAssemblyPrefixes = ["TombLib", "DarkUI", "TombIDE", "AvalonEdit", "ICSharpCode"];

	[TestMethod]
	public void NeutralCore_HasNoHostOrUiDependency()
	{
		Assembly neutralCore = typeof(TombLib.Scripting.Text.ITextSnapshot).Assembly;

		string[] forbidden = neutralCore.GetReferencedAssemblies()
			.Select(reference => reference.Name)
			.Where(name => HostAssemblyPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
			.ToArray();

		Assert.AreEqual(0, forbidden.Length, "Neutral core references host assemblies: " + string.Join(", ", forbidden));
	}

	[TestMethod]
	public void NeutralCore_IsNotWindowsTargeted()
	{
		Assembly neutralCore = typeof(TombLib.Scripting.Text.ITextSnapshot).Assembly;
		TargetFrameworkAttribute? framework = neutralCore.GetCustomAttribute<TargetFrameworkAttribute>();

		Assert.IsNotNull(framework, "Neutral core must carry a target framework attribute.");
		Assert.IsFalse(framework.FrameworkName.Contains("-windows", StringComparison.Ordinal), "Neutral core must not be Windows-targeted: " + framework.FrameworkName);
	}

	[TestMethod]
	public void NeutralCore_DeclaresProviderContracts()
	{
		Assembly neutralCore = typeof(TombLib.Scripting.Text.ITextSnapshot).Assembly;

		Assert.IsNotNull(neutralCore.GetType("TombLib.Scripting.Completion.ITextCompletionProvider"), "ITextCompletionProvider must live in the neutral core.");
		Assert.IsNotNull(neutralCore.GetType("TombLib.Scripting.Hover.ITextHoverProvider"), "ITextHoverProvider must live in the neutral core.");
		Assert.IsNotNull(neutralCore.GetType("TombLib.Scripting.Navigation.ITextDefinitionProvider"), "ITextDefinitionProvider must live in the neutral core.");
		Assert.IsNotNull(neutralCore.GetType("TombLib.Scripting.Signatures.ITextSignatureHelpProvider"), "ITextSignatureHelpProvider must live in the neutral core.");
		Assert.IsNotNull(neutralCore.GetType("TombLib.Scripting.Diagnostics.ITextDiagnosticsProvider"), "ITextDiagnosticsProvider must live in the neutral core.");
	}

	[TestMethod]
	public void UiAdapter_ReferencesNeutralCoreAndAvalonEdit()
	{
		Assembly uiAdapter = typeof(TextEditorBase).Assembly;

		Assert.IsNotNull(uiAdapter.GetReferencedAssemblies().FirstOrDefault(reference => reference.Name == "TombLib.Scripting"), "UI adapter must reference the neutral core.");
		Assert.IsNotNull(uiAdapter.GetReferencedAssemblies().FirstOrDefault(reference => reference.Name == "ICSharpCode.AvalonEdit"), "UI adapter must reference AvalonEdit.");
	}

	[TestMethod]
	public void LanguageProviders_ReferenceNeutralCore()
	{
		AssertProviderReferencesNeutralCore(typeof(ClassicScriptLanguageServices).Assembly);
		AssertProviderReferencesNeutralCore(typeof(GameFlowLanguageServices).Assembly);
		AssertProviderReferencesNeutralCore(typeof(TRXLanguageServices).Assembly);
	}

	private static void AssertProviderReferencesNeutralCore(Assembly providerAssembly)
	{
		Assert.IsNotNull(
			providerAssembly.GetReferencedAssemblies().FirstOrDefault(reference => reference.Name == "TombLib.Scripting"),
			$"{providerAssembly.GetName().Name} must reference the neutral TombLib.Scripting core.");
	}
}
