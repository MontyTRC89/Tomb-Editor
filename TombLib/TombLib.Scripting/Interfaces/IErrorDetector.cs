using System;

namespace TombLib.Scripting.Interfaces
{
	public interface IErrorDetector
	{
		object FindErrors(string editorContent, Version engineVersion);
	}
}
