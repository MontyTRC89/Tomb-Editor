﻿using DarkUI.Docking;
using System;
using System.Linq;
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

            // A hack to edit textbox size
            tbRoomTags.AutoSize = false;
            tbRoomTags.Height = 23;
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

            // Update taglist
            if (obj is Editor.LevelChangedEvent || obj is Editor.SelectedRoomChangedEvent)
            {
                tbRoomTags.AutocompleteWords.Clear();
                foreach (var room in (_editor.Level.Rooms))
                    if(room != null && room.ExistsInLevel)
                        tbRoomTags.AutocompleteWords.AddRange(room.Tags.Except(tbRoomTags.AutocompleteWords));
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

                comboPortalShade.SelectedIndex = (int)room.LightInterpolationMode;
                comboLightEffect.SelectedIndex = (int)room.LightEffect;
                numLightEffectStrength.Value = room.LightEffectStrength;

                comboReverberation.SelectedIndex = (int)room.Reverberation;

                cbFlagCold.Checked = room.FlagCold;
                cbFlagDamage.Checked = room.FlagDamage;
                cbFlagOutside.Checked = room.FlagOutside;
                cbHorizon.Checked = room.FlagHorizon;
                cbNoLensflare.Checked = room.FlagNoLensflare;
                cbNoPathfinding.Checked = room.FlagExcludeFromPathFinding;

                if (!tbRoomTags.ReadOnly) // Only update tags field if we're not in the process of editing
                {
                    if (room.Tags.Count > 0)
                        tbRoomTags.Text = string.Join(" ", room.Tags);
                    else
                        tbRoomTags.Text = "";
                }

                if (room.AlternateBaseRoom != null)
                {
                    butLocked.Enabled = false;
                    butLocked.Checked = room.AlternateBaseRoom.Locked;
                }
                else
                {
                    butLocked.Enabled = true;
                    butLocked.Checked = room.Locked;
                }

                comboFlipMap.SelectedIndex = room.Alternated ? room.AlternateGroup + 1 : 0;
            }

            // Disable version-specific controls
            if (obj is Editor.InitEvent ||
                //obj is Editor.GameVersionChangedEvent || // FIXME: UNCOMMENT WHEN MERGED WITH DEVELOP!!!!!!!!!!!!!!!!!
                obj is Editor.LevelChangedEvent)
            {
                bool isNGorT5M = _editor.Level.Settings.GameVersion >= TRVersion.Game.TRNG;
                bool isTR4or5  = _editor.Level.Settings.GameVersion >= TRVersion.Game.TR4;

                cbFlagCold.Enabled = isNGorT5M;
                cbFlagDamage.Enabled = isNGorT5M;
                cbNoLensflare.Enabled = isTR4or5;
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
                EditorActions.MakeNewRoom(comboRoom.SelectedIndex);
            else
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

            using (var colorDialog = new RealtimeColorDialog(
                _editor.Configuration.ColorDialog_Position.X,
                _editor.Configuration.ColorDialog_Position.Y, 
                c =>
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

                _editor.Configuration.ColorDialog_Position = colorDialog.Position;
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

        private void comboPortalShade_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (_editor.SelectedRoom.LightInterpolationMode == (RoomLightInterpolationMode)comboPortalShade.SelectedIndex)
                return;

            _editor.SelectedRoom.LightInterpolationMode = (RoomLightInterpolationMode)comboPortalShade.SelectedIndex;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void TbTags_TextChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null)
                return;

            tbRoomTags.ReadOnly = true; // Prevent textbox from internally recalling this event

            _editor.SelectedRoom.Tags = tbRoomTags.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            _editor.RoomPropertiesChange(_editor.SelectedRoom);

            tbRoomTags.ReadOnly = false; // Re-enable editing
        }
    }
}
