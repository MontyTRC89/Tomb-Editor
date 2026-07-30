using System;

namespace TombIDE.ScriptingStudio.DocumentOutline;

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
