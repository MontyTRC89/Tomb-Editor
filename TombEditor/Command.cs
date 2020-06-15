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
using TombLib.Controls;
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
            var controls = WinFormsUtils.AllSubControls(parent).Where(c => c is DarkButton || c is DarkCheckBox || c is DarkPanel).ToList();
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

        private static void GenericDirectionalControlCommand(CommandArgs args, BlockVertical surface, short increment, bool smooth, bool oppositeDiagonal)
        {
            if (args.Editor.LastSelection == LastSelectionType.Block && args.Editor.Mode == EditorMode.Geometry && args.Editor.SelectedSectors.Valid)
            {
                EditorActions.EditSectorGeometry(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Editor.SelectedSectors.Arrow, surface, increment, smooth, oppositeDiagonal);
            }
            else if (args.Editor.LastSelection == LastSelectionType.SpatialObject && (surface == BlockVertical.Floor || surface == BlockVertical.Ceiling) && !oppositeDiagonal && !smooth)
            {
                if (args.Editor.SelectedObject is PositionBasedObjectInstance && surface == BlockVertical.Floor)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)args.Editor.SelectedObject, new Vector3(0, increment * 256, 0), new Vector3(), true);
                else if (args.Editor.SelectedObject is GhostBlockInstance)
                {
                    ((GhostBlockInstance)args.Editor.SelectedObject).Move(increment, surface == BlockVertical.Floor);
                    args.Editor.RoomSectorPropertiesChange(args.Editor.SelectedRoom);
                }
            }
        }

        private static void AddCommand(string commandName, string friendlyName, CommandType type, Action<CommandArgs> command)
        {
            if (_commands.Any(cmd => cmd.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase)))
                throw new InvalidOperationException("You cannot add multiple commands with the same name.");

            command += delegate { logger.Info(commandName); };
            _commands.Add(new CommandObj() { Name = commandName, FriendlyName = friendlyName, Execute = command, Type = type });
        }

        static CommandHandler()
        {
            AddCommand("CancelAnyAction", "Cancel any action", CommandType.General, delegate (CommandArgs args)
            {
                if (args.Editor.Action != null)
                    args.Editor.Action = null;
                else
                {
                    args.Editor.SelectedSectors = SectorSelection.None;
                    args.Editor.SelectedObject = null;
                    args.Editor.SelectedRooms = new[] { args.Editor.SelectedRoom };
                }
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

            AddCommand("ResetCamera", "Reset camera position", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.ResetCamera();
            });

            AddCommand("AddTrigger", "Add trigger", CommandType.Objects, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndBlockSelection(args.Window))
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift) || args.Editor.SelectedObject == null ||
                        !(args.Editor.SelectedObject is PositionBasedObjectInstance))
                        EditorActions.AddTrigger(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window, args.Editor.BookmarkedObject);
                    else
                        EditorActions.AddTrigger(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, args.Window, args.Editor.SelectedObject);
                }
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

            AddCommand("SwitchBlendMode", "Switch blending mode", CommandType.Textures, delegate (CommandArgs args)
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

            AddCommand("SetBlendModeNormal", "Set blending mode: Normal", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Normal;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeAdd", "Set blending mode: Add", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Additive;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeSubtract", "Set blending mode: Subtract", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Subtract;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeExclude", "Set blending mode: Exclude", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Exclude;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeScreen", "Set blending mode: Screen", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Screen;
                args.Editor.SelectedTexture = texture;
            });

            AddCommand("SetBlendModeLighten", "Set blending mode: Lighten", CommandType.Textures, delegate (CommandArgs args)
            {
                var texture = args.Editor.SelectedTexture;
                texture.BlendMode = BlendMode.Lighten;
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

            AddCommand("ChangeTextureSelectionTileSize", "Change texture selection tile size", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.SetSelectionTileSize(args.Editor.Configuration.TextureMap_TileSelectionSize * 2);
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

            AddCommand("SelectPreviousRoom", "Select previous room", CommandType.Rooms, delegate (CommandArgs args)
            {
                var prevRoom = args.Editor.PreviousRoom;

                if (prevRoom != null)
                    args.Editor.SelectRoom(prevRoom);
                else
                    args.Editor.SendMessage("There is no previous room specified or previous room was deleted.", PopupType.Info);
            });

            AddCommand("MoveRoomLeft", "Move room left", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRooms != null) {
                    foreach(Room r in args.Editor.SelectedRooms)
                    EditorActions.MoveRooms(new VectorInt3(-1, 0, 0), r.Versions);
                }
                    
            });

            AddCommand("MoveRoomRight", "Move room right", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRooms != null)
                {
                    foreach (Room r in args.Editor.SelectedRooms)
                        EditorActions.MoveRooms(new VectorInt3(1, 0, 0), r.Versions);
                }
            });

            AddCommand("MoveRoomForward", "Move room forward", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRooms != null)
                {
                    foreach (Room r in args.Editor.SelectedRooms)
                        EditorActions.MoveRooms(new VectorInt3(0, 0, 1), r.Versions);
                }
            });

            AddCommand("MoveRoomBack", "Move room back", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRooms != null)
                {
                    foreach (Room r in args.Editor.SelectedRooms)
                        EditorActions.MoveRooms(new VectorInt3(0, 0, -1), r.Versions);
                }
            });

            AddCommand("MoveRoomUp", "Move room up", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRooms != null)
                {
                    foreach (Room r in args.Editor.SelectedRooms)
                        EditorActions.MoveRooms(new VectorInt3(0, 1, 0), r.Versions);
                }
            });

            AddCommand("MoveRoomDown", "Move room down", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRooms != null)
                {
                    foreach (Room r in args.Editor.SelectedRooms)
                        EditorActions.MoveRooms(new VectorInt3(0, -1, 0), r.Versions);
                }
            });

            AddCommand("MoveRoomUp4Clicks", "Move room up (4 clicks)", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRooms != null)
                {
                    foreach (Room r in args.Editor.SelectedRooms)
                        EditorActions.MoveRooms(new VectorInt3(0, 4, 0), r.Versions);
                }
            });

            AddCommand("MoveRoomDown4Clicks", "Move room down (4 clicks)", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRooms != null)
                {
                    foreach (Room r in args.Editor.SelectedRooms)
                        EditorActions.MoveRooms(new VectorInt3(0, -4, 0), r.Versions);
                }
            });

            AddCommand("RaiseQA1Click", "Raise selected floor or item (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, 1, false, false);
            });

            AddCommand("RaiseQA4Click", "Raise selected floor or item (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, 4, false, false);
            });

            AddCommand("LowerQA1Click", "Lower selected floor or item (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, -1, false, false);
            });

            AddCommand("LowerQA4Click", "Lower selected floor or item (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, -4, false, false);
            });

            AddCommand("RaiseWS1Click", "Raise selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, 1, false, false);
            });

            AddCommand("RaiseWS4Click", "Raise selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, 4, false, false);
            });

            AddCommand("LowerWS1Click", "Lower selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, -1, false, false);
            });

            AddCommand("LowerWS4Click", "Lower selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, -4, false, false);
            });

            AddCommand("RaiseED1Click", "Raise selected floor subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ed, 1, false, false);
            });

            AddCommand("RaiseED4Click", "Raise selected floor subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ed, 4, false, false);
            });

            AddCommand("LowerED1Click", "Lower selected floor subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ed, -1, false, false);
            });

            AddCommand("LowerED4Click", "Lower selected floor subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ed, -4, false, false);
            });

            AddCommand("RaiseRF1Click", "Raise selected ceiling subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Rf, 1, false, false);
            });

            AddCommand("RaiseRF4Click", "Raise selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Rf, 4, false, false);
            });

            AddCommand("LowerRF1Click", "Lower selected ceiling subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Rf, -1, false, false);
            });

            AddCommand("LowerRF4Click", "Lower selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Rf, -4, false, false);
            });

            AddCommand("RaiseQA1ClickSmooth", "Smoothly raise selected floor (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, 1, true, false);
            });

            AddCommand("RaiseQA4ClickSmooth", "Smoothly raise selected floor (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, 4, true, false);
            });

            AddCommand("LowerQA1ClickSmooth", "Smoothly lower selected floor (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, -1, true, false);
            });

            AddCommand("LowerQA4ClickSmooth", "Smoothly lower selected floor (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, -4, true, false);
            });

            AddCommand("RaiseWS1ClickSmooth", "Smoothly raise selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, 1, true, false);
            });

            AddCommand("RaiseWS4ClickSmooth", "Smoothly raise selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, 4, true, false);
            });

            AddCommand("LowerWS1ClickSmooth", "Smoothly lower selected ceiling (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, -1, true, false);
            });

            AddCommand("LowerWS4ClickSmooth", "Smoothly lower selected ceiling (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, -4, true, false);
            });

            AddCommand("RaiseED1ClickSmooth", "Smoothly raise selected floor subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ed, 1, true, false);
            });

            AddCommand("RaiseED4ClickSmooth", "Smoothly raise selected floor subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ed, 4, true, false);
            });

            AddCommand("LowerED1ClickSmooth", "Smoothly lower selected floor subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ed, -1, true, false);
            });

            AddCommand("LowerED4ClickSmooth", "Smoothly lower selected floor subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ed, -4, true, false);
            });

            AddCommand("RaiseRF1ClickSmooth", "Smoothly raise selected ceiling subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Rf, 1, true, false);
            });

            AddCommand("RaiseRF4ClickSmooth", "Smoothly raise selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Rf, 4, true, false);
            });

            AddCommand("LowerRF1ClickSmooth", "Smoothly lower selected ceiling subdivision (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Rf, -1, true, false);
            });

            AddCommand("LowerRF4ClickSmooth", "Smoothly lower selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Rf, -4, true, false);
            });

            AddCommand("RaiseYH1Click", "Raise selected floor diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, 1, false, true);
            });

            AddCommand("RaiseYH4Click", "Raise selected floor diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, 4, false, true);
            });

            AddCommand("LowerYH1Click", "Lower selected floor diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, -1, false, true);
            });

            AddCommand("LowerYH4Click", "Lower selected floor diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Floor, -4, false, true);
            });

            AddCommand("RaiseUJ1Click", "Raise selected ceiling diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, 1, false, true);
            });

            AddCommand("RaiseUJ4Click", "Raise selected ceiling diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, 4, false, true);
            });

            AddCommand("LowerUJ1Click", "Lower selected ceiling diagonal step (1 click)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, -1, false, true);
            });

            AddCommand("LowerUJ4Click", "Lower selected ceiling diagonal step (4 clicks)", CommandType.Geometry, delegate (CommandArgs args)
            {
                GenericDirectionalControlCommand(args, BlockVertical.Ceiling, -4, false, true);
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

                args.Editor.Level = Level.CreateSimpleLevel(args.Editor.Configuration.Editor_DefaultProjectGameVersion);
                GC.Collect(); // Clean up memory
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

            AddCommand("BuildAndPlayPreview", "Build level and play in preview mode", CommandType.File, delegate (CommandArgs args)
            {
                EditorActions.BuildLevelAndPlay(args.Window, true);
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

            AddCommand("DeleteAllObjects", "Delete all objects", CommandType.Edit, delegate (CommandArgs args)
            {

                if (DarkMessageBox.Show(args.Window, "Do you want to delete all objects in level? This action can't be undone.",
                                       "Delete all objects", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var deleteLights = (DarkMessageBox.Show(args.Window, "Delete lights as well?",
                                         "Delete all objects", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);

                    foreach (var room in args.Editor.Level.Rooms.Where(r => r != null))
                    {
                        var objects = room.Objects.Where(ob => ob is PositionBasedObjectInstance && (!(ob is LightInstance) || deleteLights)).ToList();
                        if (objects.Count > 0)
                            for (int i = objects.Count - 1; i >= 0; i--)
                            {
                                var obj = objects[i];
                                EditorActions.DeleteObjectWithoutUpdate(obj);
                                objects.RemoveAt(i);
                            }
                    }
                }
            });

            AddCommand("DeleteAllTriggers", "Delete all triggers", CommandType.Edit, delegate (CommandArgs args)
            {

                if (DarkMessageBox.Show(args.Window, "Do you want to delete all triggers in level? This action can't be undone.",
                                       "Delete all triggers", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (var room in args.Editor.Level.Rooms.Where(r => r != null))
                    {
                        var triggers = room.Triggers.ToList();
                        if (triggers.Count > 0)
                            for (int i = triggers.Count - 1; i >= 0; i--)
                            {
                                var obj = triggers[i];
                                EditorActions.DeleteObjectWithoutUpdate(obj);
                                triggers.RemoveAt(i);
                            }
                    }
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
                    var searchForm = new FormSearch(args.Editor);
                    searchForm.Show(args.Window);
                }
                else
                    existingWindow.Focus();
            });

            AddCommand("SearchAndReplaceObjects", "Search and replace objects...", CommandType.Edit, delegate (CommandArgs args)
            {
                var existingWindow = Application.OpenForms["FormReplaceObject"];
                if (existingWindow == null)
                {
                    var searchAndReplaceForm = new FormReplaceObject(args.Editor);
                    searchAndReplaceForm.Show(args.Window);
                }
                else
                    existingWindow.Focus();
            });

            AddCommand("AddNewRoom", "Add new room", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (args.Editor.Level.Rooms == null)
                    return;

                for (int i = 0; i < Level.MaxNumberOfRooms; i++)
                {
                    if (args.Editor.Level.Rooms[i] != null)
                        continue;
                    else
                    {
                        EditorActions.MakeNewRoom(i);
                        return;
                    }
                }

                args.Editor.SendMessage("Maximum amount of rooms reached. Can't create new room.", PopupType.Error);
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

            AddCommand("HideRoom", "Hide room", CommandType.Rooms, delegate (CommandArgs args)
            {
                args.Editor.SelectedRoom.Hidden = !args.Editor.SelectedRoom.Hidden;
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


            AddCommand("EditAmbientLight", "Edit ambient light...", CommandType.Rooms, delegate (CommandArgs args)
            {
                Room room = args.Editor.SelectedRoom;

                using (var colorDialog = new RealtimeColorDialog(
                    args.Editor.Configuration.ColorDialog_Position.X,
                    args.Editor.Configuration.ColorDialog_Position.Y,
                    c =>
                    {
                        room.AmbientLight = c.ToFloat3Color() * 2.0f;
                        args.Editor.SelectedRoom.BuildGeometry();
                        args.Editor.RoomPropertiesChange(room);
                    }, args.Editor.Configuration.UI_ColorScheme))
                {
                    colorDialog.Color = (room.AmbientLight * 0.5f).ToWinFormsColor();
                    var oldLightColor = colorDialog.Color;

                    if (colorDialog.ShowDialog(args.Window) != DialogResult.OK)
                        colorDialog.Color = oldLightColor;

                    var newColor = colorDialog.Color.ToFloat3Color() * 2.0f;
                    if (args.Editor.Level.Settings.GameVersion < TRVersion.Game.TR3)
                    {
                        if (!colorDialog.Color.IsGrayscale())
                            args.Editor.SendMessage("Only grayscale lighting is possible for this game version.", PopupType.Info);
                        newColor = Vector3.Clamp(new Vector3(newColor.GetLuma()), Vector3.Zero, Vector3.One);
                    }
                    room.AmbientLight = newColor;

                    args.Editor.Configuration.ColorDialog_Position = colorDialog.Position;
                }

                args.Editor.SelectedRoom.BuildGeometry();
                args.Editor.RoomPropertiesChange(room);
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

            AddCommand("ApplyAmbientLightToSelectedRooms", "Set ambient light for selected rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (EditorActions.ApplyAmbientLightToSelectedRooms(args.Window))
                    args.Editor.SendMessage("Ambient light was applied to selected rooms.", PopupType.Info);
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

            AddCommand("ReloadSounds", "Reload all sound catalogs", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.ReloadSounds(args.Window);
            });

            AddCommand("AddCamera", "Add camera", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new CameraInstance());
            });

            AddCommand("AddFlybyCamera", "Add flyby camera", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR4, "Flyby camera"))
                    return;

                args.Editor.Action = new EditorActionPlace(false, (l, r) => new FlybyCameraInstance(args.Editor.SelectedObject));
            });

            AddCommand("AddSink", "Add sink", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new SinkInstance());
            });

            AddCommand("AddGhostBlock", "Add ghost block", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new GhostBlockInstance());
            });

            AddCommand("AddSoundSource", "Add sound source", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new SoundSourceInstance());
            });

            AddCommand("AddImportedGeometry", "Add imported geometry", CommandType.Objects, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new ImportedGeometryInstance() { Model = args.Editor.ChosenImportedGeometry });
            });

            AddCommand("AddBoxVolume", "Add box volume", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main, "Volume"))
                    return;
                EditorActions.AddVolume(VolumeShape.Box);
            });

            AddCommand("AddPrismVolume", "Add prism volume", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main, "Volume"))
                    return;
                EditorActions.AddVolume(VolumeShape.Prism);
            });

            AddCommand("AddSphereVolume", "Add sphere volume", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main, "Volume"))
                    return;
                EditorActions.AddVolume(VolumeShape.Sphere);
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
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion == TRVersion.Game.TRNG, "This feature"))
                    return;

                if (args.Editor.SelectedObject == null)
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

            AddCommand("FindUntextured", "Find untextured faces...", CommandType.Textures, delegate (CommandArgs args)
            {
                var existingWindow = Application.OpenForms["FormFindUntextured"];
                if (existingWindow == null)
                {
                    var findUntexturedForm = new FormFindUntextured(args.Editor);
                    findUntexturedForm.Show(args.Window);
                }
                else
                    existingWindow.Focus();
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

            AddCommand("ClearAllTexturesInLevel", "Clear all textures in level", CommandType.Textures, delegate (CommandArgs args)
            {
                EditorActions.ClearAllTexturesInLevel(args.Editor.Level);
            });

            AddCommand("EditAnimationRanges", "Edit animation ranges...", CommandType.Textures, delegate (CommandArgs args)
            {
                var existingWindow = Application.OpenForms["FormAnimatedTextures"];
                if (existingWindow == null)
                {
                    FormAnimatedTextures form = new FormAnimatedTextures(args.Editor);
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

            AddCommand("GridWallsIn3Squares", "Grid walls in 3 (squares)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    EditorActions.GridWallsSquares(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("GridWallsIn5Squares", "Grid walls in 5 (squares)", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    EditorActions.GridWallsSquares(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, true);
            });

            AddCommand("EditLevelSettings", "Level settings...", CommandType.Settings, delegate (CommandArgs args)
            {
                using (FormLevelSettings form = new FormLevelSettings(args.Editor))
                    form.ShowDialog(args.Window);
            });

            AddCommand("EditOptions", "Editor options...", CommandType.Settings, delegate (CommandArgs args)
            {
                using (Forms.FormOptions form = new Forms.FormOptions((Editor)args.Editor))
                    form.ShowDialog((IWin32Window)args.Window);
            });

            AddCommand("StartWadTool", "Start Wad Tool...", CommandType.Settings, delegate (CommandArgs args)
            {
                try
                {
                    if (!string.IsNullOrEmpty(args.Editor.Level.Settings.LevelFilePath))
                        Process.Start("WadTool.exe", "-r \"" + args.Editor.Level.Settings.LevelFilePath + "\"");
                    else
                        Process.Start("WadTool.exe");
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Error while starting Wad Tool.");
                    args.Editor.SendMessage("Error while starting Wad Tool.", PopupType.Error);
                }
            });

            AddCommand("StartSoundTool", "Start Sound Tool...", CommandType.Settings, delegate (CommandArgs args)
            {
                try
                {
                    if (!string.IsNullOrEmpty(args.Editor.Level.Settings.LevelFilePath))
                        Process.Start("SoundTool.exe", "-r \"" + args.Editor.Level.Settings.MakeAbsolute(args.Editor.Level.Settings.LevelFilePath) + "\"");
                    else
                        Process.Start("SoundTool.exe");
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Error while starting Sound Tool.");
                    args.Editor.SendMessage("Error while starting Sound Tool.", PopupType.Error);
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

            AddCommand("DrawGhostBlocks", "Draw ghost blocks", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowGhostBlocks = !args.Editor.Configuration.Rendering3D_ShowGhostBlocks;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawVolumes", "Draw volumes", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowVolumes = !args.Editor.Configuration.Rendering3D_ShowVolumes;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawOtherObjects", "Draw other objects", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowOtherObjects = !args.Editor.Configuration.Rendering3D_ShowOtherObjects;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawLightRadius", "Draw light radius", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowLightRadius = !args.Editor.Configuration.Rendering3D_ShowLightRadius;
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

            AddCommand("HideTransparentFaces", "Hide transparent faces", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_HideTransparentFaces = !args.Editor.Configuration.Rendering3D_HideTransparentFaces;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DrawWhiteTextureLightingOnly", "Draw untextured in Lighting Mode", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowLightingWhiteTextureOnly = !args.Editor.Configuration.Rendering3D_ShowLightingWhiteTextureOnly;
                args.Editor.ConfigurationChange();
            });

            AddCommand("ShowRealTintForObjects", "Show real tint for objects", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_ShowRealTintForObjects = !args.Editor.Configuration.Rendering3D_ShowRealTintForObjects;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DisableGeometryPicking", "Disable geometry picking", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_DisablePickingForImportedGeometry = !args.Editor.Configuration.Rendering3D_DisablePickingForImportedGeometry;
                args.Editor.ConfigurationChange();
            });

            AddCommand("DisableHiddenRoomPicking", "Disable hidden room picking", CommandType.View, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Rendering3D_DisablePickingForHiddenRooms = !args.Editor.Configuration.Rendering3D_DisablePickingForHiddenRooms;
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

            AddCommand("SamplePaletteFromTextures", "Sample palette from textures", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.Configuration.Palette_TextureSamplingMode = !args.Editor.Configuration.Palette_TextureSamplingMode;
                args.Editor.ConfigurationChange();
            });

            AddCommand("ResetPalette", "Reset palette to defaults", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.RaiseEvent(new Editor.ResetPaletteEvent());
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
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Point) { Color = (Vector3)args.Editor.LastUsedPaletteColour * 2.0f });
            });

            AddCommand("AddShadow", "Add shadow", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Shadow) { Color = (Vector3)args.Editor.LastUsedPaletteColour * 2.0f });
            });

            AddCommand("AddSunLight", "Add sun light", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Sun) { Color = (Vector3)args.Editor.LastUsedPaletteColour * 2.0f });
            });

            AddCommand("AddSpotLight", "Add directional (spot) light", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Spot) { Color = (Vector3)args.Editor.LastUsedPaletteColour * 2.0f });
            });

            AddCommand("AddEffectLight", "Add effect light", CommandType.Lighting, delegate (CommandArgs args)
            {
                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.Effect) { Color = (Vector3)args.Editor.LastUsedPaletteColour * 2.0f });
            });

            AddCommand("AddFogBulb", "Add fog bulb", CommandType.Lighting, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR4, "Fog bulb"))
                    return;

                args.Editor.Action = new EditorActionPlace(false, (l, r) => new LightInstance(LightType.FogBulb)
                {
                    Color = args.Editor.Level.Settings.GameVersion.Legacy() <= TRVersion.Game.TR4 ?
                    Vector3.One : (Vector3)args.Editor.LastUsedPaletteColour * 2.0f
                });
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
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR3, "Monkeyswing"))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.Monkey);
            });

            AddCommand("SetClimbPositiveZ", "Climb on North sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR2, "Climbing"))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.ClimbPositiveZ);
            });

            AddCommand("SetClimbPositiveX", "Climb on East sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR2, "Climbing"))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.ClimbPositiveX);
            });

            AddCommand("SetClimbNegativeZ", "Climb on South sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR2, "Climbing"))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.ClimbNegativeZ);
            });

            AddCommand("SetClimbNegativeX", "Climb on West sector side", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR2, "Climbing"))
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

            AddCommand("SetBeetleCheckpoint", "Set beetle checkpoint / minecart right (TR3)", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR3, "This flag"))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.Beetle);
            });

            AddCommand("SetTriggerTriggerer", "Set trigger triggerer / minecart left (TR3)", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR3, "This flag"))
                    return;
                EditorActions.ToggleBlockFlag(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area, BlockFlags.TriggerTriggerer);
            });

            AddCommand("ToggleForceFloorSolid", "Force solid floor", CommandType.Sectors, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.ToggleForceFloorSolid(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
            });

            AddCommand("AddGhostBlocksToSelection", "Add ghost blocks to selected area", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(args.Window))
                    return;
                EditorActions.AddGhostBlocks(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Area);
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
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TR4, "NL flag"))
                    return;
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
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TRNG, "Cold flag"))
                    return;
                if (args.Editor.SelectedRoom != null)
                {
                    args.Editor.SelectedRoom.FlagCold = !args.Editor.SelectedRoom.FlagCold;
                    args.Editor.RoomPropertiesChange(args.Editor.SelectedRoom);
                }
            });

            AddCommand("SetRoomDamage", "Set room to damage (TRNG only)", CommandType.Rooms, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion >= TRVersion.Game.TRNG, "Damage flag"))
                    return;
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

            AddCommand("ResetGeometry", "Reset all geometry", CommandType.Geometry, delegate (CommandArgs args)
            {
                if (args.Editor.SelectedRoom != null && args.Editor.SelectedSectors.ValidOrNone)
                {
                    EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Valid ? args.Editor.SelectedSectors.Area : args.Editor.SelectedRoom.LocalArea.Inflate(-1), null, false, true, false);
                    EditorActions.FlattenRoomArea(args.Editor.SelectedRoom, args.Editor.SelectedSectors.Valid ? args.Editor.SelectedSectors.Area : args.Editor.SelectedRoom.LocalArea.Inflate(-1), null, true, true, true, true);
                }
            });

            AddCommand("ToggleFlyMode", "Toggle fly mode", CommandType.General, delegate (CommandArgs args)
            {
                args.Editor.SendMessage("Push ESC to exit fly mode.", PopupType.Info);
                args.Editor.ToggleFlyMode(!args.Editor.FlyMode);
            });

            AddCommand("SelectSkyRooms", "Select sky rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectSkyRooms();
            });

            AddCommand("SelectWaterRooms", "Select water rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectWaterRooms();
            });

            AddCommand("SelectOutsideRooms", "Select outside rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectOutsideRooms();
            });

            AddCommand("SelectQuicksandRooms", "Select quicksand rooms", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectQuicksandRooms();
            });

            AddCommand("SelectRoomsByTags", "Select rooms by tags", CommandType.Rooms, delegate (CommandArgs args)
            {
                EditorActions.SelectRoomsByTags(args.Window);
            });

            AddCommand("SetStaticMeshesColorToRoomLight", "Set room static meshes color to room color", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.SetStaticMeshesColorToRoomAmbientLight();
            });

            AddCommand("SetStaticMeshesColor", "Set room static meshes color", CommandType.Objects, delegate (CommandArgs args)
            {
                EditorActions.SetStaticMeshesColor(args.Window);
            });

            AddCommand("MakeQuickItemGroup", "Make quick Itemgroup", CommandType.Objects, delegate (CommandArgs args)
            {
                if (!EditorActions.VersionCheck(args.Editor.Level.Settings.GameVersion == TRVersion.Game.TRNG, "Item grouping"))
                    return;
                EditorActions.MakeQuickItemGroup(args.Window);
            });

            _commands = _commands.OrderBy(o => o.Type).ToList();
        }
    }
}
