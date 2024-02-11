using NLog;
using System.Collections.Generic;
using System.Diagnostics;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Extensions;

public static class RoomExtensions
{
	private static readonly Logger logger = LogManager.GetCurrentClassLogger();
	private static readonly Editor editor = Editor.Instance;
}
