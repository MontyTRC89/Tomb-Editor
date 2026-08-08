using System;
using TombLib.Scripting.Navigation;

namespace TombIDE.ScriptingStudio.DocumentOutline;

public class ObjectClickedEventArgs : EventArgs
{
	public string ObjectName { get; }
	public TextDefinitionDiscriminator? IdentifyingObject { get; }

	public ObjectClickedEventArgs(string objectName, TextDefinitionDiscriminator? identifyingObject = null)
	{
		ObjectName = objectName;
		IdentifyingObject = identifyingObject;
	}
}
