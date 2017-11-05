using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Geometry;
using TombLib.IO;
using System.IO;
using DarkUI.Collections;
using DarkUI.Forms;

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
        };

        private SortableBindingList<ImportedGeometryWrapper> _dataGridViewDataSource = new SortableBindingList<ImportedGeometryWrapper>();
        [Browsable(false)]
        public LevelSettings LevelSettings { get; set; }

        private Color _correctColor;
        private Color _wrongColor;

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
                    string path = BrowseFile(null);
                    if (string.IsNullOrEmpty(path))
                        return null;

                    var info = ImportedGeometryInfo.Default;
                    info.Path = path;
                    info.Name = Path.GetFileNameWithoutExtension(path);

                    ImportedGeometry newObject = new ImportedGeometry();
                    LevelSettings.ImportedGeometryUpdate(newObject, info);
                    LevelSettings.ImportedGeometries.Add(newObject);
                    return new ImportedGeometryWrapper(this, newObject);
                };
            dataGridView.DataSource = _dataGridViewDataSource;
            dataGridViewControls.DeleteRowCheckIfCancel = MessageUserAboutHimDeletingRows;
            _dataGridViewDataSource.ListChanged += delegate (object sender, ListChangedEventArgs e)
                {
                    switch (e.ListChangedType)
                    {
                        case ListChangedType.ItemDeleted:
                            var remainingElements = new HashSet<ImportedGeometry>(_dataGridViewDataSource.Select((wrapper) => wrapper.Object));
                            LevelSettings.ImportedGeometries.RemoveAll((obj) => !remainingElements.Contains(obj)); // Don't use indices here, the wrapper indices might not match with the real object if sorting was enabled.
                            break;
                    }
                };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (LevelSettings != null)
                foreach (var importedGeometry in LevelSettings.ImportedGeometries)
                    _dataGridViewDataSource.Add(new ImportedGeometryWrapper(this, importedGeometry));
        }

        private string BrowseFile(string path)
        {
            path = LevelSettings.MakeAbsolute(path);
            using (FileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = SupportedFormats.GetFilter(FileFormatType.GeometryImport);
                dialog.Title = "Select a 3D file that you want to see imported.";
                dialog.FileName = string.IsNullOrEmpty(path) ? "" : Path.GetFileName(path);
                dialog.InitialDirectory = string.IsNullOrEmpty(path) ? path : Path.GetDirectoryName(path);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;
                return LevelSettings.MakeRelative(dialog.FileName, VariableType.LevelDirectory);
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.RowIndex < 0) || (e.RowIndex >= _dataGridViewDataSource.Count))
                return;

            if ((dataGridView.Columns[e.ColumnIndex] == searchButtonColumn))
            {
                string path = BrowseFile(_dataGridViewDataSource[e.RowIndex].Path);
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
            if ((e.RowIndex < 0) || (e.RowIndex >= _dataGridViewDataSource.Count))
                return;

            ImportedGeometry object_ = _dataGridViewDataSource[e.RowIndex].Object;
            if (object_.LoadException == null)
            {
                if (dataGridView.Columns[e.ColumnIndex].DataPropertyName == "ErrorMessage")
                {
                    e.CellStyle.BackColor = _correctColor;
                    e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_correctColor, 0.4);
                }
            }
            else
            {
                e.CellStyle.BackColor = _wrongColor;
                e.CellStyle.SelectionBackColor = e.CellStyle.SelectionBackColor.MixWith(_wrongColor, 0.4);
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
                "Do you want to continue?", "Imported geometry removal", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button2) != DialogResult.Yes;
        }

        // Prevent user message from appearing multiple time for multi row deletes
        private bool? userDeletingRow_Cancel = null;

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
