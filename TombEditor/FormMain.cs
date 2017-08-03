using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Geometry;
using SharpDX;
using TombEditor.Compilers;
using System.IO;
using System.Diagnostics;
using TombEngine;
using NLog;
using TombEditor.Geometry.IO;
using TombLib.Graphics;

namespace TombEditor
{
    public partial class FormMain : DarkUI.Forms.DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private bool _pressedZorY = false;
        private Editor _editor;
        private DeviceManager _deviceManager = new DeviceManager();


        public FormMain()
        {
            InitializeComponent();
            lightPalette.SelectedColorChanged += delegate 
                {
                    if (!_editor.SelectedObject.HasValue || (_editor.SelectedObject.Value.Type != ObjectInstanceType.Light))
                        return;
                    panelLightColor.BackColor = lightPalette.SelectedColor;
                    _editor.SelectedRoom.Lights[_editor.SelectedObject.Value.Id].Color = colorDialog.Color;
                    _editor.SelectedRoom.BuildGeometry();
                    _editor.SelectedRoom.CalculateLightingForThisRoom();
                    _editor.SelectedRoom.UpdateBuffers();
                    panel3D.Draw();
                };

            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = System.Diagnostics.Debugger.IsAttached;

            // For each control bind its light parameter
            numLightIntensity.LightParameter = LightParameter.Intensity;
            numLightIn.LightParameter = LightParameter.In;
            numLightOut.LightParameter = LightParameter.Out;
            numLightLen.LightParameter = LightParameter.Len;
            numLightCutoff.LightParameter = LightParameter.CutOff;
            numLightDirectionX.LightParameter = LightParameter.DirectionX;
            numLightDirectionY.LightParameter = LightParameter.DirectionY;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initialize controls
            _editor = Editor.Instance;
            _editor.Initialize(panel3D, panel2DGrid, this, _deviceManager);

            _editor.Level = Level.CreateSimpleLevel();
            _editor.SelectedRoom = _editor.Level.Rooms[0];

            panel3D.InitializePanel(_deviceManager);
            panelItem.InitializePanel(_deviceManager);
                   
            // Populate list view
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                    comboRoom.Items.Add(i + ": " + _editor.Level.Rooms[i].Name);
                else
                    comboRoom.Items.Add(i + ": --- Empty room ---");
            }

            // Switch room
            _editor.SelectedRoom = _editor.Level.Rooms[0];
            comboRoom.SelectedIndex = _editor.Level.Rooms.ReferenceIndexOf(_editor.SelectedRoom);
            _editor.ResetCamera();

            // Update 3D view
            but3D_Click(null, null);

            this.Text = "Tomb Editor " + Application.ProductVersion + " - Untitled";

            logger.Info("Tomb Editor is ready :)");
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            panel3D.Draw();
            panelItem.Draw();
        }

        public void DrawPanelMap2D()
        {
            panel2DGrid.Invalidate();
        }

        public void LoadTriggersInUI()
        {
            lstTriggers.Items.Clear();
            
            if ((_editor.Level == null) || !_editor.SelectedSectorAvailable)
                return;

            // Search for unique triggers inside the selected area
            var triggers = new List<int>();
            for (int x = _editor.SelectedSector.X; x <= _editor.SelectedSector.Right; x++)
                for (int z = _editor.SelectedSector.Y; z <= _editor.SelectedSector.Bottom; z++)
                    foreach (int trigger in _editor.SelectedRoom.Blocks[x, z].Triggers)
                        if (!triggers.Contains(trigger))
                            triggers.Add(trigger);

            // Add triggers to listbox
            foreach (int t in triggers)
                lstTriggers.Items.Add(t + " - " + _editor.Level.Triggers[t].ToString());
        }

        public void Update2DGrid()
        {
            panel2DGrid.Invalidate();
        }

        private void butWall_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetWall(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void butBox_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSector, BlockFlags.Box);
        }

        private void butDeath_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSector, BlockFlags.Death);
        }

        private void butMonkey_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSector,  BlockFlags.Monkey);
        }

        private void butPortal_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            if (!EditorActions.AddPortal(_editor.SelectedRoom, _editor.SelectedSector))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Not a valid portal position",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }
        }

        private void butClimbNorth_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleClimb(_editor.SelectedRoom, _editor.SelectedSector, 0);
        }

        private void butClimbEast_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleClimb(_editor.SelectedRoom, _editor.SelectedSector, 1);
        }

        private void butClimbSouth_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleClimb(_editor.SelectedRoom, _editor.SelectedSector, 2);
        }

        private void butClimbWest_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleClimb(_editor.SelectedRoom, _editor.SelectedSector, 3);
        }

        private void butNotWalkableBox_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSector, BlockFlags.NotWalkableFloor);
        }

        private void butFloor_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetFloor(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void butCeiling_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetCeiling(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void butDiagonalFloor_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetDiagonalFloorSplit(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void butDiagonalCeiling_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetDiagonalCeilingSplit(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void butDiagonalWall_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetDiagonalWallSplit(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void butFlagBeetle_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSector, BlockFlags.Beetle);
        }

        private void butFlagTriggerTriggerer_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSector, BlockFlags.TriggerTriggerer);
        }

        private void butAddPointLight_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.ActionPlaceLight_LightType = LightType.Light;
        }

        private void butAddShadow_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.ActionPlaceLight_LightType = LightType.Shadow;
        }

        private void butAddSun_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.ActionPlaceLight_LightType = LightType.Sun;
        }

        private void butAddSpotLight_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.ActionPlaceLight_LightType = LightType.Spot;
        }

        private void butAddEffectLight_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.ActionPlaceLight_LightType = LightType.Effect;
        }

        private void butAddFogBulb_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.ActionPlaceLight_LightType = LightType.FogBulb;
        }

        private void comboRoomType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboRoomType.SelectedIndex)
            {
                case 0:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 0;

                    break;

                case 1:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 1;

                    break;

                case 2:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 2;

                    break;

                case 3:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 3;

                    break;

                case 4:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 4;

                    break;

                case 5:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = true;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 0;

                    break;

                case 6:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = true;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 0;

                    break;

                case 7:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = true;
                    _editor.SelectedRoom.WaterLevel = 0;

                    break;
            }
        }

        private void comboReflection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboReflection.SelectedIndex == 0)
            {
                _editor.SelectedRoom.FlagReflection = false;
                _editor.SelectedRoom.ReflectionLevel = 0;
            }
            else
            {
                _editor.SelectedRoom.FlagReflection = true;
                _editor.SelectedRoom.ReflectionLevel = (short)comboReflection.SelectedIndex;
            }
        }

        private void comboMist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboReflection.SelectedIndex == 0)
            {
                _editor.SelectedRoom.FlagMist = false;
                _editor.SelectedRoom.MistLevel = 0;
            }
            else
            {
                _editor.SelectedRoom.FlagMist = true;
                _editor.SelectedRoom.MistLevel = (short)comboMist.SelectedIndex;
            }
        }

        private void comboRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.Level.Rooms[comboRoom.SelectedIndex] == null)
            {
                _editor.Level.Rooms[comboRoom.SelectedIndex] = new Room(_editor.Level, 20, 20, "room " + comboRoom.SelectedIndex);
                comboRoom.Items[comboRoom.SelectedIndex] =
                    comboRoom.SelectedIndex + ": Room " + comboRoom.SelectedIndex;
                _editor.Level.Rooms[comboRoom.SelectedIndex].BuildGeometry();
                _editor.Level.Rooms[comboRoom.SelectedIndex].CalculateLightingForThisRoom();
                _editor.Level.Rooms[comboRoom.SelectedIndex].UpdateBuffers();
            }

            _editor.ResetSelection();

            var room = _editor.SelectedRoom = _editor.Level.Rooms[comboRoom.SelectedIndex];

            comboRoomType.SelectedIndex = 0;

            if (room.FlagWater)
            {
                comboRoomType.SelectedIndex = room.WaterLevel;
            }

            if (room.FlagRain)
            {
                comboRoomType.SelectedIndex = 5;
            }

            if (room.FlagSnow)
            {
                comboRoomType.SelectedIndex = 6;
            }

            if (room.FlagQuickSand)
            {
                comboRoomType.SelectedIndex = 7;
            }

            panelRoomAmbientLight.BackColor = room.AmbientLight;

            comboMist.SelectedIndex = room.MistLevel;
            comboReflection.SelectedIndex = room.ReflectionLevel;
            comboReverberation.SelectedIndex = (int)room.Reverberation;

            cbFlagCold.Checked = room.FlagCold;
            cbFlagDamage.Checked = room.FlagDamage;
            cbFlagOutside.Checked = room.FlagOutside;
            cbHorizon.Checked = room.FlagHorizon;
            cbNoPathfinding.Checked = room.ExcludeFromPathFinding;

            if (room.Flipped)
            {
                if (room.AlternateRoom != null)
                {
                    comboFlipMap.Enabled = true;
                    comboFlipMap.SelectedIndex = room.AlternateGroup + 1;
                    
                    butFlipMap.Checked = false;
                }
                else
                {
                    comboFlipMap.Enabled = false;
                    comboFlipMap.SelectedIndex = room.AlternateGroup + 1;
                    
                    butFlipMap.Checked = true;
                }
            }
            else
            {
                comboFlipMap.Enabled = true;
                comboFlipMap.SelectedIndex = 0;
                
                butFlipMap.Checked = false;
            }

            CenterCamera();
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();

            UpdateStatusStrip();
        }

        public void SelectRoom(Room room)
        {
            comboRoom.SelectedIndex = _editor.Level.Rooms.ReferenceIndexOf(room);
        }

        private void panelRoomAmbientLight_Click(object sender, EventArgs e)
        {
            Room room = _editor.SelectedRoom;

            colorDialog.Color = room.AmbientLight;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelRoomAmbientLight.BackColor = colorDialog.Color;

            _editor.SelectedRoom.AmbientLight = colorDialog.Color;
            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();
            panel3D.Draw();
        }

        public string BrowseTextureMap()
        {
            if (openFileDialogTextureMap.ShowDialog(this) != DialogResult.OK)
                return "";
            return openFileDialogTextureMap.FileName;
        }

        public string BrowseWAD()
        {
            if (openFileDialogWAD.ShowDialog(this) != DialogResult.OK)
                return "";
            return openFileDialogWAD.FileName;
        }
        private void but3D_Click(object sender, EventArgs e)
        {
            but3D.Checked = true;
            but2D.Checked = false;
            butLightingMode.Checked = false;
            butFaceEdit.Checked = false;

            multiPage1.SelectedIndex = 0;
            _editor.Mode = EditorMode.Geometry;
            _editor.Action = EditorAction.None;
            _editor.SelectedSectorArrow = EditorArrowType.EntireFace;

            _editor.DrawPanel3D();
        }

        private void but2D_Click(object sender, EventArgs e)
        {
            but3D.Checked = false;
            but2D.Checked = true;
            butLightingMode.Checked = false;
            butFaceEdit.Checked = false;

            multiPage1.SelectedIndex = 1;
            _editor.Mode = EditorMode.Map2D;
            _editor.Action = EditorAction.None;
        }

        private void butFaceEdit_Click(object sender, EventArgs e)
        {
            but3D.Checked = false;
            but2D.Checked = false;
            butLightingMode.Checked = false;
            butFaceEdit.Checked = true;

            multiPage1.SelectedIndex = 0;
            _editor.Mode = EditorMode.FaceEdit;
            _editor.Action = EditorAction.None;

            _editor.DrawPanel3D();
        }

        private void butLightingMode_Click(object sender, EventArgs e)
        {
            but3D.Checked = false;
            but2D.Checked = false;
            butLightingMode.Checked = true;
            butFaceEdit.Checked = false;

            multiPage1.SelectedIndex = 0;
            _editor.Mode = EditorMode.Lighting;
            _editor.Action = EditorAction.None;

            _editor.DrawPanel3D();
        }

        private void butCenterCamera_Click(object sender, EventArgs e)
        {
            CenterCamera();

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butDrawPortals_Click(object sender, EventArgs e)
        {
            panel3D.DrawPortals = !panel3D.DrawPortals;
            butDrawPortals.Checked = panel3D.DrawPortals;
            panel3D.Draw();
        }

        public void CenterCamera()
        {
            if (_editor.SelectedRoom == null)
                return;

            panel3D.ResetCamera();
        }

        private void butNoOpacity_Click(object sender, EventArgs e)
        {
            SetPortalOpacity(PortalOpacity.None);
        }

        private void butOpacity1_Click(object sender, EventArgs e)
        {
            SetPortalOpacity(PortalOpacity.Opacity1);
        }

        private void butOpacity2_Click(object sender, EventArgs e)
        {
            SetPortalOpacity(PortalOpacity.Opacity2);
        }

        private void butTextureFloor_Click(object sender, EventArgs e)
        {
            Room room = _editor.SelectedRoom;
            for (int x = 1; x < room.NumXSectors - 1; x++)
            {
                for (int z = 1; z < room.NumZSectors - 1; z++)
                {
                    EditorActions.ApplyTexture(_editor.SelectedRoom, new DrawingPoint(x, z), BlockFaces.Floor);
                    EditorActions.ApplyTexture(_editor.SelectedRoom, new DrawingPoint(x, z), BlockFaces.FloorTriangle2);
                }
            }

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            panel3D.Draw();
        }

        private void butTextureCeiling_Click(object sender, EventArgs e)
        {
            Room room = _editor.SelectedRoom;
            for (int x = 1; x < room.NumXSectors - 1; x++)
            {
                for (int z = 1; z < room.NumZSectors - 1; z++)
                {
                    EditorActions.ApplyTexture(_editor.SelectedRoom, new DrawingPoint(x, z), BlockFaces.Ceiling);
                    EditorActions.ApplyTexture(_editor.SelectedRoom, new DrawingPoint(x, z), BlockFaces.CeilingTriangle2);
                }
            }

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            panel3D.Draw();
        }

        private void butTextureWalls_Click(object sender, EventArgs e)
        {
            Room room = _editor.SelectedRoom;
            for (int x = 0; x < room.NumXSectors; x++)
            {
                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int k = 10; k <= 13; k++)
                    {
                        if (room.Blocks[x, z].Faces[k].Defined)
                            EditorActions.ApplyTexture(_editor.SelectedRoom, new DrawingPoint(x, z), (BlockFaces)k);
                    }
                }
            }

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            panel3D.Draw();
        }

        private void butTransparent_Click(object sender, EventArgs e)
        {
            _editor.Transparent = butTransparent.Checked;
        }

        private void butDoubleSided_Click(object sender, EventArgs e)
        {
            _editor.DoubleSided = butDoubleSided.Checked;
        }

        private void butInvisible_Click(object sender, EventArgs e)
        {
            _editor.InvisiblePolygon = butInvisible.Checked;
        }

        private void SetPortalOpacity(PortalOpacity opacity)
        {
            if (!_editor.SelectedObject.HasValue || (_editor.SelectedObject.Value.Type != ObjectInstanceType.Portal))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have to select a portal first",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            Portal portal = _editor.Level.Portals.First(p => p.Id == _editor.SelectedObject.Value.Id);

            for (int x = portal.X; x < portal.X + portal.NumXBlocks; x++)
                for (int z = portal.Z; z < portal.Z + portal.NumZBlocks; z++)
                {
                    switch (portal.Direction)
                    {
                        case PortalDirection.North:
                            _editor.SelectedRoom.Blocks[x, z].WallOpacity = opacity;
                            break;

                        case PortalDirection.South:
                            _editor.SelectedRoom.Blocks[x, z].WallOpacity = opacity;
                            break;

                        case PortalDirection.East:
                            _editor.SelectedRoom.Blocks[x, z].WallOpacity = opacity;
                            break;

                        case PortalDirection.West:
                            _editor.SelectedRoom.Blocks[x, z].WallOpacity = opacity;
                            break;

                        case PortalDirection.Floor:
                            if (!_editor.SelectedRoom.Blocks[x, z].IsFloorSolid)
                                _editor.SelectedRoom.Blocks[x, z].FloorOpacity = opacity;
                            else
                                _editor.SelectedRoom.Blocks[x, z].FloorOpacity =
                                    PortalOpacity.None;

                            break;

                        case PortalDirection.Ceiling:
                            if (!_editor.SelectedRoom.Blocks[x, z].IsCeilingSolid)
                                _editor.SelectedRoom.Blocks[x, z].CeilingOpacity = opacity;
                            else
                                _editor.SelectedRoom.Blocks[x, z].CeilingOpacity =
                                    PortalOpacity.None;

                            break;
                    }
                }

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();
            _editor.DrawPanel3D();
        }

        private void butAddCamera_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceCamera;
        }

        private void butAddFlybyCamera_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceFlyByCamera;
        }

        private void butAddSoundSource_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceSound;
        }

        private void butAddSink_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceSink;
        }
        
        private void panelLightColor_Click(object sender, EventArgs e)
        {
            if (!_editor.SelectedObject.HasValue || (_editor.SelectedObject.Value.Type != ObjectInstanceType.Light))
                return;

            Light light = _editor.SelectedRoom.Lights[_editor.SelectedObject.Value.Id];

            colorDialog.Color = light.Color;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelLightColor.BackColor = colorDialog.Color;

            _editor.SelectedRoom.Lights[_editor.SelectedObject.Value.Id].Color = colorDialog.Color;
            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();
            panel3D.Draw();
        }

        public void LoadTextureMapInEditor(Level level)
        {
            panelTextureMap.Image = level._textureMap;
            panelTextureMap.Height = level._textureMap.Height;
            panelTextureMap.Invalidate();
        }

        public void UpdateStatusStrip()
        {
            var room = _editor.SelectedRoom;

            // Update room labl
            if (room == null)
                statusStripSelectedRoom.Text = "Selected room: None";
            else
                statusStripSelectedRoom.Text =  "Selected room: " +
                    "ID = " + _editor.Level.Rooms.ReferenceIndexOf(room) + " | " +
                    "X = " + room.Position.X + " | " +
                    "Y = " + room.Position.Y + " | " +
                    "Z = " + room.Position.Z + " | " +
                    "Floor = " + (room.Position.Y + room.GetLowestCorner()) + " | " +
                     "Ceiling = " + (room.Position.Y + room.GetHighestCorner());

            // Update selection
            if ((room == null) || !_editor.SelectedSectorAvailable)
                statusStripSelectionArea.Text = "Selected area: None";
            else
                statusStripSelectionArea.Text = "Selected area: " +
                    "X₀ = " + (room.Position.X + _editor.SelectedSector.X) + " | " +
                    "Z₀ = " + (room.Position.Z + _editor.SelectedSector.Y) + " | " +
                    "X₁ = " + (room.Position.X + _editor.SelectedSector.Right) + " | " +
                    "Z₁ = " + (room.Position.Z + _editor.SelectedSector.Bottom);
        }

        public void LoadWadInInterface()
        {
            // carico il wad, in futuro chiedo all'utente quale file caricare
            //tbPlaceItem.Text = _editor.ObjectIds[0];
            comboItems.Items.Clear();

            // carico gli elementi del wad se esistono
            for (int i = 0; i < _editor.Level.Wad.Moveables.Count; i++)
            {
                comboItems.Items.Add(ObjectNames.GetMovableName((int)(_editor.Level.Wad.Moveables.ElementAt(i).Value.ObjectID)));
            }

            for (int i = 0; i < _editor.Level.Wad.StaticMeshes.Count; i++)
            {
                comboItems.Items.Add(ObjectNames.GetStaticName((int)(_editor.Level.Wad.StaticMeshes.ElementAt(i).Value.ObjectID)));
            }

            comboItems.SelectedIndex = 0;

            SelectItem();
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Your project will be lost. Do you really want to create a new project?",
                    "New project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            // Clean all resources before creating a new level
            _editor.Level?.Dispose();

            // Create a new level
            var level = Level.CreateSimpleLevel();

            _editor.SelectedRoom = null;
            _editor.Level = level;

            panelTextureMap.Image = null;
            panelTextureMap.Invalidate();
            
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                    comboRoom.Items.Add(i + ": " + _editor.Level.Rooms[i].Name);
                else
                    comboRoom.Items.Add(i + ": --- Empty room ---");
            }

            // Switch room
            comboRoom.SelectedIndex = 0;

            _editor.SelectedRoom = level.Rooms[0];
            _editor.ResetCamera();
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();
            
            this.Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - Untitled";
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Stamp

            switch (e.KeyCode)
            {
                case Keys.Z: // Set camera relocation mode (Z on american keyboards, Y on german keyboards)
                    _pressedZorY = true;
                    break;

                case Keys.Escape: // End any action
                    _editor.Action = EditorAction.None;
                    _editor.ResetSelection();
                    break;

                case Keys.C: // Copy
                    if (e.Control)
                        butCopy_Click(null, null);
                    break;

                case Keys.V: // Paste
                    if (e.Control)
                        butPaste_Click(null, null);
                    break;

                case Keys.B: // Stamp
                    if (e.Control)
                        butClone_Click(null, null);
                    break;

                case Keys.Delete: // Delete object
                    if (_editor.SelectedRoom == null)
                        return;
                    if (_editor.SelectedObject != null)
                        EditorActions.DeleteObjectWithWarning(_editor.SelectedRoom, _editor.SelectedObject.Value, this);
                    break;

                case Keys.T: // Add trigger
                    if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                        EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSector, this);
                    return;

                case Keys.O: // Show options dialog
                    if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                        EditorActions.EditObject(_editor.SelectedRoom, _editor.SelectedObject.Value, this);
                    break;

                case Keys.Left: 
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateCone(_editor.SelectedRoom, _editor.SelectedObject.Value, new Vector2(0, -1));
                    break;

                case Keys.Right: 
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateCone(_editor.SelectedRoom, _editor.SelectedObject.Value, new Vector2(0, 1));
                    break;

                case Keys.Up:
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateCone(_editor.SelectedRoom, _editor.SelectedObject.Value, new Vector2(1, 0));
                    break;

                case Keys.Down:
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateCone(_editor.SelectedRoom, _editor.SelectedObject.Value, new Vector2(-1, 0));
                    break;

                case Keys.Q:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, _editor.SelectedSectorArrow, 0, 1, e.Control);
                    break;

                case Keys.A:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, _editor.SelectedSectorArrow, 0, -1, e.Control);
                    break;

                case Keys.W:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, _editor.SelectedSectorArrow, 1, 1, e.Control);
                    break;

                case Keys.S:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, _editor.SelectedSectorArrow, 1, -1, e.Control);
                    break;

                case Keys.E:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, _editor.SelectedSectorArrow, 2, 1, e.Control);
                    break;

                case Keys.D:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, _editor.SelectedSectorArrow, 2, -1, e.Control);
                    break;

                case Keys.R: // Rotate object
                    if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                        EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject.Value, 1, e.Shift);
                    else if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, _editor.SelectedSectorArrow, 3, 1, e.Control);
                    break;

                case Keys.F:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, _editor.SelectedSectorArrow, 3, -1, e.Control);
                    break;

                case Keys.Y: // Set camera relocation mode (Z on american keyboards, Y on german keyboards)
                    _pressedZorY = true;
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, EditorArrowType.DiagonalFloorCorner, 0, 1, e.Control);
                    break;

                case Keys.H:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, EditorArrowType.DiagonalFloorCorner, 0, -1, e.Control);
                    break;

                case Keys.U:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, EditorArrowType.DiagonalCeilingCorner, 1, 1, e.Control);
                    break;

                case Keys.J:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectorAvailable)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSector, EditorArrowType.DiagonalCeilingCorner, 1, -1, e.Control);
                    break;
            }

            // Set camera relocation mode based on previous inputs
            if (e.Alt && _pressedZorY)
            {
                panel3D.Cursor = Cursors.Cross;
                panel2DGrid.Cursor = Cursors.Cross;
                _editor.RelocateCameraActive = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if ((e.KeyCode == Keys.Menu) || (e.KeyCode == Keys.Y) || (e.KeyCode == Keys.Z))
            {
                panel3D.Cursor = Cursors.Arrow;
                panel2DGrid.Cursor = Cursors.Arrow;
                _editor.RelocateCameraActive = false;
            }
            if ((e.KeyCode == Keys.Y) || (e.KeyCode == Keys.Z))
                _pressedZorY = false;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            panel3D.Cursor = Cursors.Arrow;
            panel2DGrid.Cursor = Cursors.Arrow;
            if (_editor != null)
                _editor.RelocateCameraActive = false;
            _pressedZorY = false;
        }
        
        private void loadTextureMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogTextureMap.ShowDialog(this) != DialogResult.OK)
                return;
            LoadTextureMap(openFileDialogTextureMap.FileName);
        }
        
        private void LoadTextureMap(string filename)
        {
            _editor.SelectedTextureIndex = -1;
            _editor.Level.LoadTextureMap(filename, _deviceManager.Device);
            panelTextureMap.Image = _editor.Level._textureMap;
            panelTextureMap.Height = _editor.Level._textureMap.Height;
            panelTextureMap.Invalidate();
        }

        private void textureFloorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butTextureFloor_Click(null, null);
        }

        private void textureCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butTextureCeiling_Click(null, null);
        }

        private void textureWallsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butTextureWalls_Click(null, null);
        }

        private void importConvertTextureToPng_Click(object sender, EventArgs e)
        {
            if (_editor.Level == null)
                return;
            if (_editor.Level.TextureFile == null)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Currently there is no texture loaded to convert it.",
                    "No texture loaded");
                return;
            }

            string pngFilePath = Path.Combine(
                Path.GetDirectoryName(_editor.Level.TextureFile),
                Path.GetFileNameWithoutExtension(_editor.Level.TextureFile) + ".png");

            if (File.Exists(pngFilePath))
            {
                if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                        "There is already a file at \"" + pngFilePath + "\". Continue and overwrite the file?",
                        "File exist already", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                    return;
            }

            logger.Debug("Converting texture map to PNG format");

            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                try
                {
                    //Convert...
                    Bitmap bitmap = TombLib.Graphics.TextureLoad.LoadToBitmap(_editor.Level.TextureFile);
                    try
                    {
                        Utils.ConvertTextureTo256Width(ref bitmap);
                        bitmap.Save(pngFilePath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    finally
                    {
                        bitmap.Dispose();
                    }
                }
                catch (Exception exc)
                {
                    NLog.LogManager.GetCurrentClassLogger().Log(NLog.LogLevel.Error, exc, "There was an error while converting TGA in PNG format.");
                    DarkUI.Forms.DarkMessageBox.ShowError("There was an error while converting TGA in PNG format. " + exc.Message, "Error");
                    return;
                }

                watch.Stop();

                logger.Info("Texture map converted");
                logger.Info("    Elapsed time: " + watch.ElapsedMilliseconds + " ms");
            }

            DarkUI.Forms.DarkMessageBox.ShowInformation(
                "TGA texture map was converted to PNG without errors and saved at \"" + pngFilePath + "\".", "Success");
            _editor.Level.LoadTextureMap(pngFilePath, _deviceManager.Device);
        }

        private void loadWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogWAD.ShowDialog(this) != DialogResult.OK)
                return;

            _editor.Level.LoadWad(openFileDialogWAD.FileName, _deviceManager.Device);
            LoadWadInInterface();

            // MessageBox.Show("WAD was loaded without errors", "Informations", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void comboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboItems.SelectedIndex == -1)
                return;

            SelectItem();
        }

        private void SelectItem()
        {
            panelItem.SelectedItem = CurrentItem;
        }

        private ItemType? CurrentItem
        {
            get
            {
                if (_editor?.Level?.Wad == null)
                    return null;
                if (comboItems.SelectedIndex < _editor.Level.Wad.Moveables.Count)
                    return new ItemType(false, (int)_editor.Level.Wad.WadMoveables
                        .ElementAt(comboItems.SelectedIndex).Value.ObjectID);
                else
                    return new ItemType(true, (int)_editor.Level.Wad.WasStaticMeshes
                        .ElementAt(comboItems.SelectedIndex - _editor.Level.Wad.Moveables.Count).Value.ObjectID);
            }
        }

        private ItemType? GetCurrentItemWithMessage()
        {
            ItemType? result = CurrentItem;
            if (result == null)
                DarkUI.Forms.DarkMessageBox.ShowError("Select an item first", "Error");
            return result;
        }

        private void butAddItem_Click(object sender, EventArgs e)
        {
            var currentItem = GetCurrentItemWithMessage();
            if (currentItem == null)
                return;

            if ((!currentItem.Value.IsStatic) && _editor.SelectedRoom.Flipped && _editor.SelectedRoom.AlternateRoom == null)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't add moveables to a flipped room", "Error");
                return;
            }

            _editor.Action = EditorAction.PlaceItem;
            _editor.ActionPlaceItem_Item = currentItem.Value;
        }

        public void LoadStaticMeshColorInUI()
        {
            if (!_editor.SelectedObject.HasValue || (_editor.SelectedObject.Value.Type != ObjectInstanceType.StaticMesh))
                panelStaticMeshColor.BackColor = System.Drawing.Color.Black;
            else
                panelStaticMeshColor.BackColor = ((StaticMeshInstance)_editor.Level.Objects[_editor.SelectedObject.Value.Id]).Color;
        }

        private void butDeleteRoom_Click(object sender, EventArgs e)
        {
            // Check if is the last room
            int numRooms = _editor.Level.Rooms.Count(r => r != null);

            if (numRooms == 1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You must have at least one room in your level", "Error");
                return;
            }

            // Check if room has portals
            foreach (var p in _editor.Level.Portals)
            {
                if (p.Room != _editor.SelectedRoom && p.AdjoiningRoom != _editor.SelectedRoom)
                    continue;

                DarkUI.Forms.DarkMessageBox.ShowError("You can't delete a room with portals to other rooms.",
                    "Error");
                return;
            }

            // Ask for confirmation
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Do you really want to delete this room? All objects inside room will be deleted and " +
                    "triggers pointing to them will be removed.",
                    "Delete room", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
            {
                return;
            }

            var roomToDelete = _editor.SelectedRoom;

            // Delete the room
            DeleteRoom(_editor.SelectedRoom);

            // Find a valid room
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (ReferenceEquals(_editor.Level.Rooms[i], roomToDelete))
                    continue;

                comboRoom.SelectedIndex = i;
                break;
            }
        }

        private void butCropRoom_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.CropRoom(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void addCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butAddCamera_Click(null, null);
        }

        private void addFlybyCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butAddFlybyCamera_Click(null, null);
        }

        private void addSinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butAddSink_Click(null, null);
        }

        private void addSoundSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butAddSoundSource_Click(null, null);
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Your project will be lost. Do you really want to open an existing project?",
                    "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            if (openFileDialogPRJ2.ShowDialog(this) != DialogResult.OK)
                return;

            Level level = Prj2Loader.LoadFromPrj2(openFileDialogPRJ2.FileName, _deviceManager.Device, this);
            if (level == null)
            {
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "There was an error while opening project file. File may be in use or may be corrupted", "Error");
                return;
            }

            // Clean all resources before creating a new level
            _editor.Level?.Dispose();

            // Set the new level and update UI
            _editor.Level = level;

            LoadWadInInterface();
            LoadTextureMapInEditor(_editor.Level);

            _editor.SelectedRoom = null;
            comboRoom.Items.Clear();

            ReloadRooms();

            // Switch room
            comboRoom.SelectedIndex = 0;

            _editor.SelectedRoom = level.Rooms[0];
            _editor.ResetCamera();
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();
            
            this.Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - " + openFileDialogPRJ2.FileName;
        }

        private void ReloadRooms()
        {
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                {
                    string description = _editor.Level.Rooms[i].Name;
                    if (_editor.Level.Rooms[i].Flipped)
                        description += " (Flipped " +
                                       _editor.Level.Rooms[i].AlternateRoom + ":" +
                                       _editor.Level.Rooms[i].AlternateGroup + ")";
                    comboRoom.Items.Add(i + ": " + description);
                }
                else
                {
                    comboRoom.Items.Add(i + ": --- Empty room ---");
                }
            }
        }

        private void UpdateLightUI()
        {
            if (!_editor.SelectedObject.HasValue || (_editor.SelectedObject.Value.Type != ObjectInstanceType.Light))
            {
                panelLightColor.Enabled = false;
                numLightIntensity.Enabled = false;
                numLightLen.Enabled = false;
                numLightCutoff.Enabled = false;
                numLightDirectionX.Enabled = false;
                numLightDirectionY.Enabled = false;
                numLightIn.Enabled = false;
                numLightOut.Enabled = false;
                return;
            }

            var light = _editor.SelectedRoom.Lights[_editor.SelectedObject.Value.Id];
            switch (light.Type)
            {
                case LightType.Light:
                    panelLightColor.BackColor = light.Color;
                    numLightIn.Value = light.In;
                    numLightOut.Value = light.Out;
                    numLightIntensity.Value = light.Intensity;

                    panelLightColor.Enabled = true;
                    numLightIntensity.Enabled = true;
                    numLightLen.Enabled = false;
                    numLightCutoff.Enabled = false;
                    numLightDirectionX.Enabled = false;
                    numLightDirectionY.Enabled = false;
                    numLightIn.Enabled = true;
                    numLightOut.Enabled = true;

                    break;

                case LightType.Shadow:
                    panelLightColor.BackColor = light.Color;
                    numLightIn.Value = light.In;
                    numLightOut.Value = light.Out;
                    numLightIntensity.Value = light.Intensity;

                    panelLightColor.Enabled = true;
                    numLightIntensity.Enabled = true;
                    numLightLen.Enabled = false;
                    numLightCutoff.Enabled = false;
                    numLightDirectionX.Enabled = false;
                    numLightDirectionY.Enabled = false;
                    numLightIn.Enabled = true;
                    numLightOut.Enabled = true;

                    break;

                case LightType.FogBulb:
                    numLightIn.Value = light.In;
                    numLightOut.Value = light.Out;
                    numLightIntensity.Value = light.Intensity;

                    panelLightColor.Enabled = false;
                    numLightIntensity.Enabled = true;
                    numLightLen.Enabled = false;
                    numLightCutoff.Enabled = false;
                    numLightDirectionX.Enabled = false;
                    numLightDirectionY.Enabled = false;
                    numLightIn.Enabled = true;
                    numLightOut.Enabled = true;

                    break;

                case LightType.Spot:
                    panelLightColor.BackColor = light.Color;
                    numLightIn.Value = light.In;
                    numLightOut.Value = light.Out;
                    numLightIntensity.Value = light.Intensity;
                    numLightLen.Value = light.Len;
                    numLightCutoff.Value = light.Cutoff;
                    numLightDirectionX.Value = light.DirectionX;
                    numLightDirectionY.Value = light.DirectionY;

                    panelLightColor.Enabled = true;
                    numLightIntensity.Enabled = true;
                    numLightLen.Enabled = true;
                    numLightCutoff.Enabled = true;
                    numLightDirectionX.Enabled = true;
                    numLightDirectionY.Enabled = true;
                    numLightIn.Enabled = true;
                    numLightOut.Enabled = true;

                    break;

                case LightType.Sun:
                    panelLightColor.BackColor = light.Color;
                    numLightIntensity.Value = light.Intensity;
                    numLightDirectionX.Value = light.DirectionX;
                    numLightDirectionY.Value = light.DirectionY;

                    panelLightColor.Enabled = true;
                    numLightIntensity.Enabled = true;
                    numLightLen.Enabled = false;
                    numLightCutoff.Enabled = false;
                    numLightDirectionX.Enabled = true;
                    numLightDirectionY.Enabled = true;
                    numLightIn.Enabled = false;
                    numLightOut.Enabled = false;

                    break;

                case LightType.Effect:
                    panelLightColor.BackColor = light.Color;
                    numLightIntensity.Value = light.Intensity;

                    panelLightColor.Enabled = true;
                    numLightIntensity.Enabled = true;
                    numLightLen.Enabled = false;
                    numLightCutoff.Enabled = false;
                    numLightDirectionX.Enabled = false;
                    numLightDirectionY.Enabled = false;
                    numLightIn.Enabled = false;
                    numLightOut.Enabled = false;

                    break;
            }
        }

        public void EditLight()
        {
            UpdateLightUI();
        }

        private void importTRLEPRJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Your project will be lost. Do you really want to open an existing project?",
                    "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            if (openFileDialogPRJ.ShowDialog(this) != DialogResult.OK)
                return;

            using (var form = new FormImportPRJ(_deviceManager))
            {
                form.FileName = openFileDialogPRJ.FileName;
                if (form.ShowDialog() != DialogResult.OK || form.Level == null)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError(
                        "There was an error while importing project file. File may be in use or may be corrupted",
                        "Error");
                    return;
                }

                // Clean all resources before creating a new level
                _editor.Level?.Dispose();

                // Set the new level and update UI
                _editor.Level = form.Level;
            }
            LoadTextureMapInEditor(_editor.Level);

            LoadWadInInterface();
            _editor.SelectedRoom = null;

            comboRoom.Items.Clear();
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                {
                    var roomName = (_editor.Level.Rooms[i].Name == null ? "Room " + i : _editor.Level.Rooms[i].Name);
                    if (_editor.Level.Rooms[i].BaseRoom != null)
                        roomName = "(Flipped of " + _editor.Level.Rooms.ReferenceIndexOf(_editor.Level.Rooms[i].BaseRoom) + ") " + roomName;

                    comboRoom.Items.Add(i + ": " + roomName);
                }
                else
                {
                    comboRoom.Items.Add(i + ": --- Empty room ---");
                }
            }

            // Switch room
            comboRoom.SelectedIndex = 0;

            _editor.SelectedRoom = _editor.Level.Rooms[0];
            _editor.ResetCamera();
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();
            
            this.Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - " + openFileDialogPRJ.FileName;
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.Level.WadFile == "")
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Can't save a project without a WAD", "Error");
                return;
            }

            if (_editor.Level.TextureFile == "")
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Can't save a project without a texture map", "Error");
                return;
            }

            if (saveFileDialogPRJ2.ShowDialog(this) != DialogResult.OK)
                return;

            bool result = Prj2Writer.SaveToPrj2(saveFileDialogPRJ2.FileName, _editor.Level);

            if (!result)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("There was an error while saving project file", "Error");
            }
            else
            {
                DarkUI.Forms.DarkMessageBox.ShowInformation("Project file was saved correctly", "Informations");
                this.Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - " +
                            saveFileDialogPRJ2.FileName;
            }
        }

        private void butRoomUp_Click(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Position += new Vector3(0.0f, 1.0f, 0.0f);

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            foreach (var portal in _editor.Level.Portals)
            {
                if (portal.Room != _editor.SelectedRoom)
                    continue;

                portal.AdjoiningRoom.BuildGeometry();
                portal.AdjoiningRoom.CalculateLightingForThisRoom();
                portal.AdjoiningRoom.UpdateBuffers();
            }

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();
            UpdateStatusStrip();
        }

        private void butRoomDown_Click(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Position += new Vector3(0.0f, -1.0f, 0.0f);

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            foreach (var portal in _editor.Level.Portals)
            {
                if (portal.Room != _editor.SelectedRoom)
                    continue;

                portal.AdjoiningRoom.BuildGeometry();
                portal.AdjoiningRoom.CalculateLightingForThisRoom();
                portal.AdjoiningRoom.UpdateBuffers();
            }

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();
            UpdateStatusStrip();
        }
        
        private void butCompileLevel_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_editor.Level.WadFile))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have not loaded a WAD file", "Error",
                    DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            if (string.IsNullOrEmpty(_editor.Level.TextureFile))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have not loaded a texture map", "Error",
                    DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            using (var form = new FormBuildLevel())
                form.ShowDialog(this);
        }

        private void BuilLevel()
        {
            string baseName = Path.GetFileNameWithoutExtension(_editor.Level.WadFile);

            var comp = new LevelCompilerTr4(_editor.Level, "Game\\Data\\" + baseName + ".tr4");
            comp.CompileLevel();
        }

        private void butCompileLevelAndPlay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_editor.Level.WadFile))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have not loaded a WAD file", "Error",
                    DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            if (string.IsNullOrEmpty(_editor.Level.TextureFile))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have not loaded a texture map", "Error",
                    DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            using (var form = new FormBuildLevel())
            {
                form.LaunchGameAfterCompile = true;
                form.ShowDialog(this);
            }

            var info = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\Game",
                FileName = "tomb4.exe"
            };

            Process.Start(info);
        }

        private void buildLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCompileLevel_Click(null, null);
        }

        private void buildLevelPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCompileLevelAndPlay_Click(null, null);
        }

        private void darkButton15_Click(object sender, EventArgs e)
        {
            using (FormAnimatedTextures form = new FormAnimatedTextures())
                form.ShowDialog(this);
        }

        private void animationRangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            darkButton15_Click(null, null);
        }

        private void butTextureSounds_Click(object sender, EventArgs e)
        {
            using (var form = new FormTextureSounds())
                form.ShowDialog(this);
        }

        private void textureSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butTextureSounds_Click(null, null);
        }

        private void butItemsBack_Click(object sender, EventArgs e)
        {
            if (comboItems.SelectedIndex == 0 || comboItems.Items.Count == 0)
                return;
            comboItems.SelectedIndex = comboItems.SelectedIndex - 1;
            panelItem.Invalidate();
            panelItem.Refresh();
            panelItem.Draw();
        }

        private void butItemsNext_Click(object sender, EventArgs e)
        {
            if (comboItems.SelectedIndex == comboItems.Items.Count - 1 || comboItems.Items.Count == 0)
                return;
            comboItems.SelectedIndex = comboItems.SelectedIndex + 1;
            panelItem.Invalidate();
            panelItem.Refresh();
            panelItem.Draw();
        }

        private void butCopyRoom_Click(object sender, EventArgs e)
        {
            if ((_editor.SelectedRoom == null) || (!_editor.SelectedSectorAvailable))
                return;
            SharpDX.Rectangle area = _editor.SelectedSector;

            // Search the first free room
            short found = -1;
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                    continue;

                found = (short)i;
                break;
            }

            if (found == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            var room = _editor.SelectedRoom;

            if (room.Flipped)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't copy a flipped room", "Error");
                return;
            }

            byte numXSectors = (byte)(area.Width + 3);
            byte numZSectors = (byte)(area.Height + 3);

            var newRoom = new Room(_editor.Level, numXSectors, numZSectors, "Room " + found);

            for (int x = 1; x < numXSectors - 1; x++)
                for (int z = 1; z < numZSectors - 1; z++)
                    newRoom.Blocks[x, z] = room.Blocks[x + area.X - 1, z + area.Y - 1].Clone();

            // Build the geometry of the new room
            newRoom.BuildGeometry();
            newRoom.CalculateLightingForThisRoom();
            newRoom.UpdateBuffers();

            _editor.Level.Rooms[found] = newRoom;

            // Update the UI
            comboRoom.Items[found] = found + ": " + newRoom.Name;
            comboRoom.SelectedIndex = found;

            _editor.CenterCamera();
            _editor.DrawPanel3D();
        }

        private void butSplitRoom_Click(object sender, EventArgs e)
        {
            if ((_editor.SelectedRoom == null) || (!_editor.SelectedSectorAvailable))
                return;
            SharpDX.Rectangle area = _editor.SelectedSector;

            // Search the first free room
            short found = -1;
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                    continue;

                found = (short)i;
                break;
            }

            if (found == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            var room = _editor.SelectedRoom;

            if (room.Flipped)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't split a flipped room", "Error");
                return;
            }

            byte numXSectors = (byte)(area.Width + 3);
            byte numZSectors = (byte)(area.Height + 3);

            var newRoom = new Room(_editor.Level, numXSectors, numZSectors, "Room " + found);

            for (int x = 1; x < numXSectors - 1; x++)
                for (int z = 1; z < numZSectors - 1; z++)
                {
                    newRoom.Blocks[x, z] = room.Blocks[x + area.X - 1, z + area.Y - 1].Clone();
                    room.Blocks[x + area.X - 1, z + area.Y - 1].Type = BlockType.Wall;
                }

            // Build the geometry of the new room
            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();

            newRoom.BuildGeometry();
            newRoom.CalculateLightingForThisRoom();
            newRoom.UpdateBuffers();

            _editor.SelectedRoom = room;
            _editor.Level.Rooms[found] = newRoom;

            // Update the UI
            comboRoom.Items[found] = found + ": " + newRoom.Name;
            comboRoom.SelectedIndex = found;

            _editor.CenterCamera();
            _editor.DrawPanel3D();
        }

        private void butEditRoomName_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox())
            {
                form.Title = "Edit room's name";
                form.Message = "Insert the name of this room:";
                form.Value = _editor.SelectedRoom.Name;

                if (form.ShowDialog(this) == DialogResult.Cancel)
                    return;
                if (form.Value == "")
                    return;

                _editor.SelectedRoom.Name = form.Value;
                comboRoom.Items[comboRoom.SelectedIndex] = comboRoom.SelectedIndex + ": " + form.Value;
            }
        }

        private bool CheckForRoomAndBlockSelection()
        {
            if ((_editor.SelectedRoom == null) || !_editor.SelectedSectorAvailable)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Please select a valid group of sectors",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return false;
            }
            return true;
        }

        private void smoothRandomFloorUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.SelectedSector, 1);
        }

        private void smoothRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.SelectedSector, -1);
        }

        private void smoothRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.SelectedSector, 1);
        }

        private void smoothRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.SelectedSector, -1);
        }


        private void sharpRandomFloorUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.SelectedSector, 1);
        }

        private void sharpRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.SelectedSector, -1);
        }

        private void sharpRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.SelectedSector, 1);
        }

        private void sharpRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.SelectedSector, -1);
        }

        private void butFlattenFloor_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void butFlattenCeiling_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void flattenFloorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void flattenCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void gridWallsIn3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckForRoomAndBlockSelection())
                EditorActions.GridWalls3(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void gridWallsIn5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.GridWalls5(_editor.SelectedRoom, _editor.SelectedSector);
        }

        private void panelStaticMeshColor_Click(object sender, EventArgs e)
        {
            if (!_editor.SelectedObject.HasValue || (_editor.SelectedObject.Value.Type != ObjectInstanceType.StaticMesh))
                return;

            var instance = (StaticMeshInstance)_editor.Level.Objects[_editor.SelectedObject.Value.Id];

            colorDialog.Color = instance.Color;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelStaticMeshColor.BackColor = colorDialog.Color;
            instance.Color = colorDialog.Color;
            
            panel3D.Draw();
        }
        
        private void butFindItem_Click(object sender, EventArgs e)
        {
            ItemType? currentItem = GetCurrentItemWithMessage();
            if (currentItem == null)
                return;

            // Search for matching objects after the previous one
            ObjectPtr? previousFind = _editor.SelectedObject;
            ObjectInstance instance = _editor.Level.Objects.Values.FindFirstAfterWithWrapAround(
                (obj) => previousFind == obj.ObjectPtr,
                (obj) => (obj is ItemInstance) && ((ItemInstance)obj).ItemType == currentItem.Value);
            
            // Show result
            if (instance == null)
                DarkUI.Forms.DarkMessageBox.ShowInformation("No object of the selected item type found.", "No object found");
            else
                _editor.ShowObject(instance);

        }

        private void butResetSearch_Click(object sender, EventArgs e)
        {
            _editor.ResetSelection();
        }

        private void butDeleteTrigger_Click(object sender, EventArgs e)
        {
            if (lstTriggers.SelectedIndex == -1)
                return;

            int trigger = int.Parse(lstTriggers.Text.Split(' ')[0]);
            string triggerDescription = _editor.Level.Triggers[trigger].ToString();

            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete '" + triggerDescription + "'?",
                    "Delete trigger", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            var triggerObject = _editor.Level.Triggers[trigger];

            for (int x = triggerObject.X; x < triggerObject.X + triggerObject.NumXBlocks; x++)
            {
                for (int z = triggerObject.Z; z < triggerObject.Z + triggerObject.NumZBlocks; z++)
                {
                    triggerObject.Room.Blocks[x, z].Triggers.Remove(trigger);
                }
            }

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.ResetSelection();
            _editor.LoadTriggersInUI();
        }

        private void butEditTrigger_Click(object sender, EventArgs e)
        {
            if (lstTriggers.SelectedIndex == -1)
                return;

            int trigger = int.Parse(lstTriggers.Text.Split(' ')[0]);
            using (var formTrigger = new FormTrigger(_editor.Level, _editor.Level.Triggers[trigger]))
                if (formTrigger.ShowDialog(this) != DialogResult.OK)
                    return;

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            _editor.ResetSelection();
            _editor.LoadTriggersInUI();
        }

        private void findObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butFindItem_Click(null, null);
        }

        private void resetFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butResetSearch_Click(null, null);
        }

        private void comboFlipMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            var room = _editor.SelectedRoom;

            // Delete flipped room
            if (comboFlipMap.SelectedIndex == 0 && room.Flipped)
            {
                // Check if room has portals
                foreach (var p in _editor.Level.Portals)
                {
                    if ((p.Room != _editor.SelectedRoom && p.AdjoiningRoom != _editor.SelectedRoom) ||
                        p.MemberOfFlippedRoom)
                        continue;

                    DarkUI.Forms.DarkMessageBox.ShowError("You can't delete a room with portals to other rooms.",
                        "Error");
                    return;
                }

                // Ask for confirmation
                if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete flipped room?",
                        "Delete flipped room", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                {
                    return;
                }

                DeleteRoom(room.AlternateRoom);

                _editor.SelectedRoom.Flipped = false;
                _editor.SelectedRoom.AlternateRoom = null;
                _editor.SelectedRoom.AlternateGroup = 0;

                return;
            }

            // Change flipped map number, not much to do here
            if (comboFlipMap.SelectedIndex != 0 && room.Flipped)
            {
                _editor.SelectedRoom.AlternateGroup = (short)(comboFlipMap.SelectedIndex - 1);
                //_editor.Level.Rooms[_editor.SelectedRoom.AlternateRoom].FlipGroup = (short)(comboFlipMap.SelectedIndex - 1);
                return;
            }

            // Create a new flipped room
            if (comboFlipMap.SelectedIndex == 0 || room.Flipped)
                return;

            // Search the first free room
            short found = -1;
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] == null)
                {
                    found = (short)i;
                    break;
                }
            }

            if (found == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            // Duplicate portals
            var duplicatedPortals = new Dictionary<Portal, Portal>();

            foreach (var p in _editor.Level.Portals)
            {
                if (p.Room != _editor.SelectedRoom)
                    continue;

                var newPortal = (Portal)p.Clone();
                newPortal.Flipped = true;

                p.Flipped = true;

                duplicatedPortals.Add(p, newPortal);
            }

            int numXSectors = room.NumXSectors;
            int numZSectors = room.NumZSectors;

            string name = "(Flipped of " + _editor.SelectedRoom.ToString() + ") Room " + found;
            var newRoom = new Room(_editor.Level, numXSectors, numZSectors, name);

            for (int x = 0; x < numXSectors; x++)
            {
                for (int z = 0; z < numZSectors; z++)
                {
                    newRoom.Blocks[x, z] = room.Blocks[x, z].Clone();
                    newRoom.Blocks[x, z].FloorPortal = (room.Blocks[x, z].FloorPortal != null
                        ? duplicatedPortals[room.Blocks[x, z].FloorPortal] : null);
                    newRoom.Blocks[x, z].CeilingPortal = (room.Blocks[x, z].CeilingPortal != null
                        ? duplicatedPortals[room.Blocks[x, z].CeilingPortal] : null);
                    newRoom.Blocks[x, z].WallPortal = (room.Blocks[x, z].WallPortal != null
                        ? duplicatedPortals[room.Blocks[x, z].WallPortal] : null);
                }
            }

            for (int i = 0; i < room.Lights.Count; i++)
            {
                newRoom.Lights.Add(room.Lights[i].Clone());
            }

            _editor.SelectedRoom.Flipped = true;
            _editor.SelectedRoom.AlternateGroup = (short)(comboFlipMap.SelectedIndex - 1);
            _editor.SelectedRoom.AlternateRoom = newRoom;

            newRoom.Flipped = true;
            newRoom.AlternateGroup = (short)(comboFlipMap.SelectedIndex - 1);
            newRoom.BaseRoom = _editor.SelectedRoom;

            // Build the geometry of the new room
            newRoom.BuildGeometry();
            newRoom.CalculateLightingForThisRoom();
            newRoom.UpdateBuffers();

            _editor.Level.Rooms[found] = newRoom;

            // Update the UI
            comboRoom.Items[found] = found + ": " + newRoom.Name;
            comboRoom.SelectedIndex = found;
            
            butFlipMap.Checked = true;

            _editor.DrawPanel3D();
        }

        private void DeleteRoom(Room room)
        {
            // Collect all triggers and objects
            var objectsToRemove = new List<int>();
            var triggersToRemove = new List<int>();

            for (int i = 0; i < _editor.Level.Objects.Count; i++)
            {
                var obj = _editor.Level.Objects.ElementAt(i).Value;
                if (!ReferenceEquals(obj.Room, room))
                    continue;

                // We must remove that object. First try to find a trigger.
                for (int j = 0; j < _editor.Level.Triggers.Count; j++)
                {
                    var trigger = _editor.Level.Triggers.ElementAt(j).Value;

                    if (trigger.TargetType == TriggerTargetType.Camera && obj.Type == ObjectInstanceType.Camera &&
                        trigger.Target == obj.Id)
                    {
                        triggersToRemove.Add(trigger.Id);
                    }

                    if (trigger.TargetType == TriggerTargetType.FlyByCamera &&
                        obj.Type == ObjectInstanceType.FlyByCamera &&
                        trigger.Target == ((FlybyCameraInstance)obj).Sequence)
                    {
                        triggersToRemove.Add(trigger.Id);
                    }

                    if (trigger.TargetType == TriggerTargetType.Sink && obj.Type == ObjectInstanceType.Sink &&
                        trigger.Target == obj.Id)
                    {
                        triggersToRemove.Add(trigger.Id);
                    }

                    if (trigger.TargetType == TriggerTargetType.Object && obj.Type == ObjectInstanceType.Moveable &&
                        trigger.Target == obj.Id)
                    {
                        triggersToRemove.Add(trigger.Id);
                    }
                }

                // Remove the object
                objectsToRemove.Add(obj.Id);
            }

            // Remove objects and triggers
            foreach (int o in objectsToRemove)
            {
                _editor.Level.Objects.Remove(o);
            }

            foreach (int t in triggersToRemove)
            {
                _editor.Level.Triggers.Remove(t);
            }

            var index = _editor.Level.Rooms.ReferenceIndexOf(room);
            comboRoom.Items[index] = index + ": --- Empty room ---";
            _editor.Level.Rooms[index] = null;
        }

        private void butFlipMap_Click(object sender, EventArgs e)
        {
            if (butFlipMap.Checked)
            {
                if (_editor.SelectedRoom.Flipped &&
                    _editor.SelectedRoom.AlternateRoom != null)
                {
                    comboRoom.SelectedIndex = _editor.Level.Rooms.ReferenceIndexOf(_editor.SelectedRoom.AlternateRoom);
                }
            }
            else
            {
                if (_editor.SelectedRoom.Flipped &&
                    _editor.SelectedRoom.BaseRoom != null)
                {
                    comboRoom.SelectedIndex = _editor.Level.Rooms.ReferenceIndexOf(_editor.SelectedRoom.BaseRoom);
                }
            }

            _editor.DrawPanel3D();
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_editor.Level.WadFile))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Can't save a project without a WAD", "Error");
                return;
            }

            if (string.IsNullOrEmpty(_editor.Level.TextureFile))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Can't save a project without a texture map", "Error");
                return;
            }

            string fileName = "";

            if (string.IsNullOrEmpty(_editor.Level.FileName))
            {
                if (saveFileDialogPRJ2.ShowDialog(this) != DialogResult.OK)
                    return;
                fileName = saveFileDialogPRJ2.FileName;
            }
            else
            {
                fileName = _editor.Level.FileName;
            }

            bool result = Prj2Writer.SaveToPrj2(fileName, _editor.Level);
            _editor.Level.FileName = fileName;
            
            if (!result)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("There was an error while saving project file", "Error");
            }
            else
            {
                DarkUI.Forms.DarkMessageBox.ShowInformation("Project file was saved correctly", "Informations");
                this.Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - " +
                            saveFileDialogPRJ2.FileName;
            }
        }

        private void cbFlagDamage_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagDamage = cbFlagDamage.Checked;
        }

        private void cbFlagCold_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagCold = cbFlagCold.Checked;
        }

        private void cbFlagOutside_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagOutside = cbFlagOutside.Checked;
        }

        private void cbHorizon_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagHorizon = cbHorizon.Checked;
        }

        private void butDrawRoomNames_Click(object sender, EventArgs e)
        {
            panel3D.DrawRoomNames = !panel3D.DrawRoomNames;
            butDrawRoomNames.Checked = panel3D.DrawRoomNames;
            panel3D.Draw();
        }

        private void cropRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCropRoom_Click(null, null);
        }

        private void splitRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butSplitRoom_Click(null, null);
        }

        private void copyRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCopyRoom_Click(null, null);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCopy_Click(null, null);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butPaste_Click(null, null);
        }

        private void stampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butClone_Click(null, null);
        }
        
        private void butCopy_Click(object sender, EventArgs e)
        {
            if ((_editor.SelectedRoom == null) || (_editor.SelectedObject == null))
            {
                MessageBox.Show(this, "You have to select an object, before you can copy it.", "No object selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Clipboard.Copy(_editor.SelectedRoom, _editor.SelectedObject.Value);
        }

        private void butPaste_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.Paste;
        }

        private void butClone_Click(object sender, EventArgs e)
        {
            butCopy_Click(null, null);
            _editor.Action = EditorAction.Stamp;
        }

        private void newRoomUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Search the first free room
            short found = -1;
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                    continue;

                found = (short)i;
                break;
            }

            if (found == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            var room = _editor.SelectedRoom;
            var newRoom = new Room(_editor.Level, room.NumXSectors, room.NumZSectors, "room " + found);
            newRoom.Position = room.Position;


            // Build the geometry of the new room
            newRoom.BuildGeometry();
            newRoom.CalculateLightingForThisRoom();
            newRoom.UpdateBuffers();

            _editor.Level.Rooms[found] = newRoom;

            // Update the UI
            comboRoom.Items[found] = found + ": " + newRoom.Name;
            comboRoom.SelectedIndex = found;
        }

        private void newRoomDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Search the first free room
            short found = -1;
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                    continue;

                found = (short)i;
                break;
            }

            if (found == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            var room = _editor.SelectedRoom;
            var newRoom = new Room(_editor.Level, room.NumXSectors, room.NumZSectors, "room " + found);
            newRoom.Position = room.Position;

            // Build the geometry of the new room
            newRoom.BuildGeometry();
            newRoom.CalculateLightingForThisRoom();
            newRoom.UpdateBuffers();

            _editor.Level.Rooms[found] = newRoom;

            // Update the UI
            comboRoom.Items[found] = found + ": " + newRoom.Name;
            comboRoom.SelectedIndex = found;
        }
        
        private void butNoCollision_Click(object sender, EventArgs e)
        {
            if (butNoCollision.Checked)
            {
                _editor.Action = EditorAction.None;
                butNoCollision.Checked = false;
            }
            else
            {
                _editor.Action = EditorAction.PlaceNoCollision;
                butNoCollision.Checked = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Your project will be lost. Do you really want to exit?",
                    "Exit", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            this.Close();
        }

        private void butAddTrigger_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSector, this);
        }

        private void butDrawHorizon_Click(object sender, EventArgs e)
        {
            panel3D.DrawHorizon = !panel3D.DrawHorizon;
            butDrawHorizon.Checked = panel3D.DrawHorizon;
            panel3D.Draw();
        }

        private void cbNoPathfinding_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.ExcludeFromPathFinding = cbNoPathfinding.Checked;
        }

        // Only for debugging purposes...

        private void debugAction0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //level.Load(""); 
            var level = new TombRaider4Level("e:\\trle\\data\\coastal.tr4");
            level.Load("originale");

            level = new TombRaider4Level("Game\\Data\\coastal.tr4");
            level.Load("editor");

            //level = new TombEngine.TombRaider4Level("e:\\trle\\data\\tut1.tr4");
            //level.Load("originale");
        }

        private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //level.Load("");

            var level = new TombRaider3Level("e:\\tomb3\\data\\crash.tr2");
            level.Load("crash");

            level = new TombRaider3Level("e:\\tomb3\\data\\jungle.tr2");
            level.Load("jungle");
        }

        private void debugAction2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tempColors = new List<int>();

            var bmp = (Bitmap)Image.FromFile("Editor\\Palette.png");
            for (int y = 2; y < bmp.Height; y += 14)
            {
                for (int x = 2; x < bmp.Width; x += 14)
                {
                    var col = bmp.GetPixel(x, y);
                    if (col.A == 0)
                        continue;
                    /* if (!tempColors.Contains(col.ToArgb()))*/
                    tempColors.Add(col.ToArgb());
                }
            }
            File.Delete("Editor\\Palette.bin");
            using (var writer = new BinaryWriter(File.OpenWrite("Editor\\Palette.bin")))
            {
                foreach (int c in tempColors)
                {
                    var col2 = System.Drawing.Color.FromArgb(c);
                    writer.Write(col2.R);
                    writer.Write(col2.G);
                    writer.Write(col2.B);
                }
            }
        }

        private void debugAction3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void debugAction5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result = Utils.GetRelativePath("E:\\Vecchi\\Tomb-Editor\\Build\\coastal.prj",
                "E:\\Vecchi\\Tomb-Editor\\Build\\Graphics\\Wads\\coastal.wad");
        }
    }
}
