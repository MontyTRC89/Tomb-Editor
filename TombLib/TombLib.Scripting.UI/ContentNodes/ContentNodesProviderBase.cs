using System.Collections.Generic;
using DarkUI.Controls;

namespace TombLib.Scripting.UI.ContentNodes
{
	public abstract class ContentNodesProviderBase
	{
		public IReadOnlyList<DarkTreeNode> GetNodes(string content, string filter)
			=> GetNodesCore(content ?? string.Empty, filter ?? string.Empty);

		protected abstract IReadOnlyList<DarkTreeNode> GetNodesCore(string content, string filter);
	}
}