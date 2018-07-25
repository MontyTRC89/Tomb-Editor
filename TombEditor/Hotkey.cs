using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using TombLib.Utils;

namespace TombEditor
{
    public struct Hotkey : IEquatable<Hotkey>, IComparable<Hotkey>
    {
        public Keys Keys { get; set; }

        public Keys MainKey
        {
            get { return Keys & Keys.KeyCode; }
            set { Keys = (value & Keys.KeyCode) | (Keys & ~Keys.KeyCode); }
        }

        public override string ToString()
        {
            // We could use KeysConverter but it has some weirness.
            // For example it will

            string result = "";
            result += (Keys & Keys.Control) != Keys.None ? "Ctrl+" : "";
            result += (Keys & Keys.Shift) != Keys.None ? "Shift+" : "";
            result += (Keys & Keys.Alt) != Keys.None ? "Alt+" : "";

            // Microsoft has weird ToString mappings for certain characters. Here is a switch to fix it.
            // Currently only PageDown and numeric values are fixed.
            switch (MainKey)
            {
                case Keys.None:
                    break;
                case Keys.PageDown:
                    result += "PageDown";
                    break;
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                    result += MainKey.ToString().Substring(1); // Remove the 'D'
                    break;
                default:
                    result += MainKey.ToString();
                    break;
            }
            return result;
        }

        public static Hotkey FromString(string str)
        {
            Hotkey result = new Hotkey();
            string[] keyNames = str.Split(new char[] { '+', ' ', '-', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string keyName in keyNames)
            {
                if (keyName.Equals("Ctrl", StringComparison.InvariantCultureIgnoreCase) ||
                    keyName.Equals("Control", StringComparison.InvariantCultureIgnoreCase))
                    result.Keys |= Keys.Control;
                else if (keyName.Equals("Shift", StringComparison.InvariantCultureIgnoreCase))
                    result.Keys |= Keys.Shift;
                else if (keyName.Equals("Alt", StringComparison.InvariantCultureIgnoreCase))
                    result.Keys |= Keys.Alt;
                else if (keyName.Length == 1 && (ushort)keyName[0] >= (ushort)'0' && (ushort)keyName[0] <= (ushort)'9')
                    result.MainKey = (Keys)((uint)(Keys.D0) + ((ushort)keyName[0] - (ushort)'0'));
                else if (keyName.Equals("PageDown", StringComparison.InvariantCultureIgnoreCase))
                    result.MainKey = Keys.PageDown;
                else
                    result.MainKey = (Keys)Enum.Parse(typeof(Keys), keyName, true);
            }
            return result;
        }

        public bool Equals(Hotkey other) => Keys == other.Keys;
        public override bool Equals(object other) => other is Hotkey && Equals((Hotkey)other);
        public override int GetHashCode() => (int)MainKey;
        public static implicit operator Hotkey(Keys keys) => new Hotkey { Keys = keys };

        public int CompareTo(Hotkey other)
        {
            return ((uint)Keys).CompareTo((uint)other.Keys);
        }
    }

    public class HotkeySets : IXmlSerializable, ICloneable, IEnumerable<KeyValuePair<string, SortedSet<Hotkey>>>
    {
        public static List<Keys> ReservedCameraKeys = new List<Keys> { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.PageDown, Keys.PageUp };

        private readonly SortedList<string, SortedSet<Hotkey>> _list = new SortedList<string, SortedSet<Hotkey>>(StringComparer.InvariantCultureIgnoreCase);

        public HotkeySets()
        {
            // Generate entries for all commands
            // We also want them to be in the XML.
            foreach (CommandObj command in CommandHandler.Commands)
                _list.Add(command.Name, new SortedSet<Hotkey>());
        }

        private HotkeySets(HotkeySets other)
        {
            _list = new SortedList<string, SortedSet<Hotkey>>(
                other._list.DicSelect(e => new SortedSet<Hotkey>(e.Value)), StringComparer.InvariantCultureIgnoreCase);
        }

        public SortedSet<Hotkey> this[string key]
        {
            get { return _list[key]; }
            set { _list[key] = value; }
        }

        public SortedSet<Hotkey> this[CommandObj command]
        {
            get { return _list[command.Name]; }
            set { _list[command.Name] = value; }
        }

        public HotkeySets Clone() => new HotkeySets(this);
        object ICloneable.Clone() => Clone();

        public IEnumerator<KeyValuePair<string, SortedSet<Hotkey>>> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        XmlSchema IXmlSerializable.GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            string startNodeName = reader.Name;
            if (reader.IsEmptyElement)
                return;
            while (reader.Name != startNodeName || reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read();
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                string commandName = reader.GetAttribute("CommandName");
                string hotkeys = reader.GetAttribute("Hotkeys");

                if (hotkeys.Equals("UseDefault", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                if (!_list.ContainsKey(commandName))
                    continue;
                string unused;
                this[commandName] = ParseHotkeys(reader.GetAttribute("Hotkeys"), out unused);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteComment("An example hotkey would be 'Ctrl+C'. You can specify multiple hotkeys per command like this 'Shift+Ctrl+T, Alt+I'.");
            writer.WriteComment("You may combine any key with Ctrl, Shift or Alt, but other combinations are not allowed.");
            writer.WriteComment("For a comprehensive list of possible keys look here: https://msdn.microsoft.com/en-us/library/system.windows.forms.keys(v=vs.110).aspx ");
            HotkeySets @default = GenerateDefault();
            foreach (var commandAndHotkey in _list)
            {
                bool isDefault = commandAndHotkey.Value.SetEquals(@default[commandAndHotkey.Key]);
                writer.WriteStartElement("HotkeySet");
                writer.WriteAttributeString("CommandName", commandAndHotkey.Key);
                writer.WriteAttributeString("Hotkeys", isDefault ? "UseDefault" : string.Join(", ", commandAndHotkey.Value));
                writer.WriteEndElement();
            }
        }

        public static SortedSet<Hotkey> ParseHotkeys(string str, out string errorMessage)
        {
            SortedSet<Hotkey> hotkeys = new SortedSet<Hotkey>();
            string[] keyGroups = str.Split(',');
            foreach (string keyGroup in keyGroups)
                if (!string.IsNullOrWhiteSpace(keyGroup))
                    try
                    {
                        hotkeys.Add(Hotkey.FromString(keyGroup));
                    }
                    catch (Exception)
                    {
                        errorMessage = "Invalid key combination '" + keyGroup + "'";
                        return null;
                    }
            errorMessage = null;
            return hotkeys;
        }

        public static HotkeySets GenerateDefault() => GenerateDefault();
        public static HotkeySets GenerateDefault(KeyboardLayout layout)
        {
            Keys Q = Keys.Q;
            Keys A = Keys.A;
            Keys W = Keys.W;
            //Keys Y = Keys.Y;
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
                //Y = Keys.Z;
                Z = Keys.Y;
            }

            HotkeySets result = new HotkeySets();
            result["CancelAnyAction"] = new SortedSet<Hotkey> { Keys.Escape };
            result["Switch2DMode"] = new SortedSet<Hotkey> { Keys.F1 };
            result["SwitchGeometryMode"] = new SortedSet<Hotkey> { Keys.F2 };
            result["SwitchFaceEditMode"] = new SortedSet<Hotkey> { Keys.F3 };
            result["SwitchLightingMode"] = new SortedSet<Hotkey> { Keys.F4 };
            result["ResetCamera"] = new SortedSet<Hotkey> { Keys.F6 };
            result["AddTrigger"] = new SortedSet<Hotkey> { Keys.T };
            result["AddTriggerWithBookmark"] = new SortedSet<Hotkey> { Keys.T | Keys.Shift };
            result["AddPortal"] = new SortedSet<Hotkey> { Keys.P };
            result["EditObject"] = new SortedSet<Hotkey> { Keys.O };
            result["SetTextureBlendMode"] = new SortedSet<Hotkey> { Keys.NumPad1 | Keys.Shift, Keys.D1 | Keys.Shift };
            result["SetTextureDoubleSided"] = new SortedSet<Hotkey> { Keys.NumPad2 | Keys.Shift, Keys.D2 | Keys.Shift };
            result["SetTextureInvisible"] = new SortedSet<Hotkey> { Keys.NumPad3 | Keys.Shift, Keys.D3 | Keys.Shift };
            result["RotateObjectLeft"] = new SortedSet<Hotkey> { Keys.Left | Keys.Shift };
            result["RotateObjectRight"] = new SortedSet<Hotkey> { Keys.Right | Keys.Shift };
            result["RotateObjectUp"] = new SortedSet<Hotkey> { Keys.Up | Keys.Shift };
            result["RotateObjectDown"] = new SortedSet<Hotkey> { Keys.Down | Keys.Shift };
            result["MoveObjectLeft"] = new SortedSet<Hotkey> { Keys.Left | Keys.Control };
            result["MoveObjectRight"] = new SortedSet<Hotkey> { Keys.Right | Keys.Control };
            result["MoveObjectForward"] = new SortedSet<Hotkey> { Keys.Up | Keys.Control };
            result["MoveObjectBack"] = new SortedSet<Hotkey> { Keys.Down | Keys.Control };
            result["MoveObjectUp"] = new SortedSet<Hotkey> { Keys.PageUp | Keys.Control };
            result["MoveObjectDown"] = new SortedSet<Hotkey> { Keys.PageDown | Keys.Control };
            result["MoveRoomLeft"] = new SortedSet<Hotkey> { Keys.Left | Keys.Alt };
            result["MoveRoomRight"] = new SortedSet<Hotkey> { Keys.Right | Keys.Alt };
            result["MoveRoomForward"] = new SortedSet<Hotkey> { Keys.Up | Keys.Alt };
            result["MoveRoomBack"] = new SortedSet<Hotkey> { Keys.Down | Keys.Alt };
            result["MoveRoomUp"] = new SortedSet<Hotkey> { Keys.PageUp | Keys.Alt };
            result["MoveRoomDown"] = new SortedSet<Hotkey> { Keys.PageDown | Keys.Alt };
            result["RaiseQA1Click"] = new SortedSet<Hotkey> { Q };
            result["RaiseQA4Click"] = new SortedSet<Hotkey> { Q | Keys.Shift };
            result["LowerQA1Click"] = new SortedSet<Hotkey> { A };
            result["LowerQA4Click"] = new SortedSet<Hotkey> { A | Keys.Shift };
            result["RaiseQA1ClickSmooth"] = new SortedSet<Hotkey> { Q | Keys.Alt };
            result["RaiseQA4ClickSmooth"] = new SortedSet<Hotkey> { Q | Keys.Alt | Keys.Shift };
            result["LowerQA1ClickSmooth"] = new SortedSet<Hotkey> { A | Keys.Alt };
            result["LowerQA4ClickSmooth"] = new SortedSet<Hotkey> { A | Keys.Alt | Keys.Shift };
            result["RaiseWS1Click"] = new SortedSet<Hotkey> { W };
            result["RaiseWS4Click"] = new SortedSet<Hotkey> { W | Keys.Shift };
            result["LowerWS1Click"] = new SortedSet<Hotkey> { Keys.S };
            result["LowerWS4Click"] = new SortedSet<Hotkey> { Keys.S | Keys.Shift };
            result["RaiseWS1ClickSmooth"] = new SortedSet<Hotkey> { W | Keys.Alt };
            result["RaiseWS4ClickSmooth"] = new SortedSet<Hotkey> { W | Keys.Alt | Keys.Shift };
            result["LowerWS1ClickSmooth"] = new SortedSet<Hotkey> { Keys.S | Keys.Alt };
            result["LowerWS4ClickSmooth"] = new SortedSet<Hotkey> { Keys.S | Keys.Alt | Keys.Shift };
            result["RaiseED1Click"] = new SortedSet<Hotkey> { Keys.E };
            result["RaiseED4Click"] = new SortedSet<Hotkey> { Keys.E | Keys.Shift };
            result["LowerED1Click"] = new SortedSet<Hotkey> { Keys.D };
            result["LowerED4Click"] = new SortedSet<Hotkey> { Keys.D | Keys.Shift };
            result["RaiseED1ClickSmooth"] = new SortedSet<Hotkey> { Keys.E | Keys.Alt };
            result["RaiseED4ClickSmooth"] = new SortedSet<Hotkey> { Keys.E | Keys.Alt | Keys.Shift };
            result["LowerED1ClickSmooth"] = new SortedSet<Hotkey> { Keys.D | Keys.Alt };
            result["LowerED4ClickSmooth"] = new SortedSet<Hotkey> { Keys.D | Keys.Alt | Keys.Shift };
            result["RaiseRF1Click"] = new SortedSet<Hotkey> { Keys.R };
            result["RaiseRF4Click"] = new SortedSet<Hotkey> { Keys.R | Keys.Shift };
            result["LowerRF1Click"] = new SortedSet<Hotkey> { Keys.F };
            result["LowerRF4Click"] = new SortedSet<Hotkey> { Keys.F | Keys.Shift };
            result["RaiseRF1ClickSmooth"] = new SortedSet<Hotkey> { Keys.R | Keys.Alt };
            result["RaiseRF4ClickSmooth"] = new SortedSet<Hotkey> { Keys.R | Keys.Alt | Keys.Shift };
            result["LowerRF1ClickSmooth"] = new SortedSet<Hotkey> { Keys.F | Keys.Alt };
            result["LowerRF4ClickSmooth"] = new SortedSet<Hotkey> { Keys.F | Keys.Alt | Keys.Shift };
            result["RaiseYH1Click"] = new SortedSet<Hotkey> { Keys.Y };
            result["RaiseYH4Click"] = new SortedSet<Hotkey> { Keys.Y | Keys.Shift };
            result["LowerYH1Click"] = new SortedSet<Hotkey> { Keys.H };
            result["LowerYH4Click"] = new SortedSet<Hotkey> { Keys.H | Keys.Shift };
            result["RaiseUJ1Click"] = new SortedSet<Hotkey> { Keys.U };
            result["RaiseUJ4Click"] = new SortedSet<Hotkey> { Keys.U | Keys.Shift };
            result["LowerUJ1Click"] = new SortedSet<Hotkey> { Keys.J };
            result["LowerUJ4Click"] = new SortedSet<Hotkey> { Keys.J | Keys.Shift };
            result["RotateObject5"] = new SortedSet<Hotkey> { Keys.R | Keys.Control };
            result["RotateObject45"] = new SortedSet<Hotkey> { Keys.R | Keys.Control | Keys.Shift };
            result["RotateTexture"] = new SortedSet<Hotkey> { Keys.OemMinus, Keys.Oemplus, Keys.Oem3, Keys.Oem5 }; // US keyboard key in documentation: OemMinus      US-keyboard key: Oem3       German-keyboard key: Oem5
            result["MirrorTexture"] = new SortedSet<Hotkey> { Keys.OemMinus | Keys.Shift, Keys.Oemplus | Keys.Shift, Keys.Oem3 | Keys.Shift, Keys.Oem5 | Keys.Shift };
            result["NewLevel"] = new SortedSet<Hotkey> { Keys.N | Keys.Control | Keys.Shift };
            result["OpenLevel"] = new SortedSet<Hotkey> { Keys.O | Keys.Control };
            result["SaveLevel"] = new SortedSet<Hotkey> { Keys.S | Keys.Control };
            result["SaveLevelAs"] = new SortedSet<Hotkey> { Keys.S | Keys.Control | Keys.Shift };
            result["BuildLevel"] = new SortedSet<Hotkey> { Keys.F5 | Keys.Shift };
            result["BuildAndPlay"] = new SortedSet<Hotkey> { Keys.F5 };
            result["Copy"] = new SortedSet<Hotkey> { Keys.C | Keys.Control };
            result["Paste"] = new SortedSet<Hotkey> { Keys.V | Keys.Control };
            result["StampObject"] = new SortedSet<Hotkey> { Keys.B | Keys.Control };
            result["Delete"] = new SortedSet<Hotkey> { Keys.Delete };
            result["SelectAll"] = new SortedSet<Hotkey> { A | Keys.Control };
            result["Search"] = new SortedSet<Hotkey> { Keys.F | Keys.Control };
            result["DeleteRooms"] = new SortedSet<Hotkey> { Keys.D | Keys.Control | Keys.Shift | Keys.Alt };
            result["DuplicateRooms"] = new SortedSet<Hotkey> { Keys.U | Keys.Control | Keys.Shift | Keys.Alt };
            result["SelectConnectedRooms"] = new SortedSet<Hotkey> { Keys.C | Keys.Control | Keys.Shift | Keys.Alt };
            result["RotateRoomsClockwise"] = new SortedSet<Hotkey> { Keys.F1 | Keys.Control };
            result["RotateRoomsCounterClockwise"] = new SortedSet<Hotkey> { Keys.F2 | Keys.Control };
            result["MirrorRoomsX"] = new SortedSet<Hotkey> { Keys.F3 | Keys.Control };
            result["MirrorRoomsZ"] = new SortedSet<Hotkey> { Keys.F4 | Keys.Control };
            result["SplitRoom"] = new SortedSet<Hotkey> { Keys.S | Keys.Control | Keys.Shift | Keys.Alt };
            result["CropRoom"] = new SortedSet<Hotkey> { Keys.O | Keys.Control | Keys.Shift | Keys.Alt };
            result["NewRoomUp"] = new SortedSet<Hotkey> { Keys.U | Keys.Control | Keys.Shift };
            result["NewRoomDown"] = new SortedSet<Hotkey> { Keys.D | Keys.Control | Keys.Shift };
            result["AddCamera"] = new SortedSet<Hotkey> { Keys.C | Keys.Alt };
            result["AddFlybyCamera"] = new SortedSet<Hotkey> { Keys.M | Keys.Alt };
            result["AddSink"] = new SortedSet<Hotkey> { Keys.K | Keys.Alt };
            result["AddSoundSource"] = new SortedSet<Hotkey> { Keys.X | Keys.Alt };
            result["AddImportedGeometry"] = new SortedSet<Hotkey> { Keys.I | Keys.Alt };
            result["MoveLara"] = new SortedSet<Hotkey> { Keys.M | Keys.Control };
            result["TextureFloor"] = new SortedSet<Hotkey> { Keys.T | Keys.Control | Keys.Alt };
            result["TextureCeiling"] = new SortedSet<Hotkey> { Keys.V | Keys.Control | Keys.Alt };
            result["TextureWalls"] = new SortedSet<Hotkey> { W | Keys.Control | Keys.Alt };
            result["FlattenFloor"] = new SortedSet<Hotkey> { Keys.E | Keys.Control | Keys.Alt };
            result["FlattenCeiling"] = new SortedSet<Hotkey> { Keys.F | Keys.Control | Keys.Alt };
            result["GridWallsIn3"] = new SortedSet<Hotkey> { Keys.D3 | Keys.Control };
            result["GridWallsIn5"] = new SortedSet<Hotkey> { Keys.D5 | Keys.Control };
            result["QuitEditor"] = new SortedSet<Hotkey> { Keys.F4 | Keys.Alt };
            result["RemapTexture"] = new SortedSet<Hotkey> { Keys.R | Keys.Shift | Keys.Alt };
            result["SmoothRandomFloorUp"] = new SortedSet<Hotkey> { Keys.A | Keys.Control | Keys.Shift };
            result["SmoothRandomFloorDown"] = new SortedSet<Hotkey> { Keys.B | Keys.Control | Keys.Shift };
            result["SmoothRandomCeilingUp"] = new SortedSet<Hotkey> { Keys.C | Keys.Control | Keys.Shift };
            result["SmoothRandomCeilingDown"] = new SortedSet<Hotkey> { Keys.D | Keys.Control | Keys.Shift };
            result["SharpRandomFloorUp"] = new SortedSet<Hotkey> { Keys.A | Keys.Control | Keys.Alt };
            result["SharpRandomFloorDown"] = new SortedSet<Hotkey> { Keys.B | Keys.Control | Keys.Alt };
            result["SharpRandomCeilingUp"] = new SortedSet<Hotkey> { Keys.C | Keys.Control | Keys.Alt };
            result["SharpRandomCeilingDown"] = new SortedSet<Hotkey> { Keys.D | Keys.Control | Keys.Alt };
            result["RelocateCamera"] = new SortedSet<Hotkey> { Keys.Alt | Z };
            return result;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;
    }
}
