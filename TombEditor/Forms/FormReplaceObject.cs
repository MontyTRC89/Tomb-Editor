using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormReplaceObject : DarkForm
    {
        private enum ObjectSelectionType
        {
            None,
            Source,
            Destination
        }

        private enum ObjectSearchType
        {
            [Description("Primary attribute only (object ID, sound ID, color, sink strength...)")]
            PrimaryAttributeOnly,
            [Description("Full (including OCB, light type, scale...)")]
            Full
        }

        private const string _sourcePrompt = " [ Please select source object in level ]";
        private const string _destPrompt   = " [ Please select destination object in level ]";

        private ObjectSelectionType SelectionType
        {
            get { return _selectionType; }
            set
            {
                if (_selectionType == value)
                    return;

                // Block button to indicate that selection is in progress
                var selectDest = (value == ObjectSelectionType.Destination);
                butSelectDestObject.Enabled   = value == ObjectSelectionType.None || !selectDest;
                butSelectSourceObject.Enabled = value == ObjectSelectionType.None ||  selectDest;

                _selectionType = value;

                // Auto-toggle item for convenience
                ToggleItem((PositionBasedObjectInstance)_editor.SelectedObject);
            }
        }
        private ObjectSelectionType _selectionType;

        private PositionBasedObjectInstance Source
        {
            get { return _source; }
            set
            {
                _source = (PositionBasedObjectInstance)value?.Clone();
                tbSourceObject.Text = (value == null) ? _sourcePrompt : GetDescription(_source);
                if (_dest == null) tbDestObject.Text = string.Empty; // Empty opposing selection if null
                UpdateUI();
            }
        }
        private PositionBasedObjectInstance _source;

        private PositionBasedObjectInstance Dest
        {
            get { return _dest; }
            set
            {
                _dest = (PositionBasedObjectInstance)value?.Clone();
                tbDestObject.Text = (value == null) ? _destPrompt : GetDescription(_dest);
                if (_source == null) tbSourceObject.Text = string.Empty; // Empty opposing selection if null
                UpdateUI();
            }
        }
        private PositionBasedObjectInstance _dest;

        private readonly Editor _editor;

        public FormReplaceObject(Editor editor)
        {
            _editor = editor;
            _editor.EditorEventRaised += _editor_EditorEventRaised;
            InitializeComponent();

            // Set window property handlers
            Configuration.LoadWindowProperties(this, _editor.Configuration);
            FormClosing += new FormClosingEventHandler((s, e) => Configuration.SaveWindowProperties(this, _editor.Configuration));

            // Populate search type combos
            foreach (ObjectSearchType searchType in Enum.GetValues(typeof(ObjectSearchType)))
            {
                cmbReplaceType.Items.Add(searchType.GetEnumDescription());
                cmbSearchType.Items.Add(searchType.GetEnumDescription());
            }

            // Init UI
            InitializeNewSearch();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= _editor_EditorEventRaised;
            base.Dispose(disposing);
        }

        private void _editor_EditorEventRaised(IEditorEvent obj)
        {
            // Level has changed, reset everything
            if (obj is Editor.LevelChangedEvent)
                InitializeNewSearch();

            // Re-toggle current object on changed object selection
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                var evt  = (Editor.SelectedObjectChangedEvent)obj;
                if (evt.Current == null) return;
                ToggleItem((PositionBasedObjectInstance)evt.Current);
            }
        }

        private string GetDescription(PositionBasedObjectInstance instance)
        {
            var result = instance.ToShortString();

            if (instance is MoveableInstance)
                result += " (OCB: " + ((MoveableInstance)instance).Ocb + ")";
            else if (instance is StaticInstance && _editor.Level.Settings.GameVersion == TRVersion.Game.TRNG)
                result += " (OCB: " + ((StaticInstance)instance).Ocb + ")";
            else if (instance is SinkInstance)
                result += " (Strength: " + ((SinkInstance)instance).Strength + ")";
            else if (instance is ImportedGeometryInstance)
                result += " (Scale: " + ((ImportedGeometryInstance)instance).Scale.ToString(".0#") + ")";
            else if (instance is SoundSourceInstance)
                result  = "Sound source (ID: " + ((SoundSourceInstance)instance).SoundId + ")"; // FIXME: Why sound source fails with ToShortString? Room data wrongly cloned?

            return result;
        }

        private bool TypeIsReplaceable(PositionBasedObjectInstance instance)
        {
            // Probably should be extended if in the future there will be other types of
            // property-less objects

            return !(instance is CameraInstance ||
                     instance is FlybyCameraInstance);
        }

        private void ToggleItem(PositionBasedObjectInstance item)
        {
            if (!TypeIsReplaceable(item))
            {
                _editor.SendMessage("Selected object can't be replaced with another object. Select another object type.", PopupType.Warning);
                return;
            }

            // Foolproofness to prevent selection of objects of different types.
            // Probably more smart approach is to reset opposite selection type, but IMO it's too intrusive. -- Lwmte

            switch (SelectionType)
            {
                case ObjectSelectionType.Destination:
                    if (Source != null && item != null && item.GetType() != Source.GetType())
                        ObjectsNotMatchMessage();
                    else
                        Dest = item;
                    break;

                case ObjectSelectionType.Source:
                    if (Dest != null && item != null && item.GetType() != Dest.GetType())
                        ObjectsNotMatchMessage();
                    else
                        Source = item;
                    break;
            }
        }

        private void InitializeNewSearch()
        {
            Source = Dest = null;
            lblResult.Text = string.Empty;
            cmbReplaceType.SelectedIndex = cmbSearchType.SelectedIndex = 0;

            if (_editor.SelectedObject is PositionBasedObjectInstance && TypeIsReplaceable((PositionBasedObjectInstance)_editor.SelectedObject))
            {
                SelectionType = ObjectSelectionType.Source;
                SelectionType = ObjectSelectionType.Destination;
                Dest = null;
            }
            else if (_editor.SelectedObject == null)
                SelectionType = ObjectSelectionType.Source; // Just arm for source selection, if selected object is null
        }

        private void UpdateUI()
        {
            butReplace.Enabled = (Source != null && Dest != null);

            // Both search / replacement type is disabled for statics in all game versions except TRNG as statics have OCBs there.
            // Sinks and sound sources have no additional parameters, therefore we block search type choice.
            cmbSearchType.Enabled  = !(Source == null ||
                                       Source is SinkInstance ||
                                       Source is SoundSourceInstance ||
                                       Source is StaticInstance && _editor.Level.Settings.GameVersion != TRVersion.Game.TRNG);

            // Additionally, light type can't be changed in runtime (thanks TRTomb?), so we block it as well for replace type choice.
            cmbReplaceType.Enabled = !(Source == null ||
                                       Source is SinkInstance ||
                                       Source is SoundSourceInstance ||
                                       Source is LightInstance ||
                                       Source is StaticInstance && _editor.Level.Settings.GameVersion != TRVersion.Game.TRNG);

            // Indicate source / dest light colour, if object type is light.

            if (Source is LightInstance)
            {
                colSource.BackColor = new Vector4(((LightInstance)Source).Color * 0.5f, 1.0f).ToWinFormsColor();
                colSource.Visible = true;
            }
            else
                colSource.Visible = false;

            if (Dest is LightInstance)
            {
                colDest.BackColor = new Vector4(((LightInstance)Dest).Color * 0.5f, 1.0f).ToWinFormsColor();
                colDest.Visible = true;
            }
            else
                colDest.Visible = false;
        }

        private void ObjectsNotMatchMessage() => _editor.SendMessage("Object types must match.\nSelect object of the same type or push \"New search\".", PopupType.Warning);
        private void butSelectSourceObject_Click(object sender, EventArgs e) => SelectionType = ObjectSelectionType.Source;
        private void butSelectDestObject_Click(object sender, EventArgs e) => SelectionType = ObjectSelectionType.Destination;
        private void butCancel_Click(object sender, EventArgs e) => Close();
        private void butNewSearch_Click(object sender, EventArgs e) => InitializeNewSearch();

        private void butReplace_Click(object sender, EventArgs e)
        {
            var sourceType = Source.GetType();

            int roomCount = 0;
            int replCount = 0;

            foreach (var room in cbSelectedRooms.Checked ? _editor.SelectedRooms : _editor.Level.Rooms)
            {
                if (room == null) continue;

                bool anyObjectsChanged = false;

                foreach (var obj in room.Objects.Where(item => item.GetType() == sourceType))
                {
                    bool objectChanged = false;

                    if (obj is MoveableInstance)
                    {
                        var refObject  = (MoveableInstance)Source;
                        var destObject = (MoveableInstance)Dest;
                        var currObject = (MoveableInstance)obj;

                        if (refObject.WadObjectId != currObject.WadObjectId)
                            continue;

                        // Moveable: secondary attrib is OCB. Search and replacement of secondary attrib is possible.

                        if (cmbSearchType.SelectedIndex != (int)ObjectSearchType.PrimaryAttributeOnly &&
                            refObject.Ocb != currObject.Ocb)
                            continue;

                        currObject.WadObjectId = destObject.WadObjectId;

                        if (cmbReplaceType.SelectedIndex != (int)ObjectSearchType.PrimaryAttributeOnly)
                            currObject.Ocb = destObject.Ocb;

                        objectChanged = true;
                    }
                    else if (obj is StaticInstance)
                    {
                        var refObject  = (StaticInstance)Source;
                        var destObject = (StaticInstance)Dest;
                        var currObject = (StaticInstance)obj;

                        if (refObject.WadObjectId != currObject.WadObjectId)
                            continue;

                        // Static: secondary attrib is OCB for TRNG only. Search and replacement of secondary attrib is possible for TRNG only.

                        if (_editor.Level.Settings.GameVersion == TRVersion.Game.TRNG &&
                            cmbSearchType.SelectedIndex != (int)ObjectSearchType.PrimaryAttributeOnly &&
                            refObject.Ocb != currObject.Ocb)
                            continue;

                        currObject.WadObjectId = destObject.WadObjectId;

                        if (_editor.Level.Settings.GameVersion == TRVersion.Game.TRNG && 
                            cmbReplaceType.SelectedIndex != (int)ObjectSearchType.PrimaryAttributeOnly)
                            currObject.Ocb = destObject.Ocb;

                        objectChanged = true;
                    }
                    else if (obj is LightInstance)
                    {
                        var refObject  = (LightInstance)Source;
                        var destObject = (LightInstance)Dest;
                        var currObject = (LightInstance)obj;

                        if (refObject.Color != currObject.Color || destObject.Color == currObject.Color)
                            continue;

                        // Light: secondary attrib is light type. Only search of secondary attrib is possible.

                        if (cmbSearchType.SelectedIndex != (int)ObjectSearchType.PrimaryAttributeOnly &&
                           refObject.Type != currObject.Type)
                            continue;

                        currObject.Color = destObject.Color;
                        objectChanged = true;
                    }
                    else if (obj is SinkInstance)
                    {
                        var refObject  = (SinkInstance)Source;
                        var destObject = (SinkInstance)Dest;
                        var currObject = (SinkInstance)obj;

                        if (refObject.Strength != currObject.Strength || destObject.Strength == currObject.Strength)
                            continue;

                        // Sink: no secondary attribs.

                        currObject.Strength = destObject.Strength;
                        objectChanged = true;
                    }
                    else if (obj is SoundSourceInstance)
                    {
                        var refObject  = (SoundSourceInstance)Source;
                        var destObject = (SoundSourceInstance)Dest;
                        var currObject = (SoundSourceInstance)obj;

                        if (refObject.SoundId != currObject.SoundId || destObject.SoundId == currObject.SoundId)
                            continue;

                        // Sound source: no secondary attribs.

                        currObject.SoundId = destObject.SoundId;
                        objectChanged = true;
                    }
                    else if (obj is ImportedGeometryInstance)
                    {
                        var refObject  = (ImportedGeometryInstance)Source;
                        var destObject = (ImportedGeometryInstance)Dest;
                        var currObject = (ImportedGeometryInstance)obj;

                        if (!refObject.Model.Equals(currObject.Model))
                            continue;

                        // Imp. geo: secondary attrib is scale. Search and replacement of secondary attrib is possible.

                        if (cmbSearchType.SelectedIndex != (int)ObjectSearchType.PrimaryAttributeOnly &&
                            refObject.Scale != currObject.Scale)
                            continue;

                        currObject.Model = destObject.Model;

                        if (cmbReplaceType.SelectedIndex != (int)ObjectSearchType.PrimaryAttributeOnly)
                            currObject.Scale *= destObject.Scale;

                        objectChanged = true;
                    }

                    if (objectChanged)
                    {
                        replCount++;
                        _editor.ObjectChange(obj, ObjectChangeType.Change);
                        anyObjectsChanged |= objectChanged;
                    }
                }

                if (anyObjectsChanged)
                {
                    roomCount++;
                    if (Source is LightInstance) room.RoomGeometry?.Relight(room); // HACK!
                }
            }

            if (replCount > 0)
                lblResult.Text = "Replacement finished. Replaced " + replCount + " objects in " + roomCount + " room" + (roomCount > 1 ? "s" : string.Empty) + ".";
            else
                lblResult.Text = "No matching objects found. No replacements were made.";
        }

        private void butSwapSrcDest_Click(object sender, EventArgs e)
        {
            var temp = Source;
            Source = Dest;
            Dest = temp;
        }

        // Convenient picker helper to replace color value right in window without selecting another light.
        private void colorPicker_Click(object sender, EventArgs e)
        {
            var panel = sender as DarkPanel;
            var light = panel == colSource ? (LightInstance)Source : (LightInstance)Dest;

            if (panel == null || light == null) return;

            using (var colorDialog = new RealtimeColorDialog(_editor.Configuration.UI_ColorScheme))
            {
                colorDialog.Color = panel.BackColor;
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (panel.BackColor != colorDialog.Color)
                {
                    panel.BackColor = colorDialog.Color;
                    light.Color = colorDialog.Color.ToFloat3Color() * 2.0f;
                }
            }
        }
    }
}
