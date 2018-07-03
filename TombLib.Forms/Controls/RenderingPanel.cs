using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Rendering;

namespace TombLib.Controls
{
    public class RenderingPanel : Panel
    {
        public event EventHandler Draw;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RenderingSwapChain SwapChain { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RenderingDevice Device { get; private set; }

        public virtual void InitializeRendering(RenderingDevice device, bool antialias)
        {
            Device = device;
            SwapChain = device.CreateSwapChain(new RenderingSwapChain.Description
            {
                Size = new VectorInt2(ClientSize.Width, ClientSize.Height),
                WindowHandle = Handle,
                Antialias = antialias
            });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (SwapChain != null)
            {
                SwapChain.Resize(new VectorInt2(ClientSize.Width, ClientSize.Height));
                Invalidate();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime || Device == null || SwapChain == null)
                e.Graphics.Clear(Parent.BackColor);
            // Don't paint the background if being rendered
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime || Device == null || SwapChain == null)
            {
                e.Graphics.DrawString("Rendering: Not Available in form designer!", Font, Brushes.DarkGray, ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                return;
            }

            SwapChain.Clear(ClearColor);
            OnDraw();
            SwapChain.Present();
        }

        protected virtual void OnDraw()
        {
            Draw?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                SwapChain.Dispose();
        }

        protected virtual Vector4 ClearColor { get; } = new Vector4(0.392f, 0.584f, 0.929f, 1.0f); // "Cornflower blue" by default
    }
}
