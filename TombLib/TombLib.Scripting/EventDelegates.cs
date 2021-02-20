using TombLib.Scripting.Objects;

namespace TombLib.Scripting
{
	public delegate void FindReplaceEventHandler(object sender, FindReplaceEventArgs e);

	public delegate void CellValueChangedEventHandler(object sender, CellContentChangedEventArgs e);

	public delegate void ObjectClickedEventHandler(object sender, ObjectClickedEventArgs e);
}
