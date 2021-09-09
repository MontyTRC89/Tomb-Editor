using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Wad;

namespace TombEditor.Forms
{
    public partial class FormQuickItemgroup : DarkUI.Forms.DarkForm
    {
        private readonly List<IWadObjectId> dropDownValues;
        private readonly Editor _editor;
        public IWadObjectId SelectedValue;
        public FormQuickItemgroup(Editor _editor)
        {
            InitializeComponent();
            this._editor = _editor;
            dropDownValues = new List<IWadObjectId>();

            cbSlots.SelectedValueChanged += CbSlots_SelectedValueChanged;
            cbSlots.Format += new ListControlConvertEventHandler(cbSlots_Format);
            cbSlots.Items.Clear();
            foreach (var moveable in _editor.Level.Settings.WadGetAllMoveables().Keys)
                dropDownValues.Add(moveable);
            foreach (var staticMesh in _editor.Level.Settings.WadGetAllStatics().Keys)
                dropDownValues.Add(staticMesh);
            if (cbSlots.Items.Count > 0 && cbSlots.SelectedIndex == -1)
                cbSlots.SelectedIndex = 0;
            cbSlots.DataSource = dropDownValues;
        }

        private void CbSlots_SelectedValueChanged(object sender, EventArgs e)
        {
            SelectedValue = (IWadObjectId)cbSlots.SelectedValue;
        }

        private void ButOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void ButCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cbSlots_Format(object sender, ListControlConvertEventArgs e)
        {
            TRVersion.Game? gameVersion = _editor?.Level?.Settings?.GameVersion;
            IWadObjectId listItem = e.ListItem as IWadObjectId;
            if (gameVersion != null && listItem != null)
                e.Value = listItem.ToString(_editor.Level.Settings.GameVersion);
        }
    }
}
