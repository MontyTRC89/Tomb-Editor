using NLog;
using System.ComponentModel;
using System.Diagnostics;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

/// <summary>
/// A command which allows to commit <see cref="Room.SmartBuildGeometry" /> while performing benchmarks and sending a global message, notifying about the change in geometry.
/// </summary>
public abstract class SmartBuildGeometryCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : UnconditionalEditorCommand(caller, editor, logger)
{
	protected void CommitSmartBuildGeometry(Room room, RectangleInt2 area)
	{
		var watch = Stopwatch.StartNew();
		room.SmartBuildGeometry(area, Editor.Configuration.Rendering3D_HighQualityLightPreview);
		watch.Stop();

		Logger.Debug($"Edit geometry time: {watch.ElapsedMilliseconds} ms");
		Editor.RoomGeometryChange(room);
	}
}
