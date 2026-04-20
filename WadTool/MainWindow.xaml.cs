using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using WadTool.Controls;
using WadTool.MeshEditor;
using WadTool.ViewModels;

namespace WadTool
{
    public partial class MainWindow : Window, IMainWindowHost
    {
        private readonly MainWindowViewModel _viewModel;
        private readonly WadToolClass _tool;

        private readonly ContextMenu _contextMenuMoveable;
        private readonly ContextMenu _contextMenuStatic;

        public MainWindow(WadToolClass tool)
        {
            // Start fully transparent to prevent flash during initialization.
            Opacity = 0;

            InitializeComponent();

            // Show debug menu only when debugger is attached.
            if (Debugger.IsAttached)
                menuDebug.Visibility = Visibility.Visible;

            _tool = tool;
            _viewModel = new MainWindowViewModel(tool);
            DataContext = _viewModel;

            // Initialize 3D preview panel.
            panel3D.FieldOfView = tool.Configuration.RenderingItem_FieldOfView;
            panel3D.NavigationSpeedMouseRotate = tool.Configuration.RenderingItem_NavigationSpeedMouseRotate;
            panel3D.NavigationSpeedMouseTranslate = tool.Configuration.RenderingItem_NavigationSpeedMouseTranslate;
            panel3D.NavigationSpeedMouseWheelZoom = tool.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
            panel3D.NavigationSpeedMouseZoom = tool.Configuration.RenderingItem_NavigationSpeedMouseZoom;
            panel3D.BackgroundColor = tool.Configuration.RenderingItem_BackgroundColor;
            panel3D.InitializeItemPreview(DeviceManager.DefaultDeviceManager.Device);

            // Set up tree view events.
            treeDestWad.ReadOnly = false;
            treeSourceWad.ReadOnly = false;

            treeDestWad.SelectedWadObjectIdsChanged += TreeDestWad_SelectedWadObjectIdsChanged;
            treeDestWad.ClickOnEmpty += (s, e) => WadActions.LoadWadOpenFileDialog(_tool, this, true);
            treeDestWad.MetadataChanged += (s, e) => _tool.ToggleUnsavedChanges(true);
            treeDestWad.ItemDoubleClicked += (s, e) => { if (treeDestWad.ItemSelected) _viewModel.EditItemCommand.Execute(null); };
            treeDestWad.ItemKeyDown += TreeDestWad_KeyDown;

            treeSourceWad.SelectedWadObjectIdsChanged += TreeSourceWad_SelectedWadObjectIdsChanged;
            treeSourceWad.ClickOnEmpty += (s, e) => WadActions.LoadWadOpenFileDialog(_tool, this, false);
            treeSourceWad.ItemDoubleClicked += (s, e) =>
            {
                if (treeSourceWad.SelectedWadObjectIds.Any())
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
                        _viewModel.AddObjectToDifferentSlotCommand.Execute(null);
                    else
                        _viewModel.AddObjectCommand.Execute(null);
                }
            };

            // Build context menus.
            _contextMenuMoveable = BuildMoveableContextMenu();
            _contextMenuStatic = BuildStaticContextMenu();

            // Subscribe to ViewModel events.
            _viewModel.RecentWadsRefreshRequested += RefreshRecentWadsList;

            // Initialize ViewModel with host reference.
            _viewModel.SetHost(this);

            // Load window position from config.
            LoadWindowProperties();

            // Defer showing the window until all content is rendered.
            ContentRendered += Window_ContentRendered;

            // Try to load reference project on start-up, if specified in config.
            if (!string.IsNullOrEmpty(tool.Configuration.Tool_ReferenceProject) &&
                File.Exists(tool.Configuration.Tool_ReferenceProject))
                WadActions.LoadReferenceLevel(tool, this, tool.Configuration.Tool_ReferenceProject);

            RefreshRecentWadsList();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Set the HWND background color to the dark theme color to prevent
            // white flash when maximizing or restoring the window.
            var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            if (hwndSource?.CompositionTarget != null)
                hwndSource.CompositionTarget.BackgroundColor = Color.FromRgb(0x3C, 0x3F, 0x41);
        }

        #region IMainWindowHost Implementation

        public IntPtr Handle => new WindowInteropHelper(this).Handle;

        public IReadOnlyList<IWadObjectId> DestSelectedIds
            => treeDestWad.SelectedWadObjectIds.ToList();

        public IReadOnlyList<IWadObjectId> SourceSelectedIds
            => treeSourceWad.SelectedWadObjectIds.ToList();

        public void SelectDestinationObjects(IWadObjectId id)
            => treeDestWad.Select(id);

        public void SelectDestinationObjects(List<IWadObjectId> ids)
            => treeDestWad.Select(ids);

        public void InvalidatePreview()
            => panel3D.Render();

        public void ResetPreviewCamera()
            => panel3D.ResetCamera();

        public void GarbageCollectPreview()
            => panel3D.GarbageCollect();

        public void SetPreviewAnimate(bool animate)
            => panel3D.AnimatePreview = animate;

        public void SetPreviewObject(IWadObject obj)
            => panel3D.CurrentObject = obj;

        public void ShowPopup(string message, PopupType type)
        {
            // TODO: Replace with WPF popup notification.
        }

        public void UpdateDestinationTree(Wad2 wad)
        {
            treeDestWad.Wad = wad;
            treeDestWad.UpdateContent();
        }

        public void UpdateSourceTree(Wad2 wad)
        {
            treeSourceWad.Wad = wad;
            treeSourceWad.UpdateContent();
        }

        public void UpdateDestinationMetadata()
            => treeDestWad.UpdateMetadata();

        #endregion

        #region Context Menus

        private ContextMenu BuildMoveableContextMenu()
        {
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "Edit animations...", Command = _viewModel.EditAnimationsCommand });
            menu.Items.Add(new MenuItem { Header = "Edit skeleton...", Command = _viewModel.EditSkeletonCommand });
            menu.Items.Add(new MenuItem { Header = "Edit meshes...", Command = new RelayCommand(() => MeshEditor_Click(null, null)) });
            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem { Header = "Change slot...", Command = _viewModel.ChangeSlotCommand });
            menu.Items.Add(new MenuItem { Header = "Delete object", Command = _viewModel.DeleteObjectCommand });
            return menu;
        }

        private ContextMenu BuildStaticContextMenu()
        {
            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "Edit object...", Command = _viewModel.EditItemCommand });
            menu.Items.Add(new MenuItem { Header = "Edit mesh...", Command = new RelayCommand(() => MeshEditor_Click(null, null)) });
            menu.Items.Add(new Separator());
            menu.Items.Add(new MenuItem { Header = "Change slot...", Command = _viewModel.ChangeSlotCommand });
            menu.Items.Add(new MenuItem { Header = "Delete object", Command = _viewModel.DeleteObjectCommand });
            return menu;
        }

        #endregion

        #region Tree View Events

        private void TreeDestWad_SelectedWadObjectIdsChanged(object sender, EventArgs e)
        {
            IWadObjectId currentSelection = treeDestWad.SelectedWadObjectIds.FirstOrDefault();
            if (currentSelection != null)
                _tool.MainSelection = new MainSelection { WadArea = WadArea.Destination, Id = currentSelection };

            if (currentSelection is WadMoveableId)
                treeDestWad.ContextMenu = _contextMenuMoveable;
            else if (currentSelection is WadStaticId)
                treeDestWad.ContextMenu = _contextMenuStatic;
            else
                treeDestWad.ContextMenu = null;

            _viewModel.SelectionActionsEnabled = treeDestWad.SelectedWadObjectIds.Any();
        }

        private void TreeSourceWad_SelectedWadObjectIdsChanged(object sender, EventArgs e)
        {
            IWadObjectId currentSelection = treeSourceWad.SelectedWadObjectIds.FirstOrDefault();
            if (currentSelection != null)
                _tool.MainSelection = new MainSelection { WadArea = WadArea.Source, Id = currentSelection };
        }

        private void TreeDestWad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                WadActions.DeleteObjects(_tool, this, WadArea.Destination, treeDestWad.SelectedWadObjectIds.ToList());
        }

        #endregion

        #region Recent Wads

        private void RefreshRecentWadsList()
        {
            menuOpenRecent.Items.Clear();

            if (Properties.Settings.Default.RecentProjects != null && Properties.Settings.Default.RecentProjects.Count > 0)
            {
                foreach (var fileName in Properties.Settings.Default.RecentProjects)
                {
                    if (fileName == _tool.DestinationWad?.FileName)
                        continue;
                    if (!File.Exists(fileName))
                        continue;

                    var item = new MenuItem { Header = fileName };
                    string capturedFileName = fileName;
                    item.Click += (s, e) =>
                    {
                        WadActions.LoadWad(_tool, this, true, capturedFileName);
                        RefreshRecentWadsList();
                    };
                    menuOpenRecent.Items.Add(item);
                }
            }

            menuOpenRecent.Items.Add(new Separator());
            var clearItem = new MenuItem { Header = "Clear recent file list" };
            clearItem.Click += (s, e) =>
            {
                Properties.Settings.Default.RecentProjects.Clear();
                RefreshRecentWadsList();
                Properties.Settings.Default.Save();
            };
            menuOpenRecent.Items.Add(clearItem);

            menuOpenRecent.IsEnabled = menuOpenRecent.Items.Count > 2;
        }

        #endregion

        #region Window Position Save/Load

        private void LoadWindowProperties()
        {
            var config = _tool.Configuration;
            var configType = config.GetType();

            var size = configType.GetProperty("Window_FormMain_Size")?.GetValue(config);
            var pos = configType.GetProperty("Window_FormMain_Position")?.GetValue(config);
            var max = configType.GetProperty("Window_FormMain_Maximized")?.GetValue(config);

            if (size is System.Drawing.Size s)
            {
                Width = s.Width;
                Height = s.Height;
            }

            if (pos is System.Drawing.Point p)
            {
                if (p.X != -1 || p.Y != -1)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    Left = Math.Max(0, p.X);
                    Top = Math.Max(0, p.Y);
                }
                else
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
                WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (max is bool m && m)
                WindowState = WindowState.Maximized;
        }

        private void SaveWindowProperties()
        {
            var config = _tool.Configuration;
            var configType = config.GetType();

            if (WindowState == WindowState.Minimized)
                return;

            configType.GetProperty("Window_FormMain_Maximized")?.SetValue(config, WindowState == WindowState.Maximized);

            if (WindowState != WindowState.Maximized)
            {
                configType.GetProperty("Window_FormMain_Size")?.SetValue(config, new System.Drawing.Size((int)Width, (int)Height));
                configType.GetProperty("Window_FormMain_Position")?.SetValue(config, new System.Drawing.Point((int)Left, (int)Top));
            }

            config.SaveTry();
        }

        #endregion

        #region WinForms Dialog Click Handlers

        private void AnimatedTextures_Click(object sender, RoutedEventArgs e)
        {
            if (_tool.DestinationWad == null)
                return;

            var context = new WadToolAnimatedTexturesContext(_tool, new List<WadTexture>());
            using (var form = new FormAnimatedTextures(
                new PanelTextureMapForAnimations(_tool),
                context,
                _tool.Configuration))
            {
                form.ShowDialog();
            }
        }

        private void MeshEditor_Click(object sender, RoutedEventArgs e)
        {
            if (_tool.DestinationWad == null)
                return;

            var window = new MeshEditorWindow(_tool, DeviceManager.DefaultDeviceManager,
                _tool.MainSelection?.Id ?? null, _tool.DestinationWad);
            window.Owner = this;
            window.ShowDialog();
        }

        private void ConvertToTEN_Click(object sender, RoutedEventArgs e)
        {
            var dest = WadActions.ConvertWad2ToTombEngine(_tool, this, _tool.DestinationWad);
            if (WadActions.SaveWad(_tool, this, dest, true))
            {
                WadActions.LoadWad(_tool, this, true, dest.FileName);
                ShowPopup("Destination wad was converted and saved to " + dest.FileName + ".", PopupType.Info);
            }
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            using (var form = new FormOptions(_tool))
                form.ShowDialog();

            _tool.UndoManager.Resize(_tool.Configuration.AnimationEditor_UndoDepth);
            panel3D.AnimatePreview = _tool.Configuration.RenderingItem_Animate;
            panel3D.Render();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            using (var form = new FormAbout(Properties.Resources.AboutScreen_800))
                form.ShowDialog();
        }

        private void RefLevelLabel_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => _viewModel.RefLevelLabelClickCommand.Execute(null);

        #endregion

        #region Debug Menu

        private void DebugAction0_Click(object sender, RoutedEventArgs e) { }
        private void DebugAction1_Click(object sender, RoutedEventArgs e) { }
        private void DebugAction2_Click(object sender, RoutedEventArgs e) { }
        private void DebugAction3_Click(object sender, RoutedEventArgs e) { }
        private void DebugAction4_Click(object sender, RoutedEventArgs e) { }
        private void DebugAction5_Click(object sender, RoutedEventArgs e) => TrCatalog.LoadCatalog("Editor\\TRCatalog.xml");
        private void DebugAction6_Click(object sender, RoutedEventArgs e) { }
        private void DebugAction7_Click(object sender, RoutedEventArgs e) { }
        private void DebugAction8_Click(object sender, RoutedEventArgs e) { }

        private void DebugAction9_Click(object sender, RoutedEventArgs e)
        {
            var window = new MeshEditorWindow(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad);
            window.Owner = this;
            window.ShowDialog();
        }

        #endregion

        #region Window Events

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var result = _viewModel.CheckIfSaved();
            if (result == System.Windows.Forms.DialogResult.Yes)
                WadActions.SaveWad(_tool, this, _tool.DestinationWad, false);
            else if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            SaveWindowProperties();
            _viewModel.Cleanup();
            panel3D.DisposeItemPreview();

            // Shut down the WPF dispatcher to allow the process to exit.
            Dispatcher.InvokeShutdown();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Opacity = 1;
            ContentRendered -= Window_ContentRendered;
        }

        #endregion
    }
}
