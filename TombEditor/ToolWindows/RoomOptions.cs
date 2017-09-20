using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Docking;
using TombEditor.Geometry;
using SharpDX;

namespace TombEditor.ToolWindows
{
    public partial class RoomOptions : DarkToolWindow
    {
        private Editor _editor;

        public RoomOptions()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update the room list
            if (obj is Editor.RoomListChangedEvent)
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
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.RoomPropertiesChangedEvent))
            {
                Room room = ((IEditorRoomChangedEvent)obj).Room;
                if (obj is Editor.SelectedRoomChangedEvent)
                    comboRoom.SelectedIndex = _editor.Level.Rooms.ReferenceIndexOf(room);

                // Update the state of other controls
                if (room.FlagQuickSand)
                    comboRoomType.SelectedIndex = 7;
                else if (room.FlagSnow)
                    comboRoomType.SelectedIndex = 6;
                else if (room.FlagRain)
                    comboRoomType.SelectedIndex = 5;
                else if (room.FlagWater)
                    comboRoomType.SelectedIndex = room.WaterLevel;
                else
                    comboRoomType.SelectedIndex = 0;

                panelRoomAmbientLight.BackColor = room.AmbientLight.ToWinFormsColor();

                comboMist.SelectedIndex = room.MistLevel;
                comboReflection.SelectedIndex = room.ReflectionLevel;
                comboReverberation.SelectedIndex = (int)room.Reverberation;

                cbFlagCold.Checked = room.FlagCold;
                cbFlagDamage.Checked = room.FlagDamage;
                cbFlagOutside.Checked = room.FlagOutside;
                cbHorizon.Checked = room.FlagHorizon;
                cbNoPathfinding.Checked = room.ExcludeFromPathFinding;
                
                comboFlipMap.Enabled = !(room.Flipped && (room.AlternateRoom == null));
                comboFlipMap.SelectedIndex = room.Flipped ? (room.AlternateGroup + 1) : 0;
            }
        }


        private void comboRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            Room selectedRoom = _editor.Level.Rooms[comboRoom.SelectedIndex];
            if (selectedRoom == null)
            {
                selectedRoom = new Room(_editor.Level, 20, 20, "Room " + comboRoom.SelectedIndex);
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

            // Delete flipped room
            if (comboFlipMap.SelectedIndex == 0 && room.Flipped)
            {
                EditorActions.AlternateRoomDisable(room);
                return;
            }

            // Change flipped map number, not much to do here
            if (comboFlipMap.SelectedIndex != 0 && room.Flipped)
            {
                if (room.AlternateGroup == (comboFlipMap.SelectedIndex - 1))
                    return;

                room.AlternateGroup = (short)(comboFlipMap.SelectedIndex - 1);
                _editor.RoomPropertiesChange(room);
                return;
            }

            // Create a new flipped room
            if (comboFlipMap.SelectedIndex != 0 && !room.Flipped)
            {
                EditorActions.AlternateRoomEnable(room, (short)(comboFlipMap.SelectedIndex - 1));
                return;
            }
        }

        private void cbFlagDamage_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagDamage = cbFlagDamage.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbFlagCold_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagCold = cbFlagCold.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbFlagOutside_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagOutside = cbFlagOutside.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbHorizon_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagHorizon = cbHorizon.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbNoPathfinding_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.ExcludeFromPathFinding = cbNoPathfinding.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboReverberation_SelectedIndexChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Reverberation = (Reverberation)(comboReverberation.SelectedIndex);
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboRoomType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboRoomType.SelectedIndex)
            {
                case 0:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 0;
                    break;

                case 1:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 1;
                    break;

                case 2:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 2;
                    break;

                case 3:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 3;
                    break;

                case 4:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 4;
                    break;

                case 5:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = true;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 0;
                    break;

                case 6:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = true;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 0;
                    break;

                case 7:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = true;
                    _editor.SelectedRoom.WaterLevel = 0;
                    break;
            }
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboReflection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboReflection.SelectedIndex == 0)
            {
                _editor.SelectedRoom.FlagReflection = false;
                _editor.SelectedRoom.ReflectionLevel = 0;
            }
            else
            {
                _editor.SelectedRoom.FlagReflection = true;
                _editor.SelectedRoom.ReflectionLevel = (short)comboReflection.SelectedIndex;
            }
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboMist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboReflection.SelectedIndex == 0)
            {
                _editor.SelectedRoom.FlagMist = false;
                _editor.SelectedRoom.MistLevel = 0;
            }
            else
            {
                _editor.SelectedRoom.FlagMist = true;
                _editor.SelectedRoom.MistLevel = (short)comboMist.SelectedIndex;
            }
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void panelRoomAmbientLight_Click(object sender, EventArgs e)
        {
            Room room = _editor.SelectedRoom;

            colorDialog.Color = room.AmbientLight.ToWinFormsColor();
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelRoomAmbientLight.BackColor = colorDialog.Color;

            _editor.SelectedRoom.AmbientLight = colorDialog.Color.ToFloatColor();
            _editor.SelectedRoom.UpdateCompletely();
            _editor.RoomPropertiesChange(room);
        }

        private void butCropRoom_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.CropRoom(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butSplitRoom_Click(object sender, EventArgs e)
        {
            EditorActions.SplitRoom();
        }

        private void butCopyRoom_Click(object sender, EventArgs e)
        {
            EditorActions.CopyRoom();
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
                _editor.RoomListChange();
            }
        }

        private void butDeleteRoom_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null)
                return;
            EditorActions.DeleteRoom(_editor.SelectedRoom);
        }

        private void butRoomUp_Click(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Position += new Vector3(0.0f, 1.0f, 0.0f);

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            foreach (var portal in _editor.SelectedRoom.Portals)
            {
                portal.AdjoiningRoom.BuildGeometry();
                portal.AdjoiningRoom.CalculateLightingForThisRoom();
                portal.AdjoiningRoom.UpdateBuffers();
            }
        }

        private void butRoomDown_Click(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Position += new Vector3(0.0f, -1.0f, 0.0f);

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            foreach (var portal in _editor.SelectedRoom.Portals)
            {
                portal.AdjoiningRoom.BuildGeometry();
                portal.AdjoiningRoom.CalculateLightingForThisRoom();
                portal.AdjoiningRoom.UpdateBuffers();
            }
        }
    }
}
