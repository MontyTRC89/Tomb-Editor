using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Forms;
using TombEditor.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using NLog;

namespace TombEditor
{
    public enum CommandType
    {
        General,
        File,
        Edit,
        Rooms,
        Geometry,
        Sectors,
        Objects,
        Textures,
        Lighting,
        View,
        Settings
    }

    public class CommandObj
    {
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public Action<CommandArgs> Execute { get; set; }
        public CommandType Type { get; set; }
    }

    public class CommandArgs
    {
        public Editor Editor;
        public IWin32Window Window;
        public Keys KeyData = Keys.None;
    }

    public static class CommandHandler
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static List<CommandObj> _commands = new List<CommandObj>();
        public static IEnumerable<CommandObj> Commands => _commands;

        public static CommandObj GetCommand(string name)
        {
            CommandObj command = _commands.FirstOrDefault(cmd => cmd.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (command == null)
                throw new KeyNotFoundException("Command with name '" + name + "' not found.");
            return command;
        }

        public static void ExecuteHotkey(CommandArgs args)
        {
            var hotkeyForCommands = args.Editor.Configuration.UI_Hotkeys.Where(set => set.Value.Contains(args.KeyData));
            foreach (var hotkeyForCommand in hotkeyForCommands)
                GetCommand(hotkeyForCommand.Key).Execute?.Invoke(args);
        }

        public static void AssignCommandsToControls(Editor editor, Control parent, ToolTip toolTip = null, bool onlyToolTips = false)
        {
            var controls = WinFormsUtils.AllSubControls(parent).Where(c => c is DarkButton || c is DarkCheckBox).ToList();
            foreach (var control in controls)
            {
                if (!string.IsNullOrEmpty(control.Tag?.ToString()))
                {
                    var command = GetCommand(control.Tag.ToString());

                    if (command != null)
                    {
                        var hotkeyLabel = string.Join(", ", editor.Configuration.UI_Hotkeys[control.Tag.ToString()]);
                        var label = command.FriendlyName + (string.IsNullOrEmpty(hotkeyLabel) ? "" : " (" + hotkeyLabel + ")");

                        if(!onlyToolTips)
                            control.Click += (sender, e) => { command.Execute?.Invoke(new CommandArgs { Editor = editor, Window = parent.FindForm() }); };

                        if (toolTip != null && !string.IsNullOrEmpty(label))
                            toolTip.SetToolTip(control, label);
                    }
                }
            }
        }

        private static void AddCommand(string commandName, string friendlyName, CommandType type, Action<CommandArgs> command)
        {
            if (_commands.Any(cmd => cmd.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase)))
                throw new InvalidOperationException("You cannot add multiple commands with the same name.");
            _commands.Add(new CommandObj() { Name = commandName, FriendlyName = friendlyName, Execute = command, Type = type });
        }

        static CommandHandler()
        {
            AddCommand("CancelAnyAction", "Cancel any action", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Action = null;
                args.Editor.SelectedSectors = SectorSelection.None;
                args.Editor.SelectedObject = null;
                args.Editor.SelectedRooms = new[] { args.Editor.SelectedRoom };
            });

            AddCommand("Switch2DMode", "Switch to 2D map", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Mode = EditorMode.Map2D;
            });

            AddCommand("SwitchGeometryMode", "Switch to Geometry mode", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Mode = EditorMode.Geometry;
            });

            AddCommand("SwitchFaceEditMode", "Switch to Face Edit mode", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Mode = EditorMode.FaceEdit;
            });

            AddCommand("SwitchLightingMode", "Switch to Lighting mode", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Mode = EditorMode.Lighting;
            });

            AddCommand("ResetCamera", "Reset camera position to default", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.ResetCamera();
            });

            AddCommand("AddTrigger", "Add trigger", CommandType.Objects, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    EditorActions.AddTrigger(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window, args.Editor.SelectedObject);
            });

            AddCommand("AddTriggerWithBookmark", "Add trigger with bookmarked object", CommandType.Objects, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    EditorActions.AddTrigger(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window, args.Editor.BookmarkedObject);
            });

            AddCommand("AddPortal", "Add portal", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    try
                    {
                        EditorActions.AddPortal(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window);
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Unable to create portal");
                        args.Editor.SendMessage("Unable to create portal. \nException: " + exc.Message, PopupType.Error);
                    }
            });

            AddCommand("EditObject", "Edit object properties", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.EditObject(args.Editor.SelectedObject, args.Window);
            });

            AddCommand("SetTextureBlendMode", "Set blending mode", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                if (texture.BlendMode == BlendMode.Normal)
                    texture.BlendMode = BlendMode.Additive;
                else if (texture.BlendMode == BlendMode.Additive)
                    texture.BlendMode = BlendMode.Subtract;
                else if (texture.BlendMode == BlendMode.Subtract)
                    texture.BlendMode = BlendMode.Exclude;
                else if (texture.BlendMode == BlendMode.Exclude)
                    texture.BlendMode = BlendMode.Screen;
                else if (texture.BlendMode == BlendMode.Screen)
                    texture.BlendMode = BlendMode.Lighten;
                else if (texture.BlendMode == BlendMode.Lighten)
                    texture.BlendMode = BlendMode.Normal;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetTextureDoubleSided", "Set double-sided attribute", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.DoubleSided = !texture.DoubleSided;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetTextureInvisible", "Set invisibility attribute", CommandType.Textures, delegate (CommandArgs args)
            {
                args.Editor.SelectedTexture = TextureArea.Invisible;
            });

            AddCommand("RotateObjectLeft", "Rotate object left", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.Y, -1);
            });

            AddCommand("RotateObjectRight", "Rotate object right", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.Y, 1);
            });

            AddCommand("RotateObjectUp", "Rotate object up", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.X, 1);
            });

            AddCommand("RotateObjectDown", "Rotate object down", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.X, -1);
            });

            AddCommand("MoveObjectLeft", "Move object left (4 clicks)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(-1024, 0, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectRight", "Move object right (4 clicks)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(1024, 0, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectForward", "Move object forward (4 clicks)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 0, 1024), new Vector3(), true);
            });

            AddCommand("MoveObjectBack", "Move object back (4 clicks)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 0, -1024), new Vector3(), true);
            });

            AddCommand("MoveObjectUp", "Move object up", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 1024, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectDown", "Move object down", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, -1024, 0), new Vector3(), true);
            });

            AddCommand("MoveRoomLeft", "Move room left", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(-1, 0, 0), args.Editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomRight", "Move room right", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(1, 0, 0), args.Editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomForward", "Move room forward", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(0, 0, 1), args.Editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomBack", "Move room back", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(0, 0, -1), args.Editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomUp", "Move room up", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(0, 1, 0), args.Editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomDown", "Move room down", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(0, -1, 0), args.Editor.SelectedRoom.Versions);
            });

            AddCommand("RaiseQA1Click", "Raise selected floor or item (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, 1, false);
                else if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 256, 0), new Vector3(), true);
            });

            AddCommand("RaiseQA4Click", "Raise selected floor or item (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, 4, false);
                else if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, 1024, 0), new Vector3(), true);
            });

            AddCommand("LowerQA1Click", "Lower selected floor or item (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, -1, false);
                else if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, -256, 0), new Vector3(), true);
            });

            AddCommand("LowerQA4Click", "Lower selected floor or item (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, -4, false);
                else if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, -1024, 0), new Vector3(), true);
            });

            AddCommand("RaiseWS1Click", "Raise selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 1, false);
            });

            AddCommand("RaiseWS4Click", "Raise selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 4, false);
            });

            AddCommand("LowerWS1Click", "Lower selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -1, false);
            });

            AddCommand("LowerWS4Click", "Lower selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -4, false);
            });

            AddCommand("RaiseED1Click", "Raise selected floor subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ed, 1, false);
            });

            AddCommand("RaiseED4Click", "Raise selected floor subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ed, 4, false);
            });

            AddCommand("LowerED1Click", "Lower selected floor subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ed, -1, false);
            });

            AddCommand("LowerED4Click", "Lower selected floor subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ed, -4, false);
            });

            AddCommand("RaiseRF1Click", "Raise selected ceiling subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Rf, 1, false);
            });

            AddCommand("RaiseRF4Click", "Raise selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Rf, 4, false);
            });

            AddCommand("LowerRF1Click", "Lower selected ceiling subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Rf, -1, false);
            });

            AddCommand("LowerRF4Click", "Lower selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Rf, -4, false);
            });

            AddCommand("RaiseQA1ClickSmooth", "Smoothly raise selected floor (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, 1, true);
            });

            AddCommand("RaiseQA4ClickSmooth", "Smoothly raise selected floor (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, 4, true);
            });

            AddCommand("LowerQA1ClickSmooth", "Smoothly lower selected floor (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, -1, true);
            });

            AddCommand("LowerQA4ClickSmooth", "Smoothly lower selected floor (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, -4, true);
            });

            AddCommand("RaiseWS1ClickSmooth", "Smoothly raise selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 1, true);
            });

            AddCommand("RaiseWS4ClickSmooth", "Smoothly raise selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 4, true);
            });

            AddCommand("LowerWS1ClickSmooth", "Smoothly lower selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -1, true);
            });

            AddCommand("LowerWS4ClickSmooth", "Smoothly lower selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -4, true);
            });

            AddCommand("RaiseED1ClickSmooth", "Smoothly raise selected floor subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ed, 1, true);
            });

            AddCommand("RaiseED4ClickSmooth", "Smoothly raise selected floor subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ed, 4, true);
            });

            AddCommand("LowerED1ClickSmooth", "Smoothly lower selected floor subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ed, -1, true);
            });

            AddCommand("LowerED4ClickSmooth", "Smoothly lower selected floor subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ed, -4, true);
            });

            AddCommand("RaiseRF1ClickSmooth", "Smoothly raise selected ceiling subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Rf, 1, true);
            });

            AddCommand("RaiseRF4ClickSmooth", "Smoothly raise selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Rf, 4, true);
            });

            AddCommand("LowerRF1ClickSmooth", "Smoothly lower selected ceiling subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Rf, -1, true);
            });

            AddCommand("LowerRF4ClickSmooth", "Smoothly lower selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Rf, -4, true);
            });

            AddCommand("RaiseYH1Click", "Raise selected floor diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, 1, false, true);
            });

            AddCommand("RaiseYH4Click", "Raise selected floor diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, 4, false, true);
            });

            AddCommand("LowerYH1Click", "Lower selected floor diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, -1, false, true);
            });

            AddCommand("LowerYH4Click", "Lower selected floor diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Floor, -4, false, true);
            });

            AddCommand("RaiseUJ1Click", "Raise selected ceiling diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 1, false, true);
            });

            AddCommand("RaiseUJ4Click", "Raise selected ceiling diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 4, false, true);
            });

            AddCommand("LowerUJ1Click", "Lower selected ceiling diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -1, false, true);
            });

            AddCommand("LowerUJ4Click", "Lower selected ceiling diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
                    EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -4, false, true);
            });

            AddCommand("RotateObject5", "Rotate object (5 degrees)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.Y, 5.0f);
            });

            AddCommand("RotateObject45", "Rotate object (45 degrees)", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject != null)
                    EditorActions.RotateObject(args.Editor.SelectedObject, RotationAxis.Y, 45.0f);
            });

            AddCommand("MoveObjectToCurrentRoom", "Move object to current room", CommandType.Objects, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectToOtherRoom((PositionBasedObjectInstance)args.Editor.SelectedObject, args.Editor.SelectedRoom);
            });

            AddCommand("RelocateCamera", "Relocate camera", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionRelocateCamera();
            });

            AddCommand("RotateTexture", "Rotate selected texture", CommandType.Textures, delegate (CommandArgs args)
            {
                TextureArea newTexture = args.Editor.SelectedTexture;
                newTexture.Rotate(1, newTexture.TextureIsTriangle);
                args.Editor.SelectedTexture = newTexture;
            });

            AddCommand("MirrorTexture", "Mirror selected texture", CommandType.Textures, delegate (CommandArgs args)
            {
                TextureArea newTexture = args.Editor.SelectedTexture;
                Swap.Do(ref newTexture.TexCoord0, ref newTexture.TexCoord3);
                Swap.Do(ref newTexture.TexCoord1, ref newTexture.TexCoord2);
                args.Editor.SelectedTexture = newTexture;
            });

            AddCommand("NewLevel", "New level", CommandType.File, delegate (CommandArgs args)
            {
                if (!EditorActions.ContinueOnFileDrop(args.Window, "New level"))
                    return;

                args.Editor.Level = Level.CreateSimpleLevel();
            });

            AddCommand("OpenLevel", "Open existing level...", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.OpenLevel(args.Window);
            });

            AddCommand("SaveLevel", "Save level", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.SaveLevel(args.Window, false);
            });

            AddCommand("SaveLevelAs", "Save level as...", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.SaveLevel(args.Window, true);
            });

            AddCommand("ImportPrj", "Import TRLE level...", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.OpenLevelPrj(args.Window);
            });

            AddCommand("BuildLevel", "Build level", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.BuildLevel(false, args.Window);
            });

            AddCommand("BuildAndPlay", "Build level and play", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.BuildLevelAndPlay(args.Window);
            });

            AddCommand("Cut", "Cut", CommandType.Edit, delegate (CommandArgs args)
            {
                GetCommand("Copy").Execute.Invoke(args);

                if (args.Editor.Mode == EditorMode.Map2D)
                {
                    if (args.Editor.SelectedObject == null)
                        EditorActions.DeleteRooms(args.Editor.SelectedRooms);
                }
                else
                {
                    if(args.Editor.SelectedObject == null && args.Editor.SelectedSectors.Valid)
                    {
                        EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, null, false, true, false);
                        EditorActions.SetSurfaceWithoutUpdate(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, false);
                        EditorActions.SetSurfaceWithoutUpdate(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, true);
                        EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, null, true, true, true, true);
                    }
                    else if (args.Editor.SelectedObject != null && !(args.Editor.SelectedObject is PortalInstance) && !(args.Editor.SelectedObject is TriggerInstance))
                        EditorActions.DeleteObject(args.Editor.SelectedObject);
                }
            });

            AddCommand("Copy", "Copy", CommandType.Edit, delegate (CommandArgs args)
            {
                if (args.Editor.Mode != EditorMode.Map2D)
                {
                    if (args.Editor.SelectedObject != null)
                        EditorActions.TryCopyObject(args.Editor.SelectedObject, args.Window);
                    else if (args.Editor.SelectedSectors.Valid)
                        EditorActions.TryCopySectors(args.Editor.SelectedSectors, args.Window);
                }
                else
                    Clipboard.SetDataObject(new RoomClipboardData(args.Editor), true);
            });

            AddCommand("Paste", "Paste", CommandType.Edit, delegate (CommandArgs args)
            {
                if (args.Editor.Mode != EditorMode.Map2D)
                {
                    if (Clipboard.ContainsData(typeof(ObjectClipboardData).FullName))
                    {
                        var data = Clipboard.GetDataObject().GetData(typeof(ObjectClipboardData)) as ObjectClipboardData;
                        if (data == null)
                            args.Editor.SendMessage("Clipboard contains no object data.", PopupType.Error);
                        else
                            args.Editor.Action = new EditorActionPlace(false, (level, room) => data.MergeGetSingleObject(args.Editor));
                    }
                    else if (args.Editor.SelectedSectors.Valid && Clipboard.ContainsData(typeof(SectorsClipboardData).FullName))
                    {
                        var data = Clipboard.GetDataObject().GetData(typeof(SectorsClipboardData)) as SectorsClipboardData;
                        if (data == null)
                            args.Editor.SendMessage("Clipboard contains no sector data.", PopupType.Error);
                        else
                            EditorActions.TryPasteSectors(data, args.Window);
                    }
                }
                else
                {
                    var roomClipboardData = Clipboard.GetDataObject().GetData(typeof(RoomClipboardData)) as RoomClipboardData;
                    if (roomClipboardData == null)
                        args.Editor.SendMessage("Clipboard contains no room data.", PopupType.Error);
                    else
                        roomClipboardData.MergeInto(args.Editor, new VectorInt2());
                }
            });

            AddCommand("StampObject", "Stamp object", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.TryStampObject(args.Editor.SelectedObject, args.Window);
            });

            AddCommand("Delete", "Delete", CommandType.Edit, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Map2D)
                    if (args.Editor.SelectedObject != null && (args.Editor.SelectedObject is PortalInstance || args.Editor.SelectedObject is TriggerInstance))
                        EditorActions.DeleteObject(args.Editor.SelectedObject, args.Window);
                    else
                        EditorActions.DeleteRooms(args.Editor.SelectedRooms, args.Window);
                else
                {
                    if (args.Editor.SelectedObject != null)
                        EditorActions.DeleteObject(args.Editor.SelectedObject, args.Window);
                    else
                        args.Editor.SendMessage("No object selected. Nothing to delete.", PopupType.Warning);
                }
            });

            AddCommand("SelectAll", "Select all", CommandType.Edit, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Map2D)
                    args.Editor.SelectRooms(args.Editor.Level.Rooms.Where(room => room != null));
                else
                    args.Editor.SelectedSectors = new SectorSelection { Area = args.Editor.SelectedRoom.LocalArea };
            });

            AddCommand("BookmarkObject", "Bookmark object", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.BookmarkObject(args.Editor.SelectedObject);
            });

            AddCommand("SelectBookmarkedObject", "Select bookmarked object", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.SelectedObject = args.Editor.BookmarkedObject;
            });

            AddCommand("Search", "Search...", CommandType.Edit, delegate (CommandArgs args)
            {
                var existingWindow = Application.OpenForms["FormSearch"];
                if (existingWindow == null)
                {
                    FormSearch searchForm = new FormSearch(args.Editor);
                    searchForm.Show(args.Window);
                }
                else
                    existingWindow.Focus();
            });

            AddCommand("DeleteRooms", "Delete rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.Mode == EditorMode.Map2D)
                    EditorActions.DeleteRooms(args.Editor.SelectedRooms, args.Window);
                else
                    EditorActions.DeleteRooms(new[] { args.Editor.SelectedRoom }, args.Window);
            });

            AddCommand("DuplicateRoom", "Duplicate rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.DuplicateRoom(args.Window);
            });

            AddCommand("Redo", "Redo", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.UndoManager.Redo();
            });

            AddCommand("Undo", "Undo", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.UndoManager.Undo();
            });

            AddCommand("SelectConnectedRooms", "Select connected rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectConnectedRooms();
            });

            AddCommand("RotateRoomsClockwise", "Rotate rooms clockwise", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = -1 }, args.Window);
            });

            AddCommand("RotateRoomsCounterClockwise", "Rotate rooms counterclockwise", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = 1 }, args.Window);
            });

            AddCommand("MirrorRoomsX", "Mirror rooms on X axis", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.TransformRooms(new RectTransformation { MirrorX = true }, args.Window);
            });

            AddCommand("MirrorRoomsZ", "Mirror rooms on Z axis", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.TransformRooms(new RectTransformation { MirrorX = true, QuadrantRotation = 2 }, args.Window);
            });

            AddCommand("LockRoom", "Lock room position", CommandType.Rooms, delegate (CommandArgs args)
            {
                args.Editor.SelectedRoom.Locked = !args.Editor.SelectedRoom.Locked;
                args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
            });

            AddCommand("SplitRoom", "Split room", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SplitRoom(args.Window);
            });

            AddCommand("CropRoom", "Crop room...", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CropRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window);
            });

            AddCommand("NewRoomUp", "New room up", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.Ceiling, 12);
            });

            AddCommand("NewRoomDown", "New room down", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.Floor, 12);
            });

            AddCommand("NewRoomLeft", "New room left", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.WallNegativeX, 12);
            });

            AddCommand("NewRoomRight", "New room right", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.WallPositiveX, 12);
            });

            AddCommand("NewRoomFront", "New room front", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.WallPositiveZ, 12);
            });

            AddCommand("NewRoomBack", "New room back", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.CreateAdjoiningRoom(args.Editor.SelectedRoom, args.Editor.SelectedSectors, PortalDirection.WallNegativeZ, 12);
            });

            AddCommand("MergeRoomsHorizontally", "Merge rooms horizontally", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.MergeRoomsHorizontally(args.Editor.SelectedRooms, args.Window);
            });

            AddCommand("ExportRooms", "Export rooms...", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.ExportRooms(args.Editor.SelectedRooms, args.Window);
            });

            AddCommand("ImportRooms", "Import rooms...", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.ImportRooms(args.Window);
            });

            AddCommand("ApplyAmbientLightToAllRooms", "Apply current ambient light to all rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (DarkMessageBox.Show(args.Window, "Do you really want to apply the ambient light of the current room to all rooms?",
                                       "Apply ambient light", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    EditorActions.ApplyCurrentAmbientLightToAllRooms();
                    args.Editor.SendMessage("Ambient light was applied to all rooms.", PopupType.Info);
                }
            });

            AddCommand("AddWad", "Add wad...", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.AddWad(args.Window);
            });

            AddCommand("RemoveWads", "Remove all wads", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.RemoveWads(args.Window);
            });

            AddCommand("ReloadWads", "Reload all wads", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.ReloadWads(args.Window);
            });

            AddCommand("AddCamera", "Add camera", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new CameraInstance());
            });

            AddCommand("AddFlybyCamera", "Add flyby camera", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new FlybyCameraInstance(args.Editor.SelectedObject));
            });

            AddCommand("AddSink", "Add sink", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new SinkInstance());
            });

            AddCommand("AddSoundSource", "Add sound source", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new SoundSourceInstance());
            });

            AddCommand("AddImportedGeometry", "Add imported geometry", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new ImportedGeometryInstance());
            });


            AddCommand("AddItem", "Add item", CommandType.Objects, delegate (CommandArgs args)
            {
                var currentItem = EditorActions.GetCurrentItemWithMessage();
                if (currentItem == null)
                    return;

                if (!currentItem.Value.IsStatic && args.Editor.SelectedRoom.Alternated && args.Editor.SelectedRoom.AlternateRoom == null)
                {
                    args.Editor.SendMessage("You can't add moveables to a flipped room.", PopupType.Info);
                    return;
                }

                args.Editor.Action = new EditorActionPlace(false, (r, l) => ItemInstance.FromItemType(currentItem.Value));
            });

            AddCommand("LocateItem", "Locate item", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.FindItem();
            });

            AddCommand("MoveLara", "Move Lara here", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;

                EditorActions.MoveLara(args.Window, args.Editor.SelectedRoom, args.Editor.SelectedSectors.Start);
            });

            AddCommand("AssignAndClipboardNgId", "Assign and copy the NG ID to clipboard", CommandType.Objects, delegate (CommandArgs args)
            {
                if(args.Editor.SelectedObject == null)
                {
                    args.Editor.SendMessage("Select an object first.", PopupType.Warning);
                    return;
                }

                var selectedObj = args.Editor.SelectedObject as IHasScriptID;
                if (selectedObj == null)
                {
                    args.Editor.SendMessage("The selected object does not have a script ID.", PopupType.Warning);
                    return;
                }
                if (selectedObj.ScriptId == null)
                    selectedObj.AllocateNewScriptId();
                Clipboard.SetText(selectedObj.ScriptId.Value.ToString());
                args.Editor.SendMessage("Script ID for selected object is " + selectedObj.ScriptId + ".\nCopied to clipboard.", PopupType.Info);
            });

            AddCommand("SplitSectorObjectOnSelection", "Split sector based object on selection", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.SplitSectorObjectOnSelection(args.Editor.SelectedObject as SectorBasedObjectInstance);
            });

            AddCommand("AddTexture", "Add texture...", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.AddTexture(args.Window);
            });

            AddCommand("RemoveTextures", "Remove all textures", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.RemoveTextures(args.Window);
            });

            AddCommand("UnloadTextures", "Unload all textures", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.UnloadTextures(args.Window);
            });

            AddCommand("ReloadTextures", "Reload all textures", CommandType.Textures, delegate (CommandArgs args)
            {
                foreach (var texture in args.Editor.Level.Settings.Textures)
                    texture.Reload(args.Editor.Level.Settings);
                args.Editor.LoadedTexturesChange();
            });

            AddCommand("ConvertTexturesToPNG", "Convert all textures to PNG", CommandType.Textures, delegate (CommandArgs args)
            {
                if (args.Editor.Level == null || args.Editor.Level.Settings.Textures.Count == 0)
                {
                    args.Editor.SendMessage("No texture loaded. Nothing to convert.", PopupType.Error);
                    return;
                }

                foreach (LevelTexture texture in args.Editor.Level.Settings.Textures)
                {
                    if (texture.LoadException != null)
                    {
                        args.Editor.SendMessage("The texture that should be converted to *.png could not be loaded. " + texture.LoadException?.Message, PopupType.Error);
                        return;
                    }

                    string currentTexturePath = args.Editor.Level.Settings.MakeAbsolute(texture.Path);
                    string pngFilePath = Path.Combine(Path.GetDirectoryName(currentTexturePath), Path.GetFileNameWithoutExtension(currentTexturePath) + ".png");

                    if (File.Exists(pngFilePath))
                    {
                        if (DarkMessageBox.Show(args.Window,
                                "There is already a file at \"" + pngFilePath + "\". Continue and overwrite the file?",
                                "File already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                            return;
                    }
                    texture.Image.Save(pngFilePath);

                    args.Editor.SendMessage("TGA texture map was converted to PNG without errors and saved at \"" + pngFilePath + "\".", PopupType.Info);
                    texture.SetPath(args.Editor.Level.Settings, pngFilePath);
                }
                args.Editor.LoadedTexturesChange();
            });

            AddCommand("RemapTexture", "Remap texture...", CommandType.Textures, delegate (CommandArgs args)
            {
                using (var form = new FormTextureRemap(args.Editor))
                    form.ShowDialog(args.Window);
            });

            AddCommand("TextureFloor", "Texture floor", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, args.Editor.SelectedTexture, BlockFaceType.Floor);
            });

            AddCommand("TextureCeiling", "Texture ceiling", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, args.Editor.SelectedTexture, BlockFaceType.Ceiling);
            });

            AddCommand("TextureWalls", "Texture walls", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, args.Editor.SelectedTexture, BlockFaceType.Wall);
            });

            AddCommand("ClearAllTexturesInRoom", "Clear all textures in room", CommandType.Textures, delegate (CommandArgs args)
            {
                var emptyTexture = new TextureArea() { Texture = null };
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, emptyTexture, BlockFaceType.Floor);
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, emptyTexture, BlockFaceType.Ceiling);
                EditorActions.TexturizeAll(args.Editor.SelectedRoom, args.Editor.SelectedSectors, emptyTexture, BlockFaceType.Wall);
            });

            AddCommand("EditAnimationRanges", "Edit animation ranges...", CommandType.Textures, delegate (CommandArgs args)
            {
                var existingWindow = Application.OpenForms["FormAnimatedTextures"];
                if (existingWindow == null)
                {
                    FormAnimatedTextures form = new FormAnimatedTextures(args.Editor, null);
                    form.Show(args.Window);
                }
                else
                    existingWindow.Focus();
            });

            AddCommand("SmoothRandomFloorUp", "Smooth random floor up", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SmoothRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, 1, BlockVertical.Floor);
            });

            AddCommand("SmoothRandomFloorDown", "Smooth random floor down", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SmoothRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, -1, BlockVertical.Floor);
            });

            AddCommand("SmoothRandomCeilingUp", "Smooth random ceiling up", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SmoothRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, 1, BlockVertical.Ceiling);
            });

            AddCommand("SmoothRandomCeilingDown", "Smooth random ceiling down", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SmoothRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, -1, BlockVertical.Ceiling);
            });

            AddCommand("SharpRandomFloorUp", "Sharp random floor up", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SharpRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, 1, BlockVertical.Floor);
            });

            AddCommand("SharpRandomFloorDown", "Sharp random floor down", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SharpRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, -1, BlockVertical.Floor);
            });

            AddCommand("SharpRandomCeilingUp", "Sharp random ceiling up", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SharpRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, 1, BlockVertical.Ceiling);
            });

            AddCommand("SharpRandomCeilingDown", "Sharp random ceiling down", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SharpRandom(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, -1, BlockVertical.Ceiling);
            });

            AddCommand("AverageFloor", "Average floor", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.AverageSectors(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockVertical.Floor);
            });

            AddCommand("AverageCeiling", "Average ceiling", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.AverageSectors(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockVertical.Ceiling);
            });

            AddCommand("GridWallsIn3", "Grid walls in 3", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    EditorActions.GridWalls(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("GridWallsIn5", "Grid walls in 5", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    EditorActions.GridWalls(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, true);
            });

            AddCommand("EditLevelSettings", "Level settings...", CommandType.Settings, delegate (CommandArgs args)
            {
                using (FormLevelSettings form = new FormLevelSettings(args.Editor))
                    form.ShowDialog(args.Window);
            });

            AddCommand("EditOptions", "Editor options...", CommandType.Settings, delegate (CommandArgs args)
            {
                using (FormOptions form = new FormOptions(args.Editor))
                    form.ShowDialog(args.Window);
            });

            AddCommand("StartWadTool", "Start Wad Tool...", CommandType.Settings, delegate (CommandArgs args)
            {
                try
                {
                    Process.Start("WadTool.exe");
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Error while starting Wad Tool.");
                    args.Editor.SendMessage("Error while starting Wad Tool.", PopupType.Error);
                }
            });

            AddCommand("EditKeyboardLayout", "Edit keyboard layout...", CommandType.Settings, delegate (CommandArgs args)
            {
                using (var f = new FormKeyboardLayout(args.Editor))
                    f.ShowDialog(args.Window);
            });

            AddCommand("SwitchTool1", "Switch tool 1", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(0);
            });

            AddCommand("SwitchTool2", "Switch tool 2", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(1);
            });

            AddCommand("SwitchTool3", "Switch tool 3", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(2);
            });

            AddCommand("SwitchTool4", "Switch tool 4", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(3);
            });

            AddCommand("SwitchTool5", "Switch tool 5", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(4);
            });

            AddCommand("SwitchTool6", "Switch tool 6", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(5);
            });

            AddCommand("SwitchTool7", "Switch tool 7", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(6);
            });

            AddCommand("SwitchTool8", "Switch tool 8", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(7);
            });

            AddCommand("SwitchTool9", "Switch tool 9", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(8);
            });

            AddCommand("SwitchTool10", "Switch tool 10", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(9);
            });

            AddCommand("SwitchTool11", "Switch tool 11", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(10);
            });

            AddCommand("SwitchTool12", "Switch tool 12", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(11);
            });

            AddCommand("SwitchTool13", "Switch tool 13", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(12);
            });

            AddCommand("SwitchTool14", "Switch tool 14", CommandType.General, delegate (CommandArgs args)
            {
                EditorActions.SwitchToolOrdered(13);
            });

            AddCommand("QuitEditor", "Quit editor", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Quit();
            });

            AddCommand("DrawPortals", "Draw portals", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowPortals = !args.Editor.Configuration.Rendering3D_ShowPortals;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawHorizon", "Draw horizon", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowHorizon = !args.Editor.Configuration.Rendering3D_ShowHorizon;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawRoomNames", "Draw room names", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowRoomNames = !args.Editor.Configuration.Rendering3D_ShowRoomNames;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawIllegalSlopes", "Draw illegal slopes", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowIllegalSlopes = !args.Editor.Configuration.Rendering3D_ShowIllegalSlopes;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawMoveables", "Draw moveables", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowMoveables = !args.Editor.Configuration.Rendering3D_ShowMoveables;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawStatics", "Draw statics", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowStatics = !args.Editor.Configuration.Rendering3D_ShowStatics;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawImportedGeometry", "Draw imported geometry", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowImportedGeometry = !args.Editor.Configuration.Rendering3D_ShowImportedGeometry;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawOtherObjects", "Draw other objects", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowOtherObjects = !args.Editor.Configuration.Rendering3D_ShowOtherObjects;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawSlideDirections", "Draw slide directions", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowSlideDirections = !args.Editor.Configuration.Rendering3D_ShowSlideDirections;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawExtraBlendingModes", "Draw extra blending modes", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowExtraBlendingModes = !args.Editor.Configuration.Rendering3D_ShowExtraBlendingModes;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DisableGeometryPicking", "Disable geometry picking", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_DisablePickingForImportedGeometry = !args.Editor.Configuration.Rendering3D_DisablePickingForImportedGeometry;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawAllRooms", "Draw all rooms", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowAllRooms = !args.Editor.Configuration.Rendering3D_ShowAllRooms;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawCardinalDirections", "Draw cardinal directions", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowCardinalDirections = !args.Editor.Configuration.Rendering3D_ShowCardinalDirections;
                args.Editor.ConfigurationChange();
            });

            AddCommand("ToggleFlipMap", "Toggle flip map", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom.Alternated)
                {
                    if (args.Editor.SelectedRoom.AlternateRoom != null && args.Editor.SelectedRoom != args.Editor.SelectedRoom.AlternateRoom)
                        args.Editor.SelectedRoom = args.Editor.SelectedRoom.AlternateRoom;
                    else if (args.Editor.SelectedRoom.AlternateBaseRoom != null && args.Editor.SelectedRoom != args.Editor.SelectedRoom.AlternateBaseRoom)
                        args.Editor.SelectedRoom = args.Editor.SelectedRoom.AlternateBaseRoom;
                }
            });

            AddCommand("ToggleNoOpacity", "Toggle no opacity", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.SetPortalOpacity(PortalOpacity.None, args.Window);
            });

            AddCommand("ToggleOpacity", "Textured and solid ('Toggle Opacity 1')", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.SetPortalOpacity(PortalOpacity.SolidFaces, args.Window);
            });

            AddCommand("ToggleOpacity2", "Textured and traversable ('Toggle Opacity 2')", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.SetPortalOpacity(PortalOpacity.TraversableFaces, args.Window);
            });

            AddCommand("AddPointLight", "Add point light", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Point));
            });

            AddCommand("AddShadow", "Add shadow", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Shadow));
            });

            AddCommand("AddSunLight", "Add sun light", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Sun));
            });

            AddCommand("AddSpotLight", "Add directional (spot) light", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Spot));
            });

            AddCommand("AddEffectLight", "Add effect light", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Effect));
            });

            AddCommand("AddFogBulb", "Add fog bulb", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.FogBulb));
            });

            AddCommand("EditRoomName", "Edit room name", CommandType.Rooms, delegate (CommandArgs args)
            {
                using (var form = new FormInputBox("Edit room's name", "Insert the name of this room:", args.Editor.SelectedRoom.Name))
                {
                    if (form.ShowDialog(args.Window) == DialogResult.Cancel)
                        return;

                    args.Editor.SelectedRoom.Name = form.Result;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                    args.Editor.RoomListChange();
                }
            });

            AddCommand("SetFloor", "Set floor", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SetFloor(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetCeiling", "Set ceiling", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SetCeiling(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetWall", "Set wall", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SetWall(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetBox", "Set box sector property", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.Box);
            });

            AddCommand("SetDeath", "Set death sector property", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.DeathFire);
            });

            AddCommand("SetMonkeyswing", "Set monkeyswing sector property", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.Monkey);
            });

            AddCommand("SetClimbPositiveZ", "Climb on North sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.ClimbPositiveZ);
            });

            AddCommand("SetClimbPositiveX", "Climb on East sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.ClimbPositiveX);
            });

            AddCommand("SetClimbNegativeZ", "Climb on South sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.ClimbNegativeZ);
            });

            AddCommand("SetClimbNegativeX", "Climb on West sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.ClimbNegativeX);
            });

            AddCommand("SetNotWalkable", "Set non-walkable floor", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.NotWalkableFloor);
            });

            AddCommand("SetDiagonalFloorStep", "Set or rotate diagonal floor step", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SetDiagonalFloorSplit(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetDiagonalCeilingStep", "Set or rotate diagonal ceiling step", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SetDiagonalCeilingSplit(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetDiagonalWall", "Set or rotate diagonal wall", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.SetDiagonalWall(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetBeetleCheckpoint", "Set beetle checkpoint", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.Beetle);
            });

            AddCommand("SetTriggerTriggerer", "Delay trigger until Trigger Triggerer is used", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.TriggerTriggerer);
            });

            AddCommand("ToggleForceFloorSolid", "Force solid floor", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleForceFloorSolid(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("SetRoomOutside", "Set to outside", CommandType.Rooms, delegate (CommandArgs args)
            {
                if(args.Editor.SelectedRoom != null )
                {
                    args.Editor.SelectedRoom.FlagOutside = !args.Editor.SelectedRoom.FlagOutside;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomSkybox", "Set skybox", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.FlagHorizon = !args.Editor.SelectedRoom.FlagHorizon;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomNoLensflare", "Disable global lensflare", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.FlagNoLensflare = !args.Editor.SelectedRoom.FlagNoLensflare;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomNoPathfinding", "Exclude from pathfinding", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.FlagExcludeFromPathFinding = !args.Editor.SelectedRoom.FlagExcludeFromPathFinding;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomCold", "Set room to cold (TRNG only)", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.FlagCold = !args.Editor.SelectedRoom.FlagCold;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomDamage", "Set room to damage (TRNG only)", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.FlagDamage = !args.Editor.SelectedRoom.FlagDamage;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("FlattenFloor", "Flatten floor", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null && args.Editor.SelectedSectors.ValidOrNone)
                    EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Valid ? args.Editor.SelectedSectors.Area : args.Editor.SelectedRoom.LocalArea.Inflate(-1), null, false, false, true);
            });

            AddCommand("FlattenCeiling", "Flatten ceiling", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null && args.Editor.SelectedSectors.ValidOrNone)
                    EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Valid ? args.Editor.SelectedSectors.Area : args.Editor.SelectedRoom.LocalArea.Inflate(-1), null, true, false, true);
            });

            _commands = _commands.OrderBy(o => o.Type).ToList();
        }
    }
}
