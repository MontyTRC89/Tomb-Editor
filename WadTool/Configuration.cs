using System.Drawing;
using System.Numerics;
using TombLib;

namespace WadTool
{
    // Just add properties to this class to add now configuration options.
    // They will be loaded and saved automatically.

    public class Configuration : ConfigurationBase
    {
        public override string ConfigName { get { return "Configs\\WadToolConfiguration.xml"; } }

        public bool Tool_MakeEmptyWadAtStartup { get; set; } = false;
        public string Tool_ReferenceProject { get; set; } = string.Empty;

        public float UI_FormColor_Brightness { get; set; } = 100.0f;

        public int AnimationEditor_UndoDepth { get; set; } = 30;
        public bool AnimationEditor_RewindAfterChainPlayback { get; set; } = true;
        public bool AnimationEditor_ScrollGrid { get; set; } = true;
        public bool AnimationEditor_RecoverGridAfterPositionChange { get; set; } = false;
        public bool AnimationEditor_ShowGrid { get; set; } = true;
        public bool AnimationEditor_ShowCollisionBox { get; set; } = true;
        public bool AnimationEditor_ShowGizmo { get; set; } = true;
        public bool AnimationEditor_SmoothAnimation { get; set; } = true;
        public bool AnimationEditor_ChainPlayback { get; set; } = false;
        public bool AnimationEditor_SoundPreview { get; set; } = false;
        public SoundPreviewType AnimationEditor_SoundPreviewType { get; set; } = SoundPreviewType.Land;

        public float RenderingItem_NavigationSpeedMouseWheelZoom { get; set; } = 6.0f;
        public float RenderingItem_NavigationSpeedMouseZoom { get; set; } = 800.0f;
        public float RenderingItem_NavigationSpeedMouseTranslate { get; set; } = 1500.0f;
        public float RenderingItem_NavigationSpeedMouseRotate { get; set; } = 4.0f;
        public float RenderingItem_FieldOfView { get; set; } = 50.0f;
        public bool RenderingItem_ShowDebugInfo { get; set; } = false;
        public bool RenderingItem_Antialias { get; set; } = false;
        public Vector4 RenderingItem_BackgroundColor { get; set; } = new Vector4(0.65f, 0.65f, 0.65f, 1.0f);

        public float GizmoStatic_Size { get; set; } = 1536.0f;
        public float GizmoStatic_TranslationConeSize { get; set; } = 220.0f;
        public float GizmoStatic_CenterCubeSize { get; set; } = 128.0f;
        public float GizmoStatic_ScaleCubeSize { get; set; } = 128.0f;
        public float GizmoStatic_LineThickness { get; set; } = 45.0f;

        public float GizmoSkeleton_Size { get; set; } = 512.0f;
        public float GizmoSkeleton_TranslationConeSize { get; set; } = 110.0f;
        public float GizmoSkeleton_CenterCubeSize { get; set; } = 64.0f;
        public float GizmoSkeleton_ScaleCubeSize { get; set; } = 64.0f;
        public float GizmoSkeleton_LineThickness { get; set; } = 16.0f;

        public float GizmoAnimationEditor_Size { get; set; } = 128.0f;
        public float GizmoAnimationEditor_TranslationConeSize { get; set; } = 48.0f;
        public float GizmoAnimationEditor_CenterCubeSize { get; set; } = 24.0f;
        public float GizmoAnimationEditor_ScaleCubeSize { get; set; } = 24.0f;
        public float GizmoAnimationEditor_LineThickness { get; set; } = 6.0f;

        public bool StartUpHelp_Show { get; set; } = false;

        public Point Window_FormMain_Position { get; set; } = new Point(-1);
        public Size Window_FormMain_Size { get; set; } = new Size(1200, 700);
        public bool Window_FormMain_Maximized { get; set; } = false;
        public Point Window_FormAnimationEditor_Position { get; set; } = new Point(-1);
        public Size Window_FormAnimationEditor_Size { get; set; } = new Size(1055, 707);
        public bool Window_FormAnimationEditor_Maximized { get; set; } = false;
        public Point Window_FormStateChangesEditor_Position { get; set; } = new Point(-1);
        public Size Window_FormStateChangesEditor_Size { get; set; } = new Size(596, 299);
        public bool Window_FormStateChangesEditor_Maximized { get; set; } = false;
        public Point Window_FormAnimCommandsEditor_Position { get; set; } = new Point(-1);
        public Size Window_FormAnimCommandsEditor_Size { get; set; } = new Size(397, 359); 
        public bool Window_FormAnimCommandsEditor_Maximized { get; set; } = false;
        public Point Window_FormReplaceAnimCommands_Position { get; set; } = new Point(-1);
        public Size Window_FormReplaceAnimCommands_Size { get; set; } = new Size(811, 460);
        public bool Window_FormReplaceAnimCommands_Maximized { get; set; } = false;
    }
}
