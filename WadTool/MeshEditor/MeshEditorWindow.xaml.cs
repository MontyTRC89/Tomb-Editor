using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DarkUI.WPF.CustomControls;
using TombLib;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using WadTool.Controls;

namespace WadTool.MeshEditor
{
    public partial class MeshEditorWindow : Window
    {
        private readonly MeshEditorViewModel _viewModel;
        private readonly WadToolClass _tool;
        private readonly Wad2 _wad;
        private readonly DeviceManager _deviceManager;

        private PanelTextureMap _panelTextureMap;
        private int _currentSearchIndex = -1;

        private readonly PopUpInfo _popup = new PopUpInfo();

        public WadMesh SelectedMesh => _viewModel.SelectedMesh;
        public bool? Result { get; private set; }

        #region Constructors

        public MeshEditorWindow(WadToolClass tool, DeviceManager deviceManager, Wad2 wad)
            : this(tool, deviceManager, wad, null) { }

        public MeshEditorWindow(WadToolClass tool, DeviceManager deviceManager, IWadObjectId obj, Wad2 wad)
            : this(tool, deviceManager, wad, null)
        {
            if (obj == null)
                return;

            // Find and select the matching mesh tree node.
            // This is deferred until Loaded event.
            Loaded += (s, e) => SelectObjectInTree(obj);
        }

        public MeshEditorWindow(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMesh mesh)
        {
            InitializeComponent();

            _tool = tool;
            _wad = wad;
            _deviceManager = deviceManager;

            _viewModel = new MeshEditorViewModel(tool, deviceManager, wad);
            DataContext = _viewModel;
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            _tool.EditorEventRaised += Tool_EditorEventRaised;

            // Initialize texture map panel (WinForms hosted).
            _panelTextureMap = new PanelTextureMap();
            _panelTextureMap.Initialize(_tool);
            _panelTextureMap.SelectedTextureChanged += (s, e) => UpdateStatusLabel();
            hostTextureMap.Child = _panelTextureMap;

            // Initialize 3D rendering panel.
            panelMesh.InitializeRendering(_tool, _deviceManager);

            PrepareUI(mesh);

            Loaded += (s, e) =>
            {
                UpdateModePanels();
                UpdateUI();
                RepopulateTextureList();
            };
        }

        #endregion

        private void ShowPopup(string message, PopupType type)
        {
            var icon = MessageBoxImage.Information;

            switch (type)
            {
                case PopupType.Warning:
                    icon = MessageBoxImage.Warning;
                    break;

                case PopupType.Error:
                    icon = MessageBoxImage.Error;
                    break;
            }

            System.Windows.MessageBox.Show(this, message, "Mesh editor", MessageBoxButton.OK, icon);
        }

        #region Window Events

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_viewModel.IsTreeMode && Result != true && _viewModel.UnsavedChanges)
            {
                var result = System.Windows.MessageBox.Show(
                    "You have unsaved changes. Do you want to save changes to current mesh?",
                    "Confirm", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Result = true;
                        break;
                    case MessageBoxResult.No:
                        Result = false;
                        break;
                    default:
                        e.Cancel = true;
                        return;
                }
            }

            if (_viewModel.IsTreeMode || Result == true)
            {
                SaveCurrentMesh();
                _viewModel.SelectedMesh = panelMesh.Mesh;
                _tool.ToggleUnsavedChanges();
                _tool.WadChanged(WadArea.Destination);
            }

            _tool.UndoManager.ClearAll();
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _tool.EditorEventRaised -= Tool_EditorEventRaised;
            panelMesh.DisposeMeshPanel();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!_viewModel.ShowEditingTools)
                return;

            switch (e.Key)
            {
                case Key.Escape:
                    panelMesh.CurrentElement = -1;
                    _panelTextureMap.SelectedTexture = TextureArea.None;
                    e.Handled = true;
                    break;

                case Key.Z when Keyboard.Modifiers == ModifierKeys.Control:
                    _tool.UndoManager.Undo();
                    RepopulateTextureListIfChanged();
                    e.Handled = true;
                    break;

                case Key.Y when Keyboard.Modifiers == ModifierKeys.Control:
                    _tool.UndoManager.Redo();
                    RepopulateTextureListIfChanged();
                    e.Handled = true;
                    break;

                case Key.F when Keyboard.Modifiers == ModifierKeys.Control:
                    if (panelMesh.EditingMode == MeshEditingMode.FaceAttributes)
                        FindTexture();
                    e.Handled = true;
                    break;

                case Key.OemMinus:
                case Key.OemPlus:
                case Key.Oem3:
                case Key.OemPipe:
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        MirrorTexture();
                    else
                        RotateTexture();
                    e.Handled = true;
                    break;
            }
        }

        #endregion

        #region Editor Events

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.MeshEditorElementChangedEvent)
            {
                var newIndex = (obj as WadToolClass.MeshEditorElementChangedEvent).ElementIndex;

                switch (panelMesh.EditingMode)
                {
                    case MeshEditingMode.VertexRemap:
                        if (newIndex == -1)
                            return;
                        UpdateUI();
                        break;

                    case MeshEditingMode.VertexEffects:
                        if (newIndex == -1)
                            return;
                        if (!panelMesh.Mesh.HasAttributes)
                            GenerateMissingVertexData();

                        if (Keyboard.Modifiers == ModifierKeys.Alt)
                        {
                            _viewModel.GlowValue = panelMesh.Mesh.VertexAttributes[newIndex].Glow;
                            _viewModel.MoveValue = panelMesh.Mesh.VertexAttributes[newIndex].Move;
                        }
                        else
                        {
                            panelMesh.Mesh.VertexAttributes[newIndex] = new VertexAttributes()
                            {
                                Glow = (int)_viewModel.GlowValue,
                                Move = (int)_viewModel.MoveValue
                            };
                            panelMesh.Render();
                        }
                        break;

                    case MeshEditingMode.VertexWeights:
                        if (newIndex == -1)
                            return;
                        if (!panelMesh.Mesh.HasWeights)
                            GenerateMissingVertexData();

                        if (Keyboard.Modifiers == ModifierKeys.Alt)
                            _viewModel.GetWeightValues(panelMesh.Mesh, newIndex);
                        else
                        {
                            _viewModel.SetWeightValues(panelMesh.Mesh, newIndex);
                            panelMesh.Render();
                        }
                        break;

                    case MeshEditingMode.VertexColorsAndNormals:
                        if (newIndex == -1)
                            return;
                        if (!panelMesh.Mesh.HasColors || !panelMesh.Mesh.HasNormals)
                            GenerateMissingVertexData();

                        if (Keyboard.Modifiers == ModifierKeys.Alt)
                        {
                            var color = panelMesh.Mesh.VertexColors[newIndex];
                            panelColor.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                                (byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255)));
                        }
                        else
                        {
                            var brush = panelColor.Background as SolidColorBrush;
                            if (brush != null)
                            {
                                var c = brush.Color;
                                panelMesh.Mesh.VertexColors[newIndex] = new Vector3(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);
                            }
                            panelMesh.Render();
                        }
                        break;

                    case MeshEditingMode.FaceAttributes:
                        if (newIndex == -1)
                            return;
                        HandleFaceAttributeEvent(newIndex);
                        break;

                    case MeshEditingMode.Sphere:
                        _viewModel.GetSphereValues(panelMesh.Mesh);
                        break;
                }

                if (panelMesh.EditingMode != MeshEditingMode.VertexRemap &&
                    panelMesh.EditingMode != MeshEditingMode.Sphere)
                    UpdateUI();
            }

            if (obj is WadToolClass.UndoStackChangedEvent stackEvent)
            {
                _viewModel.UndoEnabled = stackEvent.UndoPossible;
                _viewModel.RedoEnabled = stackEvent.RedoPossible;
                _viewModel.UnsavedChanges = true;
                SaveCurrentMesh();
                UpdateUI();
                UpdateMeshTreeName();
            }

            if (obj is WadToolClass.MessageEvent m)
                ShowPopup(m.Message, m.Type);
        }

        private void HandleFaceAttributeEvent(int newIndex)
        {
            var poly = panelMesh.Mesh.Polys[newIndex];

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (_viewModel.SheenChecked)
                    _viewModel.ShineStrength = (decimal)poly.ShineStrength;

                if (_viewModel.BlendChecked)
                {
                    var bmIndex = poly.Texture.BlendMode.ToUserIndex();
                    if (bmIndex < _viewModel.BlendModes.Count)
                        _viewModel.BlendModeIndex = bmIndex;
                    _viewModel.DoubleSideChecked = poly.Texture.DoubleSided;
                }

                if (_viewModel.TextureChecked)
                    SelectTexture(poly.Texture);
            }
            else
            {
                var currTexture = poly.Texture;

                if (_viewModel.TextureChecked)
                {
                    if (Keyboard.Modifiers == ModifierKeys.None)
                    {
                        if ((_panelTextureMap.VisibleTexture?.IsAvailable ?? false) &&
                            _panelTextureMap.SelectedTexture != TextureArea.None)
                        {
                            currTexture = _panelTextureMap.SelectedTexture;
                            if (poly.IsTriangle)
                                currTexture.TexCoord3 = currTexture.TexCoord2;
                        }
                    }
                    else
                    {
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                            currTexture.Mirror(poly.IsTriangle);
                        else if (Keyboard.Modifiers == ModifierKeys.Shift)
                            poly.Rotate(1, poly.IsTriangle);
                    }
                }

                if (_viewModel.SheenChecked)
                    poly.ShineStrength = (byte)_viewModel.ShineStrength;

                if (_viewModel.BlendChecked)
                {
                    currTexture.BlendMode = TextureExtensions.ToBlendMode(_viewModel.BlendModeIndex);
                    currTexture.DoubleSided = _viewModel.DoubleSideChecked;
                }

                poly.Texture = currTexture;
                panelMesh.Mesh.Polys[newIndex] = poly;
                panelMesh.Render();
            }
        }

        #endregion

        #region UI Helpers

        private void PrepareUI(WadMesh mesh)
        {
            if (mesh == null)
            {
                _viewModel.IsTreeMode = true;
                PopulateMeshTree();
            }
            else
            {
                _viewModel.IsTreeMode = false;
                panelMesh.Mesh = mesh;
                _viewModel.GetSphereValues(panelMesh.Mesh);
            }
        }

        private void PopulateMeshTree()
        {
            lstMeshes.Items.Clear();

            var moveablesHeader = new AlternatingTreeViewItem { Header = "Moveables", IsExpanded = false };
            foreach (var moveable in _wad.Moveables)
            {
                var moveableNode = new AlternatingTreeViewItem { Header = moveable.Key.ToString(_wad.GameVersion) };
                for (int i = 0; i < moveable.Value.Meshes.Count(); i++)
                {
                    var wadMesh = moveable.Value.Meshes.ElementAt(i);
                    var node = new AlternatingTreeViewItem { Header = wadMesh.Name, Tag = new MeshTreeNode(moveable.Key, i, wadMesh) };
                    moveableNode.Items.Add(node);
                }

                if (moveable.Value.Skin != null)
                {
                    var wadMesh = moveable.Value.Skin;
                    var node = new AlternatingTreeViewItem
                    {
                        Header = wadMesh.Name,
                        Tag = new MeshTreeNode(moveable.Key, moveable.Value.Meshes.Count(), wadMesh)
                    };
                    moveableNode.Items.Add(node);
                }

                moveablesHeader.Items.Add(moveableNode);
            }
            lstMeshes.Items.Add(moveablesHeader);

            var staticsHeader = new AlternatingTreeViewItem { Header = "Statics", IsExpanded = false };
            foreach (var stat in _wad.Statics)
            {
                var staticNode = new AlternatingTreeViewItem { Header = stat.Key.ToString(_wad.GameVersion) };
                var wadMesh = stat.Value.Mesh;
                var node = new AlternatingTreeViewItem { Header = wadMesh.Name, Tag = new MeshTreeNode(stat.Key, 0, wadMesh) };
                staticNode.Items.Add(node);
                staticsHeader.Items.Add(staticNode);
            }
            lstMeshes.Items.Add(staticsHeader);
        }

        private void SelectObjectInTree(IWadObjectId obj)
        {
            if (obj == null)
                return;

            bool isStatic = obj is WadStaticId;
            var allNodes = GetAllTreeViewItems(lstMeshes);

            foreach (var node in allNodes)
            {
                if (node.Tag is not MeshTreeNode meshNode)
                    continue;

                var objectId = meshNode.ObjectId;
                if (isStatic != (objectId is WadStaticId))
                    continue;

                if ((isStatic && ((WadStaticId)obj).TypeId == ((WadStaticId)objectId).TypeId) ||
                    (!isStatic && ((WadMoveableId)obj).TypeId == ((WadMoveableId)objectId).TypeId))
                {
                    node.IsSelected = true;
                    node.BringIntoView();
                    return;
                }
            }
        }

        private List<TreeViewItem> GetAllTreeViewItems(ItemsControl parent)
        {
            var result = new List<TreeViewItem>();
            foreach (var item in parent.Items)
            {
                if (parent.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)
                {
                    result.Add(tvi);
                    result.AddRange(GetAllTreeViewItems(tvi));
                }
                else if (item is TreeViewItem directTvi)
                {
                    result.Add(directTvi);
                    result.AddRange(GetAllTreeViewItems(directTvi));
                }
            }
            return result;
        }

        private void UpdateUI()
        {
            _viewModel.UpdateUI(panelMesh.Mesh, panelMesh.CurrentElement);
            panelMesh.EditingMode = _viewModel.EditingMode;
            panelMesh.WireframeMode = _viewModel.WireframeChecked;
            panelMesh.AlphaTest = _viewModel.AlphaTestChecked;
            panelMesh.DrawExtraInfo = _viewModel.ExtraCheckboxChecked;
            panelMesh.DrawGrid = _viewModel.GridChecked;
            UpdateModePanels();
            panelMesh.Render();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MeshEditorViewModel.EditingModeIndex):
                    panelMesh.EditingMode = _viewModel.EditingMode;
                    UpdateModePanels();
                    UpdateUI();
                    break;

                case nameof(MeshEditorViewModel.ExtraCheckboxChecked):
                    panelMesh.DrawExtraInfo = _viewModel.ExtraCheckboxChecked;
                    panelMesh.Render();
                    break;

                case nameof(MeshEditorViewModel.GridChecked):
                    panelMesh.DrawGrid = _viewModel.GridChecked;
                    panelMesh.Render();
                    break;

                case nameof(MeshEditorViewModel.WireframeChecked):
                    panelMesh.WireframeMode = _viewModel.WireframeChecked;
                    panelMesh.Render();
                    break;

                case nameof(MeshEditorViewModel.AlphaTestChecked):
                    panelMesh.AlphaTest = _viewModel.AlphaTestChecked;
                    panelMesh.Render();
                    break;

                case nameof(MeshEditorViewModel.BilinearChecked):
                    panelMesh.Bilinear = _viewModel.BilinearChecked;
                    panelMesh.Render();
                    break;
            }
        }

        private void UpdateModePanels()
        {
            if (panelFaceAttributes == null)
                return;

            panelFaceAttributes.Visibility = Visibility.Collapsed;
            panelVertexRemap.Visibility = Visibility.Collapsed;
            panelVertexColorsAndNormals.Visibility = Visibility.Collapsed;
            panelVertexEffects.Visibility = Visibility.Collapsed;
            panelVertexWeights.Visibility = Visibility.Collapsed;
            panelSphere.Visibility = Visibility.Collapsed;

            switch (_viewModel.EditingMode)
            {
                case MeshEditingMode.FaceAttributes:
                    panelFaceAttributes.Visibility = Visibility.Visible;
                    break;

                case MeshEditingMode.VertexRemap:
                    panelVertexRemap.Visibility = Visibility.Visible;
                    break;

                case MeshEditingMode.VertexColorsAndNormals:
                    panelVertexColorsAndNormals.Visibility = Visibility.Visible;
                    break;

                case MeshEditingMode.VertexEffects:
                    panelVertexEffects.Visibility = Visibility.Visible;
                    break;

                case MeshEditingMode.VertexWeights:
                    panelVertexWeights.Visibility = Visibility.Visible;
                    break;

                case MeshEditingMode.Sphere:
                    panelSphere.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void UpdateStatusLabel()
        {
            _viewModel.UpdateStatusText(panelMesh.Mesh);

            if (_panelTextureMap.SelectedTexture != TextureArea.None)
            {
                var quad = _panelTextureMap.SelectedTexture.GetRect();
                _viewModel.StatusText += "Selected texture: " + quad.Start + " to " + quad.End;
            }
        }

        private void SelectTexture(TextureArea tex)
        {
            _panelTextureMap.ShowTexture(tex);
            if (tex.Texture.IsAvailable && comboCurrentTexture.Items.Contains(tex.Texture))
                comboCurrentTexture.SelectedItem = tex.Texture;
        }

        private bool NoMesh()
            => panelMesh.Mesh == null || panelMesh.Mesh.VertexPositions.Count == 0;

        private void GenerateMissingVertexData()
        {
            if (panelMesh.Mesh.GenerateMissingVertexData())
                ShowPopup("Missing vertex data was automatically generated for this mesh.", PopupType.Info);
        }

        private void UpdateMeshTreeName()
        {
            if (!_viewModel.IsTreeMode || panelMesh.Mesh == null)
                return;

            foreach (var node in GetAllTreeViewItems(lstMeshes).Where(n => n.Tag != null))
            {
                if ((node.Tag as MeshTreeNode)?.WadMesh == panelMesh.Mesh)
                    node.Header = panelMesh.Mesh.Name;
            }
        }

        #endregion

        #region Mesh Tree Operations

        private void ShowSelectedMesh()
        {
            var selected = lstMeshes.SelectedItem as TreeViewItem;
            if (selected == null)
                return;

            var realNode = GetFirstChildNode(selected);
            var newNode = realNode?.Tag as MeshTreeNode;
            if (newNode != null && newNode.WadMesh != panelMesh.Mesh)
            {
                _tool.UndoManager.ClearAll();
                panelMesh.Mesh = newNode.WadMesh;
                _viewModel.CurrentNode = newNode;

                _viewModel.GetSphereValues(panelMesh.Mesh);
                UpdateUI();
                RepopulateTextureList();
            }
        }

        private TreeViewItem GetFirstChildNode(TreeViewItem node)
        {
            if (node.Tag is MeshTreeNode)
                return node;

            if (node.Items.Count == 0)
                return null;

            var firstChild = node.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem
                ?? node.Items[0] as TreeViewItem;

            return firstChild == null ? null : GetFirstChildNode(firstChild);
        }

        private void SaveCurrentMesh()
        {
            if (!_viewModel.IsTreeMode || _viewModel.CurrentNode == null)
                return;

            var obj = _wad.TryGet(_viewModel.CurrentNode.ObjectId);

            if (obj is WadMoveable mov)
            {
                if (mov.Meshes.Count == _viewModel.CurrentNode.MeshIndex)
                {
                    mov.Skin = _viewModel.CurrentNode.WadMesh = panelMesh.Mesh;
                }
                else
                {
                    mov.Meshes[_viewModel.CurrentNode.MeshIndex] =
                    mov.Bones[_viewModel.CurrentNode.MeshIndex].Mesh = _viewModel.CurrentNode.WadMesh = panelMesh.Mesh;
                }

                mov.Version = DataVersion.GetNext();
            }
            else if (obj is WadStatic stat)
            {
                stat.Mesh = _viewModel.CurrentNode.WadMesh = panelMesh.Mesh;
                stat.Version = DataVersion.GetNext();
            }
        }

        #endregion

        #region Texture Management

        private void RepopulateTextureListIfChanged()
        {
            var textures = new HashSet<WadTexture>(panelMesh.Mesh.TextureAreas.Select(t => t.Texture as WadTexture).Distinct());
            if (!_tool.DestinationWad.MeshTexturesUnique.SetEquals(textures))
                RepopulateTextureList();
        }

        private void RepopulateTextureList(bool force = false)
        {
            bool wholeWad = _viewModel.IsTreeMode;

            if (NoMesh() && !wholeWad)
            {
                comboCurrentTexture.Items.Clear();
                return;
            }

            var list = new List<Texture>(_viewModel.UserTextures);

            if (!NoMesh())
                foreach (var poly in panelMesh.Mesh.Polys)
                    if (poly.Texture.Texture.IsAvailable && !list.Exists(t => t == poly.Texture.Texture))
                        list.Add(poly.Texture.Texture);

            if (wholeWad)
            {
                foreach (var mesh in _tool.DestinationWad.MeshesUnique)
                    foreach (var poly in mesh.Polys)
                        if (poly.Texture.Texture.IsAvailable && !list.Exists(t => t == poly.Texture.Texture))
                            list.Add(poly.Texture.Texture);

                foreach (var set in _tool.DestinationWad.AnimatedTextureSets)
                    foreach (var frame in set.Frames)
                        if (frame.Texture.IsAvailable && !list.Exists(t => t == frame.Texture))
                            list.Add(frame.Texture);
            }

            if (wholeWad && !force && comboCurrentTexture.Items.Count == list.Count)
                return;

            comboCurrentTexture.Items.Clear();
            foreach (var tex in list)
                comboCurrentTexture.Items.Add(tex);

            if (list.Count == 0)
            {
                comboCurrentTexture.SelectedIndex = -1;
            }
            else if (_panelTextureMap.SelectedTexture == TextureArea.None || !list.Contains(_panelTextureMap.SelectedTexture.Texture))
            {
                _panelTextureMap.SelectedTexture = TextureArea.None;
                comboCurrentTexture.SelectedIndex = 0;
            }
            else
            {
                comboCurrentTexture.SelectedIndex = list.IndexOf(_panelTextureMap.SelectedTexture.Texture);
            }
        }

        private void MirrorTexture()
        {
            if (_panelTextureMap.VisibleTexture == null || _panelTextureMap.VisibleTexture.IsUnavailable)
                return;

            var tm = _panelTextureMap.SelectedTexture;
            tm.Mirror();
            _panelTextureMap.SelectedTexture = tm;
        }

        private void RotateTexture()
        {
            if (_panelTextureMap.VisibleTexture == null || _panelTextureMap.VisibleTexture.IsUnavailable)
                return;

            var tr = _panelTextureMap.SelectedTexture;
            tr.Rotate(1);
            _panelTextureMap.SelectedTexture = tr;
        }

        private bool CheckTextureSize(ImageC image)
        {
            if (image.Width > 2048 || image.Height > 2048)
            {
                System.Windows.MessageBox.Show(
                    Path.GetFileName(image.FileName) + " is oversized. UV precision loss may occur.\nResize texture up to 2048px and repeat.",
                    "Texture is oversized", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void AddTexture(bool isExternal)
        {
            var paths = LevelFileDialog.BrowseFiles(null, null, null, "Load texture file", ImageC.FileExtensions).ToList();
            if (paths.Count == 0)
                return;

            foreach (var path in paths)
            {
                var image = ImageC.FromFile(path);

                if (!CheckTextureSize(image))
                    continue;

                image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));

                var newTexture = new WadTexture(image);
                if (isExternal)
                    newTexture.AbsolutePath = path;

                if (comboCurrentTexture.Items.Contains(newTexture))
                    continue;

                comboCurrentTexture.Items.Add(newTexture);
                comboCurrentTexture.SelectedItem = newTexture;

                _viewModel.UserTextures.Add(newTexture);
            }
        }

        private void ReplaceTexture(bool isExternal)
        {
            if (_panelTextureMap.VisibleTexture == null || _panelTextureMap.VisibleTexture.IsUnavailable)
            {
                ShowPopup("Unable to replace texture.\nSelected texture is invalid.", PopupType.Error);
                return;
            }

            var path = LevelFileDialog.BrowseFile(null, null, null, "Load texture file", ImageC.FileExtensions, null, false);
            if (string.IsNullOrEmpty(path))
                return;

            var image = ImageC.FromFile(path);
            if (!CheckTextureSize(image))
                return;

            image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0));
            if (!isExternal)
                image.FileName = string.Empty;

            var newTexture = new WadTexture(image);
            newTexture.AbsolutePath = image.FileName;

            if (!_viewModel.IsTreeMode)
            {
                _tool.UndoManager.PushMeshChanged(panelMesh);

                for (int i = 0; i < panelMesh.Mesh.Polys.Count; i++)
                {
                    if (panelMesh.Mesh.Polys[i].Texture.Texture == _panelTextureMap.VisibleTexture)
                    {
                        var newPoly = panelMesh.Mesh.Polys[i];
                        newPoly.Texture.Texture = newTexture;
                        panelMesh.Mesh.Polys[i] = newPoly;
                    }
                }
            }
            else
            {
                bool textureIsUsed = false;

                foreach (var tex in _tool.DestinationWad.MeshTexInfosUnique)
                {
                    if (tex.Texture == _panelTextureMap.VisibleTexture)
                    {
                        textureIsUsed = true;
                        for (int i = 0; i < 4; i++)
                            if (tex.TexCoords[i].X > image.Width || tex.TexCoords[i].Y > image.Height)
                            {
                                ShowPopup("New texture is smaller than existing one.\nPlease specify another texture.", PopupType.Error);
                                return;
                            }
                    }
                }

                if (textureIsUsed && System.Windows.MessageBox.Show(
                    "Replacing current texture will affect all meshes using it.\nThis action can't be undone. Continue?",
                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                foreach (var moveable in _tool.DestinationWad.Moveables.Values)
                {
                    var meshList = new List<WadMesh>(moveable.Meshes);
                    if (moveable.Skin != null)
                        meshList.Add(moveable.Skin);

                    foreach (var mesh in meshList)
                        for (int i = 0; i < mesh.Polys.Count; i++)
                            if (mesh.Polys[i].Texture.Texture == _panelTextureMap.VisibleTexture)
                            {
                                var newPoly = mesh.Polys[i];
                                newPoly.Texture.Texture = newTexture;
                                mesh.Polys[i] = newPoly;
                            }
                }

                foreach (var stat in _tool.DestinationWad.Statics.Values)
                    for (int i = 0; i < stat.Mesh.Polys.Count; i++)
                        if (stat.Mesh.Polys[i].Texture.Texture == _panelTextureMap.VisibleTexture)
                        {
                            var newPoly = stat.Mesh.Polys[i];
                            newPoly.Texture.Texture = newTexture;
                            stat.Mesh.Polys[i] = newPoly;
                        }
            }

            RepopulateTextureList(true);
            UpdateUI();

            if (_panelTextureMap.SelectedTexture == TextureArea.None)
                return;

            var newSelectedTexture = _panelTextureMap.SelectedTexture;
            newSelectedTexture.Texture = newTexture;
            SelectTexture(newSelectedTexture);
        }

        private void FindTexture()
        {
            if (!_viewModel.IsTreeMode)
                return;

            if (_panelTextureMap.SelectedTexture == TextureArea.None)
            {
                ShowPopup("Please select valid texture area.", PopupType.Error);
                return;
            }

            var validNodes = GetAllTreeViewItems(lstMeshes).Where(node => node.Tag is MeshTreeNode).ToList();
            var selectedNode = lstMeshes.SelectedItem as TreeViewItem;
            int lastNodeIndex = selectedNode != null ? validNodes.IndexOf(selectedNode) + 1 : 0;
            bool searchRestarted = false;

            if (lastNodeIndex == validNodes.Count)
                lastNodeIndex = 0;

            for (int i = lastNodeIndex; i < validNodes.Count; i++)
            {
                if (i == (validNodes.Count - 1) && lastNodeIndex > 0 && !searchRestarted)
                {
                    i = 0;
                    searchRestarted = true;
                }

                var node = validNodes[i];
                var mesh = (node.Tag as MeshTreeNode).WadMesh;
                var index = mesh.Polys.IndexOf(p => p.Texture.Texture == _panelTextureMap.VisibleTexture &&
                                                    p.Texture.GetRect().Intersects(_panelTextureMap.SelectedTexture.GetRect()));
                if (index != -1)
                {
                    node.IsSelected = true;
                    node.BringIntoView();
                    panelMesh.SelectElement(index, true);
                    return;
                }
            }

            ShowPopup("No meshes with any textures from enclosed area were found.", PopupType.Info);
        }

        #endregion

        #region Vertex Remap and AutoFit

        private class FaceEdge : IEquatable<FaceEdge>
        {
            public int[] P { get; set; }
            public bool Equals(FaceEdge other) => (other.P[0] == P[0] && other.P[1] == P[1]) || (other.P[1] == P[0] && other.P[0] == P[1]);
            public override int GetHashCode() => P[0].GetHashCode() ^ P[1].GetHashCode();
        }

        private void RemapSelectedVertex()
        {
            if (NoMesh() || panelMesh.CurrentElement == -1)
                return;

            if (_viewModel.VertexNum == panelMesh.CurrentElement)
            {
                ShowPopup("Please specify other vertex number.", PopupType.Error);
                return;
            }

            var newVertexIndex = (int)_viewModel.VertexNum;
            if (newVertexIndex >= panelMesh.Mesh.VertexPositions.Count)
            {
                ShowPopup("Please specify index between 0 and " + (panelMesh.Mesh.VertexPositions.Count - 1) + ".", PopupType.Error);
                _viewModel.VertexNum = panelMesh.CurrentElement;
                return;
            }

            _tool.UndoManager.PushMeshChanged(panelMesh);
            var count = RemapSelectedVertex(panelMesh.CurrentElement, newVertexIndex);

            if (count > 0)
            {
                _tool.ToggleUnsavedChanges();
                var message = "Successfully replaced vertex " + panelMesh.CurrentElement + " with " + newVertexIndex + " in " + count + " faces.";

                if (newVertexIndex > panelMesh.SafeVertexRemapLimit)
                {
                    message += "\nSpecified vertex number is out of recommended bounds. Glitches may happen in game.";
                    ShowPopup(message, PopupType.Warning);
                }
                else
                    ShowPopup(message, PopupType.Info);

                panelMesh.CurrentElement = newVertexIndex;
            }
        }

        private int RemapSelectedVertex(int oldIndex, int newIndex)
        {
            if (oldIndex >= panelMesh.Mesh.VertexPositions.Count || newIndex >= panelMesh.Mesh.VertexPositions.Count)
                return 0;

            int count = 0;
            var oldVertex = panelMesh.Mesh.VertexPositions[newIndex];
            panelMesh.Mesh.VertexPositions[newIndex] = panelMesh.Mesh.VertexPositions[oldIndex];
            panelMesh.Mesh.VertexPositions[oldIndex] = oldVertex;

            for (int j = 0; j < panelMesh.Mesh.Polys.Count; j++)
            {
                bool done = false;
                var poly = panelMesh.Mesh.Polys[j];

                if (poly.Index0 == oldIndex) { poly.Index0 = newIndex; done = true; } else if (poly.Index0 == newIndex) { poly.Index0 = oldIndex; done = true; }
                if (poly.Index1 == oldIndex) { poly.Index1 = newIndex; done = true; } else if (poly.Index1 == newIndex) { poly.Index1 = oldIndex; done = true; }
                if (poly.Index2 == oldIndex) { poly.Index2 = newIndex; done = true; } else if (poly.Index2 == newIndex) { poly.Index2 = oldIndex; done = true; }

                if (poly.Shape == WadPolygonShape.Quad)
                    if (poly.Index3 == oldIndex) { poly.Index3 = newIndex; done = true; } else if (poly.Index3 == newIndex) { poly.Index3 = oldIndex; done = true; }

                if (done)
                {
                    panelMesh.Mesh.Polys[j] = poly;
                    count++;
                }
            }

            return count;
        }

        private int AutoFit()
        {
            if (NoMesh())
                return 0;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            var edges = new List<FaceEdge>();
            for (int i = 0; i < panelMesh.Mesh.Polys.Count; i++)
            {
                var poly = panelMesh.Mesh.Polys[i];
                edges.Add(new FaceEdge { P = new int[] { poly.Index0, poly.Index1 } });
                edges.Add(new FaceEdge { P = new int[] { poly.Index1, poly.Index2 } });

                if (poly.IsTriangle)
                    edges.Add(new FaceEdge { P = new int[] { poly.Index2, poly.Index0 } });
                else
                {
                    edges.Add(new FaceEdge { P = new int[] { poly.Index2, poly.Index3 } });
                    edges.Add(new FaceEdge { P = new int[] { poly.Index3, poly.Index0 } });
                }
            }

            var orphans = edges.GroupBy(x => x).Where(g => g.Count() == 1).Select(y => y.Key).ToList();
            var remappedIndexList = new List<int>();
            int count = 0;

            foreach (var orphan in orphans)
                foreach (var point in orphan.P)
                {
                    if (!remappedIndexList.Contains(point) && point > panelMesh.SafeVertexRemapLimit)
                    {
                        while (true)
                        {
                            if (orphans.Any(o => o.P[0] == count || o.P[1] == count))
                                count++;
                            else
                                break;

                            if (count > panelMesh.SafeVertexRemapLimit || count == panelMesh.Mesh.VertexPositions.Count - 1)
                                return count;
                        }

                        RemapSelectedVertex(point, count);
                        remappedIndexList.Add(point);
                        count++;
                    }
                }

            return count;
        }

        #endregion

        #region Search

        private void SearchTree()
        {
            if (string.IsNullOrEmpty(tbSearchMeshes.Text))
                return;

            var nodes = GetAllTreeViewItems(lstMeshes);
            var items = nodes.Select(n => n.Header?.ToString() ?? "").ToList();

            for (int i = _currentSearchIndex + 1; i <= items.Count; i++)
            {
                if (i == items.Count)
                {
                    if (_currentSearchIndex == -1)
                        break;
                    else
                    {
                        i = -1;
                        _currentSearchIndex = -1;
                        continue;
                    }
                }

                if (items[i].IndexOf(tbSearchMeshes.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    _currentSearchIndex = i;
                    break;
                }
            }

            if (_currentSearchIndex != -1 && _currentSearchIndex < nodes.Count)
            {
                nodes[_currentSearchIndex].IsSelected = true;
                nodes[_currentSearchIndex].BringIntoView();
                ShowSelectedMesh();
            }
        }

        #endregion

        #region Button Click Handlers

        private void BtOk_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void BtCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }

        private void ButTbUndo_Click(object sender, RoutedEventArgs e)
        {
            _tool.UndoManager.Undo();
            RepopulateTextureListIfChanged();
        }

        private void RenderOption_Click(object sender, RoutedEventArgs e)
        {
            UpdateUI();
        }

        private void ButTbRedo_Click(object sender, RoutedEventArgs e)
        {
            _tool.UndoManager.Redo();
            RepopulateTextureListIfChanged();
        }

        private void ButTbImport_Click(object sender, RoutedEventArgs e)
        {
            var mesh = WadActions.ImportMesh(_tool, null);
            if (mesh == null)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);
            panelMesh.Mesh = mesh;
            SaveCurrentMesh();
            _viewModel.GetSphereValues(panelMesh.Mesh);
            UpdateUI();
            RepopulateTextureList();
        }

        private void ButTbExport_Click(object sender, RoutedEventArgs e)
        {
            WadActions.ExportMesh(panelMesh.Mesh, _tool, null);
        }

        private void ButTbRename_Click(object sender, RoutedEventArgs e)
        {
            RenameMesh();
        }

        private void ButTbResetCamera_Click(object sender, RoutedEventArgs e)
        {
            panelMesh.ResetCamera();
        }

        private void ButTbFindSelectedTexture_Click(object sender, RoutedEventArgs e)
        {
            FindTexture();
        }

        private void ButTbRotateTexture_Click(object sender, RoutedEventArgs e)
        {
            RotateTexture();
        }

        private void ButTbMirrorTexture_Click(object sender, RoutedEventArgs e)
        {
            MirrorTexture();
        }

        private void ButHide_Click(object sender, RoutedEventArgs e)
        {
            if (panelMesh.Mesh != null)
            {
                panelMesh.Mesh.Hidden = !panelMesh.Mesh.Hidden;
                UpdateUI();
            }
        }

        private void ButApplyToAllFaces_Click(object sender, RoutedEventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.FaceAttributes)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            var currentShinyValue = (byte)_viewModel.ShineStrength;
            var currentBlendMode = TextureExtensions.ToBlendMode(_viewModel.BlendModeIndex);

            for (int i = 0; i < panelMesh.Mesh.Polys.Count; i++)
            {
                var poly = panelMesh.Mesh.Polys[i];

                if (_viewModel.TextureChecked && _panelTextureMap.SelectedTexture != TextureArea.None)
                    poly.Texture = _panelTextureMap.SelectedTexture;

                var texture = poly.Texture;

                if (_viewModel.SheenChecked)
                    poly.ShineStrength = currentShinyValue;

                if (_viewModel.BlendChecked)
                {
                    texture.BlendMode = currentBlendMode;
                    texture.DoubleSided = _viewModel.DoubleSideChecked;
                }

                poly.Texture = texture;
                panelMesh.Mesh.Polys[i] = poly;
            }

            UpdateStatusLabel();
            panelMesh.Render();
        }

        private void ButApplyToAllVertices_Click(object sender, RoutedEventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.VertexEffects)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);
            GenerateMissingVertexData();

            int currentGlow = (int)_viewModel.GlowValue;
            int currentMove = (int)_viewModel.MoveValue;

            for (int i = 0; i < panelMesh.Mesh.VertexPositions.Count; i++)
                panelMesh.Mesh.VertexAttributes[i] = new VertexAttributes() { Glow = currentGlow, Move = currentMove };

            panelMesh.Render();
        }

        private void ButApplyShadesToAllVertices_Click(object sender, RoutedEventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.VertexColorsAndNormals)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);
            GenerateMissingVertexData();

            var brush = panelColor.Background as SolidColorBrush;
            if (brush == null)
                return;

            var c = brush.Color;
            var currentColor = new Vector3(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f);

            for (int i = 0; i < panelMesh.Mesh.VertexPositions.Count; i++)
                panelMesh.Mesh.VertexColors[i] = currentColor;

            panelMesh.Render();
        }

        private void PanelColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            using (var colorDialog = new RealtimeColorDialog())
            {
                var brush = panelColor.Background as SolidColorBrush;
                if (brush != null)
                    colorDialog.Color = System.Drawing.Color.FromArgb(brush.Color.R, brush.Color.G, brush.Color.B);

                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                var c = colorDialog.Color;
                panelColor.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(c.R, c.G, c.B));
            }
        }

        private void ButRecalcNormals_Click(object sender, RoutedEventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.VertexColorsAndNormals)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);
            panelMesh.Mesh.CalculateNormals();
            panelMesh.Render();
        }

        private void ButRecalcNormalsAvg_Click(object sender, RoutedEventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.VertexColorsAndNormals)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);
            panelMesh.Mesh.CalculateNormals(false);
            panelMesh.Render();
        }

        private void ButConvertFromShades_Click(object sender, RoutedEventArgs e)
        {
            if (NoMesh())
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            panelMesh.Mesh.VertexAttributes.Clear();
            if (!panelMesh.Mesh.HasColors)
            {
                panelMesh.Mesh.VertexAttributes = Enumerable.Repeat(new VertexAttributes(), panelMesh.Mesh.VertexPositions.Count).ToList();
            }
            else
            {
                for (int i = 0; i < panelMesh.Mesh.VertexColors.Count; i++)
                {
                    var attr = new VertexAttributes();
                    float luma = panelMesh.Mesh.VertexColors[i].GetLuma();

                    if (luma < 0.5f)
                        attr.Move = (int)(luma * 2.0f * 63.0f);
                    else if (luma < 1.0f)
                        attr.Glow = (int)((luma - 0.5f) * 63.0f);

                    panelMesh.Mesh.VertexAttributes.Add(attr);
                }

                panelMesh.Mesh.VertexColors.Clear();
            }

            panelMesh.Render();
        }

        private void ButPreview_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.PreviewChecked)
                panelMesh.StartPreview();
            else
                panelMesh.StopPreview();
        }

        private void ButFindVertex_Click(object sender, RoutedEventArgs e)
        {
            if (NoMesh())
                return;

            var newVertexIndex = (int)_viewModel.VertexNum;
            if (newVertexIndex >= panelMesh.Mesh.VertexPositions.Count)
            {
                ShowPopup("Please specify index between 0 and " + (panelMesh.Mesh.VertexPositions.Count - 1) + ".", PopupType.Error);
                return;
            }
            panelMesh.CurrentElement = newVertexIndex;
        }

        private void ButRemapVertex_Click(object sender, RoutedEventArgs e)
        {
            RemapSelectedVertex();
        }

        private void NudVertexNum_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                RemapSelectedVertex();
        }

        private void ButAutoFit_Click(object sender, RoutedEventArgs e)
        {
            if (panelMesh.Mesh.VertexPositions.Count < panelMesh.SafeVertexRemapLimit)
            {
                ShowPopup("Vertex count is lower than remap limit. No auto-fitting is needed.", PopupType.Info);
                return;
            }

            int count = AutoFit();
            if (count == 0)
                ShowPopup("No vertices were auto-fitted. Possibly mesh is already remapped or contains no holes.", PopupType.Warning);
            else
                ShowPopup("Auto-fitted " + count + " vertices.", PopupType.Info);

            panelMesh.Render();
        }

        private void NudSphereData_ValueChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.SetSphereValues(panelMesh.Mesh);
            panelMesh.Render();
        }

        private void ButResetSphere_Click(object sender, RoutedEventArgs e)
        {
            _tool.UndoManager.PushMeshChanged(panelMesh);
            panelMesh.Mesh.BoundingSphere = panelMesh.Mesh.CalculateBoundingSphere();
            panelMesh.Render();
            _viewModel.GetSphereValues(panelMesh.Mesh);
        }

        private void ComboCurrentTexture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedTexture = comboCurrentTexture.SelectedItem as Texture;
            _panelTextureMap.ResetVisibleTexture(selectedTexture, true);
            _viewModel.MaterialEditorEnabled = selectedTexture != null && !string.IsNullOrEmpty(selectedTexture.AbsolutePath);
        }

        private void AddEmbeddedTexture_Click(object sender, RoutedEventArgs e)
        {
            AddTexture(false);
        }

        private void AddExternalTexture_Click(object sender, RoutedEventArgs e)
        {
            AddTexture(true);
        }

        private void ReplaceEmbeddedTexture_Click(object sender, RoutedEventArgs e)
        {
            ReplaceTexture(false);
        }

        private void ReplaceExternalTexture_Click(object sender, RoutedEventArgs e)
        {
            ReplaceTexture(true);
        }

        private void ButDeleteTexture_Click(object sender, RoutedEventArgs e)
        {
            if (panelMesh.Mesh.Polys.Any(p => p.Texture.Texture == _panelTextureMap.VisibleTexture))
            {
                ShowPopup("Unable to remove selected texture because it's still used in mesh.", PopupType.Error);
                return;
            }

            if (_tool.DestinationWad.MeshTexturesUnique.Contains(_panelTextureMap.VisibleTexture as WadTexture))
            {
                ShowPopup("Unable to remove selected texture because it's still used in wad.", PopupType.Error);
                return;
            }

            int index = comboCurrentTexture.Items.IndexOf(_panelTextureMap.VisibleTexture);
            if (index != -1)
            {
                if (_viewModel.UserTextures.Contains((WadTexture)_panelTextureMap.VisibleTexture))
                    _viewModel.UserTextures.Remove((WadTexture)_panelTextureMap.VisibleTexture);
                comboCurrentTexture.Items.RemoveAt(index);
            }

            comboCurrentTexture.SelectedIndex = 0;
        }

        private void ButExportTexture_Click(object sender, RoutedEventArgs e)
        {
            if (_panelTextureMap.VisibleTexture == null || _panelTextureMap.VisibleTexture.IsUnavailable)
            {
                ShowPopup("Unable to save texture.\nSelected texture is invalid.", PopupType.Error);
                return;
            }

            using (var fileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                try
                {
                    fileDialog.Filter = ImageC.SaveFileFileExtensions.GetFilter(true);
                    fileDialog.Title = "Choose a texture file name";
                    fileDialog.FileName = Path.GetFileNameWithoutExtension(_panelTextureMap.VisibleTexture.ToString());
                    fileDialog.AddExtension = true;

                    var dialogResult = fileDialog.ShowDialog();
                    if (dialogResult != System.Windows.Forms.DialogResult.OK)
                        return;

                    _panelTextureMap.VisibleTexture.Image.SaveToFile(fileDialog.FileName);
                }
                catch (Exception exc)
                {
                    ShowPopup("Unable to save texture. Exception: \n" + exc, PopupType.Error);
                }
            }
        }

        private void AddTextureMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.ContextMenu == null)
                return;

            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.IsOpen = true;
        }

        private void ButAnimationRanges_Click(object sender, RoutedEventArgs e)
        {
            var context = new WadToolAnimatedTexturesContext(_tool, _viewModel.UserTextures);
            using (var form = new FormAnimatedTextures(
                new PanelTextureMapForAnimations(_tool),
                context,
                _tool.Configuration))
            {
                form.ShowDialog();
            }
        }

        private void ButApplyWeightsToAllVertices_Click(object sender, RoutedEventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.VertexWeights)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);
            GenerateMissingVertexData();

            for (int i = 0; i < panelMesh.Mesh.VertexWeights.Count; i++)
            {
                panelMesh.Mesh.VertexWeights[i].Index[0] = (int)_viewModel.WeightIndex1;
                panelMesh.Mesh.VertexWeights[i].Index[1] = (int)_viewModel.WeightIndex2;
                panelMesh.Mesh.VertexWeights[i].Index[2] = (int)_viewModel.WeightIndex3;
                panelMesh.Mesh.VertexWeights[i].Index[3] = (int)_viewModel.WeightIndex4;
                panelMesh.Mesh.VertexWeights[i].Weight[0] = (float)_viewModel.WeightValue1;
                panelMesh.Mesh.VertexWeights[i].Weight[1] = (float)_viewModel.WeightValue2;
                panelMesh.Mesh.VertexWeights[i].Weight[2] = (float)_viewModel.WeightValue3;
                panelMesh.Mesh.VertexWeights[i].Weight[3] = (float)_viewModel.WeightValue4;
            }

            panelMesh.ColorizeVertexWeights();
            panelMesh.Render();
        }

        private void ButMaterialEditor_Click(object sender, RoutedEventArgs e)
        {
            var texture = comboCurrentTexture.SelectedItem as WadTexture;
            var list = comboCurrentTexture.Items.Cast<Texture>().Where(t => !string.IsNullOrEmpty(t.AbsolutePath));

            using (var form = new FormMaterialEditor(list, _tool.Configuration, texture))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK && form.MaterialChanged)
                    ShowPopup("Material settings for current texture were saved to " + form.MaterialFileName + ".", PopupType.Info);
            }
        }

        private void RenameMesh()
        {
            if (panelMesh.Mesh == null)
                return;

            using (var form = new FormInputBox("Edit mesh name", "Mesh name:", panelMesh.Mesh.Name))
            {
                if (form.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                    return;

                if (string.IsNullOrEmpty(form.Result))
                    return;

                if (panelMesh.Mesh.Name.Equals(form.Result, StringComparison.InvariantCultureIgnoreCase))
                    return;

                _tool.UndoManager.PushMeshChanged(panelMesh);
                panelMesh.Mesh.Name = form.Result;
                SaveCurrentMesh();
                UpdateUI();
                UpdateMeshTreeName();
            }
        }

        private void LstMeshes_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SaveCurrentMesh();
            ShowSelectedMesh();
        }

        private void LstMeshes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = lstMeshes.SelectedItem as TreeViewItem;
            if (selected?.Tag is MeshTreeNode)
                RenameMesh();
        }

        private void TbSearchMeshes_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchTree();
        }

        private void CbEditingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel == null)
                return;

            panelMesh.EditingMode = _viewModel.EditingMode;
            UpdateModePanels();
            UpdateUI();
        }

        private void ExtraOptionChanged(object sender, RoutedEventArgs e)
        {
            panelMesh.DrawExtraInfo = _viewModel.ExtraCheckboxChecked;
            panelMesh.Render();
        }

        private void ButSearchMeshes_Click(object sender, RoutedEventArgs e)
        {
            SearchTree();
        }

        #endregion

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {

        }
    }
}
