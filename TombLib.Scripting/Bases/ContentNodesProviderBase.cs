using DarkUI.Controls;
using System.Collections.Generic;
using System.ComponentModel;

namespace TombLib.Scripting.Bases
{
	public abstract class ContentNodesProviderBase : BackgroundWorker
	{
		public volatile string Filter = string.Empty;

		public void RunWorkerAsync(string content)
			=> base.RunWorkerAsync(content);

		protected override void OnDoWork(DoWorkEventArgs e)
		{
			e.Result = GetNodes(e.Argument.ToString());

			base.OnDoWork(e);
		}

		protected abstract IEnumerable<DarkTreeNode> GetNodes(string content);
	}
}
