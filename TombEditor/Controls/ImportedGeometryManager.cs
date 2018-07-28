using DarkUI.Collections;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Controls
{
    public partial class ImportedGeometryManager : UserControl
    {
        // Wrapper class around the ImportedGeometry object.
        // For every real "ImportedGeometry" object one of these is created and added to the data source.
        // This wrapper is necessary to support setting value.
        private class ImportedGeometryWrapper
        {
            private readonly ImportedGeometryManager _parent;
            [Browsable(false)]
            public ImportedGeometry Object { get; private set; }

            public ImportedGeometryWrapper(ImportedGeometryManager parent, ImportedGeometry object_)
            {
                _parent = parent;
                Object = object_;
            }

            public string Name
            {
                get { return Object.Info.Name; }
                set { Set((ref ImportedGeometryInfo info) => info.Name = value); }
            }

            public string Path
            {
                get { return Object.Info.Path; }
                set { Set((ref ImportedGeometryInfo info) => info.Path = value); }
            }

            public float Scale
            {
                get { return Object.Info.Scale; }
                set { Set((ref ImportedGeometryInfo info) => info.Scale = value); }
            }

            public bool SwapXY
            {
                get { return Object.Info.SwapXY; }
                set { Set((ref ImportedGeometryInfo info) => info.SwapXY = value); }
            }

            public bool SwapXZ
            {
                get { return Object.Info.SwapXZ; }
                set { Set((ref ImportedGeometryInfo info) => info.SwapXZ = value); }
            }

            public bool SwapYZ
            {
                get { return Object.Info.SwapYZ; }
                set { Set((ref ImportedGeometryInfo info) => info.SwapYZ = value); }
            }

            public bool FlipX
            {
                get { return Object.Info.FlipX; }
                set { Set((ref ImportedGeometryInfo info) => info.FlipX = value); }
            }

            public bool FlipY
            {
                get { return Object.Info.FlipY; }
                set { Set((ref ImportedGeometryInfo info) => info.FlipY = value); }
            }

            public bool FlipZ
            {
                get { return Object.Info.FlipZ; }
                set { Set((ref ImportedGeometryInfo info) => info.FlipZ = value); }
            }

            public bool FlipUV_V
            {
                get { return Object.Info.FlipUV_V; }
                set { Set((ref ImportedGeometryInfo info) => info.FlipUV_V = value); }
            }

            public bool InvertFaces
            {
                get { return Object.Info.InvertFaces; }
                set { Set((ref ImportedGeometryInfo info) => info.InvertFaces = value); }
            }

            public string ErrorMessage
            {
                get
                {
                    if (Object.LoadException == null)
                        return "Successfully loaded";
                    return Object.LoadException.Message + " (" + Object.LoadException.GetType().Name + ")";
                }
            }

            public delegate void SetValue(ref ImportedGeometryInfo info);
            public void Set(SetValue setValue)
            {
                ImportedGeometryInfo info = Object.Info;
                setValue(ref info);
                _parent.LevelSettings.ImportedGeometryUpdate(Object, info);
            }
        }

        private readonly SortableBindingList<ImportedGeometryWrapper> _dataGridViewDataSource = new SortableBindingList<ImportedGeometryWrapper>();
        [Browsable(false)]
        public LevelSettings LevelSettings { get; set; }

        private readonly Color _correctColor;
        private readonly Color _wrongColor;

        public ImportedGeometryManager()
        {
            InitializeComponent();

            _correctColor = dataGridView.BackColor.MixWith(Color.LimeGreen, 0.55);
            _wrongColor = dataGridView.BackColor.MixWith(Color.DarkRed, 0.55);

            // Initialize sound path data grid view
            dataGridViewControls.DataGridView = dataGridView;
            dataGridViewControls.Enabled = true;
            dataGridViewControls.CreateNewRow = delegate
            {
                List<string> paths = LevelFileDialog.BrowseFiles(this, LevelSettings, PathC.GetDirectoryNameTry(LevelSettings.LevelFilePath),
                    "Select the 3D files that you want to see imported.", ImportedGeometry.FileExtensions, VariableType.LevelDirectory).ToList();

                // Load imported geometries
                var importInfos = new List<KeyValuePair<ImportedGeometry, ImportedGeometryInfo>>();
                foreach (string path in paths)
                {
                    ImportedGeometryInfo info = ImportedGeometryInfo.Default;
                    info.Path = path;
                    info.Name = PathC.GetFileNameWithoutExtensionTry(path);

                    using (var settingsDialog = new GeometryIOSettingsDialog(new IOGeometrySettings()))
                    {
                        settingsDialog.AddPreset(IOSettingsPresets.SettingsPresets);
                        if (settingsDialog.ShowDialog(this) == DialogResult.Cancel)
                            continue;

                        info.Scale = settingsDialog.Settings.Scale;
                        info.SwapXY = settingsDialog.Settings.SwapXY;
                        info.SwapXZ = settingsDialog.Settings.SwapXZ;
                        info.SwapYZ = settingsDialog.Settings.SwapYZ;
                        info.InvertFaces = settingsDialog.Settings.InvertFaces;
                        info.FlipX = settingsDialog.Settings.FlipX;
                        info.FlipY = settingsDialog.Settings.FlipY;
                        info.FlipZ = settingsDialog.Settings.FlipZ;
                        info.FlipUV_V = settingsDialog.Settings.FlipUV_V;
                    }

                    importInfos.Add(new KeyValuePair<ImportedGeometry, ImportedGeometryInfo>(new ImportedGeometry(), info));
                }

                LevelSettings.ImportedGeometryUpdate(importInfos);
                LevelSettings.ImportedGeometries.AddRange(importInfos.Select(entry => entry.Key));
                return importInfos.Select(entry => new ImportedGeometryWrapper(this, entry.Key));
            };
            dataGridView.DataSource = _dataGridViewDataSource;
            dataGridViewControls.DeleteRowCheckIfCancel = MessageUserAboutHimDeletingRows;
            _dataGridViewDataSource.ListChanged += delegate (object sender, ListChangedEventArgs e)
                {
                    switch (e.ListChangedType)
                    {
                        case ListChangedType.ItemDeleted:
                            var remainingElements = new HashSet<ImportedGeometry>(_dataGridViewDataSource.Select(wrapper => wrapper.Object));
                            LevelSettings.ImportedGeometries.RemoveAll(obj => !remainingElements.Contains(obj)); // Don't use indices here, the wrapper indices might not match with the real object if sorting was enabled.
                            break;
                    }
                };
            Enabled = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (LevelSettings != null)
                foreach (var importedGeometry in LevelSettings.ImportedGeometries)
                    _dataGridViewDataSource.Add(new ImportedGeometryWrapper(this, importedGeometry));
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _dataGridViewDataSource.Count)
                return;

            if (dataGridView.Columns[e.ColumnIndex].Name == searchButtonColumn.Name)
            {
                string path = LevelFileDialog.BrowseFile(this, LevelSettings, _dataGridViewDataSource[e.RowIndex].Path,
                    "Select a 3D file that you want to see imported.", ImportedGeometry.FileExtensions, VariableType.LevelDirectory, false);
                if (!string.IsNullOrEmpty(path))
                {
                    var info = _dataGridViewDataSource[e.RowIndex];
                    info.Path = path;
                    _dataGridViewDataSource[e.RowIndex] = info;
                }
            }
        }

        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _dataGridViewDataSource.Count)
                return;

            if (dataGridView.Columns[e.ColumnIndex].Name == errorMessageColumn.Name)
            {
                ImportedGeometry object_ = _dataGridViewDataSource[e.RowIndex].Object;
                if (object_.LoadException == null)
                {
                    e.CellStyle.BackColor = _correctColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_correctColor, 0.4);

                }
                else
                {
                    e.CellStyle.BackColor = _wrongColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_wrongColor, 0.4);
                }
            }
            else if (dataGridView.Columns[e.ColumnIndex].Name == pathColumn.Name)
            {
                ImportedGeometry object_ = _dataGridViewDataSource[e.RowIndex].Object;
                string absolutePath = LevelSettings.MakeAbsolute(object_.Info.Path);
                dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = absolutePath;
            }
        }

        [Browsable(false)]
        public ImportedGeometry SelectedImportedGeometry
        {
            get
            {
                var selectedRows = dataGridView.SelectedRows;
                return selectedRows.Count < 1 ? null : ((ImportedGeometryWrapper)selectedRows[0].DataBoundItem).Object;
            }
            set
            {
                for (int i = 0; i < _dataGridViewDataSource.Count; ++i)
                    if (_dataGridViewDataSource[i].Object == value)
                    {
                        dataGridView.ClearSelection();
                        dataGridView.SelectedRows[i].Selected = true;
                        return;
                    }
            }
        }

        private bool MessageUserAboutHimDeletingRows()
        {
            return DarkMessageBox.Show(this, "You are about to delete " + dataGridView.SelectedRows.Count +
                " imported geometries from the list! The geometry infos will also be removed from all associated objects. " +
                "Do you want to continue?", "Imported geometry removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes;
        }

        // Prevent user message from appearing multiple time for multi row deletes
        private bool? userDeletingRow_Cancel;

        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (!userDeletingRow_Cancel.HasValue)
                userDeletingRow_Cancel = MessageUserAboutHimDeletingRows();
            e.Cancel = userDeletingRow_Cancel.Value;
        }

        private void dataGridView_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
                userDeletingRow_Cancel = null;
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (userDeletingRow_Cancel.HasValue && userDeletingRow_Cancel.Value)
                userDeletingRow_Cancel = null;
        }
    }
}
