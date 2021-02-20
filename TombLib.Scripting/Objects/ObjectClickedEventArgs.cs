using System;

namespace TombLib.Scripting.Objects
{
	public class ObjectClickedEventArgs : EventArgs
	{
		public string ObjectName { get; }
		public object IdentifyingObject { get; }

		public ObjectClickedEventArgs(string objectName, object identifyingObject = null)
		{
			ObjectName = objectName;
			IdentifyingObject = identifyingObject;
		}
	}
}
