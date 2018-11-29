using DarkUI.Docking;
using System;
using System.Numerics;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class RoomOptions : DarkToolWindow
    {
        private readonly Editor _editor;

        public RoomOptions()
        {
            InitializeComponent();
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            EditorEventRaised(new Editor.InitEvent());
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
            if (obj is Editor.InitEvent || obj is Editor.RoomListChangedEvent)
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
            if (obj is Editor.InitEvent || obj is Editor.SelectedRoomChangedEvent || obj is Editor.LevelChangedEvent ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomPropertiesChangedEvent))
            {
                Room room = _editor.SelectedRoom;
                if (obj is Editor.InitEvent || obj is Editor.SelectedRoomChangedEvent)
                    comboRoom.SelectedIndex = _editor.Level.Rooms.ReferenceIndexOf(room);

                // Update the state of other controls
                switch (room.Type)
                {
                    case RoomType.Normal:
                        comboRoomType.SelectedIndex = 0;
                        break;
                    case RoomType.Water:
                        comboRoomType.SelectedIndex = 1;
                        break;
                    case RoomType.Quicksand:
                        comboRoomType.SelectedIndex = 2;
                        break;
                    case RoomType.Rain:
                        comboRoomType.SelectedIndex = 3 + room.TypeStrength;
                        break;
                    case RoomType.Snow:
                        comboRoomType.SelectedIndex = 7 + room.TypeStrength;
                        break;
                }

                panelRoomAmbientLight.BackColor = (room.AmbientLight * new Vector3(0.5f, 0.5f, 0.5f)).ToWinFormsColor();

                comboLightEffect.SelectedIndex = (int)room.LightEffect;
                numLightEffectStrength.Value = room.LightEffectStrength;

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

            // Update tooltip texts
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
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
                _editor.UndoManager.PushRoomCreated(selectedRoom);
            }
            _editor.SelectRoom(selectedRoom);
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

        private void comboReverberation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.Reverberation == (Reverberation)comboReverberation.SelectedIndex)
                return;

            _editor.SelectedRoom.Reverberation = (Reverberation)comboReverberation.SelectedIndex;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void numLightEffectStrength_ValueChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.LightEffectStrength = (byte)numLightEffectStrength.Value;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void panelRoomAmbientLight_Click(object sender, EventArgs e)
        {
            Room room = _editor.SelectedRoom;

            using (var colorDialog = new RealtimeColorDialog(c =>
            {
                room.AmbientLight = c.ToFloat3Color() * 2.0f;
                _editor.SelectedRoom.BuildGeometry();
                _editor.RoomPropertiesChange(room);
            }, _editor.Configuration.UI_ColorScheme))
            {
                colorDialog.Color = (room.AmbientLight * 0.5f).ToWinFormsColor();
                var oldLightColor = colorDialog.Color;

                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                    colorDialog.Color = oldLightColor;

                panelRoomAmbientLight.BackColor = colorDialog.Color;
                room.AmbientLight = colorDialog.Color.ToFloat3Color() * 2.0f;
            }

            _editor.SelectedRoom.BuildGeometry();
            _editor.RoomPropertiesChange(room);
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(comboRoom);
            searchPopUp.Show(this);
        }

        private void comboRoomType_SelectedIndexChanged(object sender, EventArgs e)
        {
            RoomType newType;
            byte newStrength = 0;
            
            // Update the state of other controls
            switch (comboRoomType.SelectedIndex)
            {
                case 0:
                    newType = RoomType.Normal;
                    break;
                case 1:
                    newType = RoomType.Water;
                    break;
                case 2:
                    newType = RoomType.Quicksand;
                    break;
                default:
                    if (comboRoomType.SelectedIndex <= 6)
                    {
                        newType = RoomType.Rain;
                        newStrength = (byte)(comboRoomType.SelectedIndex - 3);
                    }
                    else
                    {
                        newType = RoomType.Snow;
                        newStrength = (byte)(comboRoomType.SelectedIndex - 7);
                    }
                    break;
            }

            if(_editor.SelectedRoom.Type != newType || _editor.SelectedRoom.TypeStrength != newStrength)
            {
                _editor.SelectedRoom.Type = newType;
                _editor.SelectedRoom.TypeStrength = newStrength;
                _editor.RoomPropertiesChange(_editor.SelectedRoom);
            }
        }

        private void comboLightEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom.LightEffect == (RoomLightEffect)comboLightEffect.SelectedIndex)
                return;

            _editor.SelectedRoom.LightEffect = (RoomLightEffect)comboLightEffect.SelectedIndex;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }
    }
}
