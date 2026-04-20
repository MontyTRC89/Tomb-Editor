using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DarkUI.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool.MeshEditor
{
    public partial class MeshEditorViewModel : ObservableObject
    {
        private readonly WadToolClass _tool;
        private readonly DeviceManager _deviceManager;
        private readonly Wad2 _wad;

        private bool _readingValues;
        private bool _unsavedChanges;
        private MeshTreeNode _currentNode;

        // Preserve user-loaded textures until user leaves editor.
        public List<WadTexture> UserTextures { get; } = new List<WadTexture>();

        public WadToolClass Tool => _tool;
        public DeviceManager DeviceManager => _deviceManager;
        public Wad2 Wad => _wad;
        public WadMesh SelectedMesh { get; set; }

        #region Observable Properties

        [ObservableProperty] private string _windowTitle = "Mesh editor";
        [ObservableProperty] private string _statusText = "";
        [ObservableProperty] private bool _showEditingTools = true;
        [ObservableProperty] private bool _isTreeMode;
        [ObservableProperty] private bool _hasMesh;
        [ObservableProperty] private bool _canExport;
        [ObservableProperty] private bool _canImport;
        [ObservableProperty] private bool _canRename;
        [ObservableProperty] private bool _undoEnabled;
        [ObservableProperty] private bool _redoEnabled;

        // Editing mode.
        [ObservableProperty] private int _editingModeIndex;
        [ObservableProperty] private string _extraCheckboxText = "Show all";
        [ObservableProperty] private bool _extraCheckboxChecked;

        // Rendering toggles.
        [ObservableProperty] private bool _wireframeChecked;
        [ObservableProperty] private bool _alphaTestChecked;
        [ObservableProperty] private bool _bilinearChecked;
        [ObservableProperty] private bool _gridChecked;
        [ObservableProperty] private bool _hideChecked;
        [ObservableProperty] private bool _hideEnabled;

        // Face attributes.
        [ObservableProperty] private bool _textureChecked;
        [ObservableProperty] private bool _blendChecked;
        [ObservableProperty] private bool _sheenChecked;
        [ObservableProperty] private bool _doubleSideChecked;
        [ObservableProperty] private int _blendModeIndex;
        [ObservableProperty] private decimal _shineStrength;

        // Vertex remap.
        [ObservableProperty] private decimal _vertexNum;
        [ObservableProperty] private bool _remapEnabled;

        // Vertex effects.
        [ObservableProperty] private decimal _glowValue;
        [ObservableProperty] private decimal _moveValue;
        [ObservableProperty] private bool _previewChecked;

        // Vertex weights.
        [ObservableProperty] private decimal _weightIndex1;
        [ObservableProperty] private decimal _weightIndex2;
        [ObservableProperty] private decimal _weightIndex3;
        [ObservableProperty] private decimal _weightIndex4;
        [ObservableProperty] private decimal _weightValue1;
        [ObservableProperty] private decimal _weightValue2;
        [ObservableProperty] private decimal _weightValue3;
        [ObservableProperty] private decimal _weightValue4;

        // Sphere.
        [ObservableProperty] private decimal _sphereX;
        [ObservableProperty] private decimal _sphereY;
        [ObservableProperty] private decimal _sphereZ;
        [ObservableProperty] private decimal _sphereRadius;

        // Texture panel visibility.
        [ObservableProperty] private bool _showTextureTools;
        [ObservableProperty] private bool _materialEditorEnabled;

        #endregion

        #region Blend Modes

        public List<string> BlendModes { get; }

        #endregion

        public MeshEditorViewModel(WadToolClass tool, DeviceManager deviceManager, Wad2 wad)
        {
            _tool = tool;
            _deviceManager = deviceManager;
            _wad = wad;

            BlendModes = TextureExtensions.BlendModeUserNames(tool.DestinationWad.GameVersion);
            ShowTextureTools = wad.GameVersion == TRVersion.Game.TombEngine;
            EditingModeIndex = 0;
            TextureChecked = true;
            BlendChecked = true;
            SheenChecked = true;

            ReadConfig();
        }

        public void ReadConfig()
        {
            AlphaTestChecked = _tool.Configuration.MeshEditor_AlphaTest;
            GridChecked = _tool.Configuration.MeshEditor_DrawGrid;
            BilinearChecked = _tool.Configuration.MeshEditor_Bilinear;
            WireframeChecked = _tool.Configuration.MeshEditor_Wireframe;
        }

        public MeshEditingMode EditingMode => (MeshEditingMode)(EditingModeIndex + 1);

        partial void OnEditingModeIndexChanged(int value)
        {
            PreviewChecked = false;
            UpdateExtraCheckboxText();
        }

        private void UpdateExtraCheckboxText()
        {
            ExtraCheckboxText = EditingMode switch
            {
                MeshEditingMode.FaceAttributes => "Show sheen",
                MeshEditingMode.VertexColorsAndNormals => "Show all normals",
                MeshEditingMode.VertexEffects => "Show all values",
                MeshEditingMode.VertexRemap => "Show all numbers",
                MeshEditingMode.VertexWeights => "Show all weights",
                MeshEditingMode.Sphere => "Show gizmo",
                _ => "Show all"
            };
        }

        partial void OnWireframeCheckedChanged(bool value)
            => _tool.Configuration.MeshEditor_Wireframe = value;

        partial void OnAlphaTestCheckedChanged(bool value)
            => _tool.Configuration.MeshEditor_AlphaTest = value;

        partial void OnBilinearCheckedChanged(bool value)
            => _tool.Configuration.MeshEditor_Bilinear = value;

        partial void OnGridCheckedChanged(bool value)
            => _tool.Configuration.MeshEditor_DrawGrid = value;

        public void UpdateUI(WadMesh mesh, int currentElement)
        {
            HasMesh = mesh != null && mesh.VertexPositions.Count > 0;
            CanExport = HasMesh;
            CanImport = HasMesh;
            CanRename = HasMesh;
            HideEnabled = mesh != null;
            HideChecked = mesh?.Hidden ?? false;

            RemapEnabled = EditingMode == MeshEditingMode.VertexRemap && currentElement != -1;
            if (RemapEnabled)
                VertexNum = currentElement;

            UpdateStatusText(mesh);
        }

        public void UpdateStatusText(WadMesh mesh)
        {
            var prompt = string.Empty;

            if (mesh != null && mesh.VertexPositions.Count > 0)
            {
                prompt += mesh.VertexPositions.Count + " vertices, " +
                          mesh.Polys.Count + " face" + (mesh.Polys.Count > 1 ? "s" : "");

                if (mesh.Polys.Count < 1024)
                {
                    int textureCount = mesh.TextureAreas.Count;
                    prompt += ", " + textureCount + " texture info" + (textureCount > 1 ? "s" : "");
                }

                prompt += ". ";
            }

            StatusText = prompt;
            WindowTitle = "Mesh editor" + (mesh == null ? "" : " - " + mesh.Name);

            if (!IsTreeMode && _unsavedChanges)
                WindowTitle += " *";
        }

        public void GetSphereValues(WadMesh mesh)
        {
            if (mesh == null || mesh.VertexPositions.Count == 0)
                return;

            _readingValues = true;
            SphereX = (decimal)mesh.BoundingSphere.Center.X;
            SphereY = (decimal)mesh.BoundingSphere.Center.Y;
            SphereZ = (decimal)mesh.BoundingSphere.Center.Z;
            SphereRadius = (decimal)Math.Abs(mesh.BoundingSphere.Radius);
            _readingValues = false;
        }

        public void GetWeightValues(WadMesh mesh, int index)
        {
            if (mesh == null || index < 0)
                return;

            _readingValues = true;
            WeightIndex1 = (decimal)mesh.VertexWeights[index].Index[0];
            WeightIndex2 = (decimal)mesh.VertexWeights[index].Index[1];
            WeightIndex3 = (decimal)mesh.VertexWeights[index].Index[2];
            WeightIndex4 = (decimal)mesh.VertexWeights[index].Index[3];
            WeightValue1 = (decimal)mesh.VertexWeights[index].Weight[0];
            WeightValue2 = (decimal)mesh.VertexWeights[index].Weight[1];
            WeightValue3 = (decimal)mesh.VertexWeights[index].Weight[2];
            WeightValue4 = (decimal)mesh.VertexWeights[index].Weight[3];
            _readingValues = false;
        }

        public void SetWeightValues(WadMesh mesh, int index)
        {
            var weight = new VertexWeight();
            weight.Index[0] = (int)WeightIndex1;
            weight.Index[1] = (int)WeightIndex2;
            weight.Index[2] = (int)WeightIndex3;
            weight.Index[3] = (int)WeightIndex4;
            weight.Weight[0] = (float)WeightValue1;
            weight.Weight[1] = (float)WeightValue2;
            weight.Weight[2] = (float)WeightValue3;
            weight.Weight[3] = (float)WeightValue4;

            mesh.VertexWeights[index] = weight;
        }

        public bool IsReadingValues => _readingValues;
        public bool UnsavedChanges
        {
            get => _unsavedChanges;
            set => _unsavedChanges = value;
        }

        public MeshTreeNode CurrentNode
        {
            get => _currentNode;
            set => _currentNode = value;
        }

        public void SetSphereValues(WadMesh mesh)
        {
            if (_readingValues || mesh == null)
                return;

            var newCoord = new Vector3((float)SphereX, (float)SphereY, (float)SphereZ);
            mesh.BoundingSphere = new BoundingSphere(newCoord, (float)SphereRadius);
        }
    }

    public class MeshTreeNode
    {
        public IWadObjectId ObjectId { get; set; }
        public int MeshIndex { get; set; }
        public WadMesh WadMesh { get; set; }

        public MeshTreeNode(IWadObjectId obj, int index, WadMesh wadMesh)
        {
            ObjectId = obj;
            MeshIndex = index;
            WadMesh = wadMesh;
        }
    }
}
