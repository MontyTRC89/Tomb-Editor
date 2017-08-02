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

        private Editor _editor;
        private int _lastSearchResult = -1;
        private bool _pressedZorY = false;

        public FormMain()
        {
            InitializeComponent();

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

        public void Draw()
        {
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
            
            if ((_editor.Level == null) || !_editor.BlockSelectionAvailable)
                return;

            // Search for unique triggers inside the selected area
            var triggers = new List<int>();
            for (int x = _editor.BlockSelection.X; x <= _editor.BlockSelection.Right; x++)
                for (int z = _editor.BlockSelection.Y; z <= _editor.BlockSelection.Bottom; z++)
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
            EditorActions.SetWall(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void butBox_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.BlockSelection, BlockFlags.Box);
        }

        private void butDeath_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.BlockSelection, BlockFlags.Death);
        }

        private void butMonkey_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.BlockSelection,  BlockFlags.Monkey);
        }

        private void butPortal_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            if (!EditorActions.AddPortal(_editor.SelectedRoom, _editor.BlockSelection))
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
            EditorActions.ToggleClimb(_editor.SelectedRoom, _editor.BlockSelection, 0);
        }

        private void butClimbEast_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleClimb(_editor.SelectedRoom, _editor.BlockSelection, 1);
        }

        private void butClimbSouth_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleClimb(_editor.SelectedRoom, _editor.BlockSelection, 2);
        }

        private void butClimbWest_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleClimb(_editor.SelectedRoom, _editor.BlockSelection, 3);
        }

        private void butNotWalkableBox_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.BlockSelection, BlockFlags.NotWalkableFloor);
        }

        private void butFloor_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetFloor(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void butCeiling_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetCeiling(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void butDiagonalFloor_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetDiagonalFloorSplit(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void butDiagonalCeiling_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetDiagonalCeilingSplit(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void butDiagonalWall_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetDiagonalWallSplit(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void butFlagBeetle_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.BlockSelection, BlockFlags.Beetle);
        }

        private void butFlagTriggerTriggerer_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.BlockSelection, BlockFlags.TriggerTriggerer);
        }

        private void butAddPointLight_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.LightType = LightType.Light;
        }

        private void butAddShadow_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.LightType = LightType.Shadow;
        }

        private void butAddSun_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.LightType = LightType.Sun;
        }

        private void butAddSpotLight_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.LightType = LightType.Spot;
        }

        private void butAddEffectLight_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.LightType = LightType.Effect;
        }

        private void butAddFogBulb_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceLight;
            _editor.LightType = LightType.FogBulb;
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
                _editor.Level.Rooms[comboRoom.SelectedIndex] = new Room(_editor.Level)
                {
                    Name = "Room " + comboRoom.SelectedIndex
                };
                _editor.Level.Rooms[comboRoom.SelectedIndex].Init(0, 0, 0, 20, 20, 12);
                comboRoom.Items[comboRoom.SelectedIndex] =
                    comboRoom.SelectedIndex + ": Room " + comboRoom.SelectedIndex;
                _editor.Level.Rooms[comboRoom.SelectedIndex].BuildGeometry();
                _editor.Level.Rooms[comboRoom.SelectedIndex].CalculateLightingForThisRoom();
                _editor.Level.Rooms[comboRoom.SelectedIndex].UpdateBuffers();
            }

            ResetSelection();

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

                    _editor.IsFlipMap = false;
                    butFlipMap.Checked = false;
                }
                else
                {
                    comboFlipMap.Enabled = false;
                    comboFlipMap.SelectedIndex = room.AlternateGroup + 1;

                    _editor.IsFlipMap = true;
                    butFlipMap.Checked = true;
                }
            }
            else
            {
                comboFlipMap.Enabled = true;
                comboFlipMap.SelectedIndex = 0;

                _editor.IsFlipMap = false;
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            logger.Info($"Tomb Editor {Application.ProductVersion} is starting");

            // Initialize controls
            _editor = Editor.Instance;
            _editor.Initialize(panel3D, panel2DGrid, this);
            _editor.Mode = EditorMode.Geometry;

            panel3D.InitializePanel();
            panelItem.InitializePanel();
            panel2DMap.ResetView();

            logger.Info("Creating new empty level");

            // Create a new empty level
            _editor.Level = new Level();
            _editor.SelectedRoom = null;
            _editor.Level.MustSave = true;

            // Create one room
            if (_editor.Level.Rooms[0] == null)
            {
                _editor.Level.Rooms[0] = new Room(_editor.Level)
                {
                    Name = "Room 0"
                };
                _editor.Level.Rooms[0].Init(0, 0, 0, 20, 20, 12);
            }

            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                    comboRoom.Items.Add(i + ": " + _editor.Level.Rooms[i].Name);
                else
                    comboRoom.Items.Add(i + ": --- Empty room ---");
            }

            // Switch room
            comboRoom.SelectedIndex = 0;

            _editor.SelectedRoom = _editor.Level.Rooms[0];
            _editor.ResetCamera();
            // labelStatistics.Text = _editor.UpdateStatusStrip();
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();

            // Update 3D view
            but3D_Click(null, null);

            this.Text = "Tomb Editor " + Application.ProductVersion + " - Untitled";

            logger.Info("Tomb Editor is ready :)");

            Draw();
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
            _editor.BlockEditingType = 0;
            _editor.LightIndex = -1;

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
            _editor.BlockEditingType = 0;
            _editor.LightIndex = -1;
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
            _editor.BlockEditingType = 0;

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
            _editor.LightIndex = -1;
            _editor.BlockEditingType = 0;

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
            _editor.DrawPortals = !_editor.DrawPortals;
            butDrawPortals.Checked = _editor.DrawPortals;
            _editor.DrawPanel3D();
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
                    EditorActions.ApplyTexture(_editor.SelectedRoom, x, z, BlockFaces.Floor);
                    EditorActions.ApplyTexture(_editor.SelectedRoom, x, z, BlockFaces.FloorTriangle2);
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
                    EditorActions.ApplyTexture(_editor.SelectedRoom, x, z, BlockFaces.Ceiling);
                    EditorActions.ApplyTexture(_editor.SelectedRoom, x, z, BlockFaces.CeilingTriangle2);
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
                            EditorActions.ApplyTexture(_editor.SelectedRoom, x, z, (BlockFaces)k);
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
            _editor.NoCollision = false;
        }

        private void butDoubleSided_Click(object sender, EventArgs e)
        {
            _editor.DoubleSided = butDoubleSided.Checked;
            _editor.NoCollision = false;
        }

        private void butInvisible_Click(object sender, EventArgs e)
        {
            _editor.InvisiblePolygon = butInvisible.Checked;
            _editor.NoCollision = false;
        }

        private void SetPortalOpacity(PortalOpacity opacity)
        {
            if (_editor.PickingResult.ElementType == PickingElementType.Portal)
            {
                Portal portal = _editor.Level.Portals[_editor.PickingResult.Element];

                for (int x = portal.X; x < portal.X + portal.NumXBlocks; x++)
                {
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
                }

                _editor.SelectedRoom.BuildGeometry();
                _editor.SelectedRoom.CalculateLightingForThisRoom();
                _editor.SelectedRoom.UpdateBuffers();
                _editor.DrawPanel3D();
            }
            else
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have to select a portal first",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }
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
            // Select sound
            using (FormSound formSound = new FormSound())
            {
                formSound.IsNew = true;
                if (formSound.ShowDialog(this) != DialogResult.OK)
                    return;

                // Add sound
                _editor.SoundID = formSound.SoundID;
                _editor.Action = EditorAction.PlaceSound;
            }
        }

        private void butAddSink_Click(object sender, EventArgs e)
        {
            _editor.Action = EditorAction.PlaceSink;
        }
        
        private void panelLightColor_Click(object sender, EventArgs e)
        {
            if (_editor.LightIndex == -1)
                return;

            Light light = _editor.SelectedRoom.Lights[_editor.LightIndex];

            colorDialog.Color = light.Color;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelLightColor.BackColor = colorDialog.Color;

            _editor.SelectedRoom.Lights[_editor.LightIndex].Color = colorDialog.Color;
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
            if ((room == null) || !_editor.BlockSelectionAvailable)
                statusStripSelectionArea.Text = "Selected area: None";
            else
                statusStripSelectionArea.Text = "Selected area: " +
                    "X₀ = " + (room.Position.X + _editor.BlockSelection.X) + " | " +
                    "Z₀ = " + (room.Position.Z + _editor.BlockSelection.Y) + " | " +
                    "X₁ = " + (room.Position.X + _editor.BlockSelection.Right) + " | " +
                    "Z₁ = " + (room.Position.Z + _editor.BlockSelection.Bottom);
        }

        public void LoadWadInInterface()
        {
            // carico il wad, in futuro chiedo all'utente quale file caricare
            //tbPlaceItem.Text = _editor.ObjectIds[0];
            _editor.SelectedItem = 0;
            comboItems.Items.Clear();

            // carico gli elementi del wad se esistono
            for (int i = 0; i < _editor.Level.Wad.Moveables.Count; i++)
            {
                comboItems.Items.Add(
                    _editor.MoveableNames[(int)(_editor.Level.Wad.Moveables.ElementAt(i).Value.ObjectID)]);
            }

            for (int i = 0; i < _editor.Level.Wad.StaticMeshes.Count; i++)
            {
                comboItems.Items.Add(
                    _editor.StaticNames[(int)(_editor.Level.Wad.StaticMeshes.ElementAt(i).Value.ObjectID)]);
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
            var level = new Level();

            _editor.SelectedRoom = null;
            _editor.Level = level;

            panelTextureMap.Image = null;
            panelTextureMap.Invalidate();

            // Create one room
            if (_editor.Level.Rooms[0] == null)
            {
                _editor.Level.Rooms[0] = new Room(_editor.Level)
                {
                    Name = "Room 0"
                };
                _editor.Level.Rooms[0].Init(0, 0, 0, 20, 20, 12);
            }

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

            _editor.Level.MustSave = true;

            this.Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - Untitled";
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            UpdateCameraRelocationMode(e, true);

            // Set camera relocation mode
            if ((e.KeyCode == Keys.Y) || (e.KeyCode == Keys.Z))
                _pressedZorY = true;
            if (e.Alt && _pressedZorY)
            {
                panel3D.Cursor = Cursors.Cross;
                panel2DGrid.Cursor = Cursors.Cross;
                _editor.RelocateCameraActive = true;
            }

            // End paste or stamp
            if (e.KeyCode == Keys.Escape)
            {
                Clipboard.Action = PasteAction.None;
                panel3D.Cursor = Cursors.Default;
            }

            // Copy
            if (e.KeyCode == Keys.C && e.Control)
            {
                butCopy_Click(null, null);
                return;
            }

            // Paste
            if (e.KeyCode == Keys.V && e.Control)
            {
                butPaste_Click(null, null);
                return;
            }

            // Stamp
            if (e.KeyCode == Keys.B && e.Control)
            {
                butClone_Click(null, null);
                return;
            }

            if (panel2DGrid.SelectedPortal != -1)
            {
                if (_editor.SelectedRoom.Flipped)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError("You can't delete portals of a flipped room", "Error");
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete Portal ID = " +
                                                            _editor.PickingResult.Element + "?",
                        "Confirm delete",
                        DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                {
                    EditorActions.DeletePortal(_editor.SelectedRoom, panel2DGrid.SelectedPortal);
                    panel2DGrid.SelectedPortal = -1;

                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();
                }
            }
            else if (panel2DGrid.SelectedTrigger != -1)
            {
                using (FormTrigger formTrigger = new FormTrigger())
                {
                    formTrigger.TriggerID = panel2DGrid.SelectedTrigger;
                    formTrigger.Trigger = _editor.Level.Triggers[panel2DGrid.SelectedTrigger];

                    if (formTrigger.ShowDialog(this) != DialogResult.OK)
                        return;

                    _editor.Level.Triggers[panel2DGrid.SelectedTrigger] = formTrigger.Trigger;
                }

                _editor.DrawPanelGrid();
                _editor.LoadTriggersInUI();

                return;
            }
            else
            {
                if (_editor.Mode == EditorMode.Geometry &&
                    _editor.PickingResult.ElementType == PickingElementType.Block)
                {
                    // If one of the four corners of selection is not valid, then do nothing
                    if (!_editor.BlockSelectionAvailable)
                        return;

                    int face = 0;
                    short increment = 0;

                    EditorActions.FaceEditorActions action = EditorActions.FaceEditorActions.EntireFace;
                    EditorActions.FaceSubdivisions sub = EditorActions.FaceSubdivisions.Q;

                    bool diagonal = false;

                    switch (e.KeyCode)
                    {
                        case Keys.Q:
                            sub = EditorActions.FaceSubdivisions.Q;
                            face = 0;
                            increment = 1;
                            break;

                        case Keys.A:
                            sub = EditorActions.FaceSubdivisions.A;
                            face = 0;
                            increment = -1;
                            break;

                        case Keys.W:
                            sub = EditorActions.FaceSubdivisions.W;
                            face = 1;
                            increment = 1;
                            break;

                        case Keys.S:
                            sub = EditorActions.FaceSubdivisions.S;
                            face = 1;
                            increment = -1;
                            break;

                        case Keys.E:
                            sub = EditorActions.FaceSubdivisions.E;
                            increment = 1;
                            break;

                        case Keys.D:
                            sub = EditorActions.FaceSubdivisions.D;
                            increment = -1;
                            break;

                        case Keys.R:
                            sub = EditorActions.FaceSubdivisions.R;
                            increment = 1;
                            break;

                        case Keys.F:
                            sub = EditorActions.FaceSubdivisions.F;
                            increment = -1;
                            break;

                        case Keys.Y:
                            sub = EditorActions.FaceSubdivisions.Q;
                            increment = 1;
                            action = EditorActions.FaceEditorActions.DiagonalFloorCorner;
                            diagonal = true;
                            break;

                        case Keys.H:
                            sub = EditorActions.FaceSubdivisions.A;
                            increment = -1;
                            action = EditorActions.FaceEditorActions.DiagonalFloorCorner;
                            diagonal = true;
                            break;

                        case Keys.U:
                            sub = EditorActions.FaceSubdivisions.W;
                            increment = 1;
                            action = EditorActions.FaceEditorActions.DiagonalCeilingCorner;
                            diagonal = true;
                            break;

                        case Keys.J:
                            sub = EditorActions.FaceSubdivisions.S;
                            increment = -1;
                            action = EditorActions.FaceEditorActions.DiagonalCeilingCorner;
                            diagonal = true;
                            break;

                        case Keys.Escape:
                            _editor.BlockSelectionReset();
                            _editor.BlockEditingType = 0;
                            UpdateStatusStrip();
                            return;

                        case Keys.T:
                            EditorActions.AddTrigger(_editor.SelectedRoom, _editor.BlockSelection, this);
                            return;

                        default:
                            return;
                    }

                    if (e.Control && _editor.BlockEditingType == 0)
                    {
                        // Prepare selection boundaries
                        int xMin = Math.Min(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
                        int xMax = Math.Max(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
                        int zMin = Math.Min(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);
                        int zMax = Math.Max(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);

                        int xMinSpecial = Math.Max(0, xMin - 1);
                        int zMinSpecial = Math.Max(0, zMin - 1);
                        int xMaxSpecial = Math.Min(_editor.SelectedRoom.NumXSectors - 1, xMax + 1);
                        int zMaxSpecial = Math.Min(_editor.SelectedRoom.NumXSectors - 1, zMax + 1);

                        EditorActions.SpecialRaiseFloorOrCeiling(_editor.SelectedRoom, face, increment,
                                                                 xMinSpecial, xMaxSpecial, zMinSpecial, zMaxSpecial,
                                                                 xMin, xMax, zMin, zMax);
                    }

                    if (!diagonal)
                    {
                        switch (_editor.BlockEditingType)
                        {
                            case 0:
                                EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection,
                                    EditorActions.FaceEditorActions.EntireFace, sub);
                                break;

                            case 1:
                                EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection,
                                    EditorActions.FaceEditorActions.EdgeN, sub);
                                break;

                            case 2:
                                EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection,
                                    EditorActions.FaceEditorActions.EdgeE, sub);
                                break;

                            case 3:
                                EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection,
                                    EditorActions.FaceEditorActions.EdgeS, sub);
                                break;

                            case 4:
                                EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection,
                                    EditorActions.FaceEditorActions.EdgeW, sub);
                                break;

                            case 5:
                                EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection,
                                    EditorActions.FaceEditorActions.CornerNW, sub);
                                break;

                            case 6:
                                EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection,
                                    EditorActions.FaceEditorActions.CornerNE, sub);
                                break;

                            case 7:
                                EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection,
                                    EditorActions.FaceEditorActions.CornerSE, sub);
                                break;

                            case 8:
                                EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection,
                                    EditorActions.FaceEditorActions.CornerSW, sub);
                                break;
                        }
                    }
                    else
                    {
                        EditorActions.EditFace(_editor.SelectedRoom, _editor.BlockSelection, action, sub);
                    }
                }
                else if (_editor.PickingResult.ElementType == PickingElementType.Moveable)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete Moveable ID = " +
                                                                        _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(_editor.SelectedRoom, EditorActions.ObjectType.Moveable,
                                    _editor.PickingResult.Element);
                                _editor.PickingResult = Editor.PickingResultEmpty;
                            }

                            break;

                        case Keys.R:
                            EditorActions.RotateObject(_editor.SelectedRoom, EditorActions.ObjectType.Moveable, _editor.PickingResult.Element,
                                1, e.Shift);
                            break;

                        case Keys.O:
                            using (FormMoveable formMoveable = new FormMoveable())
                                formMoveable.ShowDialog(this);
                            break;
                    }
                }
                else if (_editor.PickingResult.ElementType == PickingElementType.StaticMesh)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                                    "Do you really want to delete Static mesh ID = " +
                                    _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(_editor.SelectedRoom, EditorActions.ObjectType.StaticMesh,
                                    _editor.PickingResult.Element);
                                _editor.PickingResult = Editor.PickingResultEmpty;
                            }

                            break;

                        case Keys.R:
                            EditorActions.RotateObject(_editor.SelectedRoom, EditorActions.ObjectType.StaticMesh,
                                _editor.PickingResult.Element, 1, e.Shift);
                            break;
                    }
                }
                else if (_editor.PickingResult.ElementType == PickingElementType.Light)
                {
                    Light light = _editor.SelectedRoom.Lights[_editor.PickingResult.Element];

                    switch (e.KeyCode)
                    {
                        case Keys.Left:
                            if (e.Control)
                            {
                                if (light.Type == LightType.Spot || light.Type == LightType.Sun)
                                    EditorActions.MoveLightCone(_editor.SelectedRoom, _editor.PickingResult.Element, 0, -1);
                                UpdateLightUI();
                            }
                            break;

                        case Keys.Right:
                            if (e.Control)
                            {
                                if (light.Type == LightType.Spot || light.Type == LightType.Sun)
                                    EditorActions.MoveLightCone(_editor.SelectedRoom, _editor.PickingResult.Element, 0, 1);
                                UpdateLightUI();
                            }
                            break;

                        case Keys.Up:
                            if (e.Control)
                            {
                                if (light.Type == LightType.Spot || light.Type == LightType.Sun)
                                    EditorActions.MoveLightCone(_editor.SelectedRoom, _editor.PickingResult.Element, 1, 0);
                                UpdateLightUI();
                            }
                            break;

                        case Keys.Down:
                            if (e.Control)
                            {
                                if (light.Type == LightType.Spot || light.Type == LightType.Sun)
                                    EditorActions.MoveLightCone(_editor.SelectedRoom, _editor.PickingResult.Element, -1, 0);
                                UpdateLightUI();
                            }
                            else
                            {
                                EditorActions.MoveLight(_editor.SelectedRoom, _editor.PickingResult.Element,
                                    EditorActions.MoveObjectDirections.South, e.Shift);
                            }
                            break;

                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete this Light?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteLight(_editor.SelectedRoom, _editor.PickingResult.Element);

                                _editor.LightIndex = -1;
                                _editor.Action = EditorAction.None;
                                _editor.PickingResult = Editor.PickingResultEmpty;
                            }
                            break;
                    }
                }
                else if (_editor.PickingResult.ElementType == PickingElementType.Camera)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete Camera ID = " +
                                                                        _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(_editor.SelectedRoom, EditorActions.ObjectType.Camera,
                                    _editor.PickingResult.Element);
                                _editor.PickingResult = Editor.PickingResultEmpty;
                            }

                            break;
                    }
                }
                else if (_editor.PickingResult.ElementType == PickingElementType.FlyByCamera)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Left:
                            if (e.Control)
                                EditorActions.MoveFlybyCone(_editor.SelectedRoom, _editor.PickingResult.Element, 0, -1);
                            break;

                        case Keys.Right:
                            if (e.Control)
                                EditorActions.MoveFlybyCone(_editor.SelectedRoom, _editor.PickingResult.Element, 0, 1);
                            break;

                        case Keys.Up:
                            if (e.Control)
                                EditorActions.MoveFlybyCone(_editor.SelectedRoom, _editor.PickingResult.Element, 1, 0);
                            break;

                        case Keys.Down:
                            if (e.Control)
                                EditorActions.MoveFlybyCone(_editor.SelectedRoom, _editor.PickingResult.Element, -1, 0);
                            break;

                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                                    "Do you really want to delete Flyby camera ID = " +
                                    _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(_editor.SelectedRoom, EditorActions.ObjectType.FlybyCamera,
                                    _editor.PickingResult.Element);
                                _editor.PickingResult = Editor.PickingResultEmpty;
                            }

                            break;

                        case Keys.O:
                            using (FormFlybyCamera formFlyby = new FormFlybyCamera())
                                formFlyby.ShowDialog(this);
                            break;
                    }
                }
                else if (_editor.PickingResult.ElementType == PickingElementType.Sink)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete Sink ID = " +
                                                                        _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(_editor.SelectedRoom, EditorActions.ObjectType.Sink,
                                    _editor.PickingResult.Element);
                                _editor.PickingResult = Editor.PickingResultEmpty;
                            }

                            break;

                        case Keys.O:
                            using (FormSink formSink = new FormSink())
                                formSink.ShowDialog(this);
                            break;
                    }
                }
                else if (_editor.PickingResult.ElementType == PickingElementType.SoundSource)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                                    "Do you really want to delete Sound source ID = " +
                                    _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(_editor.SelectedRoom, EditorActions.ObjectType.SoundSource,
                                    _editor.PickingResult.Element);
                                _editor.PickingResult = Editor.PickingResultEmpty;
                            }

                            break;

                        case Keys.O:
                            using (FormSound formSound = new FormSound())
                                formSound.ShowDialog(this);
                            break;
                    }
                }
            }

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();

            e.Handled = true;
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
        
        private void UpdateCameraRelocationMode(KeyEventArgs e, bool Up)
        {

        }

        private void loadTextureMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogTextureMap.ShowDialog(this) != DialogResult.OK)
                return;
            LoadTextureMap(openFileDialogTextureMap.FileName);
        }

        public void ResetSelection()
        {
            _editor.LightIndex = -1;
            _editor.SelectedItem = -1;
            _editor.PickingResult = new PickingResult { ElementType = PickingElementType.None };
            _editor.BlockSelectionReset();
            _editor.BlockEditingType = 0;
            UpdateStatusStrip();
        }

        private void LoadTextureMap(string filename)
        {
            _editor.SelectedTexture = -1;
            _editor.Level.LoadTextureMap(filename, _editor.GraphicsDevice);
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
            _editor.Level.LoadTextureMap(pngFilePath, _editor.GraphicsDevice);
        }

        private void loadWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogWAD.ShowDialog(this) != DialogResult.OK)
                return;

            _editor.Level.LoadWad(openFileDialogWAD.FileName, _editor.GraphicsDevice);
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
            if (comboItems.SelectedIndex < _editor.Level.Wad.Moveables.Count)
            {
                _editor.ItemType = EditorItemType.Moveable;
                _editor.SelectedItem =
                    (int)_editor.Level.Wad.WadMoveables.ElementAt(comboItems.SelectedIndex).Value.ObjectID;
                panelItem.ItemType = _editor.ItemType;
                panelItem.SelectedItem = _editor.SelectedItem; // (int)comboItems.SelectedIndex;
                panelRoomAmbientLight.Select();
            }
            else
            {
                _editor.ItemType = EditorItemType.StaticMesh;
                _editor.SelectedItem = (int)_editor.Level.Wad.WasStaticMeshes
                    .ElementAt(comboItems.SelectedIndex - _editor.Level.Wad.Moveables.Count).Value
                    .ObjectID; //comboItems.SelectedIndex - _editor.Level.Wad.Moveables.Count;
                panelItem.ItemType = _editor.ItemType;
                panelItem.SelectedItem = _editor.SelectedItem; // (int)comboItems.SelectedIndex;
                panelRoomAmbientLight.Select();
            }
        }

        private void butAddItem_Click(object sender, EventArgs e)
        {
            if (comboItems.SelectedIndex < _editor.Level.Wad.Moveables.Count)
            {
                var room = _editor.SelectedRoom;

                if (room.Flipped && room.AlternateRoom == null)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError("You can't add moveables to a flipped room", "Error");
                    return;
                }

                _editor.Action = EditorAction.PlaceItem;
                _editor.ItemType = EditorItemType.Moveable;
                _editor.SelectedItem =
                    (int)_editor.Level.Wad.WadMoveables.ElementAt(comboItems.SelectedIndex).Value.ObjectID;
            }
            else
            {
                _editor.Action = EditorAction.PlaceItem;
                _editor.ItemType = EditorItemType.StaticMesh;
                _editor.SelectedItem = (int)_editor.Level.Wad.WasStaticMeshes
                    .ElementAt(comboItems.SelectedIndex - _editor.Level.Wad.Moveables.Count).Value
                    .ObjectID; //comboItems.SelectedIndex - _editor.Level.Wad.Moveables.Count;
            }
        }

        public void LoadStaticMeshColorInUI()
        {
            var instance = (StaticMeshInstance)_editor.Level.Objects[_editor.PickingResult.Element];
            panelStaticMeshColor.BackColor = instance.Color;
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
            for (int i = 0; i < _editor.Level.Portals.Count; i++)
            {
                var p = _editor.Level.Portals[i];
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
            EditorActions.CropRoom(_editor.SelectedRoom, _editor.BlockSelection);
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

            Level level = Prj2Loader.LoadFromPrj2(openFileDialogPRJ2.FileName, _editor.GraphicsDevice, this);
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

            _editor.Level.MustSave = false;

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
            var light = _editor.SelectedRoom.Lights[_editor.LightIndex];

            switch (light.Type)
            {
                case LightType.Light:
                    panelLightColor.BackColor = light.Color;
                    numLightIn.Value = light.In;
                    numLightOut.Value = light.Out;
                    numLightIntensity.Value = light.Intensity;

                    panelLightColor.Enabled = true;
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
            _editor.LightIndex = _editor.PickingResult.Element;
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

            using (var form = new FormImportPRJ())
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

            _editor.Level.MustSave = true;

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
                _editor.Level.MustSave = false;
            }
        }

        private void butRoomUp_Click(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Position += new Vector3(0.0f, 1.0f, 0.0f);

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            for (int p = 0; p < _editor.Level.Portals.Count; p++)
            {
                var portal = _editor.Level.Portals.ElementAt(p).Value;

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

            for (int p = 0; p < _editor.Level.Portals.Count; p++)
            {
                var portal = _editor.Level.Portals.ElementAt(p).Value;

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

        /*  private void numLightIntensity_ValueChanged(object sender, EventArgs e)
          {
              if (_editor.LightIndex != -1)
              {
                  Light light = _editor.SelectedRoom.Lights[_editor.LightIndex];
                  _editor.SelectedRoom.Lights[_editor.LightIndex].Intensity = (float)numLightIntensity.Value;
                  _editor.SelectedRoom.BuildGeometry();
                  _editor.SelectedRoom.CalculateLightingForThisRoom();
                  _editor.SelectedRoom.UpdateBuffers();
                  panel3D.Draw();
              }
          }*/

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
            if (_editor.Mode != EditorMode.Geometry || _editor.PickingResult.ElementType != PickingElementType.Block)
                return;
            if (!_editor.BlockSelectionAvailable)
                return;

            int xMin = Math.Min(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
            int xMax = Math.Max(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
            int zMin = Math.Min(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);
            int zMax = Math.Max(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);

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

            byte numXSectors = (byte)(xMax - xMin + 3);
            byte numZSectors = (byte)(zMax - zMin + 3);

            var newRoom = new Room(_editor.Level);
            newRoom.Init(0, 0, 0, numXSectors, numZSectors, room.Ceiling);

            for (int x = 1; x < numXSectors - 1; x++)
            {
                for (int z = 1; z < numZSectors - 1; z++)
                {
                    newRoom.Blocks[x, z] = room.Blocks[x + xMin - 1, z + zMin - 1].Clone();
                }
            }

            newRoom.Name = "Room " + found;

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
            if (_editor.Mode != EditorMode.Geometry || _editor.PickingResult.ElementType != PickingElementType.Block)
                return;
            if (!_editor.BlockSelectionAvailable)
                return;

            int xMin = Math.Min(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
            int xMax = Math.Max(_editor.BlockSelectionStart.X, _editor.BlockSelectionEnd.X);
            int zMin = Math.Min(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);
            int zMax = Math.Max(_editor.BlockSelectionStart.Y, _editor.BlockSelectionEnd.Y);

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

            byte numXSectors = (byte)(xMax - xMin + 3);
            byte numZSectors = (byte)(zMax - zMin + 3);

            var newRoom = new Room(_editor.Level);
            newRoom.Init(0, 0, 0, numXSectors, numZSectors, room.Ceiling);

            for (int x = 1; x < numXSectors - 1; x++)
            {
                for (int z = 1; z < numZSectors - 1; z++)
                {
                    newRoom.Blocks[x, z] = room.Blocks[x + xMin - 1, z + zMin - 1].Clone();

                    room.Blocks[x + xMin - 1, z + zMin - 1].Type = BlockType.Wall;
                }
            }

            newRoom.Name = "Room " + found;

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
            if ((_editor.SelectedRoom == null) || !_editor.BlockSelectionAvailable)
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
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.BlockSelection, 1);
        }

        private void smoothRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.BlockSelection, -1);
        }

        private void smoothRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.BlockSelection, 1);
        }

        private void smoothRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.BlockSelection, -1);
        }


        private void sharpRandomFloorUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.BlockSelection, 1);
        }

        private void sharpRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.BlockSelection, -1);
        }

        private void sharpRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.BlockSelection, 1);
        }

        private void sharpRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.BlockSelection, -1);
        }

        private void butFlattenFloor_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void butFlattenCeiling_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void flattenFloorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void flattenCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void gridWallsIn3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckForRoomAndBlockSelection())
                EditorActions.GridWalls3(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void gridWallsIn5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.GridWalls5(_editor.SelectedRoom, _editor.BlockSelection);
        }

        private void panelStaticMeshColor_DoubleClick(object sender, EventArgs e)
        {
            if (_editor.PickingResult.ElementType != PickingElementType.StaticMesh)
                return;

            var instance = (StaticMeshInstance)_editor.Level.Objects[_editor.PickingResult.Element];

            colorDialog.Color = instance.Color;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelStaticMeshColor.BackColor = colorDialog.Color;
            instance.Color = colorDialog.Color;

            _editor.Level.Objects[_editor.PickingResult.Element] = instance;

            panel3D.Draw();
        }

        private void butFindItem_Click(object sender, EventArgs e)
        {
            if (_lastSearchResult >= _editor.Level.Objects.Count - 1)
                _lastSearchResult = -1;
            _lastSearchResult++;

            for (int i = _lastSearchResult; i < _editor.Level.Objects.Count; i++)
            {
                var instance = _editor.Level.Objects.ElementAt(i).Value;

                if (_editor.Level.Wad.Moveables.Count <= panelItem.SelectedItem)
                    continue;

                if (instance.Type != ObjectInstanceType.Moveable)
                    continue;

                var moveable = (MoveableInstance)instance;
                if (moveable.Model.ObjectID != _editor.SelectedItem)
                    continue;

                _lastSearchResult = i;

                var pickingResult = new PickingResult
                {
                    ElementType = PickingElementType.Moveable,
                    Element = moveable.Id
                };
                _editor.PickingResult = pickingResult;

                var lastRoom = _editor.SelectedRoom;
                _editor.SelectedRoom = instance.Room;

                if (!ReferenceEquals(lastRoom, instance.Room))
                    CenterCamera();
                Draw();

                return;
            }
        }

        private void butResetSearch_Click(object sender, EventArgs e)
        {
            _lastSearchResult = -1;
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
            string triggerDescription = _editor.Level.Triggers[trigger].ToString();

            using (var formTrigger = new FormTrigger())
            {
                formTrigger.TriggerID = trigger;
                formTrigger.Trigger = _editor.Level.Triggers[trigger];

                if (formTrigger.ShowDialog(this) != DialogResult.OK)
                    return;

                _editor.Level.Triggers[trigger] = formTrigger.Trigger;

                _editor.DrawPanel3D();
                _editor.DrawPanelGrid();
                _editor.ResetSelection();
                _editor.LoadTriggersInUI();
            }
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
                for (int i = 0; i < _editor.Level.Portals.Count; i++)
                {
                    var p = _editor.Level.Portals[i];
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
            int numPortals = _editor.Level.Portals.Count;
            var duplicatedPortals = new Dictionary<int, int>();

            for (int i = 0; i < numPortals; i++)
            {
                var p = _editor.Level.Portals.ElementAt(i).Value;

                if (p.Room != _editor.SelectedRoom)
                    continue;

                int portalId = _editor.Level.GetNewPortalId();
                var newPortal = p.ClonePortal();
                newPortal.Id = portalId;
                newPortal.Flipped = true;

                p.Flipped = true;
                _editor.Level.Portals[p.Id] = p;

                duplicatedPortals.Add(p.Id, portalId);
                _editor.Level.Portals.Add(portalId, newPortal);
            }

            byte numXSectors = (byte)(room.NumXSectors);
            byte numZSectors = (byte)(room.NumZSectors);

            var pos = room.Position;

            var newRoom = new Room(_editor.Level);
            newRoom.Init((int)pos.X, (int)pos.Y, (int)pos.Z, numXSectors, numZSectors, room.Ceiling);

            for (int x = 0; x < numXSectors; x++)
            {
                for (int z = 0; z < numZSectors; z++)
                {
                    newRoom.Blocks[x, z] = room.Blocks[x, z].Clone();
                    newRoom.Blocks[x, z].FloorPortal = (room.Blocks[x, z].FloorPortal != -1
                        ? duplicatedPortals[room.Blocks[x, z].FloorPortal]
                        : -1);
                    newRoom.Blocks[x, z].CeilingPortal = (room.Blocks[x, z].CeilingPortal != -1
                        ? duplicatedPortals[room.Blocks[x, z].CeilingPortal]
                        : -1);
                    newRoom.Blocks[x, z].WallPortal = (room.Blocks[x, z].WallPortal != -1
                        ? duplicatedPortals[room.Blocks[x, z].WallPortal]
                        : -1);
                }
            }

            for (int i = 0; i < room.Lights.Count; i++)
            {
                newRoom.Lights.Add(room.Lights[i].Clone());
            }

            newRoom.Name = "(Flipped of " + _editor.Level.Rooms.ReferenceIndexOf(_editor.SelectedRoom) + ") Room " + found;

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

            _editor.IsFlipMap = true;
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
            _editor.IsFlipMap = !_editor.IsFlipMap;
            butFlipMap.Checked = _editor.IsFlipMap;

            if (_editor.IsFlipMap)
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

            if (_editor.Level.MustSave)
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
            _editor.DrawRoomNames = !_editor.DrawRoomNames;
            butDrawRoomNames.Checked = _editor.DrawRoomNames;
            _editor.DrawPanel3D();
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

        public void ResetPanel3DCursor()
        {
            panel3D.Cursor = Cursors.Default;
        }

        private void butCopy_Click(object sender, EventArgs e)
        {
            Clipboard.Copy();
            //panel3D.Cursor = Cursors.Cross;
            //Clipboard.Action = PasteAction.Paste;
        }

        private void butPaste_Click(object sender, EventArgs e)
        {
            panel3D.Cursor = Cursors.Cross;
            Clipboard.Action = PasteAction.Paste;
        }

        private void butClone_Click(object sender, EventArgs e)
        {
            Clipboard.Copy();
            panel3D.Cursor = Cursors.Cross;
            Clipboard.Action = PasteAction.Stamp;
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
            var newRoom = new Room(_editor.Level)
            {
                Name = "Room " + found
            };
            newRoom.Init(
                (int)room.Position.X,
                (int)(room.Position.Y + room.GetHighestCorner()),
                (int)room.Position.Z,
                20, 20, 12);


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
            var newRoom = new Room(_editor.Level)
            {
                Name = "Room " + found
            };
            newRoom.Init((int)room.Position.X,
                (int)(room.Position.Y - 12),
                (int)room.Position.Z,
                20, 20, 12);


            // Build the geometry of the new room
            newRoom.BuildGeometry();
            newRoom.CalculateLightingForThisRoom();
            newRoom.UpdateBuffers();

            _editor.Level.Rooms[found] = newRoom;

            // Update the UI
            comboRoom.Items[found] = found + ": " + newRoom.Name;
            comboRoom.SelectedIndex = found;
        }

        public void ChangeLightColorFromPalette()
        {
            if (_editor.LightIndex == -1)
                return;

            _editor.SelectedRoom.Lights[_editor.LightIndex].Color = lightPalette.SelectedColor;
            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            _editor.DrawPanel3D();

            panelLightColor.BackColor = lightPalette.SelectedColor;
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
            EditorActions.AddTrigger(_editor.SelectedRoom, _editor.BlockSelection, this);
        }

        private void butDrawHorizon_Click(object sender, EventArgs e)
        {
            _editor.DrawHorizon = !_editor.DrawHorizon;
            butDrawHorizon.Checked = _editor.DrawHorizon;
            _editor.DrawPanel3D();
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
