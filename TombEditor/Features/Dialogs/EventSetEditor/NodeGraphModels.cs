#nullable enable

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLib.LevelData.VisualScripting;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// View-model for a single node on the WPF node canvas, wrapping a <see cref="TriggerNode"/>.
    /// Position is stored on the model as a 0-256 grid coordinate; the canvas maps it to pixels via
    /// <see cref="_gridStep"/>.
    /// </summary>
    public partial class NodeViewModel : ObservableObject
    {
        private readonly double _gridStep;
        private readonly string _functionDisplay;

        public TriggerNode Node { get; }
        public ObservableCollection<ArgumentViewModel> Arguments { get; } = new();
        public event Action? Moved;

        public NodeViewModel(TriggerNode node, double gridStep, NodeFunction? function, ArgumentDataProvider provider)
        {
            Node = node;
            _gridStep = gridStep;
            _functionDisplay = function?.Name ?? node.Function;

            if (function != null)
            {
                node.FixArguments(function);
                foreach (var layout in function.Arguments)
                {
                    int index = node.Arguments.FindIndex(a => a.Name == layout.Name);
                    if (index >= 0)
                        Arguments.Add(new ArgumentViewModel(node.Arguments, index, layout, provider));
                }
            }
        }

        public bool IsCondition => Node is TriggerNodeCondition;
        public bool HasArguments => Arguments.Count > 0;

        public string Title
        {
            get => Node.Name;
            set { if (Node.Name == value) return; Node.Name = value; OnPropertyChanged(); }
        }

        public string FunctionName => string.IsNullOrEmpty(_functionDisplay) ? "(no function)" : _functionDisplay;
        public double Width => Node.Size;

        [ObservableProperty] private double _height = 64;
        [ObservableProperty] private bool _isSelected;

        partial void OnHeightChanged(double value) => RaisePorts();

        public double CanvasLeft
        {
            get => Node.ScreenPosition.X * _gridStep;
            set => SetGridPosition(value / _gridStep, Node.ScreenPosition.Y);
        }

        public double CanvasTop
        {
            get => Node.ScreenPosition.Y * _gridStep;
            set => SetGridPosition(Node.ScreenPosition.X, value / _gridStep);
        }

        private void SetGridPosition(double gx, double gy)
        {
            gx = Math.Clamp(gx, 0, 256);
            gy = Math.Clamp(gy, 0, 256);
            Node.ScreenPosition = new Vector2((float)gx, (float)gy);
            OnPropertyChanged(nameof(CanvasLeft));
            OnPropertyChanged(nameof(CanvasTop));
            RaisePorts();
        }

        // Connection ports (in canvas pixels).
        public double InX => CanvasLeft + Width / 2.0;
        public double InY => CanvasTop;
        public double OutX => CanvasLeft + Width / 2.0;
        public double OutY => CanvasTop + Height;
        public double ElseX => CanvasLeft + Width;
        public double ElseY => CanvasTop + Height / 2.0;

        private void RaisePorts()
        {
            OnPropertyChanged(nameof(InX));
            OnPropertyChanged(nameof(InY));
            OnPropertyChanged(nameof(OutX));
            OnPropertyChanged(nameof(OutY));
            OnPropertyChanged(nameof(ElseX));
            OnPropertyChanged(nameof(ElseY));
            Moved?.Invoke();
        }
    }

    /// <summary>A rendered connection (<c>Next</c> or <c>Else</c>) between two nodes.</summary>
    public partial class LinkViewModel : ObservableObject
    {
        public NodeViewModel From { get; }
        public NodeViewModel To { get; }
        public bool IsElse { get; }

        public LinkViewModel(NodeViewModel from, NodeViewModel to, bool isElse)
        {
            From = from;
            To = to;
            IsElse = isElse;
            From.Moved += RaiseEndpoints;
            To.Moved += RaiseEndpoints;
        }

        public void Detach()
        {
            From.Moved -= RaiseEndpoints;
            To.Moved -= RaiseEndpoints;
        }

        public double X1 => IsElse ? From.ElseX : From.OutX;
        public double Y1 => IsElse ? From.ElseY : From.OutY;
        public double X2 => To.InX;
        public double Y2 => To.InY;

        private void RaiseEndpoints()
        {
            OnPropertyChanged(nameof(X1));
            OnPropertyChanged(nameof(Y1));
            OnPropertyChanged(nameof(X2));
            OnPropertyChanged(nameof(Y2));
        }
    }
}
