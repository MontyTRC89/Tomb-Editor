using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
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
        Objects,
        Textures,
        Lighting,
        View,
        Settings
    }

    public class CommandObj
    {
        public string Name;
        public string FriendlyName;
        public Action Command;
        public CommandType Type;
    }

    public class CommandHandler
    {
        private readonly Editor _editor;
        private IWin32Window _editorWindow;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private bool _primaryControlFocused;

        public List<CommandObj> Commands { get; private set; }

        public CommandHandler(IWin32Window window, Editor editor)
        {
            _editorWindow = window;
            _editor = editor;

            Commands = new List<CommandObj>();
            AddCommands();
        }

        public void ProcessHotkeys(Keys keyData, bool primaryControlFocused)
        {
            _primaryControlFocused = primaryControlFocused;

            var commandNames = _editor.Configuration.Keyboard_Hotkeys.Where(set => set.Hotkeys.Contains((uint)keyData)).ToList();
            foreach (var name in commandNames)
                Execute(name.Name);
        }

        public void Execute(string commandName)
        {
            Commands.FirstOrDefault(cmd => cmd.Name.ToUpper().Equals(commandName.ToUpper()))?.Command.Invoke();
        }

        private void AddCommand(string commandName, string friendlyName, CommandType type, Action command)
        {
            if (Commands.Any(cmd => cmd.Name.ToUpper().Equals(commandName.ToUpper())))
                return;
            Commands.Add(new CommandObj() { Name = commandName, FriendlyName = friendlyName, Command = command, Type = type });
        }

        private void AddCommands()
        {
            AddCommand("CancelAnyAction", "Cancel any action", CommandType.General, delegate ()
            {
                _editor.Action = null;
                _editor.SelectedSectors = SectorSelection.None;
                _editor.SelectedObject = null;
                _editor.SelectedRooms = new[] { _editor.SelectedRoom };
            });

            AddCommand("Switch2DMode", "Switch to 2D map", CommandType.General, delegate ()
            {
                EditorActions.SwitchMode(EditorMode.Map2D);
            });

            AddCommand("SwitchGeometryMode", "Switch to Geometry mode", CommandType.General, delegate ()
            {
                EditorActions.SwitchMode(EditorMode.Geometry);
            });

            AddCommand("SwitchFaceEditMode", "Switch to Face Edit mode", CommandType.General, delegate ()
            {
                EditorActions.SwitchMode(EditorMode.FaceEdit);
            });

            AddCommand("SwitchLightingMode", "Switch to Lighting mode", CommandType.General, delegate ()
            {
                EditorActions.SwitchMode(EditorMode.Lighting);
            });

            AddCommand("ResetCamera", "Reset camera position to default", CommandType.View, delegate ()
            {
                _editor.ResetCamera();
            });

            AddCommand("AddTrigger", "Add trigger...", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editorWindow);
            });

            AddCommand("AddPortal", "Add portal", CommandType.Geometry, delegate ()
            {
                if (_editor.SelectedSectors.Valid && _primaryControlFocused)
                    try
                    {
                        EditorActions.AddPortal(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editorWindow);
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Unable to create portal");
                        _editor.SendMessage("Unable to create portal. \nException: " + exc.Message, PopupType.Error);
                    }
            });

            AddCommand("EditObject", "Edit object properties", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject != null && _primaryControlFocused)
                    EditorActions.EditObject(_editor.SelectedObject, _editorWindow);
            });

            AddCommand("SetTextureBlendMode", "Set blending mode", CommandType.Textures, delegate ()
            {
                var texture = _editor.SelectedTexture;
                if (texture.BlendMode == BlendMode.Additive)
                    texture.BlendMode = BlendMode.Normal;
                else
                    texture.BlendMode = BlendMode.Additive;
                _editor.SelectedTexture = texture;
            });

            AddCommand("SetTextureDoubleSided", "Set double-sided attribute", CommandType.Textures, delegate ()
            {
                var texture = _editor.SelectedTexture;
                texture.DoubleSided = !texture.DoubleSided;
                _editor.SelectedTexture = texture;
            });

            AddCommand("SetTextureInvisible", "Set invisibility attribute", CommandType.Textures, delegate ()
            {
                var texture = _editor.SelectedTexture;
                texture.Texture = TextureInvisible.Instance;
                _editor.SelectedTexture = texture;
            });

            AddCommand("RotateObjectLeft", "Rotate object left", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject != null)
                    EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, -1);
            });

            AddCommand("RotateObjectRight", "Rotate object right", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject != null)
                    EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, 1);
            });

            AddCommand("RotateObjectUp", "Rotate object up", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject != null)
                    EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.X, 1);
            });

            AddCommand("RotateObjectDown", "Rotate object down", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject != null)
                    EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.X, -1);
            });

            AddCommand("MoveObjectLeft", "Move object left (4 clicks)", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(-1024, 0, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectRight", "Move object right (4 clicks)", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(1024, 0, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectForward", "Move object forward (4 clicks)", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, 0, 1024), new Vector3(), true);
            });

            AddCommand("MoveObjectBack", "Move object back (4 clicks)", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, 0, -1024), new Vector3(), true);
            });

            AddCommand("MoveObjectUp", "Move object up", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, 1024, 0), new Vector3(), true);
            });

            AddCommand("MoveObjectDown", "Move object down", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject is PositionBasedObjectInstance)
                    EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, -1024, 0), new Vector3(), true);
            });

            AddCommand("MoveRoomLeft", "Move room left", CommandType.Rooms, delegate ()
            {
                if (_editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(1, 0, 0), _editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomRight", "Move room right", CommandType.Rooms, delegate ()
            {
                if (_editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(-1, 0, 0), _editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomForward", "Move room forward", CommandType.Rooms, delegate ()
            {
                if (_editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(0, 0, -1), _editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomBack", "Move room back", CommandType.Rooms, delegate ()
            {
                if (_editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(0, 0, 1), _editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomUp", "Move room up", CommandType.Rooms, delegate ()
            {
                if (_editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(0, 1, 0), _editor.SelectedRoom.Versions);
            });

            AddCommand("MoveRoomDown", "Move room down", CommandType.Rooms, delegate ()
            {
                if (_editor.SelectedRoom != null)
                    EditorActions.MoveRooms(new VectorInt3(0, -1, 0), _editor.SelectedRoom.Versions);
            });

            AddCommand("RaiseQA1Click", "Raise selected floor (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, 1, false);
            });

            AddCommand("RaiseQA4Click", "Raise selected floor (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, 4, false);
            });

            AddCommand("LowerQA1Click", "Lower selected floor (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, -1, false);
            });

            AddCommand("LowerQA4Click", "Lower selected floor (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, -4, false);
            });

            AddCommand("RaiseWS1Click", "Raise selected ceiling (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 1, false);
            });

            AddCommand("RaiseWS4Click", "Raise selected ceiling (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 4, false);
            });

            AddCommand("LowerWS1Click", "Lower selected ceiling (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -1, false);
            });

            AddCommand("LowerWS4Click", "Lower selected ceiling (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -4, false);
            });

            AddCommand("RaiseED1Click", "Raise selected floor subdivision (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ed, 1, false);
            });

            AddCommand("RaiseED4Click", "Raise selected floor subdivision (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ed, 4, false);
            });

            AddCommand("LowerED1Click", "Lower selected floor subdivision (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ed, -1, false);
            });

            AddCommand("LowerED4Click", "Lower selected floor subdivision (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ed, -4, false);
            });

            AddCommand("RaiseRF1Click", "Raise selected ceiling subdivision (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Rf, 1, false);
            });

            AddCommand("RaiseRF4Click", "Raise selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Rf, 4, false);
            });

            AddCommand("LowerRF1Click", "Lower selected ceiling subdivision (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Rf, -1, false);
            });

            AddCommand("LowerRF4Click", "Lower selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Rf, -4, false);
            });

            AddCommand("RaiseQA1ClickSmooth", "Smoothly raise selected floor (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, 1, true);
            });

            AddCommand("RaiseQA4ClickSmooth", "Smoothly raise selected floor (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, 4, true);
            });

            AddCommand("LowerQA1ClickSmooth", "Smoothly lower selected floor (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, -1, true);
            });

            AddCommand("LowerQA4ClickSmooth", "Smoothly lower selected floor (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, -4, true);
            });

            AddCommand("RaiseWS1ClickSmooth", "Smoothly raise selected ceiling (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 1, true);
            });

            AddCommand("RaiseWS4ClickSmooth", "Smoothly raise selected ceiling (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 4, true);
            });

            AddCommand("LowerWS1ClickSmooth", "Smoothly lower selected ceiling (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -1, true);
            });

            AddCommand("LowerWS4ClickSmooth", "Smoothly lower selected ceiling (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -4, true);
            });

            AddCommand("RaiseED1ClickSmooth", "Smoothly raise selected floor subdivision (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ed, 1, true);
            });

            AddCommand("RaiseED4ClickSmooth", "Smoothly raise selected floor subdivision (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ed, 4, true);
            });

            AddCommand("LowerED1ClickSmooth", "Smoothly lower selected floor subdivision (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ed, -1, true);
            });

            AddCommand("LowerED4ClickSmooth", "Smoothly lower selected floor subdivision (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ed, -4, true);
            });

            AddCommand("RaiseRF1ClickSmooth", "Smoothly raise selected ceiling subdivision (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Rf, 1, true);
            });

            AddCommand("RaiseRF4ClickSmooth", "Smoothly raise selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Rf, 4, true);
            });

            AddCommand("LowerRF1ClickSmooth", "Smoothly lower selected ceiling subdivision (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Rf, -1, true);
            });

            AddCommand("LowerRF4ClickSmooth", "Smoothly lower selected ceiling subdivision (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Rf, -4, true);
            });

            AddCommand("RaiseYH1Click", "Raise selected floor diagonal step (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, 1, false, true);
            });

            AddCommand("RaiseYH4Click", "Raise selected floor diagonal step (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, 4, false, true);
            });

            AddCommand("LowerYH1Click", "Lower selected floor diagonal step (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, -1, false, true);
            });

            AddCommand("LowerYH4Click", "Lower selected floor diagonal step (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Floor, -4, false, true);
            });

            AddCommand("RaiseUJ1Click", "Raise selected ceiling diagonal step (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 1, false, true);
            });

            AddCommand("RaiseUJ4Click", "Raise selected ceiling diagonal step (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, 4, false, true);
            });

            AddCommand("LowerUJ1Click", "Lower selected ceiling diagonal step (1 click)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -1, false, true);
            });

            AddCommand("LowerUJ4Click", "Lower selected ceiling diagonal step (4 clicks)", CommandType.Geometry, delegate ()
            {
                if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && _primaryControlFocused)
                    EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, BlockVertical.Ceiling, -4, false, true);
            });

            AddCommand("RotateObject5", "Rotate object (5 degrees)", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject != null && _primaryControlFocused)
                    EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, 5.0f);
            });

            AddCommand("RotateObject45", "Rotate object (45 degrees)", CommandType.Objects, delegate ()
            {
                if (_editor.SelectedObject != null && _primaryControlFocused)
                    EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, 45.0f);
            });

            AddCommand("RelocateCamera", "Relocate camera", CommandType.View, delegate ()
            {
                _editor.Action = new EditorActionRelocateCamera();
            });

            AddCommand("RotateTexture", "Rotate selected texture", CommandType.Textures, delegate ()
            {
                EditorActions.RotateSelectedTexture();
            });

            AddCommand("MirrorTexture", "Mirror selected texture", CommandType.Textures, delegate ()
            {
                EditorActions.MirrorSelectedTexture();
            });

            AddCommand("NewLevel", "New level", CommandType.File, delegate ()
            {
                if (!EditorActions.ContinueOnFileDrop(_editorWindow, "New level"))
                    return;

                _editor.Level = Level.CreateSimpleLevel();
            });

            AddCommand("OpenLevel", "Open existing level...", CommandType.File, delegate ()
            {
                EditorActions.OpenLevel(_editorWindow);
            });

            AddCommand("SaveLevel", "Save level", CommandType.File, delegate ()
            {
                EditorActions.SaveLevel(_editorWindow, false);
            });

            AddCommand("SaveLevelAs", "Save level as...", CommandType.File, delegate ()
            {
                EditorActions.SaveLevel(_editorWindow, true);
            });

            AddCommand("ImportPrj", "Import TRLE level...", CommandType.File, delegate ()
            {
                EditorActions.OpenLevelPrj(_editorWindow);
            });

            AddCommand("BuildLevel", "Build level", CommandType.File, delegate ()
            {
                EditorActions.BuildLevel(false, _editorWindow);
            });

            AddCommand("BuildAndPlay", "Build level and play", CommandType.File, delegate ()
            {
                EditorActions.BuildLevelAndPlay(_editorWindow);
            });

            AddCommand("Copy", "Copy", CommandType.Edit, delegate ()
            {
                if (_editor.Mode != EditorMode.Map2D)
                {
                    if (_editor.SelectedObject != null)
                        EditorActions.TryCopyObject(_editor.SelectedObject, _editorWindow);
                    else if (_editor.SelectedSectors.Valid)
                        EditorActions.TryCopySectors(_editor.SelectedSectors, _editorWindow);
                }
                else
                    Clipboard.SetDataObject(new RoomClipboardData(_editor), true);
            });

            AddCommand("Paste", "Paste", CommandType.Edit, delegate ()
            {
                if (_editor.Mode != EditorMode.Map2D)
                {
                    if (Clipboard.ContainsData(typeof(ObjectClipboardData).FullName))
                    {
                        var data = Clipboard.GetDataObject().GetData(typeof(ObjectClipboardData)) as ObjectClipboardData;
                        if (data == null)
                            _editor.SendMessage("Clipboard contains no object data.", PopupType.Error);
                        else
                            _editor.Action = new EditorActionPlace(false, (level, room) => data.MergeGetSingleObject(_editor));
                    }
                    else if (_editor.SelectedSectors.Valid && Clipboard.ContainsData(typeof(SectorsClipboardData).FullName))
                    {
                        var data = Clipboard.GetDataObject().GetData(typeof(SectorsClipboardData)) as SectorsClipboardData;
                        if (data == null)
                            _editor.SendMessage("Clipboard contains no sector data.", PopupType.Error);
                        else
                            EditorActions.TryPasteSectors(data, _editorWindow);
                    }
                }
                else
                {
                    var roomClipboardData = Clipboard.GetDataObject().GetData(typeof(RoomClipboardData)) as RoomClipboardData;
                    if (roomClipboardData == null)
                        _editor.SendMessage("Clipboard contains no room data.", PopupType.Error);
                    else
                        roomClipboardData.MergeInto(_editor, new VectorInt2());
                }
            });

            AddCommand("StampObject", "Stamp object", CommandType.Objects, delegate ()
            {
                EditorActions.TryStampObject(_editor.SelectedObject, _editorWindow);
            });

            AddCommand("Delete", "Delete", CommandType.Edit, delegate ()
            {
                if (_editor.Mode == EditorMode.Map2D)
                    if (_editor.SelectedObject != null && (_editor.SelectedObject is PortalInstance || _editor.SelectedObject is TriggerInstance))
                        EditorActions.DeleteObjectWithWarning(_editor.SelectedObject, _editorWindow);
                    else
                        EditorActions.DeleteRooms(_editor.SelectedRooms, _editorWindow);
                else
                {
                    if (_editor.SelectedObject != null)
                        EditorActions.DeleteObjectWithWarning(_editor.SelectedObject, _editorWindow);
                    else
                        _editor.SendMessage("No object selected. Nothing to delete.", PopupType.Warning);
                }
            });

            AddCommand("SelectAll", "Select all", CommandType.Edit, delegate ()
            {
                if (_editor.Mode == EditorMode.Map2D)
                    _editor.SelectRooms(_editor.Level.Rooms.Where(room => room != null));
                else
                    _editor.SelectedSectors = new SectorSelection { Area = _editor.SelectedRoom.LocalArea };
            });

            AddCommand("BookmarkObject", "Bookmark object", CommandType.Objects, delegate ()
            {
                EditorActions.BookmarkObject(_editor.SelectedObject);
            });

            AddCommand("SelectBookmarkedObject", "Select bookmarked object", CommandType.Objects, delegate ()
            {
                _editor.SelectedObject = _editor.BookmarkedObject;
            });

            AddCommand("Search", "Search...", CommandType.Edit, delegate ()
            {
                Forms.FormSearch searchForm = new Forms.FormSearch(_editor);
                searchForm.Show(_editorWindow); // Also disposes: https://social.msdn.microsoft.com/Forums/windows/en-US/5cbf16a9-1721-4861-b7c0-ea20cf328d48/any-difference-between-formclose-and-formdispose?forum=winformsdesigner
            });

            AddCommand("DeleteRooms", "Delete", CommandType.Rooms, delegate ()
            {
                if (_editor.Mode == EditorMode.Map2D)
                    EditorActions.DeleteRooms(_editor.SelectedRooms, _editorWindow);
                else
                    EditorActions.DeleteRooms(new[] { _editor.SelectedRoom }, _editorWindow);
            });

            AddCommand("DuplicateRooms", "Duplicate rooms", CommandType.Rooms, delegate ()
            {
                EditorActions.DuplicateRooms(_editorWindow);
            });

            AddCommand("SelectConnectedRooms", "Select connected rooms", CommandType.Rooms, delegate ()
            {
                EditorActions.SelectConnectedRooms();
            });

            AddCommand("RotateRoomsClockwise", "Rotate rooms clockwise", CommandType.Rooms, delegate ()
            {
                EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = -1 }, _editorWindow);
            });

            AddCommand("RotateRoomsCounterClockwise", "Rotate rooms counterclockwise", CommandType.Rooms, delegate ()
            {
                EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = 1 }, _editorWindow);
            });

            AddCommand("MirrorRoomsX", "Mirror rooms on X axis", CommandType.Rooms, delegate ()
            {
                EditorActions.TransformRooms(new RectTransformation { MirrorX = true }, _editorWindow);
            });

            AddCommand("MirrorRoomsZ", "Mirror rooms on Z axis", CommandType.Rooms, delegate ()
            {
                EditorActions.TransformRooms(new RectTransformation { MirrorX = true, QuadrantRotation = 2 }, _editorWindow);
            });

            AddCommand("SplitRoom", "Split room", CommandType.Rooms, delegate ()
            {
                EditorActions.SplitRoom(_editorWindow);
            });

            AddCommand("CropRoom", "Crop room", CommandType.Rooms, delegate ()
            {
                EditorActions.CropRoom(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editorWindow);
            });

            AddCommand("NewRoomUp", "New room up", CommandType.Rooms, delegate ()
            {
                EditorActions.CreateRoomAboveOrBelow(_editor.SelectedRoom, room => room.GetHighestCorner(), 12);
            });

            AddCommand("NewRoomDown", "New room down", CommandType.Rooms, delegate ()
            {
                EditorActions.CreateRoomAboveOrBelow(_editor.SelectedRoom, room => room.GetLowestCorner() - 12, 12);
            });

            AddCommand("ExportRooms", "Export rooms...", CommandType.Rooms, delegate ()
            {
                EditorActions.ExportRooms(_editor.SelectedRooms, _editorWindow);
            });

            AddCommand("ImportRooms", "Import rooms...", CommandType.Rooms, delegate ()
            {
                EditorActions.ImportRooms(_editorWindow);
            });

            AddCommand("ApplyAmbientLightToAllRooms", "Apply current ambient light to all rooms", CommandType.Rooms, delegate ()
            {
                if (DarkMessageBox.Show(_editorWindow, "Do you really want to apply the ambient light of the current room to all rooms?",
                                       "Apply ambient light", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    EditorActions.ApplyCurrentAmbientLightToAllRooms();
                    _editor.SendMessage("Ambient light was applied to all rooms.", PopupType.Info);
                }
            });

            AddCommand("AddWad", "Add wad...", CommandType.Objects, delegate ()
            {
                EditorActions.AddWad(_editorWindow);
            });

            AddCommand("RemoveWads", "Remove all wads", CommandType.Objects, delegate ()
            {
                EditorActions.RemoveWads(_editorWindow);
            });

            AddCommand("ReloadWads", "Reload all wads", CommandType.Objects, delegate ()
            {
                EditorActions.ReloadWads(_editorWindow);
            });

            AddCommand("AddCamera", "Add camera", CommandType.Objects, delegate ()
            {
                _editor.Action = new EditorActionPlace(false, (l, r) => new CameraInstance());
            });

            AddCommand("AddFlybyCamera", "Add flyby camera", CommandType.Objects, delegate ()
            {
                _editor.Action = new EditorActionPlace(false, (l, r) => new FlybyCameraInstance());
            });

            AddCommand("AddSink", "Add sink", CommandType.Objects, delegate ()
            {
                _editor.Action = new EditorActionPlace(false, (l, r) => new SinkInstance());
            });

            AddCommand("AddSoundSource", "Add sound source", CommandType.Objects, delegate ()
            {
                _editor.Action = new EditorActionPlace(false, (l, r) => new SoundSourceInstance());
            });

            AddCommand("AddImportedGeometry", "Add imported geometry", CommandType.Objects, delegate ()
            {
                _editor.Action = new EditorActionPlace(false, (l, r) => new ImportedGeometryInstance());
            });

            AddCommand("LocateItem", "Locate item", CommandType.Objects, delegate ()
            {
                EditorActions.FindItem();
            });

            AddCommand("MoveLara", "Move Lara here", CommandType.Objects, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;

                EditorActions.MoveLara(_editorWindow, _editor.SelectedSectors.Start);
            });

            AddCommand("AddTexture", "Add texture...", CommandType.Textures, delegate ()
            {
                EditorActions.AddTexture(_editorWindow);
            });

            AddCommand("RemoveTextures", "Remove all textures", CommandType.Textures, delegate ()
            {
                EditorActions.RemoveTextures(_editorWindow);
            });

            AddCommand("UnloadTextures", "Unload all textures", CommandType.Textures, delegate ()
            {
                EditorActions.UnloadTextures(_editorWindow);
            });

            AddCommand("ReloadTextures", "Reload all textures", CommandType.Textures, delegate ()
            {
                foreach (var texture in _editor.Level.Settings.Textures)
                    texture.Reload(_editor.Level.Settings);
                _editor.LoadedTexturesChange();
            });

            AddCommand("ConvertTexturesToPNG", "Convert all textures to PNG", CommandType.Textures, delegate ()
            {
                if (_editor.Level == null || _editor.Level.Settings.Textures.Count == 0)
                {
                    _editor.SendMessage("No texture loaded. Nothing to convert.", PopupType.Error);
                    return;
                }

                foreach (LevelTexture texture in _editor.Level.Settings.Textures)
                {
                    if (texture.LoadException != null)
                    {
                        _editor.SendMessage("The texture that should be converted to *.png could not be loaded. " + texture.LoadException?.Message, PopupType.Error);
                        return;
                    }

                    string currentTexturePath = _editor.Level.Settings.MakeAbsolute(texture.Path);
                    string pngFilePath = Path.Combine(Path.GetDirectoryName(currentTexturePath), Path.GetFileNameWithoutExtension(currentTexturePath) + ".png");

                    if (File.Exists(pngFilePath))
                    {
                        if (DarkMessageBox.Show(_editorWindow,
                                "There is already a file at \"" + pngFilePath + "\". Continue and overwrite the file?",
                                "File already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                            return;
                    }
                    texture.Image.Save(pngFilePath);

                    _editor.SendMessage("TGA texture map was converted to PNG without errors and saved at \"" + pngFilePath + "\".", PopupType.Info);
                    texture.SetPath(_editor.Level.Settings, pngFilePath);
                }
                _editor.LoadedTexturesChange();
            });

            AddCommand("RemapTexture", "Remap texture...", CommandType.Textures, delegate ()
            {
                using (var form = new Forms.FormTextureRemap(_editor))
                    form.ShowDialog(_editorWindow);
            });

            AddCommand("TextureFloor", "Texture floor", CommandType.Textures, delegate ()
            {
                EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Floor);
            });

            AddCommand("TextureCeiling", "Texture ceiling", CommandType.Textures, delegate ()
            {
                EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Ceiling);
            });

            AddCommand("TextureWalls", "Texture walls", CommandType.Textures, delegate ()
            {
                EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Wall);
            });

            AddCommand("EditAnimationRanges", "Edit animation ranges...", CommandType.Textures, delegate ()
            {
                using (Forms.FormAnimatedTextures form = new Forms.FormAnimatedTextures(_editor, null))
                    form.ShowDialog(_editorWindow);
            });

            AddCommand("SmoothRandomFloorUp", "Smooth random floor up", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.SmoothRandom(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1, BlockVertical.Floor);
            });

            AddCommand("SmoothRandomFloorDown", "Smooth random floor down", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.SmoothRandom(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1, BlockVertical.Floor);
            });

            AddCommand("SmoothRandomCeilingUp", "Smooth random ceiling up", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.SmoothRandom(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1, BlockVertical.Ceiling);
            });

            AddCommand("SmoothRandomCeilingDown", "Smooth random ceiling down", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.SmoothRandom(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1, BlockVertical.Ceiling);
            });

            AddCommand("SharpRandomFloorUp", "Sharp random floor up", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.SharpRandom(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1, BlockVertical.Floor);
            });

            AddCommand("SharpRandomFloorDown", "Sharp random floor down", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.SharpRandom(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1, BlockVertical.Floor);
            });

            AddCommand("SharpRandomCeilingUp", "Sharp random ceiling up", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.SharpRandom(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1, BlockVertical.Ceiling);
            });

            AddCommand("SharpRandomCeilingDown", "Sharp random ceiling down", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.SharpRandom(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1, BlockVertical.Ceiling);
            });

            AddCommand("FlattenFloor", "Flatten floor", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.Flatten(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockVertical.Floor);
            });

            AddCommand("FlattenCeiling", "Flatten ceiling", CommandType.Geometry, delegate ()
            {
                if (!EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    return;
                EditorActions.Flatten(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockVertical.Ceiling);
            });

            AddCommand("GridWallsIn3", "Grid walls in 3", CommandType.Geometry, delegate ()
            {
                if (EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    EditorActions.GridWalls(_editor.SelectedRoom, _editor.SelectedSectors.Area);
            });

            AddCommand("GridWallsIn5", "Grid walls in 5", CommandType.Geometry, delegate ()
            {
                if (EditorActions.CheckForRoomAndBlockSelection(_editorWindow))
                    EditorActions.GridWalls(_editor.SelectedRoom, _editor.SelectedSectors.Area, true);
            });

            AddCommand("EditLevelSettings", "Level settings...", CommandType.Settings, delegate ()
            {
                using (Forms.FormLevelSettings form = new Forms.FormLevelSettings(_editor))
                    form.ShowDialog(_editorWindow);
            });

            AddCommand("StartWadTool", "Start Wad Tool...", CommandType.Settings, delegate ()
            {
                try
                {
                    Process.Start("WadTool.exe");
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Error while starting Wad Tool.");
                    _editor.SendMessage("Error while starting Wad Tool.", PopupType.Error);
                }
            });

            AddCommand("StartSoundTool", "Start Sound Tool...", CommandType.Settings, delegate ()
            {
                try
                {
                    Process.Start("SoundTool.exe");
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Error while starting Sound Tool.");
                    _editor.SendMessage("Error while starting Sound Tool.", PopupType.Error);
                }
            });

            AddCommand("EditKeyboardLayout", "Edit keyboard layout...", CommandType.Settings, delegate ()
            {
                using (var f = new FormKeyboardLayout(_editor))
                    f.ShowDialog();
            });

            AddCommand("SwitchTool1", "Switch tool 1", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(0);
            });

            AddCommand("SwitchTool2", "Switch tool 2", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(1);
            });

            AddCommand("SwitchTool3", "Switch tool 3", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(2);
            });

            AddCommand("SwitchTool4", "Switch tool 4", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(3);
            });

            AddCommand("SwitchTool5", "Switch tool 5", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(4);
            });

            AddCommand("SwitchTool6", "Switch tool 6", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(5);
            });

            AddCommand("SwitchTool7", "Switch tool 7", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(6);
            });

            AddCommand("SwitchTool8", "Switch tool 8", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(7);
            });

            AddCommand("SwitchTool9", "Switch tool 9", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(8);
            });

            AddCommand("SwitchTool10", "Switch tool 10", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(9);
            });

            AddCommand("SwitchTool11", "Switch tool 11", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(10);
            });

            AddCommand("SwitchTool12", "Switch tool 12", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(11);
            });

            AddCommand("SwitchTool13", "Switch tool 13", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(12);
            });

            AddCommand("SwitchTool14", "Switch tool 14", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(13);
            });

            AddCommand("SwitchTool15", "SwitchTool15", CommandType.General, delegate ()
            {
                EditorActions.SwitchToolOrdered(14);
            });

            AddCommand("QuitEditor", "Quit editor", CommandType.General, delegate ()
            {
                _editor.Quit();
            });

            Commands = Commands.OrderBy(o => o.Type).ToList();
        }

        public static string KeysToString(Keys keys)
        {
            string result = "";

            result += keys.HasFlag(Keys.Control) ? "Ctrl" + "+" : ""; // Instead of 'Control'
            result += keys.HasFlag(Keys.Alt) ? ((keys & Keys.Alt)).ToString() + "+" : "";
            result += keys.HasFlag(Keys.Shift) ? ((keys & Keys.Shift)).ToString() + "+" : "";

            var realKey = (keys & ~(Keys.Alt | Keys.Shift | Keys.Control));
            if (realKey != Keys.None)
            {
                // Microsoft has weird ToString mappings for certain characters. Here is a switch to fix it.
                // Currently only PageUp is fixed.
                switch(realKey)
                {
                    case Keys.PageDown:
                        result += "PageDown";
                        break;
                    default:
                        result += realKey.ToString();
                        break;
                }
            }
            return result;
        }
    }
}
