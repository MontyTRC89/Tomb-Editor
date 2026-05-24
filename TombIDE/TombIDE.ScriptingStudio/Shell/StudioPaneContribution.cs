using DarkUI.Docking;
using System;
using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Shell
{
	public sealed class StudioPaneContribution
	{
		public StudioPaneContribution(UICommand command, string serializationKey, Func<DarkDockContent> createContent)
		{
			Command = command;
			SerializationKey = serializationKey ?? throw new ArgumentNullException(nameof(serializationKey));
			CreateContent = createContent ?? throw new ArgumentNullException(nameof(createContent));
		}

		public UICommand Command { get; }

		public string SerializationKey { get; }

		public Func<DarkDockContent> CreateContent { get; }
	}

	public interface IStudioPaneContributionProvider
	{
		IReadOnlyList<StudioPaneContribution> GetPaneContributions();
	}

	internal sealed class StaticStudioPaneContributionProvider : IStudioPaneContributionProvider
	{
		private readonly IReadOnlyList<StudioPaneContribution> _contributions;

		public StaticStudioPaneContributionProvider(IReadOnlyList<StudioPaneContribution> contributions)
			=> _contributions = contributions ?? Array.Empty<StudioPaneContribution>();

		public IReadOnlyList<StudioPaneContribution> GetPaneContributions()
			=> _contributions;
	}
}