using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DarkUI;
using TombEditor.Geometry;
using SharpDX;
using TombEditor.Compilers;
using System.IO;
using System.Runtime;
using System.Diagnostics;
using TombEngine;
using System.Runtime.InteropServices;
using NLog;

namespace TombEditor
{
    public partial class FormMain : DarkUI.Forms.DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Editor _editor;
        private int _lastSearchResult = -1;
        
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

        private void butFloor_Click(object sender, EventArgs e)
        {
            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            EditorActions.SetFloor(_editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        public void LoadTriggersInUI()
        {
            lstTriggers.Items.Clear();

            if (_editor.BlockSelectionStartX != -1)
            {
                List<int> triggers = new List<int>();

                int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
                int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

                if (xMin < 1 || zMin < 1 || xMax > _editor.Level.Rooms[_editor.RoomIndex].NumXSectors - 1 ||
                    zMax > _editor.Level.Rooms[_editor.RoomIndex].NumZSectors - 1)
                    return;
                
                // Search for unique triggers inside the selected area
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        for (int i = 0; i < _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Triggers.Count; i++)
                        {
                            int trigger = _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Triggers[i];
                            if (!triggers.Contains(trigger))
                                triggers.Add(trigger);
                        }
                    }
                }

                // Add triggers to listbox
                for (int j = 0; j < triggers.Count; j++)
                {
                    lstTriggers.Items.Add(triggers[j] + " - " + _editor.Level.Triggers[triggers[j]].ToString());
                }
            }
        }

        private void butWall_Click(object sender, EventArgs e)
        {
            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            EditorActions.SetWall(_editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butBox_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleBlockFlag(BlockFlags.Box);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butDeath_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleBlockFlag(BlockFlags.Death);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butMonkey_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleBlockFlag(BlockFlags.Monkey);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butPortal_Click(object sender, EventArgs e)
        {
            if (!EditorActions.AddPortal())
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Not a valid portal position",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butClimbNorth_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleClimb(0);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butClimbEast_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleClimb(1);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butClimbSouth_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleClimb(2);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butClimbWest_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleClimb(3);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
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
                    _editor.Level.Rooms[_editor.RoomIndex].FlagWater = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagRain = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagSnow = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagQuickSand = false;
                    _editor.Level.Rooms[_editor.RoomIndex].WaterLevel = 0;

                    break;

                case 1:
                    _editor.Level.Rooms[_editor.RoomIndex].FlagWater = true;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagRain = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagSnow = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagQuickSand = false;
                    _editor.Level.Rooms[_editor.RoomIndex].WaterLevel = 1;

                    break;

                case 2:
                    _editor.Level.Rooms[_editor.RoomIndex].FlagWater = true;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagRain = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagSnow = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagQuickSand = false;
                    _editor.Level.Rooms[_editor.RoomIndex].WaterLevel = 2;

                    break;

                case 3:
                    _editor.Level.Rooms[_editor.RoomIndex].FlagWater = true;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagRain = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagSnow = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagQuickSand = false;
                    _editor.Level.Rooms[_editor.RoomIndex].WaterLevel = 3;

                    break;

                case 4:
                    _editor.Level.Rooms[_editor.RoomIndex].FlagWater = true;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagRain = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagSnow = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagQuickSand = false;
                    _editor.Level.Rooms[_editor.RoomIndex].WaterLevel = 4;

                    break;

                case 5:
                    _editor.Level.Rooms[_editor.RoomIndex].FlagWater = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagRain = true;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagSnow = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagQuickSand = false;
                    _editor.Level.Rooms[_editor.RoomIndex].WaterLevel = 0;

                    break;

                case 6:
                    _editor.Level.Rooms[_editor.RoomIndex].FlagWater = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagRain = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagSnow = true;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagQuickSand = false;
                    _editor.Level.Rooms[_editor.RoomIndex].WaterLevel = 0;

                    break;

                case 7:
                    _editor.Level.Rooms[_editor.RoomIndex].FlagWater = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagRain = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagSnow = false;
                    _editor.Level.Rooms[_editor.RoomIndex].FlagQuickSand = true;
                    _editor.Level.Rooms[_editor.RoomIndex].WaterLevel = 0;

                    break;
            }
        }

        private void comboReflection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboReflection.SelectedIndex == 0)
            {
                _editor.Level.Rooms[_editor.RoomIndex].FlagReflection = false;
                _editor.Level.Rooms[_editor.RoomIndex].ReflectionLevel = 0;
            }
            else
            {
                _editor.Level.Rooms[_editor.RoomIndex].FlagReflection = true;
                _editor.Level.Rooms[_editor.RoomIndex].ReflectionLevel = (short) comboReflection.SelectedIndex;
            }
        }

        private void comboMist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboReflection.SelectedIndex == 0)
            {
                _editor.Level.Rooms[_editor.RoomIndex].FlagMist = false;
                _editor.Level.Rooms[_editor.RoomIndex].MistLevel = 0;
            }
            else
            {
                _editor.Level.Rooms[_editor.RoomIndex].FlagMist = true;
                _editor.Level.Rooms[_editor.RoomIndex].MistLevel = (short) comboMist.SelectedIndex;
            }
        }

        private void comboRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.Level.Rooms[comboRoom.SelectedIndex] == null)
            {
                _editor.Level.Rooms[comboRoom.SelectedIndex] = new Geometry.Room(_editor.Level, 0, 0, 0, 20, 20, 12);
                _editor.Level.Rooms[comboRoom.SelectedIndex].Name = "Room " + comboRoom.SelectedIndex;
                comboRoom.Items[comboRoom.SelectedIndex] =
                    comboRoom.SelectedIndex + ": Room " + comboRoom.SelectedIndex;
                _editor.Level.Rooms[comboRoom.SelectedIndex].BuildGeometry();
                _editor.Level.Rooms[comboRoom.SelectedIndex].CalculateLightingForThisRoom();
                _editor.Level.Rooms[comboRoom.SelectedIndex].UpdateBuffers();
            }

            ResetSelection();

            _editor.RoomIndex = (short) comboRoom.SelectedIndex;

            Room room = _editor.Level.Rooms[_editor.RoomIndex];

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
            comboReverberation.SelectedIndex = (int) room.Reverberation;

            cbFlagCold.Checked = room.FlagCold;
            cbFlagDamage.Checked = room.FlagDamage;
            cbFlagOutside.Checked = room.FlagOutside;
            cbHorizon.Checked = room.FlagHorizon;
            cbNoPathfinding.Checked = room.ExcludeFromPathFinding;

            if (room.Flipped)
            {
                if (room.AlternateRoom != -1)
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

            UpdateStatistics();
        }

        public void SelectRoom(int index)
        {
            comboRoom.SelectedIndex = index;
        }

        private void panelRoomAmbientLight_Click(object sender, EventArgs e)
        {
            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            colorDialog.Color = room.AmbientLight;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelRoomAmbientLight.BackColor = colorDialog.Color;

            _editor.Level.Rooms[_editor.RoomIndex].AmbientLight = colorDialog.Color;
            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
            panel3D.Draw();
        }

        private void FormMainNew_Shown(object sender, EventArgs e)
        {
            logger.Info($"Tomb Editor {Application.ProductVersion} is starting");

            // Initialize controls
            _editor = Editor.Instance;
            _editor.Initialize(panel3D, panel2DGrid, this);
            _editor.Mode = EditorMode.Geometry;

            panel3D.InitializePanel();
            panelItem.InitializePanel();

            logger.Info("Creating new empty level");

            // Create a new empty level
            _editor.Level = new Level();
            _editor.RoomIndex = -1;
            _editor.Level.MustSave = true;

            // Create one room
            if (_editor.Level.Rooms[0] == null)
            {
                _editor.Level.Rooms[0] = new Room(_editor.Level, 0, 0, 0, 20, 20, 12);
                _editor.Level.Rooms[0].Name = "Room 0";
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

            _editor.RoomIndex = 0;
            _editor.ResetCamera();
            // labelStatistics.Text = _editor.UpdateStatistics();
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();

            // Update 3D view
            but3D_Click(null, null);

            this.Text = "Tomb Editor " + Application.ProductVersion + " - Untitled";

            logger.Info("Tomb Editor is ready :)");
        }

        private void LoadTextureMap()
        {
        }

        private void LoadWad()
        {
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
            _editor.BlockSelectionStartX = -1;
            _editor.BlockSelectionStartZ = -1;
            _editor.BlockSelectionEndX = -1;
            _editor.BlockSelectionEndZ = -1;
            _editor.BlockEditingType = 0;
            _editor.LightIndex = -1;
            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            ResetSelection();

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
            _editor.BlockSelectionStartX = -1;
            _editor.BlockSelectionStartZ = -1;
            _editor.BlockSelectionEndX = -1;
            _editor.BlockSelectionEndZ = -1;
            _editor.BlockEditingType = 0;
            _editor.LightIndex = -1;

            ResetSelection();
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
            _editor.BlockSelectionStartX = -1;
            _editor.BlockSelectionStartZ = -1;
            _editor.BlockSelectionEndX = -1;
            _editor.BlockSelectionEndZ = -1;
            _editor.BlockEditingType = 0;

            ResetSelection();

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
            _editor.BlockSelectionStartX = -1;
            _editor.BlockSelectionStartZ = -1;
            _editor.BlockSelectionEndX = -1;
            _editor.BlockSelectionEndZ = -1;
            _editor.LightIndex = -1;
            _editor.BlockEditingType = 0;

            ResetSelection();

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
            Room room = _editor.Level.Rooms[_editor.RoomIndex];
            if (room == null)
                return;

            // aggiorno la telecamera
            /*panel3D.Camera.Target = new Vector3(room.Position.X * 1024.0f + room.NumXSectors * 512.0f, room.Position.Y * 128.0f + room.Ceiling * 64.0f,
                                                room.Position.Z * 1024.0f + room.NumZSectors * 512.0f);*/
            panel3D.Camera.Target = new Vector3(room.Position.X * 1024.0f + room.Centre.X,
                room.Position.Y * 256.0f + room.Centre.Y,
                room.Position.Z * 1024.0f + room.Centre.Z);
            panel3D.Camera.Distance = 3072.0f;
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
            Room room = _editor.Level.Rooms[_editor.RoomIndex];
            for (int x = 1; x < room.NumXSectors - 1; x++)
            {
                for (int z = 1; z < room.NumZSectors - 1; z++)
                {
                    EditorActions.ApplyTexture(x, z, BlockFaces.Floor);
                    EditorActions.ApplyTexture(x, z, BlockFaces.FloorTriangle2);
                }
            }

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

            panel3D.Draw();
        }

        private void butTextureCeiling_Click(object sender, EventArgs e)
        {
            Room room = _editor.Level.Rooms[_editor.RoomIndex];
            for (int x = 1; x < room.NumXSectors - 1; x++)
            {
                for (int z = 1; z < room.NumZSectors - 1; z++)
                {
                    EditorActions.ApplyTexture(x, z, BlockFaces.Ceiling);
                    EditorActions.ApplyTexture(x, z, BlockFaces.CeilingTriangle2);
                }
            }

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

            panel3D.Draw();
        }

        private void butTextureWalls_Click(object sender, EventArgs e)
        {
            Room room = _editor.Level.Rooms[_editor.RoomIndex];
            for (int x = 0; x < room.NumXSectors; x++)
            {
                for (int z = 0; z < room.NumZSectors; z++)
                {
                    for (int k = 10; k <= 13; k++)
                    {
                        if (room.Blocks[x, z].Faces[k].Defined)
                            EditorActions.ApplyTexture(x, z, (BlockFaces) k);
                    }
                }
            }

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

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
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WallOpacity = opacity;
                                break;

                            case PortalDirection.South:
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WallOpacity = opacity;
                                break;

                            case PortalDirection.East:
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WallOpacity = opacity;
                                break;

                            case PortalDirection.West:
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WallOpacity = opacity;
                                break;

                            case PortalDirection.Floor:
                                if (!_editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].IsFloorSolid)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].FloorOpacity = opacity;
                                else
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].FloorOpacity =
                                        PortalOpacity.None;

                                break;

                            case PortalDirection.Ceiling:
                                if (!_editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].IsCeilingSolid)
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].CeilingOpacity = opacity;
                                else
                                    _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].CeilingOpacity =
                                        PortalOpacity.None;

                                break;
                        }
                    }
                }

                _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
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

        /* private void numLightIn_ValueChanged(object sender, EventArgs e)
         {
             if (_editor.LightIndex != -1)
             {
                 Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex];
                 _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex].In = (float)numLightIn.Value;
                 _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                 _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                 _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                 panel3D.Draw();
             }
         }
 
         private void numLightOut_ValueChanged(object sender, EventArgs e)
         {
             if (_editor.LightIndex != -1)
             {
                 Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex];
                 _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex].Out = (float)numLightOut.Value;
                 _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                 _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                 _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                 panel3D.Draw();
             }
         }
 
         private void numLightLen_ValueChanged(object sender, EventArgs e)
         {
             if (_editor.LightIndex != -1)
             {
                 Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex];
                 _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex].Len = (float)numLightLen.Value;
                 _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                 _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                 _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                 panel3D.Draw();
             }
         }
 
         private void numLightCutoff_ValueChanged(object sender, EventArgs e)
         {
             if (_editor.LightIndex != -1)
             {
                 Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex];
                 _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex].Cutoff = (float)numLightCutoff.Value;
                 _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                 _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                 _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                 panel3D.Draw();
             }
         }
 
         private void numLightDirectionX_ValueChanged(object sender, EventArgs e)
         {
             if (_editor.LightIndex != -1)
             {
                 Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex];
                 _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex].DirectionX = (float)numLightDirectionX.Value;
                 _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                 _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                 _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                 panel3D.Draw();
             }
         }
 
         private void numLightDirectionY_ValueChanged(object sender, EventArgs e)
         {
             if (_editor.LightIndex != -1)
             {
                 Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex];
                 _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex].DirectionY = (float)numLightDirectionY.Value;
                 _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                 _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                 _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                 panel3D.Draw();
             }
         }
         */
        private void panelLightColor_Click(object sender, EventArgs e)
        {
            if (_editor.LightIndex == -1)
                return;
            
            Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex];

            colorDialog.Color = light.Color;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelLightColor.BackColor = colorDialog.Color;

            _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex].Color = colorDialog.Color;
            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
            panel3D.Draw();
        }

        public void LoadTextureMapInEditor(Level level)
        {
            panelTextureMap.Image = level._textureMap;
            panelTextureMap.Height = level._textureMap.Height;
            panelTextureMap.Invalidate();
        }

        public void UpdateStatistics()
        {
            if (_editor.RoomIndex == -1)
                return;

            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            labelRoomStatistics.Text = "X: " + room.Position.X + " | " +
                                       "Y: " + room.Position.Y + " | " +
                                       "Z: " + room.Position.Z + " | " +
                                       "F: " + (room.Position.Y + room.GetLowestCorner()) + " | " +
                                       "C: " + (room.Position.Y + room.GetHighestCorner());
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
                    _editor.MoveablesObjectIds[(int) (_editor.Level.Wad.Moveables.ElementAt(i).Value.ObjectID)]);
            }

            for (int i = 0; i < _editor.Level.Wad.StaticMeshes.Count; i++)
            {
                comboItems.Items.Add(
                    _editor.StaticMeshesObjectIds[(int) (_editor.Level.Wad.StaticMeshes.ElementAt(i).Value.ObjectID)]);
            }

            comboItems.SelectedIndex = 0;

            SelectItem(0);
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Your project will be lost. Do you really want to create a new project?",
                                                        "New project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            // Clean all resources before creating a new level
            if (_editor.Level != null)
                _editor.Level.Dispose();

            // Create a new level
            Level level = new Level();

            _editor.RoomIndex = -1;
            _editor.Level = level;

            panelTextureMap.Image = null;
            panelTextureMap.Invalidate();

            // Create one room
            if (_editor.Level.Rooms[0] == null)
            {
                _editor.Level.Rooms[0] = new Room(_editor.Level, 0, 0, 0, 20, 20, 12);
                _editor.Level.Rooms[0].Name = "Room 0";
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

            _editor.RoomIndex = 0;
            _editor.ResetCamera();
            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();

            _editor.Level.MustSave = true;

            this.Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - Untitled";
        }

        private void ResetInterface()
        { }

        private void DeletePortal(int id)
        {
            int otherPortalId = _editor.Level.Portals[id].OtherID;

            Portal current = _editor.Level.Portals[id];
            Portal other = _editor.Level.Portals[otherPortalId];

            for (int x = current.X; x < current.X + current.NumXBlocks; x++)
            {
                for (int z = current.Z; z < current.Z + current.NumZBlocks; z++)
                {
                    if (current.Direction == PortalDirection.Floor)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].FloorPortal = -1;
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].FloorOpacity = PortalOpacity.None;
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Type = BlockType.Floor;
                    }

                    if (current.Direction == PortalDirection.North || current.Direction == PortalDirection.South ||
                        current.Direction == PortalDirection.West || current.Direction == PortalDirection.East)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WallPortal = -1;
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].WallOpacity = PortalOpacity.None;
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Type = BlockType.BorderWall;
                    }
                }
            }

            for (int x = other.X; x < other.X + other.NumXBlocks; x++)
            {
                for (int z = other.Z; z < other.Z + other.NumZBlocks; z++)
                {
                    if (other.Direction == PortalDirection.Ceiling)
                    {
                        _editor.Level.Rooms[other.Room].Blocks[x, z].CeilingPortal = -1;
                        _editor.Level.Rooms[other.Room].Blocks[x, z].CeilingOpacity = PortalOpacity.None;
                        _editor.Level.Rooms[other.Room].Blocks[x, z].Type = BlockType.Floor;
                    }

                    if (other.Direction == PortalDirection.North || other.Direction == PortalDirection.South ||
                        other.Direction == PortalDirection.West || other.Direction == PortalDirection.East)
                    {
                        _editor.Level.Rooms[other.Room].Blocks[x, z].WallPortal = -1;
                        _editor.Level.Rooms[other.Room].Blocks[x, z].WallOpacity = PortalOpacity.None;
                        _editor.Level.Rooms[other.Room].Blocks[x, z].Type = BlockType.BorderWall;
                    }
                }
            }

            _editor.Level.Rooms[_editor.Level.Portals[id].Room].Portals.Remove(id);
            _editor.Level.Rooms[_editor.Level.Portals[otherPortalId].Room].Portals.Remove(otherPortalId);

            _editor.Level.Portals.Remove(id);
            _editor.Level.Portals.Remove(otherPortalId);

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

            _editor.Level.Rooms[other.Room].BuildGeometry();
            _editor.Level.Rooms[other.Room].CalculateLightingForThisRoom();
            _editor.Level.Rooms[other.Room].UpdateBuffers();
        }

        private void FormMainNew_KeyDown(object sender, KeyEventArgs e)
        {
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
                if (_editor.Level.Rooms[_editor.RoomIndex].Flipped)
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
                    DeletePortal(panel2DGrid.SelectedPortal);
                    panel2DGrid.SelectedPortal = -1;

                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();
                }

                //labelStatistics.Text = _editor.UpdateStatistics();
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

                //_editor.DrawPanel3D();
                _editor.DrawPanelGrid();
                //_editor.ResetSelection();
                _editor.LoadTriggersInUI();

                return;
            }
            else
            {
                if (_editor.Mode == EditorMode.Geometry &&
                    _editor.PickingResult.ElementType == PickingElementType.Block)
                {
                    // Prepare selection boundaries
                    int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                    int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                    int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
                    int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

                    // If one of the four corners of selection is not valid, then do nothing
                    if (xMin == -1 || xMax == -1 || zMin == -1 || zMax == -1)
                        return;

                    int face = 0;
                    short increment = 0;
                    bool addTrigger = false;

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
                            _editor.BlockSelectionStartX = -1;
                            _editor.BlockSelectionStartZ = -1;
                            _editor.BlockSelectionEndX = -1;
                            _editor.BlockSelectionEndZ = -1;
                            _editor.BlockEditingType = 0;
                            return;

                        case Keys.T:
                            addTrigger = true;
                            break;

                        default:
                            return;
                    }

                    // Add trigger is T key was pressed
                    if (addTrigger)
                    {
                        EditorActions.AddTrigger(this, xMin, xMax, zMin, zMax);
                        return;
                    }

                    if (e.Control && !addTrigger && _editor.BlockEditingType == 0)
                    {
                        int xMinSpecial = Math.Max(0, xMin - 1);
                        int zMinSpecial = Math.Max(0, zMin - 1);
                        int xMaxSpecial = Math.Min(_editor.Level.Rooms[_editor.RoomIndex].NumXSectors - 1, xMax + 1);
                        int zMaxSpecial = Math.Min(_editor.Level.Rooms[_editor.RoomIndex].NumXSectors - 1, zMax + 1);

                        EditorActions.SpecialRaiseFloorOrCeiling(face, increment,
                                                                 xMinSpecial, xMaxSpecial, zMinSpecial, zMaxSpecial,
                                                                 xMin, xMax, zMin, zMax);
                        ///_editor.DrawPanel3D();
                        //return;
                    }

                    if (!diagonal)
                    {
                        switch (_editor.BlockEditingType)
                        {
                            case 0:
                                EditorActions.EditFace(xMin, xMax, zMin, zMax,
                                    EditorActions.FaceEditorActions.EntireFace, sub);
                                break;

                            case 1:
                                EditorActions.EditFace(xMin, xMax, zMin, zMax, EditorActions.FaceEditorActions.EdgeN,
                                    sub);
                                break;

                            case 2:
                                EditorActions.EditFace(xMin, xMax, zMin, zMax, EditorActions.FaceEditorActions.EdgeE,
                                    sub);
                                break;

                            case 3:
                                EditorActions.EditFace(xMin, xMax, zMin, zMax, EditorActions.FaceEditorActions.EdgeS,
                                    sub);
                                break;

                            case 4:
                                EditorActions.EditFace(xMin, xMax, zMin, zMax, EditorActions.FaceEditorActions.EdgeW,
                                    sub);
                                break;

                            case 5:
                                EditorActions.EditFace(xMin, xMax, zMin, zMax, EditorActions.FaceEditorActions.CornerNW,
                                    sub);
                                break;

                            case 6:
                                EditorActions.EditFace(xMin, xMax, zMin, zMax, EditorActions.FaceEditorActions.CornerNE,
                                    sub);
                                break;

                            case 7:
                                EditorActions.EditFace(xMin, xMax, zMin, zMax, EditorActions.FaceEditorActions.CornerSE,
                                    sub);
                                break;

                            case 8:
                                EditorActions.EditFace(xMin, xMax, zMin, zMax, EditorActions.FaceEditorActions.CornerSW,
                                    sub);
                                break;
                        }
                    }
                    else
                    {
                        EditorActions.EditFace(xMin, xMax, zMin, zMax, action, sub);
                    }
                }
                else if (_editor.PickingResult.ElementType == PickingElementType.Moveable)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Q:
                            EditorActions.MoveObject(EditorActions.ObjectType.Moveable, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Up, e.Shift);
                            break;

                        case Keys.A:
                            EditorActions.MoveObject(EditorActions.ObjectType.Moveable, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Down, e.Shift);
                            break;

                        case Keys.Left:
                            EditorActions.MoveObject(EditorActions.ObjectType.Moveable, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.West, e.Shift);
                            break;

                        case Keys.Right:
                            EditorActions.MoveObject(EditorActions.ObjectType.Moveable, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.East, e.Shift);
                            break;

                        case Keys.Up:
                            EditorActions.MoveObject(EditorActions.ObjectType.Moveable, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.North, e.Shift);
                            break;

                        case Keys.Down:
                            EditorActions.MoveObject(EditorActions.ObjectType.Moveable, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.South, e.Shift);
                            break;

                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete Moveable ID = " +
                                                                        _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(EditorActions.ObjectType.Moveable,
                                    _editor.PickingResult.Element);
                                _editor.PickingResult = Editor.PickingResultEmpty;
                            }

                            break;

                        case Keys.R:
                            EditorActions.RotateObject(EditorActions.ObjectType.Moveable, _editor.PickingResult.Element,
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
                        case Keys.Q:
                            EditorActions.MoveObject(EditorActions.ObjectType.StaticMesh, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Up, e.Shift);
                            break;

                        case Keys.A:
                            EditorActions.MoveObject(EditorActions.ObjectType.StaticMesh, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Down, e.Shift);
                            break;

                        case Keys.Left:
                            EditorActions.MoveObject(EditorActions.ObjectType.StaticMesh, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.West, e.Shift);
                            break;

                        case Keys.Right:
                            EditorActions.MoveObject(EditorActions.ObjectType.StaticMesh, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.East, e.Shift);
                            break;

                        case Keys.Up:
                            EditorActions.MoveObject(EditorActions.ObjectType.StaticMesh, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.North, e.Shift);
                            break;

                        case Keys.Down:
                            EditorActions.MoveObject(EditorActions.ObjectType.StaticMesh, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.South, e.Shift);
                            break;

                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                                    "Do you really want to delete Static mesh ID = " +
                                    _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(EditorActions.ObjectType.StaticMesh,
                                    _editor.PickingResult.Element);
                                _editor.PickingResult = Editor.PickingResultEmpty;
                            }

                            break;

                        case Keys.R:
                            EditorActions.RotateObject(EditorActions.ObjectType.StaticMesh,
                                _editor.PickingResult.Element, 1, e.Shift);
                            break;
                    }
                }
                else if (_editor.PickingResult.ElementType == PickingElementType.Light)
                {
                    Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.PickingResult.Element];

                    switch (e.KeyCode)
                    {
                        case Keys.Q:
                            EditorActions.MoveLight(_editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Up, e.Shift);
                            break;

                        case Keys.A:
                            EditorActions.MoveLight(_editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Down, e.Shift);
                            break;

                        case Keys.Left:
                            if (e.Control)
                            {
                                if (light.Type == LightType.Spot || light.Type == LightType.Sun)
                                {
                                    EditorActions.MoveLightCone(_editor.PickingResult.Element, 0, -1);
                                }

                                UpdateLightUI();
                            }
                            else
                            {
                                EditorActions.MoveLight(_editor.PickingResult.Element,
                                    EditorActions.MoveObjectDirections.West, e.Shift);
                            }

                            break;

                        case Keys.Right:
                            if (e.Control)
                            {
                                if (light.Type == LightType.Spot || light.Type == LightType.Sun)
                                {
                                    EditorActions.MoveLightCone(_editor.PickingResult.Element, 0, 1);
                                }

                                UpdateLightUI();
                            }
                            else
                            {
                                EditorActions.MoveLight(_editor.PickingResult.Element,
                                    EditorActions.MoveObjectDirections.East, e.Shift);
                            }

                            break;

                        case Keys.Up:
                            if (e.Control)
                            {
                                if (light.Type == LightType.Spot || light.Type == LightType.Sun)
                                {
                                    EditorActions.MoveLightCone(_editor.PickingResult.Element, 1, 0);
                                }

                                UpdateLightUI();
                            }
                            else
                            {
                                EditorActions.MoveLight(_editor.PickingResult.Element,
                                    EditorActions.MoveObjectDirections.North, e.Shift);
                            }

                            break;

                        case Keys.Down:
                            if (e.Control)
                            {
                                if (light.Type == LightType.Spot || light.Type == LightType.Sun)
                                {
                                    EditorActions.MoveLightCone(_editor.PickingResult.Element, -1, 0);
                                }

                                UpdateLightUI();
                            }
                            else
                            {
                                EditorActions.MoveLight(_editor.PickingResult.Element,
                                    EditorActions.MoveObjectDirections.South, e.Shift);
                            }
                            break;

                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete this Light?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteLight(_editor.PickingResult.Element);

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
                        case Keys.Q:
                            EditorActions.MoveObject(EditorActions.ObjectType.Camera, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Up, e.Shift);
                            break;

                        case Keys.A:
                            EditorActions.MoveObject(EditorActions.ObjectType.Camera, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Down, e.Shift);
                            break;

                        case Keys.Left:
                            EditorActions.MoveObject(EditorActions.ObjectType.Camera, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.West, e.Shift);
                            break;

                        case Keys.Right:
                            EditorActions.MoveObject(EditorActions.ObjectType.Camera, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.East, e.Shift);
                            break;

                        case Keys.Up:
                            EditorActions.MoveObject(EditorActions.ObjectType.Camera, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.North, e.Shift);
                            break;

                        case Keys.Down:
                            EditorActions.MoveObject(EditorActions.ObjectType.Camera, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.South, e.Shift);
                            break;

                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete Camera ID = " +
                                                                        _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(EditorActions.ObjectType.Camera,
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
                        case Keys.Q:
                            EditorActions.MoveObject(EditorActions.ObjectType.FlybyCamera,
                                _editor.PickingResult.Element, EditorActions.MoveObjectDirections.Up, e.Shift);
                            break;

                        case Keys.A:
                            EditorActions.MoveObject(EditorActions.ObjectType.FlybyCamera,
                                _editor.PickingResult.Element, EditorActions.MoveObjectDirections.Down, e.Shift);
                            break;

                        case Keys.Left:
                            if (e.Control)
                            {
                                EditorActions.MoveFlybyCone(_editor.PickingResult.Element, 0, -1);
                            }
                            else
                            {
                                EditorActions.MoveObject(EditorActions.ObjectType.FlybyCamera,
                                    _editor.PickingResult.Element, EditorActions.MoveObjectDirections.West, e.Shift);
                            }

                            break;

                        case Keys.Right:
                            if (e.Control)
                            {
                                EditorActions.MoveFlybyCone(_editor.PickingResult.Element, 0, 1);
                            }
                            else
                            {
                                EditorActions.MoveObject(EditorActions.ObjectType.FlybyCamera,
                                    _editor.PickingResult.Element, EditorActions.MoveObjectDirections.East, e.Shift);
                            }

                            break;

                        case Keys.Up:
                            if (e.Control)
                            {
                                EditorActions.MoveFlybyCone(_editor.PickingResult.Element, 1, 0);
                            }
                            else
                            {
                                EditorActions.MoveObject(EditorActions.ObjectType.FlybyCamera,
                                    _editor.PickingResult.Element, EditorActions.MoveObjectDirections.North, e.Shift);
                            }

                            break;

                        case Keys.Down:
                            if (e.Control)
                            {
                                EditorActions.MoveFlybyCone(_editor.PickingResult.Element, -1, 0);
                            }
                            else
                            {
                                EditorActions.MoveObject(EditorActions.ObjectType.FlybyCamera,
                                    _editor.PickingResult.Element, EditorActions.MoveObjectDirections.South, e.Shift);
                            }

                            break;

                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                                    "Do you really want to delete Flyby camera ID = " +
                                    _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(EditorActions.ObjectType.FlybyCamera,
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
                        case Keys.Q:
                            EditorActions.MoveObject(EditorActions.ObjectType.Sink, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Up, e.Shift);
                            break;

                        case Keys.A:
                            EditorActions.MoveObject(EditorActions.ObjectType.Sink, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.Down, e.Shift);
                            break;

                        case Keys.Left:
                            EditorActions.MoveObject(EditorActions.ObjectType.Sink, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.West, e.Shift);
                            break;

                        case Keys.Right:
                            EditorActions.MoveObject(EditorActions.ObjectType.Sink, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.East, e.Shift);
                            break;

                        case Keys.Up:
                            EditorActions.MoveObject(EditorActions.ObjectType.Sink, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.North, e.Shift);
                            break;

                        case Keys.Down:
                            EditorActions.MoveObject(EditorActions.ObjectType.Sink, _editor.PickingResult.Element,
                                EditorActions.MoveObjectDirections.South, e.Shift);
                            break;

                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete Sink ID = " +
                                                                        _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(EditorActions.ObjectType.Sink,
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
                        case Keys.Q:
                            EditorActions.MoveObject(EditorActions.ObjectType.SoundSource,
                                _editor.PickingResult.Element, EditorActions.MoveObjectDirections.Up, e.Shift);
                            break;

                        case Keys.A:
                            EditorActions.MoveObject(EditorActions.ObjectType.SoundSource,
                                _editor.PickingResult.Element, EditorActions.MoveObjectDirections.Down, e.Shift);
                            break;

                        case Keys.Left:
                            EditorActions.MoveObject(EditorActions.ObjectType.SoundSource,
                                _editor.PickingResult.Element, EditorActions.MoveObjectDirections.West, e.Shift);
                            break;

                        case Keys.Right:
                            EditorActions.MoveObject(EditorActions.ObjectType.SoundSource,
                                _editor.PickingResult.Element, EditorActions.MoveObjectDirections.East, e.Shift);
                            break;

                        case Keys.Up:
                            EditorActions.MoveObject(EditorActions.ObjectType.SoundSource,
                                _editor.PickingResult.Element, EditorActions.MoveObjectDirections.North, e.Shift);
                            break;

                        case Keys.Down:
                            EditorActions.MoveObject(EditorActions.ObjectType.SoundSource,
                                _editor.PickingResult.Element, EditorActions.MoveObjectDirections.South, e.Shift);
                            break;

                        case Keys.Delete:
                            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                                    "Do you really want to delete Sound source ID = " +
                                    _editor.PickingResult.Element + "?",
                                    "Confirm delete",
                                    DarkUI.Forms.DarkDialogButton.YesNo) == DialogResult.Yes)
                            {
                                EditorActions.DeleteObject(EditorActions.ObjectType.SoundSource,
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

                // _editor.DrawPanel3D();
            }

            _editor.DrawPanel3D();

            /*if (e.Control && !panel3D.Drag)
            {
                    
            }
            else
            {
                _editor.DrawPanel3D();
            }*/

            _editor.DrawPanelGrid();

            e.Handled = true;
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
            _editor.PickingResult = new PickingResult {ElementType = PickingElementType.None};
            _editor.BlockSelectionStartX = -1;
            _editor.BlockSelectionStartZ = -1;
            _editor.BlockSelectionEndX = -1;
            _editor.BlockSelectionEndZ = -1;
            _editor.BlockEditingType = 0;
            //if (_editor.Mode != EditorMode.FaceEdit) _editor.SelectedTexture = -1;
        }

        private void LoadTextureMap(string filename)
        {
            _editor.SelectedTexture = -1;
            _editor.Level.LoadTextureMap(filename);
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
                DarkUI.Forms.DarkMessageBox.ShowError("Currently there is no texture loaded to convert it.", "No texture loaded");
                return;
            }

            string pngFilePath = Path.Combine(
                Path.GetDirectoryName(_editor.Level.TextureFile),
                Path.GetFileNameWithoutExtension(_editor.Level.TextureFile) + ".png");

            if (File.Exists(pngFilePath))
            {
                if (DarkUI.Forms.DarkMessageBox.ShowWarning("There is already a file at \"" + pngFilePath + "\". Continue and overwrite the file?",
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
                    DarkUI.Forms.DarkMessageBox.ShowError("There was an error while converting TGA in PNG format. " + exc.Message, "Error");
                    return;
                }

                watch.Stop();

                logger.Info("Texture map converted");
                logger.Info("    Elapsed time: " + watch.ElapsedMilliseconds + " ms");
            }

            DarkUI.Forms.DarkMessageBox.ShowInformation("TGA texture map was converted to PNG without errors and saved at \"" + pngFilePath + "\".", "Success");
            _editor.Level.LoadTextureMap(pngFilePath);
        }

        private void loadWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialogWAD.ShowDialog(this) != DialogResult.OK)
                return;

            _editor.Level.LoadWad(openFileDialogWAD.FileName);
            LoadWadInInterface();

            // MessageBox.Show("WAD was loaded without errors", "Informations", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void comboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboItems.SelectedIndex == -1)
                return;

            SelectItem(comboItems.SelectedIndex);
        }

        private void SelectItem(int index)
        {
            if (comboItems.SelectedIndex < _editor.Level.Wad.Moveables.Count)
            {
                _editor.ItemType = EditorItemType.Moveable;
                _editor.SelectedItem =
                    (int) _editor.Level.Wad.WadMoveables.ElementAt(comboItems.SelectedIndex).Value.ObjectID;
                panelItem.ItemType = _editor.ItemType;
                panelItem.SelectedItem = _editor.SelectedItem; // (int)comboItems.SelectedIndex;
                panelRoomAmbientLight.Select();
            }
            else
            {
                _editor.ItemType = EditorItemType.StaticMesh;
                _editor.SelectedItem = (int) _editor.Level.Wad.WasStaticMeshes
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
                Room room = _editor.Level.Rooms[_editor.RoomIndex];

                if (room.Flipped && room.AlternateRoom == -1)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError("You can't add moveables to a flipped room", "Error");
                    return;
                }

                _editor.Action = EditorAction.PlaceItem;
                _editor.ItemType = EditorItemType.Moveable;
                _editor.SelectedItem =
                    (int) _editor.Level.Wad.WadMoveables.ElementAt(comboItems.SelectedIndex).Value.ObjectID;
            }
            else
            {
                _editor.Action = EditorAction.PlaceItem;
                _editor.ItemType = EditorItemType.StaticMesh;
                _editor.SelectedItem = (int) _editor.Level.Wad.WasStaticMeshes
                    .ElementAt(comboItems.SelectedIndex - _editor.Level.Wad.Moveables.Count).Value
                    .ObjectID; //comboItems.SelectedIndex - _editor.Level.Wad.Moveables.Count;
            }
        }

        public void LoadStaticMeshColorInUI()
        {
            StaticMeshInstance instance = (StaticMeshInstance) _editor.Level.Objects[_editor.PickingResult.Element];
            panelStaticMeshColor.BackColor = instance.Color;
        }

        private void butDeleteRoom_Click(object sender, EventArgs e)
        {
            // Check if is the last room
            int numRooms = 0;
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null)
                    numRooms++;
            }

            if (numRooms == 1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You must have at least one room in your level", "Error");
                return;
            }

            // Check if room has portals
            for (int i = 0; i < _editor.Level.Portals.Count; i++)
            {
                Portal p = _editor.Level.Portals[i];
                if (p.Room == _editor.RoomIndex || p.AdjoiningRoom == _editor.RoomIndex)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError("You can't delete a room with portals to other rooms.",
                        "Error");
                    return;
                }
            }

            // Ask for confirmation
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Do you really want to delete this room? All objects inside room will be deleted and " +
                    "triggers pointing to them will be removed.",
                    "Delete room", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
            {
                return;
            }

            int roomToDelete = _editor.RoomIndex;

            // Delete the room
            DeleteRoom(_editor.RoomIndex);

            // Find a valid room
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                if (_editor.Level.Rooms[i] != null && i != roomToDelete)
                {
                    comboRoom.SelectedIndex = i;
                    break;
                }
            }
        }

        private void butCropRoom_Click(object sender, EventArgs e)
        {
            if (_editor.Mode == EditorMode.Geometry && _editor.PickingResult.ElementType == PickingElementType.Block)
            {
                int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
                int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

                if (xMin == -1 || xMax == -1 || zMin == -1 || zMax == -1)
                    return;

                EditorActions.CropRoom(xMin, xMax, zMin, zMax);

                _editor.CenterCamera();
                _editor.DrawPanel3D();
            }
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
                    "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes) return;

            if (openFileDialogPRJ2.ShowDialog(this) != DialogResult.OK)
                return;

            Level level = Level.LoadFromPrj2(openFileDialogPRJ2.FileName);
            if (level == null)
            {
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "There was an error while opening project file. File may be in use or may be corrupted", "Error");
                return;
            }

            // Clean all resources before creating a new level
            if (_editor.Level != null)
                _editor.Level.Dispose();

            // Set the new level and update UI
            _editor.Level = level;

            LoadWadInInterface();
            LoadTextureMapInEditor(_editor.Level);

            _editor.RoomIndex = -1;
            comboRoom.Items.Clear();

            ReloadRooms();

            // Switch room
            comboRoom.SelectedIndex = 0;

            _editor.RoomIndex = 0;
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
            Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex];

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
                    "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes) return;

            if (openFileDialogPRJ.ShowDialog(this) != DialogResult.OK)
                return;

            using (FormImportPRJ form = new FormImportPRJ())
            {
                form.FileName = openFileDialogPRJ.FileName;
                if (form.ShowDialog() != DialogResult.OK || form.Level == null)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError(
                        "There was an error while importing project file. File may be in use or may be corrupted", "Error");
                    return;
                }

                // Clean all resources before creating a new level
                if (_editor.Level != null)
                    _editor.Level.Dispose();

                // Set the new level and update UI
                _editor.Level = form.Level;
            }
            LoadTextureMapInEditor(_editor.Level);

            LoadWadInInterface();
            _editor.RoomIndex = -1;

            comboRoom.Items.Clear();
            for (int i = 0; i < _editor.Level.Rooms.Length; i++)
            {
                string roomName = "";

                if (_editor.Level.Rooms[i] != null)
                {
                    roomName = (_editor.Level.Rooms[i].Name == null ? "Room " + i : _editor.Level.Rooms[i].Name);
                    if (_editor.Level.Rooms[i].BaseRoom != -1)
                        roomName = "(Flipped of " + _editor.Level.Rooms[i].BaseRoom + ") " + roomName;

                    comboRoom.Items.Add(i + ": " + roomName);
                }
                else
                {
                    comboRoom.Items.Add(i + ": --- Empty room ---");
                }
            }

            // Switch room
            comboRoom.SelectedIndex = 0;

            _editor.RoomIndex = 0;
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

            bool result = Level.SaveToPrj2(saveFileDialogPRJ2.FileName, _editor.Level);

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
            _editor.Level.Rooms[_editor.RoomIndex].Position += new Vector3(0.0f, 1.0f, 0.0f);

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

            for (int p = 0; p < _editor.Level.Portals.Count; p++)
            {
                Portal portal = _editor.Level.Portals.ElementAt(p).Value;

                if (portal.Room == _editor.RoomIndex)
                {
                    _editor.Level.Rooms[portal.AdjoiningRoom].BuildGeometry();
                    _editor.Level.Rooms[portal.AdjoiningRoom].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[portal.AdjoiningRoom].UpdateBuffers();
                }
            }

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();

            UpdateStatistics();
        }

        private void butRoomDown_Click(object sender, EventArgs e)
        {
            _editor.Level.Rooms[_editor.RoomIndex].Position += new Vector3(0.0f, -1.0f, 0.0f);

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

            for (int p = 0; p < _editor.Level.Portals.Count; p++)
            {
                Portal portal = _editor.Level.Portals.ElementAt(p).Value;

                if (portal.Room == _editor.RoomIndex)
                {
                    _editor.Level.Rooms[portal.AdjoiningRoom].BuildGeometry();
                    _editor.Level.Rooms[portal.AdjoiningRoom].CalculateLightingForThisRoom();
                    _editor.Level.Rooms[portal.AdjoiningRoom].UpdateBuffers();
                }
            }

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
            panel2DMap.Invalidate();

            UpdateStatistics();
        }

        /*  private void numLightIntensity_ValueChanged(object sender, EventArgs e)
          {
              if (_editor.LightIndex != -1)
              {
                  Light light = _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex];
                  _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex].Intensity = (float)numLightIntensity.Value;
                  _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                  _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                  _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                  panel3D.Draw();
              }
          }*/

        private void butCompileLevel_Click(object sender, EventArgs e)
        {
            if (_editor.Level.WadFile == null || _editor.Level.WadFile == "")
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have not loaded a WAD file", "Error",
                    DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            if (_editor.Level.TextureFile == null || _editor.Level.TextureFile == "")
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have not loaded a texture map", "Error",
                    DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            using (FormBuildLevel form = new FormBuildLevel())
                form.ShowDialog(this);
        }

        private void BuilLevel()
        {
            string baseName = System.IO.Path.GetFileNameWithoutExtension(_editor.Level.WadFile);

            LevelCompilerTr4 comp = new LevelCompilerTr4(_editor.Level, "Game\\Data\\" + baseName + ".tr4");
            comp.CompileLevel();
        }

        private void butCompileLevelAndPlay_Click(object sender, EventArgs e)
        {
            if (_editor.Level.WadFile == null || _editor.Level.WadFile == "")
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have not loaded a WAD file", "Error",
                    DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            if (_editor.Level.TextureFile == null || _editor.Level.TextureFile == "")
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have not loaded a texture map", "Error",
                    DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            using (FormBuildLevel form = new FormBuildLevel())
            {
                form.LaunchGameAfterCompile = true;
                form.ShowDialog(this);
            }

            ProcessStartInfo info = new ProcessStartInfo();

            info.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\Game";
            info.FileName = "tomb4.exe";

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
            using (FormTextureSounds form = new FormTextureSounds())
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
            if (_editor.Mode == EditorMode.Geometry && _editor.PickingResult.ElementType == PickingElementType.Block)
            {
                int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
                int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

                if (xMin == -1 || xMax == -1 || zMin == -1 || zMax == -1)
                    return;

                // Search the first free room
                short found = -1;
                for (int i = 0; i < _editor.Level.Rooms.Length; i++)
                {
                    if (_editor.Level.Rooms[i] == null)
                    {
                        found = (short) i;
                        break;
                    }
                }

                if (found == -1)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError("You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                                                          "Error", DarkUI.Forms.DarkDialogButton.Ok);
                    return;
                }

                Room room = _editor.Level.Rooms[_editor.RoomIndex];

                if (room.Flipped)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError("You can't copy a flipped room", "Error");
                    return;
                }

                byte numXSectors = (byte) (xMax - xMin + 3);
                byte numZSectors = (byte) (zMax - zMin + 3);

                Room newRoom = new Geometry.Room(_editor.Level, 0, 0, 0, numXSectors, numZSectors, room.Ceiling);

                for (int x = 1; x < numXSectors - 1; x++)
                {
                    for (int z = 1; z < numZSectors - 1; z++)
                    {
                        newRoom.Blocks[x, z] = room.Blocks[x + xMin - 1, z + zMin - 1].Clone();

                        for (int f = 0; f < newRoom.Blocks[x, z].Faces.Length; f++)
                        {
                            if (newRoom.Blocks[x, z].Faces[f].Texture != -1)
                            {
                                // _editor.Level.TextureSamples[newRoom.Blocks[x, z].Faces[f].Texture].UsageCount++;
                            }
                        }

                        // TODO: remove
                        /*if (newRoom.Blocks[x, z].Type == BlockType.Portal ||
                            newRoom.Blocks[x, z].Type == BlockType.FloorPortal ||
                            newRoom.Blocks[x, z].Type == BlockType.CeilingPortal)
                        {
                            newRoom.Blocks[x, z].Type = BlockType.Floor;
                        }*/
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
        }

        private void butSplitRoom_Click(object sender, EventArgs e)
        {
            if (_editor.Mode == EditorMode.Geometry && _editor.PickingResult.ElementType == PickingElementType.Block)
            {
                int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
                int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
                int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

                if (xMin == -1 || xMax == -1 || zMin == -1 || zMax == -1)
                    return;

                // Search the first free room
                short found = -1;
                for (int i = 0; i < _editor.Level.Rooms.Length; i++)
                {
                    if (_editor.Level.Rooms[i] == null)
                    {
                        found = (short) i;
                        break;
                    }
                }

                if (found == -1)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError("You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                                                          "Error", DarkUI.Forms.DarkDialogButton.Ok);
                    return;
                }

                Room room = _editor.Level.Rooms[_editor.RoomIndex];

                if (room.Flipped)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError("You can't split a flipped room", "Error");
                    return;
                }

                byte numXSectors = (byte) (xMax - xMin + 3);
                byte numZSectors = (byte) (zMax - zMin + 3);

                Room newRoom = new Geometry.Room(_editor.Level, 0, 0, 0, numXSectors, numZSectors, room.Ceiling);

                for (int x = 1; x < numXSectors - 1; x++)
                {
                    for (int z = 1; z < numZSectors - 1; z++)
                    {
                        newRoom.Blocks[x, z] = room.Blocks[x + xMin - 1, z + zMin - 1].Clone();

                        room.Blocks[x + xMin - 1, z + zMin - 1].Type = BlockType.Wall;

                        for (int f = 0; f < newRoom.Blocks[x, z].Faces.Length; f++)
                        {
                            if (newRoom.Blocks[x, z].Faces[f].Texture != -1)
                            {
                                //  _editor.Level.TextureSamples[newRoom.Blocks[x, z].Faces[f].Texture].UsageCount++;
                                //   _editor.Level.TextureSamples[room.Blocks[x + xMin - 1, z + zMin - 1].Faces[f].Texture].UsageCount--;
                            }
                        }

                        // TODO: remove
                        /*
                        if (newRoom.Blocks[x, z].Type == BlockType.Portal ||
                            newRoom.Blocks[x, z].Type == BlockType.FloorPortal ||
                            newRoom.Blocks[x, z].Type == BlockType.CeilingPortal)
                        {
                            newRoom.Blocks[x, z].Type = BlockType.Floor;
                        }*/
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

                _editor.Level.Rooms[_editor.RoomIndex] = room;
                _editor.Level.Rooms[found] = newRoom;

                // Update the UI
                comboRoom.Items[found] = found + ": " + newRoom.Name;
                comboRoom.SelectedIndex = found;

                _editor.CenterCamera();
                _editor.DrawPanel3D();
            }
        }

        private void butEditRoomName_Click(object sender, EventArgs e)
        {
            using (FormInputBox form = new FormInputBox())
            {
                form.Title = "Edit room's name";
                form.Message = "Insert the name of this room:";
                form.Value = _editor.Level.Rooms[_editor.RoomIndex].Name;

                if (form.ShowDialog(this) == DialogResult.Cancel)
                    return;
                if (form.Value == "")
                    return;

                _editor.Level.Rooms[_editor.RoomIndex].Name = form.Value;
                comboRoom.Items[comboRoom.SelectedIndex] = comboRoom.SelectedIndex + ": " + form.Value;
            }
        }

        private void butGridWalls_Click(object sender, EventArgs e)
        {
            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            int highest = room.GetHighestCorner();
            int lowest = room.GetLowestCorner();
            short delta = (short) ((highest - lowest) / 3);

            for (int x = 1; x < room.NumXSectors - 1; x++)
            {
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].QAFaces[0] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].QAFaces[1] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].EDFaces[0] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].EDFaces[1] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].RFFaces[0] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].RFFaces[1] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].WSFaces[0] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].WSFaces[1] = (short) -delta;

                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].QAFaces[2] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].QAFaces[3] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].EDFaces[2] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].EDFaces[3] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].RFFaces[2] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].RFFaces[3] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].WSFaces[2] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].WSFaces[3] = (short) -delta;
            }

            for (int z = 1; z < room.NumZSectors - 1; z++)
            {
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].QAFaces[1] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].QAFaces[2] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].EDFaces[1] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].EDFaces[2] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].RFFaces[1] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].RFFaces[2] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].WSFaces[1] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].WSFaces[2] = (short) -delta;

                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].QAFaces[0] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].QAFaces[3] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].EDFaces[0] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].EDFaces[3] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].RFFaces[0] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].RFFaces[3] = 0;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].WSFaces[0] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].WSFaces[3] = (short) -delta;
            }

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

            Draw();
        }

        private void butGridWalls5_Click(object sender, EventArgs e)
        {
            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            int highest = room.GetHighestCorner();
            int lowest = room.GetLowestCorner();
            short delta = (short) ((highest - lowest) / 5);

            for (int x = 1; x < room.NumXSectors - 1; x++)
            {
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].QAFaces[0] = (short) (2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].QAFaces[1] = (short) (2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].EDFaces[0] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].EDFaces[1] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].RFFaces[0] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].RFFaces[1] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].WSFaces[0] = (short) (-2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].WSFaces[1] = (short) (-2 * delta);

                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].QAFaces[2] = (short) (2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].QAFaces[3] = (short) (2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].EDFaces[2] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].EDFaces[3] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].RFFaces[2] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].RFFaces[3] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].WSFaces[2] =
                    (short) (-2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, room.NumZSectors - 1].WSFaces[3] =
                    (short) (-2 * delta);
            }

            for (int z = 1; z < room.NumZSectors - 1; z++)
            {
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].QAFaces[1] = (short) (2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].QAFaces[2] = (short) (2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].EDFaces[1] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].EDFaces[2] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].RFFaces[1] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].RFFaces[2] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].WSFaces[1] = (short) (-2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].WSFaces[2] = (short) (-2 * delta);

                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].QAFaces[0] = (short) (2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].QAFaces[3] = (short) (2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].EDFaces[0] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].EDFaces[3] = delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].RFFaces[0] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].RFFaces[3] = (short) -delta;
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].WSFaces[0] =
                    (short) (-2 * delta);
                _editor.Level.Rooms[_editor.RoomIndex].Blocks[room.NumXSectors - 1, z].WSFaces[3] =
                    (short) (-2 * delta);
            }

            _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
            _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
            _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

            Draw();
        }

        private void RandomFloor(short sign)
        {
            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            if (xMin == -1 || xMax == -1 || zMin == -1 || zMax == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Please select a valid group of sectors",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            EditorActions.RandomFloor(sign, _editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
        }

        private void RandomCeiling(short sign)
        {
            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            if (xMin == -1 || xMax == -1 || zMin == -1 || zMax == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Please select a valid group of sectors",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            EditorActions.RandomCeiling(sign, _editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
        }

        private void AverageFloor()
        {
            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            if (xMin == -1 || xMax == -1 || zMin == -1 || zMax == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Please select a valid group of sectors",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            EditorActions.AverageFloor(_editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
        }

        private void AverageCeiling()
        {
            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            if (xMin == -1 || xMax == -1 || zMin == -1 || zMax == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Please select a valid group of sectors",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            EditorActions.AverageCeiling(_editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
        }

        private void butRandomFloorUp_Click(object sender, EventArgs e)
        {
            RandomFloor(1);
        }

        private void butRandomFloorDown_Click(object sender, EventArgs e)
        {
            RandomFloor(-1);
        }

        private void butRandomCeilingUp_Click(object sender, EventArgs e)
        {
            RandomCeiling(1);
        }

        private void butRandomCeilingDown_Click(object sender, EventArgs e)
        {
            RandomCeiling(-1);
        }

        private void randomFloorUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomFloor(1);
        }

        private void randomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomFloor(-1);
        }

        private void randomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomCeiling(1);
        }

        private void randomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomCeiling(-1);
        }

        private void butAverageFloor_Click(object sender, EventArgs e)
        {
            AverageFloor();
        }

        private void butAverageCeiling_Click(object sender, EventArgs e)
        {
            AverageCeiling();
        }

        private void averageFloorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AverageFloor();
        }

        private void averageCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AverageCeiling();
        }

        private void panelStaticMeshColor_DoubleClick(object sender, EventArgs e)
        {
            if (_editor.PickingResult.ElementType == PickingElementType.StaticMesh)
            {
                StaticMeshInstance instance = (StaticMeshInstance) _editor.Level.Objects[_editor.PickingResult.Element];

                colorDialog.Color = instance.Color;
                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                panelStaticMeshColor.BackColor = colorDialog.Color;
                instance.Color = colorDialog.Color;

                _editor.Level.Objects[_editor.PickingResult.Element] = instance;

                panel3D.Draw();
            }
        }

        private void butFindItem_Click(object sender, EventArgs e)
        {
            if (_lastSearchResult >= _editor.Level.Objects.Count - 1)
                _lastSearchResult = -1;
            _lastSearchResult++;

            for (int i = _lastSearchResult; i < _editor.Level.Objects.Count; i++)
            {
                IObjectInstance instance = _editor.Level.Objects.ElementAt(i).Value;

                if (_editor.Level.Wad.Moveables.Count > panelItem.SelectedItem)
                {
                    if (instance.Type == ObjectInstanceType.Moveable)
                    {
                        MoveableInstance moveable = (MoveableInstance) instance;
                        if (moveable.Model.ObjectID == _editor.SelectedItem)
                        {
                            _lastSearchResult = i;

                            PickingResult pickingResult = new PickingResult();
                            pickingResult.ElementType = PickingElementType.Moveable;
                            pickingResult.Element = moveable.ID;
                            _editor.PickingResult = pickingResult;

                            int lastRoom = _editor.RoomIndex;
                            _editor.RoomIndex = instance.Room;

                            if (lastRoom != instance.Room)
                                CenterCamera();
                            Draw();

                            return;
                        }
                    }
                }
                else
                {
                }
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

            int trigger = Int32.Parse(lstTriggers.Text.Split(' ')[0]);
            string triggerDescription = _editor.Level.Triggers[trigger].ToString();

            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete '" + triggerDescription + "'?",
                    "Delete trigger", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes) return;

            TriggerInstance triggerObject = _editor.Level.Triggers[trigger];

            for (int x = triggerObject.X; x < triggerObject.X + triggerObject.NumXBlocks; x++)
            {
                for (int z = triggerObject.Z; z < triggerObject.Z + triggerObject.NumZBlocks; z++)
                {
                    _editor.Level.Rooms[triggerObject.Room].Blocks[x, z].Triggers.Remove(trigger);
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

            int trigger = Int32.Parse(lstTriggers.Text.Split(' ')[0]);
            string triggerDescription = _editor.Level.Triggers[trigger].ToString();

            using (FormTrigger formTrigger = new FormTrigger())
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
            Room room = _editor.Level.Rooms[_editor.RoomIndex];

            // Delete flipped room
            if (comboFlipMap.SelectedIndex == 0 && room.Flipped)
            {
                // Check if room has portals
                for (int i = 0; i < _editor.Level.Portals.Count; i++)
                {
                    Portal p = _editor.Level.Portals[i];
                    if ((p.Room == _editor.RoomIndex || p.AdjoiningRoom == _editor.RoomIndex) && !p.MemberOfFlippedRoom)
                    {
                        DarkUI.Forms.DarkMessageBox.ShowError("You can't delete a room with portals to other rooms.",
                            "Error");
                        return;
                    }
                }

                // Ask for confirmation
                if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete flipped room?",
                        "Delete flipped room", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                {
                    return;
                }

                DeleteRoom(room.AlternateRoom);

                _editor.Level.Rooms[_editor.RoomIndex].Flipped = false;
                _editor.Level.Rooms[_editor.RoomIndex].AlternateRoom = -1;
                _editor.Level.Rooms[_editor.RoomIndex].AlternateGroup = 0;

                return;
            }

            // Change flipped map number, not much to do here
            if (comboFlipMap.SelectedIndex != 0 && room.Flipped)
            {
                _editor.Level.Rooms[_editor.RoomIndex].AlternateGroup = (short) (comboFlipMap.SelectedIndex - 1);
                //_editor.Level.Rooms[_editor.Level.Rooms[_editor.RoomIndex].AlternateRoom].FlipGroup = (short)(comboFlipMap.SelectedIndex - 1);
                return;
            }

            // Create a new flipped room
            if (comboFlipMap.SelectedIndex != 0 && !room.Flipped)
            {
                // Search the first free room
                short found = -1;
                for (int i = 0; i < _editor.Level.Rooms.Length; i++)
                {
                    if (_editor.Level.Rooms[i] == null)
                    {
                        found = (short) i;
                        break;
                    }
                }

                if (found == -1)
                {
                    DarkUI.Forms.DarkMessageBox.ShowError("You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                                                          "Error", DarkUI.Forms.DarkDialogButton.Ok);
                    return;
                }

                // Duplicate portals
                int numPortals = _editor.Level.Portals.Count;
                Dictionary<int, int> duplicatedPortals = new Dictionary<int, int>();

                for (int i = 0; i < numPortals; i++)
                {
                    Portal p = _editor.Level.Portals.ElementAt(i).Value;

                    if (p.Room == _editor.RoomIndex)
                    {
                        int portalId = _editor.Level.GetNewPortalId();
                        Portal newPortal = p.ClonePortal();
                        newPortal.ID = portalId;
                        newPortal.Flipped = true;

                        p.Flipped = true;
                        _editor.Level.Portals[p.ID] = p;

                        duplicatedPortals.Add(p.ID, portalId);
                        _editor.Level.Portals.Add(portalId, newPortal);
                    }
                }

                for (int i = 0; i < duplicatedPortals.Count; i++)
                {
                    Portal p = _editor.Level.Portals[duplicatedPortals.ElementAt(i).Key];
                    _editor.Level.Portals[p.OtherID].OtherIDFlipped = duplicatedPortals.ElementAt(i).Value;
                }

                byte numXSectors = (byte) (room.NumXSectors);
                byte numZSectors = (byte) (room.NumZSectors);

                Vector3 pos = room.Position;

                Room newRoom = new Geometry.Room(_editor.Level, (int) pos.X, (int) pos.Y, (int) pos.Z, numXSectors,
                    numZSectors, room.Ceiling);

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

                        for (int f = 0; f < newRoom.Blocks[x, z].Faces.Length; f++)
                        {
                            if (newRoom.Blocks[x, z].Faces[f].Texture != -1)
                            {
                                // _editor.Level.TextureSamples[newRoom.Blocks[x, z].Faces[f].Texture].UsageCount++;
                            }
                        }
                    }
                }

                for (int i = 0; i < room.Lights.Count; i++)
                {
                    newRoom.Lights.Add(room.Lights[i].Clone());
                }

                newRoom.Name = "(Flipped of " + _editor.RoomIndex + ") Room " + found;

                _editor.Level.Rooms[_editor.RoomIndex].Flipped = true;
                _editor.Level.Rooms[_editor.RoomIndex].AlternateGroup = (short) (comboFlipMap.SelectedIndex - 1);
                _editor.Level.Rooms[_editor.RoomIndex].AlternateRoom = found;

                newRoom.Flipped = true;
                newRoom.AlternateGroup = (short) (comboFlipMap.SelectedIndex - 1);
                newRoom.BaseRoom = _editor.RoomIndex;

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
        }

        private void DeleteRoom(int index)
        {
            // Collect all triggers and objects
            List<int> objectsToRemove = new List<int>();
            List<int> triggersToRemove = new List<int>();

            for (int i = 0; i < _editor.Level.Objects.Count; i++)
            {
                IObjectInstance obj = _editor.Level.Objects.ElementAt(i).Value;
                if (obj.Room == index)
                {
                    // We must remove that object. First try to find a trigger.
                    for (int j = 0; j < _editor.Level.Triggers.Count; j++)
                    {
                        TriggerInstance trigger = _editor.Level.Triggers.ElementAt(j).Value;

                        if (trigger.TargetType == TriggerTargetType.Camera && obj.Type == ObjectInstanceType.Camera &&
                            trigger.Target == obj.ID)
                        {
                            triggersToRemove.Add(trigger.ID);
                        }

                        if (trigger.TargetType == TriggerTargetType.FlyByCamera &&
                            obj.Type == ObjectInstanceType.FlyByCamera &&
                            trigger.Target == ((FlybyCameraInstance) obj).Sequence)
                        {
                            triggersToRemove.Add(trigger.ID);
                        }

                        if (trigger.TargetType == TriggerTargetType.Sink && obj.Type == ObjectInstanceType.Sink &&
                            trigger.Target == obj.ID)
                        {
                            triggersToRemove.Add(trigger.ID);
                        }

                        if (trigger.TargetType == TriggerTargetType.Object && obj.Type == ObjectInstanceType.Moveable &&
                            trigger.Target == obj.ID)
                        {
                            triggersToRemove.Add(trigger.ID);
                        }
                    }

                    // Remove the object
                    objectsToRemove.Add(obj.ID);
                }
            }

            // Remove objects and triggers
            for (int i = 0; i < objectsToRemove.Count; i++)
            {
                _editor.Level.Objects.Remove(objectsToRemove[i]);
            }

            for (int i = 0; i < triggersToRemove.Count; i++)
            {
                _editor.Level.Triggers.Remove(triggersToRemove[i]);
            }

            comboRoom.Items[index] = index + ": --- Empty room ---";
            _editor.Level.Rooms[index] = null;
        }

        private void FlipMap(int map)
        {
            _editor.FlipMap = map;

            Room room = _editor.Level.Rooms[_editor.RoomIndex];
            if (room.Flipped)
            {
                if (room.AlternateRoom != -1 && room.AlternateGroup == map)
                {
                    SelectRoom(room.AlternateRoom);
                }
                if (room.BaseRoom != -1 && room.AlternateGroup != map)
                {
                    SelectRoom(room.BaseRoom);
                }
            }

            _editor.DrawPanel3D();
        }

        private void baseMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.FlipMap = -1;
            Room room = _editor.Level.Rooms[_editor.RoomIndex];
            if (room.Flipped && room.BaseRoom != -1)
                SelectRoom(room.BaseRoom);
            _editor.DrawPanel3D();
        }

        private void butFlipMap_Click(object sender, EventArgs e)
        {
            _editor.IsFlipMap = !_editor.IsFlipMap;
            butFlipMap.Checked = _editor.IsFlipMap;

            if (_editor.IsFlipMap)
            {
                if (_editor.Level.Rooms[_editor.RoomIndex].Flipped &&
                    _editor.Level.Rooms[_editor.RoomIndex].AlternateRoom != -1)
                {
                    comboRoom.SelectedIndex = _editor.Level.Rooms[_editor.RoomIndex].AlternateRoom;
                }
            }
            else
            {
                if (_editor.Level.Rooms[_editor.RoomIndex].Flipped &&
                    _editor.Level.Rooms[_editor.RoomIndex].BaseRoom != -1)
                {
                    comboRoom.SelectedIndex = _editor.Level.Rooms[_editor.RoomIndex].BaseRoom;
                }
            }

            _editor.DrawPanel3D();
        }

        private void butFlagBeetle_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleBlockFlag(BlockFlags.Beetle);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butFlagTriggerTriggerer_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleBlockFlag(BlockFlags.TriggerTriggerer);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.Level.WadFile == "" || _editor.Level.WadFile == null)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Can't save a project without a WAD", "Error");
                return;
            }

            if (_editor.Level.TextureFile == "" || _editor.Level.TextureFile == null)
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

            bool result = Level.SaveToPrj2(fileName, _editor.Level);

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
            _editor.Level.Rooms[_editor.RoomIndex].FlagDamage = cbFlagDamage.Checked;
        }

        private void cbFlagCold_CheckedChanged(object sender, EventArgs e)
        {
            _editor.Level.Rooms[_editor.RoomIndex].FlagCold = cbFlagCold.Checked;
        }

        private void cbFlagOutside_CheckedChanged(object sender, EventArgs e)
        {
            _editor.Level.Rooms[_editor.RoomIndex].FlagOutside = cbFlagOutside.Checked;
        }

        private void cbHorizon_CheckedChanged(object sender, EventArgs e)
        {
            _editor.Level.Rooms[_editor.RoomIndex].FlagHorizon = cbHorizon.Checked;
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
                if (_editor.Level.Rooms[i] == null)
                {
                    found = (short) i;
                    break;
                }
            }

            if (found == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                                                      "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            Room room = _editor.Level.Rooms[_editor.RoomIndex];
            Room newRoom = new Geometry.Room(_editor.Level,
                (int) room.Position.X,
                (int) (room.Position.Y + room.GetHighestCorner()),
                (int) room.Position.Z,
                20, 20, 12);

            newRoom.Name = "Room " + found;

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
                if (_editor.Level.Rooms[i] == null)
                {
                    found = (short) i;
                    break;
                }
            }

            if (found == -1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have reached the maximum number of " + Level.MaxNumberOfRooms + " rooms",
                                                      "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }

            Room room = _editor.Level.Rooms[_editor.RoomIndex];
            Room newRoom = new Geometry.Room(_editor.Level,
                (int) room.Position.X,
                (int) (room.Position.Y - 12),
                (int) room.Position.Z,
                20, 20, 12);

            newRoom.Name = "Room " + found;

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
            if (_editor.LightIndex != -1)
            {
                _editor.Level.Rooms[_editor.RoomIndex].Lights[_editor.LightIndex].Color = lightPalette.SelectedColor;
                _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                _editor.Level.Rooms[_editor.RoomIndex].CalculateLightingForThisRoom();
                _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();

                _editor.DrawPanel3D();

                panelLightColor.BackColor = lightPalette.SelectedColor;
            }
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
                    "Exit", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes) return;

            this.Close();
        }

        private void butAddTrigger_Click(object sender, EventArgs e)
        {
            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            EditorActions.AddTrigger(this, xMin, xMax, zMin, zMax);

            LoadTriggersInUI();
        }

        private void butDrawHorizon_Click(object sender, EventArgs e)
        {
            _editor.DrawHorizon = !_editor.DrawHorizon;
            butDrawHorizon.Checked = _editor.DrawHorizon;
            _editor.DrawPanel3D();
        }

        private void cbNoPathfinding_CheckedChanged(object sender, EventArgs e)
        {
            _editor.Level.Rooms[_editor.RoomIndex].ExcludeFromPathFinding = cbNoPathfinding.Checked;
        }

        private void butNotWalkableBox_Click(object sender, EventArgs e)
        {
            EditorActions.ToggleBlockFlag(BlockFlags.NotWalkableFloor);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butCeiling_Click(object sender, EventArgs e)
        {
            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            EditorActions.SetCeiling(_editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butDiagonalFloor_Click(object sender, EventArgs e)
        {
            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            EditorActions.SetDiagonalFloorSplit(_editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butDiagonalCeiling_Click(object sender, EventArgs e)
        {
            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            EditorActions.SetDiagonalCeilingSplit(_editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }

        private void butDiagonalWall_Click(object sender, EventArgs e)
        {
            int xMin = Math.Min(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int xMax = Math.Max(_editor.BlockSelectionStartX, _editor.BlockSelectionEndX);
            int zMin = Math.Min(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);
            int zMax = Math.Max(_editor.BlockSelectionStartZ, _editor.BlockSelectionEndZ);

            EditorActions.SetDiagonalWallSplit(_editor.RoomIndex, xMin, xMax, zMin, zMax);

            _editor.DrawPanel3D();
            _editor.DrawPanelGrid();
        }
        
        // Only for debugging purposes...

        private void debugAction0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TombRaider4Level level; //= new TombEngine.TombRaider4Level("e:\\trle\\data\\coastal.tr4"); // new TombEngine.TombRaider4Level("c:\\Program Files (x86)\\Steam\\steamapps\\common\\Tomb Raider (IV) The Last Revelation\\data\\karnak1.tr4");
                                    //level.Load(""); 
            level = new TombEngine.TombRaider4Level("e:\\trle\\data\\coastal.tr4");
            level.Load("originale");

            level = new TombEngine.TombRaider4Level("Game\\Data\\coastal.tr4");
            level.Load("editor");

            //level = new TombEngine.TombRaider4Level("e:\\trle\\data\\tut1.tr4");
            //level.Load("originale");
        }

        private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TombRaider3Level level; //= new TombEngine.TombRaider4Level("e:\\trle\\data\\settomb.tr4"); // new TombEngine.TombRaider4Level("c:\\Program Files (x86)\\Steam\\steamapps\\common\\Tomb Raider (IV) The Last Revelation\\data\\karnak1.tr4");
            //level.Load("");

            level = new TombEngine.TombRaider3Level("e:\\tomb3\\data\\crash.tr2");
            level.Load("crash");

            level = new TombEngine.TombRaider3Level("e:\\tomb3\\data\\jungle.tr2");
            level.Load("jungle");
        }

        private void debugAction2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<System.Drawing.Color> colors = new List<System.Drawing.Color>();
            List<int> tempColors = new List<int>();

            Bitmap bmp = (Bitmap)Bitmap.FromFile("Editor\\Palette.png");
            for (int y = 2; y < bmp.Height; y += 14)
            {
                for (int x = 2; x < bmp.Width; x += 14)
                {
                    System.Drawing.Color col = bmp.GetPixel(x, y);
                    if (col.A == 0)
                        continue;
                    /* if (!tempColors.Contains(col.ToArgb()))*/
                    tempColors.Add(col.ToArgb());
                }
            }
            File.Delete("Editor\\Palette.bin");
            BinaryWriter writer = new BinaryWriter(File.OpenWrite("Editor\\Palette.bin"));
            for (int i = 0; i < tempColors.Count; i++)
            {
                System.Drawing.Color col2 = System.Drawing.Color.FromArgb(tempColors[i]);
                writer.Write(col2.R);
                writer.Write(col2.G);
                writer.Write(col2.B);

            }

            writer.Flush();
            writer.Close();
        }

        private void debugAction3ToolStripMenuItem_Click(object sender, EventArgs e)
        { }

        private void debugAction4ToolStripMenuItem_Click(object sender, EventArgs e)
        { }

        private void debugAction5ToolStripMenuItem_Click(object sender, EventArgs e)
        { }
    }
}
