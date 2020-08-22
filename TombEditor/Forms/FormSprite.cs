using System;
using System.ComponentModel;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.Forms
{
    public partial class FormSprite : DarkForm
    {
        private readonly SpriteInstance _instance;
        private readonly Editor _editor;

        public FormSprite(SpriteInstance instance)
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;
                _instance = instance;

                InitializeRendering(_editor.RenderingDevice);
                PopulateSpriteList();
            }
        }

        public void InitializeRendering(RenderingDevice device)
        {
            panelRenderingSprite.InitializeRendering(device, _editor.Configuration.RenderingItem_Antialias);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.LevelChangedEvent)
                PopulateSpriteList();
        }

        private void PopulateSpriteList()
        {
            var sprites = _editor.Level.Settings.WadGetAllSpriteSequences();
            cmbSprites.Items.Clear();

            // Sprites are not referenced by sequence ID, but by absolute ID in whole sprite array.
            // Therefore, sadly we can't point user to specific sprite sequence, but only to absolute sprite ID.

            if (sprites.Count > 0)
            {
                int num = 0;
                foreach (var spriteSeq in sprites.Values)
                    for (int i = 0; i < spriteSeq.Sprites.Count; i++)
                    {
                        cmbSprites.Items.Add("(" + num + ") Sequence #" + spriteSeq.Id.TypeId + ", sprite #" + i);
                        num++;
                    }

                if (_instance.SpriteID < cmbSprites.Items.Count)
                    cmbSprites.SelectedIndex = _instance.SpriteID;
            }
            else
                cmbSprites.SelectedIndex = (cmbSprites.Items.Count > 0 ? 0 : -1);
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            if (cmbSprites.SelectedIndex >= 0)
                _instance.SpriteID = (ushort)cmbSprites.SelectedIndex;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cmbSprites_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSprites.SelectedIndex >= 0)
                panelRenderingSprite.SpriteID = cmbSprites.SelectedIndex;
        }
    }
}
