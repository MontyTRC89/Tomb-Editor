using DarkUI.Docking;
using System;
using System.Numerics;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class RoomOptions : DarkToolWindow
    {
        private readonly Editor _editor;

        private class InitEvent : IEditorEvent { }

        public RoomOptions()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            EditorEventRaised(new InitEvent());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update the room list
            if (obj is InitEvent || obj is Editor.RoomListChangedEvent)
            {
                // Adjust the amount of entries in the combo list
                while (comboRoom.Items.Count > _editor.Level.Rooms.GetLength(0))
                    comboRoom.Items.RemoveAt(comboRoom.Items.Count - 1);
                while (comboRoom.Items.Count < _editor.Level.Rooms.GetLength(0))
                    comboRoom.Items.Add("");

                // Update the room list
                for (int i = 0; i < _editor.Level.Rooms.GetLength(0); i++)
                    if (_editor.Level.Rooms[i] != null)
                        comboRoom.Items[i] = i + ": " + _editor.Level.Rooms[i].Name;
                    else
                        comboRoom.Items[i] = i + ": --- Empty room ---";
            }

            // Update the room property controls
            if (obj is InitEvent || obj is Editor.SelectedRoomChangedEvent || obj is Editor.LevelChangedEvent ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomPropertiesChangedEvent))
            {
                Room room = _editor.SelectedRoom;
                if (obj is InitEvent || obj is Editor.SelectedRoomChangedEvent)
                    comboRoom.SelectedIndex = _editor.Level.Rooms.ReferenceIndexOf(room);

                // Update the state of other controls
                if (room.QuickSandLevel > 0)
                    comboRoomType.SelectedIndex = 12 + room.QuickSandLevel;
                else if (room.SnowLevel > 0)
                    comboRoomType.SelectedIndex = 8 + room.SnowLevel;
                else if (room.RainLevel > 0)
                    comboRoomType.SelectedIndex = 4 + room.RainLevel;
                else
                    comboRoomType.SelectedIndex = room.WaterLevel;

                panelRoomAmbientLight.BackColor = (room.AmbientLight * new Vector3(0.5f, 0.5f, 0.5f)).ToWinFormsColor();

                comboMist.SelectedIndex = room.MistLevel;
                comboReflection.SelectedIndex = room.ReflectionLevel;
                comboReverberation.SelectedIndex = (int)room.Reverberation;

                cbFlagCold.Checked = room.FlagCold;
                cbFlagDamage.Checked = room.FlagDamage;
                cbFlagOutside.Checked = room.FlagOutside;
                cbHorizon.Checked = room.FlagHorizon;
                cbNoLensflare.Checked = room.FlagNoLensflare;
                cbNoPathfinding.Checked = room.FlagExcludeFromPathFinding;

                if (room.AlternateBaseRoom != null)
                {
                    butLocked.Enabled = false;
                    butLocked.BackColorUseGeneric = !room.AlternateBaseRoom.Locked;
                }
                else
                {
                    butLocked.Enabled = true;
                    butLocked.BackColorUseGeneric = !room.Locked;
                }

                comboFlipMap.SelectedIndex = room.Alternated ? room.AlternateGroup + 1 : 0;
            }
        }

        private void comboRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            Room selectedRoom = _editor.Level.Rooms[comboRoom.SelectedIndex];
            if (selectedRoom == null)
            {
                selectedRoom = new Room(_editor.Level, Room.DefaultRoomDimensions, Room.DefaultRoomDimensions,
                                        _editor.Level.Settings.DefaultAmbientLight,
                                        "Room " + comboRoom.SelectedIndex);
                _editor.Level.Rooms[comboRoom.SelectedIndex] = selectedRoom;
                _editor.RoomListChange();
            }
            _editor.SelectRoomAndResetCamera(selectedRoom);
        }

        private void comboFlipMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null)
                return;

            var room = _editor.SelectedRoom;
            short alternateGroupIndex = (short)(comboFlipMap.SelectedIndex - 1);

            if (room.Alternated)
            {
                if (alternateGroupIndex == -1)
                { // Delete flipped room
                    EditorActions.AlternateRoomDisableWithWarning(room, this);
                }
                else
                { // Change flipped map number, not much to do here
                    if (room.AlternateGroup != alternateGroupIndex &&
                        room.AlternateOpposite.AlternateGroup != alternateGroupIndex)
                    {

                        room.AlternateGroup = alternateGroupIndex;
                        room.AlternateOpposite.AlternateGroup = alternateGroupIndex;
                        _editor.RoomPropertiesChange(room);
                        _editor.RoomPropertiesChange(room.AlternateOpposite);
                    }
                }
            }
            else
            {
                if (alternateGroupIndex != -1)
                {// Create a new flipped room
                    EditorActions.AlternateRoomEnable(room, alternateGroupIndex);
                }
            }

            // Update combo box even if nothing changed internally
            // to correct invalid user input
            EditorEventRaised(new Editor.RoomPropertiesChangedEvent { Room = room });
        }

        private void cbFlagDamage_CheckedChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.FlagDamage == cbFlagDamage.Checked)
                return;

            _editor.SelectedRoom.FlagDamage = cbFlagDamage.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbFlagCold_CheckedChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.FlagCold == cbFlagCold.Checked)
                return;

            _editor.SelectedRoom.FlagCold = cbFlagCold.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbFlagOutside_CheckedChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.FlagOutside == cbFlagOutside.Checked)
                return;

            _editor.SelectedRoom.FlagOutside = cbFlagOutside.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbHorizon_CheckedChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.FlagHorizon == cbHorizon.Checked)
                return;

            _editor.SelectedRoom.FlagHorizon = cbHorizon.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbNoPathfinding_CheckedChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.FlagExcludeFromPathFinding == cbNoPathfinding.Checked)
                return;

            _editor.SelectedRoom.FlagExcludeFromPathFinding = cbNoPathfinding.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboReverberation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.Reverberation == (Reverberation)comboReverberation.SelectedIndex)
                return;

            _editor.SelectedRoom.Reverberation = (Reverberation)comboReverberation.SelectedIndex;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboRoomType_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte waterLevel, rainLevel, snowLevel, quicksandLevel;

            var i = (byte)comboRoomType.SelectedIndex;

            if (i == 0)
            {
                waterLevel = 0;
                rainLevel = 0;
                snowLevel = 0;
                quicksandLevel = 0;
            }
            else if (i >= 1 && i <= 4)
            {
                waterLevel = i;
                rainLevel = 0;
                snowLevel = 0;
                quicksandLevel = 0;
            }
            else if (i >= 5 && i <= 8)
            {
                waterLevel = 0;
                rainLevel = (byte)(i - 4);
                snowLevel = 0;
                quicksandLevel = 0;
            }
            else if (i >= 9 && i <= 12)
            {
                waterLevel = 0;
                rainLevel = 0;
                snowLevel = (byte)(i - 8);
                quicksandLevel = 0;
            }
            else
            {
                waterLevel = 0;
                rainLevel = 0;
                snowLevel = 0;
                quicksandLevel = (byte)(i - 12);
            }

            if (_editor.SelectedRoom.WaterLevel == waterLevel &&
                _editor.SelectedRoom.RainLevel == rainLevel &&
                _editor.SelectedRoom.SnowLevel == snowLevel &&
                _editor.SelectedRoom.QuickSandLevel == quicksandLevel)
                return;

            _editor.SelectedRoom.WaterLevel = waterLevel;
            _editor.SelectedRoom.RainLevel = rainLevel;
            _editor.SelectedRoom.SnowLevel = snowLevel;
            _editor.SelectedRoom.QuickSandLevel = quicksandLevel;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboReflection_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte reflectionLevel = unchecked((byte)comboReflection.SelectedIndex);
            if (_editor.SelectedRoom.ReflectionLevel == reflectionLevel)
                return;

            _editor.SelectedRoom.ReflectionLevel = reflectionLevel;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboMist_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte mistLevel = unchecked((byte)comboMist.SelectedIndex);
            if (_editor.SelectedRoom.MistLevel == mistLevel)
                return;

            _editor.SelectedRoom.MistLevel = mistLevel;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void panelRoomAmbientLight_Click(object sender, EventArgs e)
        {
            Room room = _editor.SelectedRoom;

            colorDialog.Color = (room.AmbientLight * 0.5f).ToWinFormsColor();
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelRoomAmbientLight.BackColor = colorDialog.Color;

            _editor.SelectedRoom.AmbientLight = colorDialog.Color.ToFloatColor() * 2.0f;
            _editor.SelectedRoom.BuildGeometry();
            _editor.RoomPropertiesChange(room);
        }

        private void butCropRoom_Click(object sender, EventArgs e)
        {
            EditorActions.CropRoom(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
        }

        private void butSplitRoom_Click(object sender, EventArgs e)
        {
            EditorActions.SplitRoom(this);
        }

        private void butEditRoomName_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox())
            {
                form.Title = "Edit room's name";
                form.Message = "Insert the name of this room:";
                form.Value = _editor.SelectedRoom.Name;

                if (form.ShowDialog(this) == DialogResult.Cancel)
                    return;

                _editor.SelectedRoom.Name = form.Value;
                _editor.RoomPropertiesChange(_editor.SelectedRoom);
                _editor.RoomListChange();
            }
        }

        private void butRoomUp_Click(object sender, EventArgs e)
        {
            EditorActions.MoveRooms(new VectorInt3(0, 1, 0), _editor.SelectedRoom.Versions);
        }

        private void butRoomDown_Click(object sender, EventArgs e)
        {
            EditorActions.MoveRooms(new VectorInt3(0, -1, 0), _editor.SelectedRoom.Versions);
        }

        private void cbNoLensflare_CheckedChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.FlagNoLensflare == cbNoLensflare.Checked)
                return;

            _editor.SelectedRoom.FlagNoLensflare = cbNoLensflare.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void butLocked_Click(object sender, EventArgs e)
        {
            butLocked.BackColorUseGeneric = !butLocked.BackColorUseGeneric;

            if (_editor.SelectedRoom.Locked == !butLocked.BackColorUseGeneric)
                return;

            _editor.SelectedRoom.Locked = !butLocked.BackColorUseGeneric;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }
    }
}
