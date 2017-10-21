using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.IO;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using TombLib.Wad.Tr4Wad;
using TombLib.Wad.TrLevels;

namespace WadTool
{
    public partial class FormMain : DarkForm
    {
        private WadToolClass _tool;

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

            if (node.Tag.GetType() == typeof(WadMoveable))
            {
                var moveable = (WadMoveable)node.Tag;
                panel3D.CurrentObject = _tool.SourceWad.DirectXMoveables[moveable.ObjectID];
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

            // Load sounds
            treeSounds.Nodes.Clear();

            if (isMoveable)
            {
                var moveable = (WadMoveable)node.Tag;
                var sounds = moveable.GetSounds(_tool.DestinationWad);

                foreach (var sound in sounds)
                {
                    var nodeSound = new DarkUI.Controls.DarkTreeNode(sound.Name);
                    nodeSound.Tag = sound;
                    treeSounds.Nodes.Add(nodeSound);

                    int i = 0;

                    foreach (var wave in sound.WaveSounds)
                    {
                        var nodeWave = new DarkUI.Controls.DarkTreeNode("Sample " + i);
                        nodeWave.Tag = wave;
                        treeSounds.Nodes[treeSounds.Nodes.Count - 1].Nodes.Add(nodeWave);

                        i++;
                    }
                }
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
            if (openFileDialogWad.ShowDialog() == DialogResult.Cancel)
                return;

            // Load the Wad2
            string fileName = openFileDialogWad.FileName.ToLower();
            using (var stream = File.OpenRead(fileName))
            {
                var newWad = Wad2.LoadFromStream(stream);
                if (newWad == null)
                    return;

                if (_tool.DestinationWad != null)
                    _tool.DestinationWad.Dispose();

                newWad.FileName = openFileDialogWad.FileName;
                newWad.GraphicsDevice = _tool.Device;
                newWad.PrepareDataForDirectX();
                _tool.DestinationWad = newWad;
            }

            // Update the UI
            UpdateDestinationWad2UI();
        }

        private void UpdateDestinationWad2UI()
        {
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
            openFileDialogWad.Filter = SupportedFormats.GetFilter(FileFormatType.ObjectForWadTool);
            openFileDialogWad.Title = "Open source WAD - Wad2 - Level";
            if (openFileDialogWad.ShowDialog() == DialogResult.Cancel)
                return;

            // Load the WAD/Wad2
            string fileName = openFileDialogWad.FileName.ToLower();
            if (fileName.EndsWith(".wad"))
            {
                var originalWad = new Tr4Wad();
                originalWad.LoadWad(fileName);

                var newWad = Tr4WadOperations.ConvertTr4Wad(originalWad, wadSoundPaths);
                if (newWad == null)
                    return;

                if (_tool.SourceWad != null)
                    _tool.SourceWad.Dispose();

                newWad.GraphicsDevice = _tool.Device;
                newWad.PrepareDataForDirectX();
                _tool.SourceWad = newWad;
            }
            else if (fileName.EndsWith("wad2"))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var newWad = Wad2.LoadFromStream(stream);
                    if (newWad == null)
                        return;

                    if (_tool.SourceWad != null)
                        _tool.SourceWad.Dispose();

                    newWad.FileName = openFileDialogWad.FileName;
                    newWad.GraphicsDevice = _tool.Device;
                    newWad.PrepareDataForDirectX();
                    _tool.SourceWad = newWad;
                }
            }
            else
            {
                var originalLevel = new TrLevel();
                originalLevel.LoadLevel(fileName, _tool.Configuration.Sounds_Tr2MainSfxPath, _tool.Configuration.Sounds_Tr3MainSfxPath);

                var newWad = TrLevelOperations.ConvertTrLevel(originalLevel);
                if (newWad == null)
                    return;

                if (_tool.SourceWad != null)
                    _tool.SourceWad.Dispose();

                newWad.GraphicsDevice = _tool.Device;
                newWad.PrepareDataForDirectX();
                _tool.SourceWad = newWad;
            }

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

        private void butAddObject_Click(object sender, EventArgs e)
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

            // Check if object could be overwritten
            if (isMoveable && _tool.DestinationWad.Moveables.ContainsKey(currentObject.ObjectID) ||
                !isMoveable && _tool.DestinationWad.Statics.ContainsKey(currentObject.ObjectID))
            {
                if (DarkMessageBox.Show(this,
                   "Destination Wad2 already contains '" + currentObject.ToString() + "'. Do you want to overwrite it?",
                   "Copy object", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            // Copy the object
            _tool.DestinationWad.AddObject(currentObject, _tool.SourceWad, currentObject.ObjectID);

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
                using (var stream = new FileStream(_tool.DestinationWad.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Wad2.SaveToStream(_tool.DestinationWad, stream);
                }
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

            if (saveFileDialogWad2.ShowDialog() == DialogResult.Cancel)
                return;

            try
            {
                using (var stream = new FileStream(saveFileDialogWad2.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Wad2.SaveToStream(_tool.DestinationWad, stream);
                }
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
            if (form.ShowDialog() != DialogResult.OK)
                return;

            var objectId = form.ObjectId;
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

            // Copy the object
            _tool.DestinationWad.AddObject(currentObject, _tool.SourceWad, objectId);

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
            if (node.Tag == null || node.Tag.GetType() != typeof(WadSound))
                return;

            var currentSound = (WadSound)node.Tag;

            currentSound.Play();
        }

        private void convertWADToWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_tool.SourceWad == null)
            {
                DarkMessageBox.Show(this, "You must load a source WAD file", "Error", MessageBoxIcon.Error);
                return;
            }

            if (saveFileDialogWad2.ShowDialog() == DialogResult.Cancel)
                return;

            using (var stream = new FileStream(saveFileDialogWad2.FileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Wad2.SaveToStream(_tool.SourceWad, stream);
            }
        }

        private void debugAction0ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void butSoundEditor_Click(object sender, EventArgs e)
        {
            if (_tool.DestinationWad == null)
            {
                DarkMessageBox.Show(this, "You must load a destination Wad2 file", "Error", MessageBoxIcon.Error);
                return;
            }

            var form = new FormSoundEditor();
            form.ShowDialog();
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
            form.ShowDialog();

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
            if (form.ShowDialog() == DialogResult.Cancel)
                return;

            _tool.DestinationWad.SpriteSequences.Add(form.SpriteSequence);

            UpdateDestinationWad2UI();
        }

        private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _tool.DestinationWad.CreateNewStaticMeshFromExternalModel("tank.3ds", 1.0f);
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

            using (var form = new FormStaticMeshEditor())
            {
                form.StaticMesh = staticMesh;
                if (form.ShowDialog() == DialogResult.Cancel) return;

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
    }
}
