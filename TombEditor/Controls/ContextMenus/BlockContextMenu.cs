using System;
using System.Windows.Forms;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Controls.ContextMenus
{
    class BlockContextMenu : BaseContextMenu
    {
        public BlockContextMenu(Editor editor, IWin32Window owner, Room targetRoom, VectorInt2 targetBlock)
            : base(editor, owner)
        {
			
			Items.Add(new ToolStripMenuItem("Paste object", Properties.Resources.general_clipboard_16, (o, e) =>
            {
                EditorActions.PasteObject(targetBlock, targetRoom);
            }) { Enabled = Clipboard.ContainsData(typeof(ObjectClipboardData).FullName) });
            Items.Add(new ToolStripSeparator());
			
			Items.Add(new ToolStripMenuItem("Move Lara", null, (o, e) =>
            {
                EditorActions.MoveLara(this, targetRoom, targetBlock);
            }));

			Items.Add(new ToolStripMenuItem("Move Object", Properties.Resources.general_target_16, (o, e) => 
            {
				var obj = editor.SelectedObject as PositionBasedObjectInstance;
				EditorActions.MoveObject(obj, targetRoom, targetBlock);
			}) { Enabled = _editor.SelectedObject is PositionBasedObjectInstance });
			Items.Add(new ToolStripSeparator());

            Items.Add(new ToolStripMenuItem("Add camera", Properties.Resources.objects_Camera_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new CameraInstance());
            }));

            if (editor.Level.Settings.GameVersion <= TRVersion.Game.TR2)
            Items.Add(new ToolStripMenuItem("Add room sprite", Properties.Resources.objects_Sprite_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new SpriteInstance());
            }));

            if (editor.Level.Settings.GameVersion >= TRVersion.Game.TR4)
            Items.Add(new ToolStripMenuItem("Add flyby camera", Properties.Resources.objects_movie_projector_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new FlybyCameraInstance(editor.SelectedObject));
            }));

            Items.Add(new ToolStripMenuItem("Add sink", Properties.Resources.objects_tornado_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new SinkInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add sound source", Properties.Resources.objects_speaker_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new SoundSourceInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add ghost block", Properties.Resources.objects_geometry_override_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new GhostBlockInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add imported geometry", Properties.Resources.objects_custom_geometry, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new ImportedGeometryInstance());
            }));

            Items.Add(new ToolStripMenuItem("Add memo", Properties.Resources.objects_Memo_16, (o, e) =>
            {
                EditorActions.PlaceObject(targetRoom, targetBlock, new MemoInstance());
            }));

            if (_editor.Level.IsTombEngine)
            {
                Items.Add(new ToolStripMenuItem("Add box volume", Properties.Resources.objects_volume_box_16, (o, e) =>
                {
                    EditorActions.PlaceObject(targetRoom, targetBlock, new BoxVolumeInstance());
                }));
            }
        }
    }
}
