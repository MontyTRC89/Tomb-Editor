#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib.LevelData.VisualScripting;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// Drives the pure-WPF node canvas for one <see cref="Event"/>: builds node/link view-models from
    /// the event's <see cref="TriggerNode"/> graph, and supports creating (from the function catalog),
    /// dragging, deleting and connecting (Next/Else) nodes. Argument editing follows in step 3.
    /// </summary>
    public partial class NodeEditorViewModel : ObservableObject
    {
        private readonly TombLib.LevelData.Event _event;
        private readonly int _gridSize;
        private readonly double _gridStep;

        public IReadOnlyList<NodeFunction> Functions { get; }
        public ObservableCollection<NodeViewModel> Nodes { get; } = new();
        public ObservableCollection<LinkViewModel> Links { get; } = new();

        [ObservableProperty] private NodeViewModel? _selectedNode;
        [ObservableProperty] private NodeFunction? _functionToAdd;

        public double CanvasWidth => _gridSize * _gridStep;
        public double CanvasHeight => _gridSize * _gridStep;

        public NodeEditorViewModel(TombLib.LevelData.Event evt, IReadOnlyList<NodeFunction> functions, int gridSize, double gridStep)
        {
            _event = evt;
            Functions = functions;
            _gridSize = gridSize;
            _gridStep = gridStep;
            Rebuild();
        }

        private void Rebuild()
        {
            foreach (var link in Links)
                link.Detach();
            Links.Clear();
            Nodes.Clear();

            var map = new Dictionary<TriggerNode, NodeViewModel>();
            foreach (var node in TriggerNode.LinearizeNodes(_event.Nodes))
            {
                var function = Functions.FirstOrDefault(f => f.Signature == node.Function);
                var vm = new NodeViewModel(node, _gridStep, function?.Name ?? node.Function);
                map[node] = vm;
                Nodes.Add(vm);
            }

            foreach (var vm in Nodes)
            {
                if (vm.Node.Next != null && map.TryGetValue(vm.Node.Next, out var next))
                    Links.Add(new LinkViewModel(vm, next, false));

                if (vm.Node is TriggerNodeCondition condition && condition.Else != null && map.TryGetValue(condition.Else, out var elseVm))
                    Links.Add(new LinkViewModel(vm, elseVm, true));
            }
        }

        private void SyncRootsAndRebuild()
        {
            var all = TriggerNode.LinearizeNodes(_event.Nodes);
            _event.Nodes = all.Where(n => n.Previous == null).ToList();
            Rebuild();
        }

        [RelayCommand]
        private void AddNode()
        {
            if (FunctionToAdd == null)
                return;

            TriggerNode node = FunctionToAdd.Conditional
                ? new TriggerNodeCondition { Name = "If " + (Nodes.Count(n => n.IsCondition) + 1) }
                : new TriggerNodeAction { Name = "Action " + (Nodes.Count(n => !n.IsCondition) + 1) };

            node.Size = TriggerNode.DefaultSize;
            node.Function = FunctionToAdd.Signature;
            node.FixArguments(FunctionToAdd);

            int count = Nodes.Count;
            node.ScreenPosition = new Vector2(40 + count % 8 * 6, 40 + count % 8 * 6);

            _event.Nodes.Add(node);
            SyncRootsAndRebuild();
            SelectedNode = Nodes.FirstOrDefault(n => n.Node == node);
        }

        [RelayCommand]
        private void DeleteSelectedNode()
        {
            if (SelectedNode == null)
                return;

            var node = SelectedNode.Node;

            // Detach from previous.
            if (node.Previous != null)
            {
                if (node.Previous.Next == node)
                    node.Previous.Next = null;
                if (node.Previous is TriggerNodeCondition prevCondition && prevCondition.Else == node)
                    prevCondition.Else = null;
            }

            // Promote children to roots.
            if (node.Next != null)
            {
                node.Next.Previous = null;
                _event.Nodes.Add(node.Next);
                node.Next = null;
            }
            if (node is TriggerNodeCondition condition && condition.Else != null)
            {
                condition.Else.Previous = null;
                _event.Nodes.Add(condition.Else);
                condition.Else = null;
            }

            _event.Nodes.Remove(node);
            SelectedNode = null;
            SyncRootsAndRebuild();
        }

        /// <summary>Connects <paramref name="from"/>'s Next (or Else) output to <paramref name="to"/>.</summary>
        public void Connect(NodeViewModel from, NodeViewModel to, bool asElse)
        {
            if (from == to || to.Node.Previous != null || WouldCycle(from.Node, to.Node))
                return;

            if (asElse && from.Node is TriggerNodeCondition condition)
            {
                if (condition.Else != null)
                    condition.Else.Previous = null;
                condition.Else = to.Node;
            }
            else
            {
                if (from.Node.Next != null)
                    from.Node.Next.Previous = null;
                from.Node.Next = to.Node;
            }

            to.Node.Previous = from.Node;
            SyncRootsAndRebuild();
        }

        /// <summary>Clears <paramref name="from"/>'s Next (or Else) connection.</summary>
        public void Disconnect(NodeViewModel from, bool asElse)
        {
            if (asElse && from.Node is TriggerNodeCondition condition)
            {
                if (condition.Else != null)
                {
                    condition.Else.Previous = null;
                    condition.Else = null;
                }
            }
            else if (from.Node.Next != null)
            {
                from.Node.Next.Previous = null;
                from.Node.Next = null;
            }

            SyncRootsAndRebuild();
        }

        private static bool WouldCycle(TriggerNode from, TriggerNode candidate)
        {
            // Walk down from candidate; if we reach 'from', linking would create a cycle.
            foreach (var node in TriggerNode.LinearizeNodes(new List<TriggerNode> { candidate }))
                if (node == from)
                    return true;
            return false;
        }
    }
}
