using System;
using System.Threading;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public interface IEditorEvent { }
    public interface IWadChangedEvent : IEditorEvent { }

    public enum WadArea
    {
        Source,
        Destination
    }

    public struct MainSelection
    {
        public WadArea WadArea;
        public IWadObjectId Id;
    }

    public class WadToolClass : IDisposable
    {
        // The editor event
        public event Action<IEditorEvent> EditorEventRaised;

        public void RaiseEvent(IEditorEvent eventObj)
        {
            SynchronizationContext.Current.Send(eventObj_ => EditorEventRaised?.Invoke((IEditorEvent)eventObj_), eventObj);
        }

        // The configuration
        public Configuration Configuration { get; }

        // Open wads
        private Wad2 _destinationWad;
        public Wad2 DestinationWad
        {
            get { return _destinationWad; }
            set
            {
                if (_destinationWad == value)
                    return;
                _destinationWad = value;
                DestinationWadChanged();

                // Update selection
                if (_mainSelection.HasValue)
                    if (_mainSelection.Value.WadArea == WadArea.Destination)
                        if (value.Contains(_mainSelection.Value.Id))
                            RaiseEvent(new MainSelectionChangedEvent());
                        else
                            MainSelection = null;
            }
        }

        private Wad2 _sourceWad;
        public Wad2 SourceWad
        {
            get { return _sourceWad; }
            set
            {
                if (_sourceWad == value)
                    return;
                _sourceWad = value;

                SourceWadChanged();

                // Update selection
                if (_mainSelection.HasValue)
                    if (_mainSelection.Value.WadArea == WadArea.Source)
                        if (value.Contains(_mainSelection.Value.Id))
                            RaiseEvent(new MainSelectionChangedEvent());
                        else
                            MainSelection = null;
            }
        }

        public Wad2 GetWad(WadArea? wadArea)
        {
            if (!wadArea.HasValue)
                return null;
            switch (wadArea.Value)
            {
                case WadArea.Source: return SourceWad;
                case WadArea.Destination: return DestinationWad;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void WadChanged(WadArea wadArea)
        {
            switch (wadArea)
            {
                case WadArea.Source:
                    SourceWadChanged();
                    break;
                case WadArea.Destination:
                    DestinationWadChanged();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public class DestinationWadChangedEvent : IWadChangedEvent
        { }
        public void DestinationWadChanged()
        {
            RaiseEvent(new DestinationWadChangedEvent());
        }

        public class SourceWadChangedEvent : IWadChangedEvent
        { }
        public void SourceWadChanged()
        {
            RaiseEvent(new SourceWadChangedEvent());
        }

        // Selection
        public class MainSelectionChangedEvent : IWadChangedEvent
        { }
        private MainSelection? _mainSelection;
        public MainSelection? MainSelection
        {
            get { return _mainSelection; }
            set
            {
                if (_mainSelection == null && value == null)
                    return;
                if (_mainSelection != null && value != null && _mainSelection.Equals(value))
                    return;
                _mainSelection = value;
                RaiseEvent(new MainSelectionChangedEvent());
            }
        }

        public class StaticSelectedLightChangedEvent : IEditorEvent
        { }
        public void StaticSelectedLightChanged()
        {
            RaiseEvent(new StaticSelectedLightChangedEvent());
        }

        public class StaticLightsChangedEvent : IEditorEvent
        { }
        public void StaticLightsChanged()
        {
            RaiseEvent(new StaticLightsChangedEvent());
        }

        public class BoneOffsetMovedEvent : IEditorEvent
        { }
        public void BoneOffsetMoved()
        {
            RaiseEvent(new BoneOffsetMovedEvent());
        }

        public class BonePickedEvent : IEditorEvent
        { }
        public void BonePicked()
        {
            RaiseEvent(new BonePickedEvent());
        }

        public class SelectedObjectEditedEvent : IEditorEvent
        { }
        public void SelectedObjectEdited()
        {
            RaiseEvent(new SelectedObjectEditedEvent());
        }

        public class AnimationEditorMeshSelectedEvent : IEditorEvent
        {
            public ObjectMesh Mesh { get; set; }
            public AnimatedModel Model { get; set; }
            public AnimationEditorMeshSelectedEvent(AnimatedModel model, ObjectMesh mesh)
            {
                Model = model;
                Mesh = mesh;
            }
        }
        public void AnimationEditorMeshSelected(AnimatedModel model, ObjectMesh mesh)
        {
            RaiseEvent(new AnimationEditorMeshSelectedEvent(model, mesh));
        }

        // Construction and destruction
        public WadToolClass(Configuration configuration)
        {
            Configuration = configuration;
        }

        public void Dispose()
        {

        }
    }
}
