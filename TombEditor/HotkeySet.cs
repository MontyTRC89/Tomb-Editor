using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor
{
    public class HotkeySet : ICloneable
    {
        public static List<Keys> ReservedCameraKeys = new List<Keys>
        { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.PageDown, Keys.PageUp };

        public string Name;
        public List<uint> Hotkeys;

        public HotkeySet Clone()
        {
            var result = new HotkeySet();
            result.Name = Name;
            result.Hotkeys = new List<uint>();

            foreach (var hotkey in Hotkeys)
                result.Hotkeys.Add(hotkey);

            return result;
        }
        object ICloneable.Clone() => Clone();

        public static HotkeySet[] GenerateDefaultHotkeys(KeyboardLayout layout)
        {
            Keys Q = Keys.Q;
            Keys A = Keys.A;
            Keys W = Keys.W;
            Keys Y = Keys.Y;
            Keys Z = Keys.Z;

            if (layout == KeyboardLayout.Azerty)
            {
                Q = Keys.A;
                W = Keys.Z;
                A = Keys.Q;
                Z = Keys.W;
            }
            else if (layout == KeyboardLayout.Qwertz)
            {
                Y = Keys.Z;
                Z = Keys.Y;
            }

            return new HotkeySet[]
            {
                new HotkeySet { Name = "CancelAnyAction", Hotkeys = new List<uint> { (uint)(Keys.Escape) } },
                new HotkeySet { Name = "Switch2DMode", Hotkeys = new List<uint> { (uint)(Keys.F1) } },
                new HotkeySet { Name = "SwitchGeometryMode", Hotkeys = new List<uint> { (uint)(Keys.F2) } },
                new HotkeySet { Name = "SwitchFaceEditMode", Hotkeys = new List<uint> { (uint)(Keys.F3) } },
                new HotkeySet { Name = "SwitchLightingMode", Hotkeys = new List<uint> { (uint)(Keys.F4) } },
                new HotkeySet { Name = "ResetCamera", Hotkeys = new List<uint> { (uint)(Keys.F6) } },
                new HotkeySet { Name = "AddTrigger", Hotkeys = new List<uint> { (uint)(Keys.T) } },
                new HotkeySet { Name = "AddPortal", Hotkeys = new List<uint> { (uint)(Keys.P) } },
                new HotkeySet { Name = "EditObject", Hotkeys = new List<uint> { (uint)(Keys.O) } },
                new HotkeySet { Name = "SetTextureBlendMode", Hotkeys = new List<uint> { (uint)(Keys.NumPad1 | Keys.Shift), (uint)(Keys.D1 | Keys.Shift) } },
                new HotkeySet { Name = "SetTextureDoubleSided", Hotkeys = new List<uint> { (uint)(Keys.NumPad2 | Keys.Shift), (uint)(Keys.D2 | Keys.Shift) } },
                new HotkeySet { Name = "SetTextureInvisible", Hotkeys = new List<uint> { (uint)(Keys.NumPad3 | Keys.Shift), (uint)(Keys.D3 | Keys.Shift) } },
                new HotkeySet { Name = "RotateObjectLeft", Hotkeys = new List<uint> { (uint)(Keys.Left | Keys.Shift) } },
                new HotkeySet { Name = "RotateObjectRight", Hotkeys = new List<uint> { (uint)(Keys.Right | Keys.Shift) } },
                new HotkeySet { Name = "RotateObjectUp", Hotkeys = new List<uint> { (uint)(Keys.Up | Keys.Shift) } },
                new HotkeySet { Name = "RotateObjectDown", Hotkeys = new List<uint> { (uint)(Keys.Down | Keys.Shift) } },
                new HotkeySet { Name = "MoveObjectLeft", Hotkeys = new List<uint> { (uint)(Keys.Left | Keys.Control) } },
                new HotkeySet { Name = "MoveObjectRight", Hotkeys = new List<uint> { (uint)(Keys.Right | Keys.Control) } },
                new HotkeySet { Name = "MoveObjectForward", Hotkeys = new List<uint> { (uint)(Keys.Up | Keys.Control) } },
                new HotkeySet { Name = "MoveObjectBack", Hotkeys = new List<uint> { (uint)(Keys.Down | Keys.Control) } },
                new HotkeySet { Name = "MoveObjectUp", Hotkeys = new List<uint> { (uint)(Keys.PageUp | Keys.Control) } },
                new HotkeySet { Name = "MoveObjectDown", Hotkeys = new List<uint> { (uint)(Keys.PageDown | Keys.Control) } },
                new HotkeySet { Name = "MoveRoomLeft", Hotkeys = new List<uint> { (uint)(Keys.Left | Keys.Alt) } },
                new HotkeySet { Name = "MoveRoomRight", Hotkeys = new List<uint> { (uint)(Keys.Right | Keys.Alt) } },
                new HotkeySet { Name = "MoveRoomForward", Hotkeys = new List<uint> { (uint)(Keys.Up | Keys.Alt) } },
                new HotkeySet { Name = "MoveRoomBack", Hotkeys = new List<uint> { (uint)(Keys.Down | Keys.Alt) } },
                new HotkeySet { Name = "MoveRoomUp", Hotkeys = new List<uint> { (uint)(Keys.PageUp | Keys.Alt) } },
                new HotkeySet { Name = "MoveRoomDown", Hotkeys = new List<uint> { (uint)(Keys.PageDown | Keys.Alt) } },
                new HotkeySet { Name = "RaiseQA1Click", Hotkeys = new List<uint> { (uint)(Q) } },
                new HotkeySet { Name = "RaiseQA4Click", Hotkeys = new List<uint> { (uint)(Q | Keys.Shift) } },
                new HotkeySet { Name = "LowerQA1Click", Hotkeys = new List<uint> { (uint)(A) } },
                new HotkeySet { Name = "LowerQA4Click", Hotkeys = new List<uint> { (uint)(A | Keys.Shift) } },
                new HotkeySet { Name = "RaiseQA1ClickSmooth", Hotkeys = new List<uint> { (uint)(Q | Keys.Alt) } },
                new HotkeySet { Name = "RaiseQA4ClickSmooth", Hotkeys = new List<uint> { (uint)(Q | Keys.Alt | Keys.Shift) } },
                new HotkeySet { Name = "LowerQA1ClickSmooth", Hotkeys = new List<uint> { (uint)(A | Keys.Alt) } },
                new HotkeySet { Name = "LowerQA4ClickSmooth", Hotkeys = new List<uint> { (uint)(A | Keys.Alt | Keys.Shift) } },
                new HotkeySet { Name = "RaiseWS1Click", Hotkeys = new List<uint> { (uint)(W) } },
                new HotkeySet { Name = "RaiseWS4Click", Hotkeys = new List<uint> { (uint)(W | Keys.Shift) } },
                new HotkeySet { Name = "LowerWS1Click", Hotkeys = new List<uint> { (uint)(Keys.S) } },
                new HotkeySet { Name = "LowerWS4Click", Hotkeys = new List<uint> { (uint)(Keys.S | Keys.Shift) } },
                new HotkeySet { Name = "RaiseWS1ClickSmooth", Hotkeys = new List<uint> { (uint)(W | Keys.Alt) } },
                new HotkeySet { Name = "RaiseWS4ClickSmooth", Hotkeys = new List<uint> { (uint)(W | Keys.Alt | Keys.Shift) } },
                new HotkeySet { Name = "LowerWS1ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.S | Keys.Alt) } },
                new HotkeySet { Name = "LowerWS4ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.S | Keys.Alt | Keys.Shift) } },
                new HotkeySet { Name = "RaiseED1Click", Hotkeys = new List<uint> { (uint)(Keys.E) } },
                new HotkeySet { Name = "RaiseED4Click", Hotkeys = new List<uint> { (uint)(Keys.E | Keys.Shift) } },
                new HotkeySet { Name = "LowerED1Click", Hotkeys = new List<uint> { (uint)(Keys.D) } },
                new HotkeySet { Name = "LowerED4Click", Hotkeys = new List<uint> { (uint)(Keys.D | Keys.Shift) } },
                new HotkeySet { Name = "RaiseED1ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.E | Keys.Alt) } },
                new HotkeySet { Name = "RaiseED4ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.E | Keys.Alt | Keys.Shift) } },
                new HotkeySet { Name = "LowerED1ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.D | Keys.Alt) } },
                new HotkeySet { Name = "LowerED4ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.D | Keys.Alt | Keys.Shift) } },
                new HotkeySet { Name = "RaiseRF1Click", Hotkeys = new List<uint> { (uint)(Keys.R) } },
                new HotkeySet { Name = "RaiseRF4Click", Hotkeys = new List<uint> { (uint)(Keys.R | Keys.Shift) } },
                new HotkeySet { Name = "LowerRF1Click", Hotkeys = new List<uint> { (uint)(Keys.F) } },
                new HotkeySet { Name = "LowerRF4Click", Hotkeys = new List<uint> { (uint)(Keys.F | Keys.Shift) } },
                new HotkeySet { Name = "RaiseRF1ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.R | Keys.Alt) } },
                new HotkeySet { Name = "RaiseRF4ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.R | Keys.Alt | Keys.Shift) } },
                new HotkeySet { Name = "LowerRF1ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.F | Keys.Alt) } },
                new HotkeySet { Name = "LowerRF4ClickSmooth", Hotkeys = new List<uint> { (uint)(Keys.F | Keys.Alt | Keys.Shift) } },
                new HotkeySet { Name = "RaiseYH1Click", Hotkeys = new List<uint> { (uint)(Y) } },
                new HotkeySet { Name = "RaiseYH4Click", Hotkeys = new List<uint> { (uint)(Y | Keys.Shift) } },
                new HotkeySet { Name = "LowerYH1Click", Hotkeys = new List<uint> { (uint)(Keys.H) } },
                new HotkeySet { Name = "LowerYH4Click", Hotkeys = new List<uint> { (uint)(Keys.H | Keys.Shift) } },
                new HotkeySet { Name = "RaiseUJ1Click", Hotkeys = new List<uint> { (uint)(Keys.U) } },
                new HotkeySet { Name = "RaiseUJ4Click", Hotkeys = new List<uint> { (uint)(Keys.U | Keys.Shift) } },
                new HotkeySet { Name = "LowerUJ1Click", Hotkeys = new List<uint> { (uint)(Keys.J) } },
                new HotkeySet { Name = "LowerUJ4Click", Hotkeys = new List<uint> { (uint)(Keys.J | Keys.Shift) } },
                new HotkeySet { Name = "RotateObject5", Hotkeys = new List<uint> { (uint)(Keys.R | Keys.Control) } },
                new HotkeySet { Name = "RotateObject45", Hotkeys = new List<uint> { (uint)(Keys.R | Keys.Control | Keys.Shift) } },
                new HotkeySet { Name = "RotateTexture", Hotkeys = new List<uint> { (uint)(Keys.OemMinus), (uint)(Keys.Oemplus), (uint)(Keys.Oem3), (uint)(Keys.Oem5) } },
                new HotkeySet { Name = "MirrorTexture", Hotkeys = new List<uint> { (uint)(Keys.OemMinus | Keys.Shift), (uint)(Keys.Oemplus | Keys.Shift), (uint)(Keys.Oem3 | Keys.Shift), (uint)(Keys.Oem5 | Keys.Shift) } },
                new HotkeySet { Name = "NewLevel", Hotkeys = new List<uint> { (uint)(Keys.N | Keys.Control | Keys.Shift) } },
                new HotkeySet { Name = "OpenLevel", Hotkeys = new List<uint> { (uint)(Keys.O | Keys.Control) } },
                new HotkeySet { Name = "SaveLevel", Hotkeys = new List<uint> { (uint)(Keys.S | Keys.Control) } },
                new HotkeySet { Name = "SaveLevelAs", Hotkeys = new List<uint> { (uint)(Keys.S | Keys.Control | Keys.Shift) } },
                new HotkeySet { Name = "BuildLevel", Hotkeys = new List<uint> { (uint)(Keys.F5) } },
                new HotkeySet { Name = "BuildAndPlay", Hotkeys = new List<uint> { (uint)(Keys.F5 | Keys.Shift) } },
                new HotkeySet { Name = "Copy", Hotkeys = new List<uint> { (uint)(Keys.C | Keys.Control) } },
                new HotkeySet { Name = "Paste", Hotkeys = new List<uint> { (uint)(Keys.V | Keys.Control) } },
                new HotkeySet { Name = "StampObject", Hotkeys = new List<uint> { (uint)(Keys.B | Keys.Control) } },
                new HotkeySet { Name = "Delete", Hotkeys = new List<uint> { (uint)(Keys.Delete) } },
                new HotkeySet { Name = "SelectAll", Hotkeys = new List<uint> { (uint)(A | Keys.Control) } },
                new HotkeySet { Name = "Search", Hotkeys = new List<uint> { (uint)(Keys.F | Keys.Control) } },
                new HotkeySet { Name = "DeleteRooms", Hotkeys = new List<uint> { (uint)(Keys.D | Keys.Control | Keys.Shift | Keys.Alt) } },
                new HotkeySet { Name = "DuplicateRooms", Hotkeys = new List<uint> { (uint)(Keys.U | Keys.Control | Keys.Shift | Keys.Alt) } },
                new HotkeySet { Name = "SelectConnectedRooms", Hotkeys = new List<uint> { (uint)(Keys.C | Keys.Control | Keys.Shift | Keys.Alt) } },
                new HotkeySet { Name = "RotateRoomsClockwise", Hotkeys = new List<uint> { (uint)(Keys.F1 | Keys.Control) } },
                new HotkeySet { Name = "RotateRoomsCounterClockwise", Hotkeys = new List<uint> { (uint)(Keys.F2 | Keys.Control) } },
                new HotkeySet { Name = "MirrorRoomsX", Hotkeys = new List<uint> { (uint)(Keys.F3 | Keys.Control) } },
                new HotkeySet { Name = "MirrorRoomsZ", Hotkeys = new List<uint> { (uint)(Keys.F4 | Keys.Control) } },
                new HotkeySet { Name = "SplitRoom", Hotkeys = new List<uint> { (uint)(Keys.S | Keys.Control | Keys.Shift | Keys.Alt) } },
                new HotkeySet { Name = "CropRoom", Hotkeys = new List<uint> { (uint)(Keys.O | Keys.Control | Keys.Shift | Keys.Alt) } },
                new HotkeySet { Name = "NewRoomUp", Hotkeys = new List<uint> { (uint)(Keys.U | Keys.Control | Keys.Shift) } },
                new HotkeySet { Name = "NewRoomDown", Hotkeys = new List<uint> { (uint)(Keys.D | Keys.Control | Keys.Shift) } },
                new HotkeySet { Name = "AddCamera", Hotkeys = new List<uint> { (uint)(Keys.C | Keys.Alt) } },
                new HotkeySet { Name = "AddFlybyCamera", Hotkeys = new List<uint> { (uint)(Keys.M | Keys.Alt) } },
                new HotkeySet { Name = "AddSink", Hotkeys = new List<uint> { (uint)(Keys.K | Keys.Alt) } },
                new HotkeySet { Name = "AddSoundSource", Hotkeys = new List<uint> { (uint)(Keys.X | Keys.Alt) } },
                new HotkeySet { Name = "AddImportedGeometry", Hotkeys = new List<uint> { (uint)(Keys.I | Keys.Alt) } },
                new HotkeySet { Name = "MoveLara", Hotkeys = new List<uint> { (uint)(Keys.M | Keys.Control) } },
                new HotkeySet { Name = "TextureFloor", Hotkeys = new List<uint> { (uint)(Keys.T | Keys.Control | Keys.Alt) } },
                new HotkeySet { Name = "TextureCeiling", Hotkeys = new List<uint> { (uint)(Keys.V | Keys.Control | Keys.Alt) } },
                new HotkeySet { Name = "TextureWalls", Hotkeys = new List<uint> { (uint)(W | Keys.Control | Keys.Alt) } },
                new HotkeySet { Name = "FlattenFloor", Hotkeys = new List<uint> { (uint)(Keys.E | Keys.Control | Keys.Alt) } },
                new HotkeySet { Name = "FlattenCeiling", Hotkeys = new List<uint> { (uint)(Keys.F | Keys.Control | Keys.Alt) } },
                new HotkeySet { Name = "GridWallsIn3", Hotkeys = new List<uint> { (uint)(Keys.D3 | Keys.Control) } },
                new HotkeySet { Name = "GridWallsIn5", Hotkeys = new List<uint> { (uint)(Keys.D5 | Keys.Control) } },
                new HotkeySet { Name = "QuitEditor", Hotkeys = new List<uint> { (uint)(Keys.F4 | Keys.Alt) } }
            };
        }
    }

}
