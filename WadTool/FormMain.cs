using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.LevelData;
using TombLib.Sounds;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.Wad.Tr4Wad;
using TombLib.Wad.TrLevels;

namespace WadTool
{
    public partial class FormMain : DarkForm
    {
        private WadToolClass _tool;
        private WadObject _selectedObject;

        // TODO ask the user for the paths
        private readonly static List<string> wadSoundPaths =
            new List<string>
            {
                "Sounds",
                "",
                Path.Combine(Application.StartupPath, "Sounds\\Samples")
            };

        public FormMain()
        {
            InitializeComponent();

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _tool = WadToolClass.Instance;
            _tool.Initialize();

            panel3D.InitializePanel(_tool.Device);
        }

        private void butTest_Click(object sender, EventArgs e)
        {

        }

        private void openSourceWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butOpenSourceWad_Click(null, null);
        }

        private void treeSourceWad_MouseClick(object sender, MouseEventArgs e)
        {
            if (treeSourceWad.SelectedNodes.Count == 0)
                return;

            var node = treeSourceWad.SelectedNodes[0];

            if (node.Tag == null ||
                (node.Tag.GetType() != typeof(WadMoveable) &&
                 node.Tag.GetType() != typeof(WadStatic) &&
                 node.Tag.GetType() != typeof(WadSprite)))
                return;

            panel3D.CurrentWad = _tool.SourceWad;
            panel3D.Animation = -1;
            panel3D.KeyFrame = 0;

            if (node.Tag.GetType() == typeof(WadMoveable))
            {
                var moveable = (WadMoveable)node.Tag;
                panel3D.CurrentObject = _tool.SourceWad.DirectXMoveables[moveable.ObjectID];
                if (moveable.Animations.Count != 0) panel3D.Animation = 0;
            }
            else if (node.Tag.GetType() == typeof(WadStatic))
            {
                var staticMesh = (WadStatic)node.Tag;
                panel3D.CurrentObject = _tool.SourceWad.DirectXStatics[staticMesh.ObjectID];
            }
            else
            {
                var sprite = (WadSprite)node.Tag;
                panel3D.CurrentObject = sprite;
            }

            panel3D.Invalidate();
        }

        private void openDestinationWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butOpenDestWad2_Click(null, null);
        }

        private void treeDestWad_MouseClick(object sender, MouseEventArgs e)
        {
            if (treeDestWad.SelectedNodes.Count == 0)
                return;

            var node = treeDestWad.SelectedNodes[0];
            var isMoveable = node.Tag?.GetType() == typeof(WadMoveable);

            if (node.Tag == null ||
                (node.Tag.GetType() != typeof(WadMoveable) &&
                 node.Tag.GetType() != typeof(WadStatic) &&
                 node.Tag.GetType() != typeof(WadSprite)))
                return;

            if (node.Tag.GetType() != typeof(WadSprite)) _selectedObject = (WadObject)node.Tag;
            panel3D.Animation = -1;
            panel3D.KeyFrame = 0;

            // Load sounds
            treeSounds.Nodes.Clear();
            treeAnimations.Nodes.Clear();

            if (isMoveable)
            {
                var moveable = (WadMoveable)node.Tag;
                var sounds = moveable.GetSounds(_tool.DestinationWad);

                // Load sounds in UI
                foreach (var sound in sounds)
                {
                    var nodeSound = new DarkUI.Controls.DarkTreeNode(sound.Name);
                    nodeSound.Tag = sound;
                    treeSounds.Nodes.Add(nodeSound);

                    int i = 0;

                    foreach (var wave in sound.Samples)
                    {
                        var nodeWave = new DarkUI.Controls.DarkTreeNode("Sample " + i);
                        nodeWave.Tag = wave;
                        treeSounds.Nodes[treeSounds.Nodes.Count - 1].Nodes.Add(nodeWave);

                        i++;
                    }
                }

                // Load animations in UI
                var animationsNodes = new List<DarkUI.Controls.DarkTreeNode>();
                for (int i = 0; i < moveable.Animations.Count; i++)
                {
                    var nodeAnimation = new DarkUI.Controls.DarkTreeNode(moveable.Animations[i].Name);
                    nodeAnimation.Tag = i;
                    animationsNodes.Add(nodeAnimation);
                }
                treeAnimations.Nodes.AddRange(animationsNodes);

                groupSelectedMoveable.Enabled = true;
                groupSelectedMoveable.Text = "Selected moveable: " + moveable.ToString();

                if (moveable.Animations.Count != 0)
                {
                    // Reset scrollbar
                    scrollbarAnimations.Value = 0;
                    scrollbarAnimations.Maximum = (_selectedObject as WadMoveable).Animations[0].KeyFrames.Count - 1;
                    panel3D.Animation = 0;
                    panel3D.KeyFrame = 0;
                }
            }
            else
            {
                groupSelectedMoveable.Enabled = false;
                groupSelectedMoveable.Text = "Selected moveable: ";
            }

            panel3D.CurrentWad = _tool.DestinationWad;

            if (node.Tag.GetType() == typeof(WadMoveable))
            {
                var moveable = (WadMoveable)node.Tag;
                panel3D.CurrentObject = _tool.DestinationWad.DirectXMoveables[moveable.ObjectID];
            }
            else if (node.Tag.GetType() == typeof(WadStatic))
            {
                var staticMesh = (WadStatic)node.Tag;
                panel3D.CurrentObject = _tool.DestinationWad.DirectXStatics[staticMesh.ObjectID];
            }
            else
            {
                var sprite = (WadSprite)node.Tag;
                panel3D.CurrentObject = sprite;
            }

            panel3D.Invalidate();
        }

        private void butOpenDestWad2_Click(object sender, EventArgs e)
        {
            // Open the file dialog
            openFileDialogWad.Filter = "Tomb Editor Wad2 (*.wad2)|*.wad2";
            openFileDialogWad.Title = "Open destination Wad2";
            if (openFileDialogWad.ShowDialog(this) == DialogResult.Cancel)
                return;

            // Load the Wad2
            var fileName = openFileDialogWad.FileName.ToLower();
            var newWad = Wad2.LoadFromFile(fileName);
            if (newWad == null)
                return;

            if (_tool.DestinationWad != null)
                _tool.DestinationWad.Dispose();

            newWad.FileName = openFileDialogWad.FileName;
            newWad.GraphicsDevice = _tool.Device;
            newWad.PrepareDataForDirectX();
            _tool.DestinationWad = newWad;

            // Update the UI
            UpdateDestinationWad2UI();
        }

        private void UpdateDestinationWad2UI()
        {
            // Disable rendering
            treeDestWad.SelectedNodes.Clear();
            panel3D.CurrentObject = null;
            panel3D.CurrentWad = null;
            panel3D.Invalidate();

            labelDestinationVersion.Text = TrCatalog.GetVersionString(_tool.DestinationWad.Version);
            if (_tool.DestinationWad.IsNg) labelDestinationVersion.Text += " (NG)";
            labelDestinationVersion.Text += " (" + _tool.DestinationWad.SoundManagementSystem + ")";

            convertWad2ToNewDynamicSoundmapSystemToolStripMenuItem.Enabled = (_tool.DestinationWad.Version >= WadTombRaiderVersion.TR4 &&
                                                                              _tool.DestinationWad.SoundManagementSystem != WadSoundManagementSystem.DynamicSoundMap);

            treeDestWad.Nodes.Clear();

            var nodeMoveables = new DarkUI.Controls.DarkTreeNode("Moveables");
            treeDestWad.Nodes.Add(nodeMoveables);

            foreach (var moveable in _tool.DestinationWad.Moveables)
            {
                var nodeMoveable = new DarkUI.Controls.DarkTreeNode(moveable.Value.ToString());
                nodeMoveable.Tag = moveable.Value;

                treeDestWad.Nodes[0].Nodes.Add(nodeMoveable);
            }

            var nodeStatics = new DarkUI.Controls.DarkTreeNode("Statics");
            treeDestWad.Nodes.Add(nodeStatics);

            foreach (var staticMesh in _tool.DestinationWad.Statics)
            {
                var nodeStatic = new DarkUI.Controls.DarkTreeNode(staticMesh.Value.ToString());
                nodeStatic.Tag = staticMesh.Value;

                treeDestWad.Nodes[1].Nodes.Add(nodeStatic);
            }

            var nodeSprites = new DarkUI.Controls.DarkTreeNode("Sprites");
            treeDestWad.Nodes.Add(nodeSprites);

            foreach (var sequence in _tool.DestinationWad.SpriteSequences)
            {
                var nodeSequence = new DarkUI.Controls.DarkTreeNode(sequence.ToString());
                nodeSequence.Tag = sequence;

                treeDestWad.Nodes[2].Nodes.Add(nodeSequence);

                int spriteIndex = 0;
                int currentNode = treeDestWad.Nodes[2].Nodes.Count - 1;

                foreach (var sprite in sequence.Sprites)
                {
                    var nodeSprite = new DarkUI.Controls.DarkTreeNode("Sprite #" + spriteIndex);
                    nodeSprite.Tag = sprite;

                    treeDestWad.Nodes[2].Nodes[currentNode].Nodes.Add(nodeSprite);

                    spriteIndex++;
                }
            }
        }

        private void butOpenSourceWad_Click(object sender, EventArgs e)
        {
            // Open the file dialog
            openFileDialogWad.Filter = Wad2.WadFormatImportExtensions.GetFilter();
            openFileDialogWad.Title = "Open source WAD - Wad2 - Level";
            if (openFileDialogWad.ShowDialog(this) == DialogResult.Cancel)
                return;

            // Load the WAD/Wad2
            string fileName = openFileDialogWad.FileName.ToLower();
            if (fileName.EndsWith(".wad"))
            {
                var originalWad = new Tr4Wad();
                originalWad.LoadWad(fileName);

                Wad2 newWad;
                try
                {
                    newWad = Tr4WadOperations.ConvertTr4Wad(originalWad, wadSoundPaths, new GraphicalDialogHandler(this));
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception exc)
                {
                    // logger.Info(exc, "Unable to load *.wad");
                    DarkMessageBox.Show(this, "Loading the *.wad file failed! \n" + exc.Message, "Loading failed", MessageBoxIcon.Error);
                    return;
                }

                if (_tool.SourceWad != null)
                    _tool.SourceWad.Dispose();

                newWad.GraphicsDevice = _tool.Device;
                newWad.PrepareDataForDirectX();
                _tool.SourceWad = newWad;

                labelType.Text = "WAD";
            }
            else if (fileName.EndsWith("wad2"))
            {
                var newWad = Wad2.LoadFromFile(fileName);
                if (newWad == null)
                    return;

                if (_tool.SourceWad != null)
                    _tool.SourceWad.Dispose();

                newWad.FileName = openFileDialogWad.FileName;
                newWad.GraphicsDevice = _tool.Device;
                newWad.PrepareDataForDirectX();
                _tool.SourceWad = newWad;

                labelType.Text = "Wad2";
            }
            else
            {
                var originalLevel = new TrLevel();
                originalLevel.LoadLevel(fileName, 
                                        _tool.Configuration.MainSfx_Path_Tr2, 
                                        _tool.Configuration.MainSfx_Path_Tr3);

                var newWad = TrLevelOperations.ConvertTrLevel(originalLevel);
                if (newWad == null)
                    return;

                if (_tool.SourceWad != null)
                    _tool.SourceWad.Dispose();

                newWad.GraphicsDevice = _tool.Device;
                newWad.PrepareDataForDirectX();
                _tool.SourceWad = newWad;

                labelType.Text = TrCatalog.GetVersionString(newWad.Version) + " level";
            }

            // Disable rendering
            treeSourceWad.SelectedNodes.Clear();
            panel3D.CurrentObject = null;
            panel3D.CurrentWad = null;
            panel3D.Invalidate();

            // Update the UI
            treeSourceWad.Nodes.Clear();

            var nodeMoveables = new DarkUI.Controls.DarkTreeNode("Moveables");
            treeSourceWad.Nodes.Add(nodeMoveables);

            foreach (var moveable in _tool.SourceWad.Moveables)
            {
                var nodeMoveable = new DarkUI.Controls.DarkTreeNode(moveable.Value.ToString());
                nodeMoveable.Tag = moveable.Value;

                treeSourceWad.Nodes[0].Nodes.Add(nodeMoveable);
            }

            var nodeStatics = new DarkUI.Controls.DarkTreeNode("Statics");
            treeSourceWad.Nodes.Add(nodeStatics);

            foreach (var staticMesh in _tool.SourceWad.Statics)
            {
                var nodeStatic = new DarkUI.Controls.DarkTreeNode(staticMesh.Value.ToString());
                nodeStatic.Tag = staticMesh.Value;

                treeSourceWad.Nodes[1].Nodes.Add(nodeStatic);
            }

            var nodeSprites = new DarkUI.Controls.DarkTreeNode("Sprites");
            treeSourceWad.Nodes.Add(nodeSprites);

            foreach (var sequence in _tool.SourceWad.SpriteSequences)
            {
                var nodeSequence = new DarkUI.Controls.DarkTreeNode(sequence.ToString());
                nodeSequence.Tag = sequence;

                treeSourceWad.Nodes[2].Nodes.Add(nodeSequence);

                int spriteIndex = 0;
                int currentNode = treeSourceWad.Nodes[2].Nodes.Count - 1;

                foreach (var sprite in sequence.Sprites)
                {
                    var nodeSprite = new DarkUI.Controls.DarkTreeNode("Sprite #" + spriteIndex);
                    nodeSprite.Tag = sprite;

                    treeSourceWad.Nodes[2].Nodes[currentNode].Nodes.Add(nodeSprite);

                    spriteIndex++;
                }
            }
        }

        private void butAddObject_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You must load or create a new destination Wad2", "Error", MessageBoxIcon.Error);
                return;
            }

            // Check if same version
            if (_tool.DestinationWad.Version != _tool.SourceWad.Version)
            {
                butAddObjectToDifferentSlot_Click(null, null);
                return;
            }

            // Get the selected object
            if (treeSourceWad.SelectedNodes.Count == 0)
                return;
            var node = treeSourceWad.SelectedNodes[0];
            if (node.Tag == null || (node.Tag.GetType() != typeof(WadMoveable) && node.Tag.GetType() != typeof(WadStatic)))
                return;

            var currentObject = (WadObject)node.Tag;
            var isMoveable = (currentObject.GetType() == typeof(WadMoveable));

            // Check if object could be overwritten
            if (isMoveable && _tool.DestinationWad.Moveables.ContainsKey(currentObject.ObjectID) ||
                !isMoveable && _tool.DestinationWad.Statics.ContainsKey(currentObject.ObjectID))
            {
                if (DarkMessageBox.Show(this,
                   "Destination Wad2 already contains '" + currentObject.ToString() + "'. Do you want to overwrite it?",
                   "Copy object", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            var soundsNotCopied = new List<ushort>();

            // Copy the object
            _tool.DestinationWad.AddObject(currentObject, _tool.SourceWad, currentObject.ObjectID, soundsNotCopied);
            
            // Warn the user about not copied sounds
            if (soundsNotCopied.Count != 0)
            {
                var message = "Some sounds were already present in destination Wad2 and they were not copied:" + Environment.NewLine;
                foreach (var id in soundsNotCopied)
                {
                    var info = SoundsCatalog.GetSound(_tool.SourceWad.Version, id);
                    if (info == null)
                        message += "UNKNOWN_" + id + Environment.NewLine;
                    else
                        message += info.Name + Environment.NewLine;
                }
                DarkMessageBox.Show(this, message, "Sounds not copied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Update UI
            UpdateDestinationWad2UI();

            // Rebuild DirectX data
            _tool.DestinationWad.PrepareDataForDirectX();
        }

        private void butDeleteObject_Click(object sender, EventArgs e)
        {
            // Get the selected object
            if (treeDestWad.SelectedNodes.Count == 0)
                return;
            var node = treeDestWad.SelectedNodes[0];
            if (!(node.Tag is WadMoveable || node.Tag is WadStatic))
                return;

            var currentObject = (WadObject)node.Tag;
            var isMoveable = currentObject is WadMoveable;

            // Ask to the user the permission to delete object
            if (DarkMessageBox.Show(this,
                   "Are you really sure to delete '" + currentObject.ToString() + "'?",
                   "Delete object", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            // Delete the object
            _tool.DestinationWad.DeleteObject(currentObject);

            // Update UI
            UpdateDestinationWad2UI();

            // Rebuild DirectX data
            _tool.DestinationWad.PrepareDataForDirectX();
        }

        private void butSave_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You don't have a valid opened Wad2", "Error", MessageBoxIcon.Error);
                return;
            }

            try
            {
                Wad2.SaveToFile(_tool.DestinationWad, _tool.DestinationWad.FileName);
            }
            catch (Exception exc)
            {
                DarkMessageBox.Show(this, "Unable to save Wad2: " + exc.Message, "Error", MessageBoxIcon.Error);
            }
        }

        private void butSaveAs_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You don't have a valid opened Wad2", "Error", MessageBoxIcon.Error);
                return;
            }

            if (saveFileDialogWad2.ShowDialog(this) == DialogResult.Cancel)
                return;

            try
            {
                Wad2.SaveToFile(_tool.DestinationWad, saveFileDialogWad2.FileName);
            }
            catch (Exception exc)
            {
                DarkMessageBox.Show(this, "Unable to save Wad2: " + exc.Message, "Error", MessageBoxIcon.Error);
            }
        }

        private void saveWad2AsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butSaveAs_Click(null, null);
        }

        private void butAddObjectToDifferentSlot_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You must load or create a new destination Wad2", "Error", MessageBoxIcon.Error);
                return;
            }

            // Get the selected object
            if (treeSourceWad.SelectedNodes.Count == 0)
                return;
            var node = treeSourceWad.SelectedNodes[0];
            if (node.Tag == null || (node.Tag.GetType() != typeof(WadMoveable) && node.Tag.GetType() != typeof(WadStatic)))
                return;

            var currentObject = (WadObject)node.Tag;
            var isMoveable = (currentObject.GetType() == typeof(WadMoveable));

            // Ask for the new slot
            var form = new FormSelectSlot();
            form.IsMoveable = isMoveable;
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            var objectId = (uint)form.ObjectId;
            var objectName = form.ObjectName;

            // Check if object could be overwritten
            if (isMoveable && _tool.DestinationWad.Moveables.ContainsKey(objectId) ||
                !isMoveable && _tool.DestinationWad.Statics.ContainsKey(objectId))
            {
                if (DarkMessageBox.Show(this,
                   "Destination Wad2 already contains '" + objectName + "'. Do you want to overwrite it?",
                   "Copy object", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            var soundsNotCopied = new List<ushort>();

            // Copy the object
            _tool.DestinationWad.AddObject(currentObject, _tool.SourceWad, objectId, soundsNotCopied);

            // Warn the user about not copied sounds
            if (soundsNotCopied.Count != 0)
            {
                var message = "Some sounds were already present in destination Wad2 and they were not copied:" + Environment.NewLine;
                foreach (var id in soundsNotCopied)
                {
                    var info = SoundsCatalog.GetSound(_tool.SourceWad.Version, id);
                    if (info == null)
                        message += "UNKNOWN_" + id + Environment.NewLine;
                    else
                        message += info.Name + Environment.NewLine;
                }
                DarkMessageBox.Show(this, message, "Sounds not copied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Update UI
            UpdateDestinationWad2UI();

            // Rebuild DirectX data
            _tool.DestinationWad.PrepareDataForDirectX();
        }

        private void butPlaySound_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You must load or create a new destination Wad2", "Error", MessageBoxIcon.Error);
                return;
            }

            // Get the selected sound
            if (treeSounds.SelectedNodes.Count == 0)
                return;
            var node = treeSounds.SelectedNodes[0];
            if (node.Tag == null || node.Tag.GetType() != typeof(WadSample))
                return;

            var currentSound = (WadSample)node.Tag;

            currentSound.Play();
        }

        private void convertWADToWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_tool.SourceWad == null)
            {
                DarkMessageBox.Show(this, "You must load a source WAD file", "Error", MessageBoxIcon.Error);
                return;
            }

            if (saveFileDialogWad2.ShowDialog(this) == DialogResult.Cancel)
                return;

            Wad2.SaveToFile(_tool.SourceWad, saveFileDialogWad2.FileName);
        }

        private void debugAction0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //_tool.DestinationWad.ImportWadMeshTest("tank.3ds", 1.0f);
            UpdateDestinationWad2UI();
        }

        private void butSoundEditor_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You must load a destination Wad2 file", "Error", MessageBoxIcon.Error);
                return;
            }

            if (_tool.DestinationWad.Version == WadTombRaiderVersion.TR2 || 
                _tool.DestinationWad.Version == WadTombRaiderVersion.TR3)
            {
                using (var form1 = new FormTr2r3SoundManager())
                    form1.ShowDialog();
            }
            else
            {
                using (var form2 = new FormSoundEditor(_tool.DestinationWad))
                    form2.ShowDialog();
            }
        }

        private void spriteEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butSpriteEditor_Click(null, null);
        }

        private void butSpriteEditor_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You must load a destination Wad2 file", "Error", MessageBoxIcon.Error);
                return;
            }

            var form = new FormSpriteSequencesEditor();
            form.ShowDialog(this);

            UpdateDestinationWad2UI();
        }

        private void addNewSpriteSequenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You must load a destination Wad2 file", "Error", MessageBoxIcon.Error);
                return;
            }

            var sequence = new WadSpriteSequence();

            var form = new FormSpriteEditor();
            form.SpriteSequence = sequence;
            if (form.ShowDialog(this) == DialogResult.Cancel)
                return;

            _tool.DestinationWad.SpriteSequences.Add(form.SpriteSequence);

            UpdateDestinationWad2UI();
        }

        private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //_tool.DestinationWad.CreateNewStaticMeshFromExternalModel("tank.3ds", 1.0f);
            UpdateDestinationWad2UI();
        }

        private void debugAction1ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //_tool.DestinationWad.DirectXTexture.Save("E:\\atlas.png", SharpDX.Toolkit.Graphics.ImageFileType.Png);
            Wad2.SaveToFile(_tool.SourceWad, "E:\\testchunk.wad2");
            var newWad = Wad2.LoadFromFile("E:\\testchunk.wad2");
            if (_tool.DestinationWad != null)
                _tool.DestinationWad.Dispose();

            newWad.FileName = "E:\\testchunk.wad2";
            newWad.GraphicsDevice = _tool.Device;
            newWad.PrepareDataForDirectX();
            _tool.DestinationWad = newWad;


            // Update the UI
            UpdateDestinationWad2UI();
        }

        private void treeDestWad_DoubleClick(object sender, EventArgs e)
        {
            // Get the selected sound
            if (treeDestWad.SelectedNodes.Count == 0)
                return;
            var node = treeDestWad.SelectedNodes[0];
            if (node.Tag == null || node.Tag.GetType() != typeof(WadStatic))
                return;

            var staticMesh = (WadStatic)node.Tag;

            using (var form = new FormStaticMeshEditor(staticMesh))
            {
                if (form.ShowDialog(this) == DialogResult.Cancel) return;

                UpdateDestinationWad2UI();
            }
        }

        private void debugAction4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var level = new TrLevel();
            //level.LoadLevel("E:\\Andrea1.trc");
        }

        private void soundManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butSoundEditor_Click(null, null);
        }

        private void debugAction5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TrCatalog.LoadCatalog("Editor\\TRCatalog.xml");
        }

        private void debugAction6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var level = new TrLevel();
            //level.LoadLevel("E:\\TR2\\data\\venice.tr2");
            var newWad = TrLevelOperations.ConvertTrLevel(level);

            if (_tool.SourceWad != null)
                _tool.SourceWad.Dispose();

            newWad.GraphicsDevice = _tool.Device;
            newWad.PrepareDataForDirectX();
            _tool.SourceWad = newWad;

            // Update the UI
            treeSourceWad.Nodes.Clear();

            var nodeMoveables = new DarkUI.Controls.DarkTreeNode("Moveables");
            treeSourceWad.Nodes.Add(nodeMoveables);

            foreach (var moveable in _tool.SourceWad.Moveables)
            {
                var nodeMoveable = new DarkUI.Controls.DarkTreeNode(moveable.Value.ToString());
                nodeMoveable.Tag = moveable.Value;

                treeSourceWad.Nodes[0].Nodes.Add(nodeMoveable);
            }

            var nodeStatics = new DarkUI.Controls.DarkTreeNode("Statics");
            treeSourceWad.Nodes.Add(nodeStatics);

            foreach (var staticMesh in _tool.SourceWad.Statics)
            {
                var nodeStatic = new DarkUI.Controls.DarkTreeNode(staticMesh.Value.ToString());
                nodeStatic.Tag = staticMesh.Value;

                treeSourceWad.Nodes[1].Nodes.Add(nodeStatic);
            }
        }

        private void aboutWadToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout form = new FormAbout())
                form.ShowDialog(this);
        }

        private void debugAction7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*for (var i = 1; i <= 4; i++)
            {
                var addedSounds = new List<int>();

                using (var reader = new StreamReader(File.OpenRead("E:\\SFX_TR" + i + ".txt")))
                {
                    if (File.Exists("E:\\TR" + i + ".xml")) File.Delete("E:\\TR" + i + ".xml");

                    using (var writer = new StreamWriter(File.OpenWrite("E:\\TR" + i + ".xml")))
                    {
                        while (!reader.EndOfStream)
                        {
                            var s = reader.ReadLine();
                            int id = Int32.Parse(s.Split(' ')[0]);
                            if (addedSounds.Contains(id)) continue;
                            addedSounds.Add(id);

                            s = s.Substring(4, s.Length - 4);
                            s = s.Replace("\"", "");
                            s = s.Replace(" -- ", "\" name=\"");
                            s = "<sound id=\"" + id + s + "\" ";
                            if (i == 4 && Wad2.MandatorySounds.Contains((ushort)id)) s += "mandatory=\"true\"";
                            s+= "/>";
                            writer.WriteLine(s);
                        }
                    }
                }
            }*/

            using (var reader = new StreamReader(File.OpenRead("E:\\sounds.txt")))
            {
                using (var writer = new StreamWriter(File.OpenWrite("E:\\TR5.xml")))
                {
                    int i = 0;
                    while (!reader.EndOfStream)
                    {
                        var s = reader.ReadLine();
                        s = s.Substring(0, s.IndexOf(":"));
                        s = s[0] + s.Replace("_", " ").Substring(1, s.Length - 1).ToLower();
                        writer.WriteLine("<sound id=\"" +i + "\" name=\"" + s + "\"/>");
                        i++;
                    }
                }
            }
        }

        private void debugAction8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var wad = _tool.SourceWad;
            wad.Textures.Clear();
            wad.Moveables.Clear();
            wad.Statics.Clear();
            wad.Meshes.Clear();

            var newSounds = new Dictionary<ushort, WadSoundInfo>();

            foreach(var info in wad.Sounds)
            {
                if (TrCatalog.IsSoundMandatory(wad.Version, info.Key))
                {
                    newSounds.Add(info.Key, info.Value);
                }
            }

            wad.Sounds.Clear();
            wad.Samples.Clear();

            foreach (var info in newSounds)
            {
                wad.Sounds.Add(info.Key, info.Value);

                foreach (var sample in info.Value.Samples)
                {
                    if (!wad.Samples.ContainsKey(sample.Hash)) wad.Samples.Add(sample.Hash, sample);
                }
            }

            Wad2.SaveToFile(wad, "E:\\BaseWad.wad2");
        }

        private void debugAction9ToolStripMenuItem_Click(object sender, EventArgs e)
        {


            //tempBitmap.Save("testpack.png");
        }

        private void butNewWad2_Click(object sender, EventArgs e)
        {
            using (var form = new FormNewWad2())
            {
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;

                if (_tool.DestinationWad != null) _tool.DestinationWad.Dispose();
                var wad = new Wad2(form.Version, false);
                wad.IsNg = form.IsNG;
                wad.SoundManagementSystem = form.SoundManagementSystem;
                wad.GraphicsDevice = _tool.Device;
                wad.PrepareDataForDirectX();
                _tool.DestinationWad = wad;

                UpdateDestinationWad2UI();
            }
        }

        private void butNewWad2ForLevel_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad != null) _tool.DestinationWad.Dispose();
            var wad = Wad2.LoadFromFile("Editor\\BaseWad2\\BaseTR4.wad2");
            if (wad == null)
            {
                DarkMessageBox.Show(this,
                                    "There was an error while creating the new Wad2",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            wad.GraphicsDevice = _tool.Device;
            wad.PrepareDataForDirectX();
            _tool.DestinationWad = wad;

            UpdateDestinationWad2UI();
        }

        private void butChangeSlot_Click(object sender, EventArgs e)
        {
            /*if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You must load or create a new destination Wad2", "Error", MessageBoxIcon.Error);
                return;
            }

            // Get the selected object
            if (treeSourceWad.SelectedNodes.Count == 0)
                return;
            var node = treeSourceWad.SelectedNodes[0];
            if (node.Tag == null || (node.Tag.GetType() != typeof(WadMoveable) && node.Tag.GetType() != typeof(WadStatic)))
                return;

            var currentObject = (WadObject)node.Tag;
            var isMoveable = (currentObject.GetType() == typeof(WadMoveable));

            // Ask for the new slot
            var form = new FormSelectSlot();
            form.IsMoveable = isMoveable;
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            var objectId = (uint)form.ObjectId;
            var objectName = form.ObjectName;

            if (objectId == currentObject.ObjectID)
            {
                DarkMessageBox.Show(this, "The slot that you have selected is the same of the current object", "Error", MessageBoxIcon.Error);
                return;
            }

            currentObject.ObjectID = objectId;

            UpdateDestinationWad2UI();*/
        }

        private void lstAnimations_Click(object sender, EventArgs e)
        {
            // Get selected animation
            if (treeAnimations.SelectedNodes.Count == 0) return;
            var node = treeAnimations.SelectedNodes[0];
            var animationIndex = (int)node.Tag;

            // Reset scrollbar
            scrollbarAnimations.Value = 0;
            scrollbarAnimations.Maximum = (_selectedObject as WadMoveable).Animations[animationIndex].KeyFrames.Count - 1;

            // Reset panel 3D
            panel3D.Animation = animationIndex;
            panel3D.KeyFrame = 0;
            panel3D.Invalidate();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }

        private void scrollbarAnimations_ValueChanged(object sender, DarkUI.Controls.ScrollValueEventArgs e)
        {
            panel3D.KeyFrame = scrollbarAnimations.Value;
            panel3D.Invalidate();
        }

        private void importModelAsStaticMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = ImportedGeometry.FileExtensions.GetFilter();
                dialog.Title = "Select a 3D file that you want to see imported.";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

                using (var form = new GeometryIOSettingsDialog(new IOGeometrySettings()))
                {
                    form.AddPreset(IOSettingsPresets.SettingsPresets);
                    if (form.ShowDialog(this) == DialogResult.Cancel) return;
                    _tool.DestinationWad.CreateNewStaticMeshFromExternalModel(dialog.FileName, form.Settings);
                    UpdateDestinationWad2UI();
                }

            }
        }

        private void newWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butNewWad2_Click(null, null);
        }

        private void convertWad2ToNewDynamicSoundmapSystemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this, "Warning: converting this Wad2 to the new dynamic soundmap system " +
                                    "is definitive and can't be reverted. Do you really want to continue?",
                                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _tool.DestinationWad.SoundManagementSystem = WadSoundManagementSystem.DynamicSoundMap;

            UpdateDestinationWad2UI();
        }
    }
}
