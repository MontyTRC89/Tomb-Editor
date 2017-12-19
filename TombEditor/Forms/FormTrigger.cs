using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;
using TombLib.NG;
using TombLib.Wad.Catalog;

namespace TombEditor
{
    public partial class FormTrigger : DarkForm
    {
        private Level _level;
        private TriggerInstance _trigger;
        private bool comboParameterBeingInitialized = false;
        private Editor _editor;
        private bool _loaded = false;
        private Action<ObjectInstance> _selectObject;
        private Action<Room> _selectRoom;
        private Room _room;
        private bool _isNg;

        public FormTrigger(Level level, TriggerInstance trigger, Action<ObjectInstance> selectObject,
                           Action<Room> selectRoom)
        {
            _level = level;
            _trigger = trigger;
            _editor = Editor.Instance;
            _selectRoom = selectRoom;
            _selectObject = selectObject;
            _room = _editor.SelectedRoom;
            _isNg = _level.Settings.GameVersion == GameVersion.TRNG;

            InitializeComponent();

            // Resize components to make all sizes consistent

            tbParameter.AutoSize = false;
            tbTimer.AutoSize = false;
            tbParameter.Height = 23;
            tbTimer.Height = 23;

            //if (!_isNg)
            //    this.Height = 220 + (this.Height - ClientSize.Height);
            //else
            //    this.Height = 240 + (this.Height - ClientSize.Height);
        }

        private void LoadNgFlipeffectTrigger()
        {
            var flipeffectList = NgCatalog.FlipEffectTrigger.GetListForComboBox();

            tbParameter.Visible = false;
            comboParameter.Visible = true;
            comboParameter.Items.Clear();
            comboParameter.Items.AddRange(flipeffectList);
            comboParameter.SelectedIndex = 0;
            OnParameterChanged();

            tbTimer.Visible = true;
            comboTimer.Visible = false;

            labelExtra.Visible = false;
            comboExtraParameter.Visible = false;
        }

        private void LoadNgTimerFieldTrigger()
        {
            var timerfieldList = NgCatalog.TimerFieldTrigger.GetListForComboBox();

            tbParameter.Visible = false;
            comboParameter.Visible = true;
            comboParameter.Items.Clear();
            comboParameter.Items.AddRange(timerfieldList);
            comboParameter.SelectedIndex = 0;
            OnParameterChanged();

            tbTimer.Visible = true;
            comboTimer.Visible = false;

            labelExtra.Visible = false;
            comboExtraParameter.Visible = false;
        }

        private void LoadNgActionTrigger()
        {
            var timerfieldList = NgCatalog.ActionTrigger.GetListForComboBox();

            tbTimer.Visible = false;
            comboTimer.Visible = true;
            comboTimer.Items.Clear();
            comboTimer.Items.AddRange(timerfieldList);
            comboTimer.SelectedIndex = 0;
            OnTimerChanged();

            comboParameter.Visible = true;
            tbParameter.Visible = false;
        }

        private void LoadNgConditionTrigger()
        {
            var conditionList = NgCatalog.ConditionTrigger.GetListForComboBox();

            tbTimer.Visible = false;
            comboTimer.Visible = true;
            comboTimer.Items.Clear();
            comboTimer.Items.AddRange(conditionList);
            comboTimer.SelectedIndex = 0;
            OnTimerChanged();

            comboParameter.Visible = true;
            tbParameter.Visible = false;
        }

        private void FindAndAddObjects<T>() where T : ObjectInstance
        {
            try
            {
                comboParameterBeingInitialized = true;

                // Save selected item
                var selectedItem = comboParameter.SelectedItem;
                comboParameter.SelectedItem = null;

                // Populate list with new items
                comboParameter.Items.Clear();
                foreach (Room room in _level.Rooms.Where(room => room != null))
                    foreach (var instance in room.Objects.OfType<T>())
                    {
                        comboParameter.Items.Add(instance);
                        if (_trigger.TargetObj == instance)
                            comboParameter.SelectedItem = instance;
                    }

                // Select old item if possible
                if ((selectedItem != null) && comboParameter.Items.Contains(selectedItem))
                    comboParameter.SelectedItem = selectedItem;
            }
            finally
            {
                comboParameterBeingInitialized = false;
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            // Check if all inputs are valid
            short parsedTimer = 0;
            if (tbTimer.Visible)
            {
                if (!short.TryParse(tbTimer.Text, out parsedTimer))
                {
                    DarkMessageBox.Show(this, "Invalid data in Timer field", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            short parsedTarget = 0;
            if (tbParameter.Visible)
            {
                if (!short.TryParse(tbParameter.Text, out parsedTarget))
                {
                    DarkMessageBox.Show(this, "Invalid data in Parameter field", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (comboParameter.Visible && comboParameter.SelectedIndex == -1)
            {
                DarkMessageBox.Show(this, "You have to select a value for the Parameter field", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboTimer.Visible && comboTimer.SelectedIndex == -1)
            {
                DarkMessageBox.Show(this, "You have to select a value for the Timer field", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboExtraParameter.Visible && comboExtraParameter.SelectedIndex == -1)
            {
                DarkMessageBox.Show(this, "You have to select a value for the Extra field", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Store some values like we have not NG triggers
            _trigger.TriggerType = (TriggerType)comboType.SelectedItem;
            _trigger.TargetType = (TriggerTargetType)comboTargetType.SelectedItem;
            _trigger.Timer = parsedTimer;
            byte codeBits = 0;
            codeBits |= (byte)(cbBit1.Checked ? (1 << 0) : 0);
            codeBits |= (byte)(cbBit2.Checked ? (1 << 1) : 0);
            codeBits |= (byte)(cbBit3.Checked ? (1 << 2) : 0);
            codeBits |= (byte)(cbBit4.Checked ? (1 << 3) : 0);
            codeBits |= (byte)(cbBit5.Checked ? (1 << 4) : 0);
            _trigger.CodeBits = codeBits;
            _trigger.OneShot = cbOneShot.Checked;

            // Now, if NG triggers, we must save them differently
            if (_trigger.TriggerType==TriggerType.ConditionNg && _isNg)
            {
                // NG condition trigger
                var conditionId = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                var conditionTrigger = NgCatalog.ConditionTrigger.MainList[conditionId];
                _trigger.TargetData = (short)(comboParameter.SelectedItem as NgTriggerKeyValuePair).Key;

                _trigger.Timer = (short)((comboTimer.SelectedItem as NgTriggerKeyValuePair).Key & 0xFF);
                if (conditionTrigger.HasExtraList)
                    _trigger.Timer |= (short)((comboExtraParameter.SelectedItem as NgTriggerKeyValuePair).Key << 8);
            }
            else if (_trigger.TargetType == TriggerTargetType.FlipEffect && _isNg)
            {
                // NG flipeffect trigger
                var flipeffectId = (comboParameter.SelectedItem as NgTriggerKeyValuePair).Key;
                var flipeffect = NgCatalog.FlipEffectTrigger.MainList[flipeffectId];
                _trigger.TargetData = (short)flipeffectId;

                if (flipeffect.HasTimerList && flipeffect.HasExtraList)
                {
                    var timer = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                    var extra = (comboExtraParameter.SelectedItem as NgTriggerKeyValuePair).Key;
                    _trigger.Timer = (short)(((extra & 0xFF) << 8) + (timer & 0xFF));
                }
                else
                {
                    var timer = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                    _trigger.Timer = (short)timer;
                }
            }
            else if (_trigger.TargetType == TriggerTargetType.TimerfieldNg && _isNg)
            {
                // NG timer trigger
                _trigger.TargetData = (short)(comboParameter.SelectedItem as NgTriggerKeyValuePair).Key;
                _trigger.Timer = short.Parse(tbTimer.Text);
            }
            else if (_trigger.TargetType == TriggerTargetType.ActionNg && _isNg)
            {
                // NG action trigger
                var action = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                var actionTrigger = NgCatalog.ActionTrigger.MainList[action];

                // Set value for object
                var selectedObject = comboParameter.SelectedItem as ObjectInstance;
                if (selectedObject == null)
                {
                    DarkMessageBox.Show(this, "You don't have in your project any object of the type that you have required", "Save trigger", MessageBoxIcon.Warning);
                    return;
                }

                _trigger.TargetObj = selectedObject;
                _trigger.TargetData = 0;

                var timer = (short)action;
                if (actionTrigger.HasExtraList)
                    timer |= (short)(((comboExtraParameter.SelectedItem as NgTriggerKeyValuePair).Key & 0xFF) << 8);
                _trigger.Timer = timer;
            }
            else if (TriggerInstance.UsesTargetObj(_trigger.TargetType))
            {
                // Non NG trigger that uses object parameters
                var selectedObject = comboParameter.SelectedItem as ObjectInstance;
                if (selectedObject == null)
                {
                    DarkMessageBox.Show(this, "You don't have in your project any object of the type that you have required", "Save trigger", MessageBoxIcon.Warning);
                    return;
                }

                _trigger.TargetObj = selectedObject;
                _trigger.TargetData = 0;
            }
            else
            {
                // Non NG trigger that uses numeric parameter
                _trigger.TargetObj = null;
                _trigger.TargetData = parsedTarget;
            }

            // Reset the selection to room where the trigger is
            _selectRoom(_room);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private object[] GetSpecialNgList(NgListKind kind)
        {
            var result = new List<object>();

            if (kind == NgListKind.MoveablesInLevel)
            {
                foreach (Room room in _level.Rooms.Where(room => room != null))
                    foreach (var instance in room.Objects.OfType<MoveableInstance>())
                    {
                        result.Add(instance);
                    }
            }
            else if (kind == NgListKind.StaticsInLevel)
            {
                foreach (Room room in _level.Rooms.Where(room => room != null))
                    foreach (var instance in room.Objects.OfType<StaticInstance>())
                    {
                        result.Add(instance);
                    }
            }
            else if (kind == NgListKind.CamerasInLevel)
            {
                foreach (Room room in _level.Rooms.Where(room => room != null))
                    foreach (var instance in room.Objects.OfType<CameraInstance>())
                    {
                        result.Add(instance);
                    }
            }
            else if (kind == NgListKind.SinksInLevel)
            {
                foreach (Room room in _level.Rooms.Where(room => room != null))
                    foreach (var instance in room.Objects.OfType<SinkInstance>())
                    {
                        result.Add(instance);
                    }
            }
            else if (kind == NgListKind.FlybyCamerasInLevel)
            {
                foreach (Room room in _level.Rooms.Where(room => room != null))
                    foreach (var instance in room.Objects.OfType<FlybyCameraInstance>())
                    {
                        result.Add(instance);
                    }
            }
            else if (kind == NgListKind.SoundEffectsA)
            {
                for (var i = 0; i < 256; i++)
                {
                    if (_editor.Level.Wad != null && _editor.Level.Wad.SoundInfo.ContainsKey((ushort)i))
                    {
                        var sound = _editor.Level.Wad.SoundInfo[(ushort)i];
                        result.Add(new NgTriggerKeyValuePair(i, i + ": " + sound.Name));
                    }
                    else
                        result.Add(new NgTriggerKeyValuePair(i, i + ": --- Not present ---"));
                }
            }
            else if (kind == NgListKind.SoundEffectsB)
            {
                var soundMapSize = (_editor.Level.Wad != null ? _editor.Level.Wad.SoundMapSize : TrCatalog.GetSoundMapSize(TombLib.Wad.TombRaiderVersion.TR4, false));
                for (var i = 256; i < soundMapSize; i++)
                {
                    if (_editor.Level.Wad != null && _editor.Level.Wad.SoundInfo.ContainsKey((ushort)i))
                    {
                        var sound = _editor.Level.Wad.SoundInfo[(ushort)i];
                        result.Add(new NgTriggerKeyValuePair(i, i + ": " + sound.Name));
                    }
                    else
                        result.Add(new NgTriggerKeyValuePair(i, i + ": --- Not present ---"));
                }
            }
            else if (kind == NgListKind.Rooms255)
            {
                int lastRoomIndex = 0;
                foreach (Room room in _level.Rooms.Where(room => room != null))
                {
                    result.Add(new NgTriggerKeyValuePair(lastRoomIndex, "[" + lastRoomIndex + "] " + room.Name));
                    lastRoomIndex++;
                }
            }
            else if (kind == NgListKind.Sfx1024)
            {
                var soundMapSize = (_editor.Level.Wad != null ?
                                    _editor.Level.Wad.SoundMapSize :
                                    TrCatalog.GetSoundMapSize(TombLib.Wad.TombRaiderVersion.TR4, false));
                soundMapSize = Math.Min(soundMapSize, 1024);
                for (var i = 0; i < soundMapSize; i++)
                {
                    if (_editor.Level.Wad != null && _editor.Level.Wad.SoundInfo.ContainsKey((ushort)i))
                    {
                        var sound = _editor.Level.Wad.SoundInfo[(ushort)i];
                        result.Add(new NgTriggerKeyValuePair(i, i + ": " + sound.Name));
                    }
                    else
                        result.Add(new NgTriggerKeyValuePair(i, i + ": --- Not present ---"));
                }
            }
            else if (kind == NgListKind.WadSlots)
            {
                if (_editor.Level.Wad != null)
                {
                    foreach (var moveable in _editor.Level.Wad.Moveables)
                        result.Add(moveable.Value);
                }
            }
            else if (kind == NgListKind.PcStringsList)
            {
                result = LoadStringsFromTxt("PCStrings");
            }
            else if (kind == NgListKind.PsxStringsList)
            {
                result = LoadStringsFromTxt("PSXStrings");
            }
            else if (kind == NgListKind.NgStringsList255)
            {
                result = LoadStringsFromTxt("ExtraNG", 256);
            }
            else if (kind == NgListKind.NgStringsAll)
            {
                result = LoadStringsFromTxt("ExtraNG");
            }
            else if (kind == NgListKind.StringsList255)
            {
                result = LoadStringsFromTxt("Strings", 256);
            }

            return result.ToArray();
        }

        private List<object> LoadStringsFromTxt(string block, int max = 1024)
        {
            var result = new List<object>();
            var path = _level.Settings.MakeAbsolute(_level.Settings.ScriptDirectory) + "\\english.txt";
            if (!File.Exists(path)) return result;

            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                var foundBlock = false;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Trim();
                    if (line.StartsWith("["))
                    {
                        line = line.Replace("[", "").Replace("]", "");
                        if (line == block)
                        {
                            foundBlock = true;
                            break;
                        }
                    }
                }

                // If block is not found, then exit
                if (!foundBlock) return result;

                // Read strings of block until end of stream or next block or limit reached
                var numStrings = 0;
                while (!reader.EndOfStream && numStrings <= max)
                {
                    var line = reader.ReadLine().Trim();
                    if (line.StartsWith("[")) break;
                    result.Add(new NgTriggerKeyValuePair(numStrings, line));
                    numStrings++;
                }
            }

            return result;
        }

        private void SetNgComboboxValue(int value, DarkComboBox combobox)
        {
            foreach (NgTriggerKeyValuePair item in combobox.Items)
                if (item.Key == value)
                {
                    combobox.SelectedItem = item;
                    return;
                }
        }

        private void OnTriggerTypeChanged()
        {
            var triggerType = (TriggerType)comboType.SelectedItem;
            var targetType = TriggerTargetType.Object;

            if (triggerType == TriggerType.ConditionNg)
            {
                if (targetType == TriggerTargetType.FlipEffect || targetType== TriggerTargetType.ActionNg ||
                    targetType == TriggerTargetType.ParameterNg)
                {
                    DarkMessageBox.Show(this, "You can't combine ConditionNg trigger with Flipeffect, ActionNg or ParameterNg",
                                        "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    comboTargetType.SelectedItem = TriggerTargetType.Object;
                }
                comboTargetType.Enabled = false;
                LoadNgConditionTrigger();
            }
            else
            {
                comboTargetType.Enabled = true;
            }

            if (_loaded) UpdateExportToTrigger();
        }

        private void OnTriggerTargetChanged()
        {
            // Get informations about current tigger
            var targetType = (TriggerTargetType)comboTargetType.SelectedItem;
            var triggerType = (TriggerType)comboType.SelectedItem;
            bool usesObject = TriggerInstance.UsesTargetObj(targetType);

            // This is the general case, NG triggers will override this in their functions
            tbParameter.Visible = !usesObject && triggerType != TriggerType.ConditionNg;
            comboParameter.Visible = usesObject || triggerType == TriggerType.ConditionNg;
            if (triggerType != TriggerType.ConditionNg)
            {
                comboTimer.Visible = false;
                tbTimer.Visible = true;
                labelExtra.Visible = false;
                comboExtraParameter.Visible = false;
            }

            switch (targetType)
            {
                case TriggerTargetType.Object:
                    FindAndAddObjects<MoveableInstance>();
                    break;
                case TriggerTargetType.Camera:
                    FindAndAddObjects<CameraInstance>();
                    break;
                case TriggerTargetType.Sink:
                    FindAndAddObjects<SinkInstance>();
                    break;
                case TriggerTargetType.Target:
                    // Actually it is possible to not only target Target objects, but all movables.
                    // This is also useful: It makes sense to target egg a trap or an enemy.
                    FindAndAddObjects<MoveableInstance>();
                    break;
                case TriggerTargetType.FlyByCamera:
                    FindAndAddObjects<FlybyCameraInstance>();
                    break;
                case TriggerTargetType.FlipEffect:
                    if (_level.Settings.GameVersion == GameVersion.TRNG)
                        LoadNgFlipeffectTrigger();
                    break;
                case TriggerTargetType.ActionNg:
                    if (_level.Settings.GameVersion == GameVersion.TRNG)
                        LoadNgActionTrigger();
                    break;
                case TriggerTargetType.TimerfieldNg:
                    if (_level.Settings.GameVersion == GameVersion.TRNG)
                        LoadNgTimerFieldTrigger();
                    break;
            }

            if (_loaded) UpdateExportToTrigger();
        }

        private void OnParameterChanged()
        {
            var targetType = (TriggerTargetType)comboTargetType.SelectedItem;

            if (targetType == TriggerTargetType.FlipEffect && _isNg)
            {
                // NG flipeffect trigger
                var flipeffect = (comboParameter.SelectedItem as NgTriggerKeyValuePair).Key;
                var selectedItem = NgCatalog.FlipEffectTrigger.MainList[flipeffect];

                if (selectedItem.HasTimerList)
                {
                    labelTimer.Visible = true;
                    tbTimer.Visible = false;
                    comboTimer.Visible = true;
                    comboTimer.Items.Clear();
                    comboTimer.Text = "";

                    if (selectedItem.TimerListKind == NgListKind.Fixed || selectedItem.TimerListKind == NgListKind.Unknown)
                        comboTimer.Items.AddRange(selectedItem.GetListForComboBox(NgParameterType.Timer));
                    else
                        comboTimer.Items.AddRange(GetSpecialNgList(selectedItem.TimerListKind));
                }
                else
                {
                    tbTimer.Visible = false;
                    labelTimer.Visible = false;
                    comboTimer.Visible = false;
                }

                if (selectedItem.HasExtraList)
                {
                    labelExtra.Visible = true;
                    comboExtraParameter.Visible = true;
                    comboExtraParameter.Items.Clear();
                    comboExtraParameter.Text = "";

                    if (selectedItem.ExtraListKind == NgListKind.Fixed || selectedItem.ExtraListKind == NgListKind.Unknown)
                        comboExtraParameter.Items.AddRange(selectedItem.GetListForComboBox(NgParameterType.Extra));
                    else
                        comboExtraParameter.Items.AddRange(GetSpecialNgList(selectedItem.ExtraListKind));
                }
                else
                {
                    labelExtra.Visible = false;
                    comboExtraParameter.Visible = false;
                }
            }

            if (_loaded) UpdateExportToTrigger();
        }

        private void OnTimerChanged()
        {
            var targetType = (TriggerTargetType)comboTargetType.SelectedItem;
            var triggerType = (TriggerType)comboType.SelectedItem;

            if (triggerType == TriggerType.ConditionNg)
            {
                var condition = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                var selectedItem = NgCatalog.ConditionTrigger.MainList[condition];

                // Condition trigger always has an object
                labelParameter.Visible = true;
                tbParameter.Visible = false;
                comboParameter.Visible = true;
                comboParameter.Items.Clear();
                comboParameter.Text = "";

                if (selectedItem.HasObjectList)
                {
                    if (selectedItem.ObjectListKind == NgListKind.Fixed || selectedItem.ObjectListKind == NgListKind.Unknown)
                    {
                        comboTargetType.SelectedItem = TriggerTargetType.ParameterNg;
                        comboParameter.Items.AddRange(selectedItem.GetListForComboBox(NgParameterType.Object));
                    }
                    else if (selectedItem.ObjectListKind == NgListKind.MoveablesInLevel)
                    {
                        comboTargetType.SelectedItem = TriggerTargetType.Object;
                        FindAndAddObjects<MoveableInstance>();
                    }
                    else if (selectedItem.ObjectListKind == NgListKind.StaticsInLevel)
                    {
                        comboTargetType.SelectedItem = TriggerTargetType.Object;
                        FindAndAddObjects<StaticInstance>();
                    }
                    else if (selectedItem.ObjectListKind == NgListKind.SinksInLevel)
                    {
                        comboTargetType.SelectedItem = TriggerTargetType.Object;
                        FindAndAddObjects<SinkInstance>();
                    }
                    else if (selectedItem.ObjectListKind == NgListKind.CamerasInLevel)
                    {
                        comboTargetType.SelectedItem = TriggerTargetType.Object;
                        FindAndAddObjects<CameraInstance>();
                    }
                    else if (selectedItem.ObjectListKind == NgListKind.FlybyCamerasInLevel)
                    {
                        comboTargetType.SelectedItem = TriggerTargetType.Object;
                        FindAndAddObjects<FlybyCameraInstance>();
                    }
                    else
                    {
                        comboTargetType.SelectedItem = TriggerTargetType.ParameterNg;
                        comboParameter.Items.AddRange(GetSpecialNgList(selectedItem.ObjectListKind));
                    }

                    // Select a default item
                    if (comboParameter.Items.Count != 0) comboParameter.SelectedIndex = 0;
                }
                else
                {
                    // TODO: need to check this. Special case: if empty object list, then populate the parameter combobox with moveables by default
                    FindAndAddObjects<MoveableInstance>();
                    comboTargetType.SelectedItem = TriggerTargetType.Object;
                }

                if (selectedItem.HasExtraList)
                {
                    labelExtra.Visible = true;
                    comboExtraParameter.Visible = true;
                    comboExtraParameter.Items.Clear();
                    comboExtraParameter.Text = "";

                    if (selectedItem.ExtraListKind == NgListKind.Fixed || selectedItem.ExtraListKind == NgListKind.Unknown)
                        comboExtraParameter.Items.AddRange(selectedItem.GetListForComboBox(NgParameterType.Extra));
                    else
                        comboExtraParameter.Items.AddRange(GetSpecialNgList(selectedItem.ExtraListKind));

                    // Select a default item
                    if (comboExtraParameter.Items.Count != 0) comboExtraParameter.SelectedIndex = 0;
                }
                else
                {
                    labelExtra.Visible = false;
                    comboExtraParameter.Visible = false;
                }
            }

            if (targetType == TriggerTargetType.ActionNg)
            {
                var action = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                var selectedItem = NgCatalog.ActionTrigger.MainList[action];

                // Action trigger always has an object
                labelParameter.Visible = true;
                tbParameter.Visible = false;
                comboParameter.Visible = true;
                comboParameter.Items.Clear();
                comboParameter.Text = "";

                //comboTargetType.SelectedItem = TriggerTargetType.Object;

                if (selectedItem.HasObjectList)
                {
                    if (selectedItem.ObjectListKind == NgListKind.Fixed || selectedItem.ObjectListKind == NgListKind.Unknown)
                        comboParameter.Items.AddRange(selectedItem.GetListForComboBox(NgParameterType.Object));
                    else if (selectedItem.ObjectListKind == NgListKind.MoveablesInLevel)
                        FindAndAddObjects<MoveableInstance>();
                    else if (selectedItem.ObjectListKind == NgListKind.StaticsInLevel)
                        FindAndAddObjects<StaticInstance>();
                    else if (selectedItem.ObjectListKind == NgListKind.SinksInLevel)
                        FindAndAddObjects<SinkInstance>();
                    else if (selectedItem.ObjectListKind == NgListKind.CamerasInLevel)
                        FindAndAddObjects<CameraInstance>();
                    else if (selectedItem.ObjectListKind == NgListKind.FlybyCamerasInLevel)
                        FindAndAddObjects<FlybyCameraInstance>();
                    else
                        comboParameter.Items.AddRange(GetSpecialNgList(selectedItem.ObjectListKind));

                    // Select a default item
                    if (comboParameter.Items.Count != 0) comboParameter.SelectedIndex = 0;
                }
                else
                {
                    // Special case: if empty object list, then populate the parameter combobox with moveables by default
                    FindAndAddObjects<MoveableInstance>();
                }

                if (selectedItem.HasExtraList)
                {
                    labelExtra.Visible = true;
                    comboExtraParameter.Visible = true;
                    comboExtraParameter.Items.Clear();
                    comboExtraParameter.Text = "";

                    if (selectedItem.ExtraListKind == NgListKind.Fixed || selectedItem.ExtraListKind == NgListKind.Unknown)
                        comboExtraParameter.Items.AddRange(selectedItem.GetListForComboBox(NgParameterType.Extra));
                    else
                        comboExtraParameter.Items.AddRange(GetSpecialNgList(selectedItem.ExtraListKind));

                    // Select a default item
                    if (comboExtraParameter.Items.Count != 0) comboExtraParameter.SelectedIndex = 0;
                }
                else
                {
                    labelExtra.Visible = false;
                    comboExtraParameter.Visible = false;
                }
            }

            if (_loaded) UpdateExportToTrigger();
        }

        private void OnExtraChanged()
        {
            var triggerType = (TriggerType)comboType.SelectedItem;
            if (triggerType == TriggerType.ConditionNg)
            {
                var item = (comboExtraParameter.SelectedItem as NgTriggerKeyValuePair).Key;
                BitArray b = new BitArray(new int[] { item });
                cbBit1.Checked = b[0];
                cbBit2.Checked = b[1];
                cbBit3.Checked = b[2];
                cbBit4.Checked = b[3];
                cbBit5.Checked = b[4];
            }

            if (_loaded) UpdateExportToTrigger();
        }

        private void comboExtraParameter_SelectionChangeCommitted(object sender, EventArgs e)
        {
            OnExtraChanged();
        }

        private void comboTimer_SelectionChangeCommitted(object sender, EventArgs e)
        {
            OnTimerChanged();
        }

        private void comboParameter_SelectionChangeCommitted(object sender, EventArgs e)
        {
            OnParameterChanged();
        }

        private void comboType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            OnTriggerTypeChanged();
        }

        private void comboTargetType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            OnTriggerTargetChanged();
        }

        private void FormTrigger_Load(object sender, EventArgs e)
        {
            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MaximumSize = new Size(32000, Size.Height);
            MinimumSize = new Size(347 + (Size.Height - ClientSize.Height), Size.Height);

            // Populate lists
            foreach (TriggerType triggerType in Enum.GetValues(typeof(TriggerType)))
                comboType.Items.Add(triggerType);
            foreach (TriggerTargetType triggerTargetType in Enum.GetValues(typeof(TriggerTargetType)))
                comboTargetType.Items.Add(triggerTargetType);

            // Center items in the trigger window
            comboParameter.SelectedIndexChanged += delegate
            {
                if (!comboParameterBeingInitialized)
                {
                    var objectToSelect = comboParameter.SelectedItem as ObjectInstance;
                    if (objectToSelect != null)
                        _selectObject(objectToSelect);
                }
            };

            // Lot of things to do there

            // Following values can be set here without problems
            cbBit1.Checked = (_trigger.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_trigger.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_trigger.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_trigger.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_trigger.CodeBits & (1 << 4)) != 0;
            cbOneShot.Checked = _trigger.OneShot;

            comboTargetType.SelectedItem = _trigger.TargetType;

            // Now I have to load UI based on trigger
            if (_trigger.TriggerType == TriggerType.ConditionNg && _isNg)
            {
                comboType.SelectedItem = TriggerType.ConditionNg;
                OnTriggerTypeChanged();

                var conditionId = (_trigger.Timer & 0xFF);
                var conditionTrigger = NgCatalog.ConditionTrigger.MainList[conditionId];

                // Load conditions
                tbTimer.Visible = false;
                comboTimer.Visible = true;
                LoadNgConditionTrigger();
                SetNgComboboxValue(conditionId, comboTimer);
                OnTimerChanged();

                comboParameter.Visible = true;
                tbParameter.Visible = false;
                switch (conditionTrigger.ObjectListKind)
                {
                    case NgListKind.MoveablesInLevel:
                    case NgListKind.StaticsInLevel:
                    case NgListKind.CamerasInLevel:
                    case NgListKind.SinksInLevel:
                    case NgListKind.FlybyCamerasInLevel:
                        comboParameter.SelectedItem = _trigger.TargetObj;
                        OnParameterChanged();
                        break;
                    default:
                        SetNgComboboxValue(_trigger.TargetData, comboParameter);
                        OnParameterChanged();
                        break;
                }

                if (conditionTrigger.HasExtraList)
                {
                    var extra = ((_trigger.Timer & 0xFF00) >> 8);
                    SetNgComboboxValue(extra, comboExtraParameter);
                    OnExtraChanged();
                }
            }
            else
            {
                comboType.SelectedItem = _trigger.TriggerType;
                OnTriggerTypeChanged();
            }

            OnTriggerTargetChanged();
            if (_trigger.TargetType == TriggerTargetType.FlipEffect && _isNg)
            {
                LoadNgFlipeffectTrigger();
                if (NgCatalog.FlipEffectTrigger.MainList.ContainsKey(_trigger.TargetData))
                {
                    // Set the correct flipeffect
                    var flipeffect = NgCatalog.FlipEffectTrigger.MainList[_trigger.TargetData];
                    SetNgComboboxValue(_trigger.TargetData, comboParameter);
                    OnParameterChanged();

                    // Set values for timer and extra
                    if (flipeffect.HasTimerList && flipeffect.HasExtraList)
                    {
                        var timer = (_trigger.Timer & 0xFF);
                        var extra = ((_trigger.Timer & 0xFF00) >> 8);
                        SetNgComboboxValue(timer, comboTimer);
                        SetNgComboboxValue(extra, comboExtraParameter);
                        OnTimerChanged();
                        OnExtraChanged();
                    }
                    else
                    {
                        SetNgComboboxValue(_trigger.Timer, comboTimer);
                        OnTimerChanged();
                    }
                }
                else
                {
                    comboParameter.SelectedIndex = 0;
                    OnParameterChanged();
                }
            }
            else if (_trigger.TargetType == TriggerTargetType.TimerfieldNg && _isNg)
            {
                LoadNgTimerFieldTrigger();
                if (NgCatalog.TimerFieldTrigger.MainList.ContainsKey(_trigger.TargetData))
                {
                    // Set the correct flipeffect
                    var timerfield = NgCatalog.TimerFieldTrigger.MainList[_trigger.TargetData];
                    SetNgComboboxValue(_trigger.TargetData, comboParameter);
                    OnParameterChanged();
                    tbTimer.Text = _trigger.Timer.ToString();
                }
                else
                {
                    comboTimer.SelectedIndex = 0;
                    OnParameterChanged();
                }
            }
            else if (_trigger.TargetType == TriggerTargetType.ActionNg && _isNg)
            {
                LoadNgActionTrigger();
                var action = (_trigger.Timer & 0xFF);
                if (NgCatalog.ActionTrigger.MainList.ContainsKey(action))
                {
                    // Set the correct action
                    var actionTrigger = NgCatalog.ActionTrigger.MainList[action];
                    SetNgComboboxValue(action, comboTimer);
                    OnTimerChanged();

                    // Set values for object
                    comboParameter.SelectedItem = _trigger.TargetObj;
                    OnParameterChanged();

                    // Set the value for extra
                    if (actionTrigger.HasExtraList)
                    {
                        var extra = ((_trigger.Timer & 0xFF00) >> 8);
                        SetNgComboboxValue(extra, comboExtraParameter);
                        OnExtraChanged();
                    }
                }
                else
                {
                    comboTimer.SelectedIndex = 0;
                    OnTimerChanged();
                }
            }
            else if (_trigger.TargetType == TriggerTargetType.ParameterNg && _isNg)
            {
                // Do  nothing here...
            }
            else
            {
                if (TriggerInstance.UsesTargetObj(_trigger.TargetType))
                {
                    tbParameter.Visible = false;
                    comboParameter.SelectedItem = _trigger.TargetObj;
                }
                else
                {
                    comboParameter.Visible = false;
                    tbParameter.Text = _trigger.TargetData.ToString();
                }
                OnParameterChanged();

                comboTimer.Visible = false;
                tbTimer.Text = _trigger.Timer.ToString();

                comboExtraParameter.Visible = false;
                labelExtra.Visible = false;
            }

            UpdateExportToTrigger();

            _loaded = true;
        }

        private const string _noScriptIdStr = "<NoScriptID>";

        private void UpdateExportToTrigger()
        {
            var output = "";

            var triggerType = (TriggerType)comboType.SelectedItem;
            var targetType = (TriggerTargetType)comboTargetType.SelectedItem;

            if (!_isNg || !(triggerType == TriggerType.ConditionNg || targetType == TriggerTargetType.FlipEffect ||
                targetType == TriggerTargetType.ActionNg || targetType == TriggerTargetType.ParameterNg ||
                targetType == TriggerTargetType.TimerfieldNg))
            {
                labelScript.Visible = false;
                tbScript.Visible = false;
                butCopyToClipboard.Visible = false;
                return;
            }

            labelScript.Visible = true;
            tbScript.Visible = true;
            butCopyToClipboard.Visible = true;

            /*if ((!comboParameter.Visible || comboParameter.SelectedItem == null) ||
                (!comboTimer.Visible || comboTimer.SelectedItem == null) ||
                (!comboExtraParameter.Visible || comboExtraParameter.SelectedItem == null))
            {
                tbScript.Text = "Check all fields!";
                return;
            }*/

            if (triggerType == TriggerType.ConditionNg)
            {
                var condition = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                var conditionTrigger = NgCatalog.ConditionTrigger.MainList[condition];

                if (targetType == TriggerTargetType.Object)
                {
                    var selectedObject = comboParameter.SelectedItem as ObjectInstance;
                    if (selectedObject == null)
                    {
                        DarkMessageBox.Show(this, "You don't have in your project any object of the type that you have required", "Export trigger", MessageBoxIcon.Warning);
                        return;
                    }

                    string target = (selectedObject as IHasScriptID)?.ScriptId?.ToString() ?? _noScriptIdStr;
                    var timer = condition;
                    if (conditionTrigger.HasExtraList)
                    {
                        if (comboExtraParameter.SelectedItem == null) { tbScript.Text = "Check all fields!"; return; }
                        timer |= ((comboExtraParameter.SelectedItem as NgTriggerKeyValuePair).Key << 8);
                    }
                    output = "$9000," + target + ",$" + timer.ToString("X4");
                }
                else
                {
                    var parameter = (comboParameter.SelectedItem as NgTriggerKeyValuePair).Key;
                    var timer = condition;
                    if (conditionTrigger.HasExtraList)
                    {
                        if (comboExtraParameter.SelectedItem == null) { tbScript.Text = "Check all fields!"; return; }
                        timer |= ((comboExtraParameter.SelectedItem as NgTriggerKeyValuePair).Key << 8);
                    }
                    output = "$8000," + parameter + ",$" + timer.ToString("X4");
                }
            }
            else
            {
                if (targetType == TriggerTargetType.FlipEffect)
                {
                    var flipeffectId = (comboParameter.SelectedItem as NgTriggerKeyValuePair).Key;
                    var flipeffectTrigger = NgCatalog.FlipEffectTrigger.MainList[flipeffectId];
                    output = "$2000," + flipeffectId + ",";
                    if (flipeffectTrigger.HasExtraList)
                    {
                        if (comboTimer.SelectedItem == null) { tbScript.Text = "Check all fields!"; return; }
                        if (comboExtraParameter.SelectedItem == null) { tbScript.Text = "Check all fields!"; return; }
                        var timer = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                        var extra = (comboExtraParameter.SelectedItem as NgTriggerKeyValuePair).Key;
                        output += "$" + ((extra << 8) | timer).ToString("X4");
                    }
                    else
                    {
                        if (comboTimer.SelectedItem == null) { tbScript.Text = "Check all fields!"; return; }
                        var timer = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                        output += "$" + timer.ToString("X4");
                    }
                }
                else if (targetType == TriggerTargetType.ActionNg)
                {
                    var action = (comboTimer.SelectedItem as NgTriggerKeyValuePair).Key;
                    var actionTrigger = NgCatalog.ActionTrigger.MainList[action];
                    var timer = (short)action;

                    var selectedObject = comboParameter.SelectedItem as ObjectInstance;
                    if (selectedObject == null)
                    {
                        DarkMessageBox.Show(this, "You don't have in your project any object of the type that you have required", "Export trigger", MessageBoxIcon.Warning);
                        return;
                    }

                    if (actionTrigger.HasExtraList)
                    {
                        if (comboExtraParameter.SelectedItem == null) { tbScript.Text = "Check all fields!"; return; }
                        var extra = (comboExtraParameter.SelectedItem as NgTriggerKeyValuePair).Key;
                        timer += (short)(extra << 8);
                    }

                    string target = (selectedObject as IHasScriptID)?.ScriptId?.ToString() ?? _noScriptIdStr;

                    output = "$5000," + target + ",$" + timer.ToString("X4");
                }
            }

            tbScript.Text = output;
        }

        private void butCopyToClipboard_Click(object sender, EventArgs e)
        {
            (comboParameter.SelectedItem as IHasScriptID)?.AllocateNewScriptId();
            UpdateExportToTrigger();
            Clipboard.SetText(tbScript.Text);
        }

        private void FormTrigger_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (tbScript.Bounds.Contains(e.Location))
            {
                (comboParameter.SelectedItem as IHasScriptID)?.AllocateNewScriptId();
                UpdateExportToTrigger();
            }
        }

        private void tbScript_TextChanged(object sender, EventArgs e)
        {
            tbScript.Enabled = tbScript.Text.IndexOf(_noScriptIdStr, StringComparison.InvariantCulture) == -1;
        }
    }
}
