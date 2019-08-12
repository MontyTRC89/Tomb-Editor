namespace TombLib.Projects
{
	public class Plugin
	{
		public string Name { get; set; }
		public string InternalPath { get; set; }

		public Plugin Clone()
		{
			return new Plugin
			{
				Name = Name,
				InternalPath = InternalPath
			};
		}
	}
}
