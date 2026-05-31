using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using WinFormsAutoScaleMode = System.Windows.Forms.AutoScaleMode;
using WinFormsContainerControl = System.Windows.Forms.ContainerControl;
using WinFormsControl = System.Windows.Forms.Control;
using WinFormsElementHost = System.Windows.Forms.Integration.ElementHost;
using WpfBorder = System.Windows.Controls.Border;
using WpfHorizontalAlignment = System.Windows.HorizontalAlignment;
using WpfScaleTransform = System.Windows.Media.ScaleTransform;
using WpfUIElement = System.Windows.UIElement;
using WpfVerticalAlignment = System.Windows.VerticalAlignment;

namespace TombLib.Utils
{
    public static class ElementHostScaling
    {
        public const float ReferenceDpi = 96.0f;
        private const float DpiScaleTolerance = 0.001f;

        private static readonly ConditionalWeakTable<WinFormsControl, ScalingMarker> _scaledRoots = new ConditionalWeakTable<WinFormsControl, ScalingMarker>();
        private static readonly ConditionalWeakTable<WinFormsElementHost, ElementHostScaleState> _lockedElementHosts = new ConditionalWeakTable<WinFormsElementHost, ElementHostScaleState>();

        public static void LockToReferenceDpi(WinFormsControl root, float? dpiScale = null)
        {
            if (root == null)
                return;

            float resolvedDpiScale = dpiScale ?? GetSystemDpiScale();

            root.SuspendLayout();

            try
            {
                ScaleRootOnce(root, resolvedDpiScale);

                foreach (var control in WinFormsUtils.AllSubControls(root))
                {
                    if (control is WinFormsContainerControl container)
                        container.AutoScaleMode = WinFormsAutoScaleMode.None;

                    if (control is WinFormsElementHost elementHost)
                        LockElementHostToReferenceDpi(elementHost, resolvedDpiScale);
                }
            }
            finally
            {
                root.ResumeLayout(true);
            }
        }

        public static void LockToReferenceDpi(IEnumerable<WinFormsControl> roots, float? dpiScale = null)
        {
            if (roots == null)
                return;

            float resolvedDpiScale = dpiScale ?? GetSystemDpiScale();

            foreach (var root in roots)
                LockToReferenceDpi(root, resolvedDpiScale);
        }

        public static float GetSystemDpiScale()
        {
            using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
                return graphics.DpiX / ReferenceDpi;
        }

        private static void ScaleRootOnce(WinFormsControl root, float dpiScale)
        {
            if (_scaledRoots.TryGetValue(root, out _) || Math.Abs(dpiScale - 1.0f) <= DpiScaleTolerance)
                return;

            _scaledRoots.Add(root, new ScalingMarker());
            root.Scale(new SizeF(1.0f / dpiScale, 1.0f / dpiScale));
        }

        private static void LockElementHostToReferenceDpi(WinFormsElementHost elementHost, float dpiScale)
        {
            if (elementHost.Child == null)
            {
                UnlockElementHost(elementHost);
                return;
            }

            if (Math.Abs(dpiScale - 1.0f) <= DpiScaleTolerance)
                return;

            if (!_lockedElementHosts.TryGetValue(elementHost, out var scaleState) ||
                elementHost.Child != scaleState.Root)
            {
                _lockedElementHosts.Remove(elementHost);

                scaleState = new ElementHostScaleState(elementHost.Child);
                _lockedElementHosts.Add(elementHost, scaleState);

                elementHost.Child = scaleState.Root;
                elementHost.SizeChanged -= ElementHost_SizeChanged;
                elementHost.SizeChanged += ElementHost_SizeChanged;
                elementHost.Disposed -= ElementHost_Disposed;
                elementHost.Disposed += ElementHost_Disposed;
            }

            scaleState.DpiScale = dpiScale;
            UpdateLockedElementHost(elementHost, scaleState);
        }

        private static void UnlockElementHost(WinFormsElementHost elementHost)
        {
            elementHost.SizeChanged -= ElementHost_SizeChanged;
            elementHost.Disposed -= ElementHost_Disposed;
            _lockedElementHosts.Remove(elementHost);
        }

        private static void UpdateLockedElementHost(WinFormsElementHost elementHost, ElementHostScaleState scaleState)
        {
            double inverseScale = 1.0 / scaleState.DpiScale;

            scaleState.Root.LayoutTransform = new WpfScaleTransform(inverseScale, inverseScale);
            scaleState.Root.Width = Math.Max(1.0, elementHost.ClientSize.Width * scaleState.DpiScale);
            scaleState.Root.Height = Math.Max(1.0, elementHost.ClientSize.Height * scaleState.DpiScale);
        }

        private static void ElementHost_SizeChanged(object sender, EventArgs e)
        {
            if (sender is WinFormsElementHost elementHost &&
                _lockedElementHosts.TryGetValue(elementHost, out var scaleState))
                UpdateLockedElementHost(elementHost, scaleState);
        }

        private static void ElementHost_Disposed(object sender, EventArgs e)
        {
            if (sender is WinFormsElementHost elementHost)
                UnlockElementHost(elementHost);
        }

        private sealed class ScalingMarker
        {
        }

        private sealed class ElementHostScaleState
        {
            public ElementHostScaleState(WpfUIElement child)
            {
                Root = new WpfBorder
                {
                    Child = child,
                    HorizontalAlignment = WpfHorizontalAlignment.Left,
                    VerticalAlignment = WpfVerticalAlignment.Top,
                    SnapsToDevicePixels = true,
                    UseLayoutRounding = false
                };
            }

            public float DpiScale { get; set; }

            public WpfBorder Root { get; }
        }
    }
}